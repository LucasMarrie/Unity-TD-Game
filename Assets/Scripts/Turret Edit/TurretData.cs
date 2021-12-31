using UnityEngine;

[CreateAssetMenu(fileName = "Turret", menuName = "Script Assets/Turrets/Turret")]
public class TurretData : ScriptableObject
{
    public string name;
    public int buildCost;
    public Vector3Int size = Vector3Int.one;
    public GameObject turret;
}

