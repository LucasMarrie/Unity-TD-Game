using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretEditor : MonoBehaviour
{
    MapGenerator map;
    MapGrid grid;
    Camera cam;
    public LayerMask mapMask;

    public TurretList turretList;
    int turretIdx = 0;
    TurretData selectedTurret;

    [Header("Highlight Object")]
    public GameObject highlightObj;
    public Material validMat;
    public Material invalidMat;
    bool? prevHighlight = null;
    bool showHighlight = false;

    //turret editing
    int rotationIndex = 0; //out of 4

    void Start()
    {
        cam = Camera.main;
        map = MapGenerator.map;
        grid = map.GetGrid();
        highlightObj = Instantiate(highlightObj, Vector3.zero, Quaternion.identity);
        CycleTurret(0);
    }

    // Update is called once per frame
    void Update()
    {
        SelectSide();
        highlightObj.SetActive(showHighlight);
    }

    void CycleTurret(int cycle){
        turretIdx += cycle;
        int turretCount = turretList.turrets.Count - 1;
        if(turretIdx < 0){
            turretIdx = turretCount;
        }
        else if(turretIdx > turretCount){
            turretIdx = 0;
        }
        selectedTurret = turretList.turrets[turretIdx];
    }

    bool SelectSide(){
        showHighlight = false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, 100, mapMask) && Vector3Int.RoundToInt(hit.normal) == Vector3Int.up){
            Vector3Int placementCell = grid.WorldToGrid(hit.point + Vector3.down * 0.001f);
            if(placementCell == -Vector3Int.one) return false;

            List<Vector3Int> occupiedSpaces;
            if(SpaceAvailable(placementCell, out occupiedSpaces)){
                PositionHighlight(placementCell, true);
                if(Input.GetButtonDown("Fire1")){
                    AddTurret(placementCell, occupiedSpaces);
                }
            }else{
                PositionHighlight(placementCell, false);
            }

        }else{
            return false;
        }
        return true;
    }

    bool SpaceAvailable(Vector3Int cell, out List<Vector3Int> occupiedSpaces){
        occupiedSpaces = new List<Vector3Int>();
        Vector3Int size = RotatedTurretSize();
        for (int y = 0; y <= size.y; y++){
            for (int x = 0; x < size.x; x++){
                for (int z = 0; z < size.z; z++){
                    Vector3Int tempCell = cell + new Vector3Int(x,y,z);
                    if(y == 0){
                        if(!grid.InBounds(tempCell) || grid.GetCell(tempCell).blockData.content != BlockContent.buildable){
                            return false;
                        }
                    }else{
                        if(grid.InBounds(tempCell) && grid.GetCell(tempCell) == GridInfo.empty){
                            occupiedSpaces.Add(tempCell);
                        }else{
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    void PositionHighlight(Vector3Int cell, bool valid){
        highlightObj.transform.position = GetTurretCenter(cell);
        highlightObj.transform.localScale = new Vector3(selectedTurret.size.x, 0.001f, selectedTurret.size.z) * grid.cellSize;
        if(prevHighlight != valid){
            if(valid){
                highlightObj.GetComponent<MeshRenderer>().material = validMat;
            }else{
                highlightObj.GetComponent<MeshRenderer>().material = invalidMat;
            }
        }
        showHighlight = true;
    }

    Vector3 GetTurretCenter(Vector3Int cell){
        Vector3Int size = RotatedTurretSize();
        Vector3 offsetX = Vector3.right * size.x / 2;
        Vector3 offsetZ = Vector3.forward * size.z / 2;
        Vector3 corner = grid.GridToWorld(cell, side : Vector3.up) - new Vector3(1, 0, 1) * grid.cellSize / 2;
        return corner + offsetX + offsetZ;
    }

    Vector3Int RotatedTurretSize(){
        return Vector3Int.RoundToInt(Quaternion.Euler(Vector3.up * 180 * rotationIndex) * selectedTurret.size);
    }

    void AddTurret(Vector3Int cell, List<Vector3Int> occupiedSpaces){
        //to-do: money checking
        foreach(Vector3Int occupiedCell in occupiedSpaces){
            grid.SetCell(occupiedCell, GridInfo.occupied);
        }
        Vector3 center = GetTurretCenter(cell);
        Instantiate(selectedTurret.turret, center, Quaternion.Euler(Vector3.up * 180 * rotationIndex));
    }

}
