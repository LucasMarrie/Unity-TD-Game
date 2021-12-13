using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEditor : MonoBehaviour
{
    public MapGenerator mapGenerator;

    public GameObject highlightObj;
    Material highlighMat;
    
    [SerializeField]
    float timeBetweenAction = 0.1f;
    float nextActionTime;
    public float uv = 0;
    
    Dictionary<HighlightType, Color> highlightColors;
    Grid grid;
    EditType selectedEdit = EditType.Add;
    GridContent selectedBlock = GridContent.block;
    float minPlaceDistance;
    // Start is called before the first frame update
    void Start()
    {
        grid = mapGenerator.GetGrid();
        highlightColors = new Dictionary<HighlightType, Color>();
        highlightColors.Add(HighlightType.Valid, Color.green);
        highlightColors.Add(HighlightType.Invalid, Color.red);

        highlightObj = GameObject.Instantiate(highlightObj, Vector3.zero, Quaternion.identity);
        highlightObj.SetActive(false);
        highlighMat = highlightObj.GetComponent<MeshRenderer>().material;

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
    }

    void SelectSide(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool showHighlight = false;

        if(Physics.Raycast(ray, out hit)){
            Vector3Int topCell = grid.WorldToGrid(hit.point + hit.normal * grid.cellSize/2);
            Vector3Int bottomCell = grid.WorldToGrid(hit.point - hit.normal * grid.cellSize/2);

            if(selectedEdit == EditType.Add){
                if(topCell != Vector3.one * -1){
                    if(hit.distance > minPlaceDistance){
                        showHighlight = PositionHighlight(topCell, -hit.normal, HighlightType.Valid, EditType.Add);
                        if(Input.GetButton("Fire1") && Time.time >= nextActionTime){
                            mapGenerator.AddBlock(topCell, selectedBlock);
                            nextActionTime = Time.time + timeBetweenAction;
                        }
                    }else{
                        showHighlight = PositionHighlight(topCell, -hit.normal, HighlightType.Invalid, EditType.Add);
                    }
                }else if(bottomCell != Vector3.one * -1){
                    showHighlight = PositionHighlight(bottomCell, hit.normal, HighlightType.Invalid, EditType.Add);
                }
            }
            else if(selectedEdit == EditType.Delete){
                if(bottomCell != Vector3.one * -1){
                    showHighlight = PositionHighlight(bottomCell, hit.normal, HighlightType.Valid, EditType.Delete);
                    if(Input.GetButton("Fire1") && Time.time >= nextActionTime){
                        mapGenerator.DeleteBlock(bottomCell);
                        nextActionTime = Time.time + timeBetweenAction;
                    }
                }
                else if(topCell != Vector3.one * -1){
                    showHighlight = PositionHighlight(topCell, -hit.normal, HighlightType.Invalid, EditType.Add);
                }
            }
        }
        if(showHighlight)
            highlightObj.SetActive(true);
        else
            highlightObj.SetActive(false);
    }

    bool PositionHighlight(Vector3Int cell, Vector3 normal, HighlightType highlightType, EditType editTpye){
        if(editTpye == EditType.Add){
            highlightObj.transform.position = grid.GridToWorld(cell, side: normal);
            highlightObj.transform.localScale = new Vector3(grid.cellSize, 0.01f, grid.cellSize);
        }else if(editTpye == EditType.Delete){
            highlightObj.transform.position = grid.GridToWorld(cell);
            highlightObj.transform.localScale = Vector3.one * (grid.cellSize + 0.01f);
        }
        highlightObj.transform.rotation = Quaternion.LookRotation(normal) * Quaternion.LookRotation(Vector3.up);
        highlighMat.color = highlightColors[highlightType];
        return true;
    }

    enum HighlightType
    {
        Invalid,
        Valid
    }

    enum EditType
    {
        Add,
        Delete,
    }
}
