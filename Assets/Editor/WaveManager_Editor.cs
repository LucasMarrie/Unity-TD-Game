using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaveManager))]
public class WaveManager_Editor : Editor
{
    public override void OnInspectorGUI(){
        base.OnInspectorGUI();

        WaveManager waveManager = (WaveManager)target;
        
        if(GUILayout.Button("Send Next Wave")){
             waveManager.NextWave();
        }
    }
}
