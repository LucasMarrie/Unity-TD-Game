using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInputs))]
public class MapEditor : MonoBehaviour
{
    MapGenerator map;
    PlayerInputs playerInputs;
    [SerializeField] LayerMask mapMask;
    [Header("Highlight Object")]
    [SerializeField] GameObject highlightObj;
    List<GameObject> highlightObjects = new List<GameObject>();
    [SerializeField] Material invalidMat;
    [SerializeField] Material validMat;
    bool? prevValidity = null;
    bool showHighlight = false;

    [Header("Projection Objects")]
    [SerializeField] GameObject projectionObj;
    List<GameObject> projectionObjects = new List<GameObject>();
    BlockData projectionData;
    bool showProjection = false;

    [Header("Showcase Object")]
    [SerializeField] GameObject showcaseObj;
    [SerializeField] float showcaseRotationSpeed = 90f;
    MeshFilter showcaseMesh;
    MeshRenderer showcaseRenderer;

    [Header("Editing Tool")]
    [SerializeField] float timeBetweenAction = 0.15f;
    [SerializeField] int maxtoolSize = 15;
    int toolSize = 1;
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
        playerInputs = GetComponent<PlayerInputs>();

        //projection object
        projectionData = new BlockData{
            name = "Projection",
            content = BlockContent.empty,
            material = projectionObj.GetComponent<MeshRenderer>().sharedMaterial,
        };
        //showcase object
        showcaseMesh = showcaseObj.GetComponent<MeshFilter>();
        showcaseRenderer = showcaseObj.GetComponent<MeshRenderer>();

        CycleBlock(0,0);

        minPlaceDistance = GetComponent<CharacterController>().radius + grid.cellSize;
    }

    // Update is called once per frame
    void Update()
    {
        SelectSide();
        Inputs();
        RotateShowcaseObj();
    }

    void Inputs(){
        if(Input.GetMouseButton(1)){
            selectedEdit = EditType.Delete;
        }else{
            selectedEdit = EditType.Add;
        }


        CycleToolSize((int) (Input.GetAxisRaw("Mouse ScrollWheel")* 10));

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

    void CycleToolSize(int dir){
        toolSize += dir * 2;
        if(toolSize < 1){ 
            toolSize = 1;
        }else if(toolSize > maxtoolSize){
            toolSize = maxtoolSize;
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
        UpdateShowcaseMesh();
    }

    void SelectSide(){
        showHighlight = false;
        showProjection = false;

        if(playerInputs.RaycastHitLayer(mapMask)){
            RaycastHit hit = playerInputs.Hit;
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
                    List<Vector3Int> selectedCells = GetSelection(topCell, normal);
                    if(hit.distance > minPlaceDistance){
                        PositionHighlight(selectedCells, -normal, true, EditType.Add);
                        if(Input.GetButton("Fire1") && Time.time >= nextActionTime){
                            map.AddBlocks(selectedCells, selectedBlock, rotation, selectedShape);
                            nextActionTime = Time.time + timeBetweenAction;
                        }     
                        ProjectObject(selectedCells, rotation);
                    }
                }else if(bottomCell != Vector3.one * -1){
                    PositionHighlight(new List<Vector3Int>{bottomCell}, normal, true, EditType.Add);
                }
            }
            else if(selectedEdit == EditType.Delete){
                if(bottomCell != Vector3.one * -1){
                    List<Vector3Int> selectedCells = GetSelection(bottomCell, normal);
                    PositionHighlight(selectedCells, normal, false, EditType.Delete);
                    if(Input.GetButton("Fire1") && Time.time >= nextActionTime){
                        map.DeleteBlocks(selectedCells);
                        nextActionTime = Time.time + timeBetweenAction;
                    }
                }
            }
        }
        if(!showHighlight){
            HideHighlight();
        }
        if(!showProjection){
            HideProjection();
        }
    }

    List<Vector3Int> GetSelection(Vector3Int cell, Vector3 normal){
        List<Vector3Int> selection = new List<Vector3Int>();
        Vector3Int surface = Vector3Int.one - Vector3Int.RoundToInt(normal);
        Vector3Int[] dirs = new Vector3Int[2];
        int dirIdx = 0;
        if(surface.x == 1){
            dirs[dirIdx++] = Vector3Int.right;
        }
        if(surface.y == 1){
            dirs[dirIdx++] = Vector3Int.up;
        }
        if(surface.z == 1){
            dirs[dirIdx++] = Vector3Int.forward;
        }
        
        for (int x = -(toolSize - 1)/2; x <= (toolSize - 1)/2; x++)
        {
            for (int z = -(toolSize - 1)/2; z <= (toolSize - 1)/2; z++)
            {
                Vector3Int tempCell = cell + dirs[0] * x + dirs[1] * z;
                if(grid.InBounds(tempCell)){
                    if((selectedEdit == EditType.Add && grid.GetCell(tempCell) == GridInfo.empty)
                    || (selectedEdit == EditType.Delete && grid.GetCell(tempCell) != GridInfo.empty)){
                        selection.Add(tempCell);
                    }
                }
            }
        }
        return selection;
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

    void HideProjection(){
        if(projectionObjects.Count > 0 && projectionObjects[0].activeSelf){
            foreach(GameObject obj in projectionObjects){
                obj.SetActive(false);
            }
        }
    }

    void ProjectObject(List<Vector3Int> cells, Quaternion rotation){
        bool addedObject = false;
        for (int i = 0; i < cells.Count; i++)
        {
            if(i >= projectionObjects.Count){
                projectionObjects.Add(Instantiate(projectionObj, Vector3.zero, Quaternion.identity));
                addedObject = true;
            }
            GameObject obj = projectionObjects[i];
            obj.transform.position = grid.GridToWorld(cells[i]);
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            showProjection = true;
        }
        for (int i = cells.Count; i < projectionObjects.Count; i++)
        {
            projectionObjects[i].SetActive(false);
        }
        if(addedObject){
            UpdateProjectionMesh();
        }
    }

    void UpdateProjectionMesh(){
        Mesh mesh = MeshGenerator.CreateMesh(new GridInfo(projectionData, Quaternion.identity, selectedShape), grid.cellSize, false);
        foreach(GameObject projObj in projectionObjects){
            projObj.GetComponent<MeshFilter>().mesh = mesh;
        }
    }

    void UpdateShowcaseMesh(){
        showcaseRenderer.material = selectedBlock.material;
        showcaseMesh.mesh = MeshGenerator.CreateMesh(new GridInfo(selectedBlock, Quaternion.identity, selectedShape), grid.cellSize,  true);
    }

    void RotateShowcaseObj(){
        showcaseObj.transform.Rotate(Vector3.up * showcaseRotationSpeed * Time.deltaTime);
    }

    void PositionHighlight(List<Vector3Int> cells, Vector3 normal, bool valid, EditType editType){
        if(prevValidity != valid){
            SwitchHighlightMat(valid);
            prevValidity = valid;
        }
        for (int i = 0; i < cells.Count; i++)
        {
            if(i >= highlightObjects.Count){
                highlightObjects.Add(Instantiate(highlightObj, Vector3.zero, Quaternion.identity));
            }
            GameObject obj = highlightObjects[i];
            if(editType == EditType.Add){
                obj.transform.position = grid.GridToWorld(cells[i], side: normal);
                obj.transform.localScale = new Vector3(grid.cellSize, 0.01f, grid.cellSize);
            }else if(editType == EditType.Delete){
                obj.transform.position = grid.GridToWorld(cells[i]);
                obj.transform.localScale = Vector3.one * (grid.cellSize + 0.01f);
            }
            obj.transform.rotation = Quaternion.LookRotation(normal) * Quaternion.LookRotation(Vector3.up);
            obj.SetActive(true);
            showHighlight = true;
        }
        for (int i = cells.Count; i < highlightObjects.Count; i++)
        {
            highlightObjects[i].SetActive(false);
        }
    }

    void SwitchHighlightMat(bool valid){
        foreach(GameObject obj in highlightObjects){
            if(valid){
                obj.GetComponent<MeshRenderer>().material = validMat;
            }else{
                obj.GetComponent<MeshRenderer>().material = invalidMat;
            }
        }
    }

    void HideHighlight(){
        if(highlightObjects.Count > 0 && highlightObjects[0].activeSelf){
            foreach(GameObject obj in highlightObjects){
                obj.SetActive(false);
            }
        }
    }

    enum EditType
    {
        Add,
        Delete,
    }
}
