using System.Collections.Generic;
using UnityEngine;

public class ShapeData
{
    public Vector3[] vertices;
    public Dictionary<Vector3,int[]> faces;
    public Vector3Int traversableNormal;

    public static ShapeData cube = new ShapeData(
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
    public static ShapeData prism = new ShapeData(
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
        },
        Vector3Int.forward + Vector3Int.up
    );

    //2 vertex face is forward  (with 1 vertex top left)
    public static ShapeData pyramid = new ShapeData(
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

    public static Dictionary<Shape, ShapeData> shapeDict = new Dictionary<Shape, ShapeData>{
        {Shape.block, ShapeData.cube},
        {Shape.prism, ShapeData.prism}, 
        {Shape.pyramid, ShapeData.pyramid},
    };

    public ShapeData(Vector3[] _vertices, Dictionary<Vector3,int[]> _faces, Vector3Int _traversableNormal = default){
        vertices = _vertices;
        faces = _faces;
        traversableNormal = _traversableNormal;
    }

    public static Vector3 MergeDir(Vector3[] dirs){
        Vector3 cumVec = Vector3.zero;
        for(int i = 0; i < dirs.Length; i++){
            cumVec += dirs[i];
        }
        return cumVec.normalized;
    }

}

public enum Shape{
    none,
    block,
    prism,
    pyramid
}
