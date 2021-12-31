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
    int rotation = 0; 

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
        Inputs();
        SelectSide();
        highlightObj.SetActive(showHighlight);
    }

    void Inputs(){
        if(Input.GetKeyDown(KeyCode.R)){
            rotation = ++rotation % 4;
        }
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
        int xSign = GetSign(size.x);
        int zSign = GetSign(size.z);
        for (int y = 0; y <= size.y; y++){
            for (int x = 0; x < size.x * (xSign); x++){
                for (int z = 0; z < size.z * (zSign); z++){
                    Vector3Int tempCellPos = cell + new Vector3Int(x * xSign,y,z * zSign);
                    GridInfo tempCell = grid.GetCell(tempCellPos);
                    if(y == 0){
                        if(grid.InBounds(tempCellPos) && tempCell.blockData.content == BlockContent.buildable){
                            if(ShapeData.shapeDict[tempCell.shape].traversableNormals != null){
                                foreach(Vector3Int normal in ShapeData.shapeDict[tempCell.shape].traversableNormals){
                                    if(Vector3Int.RoundToInt(tempCell.rotation * normal).y >= 0) return false;
                                }
                            }
                        }else{
                            return false;
                        }
                    }else{
                        if(grid.InBounds(tempCellPos) && tempCell == GridInfo.empty){
                            occupiedSpaces.Add(tempCellPos);
                        }else{
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    int GetSign(int x){
        return x < 0 ? -1 : 1;
    }

    void PositionHighlight(Vector3Int cell, bool valid){
        highlightObj.transform.position = GetTurretCenter(cell);
        Vector3Int size = RotatedTurretSize();
        highlightObj.transform.localScale = new Vector3(size.x, 0.001f, size.z) * grid.cellSize;
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
        Vector3 offsetX = Vector3.right * size.x / 2 * grid.cellSize;
        Vector3 offsetZ = Vector3.forward * size.z / 2 * grid.cellSize;
        int xSign = GetSign(size.x);
        int zSign = GetSign(size.z);
        Vector3 corner = grid.GridToWorld(cell, side : Vector3.up) - new Vector3(xSign, 0, zSign) * grid.cellSize / 2;
        return corner + offsetX + offsetZ;
    }

    Vector3Int RotatedTurretSize(){
        Vector3Int size = Vector3Int.RoundToInt(Quaternion.Euler(Vector3.up * rotation * 90) * selectedTurret.size);
        return size;
    }

    void AddTurret(Vector3Int cell, List<Vector3Int> occupiedSpaces){
        //to-do: money checking
        foreach(Vector3Int occupiedCell in occupiedSpaces){
            grid.SetCell(occupiedCell, GridInfo.occupied);
        }
        Vector3 center = GetTurretCenter(cell);
        Instantiate(selectedTurret.turret, center, Quaternion.Euler(Vector3.up * rotation * 90));
    }

}
