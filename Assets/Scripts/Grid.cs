using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Grid {
    public GridContent[,,] cells;
    public float cellSize;
    public Vector3 gridSize;
    public Vector3 worldSize;
    public Vector3 worldPos;

    public Grid(Vector3 worldPos, Vector3Int gridSize, float cellSize){
        this.gridSize = gridSize;
        this.cellSize = cellSize;
        this.worldPos = worldPos;
        this.cells = new GridContent[gridSize.x, gridSize.y, gridSize.z];;
        this.worldSize =  new Vector3(gridSize.x, gridSize.y, gridSize.z) * cellSize;
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

        neighbours.ForEach(t => {
            if(InBounds(cell + t[0])){
                t[1] = cell + t[0];
            }else{
                t[1] = Vector3Int.one * -1;
            }
        });
        
        return neighbours;
    }

}

public enum GridContent{
    empty,
    block,
}