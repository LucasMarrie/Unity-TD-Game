using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Waves", menuName = "Script Assets/Waves/Wave")]
public class Wave : ScriptableObject{
    [Tooltip("Cooldown before sending next battalion")]
    public float cooldown = 3f;
    public List<Battalion> battalions;
}

[System.Serializable]
public struct Battalion{
    [Tooltip("Amount of times to repeat batallion")]
    public int amount;
    [Tooltip("Units spawn in all start positions")]
    public bool allStarts;
    [Tooltip("Time between Spawns (for same start)")]
    public float spawnDelay;
    [Tooltip("Units to ")]
    public List<UnitGroup> units;
}

[System.Serializable]
public struct UnitGroup{
    public GameObject unit;
    public int amount;
}
