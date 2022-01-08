using System.Collections.Generic;
using UnityEngine;

public class HitscanTargetting : MonoBehaviour, ITurretTargetting
{
    Transform target;
    public float range;
    [Space]
    public float minHAngle = -180;
    public float maxHAngle = 180;
    [Space]
    public float minVAngle;
    public float maxVAngle;
    [Space]
    public float bulletRadius;
    public float bulletSpeed;
    public Transform targettingCenter;
    public LayerMask targettingLayers = 0b_0000_1000_0000;
    public LayerMask blockingLayers = 0b_0001_1100_0000;

    public Transform Target{
        get {return target;}
    }

    bool FindTarget(){
        Vector3 position = targettingCenter.position;
        Collider[] targetCols = Physics.OverlapSphere(targettingCenter.position, range, targettingLayers);
        List<Transform> targets = new List<Transform>();
        HashSet<Transform> addedTargets = new HashSet<Transform>();
        foreach(Collider targetCol in targetCols){
            Transform targetCenter = targetCol.transform.root.GetComponent<ITargetable>().targetCenter;
            if(!addedTargets.Contains(targetCenter)){
                targets.Add(targetCenter);
                addedTargets.Add(targetCenter);    
            }
        }
        targets.Sort((t1, t2) => ((t1.position - position).sqrMagnitude).CompareTo((t2.position - position).sqrMagnitude));
        foreach(Transform tgt in targets){
            if(ValidTarget(tgt.position)){
                target = tgt;
                return true;
            }
        }
        return false;
    }    

    bool ValidTarget(Vector3 position){
        Vector3 direction = position - targettingCenter.position;
        if(direction.sqrMagnitude > range * range){
            return false;
        }
        Quaternion rotation = Quaternion.LookRotation(direction - transform.forward);
        if(!RotationInRange(rotation)){
            return false;
        }
        RaycastHit hit;
        if(bulletRadius > 0){
            if(Physics.SphereCast(targettingCenter.position, bulletRadius, direction, out hit, range, blockingLayers)){
                if((targettingLayers & 1 << hit.transform.gameObject.layer) == 0){
                    return false;
                }
            }
        }else{
            if(Physics.Raycast(targettingCenter.position, direction, out hit, range, blockingLayers)){ 
                if((targettingLayers & 1 << hit.transform.gameObject.layer) == 0){
                    return false;
                }
            }
        }
        return true;
    }

    bool RotationInRange(Quaternion rotation){
        float hRot = NormalizeAngle(rotation.eulerAngles.y);
        float vRot = NormalizeAngle(rotation.eulerAngles.x);
        if(hRot == Mathf.Clamp(hRot,  minHAngle, maxHAngle) && vRot == Mathf.Clamp(vRot, -maxVAngle, -minVAngle)){  //max and min are flipped because unity calculates it that way
            return true;                                                   
        }
        return false;
    }

    float NormalizeAngle(float a){
        if(a > 180)
            return a - 360;
        return a;
    }

    public bool HasTarget(){
        if(target != null){
            if(ValidTarget(target.position)){
                return true;
            }
        }
        return FindTarget();
    }

    public Quaternion GetTargetAngle(){
        Vector3 direction = target.position - targettingCenter.position;
        Quaternion rotation = Quaternion.LookRotation(direction.normalized);
        return rotation;
    }
}
