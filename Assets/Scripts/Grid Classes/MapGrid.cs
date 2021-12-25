using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapGrid {
    public GridInfo[,,] cells;
    public float cellSize;
    public Vector3Int gridSize;
    public Vector3 worldSize;
    public Vector3 worldPos;

    public MapGrid(Vector3 worldPos, Vector3Int gridSize, float cellSize){
        this.gridSize = gridSize;
        this.cellSize = cellSize;
        this.worldPos = worldPos;
        this.cells = new GridInfo[gridSize.x, gridSize.y, gridSize.z];;
        this.worldSize =  new Vector3(gridSize.x, gridSize.y, gridSize.z) * cellSize;
        ReInitialise();
    }

    void ReInitialise(){
        for(int x = 0; x < gridSize.x; x++){
            for(int y = 0; y < gridSize.y; y++){
                for(int z = 0; z < gridSize.z; z++){
                    cells[x,y,z] = GridInfo.Empty;
                }
            }
        }
    }

    //side allows to pick the center of the desired face with a unit vector like Vector3.up
    public Vector3 GridToWorld(Vector3 gridPoint, bool local = false, Vector3 side = default){
        Vector3 worldPoint = (local ? Vector3.zero : worldPos) + (gridPoint + Vector3.one / 2) * cellSize - worldSize/2 + side * cellSize/2;
        return worldPoint;
    }

    public Vector3Int WorldToGrid(Vector3 worldPoint, bool local = false){
        Vector3Int point = Vector3Int.FloorToInt((worldPoint - (local ? Vector3.zero : worldPos) + worldSize/2) / cellSize);
        if(InBounds(point))
            return point;
        return Vector3Int.one * -1;
    }

    public bool InBounds(Vector3Int cell){
        if(cell.x < 0 || cell.y < 0 || cell.z < 0 ||
            cell.x >= gridSize.x || cell.y >= gridSize.y || cell.z >= gridSize.z){
                return false;
        }
        return true;
    }

    public GridInfo GetCell(Vector3Int point){
        return cells[point.x, point.y, point.z];
    }

    public void SetCell(Vector3Int point, GridInfo gridInfo){
        cells[point.x, point.y, point.z] = gridInfo;
    }

    //returns a list of arrays of size 2 where the [0] is direction, and [1] the neighbooring cell
    //if there is no neighbour, direction will be set to a negative vector3.one
    public List<Vector3Int[]> GetNeighbours(Vector3Int cell){
        List<Vector3Int[]> neighbours = new List<Vector3Int[]>{
            new Vector3Int[]{Vector3Int.up, Vector3Int.zero},
            new Vector3Int[]{Vector3Int.down, Vector3Int.zero},
            new Vector3Int[]{Vector3Int.left, Vector3Int.zero},
            new Vector3Int[]{Vector3Int.right, Vector3Int.zero},
            new Vector3Int[]{Vector3Int.forward, Vector3Int.zero},
            new Vector3Int[]{Vector3Int.back, Vector3Int.zero}
        };

        for(int i = 0; i < neighbours.Count; i++){
            Vector3Int[] n = neighbours[i];
            Vector3Int trueN = Vector3Int.RoundToInt(cells[cell.x, cell.y, cell.z].rotation * new Vector3(n[0].x, n[0].y, n[0].z));
            if(InBounds(cell + trueN)){
                n[1] = cell + trueN;
            }else{
                n[1] = Vector3Int.one * -1;
            }
        }
        
        return neighbours;
    }

}

public class GridInfo
{
    public BlockData blockData;
    public Quaternion rotation; 
    public Shape shape;

    public static GridInfo Empty = new GridInfo(
        new BlockData{name = "empty", content = BlockContent.empty},
        Quaternion.identity,
        Shape.none
    );

    public GridInfo(BlockData _blockData, Quaternion _rotation, Shape _shape){
        blockData = _blockData;
        rotation = _rotation;
        shape = _shape;
    }

}

[System.Serializable]
public struct BlockData {
    public string name;
    public BlockContent content;
    public Shape[] shapes;
    public Material material;
    public Color color;
}


public enum BlockContent{
    empty,
    buildable,
    path,
}




