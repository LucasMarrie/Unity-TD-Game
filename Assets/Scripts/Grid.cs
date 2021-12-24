using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Grid {
    public GridInfo[,,] cells;
    public float cellSize;
    public Vector3Int gridSize;
    public Vector3 worldSize;
    public Vector3 worldPos;

    public Grid(Vector3 worldPos, Vector3Int gridSize, float cellSize){
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
    public GridContent content;
    public Quaternion rotation; 
    public Material material;
    public Color color;

    public static Dictionary<GridContent, Shape> contentShape = new  Dictionary<GridContent, Shape>{
        {GridContent.block, Shape.cube},
        {GridContent.prism, Shape.prism}, 
        {GridContent.pyramid, Shape.pyramid},
    };

    public static GridInfo Empty = new GridInfo(GridContent.empty, Quaternion.identity, null, Color.black);

    public GridInfo(GridContent _content, Quaternion _rotation, Material _material, Color _color){
        content = _content;
        rotation = _rotation;
        material = _material;
        color = _color;
    }

}

public class Shape
{
    public Vector3[] vertices;
    public Dictionary<Vector3,int[]> faces;
    
    public static Shape cube = new Shape(
        new Vector3[]{
            Vector3.forward/2 + Vector3.up/2 + Vector3.left/2, // forward top left 0
            Vector3.forward/2 + Vector3.up/2 + Vector3.right/2, // forward top right 1
            Vector3.forward/2 + Vector3.down/2 + Vector3.left/2, // forward bottom left 2
            Vector3.forward/2 + Vector3.down/2 + Vector3.right/2, // forward bottom right 3
            Vector3.back/2 + Vector3.up/2 + Vector3.left/2, // back top left 4
            Vector3.back/2 + Vector3.up/2 + Vector3.right/2, // back top right 5
            Vector3.back/2 + Vector3.down/2 + Vector3.left/2, // back bottom left 6
            Vector3.back/2 + Vector3.down/2 + Vector3.right/2, // back bottom right 7
        },

        new Dictionary<Vector3,int[]>{
            { Vector3.forward, new int[]{2, 1, 0, 2, 3, 1} },
            { Vector3.back, new int[]{7, 4, 5, 7, 6, 4} },
            { Vector3.up, new int[]{5, 0, 1, 5, 4, 0} },
            { Vector3.down, new int[]{6, 3, 2, 6, 7, 3} },
            { Vector3.left, new int[]{6, 0, 4, 6, 2, 0} },
            { Vector3.right, new int[]{3, 7, 1, 1, 7, 5} },
        }
    );

    //2 vertex face is forward (with 2 vertex top)
    public static Shape prism = new Shape(
        new Vector3[]{
            Vector3.forward/2 + Vector3.down/2 + Vector3.left/2, // forward bottom left 0
            Vector3.forward/2 + Vector3.down/2 + Vector3.right/2, // forward bottom right 1
            Vector3.back/2 + Vector3.up/2 + Vector3.left/2, // back top left 2
            Vector3.back/2 + Vector3.up/2 + Vector3.right/2, // back top right 3
            Vector3.back/2 + Vector3.down/2 + Vector3.left/2, // back bottom left 4
            Vector3.back/2 + Vector3.down/2 + Vector3.right/2, // back bottom right 5
        },
        new Dictionary<Vector3,int[]>{
            { Vector3.back, new int[]{5, 2, 3, 5, 4, 2} },
            { Vector3.down, new int[]{4, 1, 0, 4, 5, 1} },
            { Vector3.left, new int[]{4, 0, 2} },
            { Vector3.right, new int[]{1, 5, 3} },
            { MergeDir(new Vector3[]{Vector3.up, Vector3.forward}), new int[]{0, 3, 2, 0, 1, 3} }
        }
    );

    //2 vertex face is forward  (with 1 vertex top left)
    public static Shape pyramid = new Shape(
        new Vector3[]{
            Vector3.forward/2 + Vector3.down/2 + Vector3.left/2, // forward bottom left 0
            Vector3.forward/2 + Vector3.down/2 + Vector3.right/2, // forward bottom right 1
            Vector3.back/2 + Vector3.up/2 + Vector3.left/2, // back top left 2
            Vector3.back/2 + Vector3.down/2 + Vector3.left/2, // back bottom left 3
            Vector3.back/2 + Vector3.down/2 + Vector3.right/2, // back bottom right 4
        },
        new Dictionary<Vector3,int[]>{
            { Vector3.back, new int[]{4, 3, 2} },
            { Vector3.down, new int[]{4, 1, 0, 0, 3, 4} },
            { Vector3.left, new int[]{3, 0, 2} },
            { MergeDir(new Vector3[]{Vector3.up, Vector3.forward}), new int[]{0, 1, 2} },
            { MergeDir(new Vector3[]{Vector3.up, Vector3.right}), new int[]{1, 4, 2} },
        }
    );


    public Shape(Vector3[] _vertices, Dictionary<Vector3,int[]> _faces){
        vertices = _vertices;
        faces = _faces;
    }

    public static Vector3 MergeDir(Vector3[] dirs){
        Vector3 cumVec = Vector3.zero;
        for(int i = 0; i < dirs.Length; i++){
            cumVec += dirs[i];
        }
        return cumVec.normalized;
    }

}

public enum GridContent{
    empty,
    block,
    prism,
    pyramid
}