using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    static List<int> triangles;
    static int vertCount;
    static List<Vector3> vertices;
    static Dictionary<Vector3, int> vertMap;
    static Grid grid;
    public static Mesh CreateMesh(Grid _grid){
        grid = _grid;
        triangles = new List<int>();
        vertCount = 0;
        vertices = new List<Vector3>();
        vertMap = new Dictionary<Vector3, int>();

        for(int x = 0; x < grid.gridSize.x; x++){
            for(int y = 0; y < grid.gridSize.y; y++){
                for(int z = 0; z < grid.gridSize.z; z++){
                    if(grid.cells[x,y,z] != GridContent.empty){
                        RenderSides(new Vector3Int(x,y,z));
                    }
                }
            }
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();        
        mesh.RecalculateNormals();

        return mesh;
    }

    static void RenderSides(Vector3Int cell){
        //checking all sides of the square to decide wheather a mesh should be assigned
        List<Vector3Int[]> neighbours = grid.GetNeighbours(cell);
        foreach(Vector3Int[] nbr in neighbours){
            if(nbr[1] == Vector3Int.one * -1 || grid.cells[nbr[1].x, nbr[1].y, nbr[1].z] == GridContent.empty){
                List<Vector3> verts = GetFaceVertices(cell, nbr[0]);
                AssignVertices(verts);
            }
        }
    }

    //adding vertices to a (coordinate, index) dictionary to avoid repitition of vertices
    static void AssignVertices(List<Vector3> verts){
        foreach(Vector3 vert in verts){
            if(!vertMap.ContainsKey(vert)){
                vertices.Add(vert);
                vertMap.Add(vert, vertCount);
                vertCount++;
            }
        }
        CreateTriangle(verts[0], verts[1], verts[2]);
        CreateTriangle(verts[0], verts[2], verts[3]);
    }

    static void CreateTriangle(Vector3 a, Vector3 b, Vector3 c){
        triangles.Add(vertMap[a]);
        triangles.Add(vertMap[b]);
        triangles.Add(vertMap[c]);  
    }

    //gets coordinates of vertices  of the face depending on the direction of the face
    static List<Vector3> GetFaceVertices(Vector3Int cell, Vector3Int dir){

        List<Vector3> verts; 
        Vector3 cellPos = grid.GridToWorld(cell, true, dir);
        float halfCell = grid.cellSize/2;

        if(dir == Vector3Int.up || dir == Vector3Int.down){
            verts = new List<Vector3>{
                new Vector3(cellPos.x + halfCell, cellPos.y, cellPos.z + halfCell),
                new Vector3(cellPos.x + halfCell, cellPos.y, cellPos.z - halfCell),
                new Vector3(cellPos.x - halfCell, cellPos.y, cellPos.z - halfCell),
                new Vector3(cellPos.x - halfCell, cellPos.y, cellPos.z + halfCell),
            };
        }
        else if(dir == Vector3Int.left || dir == Vector3Int.right){
            verts = new List<Vector3>{
                new Vector3(cellPos.x, cellPos.y  + halfCell, cellPos.z + halfCell),
                new Vector3(cellPos.x, cellPos.y  - halfCell, cellPos.z + halfCell),
                new Vector3(cellPos.x , cellPos.y - halfCell, cellPos.z - halfCell),
                new Vector3(cellPos.x, cellPos.y + halfCell, cellPos.z - halfCell),
            };
        }
        else{
            verts = new List<Vector3>{
                new Vector3(cellPos.x  + halfCell, cellPos.y  + halfCell, cellPos.z),
                new Vector3(cellPos.x - halfCell, cellPos.y  + halfCell, cellPos.z ),
                new Vector3(cellPos.x - halfCell , cellPos.y - halfCell, cellPos.z),
                new Vector3(cellPos.x + halfCell, cellPos.y - halfCell, cellPos.z),
            };
        }

        if(dir.x < 0 || dir.y < 0 || dir.z < 0) verts.Reverse();

        return verts;
    }
}
