using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//interface for turret's targeting module
// public interface ITurretTargetting
// {
//     Transform Target { get; }
//     //validates the existing target, or tries to find a new one if current target is invalid or null, returns false if nothing was found
//     bool HasTarget();
//     Quaternion GetTargetAngle();
    
// }

//interface for turret's firing module
public interface ITurretFiring
{
    float BulletSpeed {get;}
    float BulletRadius {get;}
    void Fire(Transform target);
}

//interface for turret's upgrading/selling
public interface ITurretUpgradable 
{
    List<Vector3Int> occupiedCells {get; set;}
    int sellPrice { get; }
    int? upgradePrice { get; }
    Vector3 windowPosition {get;}
    GameObject Upgrade();
    void Sell();
    
}