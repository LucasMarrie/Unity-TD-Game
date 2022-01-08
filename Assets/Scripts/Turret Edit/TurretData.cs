using UnityEngine;

[CreateAssetMenu(fileName = "Turret", menuName = "Script Assets/Turrets/Turret")]
public class TurretData : ScriptableObject
{
    public string Name;
    public int buildCost;
    public Vector3Int size = Vector3Int.one;
    public GameObject turret;
}

