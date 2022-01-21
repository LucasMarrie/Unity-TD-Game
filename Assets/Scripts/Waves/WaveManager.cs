using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    MapGenerator map;
    MapGrid grid;

    public WaveList waves;
    int wave;

    List<Vector3Int> startCells;
    
    void Start()
    {
        map = MapGenerator.map;
        grid = map.GetGrid();
        SetStarts();
        NextWave();
    }

    void SetStarts(){
        startCells = new List<Vector3Int>();
        foreach(Vector3Int startCell in grid.startCells){
            if(PathNode.nodes.ContainsKey(startCell)){
                startCells.Add(startCell);
            }
        }
    }

    IEnumerator SendWave(Wave wave){
        int startPos = 0;
        foreach(Battalion battalion in wave.battalions){
            for (int i = 0; i < battalion.amount; i++)
            {
                foreach(UnitGroup units in battalion.units){
                    for (int n = 0; n < units.amount; n++)
                    {
                        if(battalion.allStarts){
                            foreach(Vector3Int startCell in startCells){
                                SpawnUnit(units.unit, startCell);
                            }
                        }else{
                            SpawnUnit(units.unit, startCells[startPos % startCells.Count]);
                        }
                        yield return new WaitForSeconds(battalion.spawnDelay);
                    }
                }
                if(!battalion.allStarts) startPos++;
                yield return new WaitForSeconds(wave.cooldown);
            }
        }
        Debug.Log("Wave sent out");
    }

    void SpawnUnit(GameObject unit, Vector3Int startPosition){
        Instantiate(unit, grid.GridToWorld(startPosition, side : Vector3.up), Quaternion.identity);
    }

    public void NextWave(){
        if(wave < waves.waves.Count){
            StartCoroutine(SendWave(waves.waves[wave++]));
        }else{
            Debug.Log("Out of waves mate");
        }
    }

    public void ResetWaves(){
        wave = 0;
    }


}
