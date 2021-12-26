using UnityEngine;
using UnityEditor;
using System.Diagnostics;

[CustomEditor(typeof(MapGenerator))]
public class MapGen_Editor : Editor
{
    public override void OnInspectorGUI(){
        base.OnInspectorGUI();
        MapGenerator mapgen = (MapGenerator)target;
        Transform floor = mapgen.floor;
        floor.position = mapgen.gameObject.transform.position + Vector3.down * (mapgen.gridSize.y + 1)/2 * mapgen.cellSize;
        floor.localScale = new Vector3(mapgen.gridSize.x, 1, mapgen.gridSize.z) * mapgen.cellSize;

        GUILayout.BeginHorizontal();

        if(GUILayout.Button("Save Map")){
            GridData.SaveGrid(mapgen.GetGrid(), "Mabamba.json", false);
        }

        if(GUILayout.Button("Load Map")){
             mapgen.SetGrid(GridData.LoadGrid("Mabamba.json", false));
        }

        GUILayout.EndHorizontal();
    }
}