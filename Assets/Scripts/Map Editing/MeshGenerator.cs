using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class MeshGenerator
{
    static List<List<int>> triangles = new List<List<int>>();
    static int subMeshCount = 0;
    static int vertCount = 0;
    static List<Vector3> vertices = new List<Vector3>();
    static List<Color> colors = new List<Color>();
    static MapGrid grid;
    static Dictionary<Material,int> matDict = new Dictionary<Material, int>();

    public static Mesh CreateMesh(MapGrid _grid, out Material[] materials){
        grid = _grid;
        grid.LoopGrid((x, y, z) => {
            if(grid.cells[x,y,z].blockData.content != BlockContent.empty){
                RenderSides(new Vector3Int(x,y,z), grid.cells[x,y,z]);
            }
        });

        List<Material> mats = new List<Material>();
        foreach(var key in matDict.Keys){
            mats.Add(key);
        }
        materials = mats.ToArray();

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();  
        mesh.colors = colors.ToArray();
        mesh.subMeshCount = subMeshCount;
        for(int i = 0; i < subMeshCount; i++){
            mesh.SetTriangles(triangles[i].ToArray(), i);
        } 
        mesh.RecalculateNormals();

        ResetVariables();        

        return mesh;
    }


    public static Mesh CreateMesh(GridInfo objInfo, float size, bool hasColor){
        foreach(var dir in ShapeData.shapeDict[objInfo.shape].faces.Keys){
            AddFace(Vector3.zero, size, dir, objInfo);
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles[0].ToArray(); 
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();

        ResetVariables();
        return mesh;
    }

    static void ResetVariables(){
        vertices.Clear();
        triangles.Clear();
        matDict.Clear();
        colors.Clear();
        subMeshCount = 0;
        vertCount = 0;
    }

    static void RenderSides(Vector3Int cell, GridInfo cellInfo){
        //checking all sides of the square to decide wheather a mesh should be assigned
        
        List<Vector3Int[]> neighbours = grid.GetNeighbours(cell);
        HashSet<Vector3> unrenderedFaces = new HashSet<Vector3>();
        foreach(Vector3Int[] nbr in neighbours){
            if(nbr[1] == Vector3.one * -1) continue;
            GridInfo nbrCell = grid.GetCell(nbr[1]);
            BlockContent content = nbrCell.blockData.content;
            Shape shape = nbrCell.shape;
              //future optimisation: add way of detecting if overlapping faces for pyramid and prism use cases
            if(content != BlockContent.empty && shape != Shape.pyramid && shape != Shape.prism){
                unrenderedFaces.Add(nbr[0]);
            }
        }

        foreach(var key in ShapeData.shapeDict[cellInfo.shape].faces.Keys){
            if(!unrenderedFaces.Contains(key)){
                AddFace(grid.GridToWorld(cell, true), grid.cellSize, key, cellInfo);
            }
        }
    }

    //gets coordinates of vertices  of the face depending on the direction of the face
    static void AddFace(Vector3 center, float size, Vector3 dir, GridInfo cellInfo){

        BlockData blockData = cellInfo.blockData;
        ShapeData shape = ShapeData.shapeDict[cellInfo.shape];
        Vector3[] verts = shape.vertices;

        int[] faceTriangles = shape.faces[dir];
        HashSet<int> vertsToAdd = new HashSet<int>(faceTriangles);
        Dictionary<int, int> oldToNewIndex = new Dictionary<int, int>();
        
        for(int i = 0; i < verts.Length; i++){
            if(vertsToAdd.Contains(i)){
                Vector3 newVertex = cellInfo.rotation * verts[i] * size + center;
                vertices.Add(newVertex);
                colors.Add(blockData.color);
                oldToNewIndex.Add(i, vertCount);
                vertCount++;
            }
        }

        Material mat = blockData.material;
        if(!matDict.ContainsKey(mat)){
            matDict.Add(mat, subMeshCount);
            triangles.Add(new List<int>());
            subMeshCount++;
        }

        for(int i = 0; i < faceTriangles.Length; i++){
            triangles[matDict[mat]].Add(oldToNewIndex[faceTriangles[i]]);
        }
    }
}
