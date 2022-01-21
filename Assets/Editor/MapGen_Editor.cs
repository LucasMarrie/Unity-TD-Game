using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGen_Editor : Editor
{
    public override void OnInspectorGUI(){
        base.OnInspectorGUI();
        MapGenerator mapgen = (MapGenerator)target;
        Transform floor = mapgen.floor;
        if(floor != null){
        floor.position = mapgen.gameObject.transform.position + Vector3.down * (mapgen.gridSize.y + 1)/2 * mapgen.cellSize;
        floor.localScale = new Vector3(mapgen.gridSize.x, 1, mapgen.gridSize.z) * mapgen.cellSize;
        }

        GUILayout.BeginHorizontal();

        if(GUILayout.Button("Save Map")){
            GridData.SaveGrid(mapgen.GetGrid(), MapGenerator.mapName, false);
        }

        if(GUILayout.Button("Load Map")){
             mapgen.SetGrid(GridData.LoadGrid(MapGenerator.mapName, false));
        }

        GUILayout.EndHorizontal();

        if(GUILayout.Button("Pathfind")){
             mapgen.TestPathfinder();
        }
    }
}
