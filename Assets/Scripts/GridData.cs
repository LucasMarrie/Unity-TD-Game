using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

//class used for saving grid data in file

public class GridData
{
    static string mapdataPath = Application.dataPath + "/MapData";
    static string customMapsPath =  mapdataPath + "/Custom";
    static Material[] mapMats;
    static Dictionary<Material,int> mapMatsIndex;

    public float cellSize;
    public Vector3Int gridSize;
    public Vector3 worldPos;

    //grid block positions
    public Vector3Int[] cells;
    public int[] content;
    public Quaternion[] rotation;
    public int[] material; 
    public Color[] colors;

    public GridData(Grid grid){
        cellSize = grid.cellSize;
        gridSize = grid.gridSize;
        worldPos = grid.worldPos;

        List<Vector3Int> cellsList = new List<Vector3Int>();
        List<int> contentList = new List<int>();
        List<Quaternion> rotationList = new List<Quaternion>();
        List<int> materialList = new List<int>();
        List<Color> colorList = new List<Color>();

        for(int x = 0; x < grid.gridSize.x; x++){
            for(int y = 0; y < grid.gridSize.y; y++){
                for(int z = 0; z < grid.gridSize.z; z++){
                    GridInfo cell = grid.cells[x,y,z];
                    if(cell.content != GridContent.empty){
                        cellsList.Add(new Vector3Int(x,y,z));
                        contentList.Add((int)cell.content);
                        rotationList.Add(cell.rotation);
                        materialList.Add(mapMatsIndex[cell.material]);
                        colorList.Add(cell.color);
                    }
                }
            }
        }
        cells = cellsList.ToArray();
        content = contentList.ToArray();
        rotation = rotationList.ToArray();
        material = materialList.ToArray();
        colors = colorList.ToArray();
    }

    public static void InitMaterials(Material[] _materials){
        mapMats = _materials;
        mapMatsIndex = new Dictionary<Material, int>();
        for(int i = 0; i < mapMats.Length; i++){
            mapMatsIndex.Add(mapMats[i], i);
        }
    }

    public static void SaveGrid(Grid grid, string fileName, bool custom){
        GridData gridData = new GridData(grid);
        string json = JsonUtility.ToJson(gridData);
        string path = custom ? customMapsPath : mapdataPath;
        File.WriteAllText(path + "/" + fileName, json);
        Debug.Log(mapdataPath);
    }

    public static Grid LoadGrid(string fileName, bool custom){
        string path = custom ? customMapsPath : mapdataPath;
        string json = File.ReadAllText(path + "/" + fileName);
        GridData gridData = JsonUtility.FromJson<GridData>(json);

        Grid grid = new Grid(gridData.worldPos, gridData.gridSize, gridData.cellSize);
        for (int i = 0; i < gridData.cells.Length; i++)
        {
            GridContent cellContent = (GridContent)gridData.content[i];
            Quaternion cellRotation = gridData.rotation[i];
            Color cellColor = gridData.colors[i];
            Material cellMaterial = mapMats[gridData.material[i]];
            GridInfo cell = new GridInfo(cellContent, cellRotation, cellMaterial, cellColor);
            grid.SetCell(gridData.cells[i], cell);
        }
        return grid;
    }
}
