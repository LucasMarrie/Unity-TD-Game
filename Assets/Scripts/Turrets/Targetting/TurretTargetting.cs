using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TurretTargetting : MonoBehaviour
{
    Transform target;
    [Header("Distance Range")]
    public float minRange;
    public float maxRange;
    public bool ignoreVerticalRange;
    float heightExtents = 20; // for ignoring vertical range
    [Header("Angle Range")]
    public float minHAngle = -180;
    public float maxHAngle = 180;
    [Space]
    public float minVAngle;
    public float maxVAngle;

    //turretInfo
    protected float bulletRadius;
    protected float bulletSpeed;
    
    [Header("Targetting")]
    public Transform targettingCenter;
    public Transform muzzle;
    public LayerMask targettingLayers = 0b_0000_1000_0000;
    public LayerMask blockingLayers = 0b_0001_1100_0000;

    public Transform Target{
        get {return target;}
    }

    protected void Start(){
        bulletRadius = GetComponent<ITurretFiring>().BulletRadius;
        bulletSpeed = GetComponent<ITurretFiring>().BulletSpeed;
    }

    protected void OnDrawGizmos(){
        Gizmos.DrawWireSphere(targettingCenter.position, maxRange);
    }

    bool FindTarget(){
        Vector3 position = targettingCenter.position;
        Collider[] targetCols = ignoreVerticalRange ? 
        Physics.OverlapCapsule(targettingCenter.position + Vector3.up * heightExtents, targettingCenter.position - Vector3.up * heightExtents, maxRange, targettingLayers)
        : Physics.OverlapSphere(targettingCenter.position, maxRange, targettingLayers);

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

    protected abstract bool ValidTarget(Vector3 position);

    protected bool DistanceInRange(Vector3 position){
        Vector3 dir = targettingCenter.position - position;
        if(ignoreVerticalRange) dir.y = 0;
        float sqrDistance = dir.sqrMagnitude;
        if(sqrDistance < minRange * minRange || sqrDistance > maxRange * maxRange){
            return false;
        }else{
            return true;
        }
    }

    protected bool RotationInRange(Quaternion rotation){
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
        return AngleToTarget(target.position);
    }

    protected abstract Quaternion AngleToTarget(Vector3 targetPos);
}
