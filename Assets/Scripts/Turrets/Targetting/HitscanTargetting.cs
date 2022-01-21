using System.Collections.Generic;
using UnityEngine;

public class HitscanTargetting : TurretTargetting
{
    protected override bool ValidTarget(Vector3 position)
    {
        Quaternion rotation = AngleToTarget(position);
        if(!RotationInRange(rotation) || !DistanceInRange(position)){
            return false;
        }
        RaycastHit hit;
        Vector3 direction = position - targettingCenter.position;
        if(bulletRadius > 0){
            if(Physics.SphereCast(targettingCenter.position, bulletRadius, direction, out hit, maxRange, blockingLayers)){
                if((targettingLayers & 1 << hit.transform.gameObject.layer) == 0){
                    return false;
                }
            }
        }else{
            if(Physics.Raycast(targettingCenter.position, direction, out hit, maxRange, blockingLayers)){ 
                if((targettingLayers & 1 << hit.transform.gameObject.layer) == 0){
                    return false;
                }
            }
        }
        return true;
    }


    protected override Quaternion AngleToTarget(Vector3 targetPos){
        Vector3 direction = targetPos - targettingCenter.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        return rotation;
    }

}
