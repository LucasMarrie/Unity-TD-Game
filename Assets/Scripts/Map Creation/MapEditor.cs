using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEditor : MonoBehaviour
{
    MapGenerator map;
    Camera cam;
    public LayerMask mapMask;
    [Header("Visualisation Objects")]
    public GameObject highlightObj;
    MeshRenderer highlighMesh;
    public Material invalidMat;
    public Material validMat;
    bool showHighlight = false;

    public GameObject projectionObj;
    BlockData projectionData;
    bool showProjection = false;
    MeshFilter projectionMesh;

    public float timeBetweenAction = 0.15f;
    float nextActionTime;

    MapGrid grid;
    EditType selectedEdit = EditType.Add;
    int blockIndex = 0;
    BlockData selectedBlock;
    int shapeIndex = 0;
    Shape selectedShape;

    float minPlaceDistance;

    Dictionary<Vector3,Vector3[]> relFace = new Dictionary<Vector3,Vector3[]>{
        { Vector3.forward, new Vector3[]{Vector3.up, Vector3.left} },
        { Vector3.back, new Vector3[]{Vector3.up, Vector3.right} },
        { Vector3.up, new Vector3[]{Vector3.forward, Vector3.right} },
        { Vector3.down, new Vector3[]{Vector3.forward, Vector3.left} },
        { Vector3.left, new Vector3[]{Vector3.up, Vector3.back} },
        { Vector3.right, new Vector3[]{Vector3.up, Vector3.forward} },
    };

    HashSet<Vector3> straightAngles = new HashSet<Vector3>{
        Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
    };
    // Start is called before the first frame update
    void Start()
    {
        map = MapGenerator.map;
        grid = map.GetGrid();
        cam = Camera.main;

        highlightObj = GameObject.Instantiate(highlightObj, Vector3.zero, Quaternion.identity);
        highlightObj.SetActive(false);
        highlighMesh = highlightObj.GetComponent<MeshRenderer>();

        projectionObj = GameObject.Instantiate(projectionObj, Vector3.zero, Quaternion.identity);
        projectionMesh = projectionObj.GetComponent<MeshFilter>();
        projectionData = new BlockData{
            name = "Projection",
            content = BlockContent.empty,
            material = projectionObj.GetComponent<MeshRenderer>().material,
        };
        projectionObj.SetActive(false);
        CycleBlock(0,0);

        minPlaceDistance = GetComponent<CharacterController>().radius + grid.cellSize;
    }

    // Update is called once per frame
    void Update()
    {
        SelectSide();
        if(Input.GetMouseButton(1)){
            selectedEdit = EditType.Delete;
        }else{
            selectedEdit = EditType.Add;
        }

        if(Input.GetKeyDown(KeyCode.J)){
            CycleBlock(0, -1);
        }else if(Input.GetKeyDown(KeyCode.L)){
            CycleBlock(0, 1);
        }

        if(Input.GetKeyDown(KeyCode.I)){
            CycleBlock(-1, 0);
        }else if(Input.GetKeyDown(KeyCode.K)){
            CycleBlock(1, 0);
        }
        
    }

    void CycleBlock(int blockShift, int shapeShift){
        blockIndex += blockShift;
        int lastBlock = BlockList.blockList.Length - 1;
        if(blockIndex < 0)
            blockIndex = lastBlock;
        else if(blockIndex > lastBlock)
            blockIndex = 0;
        selectedBlock = BlockList.blockList[blockIndex];

        shapeIndex += shapeShift;
        int lastShape = selectedBlock.shapes.Length - 1;
        if(shapeIndex < 0) 
            shapeIndex = lastShape;
        else if(shapeIndex > lastShape) 
            shapeIndex = 0;
        selectedShape = selectedBlock.shapes[shapeIndex];

        UpdateProjectionMesh();
        Debug.Log(selectedBlock.name + " : " + selectedShape);
    }

    void SelectSide(){
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        showHighlight = false;
        showProjection = false;

        if(Physics.Raycast(ray, out hit, 100, mapMask)){
            Vector3 normal = StraightenNormal(hit.normal, hit.point);
            Vector3Int topCell = grid.WorldToGrid(hit.point + normal * 0.01f);
            Vector3Int bottomCell = grid.WorldToGrid(hit.point - normal * 0.01f);
            Quaternion rotation = GetBlockRotation(normal, grid.GridToWorld(topCell), hit.point);

            if(bottomCell != Vector3Int.one * -1 && topCell != Vector3Int.one * -1){
                topCell = bottomCell + Vector3Int.RoundToInt(normal);
                if(!grid.InBounds(topCell))
                    topCell = Vector3Int.one * -1;
            }
            
            if(selectedEdit == EditType.Add){
                if(topCell != Vector3Int.one * -1 && grid.GetCell(topCell).blockData.content == BlockContent.empty){
                        if(hit.distance > minPlaceDistance){
                            PositionHighlight(topCell, -normal, HighlighType.valid, EditType.Add);
                            if(Input.GetButton("Fire1") && Time.time >= nextActionTime){
                                map.AddBlock(topCell, selectedBlock, rotation, selectedShape);
                                nextActionTime = Time.time + timeBetweenAction;
                            }     
                        ProjectObject(topCell, rotation);
                    }
                }else if(bottomCell != Vector3.one * -1){
                    PositionHighlight(bottomCell, normal, HighlighType.invalid, EditType.Add);
                }
            }
            else if(selectedEdit == EditType.Delete){
                if(bottomCell != Vector3.one * -1){
                    PositionHighlight(bottomCell, normal, HighlighType.invalid, EditType.Delete);
                    if(Input.GetButton("Fire1") && Time.time >= nextActionTime){
                        map.DeleteBlock(bottomCell);
                        nextActionTime = Time.time + timeBetweenAction;
                    }
                }
            }
        }
        highlightObj.SetActive(showHighlight);
        projectionObj.SetActive(showProjection);

    }

    Vector3 StraightenNormal(Vector3 normal, Vector3 position){
        if(!straightAngles.Contains(normal)){
            foreach(Vector3 straightAngle in straightAngles){
                if(Mathf.Abs(Vector3.Angle(straightAngle, normal)) <= 60){
                    if(grid.WorldToGrid(position - normal * 0.01f) == grid.WorldToGrid(position - straightAngle * grid.cellSize/2))
                        return straightAngle;
                }
            }
        }
        return normal;
    }

    Quaternion GetBlockRotation(Vector3 normal, Vector3 midPoint, Vector3 point){
        Quaternion rotation = Quaternion.LookRotation(normal) * Quaternion.LookRotation(Vector3.down);
        if(normal == Vector3.up){
            rotation *= Quaternion.LookRotation(Vector3.left);
        }else{
            rotation *= Quaternion.LookRotation(Vector3.right);
        }

        Vector3 relUp = relFace[normal][0];
        Vector3 relLeft = relFace[normal][1];

        bool isUp = VectorSum(Vector3.Scale(relUp, midPoint)) >=  VectorSum(Vector3.Scale(relUp, point));
        bool isLeft = VectorSum(Vector3.Scale(relLeft, midPoint)) <=  VectorSum(Vector3.Scale(relLeft, point));

        if(isUp && isLeft){
            //rotation *= Quaternion.LookRotation(Vector3.forward);
        }else if(isUp){
            rotation *= Quaternion.LookRotation(Vector3.right);
        }else if(isLeft){
            rotation *= Quaternion.LookRotation(Vector3.left);
        }else{
            rotation *= Quaternion.LookRotation(Vector3.back);
        }

        return rotation;
    }

    float VectorSum(Vector3 vec){
        return vec.x + vec.y + vec.z;
    }

    void ProjectObject(Vector3Int cell, Quaternion rotation){
        projectionObj.transform.position = grid.GridToWorld(cell);
        projectionObj.transform.rotation = rotation;
        showProjection = true;
    }

    void UpdateProjectionMesh(){
        Material projectionMat = projectionObj.GetComponent<MeshRenderer>().material;

        projectionMesh.mesh = MeshGenerator.CreateMesh(new GridInfo(projectionData, Quaternion.identity, selectedShape), grid.cellSize);
    }

    void PositionHighlight(Vector3Int cell, Vector3 normal, HighlighType hltType, EditType editTpye){
        if(editTpye == EditType.Add){
            highlightObj.transform.position = grid.GridToWorld(cell, side: normal);
            highlightObj.transform.localScale = new Vector3(grid.cellSize, 0.01f, grid.cellSize);
        }else if(editTpye == EditType.Delete){
            highlightObj.transform.position = grid.GridToWorld(cell);
            highlightObj.transform.localScale = Vector3.one * (grid.cellSize + 0.01f);
        }
        if(hltType == HighlighType.valid){
            highlighMesh.material = validMat;
        }else{
            highlighMesh.material = invalidMat;
        }
        highlightObj.transform.rotation = Quaternion.LookRotation(normal) * Quaternion.LookRotation(Vector3.up);
        showHighlight = true;
    }

    enum HighlighType{
        valid,
        invalid,
    }

    enum EditType
    {
        Add,
        Delete,
    }
}
