using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Turret", menuName = "Script Assets/Turrets/Turret List")]
public class TurretList : ScriptableObject {
    public List<TurretData> turrets;
}


