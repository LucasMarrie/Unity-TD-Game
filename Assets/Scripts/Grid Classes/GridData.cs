using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

//class used for saving grid data in file

public class GridData
{
    public static string mapdataPath = Application.dataPath + "/MapData";
    public static string customMapsPath =  mapdataPath + "/Custom";

    static Dictionary<BlockData,int> blockDataDict;

    public float cellSize;
    public Vector3Int gridSize;
    public Vector3 worldPos;

    //grid block positions
    public Vector3Int[] cells;
    public Quaternion[] rotations;
    public int[] blockDatas;
    public int[] shapes;

    public GridData(MapGrid grid){
        cellSize = grid.cellSize;
        gridSize = grid.gridSize;
        worldPos = grid.worldPos;


        List<Vector3Int> cellsList = new List<Vector3Int>();
        List<Quaternion> rotationList = new List<Quaternion>();
        List<int> blockDataList = new List<int>();
        List<int> shapeList = new List<int>();

        for(int x = 0; x < grid.gridSize.x; x++){
            for(int y = 0; y < grid.gridSize.y; y++){
                for(int z = 0; z < grid.gridSize.z; z++){
                    GridInfo cell = grid.cells[x,y,z];
                    BlockData blockData = cell.blockData;
                    if(blockData.content != BlockContent.empty){
                        cellsList.Add(new Vector3Int(x,y,z));
                        rotationList.Add(cell.rotation);
                        shapeList.Add((int)cell.shape);
                        blockDataList.Add(blockDataDict[blockData]);
                    }
                }
            }
        }
        cells = cellsList.ToArray();
        rotations = rotationList.ToArray();
        shapes = shapeList.ToArray();
        blockDatas = blockDataList.ToArray();
    }
    
    public static void InitBlockData(){
        blockDataDict = new Dictionary<BlockData, int>();
        for(int i = 0; i < BlockList.blockDataList.Length; i++){
            blockDataDict.Add(BlockList.blockDataList[i], i);
        }
    }

    public static void SaveGrid(MapGrid grid, string fileName, bool custom){
        GridData gridData = new GridData(grid);
        string json = JsonUtility.ToJson(gridData);
        string path = custom ? customMapsPath : mapdataPath;
        File.WriteAllText(path + "/" + fileName, json);
    }

    public static MapGrid LoadGrid(string fileName, bool custom){
        string path = custom ? customMapsPath : mapdataPath;
        string json = File.ReadAllText(path + "/" + fileName);
        GridData gridData = JsonUtility.FromJson<GridData>(json);

        MapGrid grid = new MapGrid(gridData.worldPos, gridData.gridSize, gridData.cellSize);
        for (int i = 0; i < gridData.cells.Length; i++)
        {
            BlockData blockData = BlockList.blockDataList[gridData.blockDatas[i]];
            Quaternion rotation = gridData.rotations[i];
            Shape shape = (Shape)gridData.shapes[i];
            GridInfo cell = new GridInfo(blockData, rotation, shape);
            grid.SetCell(gridData.cells[i], cell);
        }
        return grid;
    }
}
