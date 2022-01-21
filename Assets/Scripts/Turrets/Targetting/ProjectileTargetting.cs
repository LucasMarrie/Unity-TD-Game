using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTargetting : TurretTargetting
{
    float distanceIncrement = 1f;

    List<Vector3> trajectory;

    new void Start(){
        base.Start();
        float optimalAngle = Mathf.Clamp(45, minHAngle, maxHAngle);
        maxRange = ComputeRange(optimalAngle);
    }

    new void OnDrawGizmos(){
        base.OnDrawGizmos();
        if(trajectory != null){
            Gizmos.color = Color.red;
            foreach(Vector3 point in trajectory){
                Gizmos.DrawSphere(point, 0.1f); 
            }
        }
    }

    protected override bool ValidTarget(Vector3 position)
    {
        Quaternion firingAngle = AngleToTarget(position);
        if(!RotationInRange(firingAngle)){
            return false;
        }
        Vector3 dir = position - targettingCenter.position;
        if(ComputeAngle(dir) is float angle){
            angle *= Mathf.Deg2Rad;
            Vector3 startPos = targettingCenter.position + firingAngle * Vector3.forward * ((targettingCenter.position - muzzle.position).magnitude + bulletRadius);
            dir.y = 0;
            float timeIncrement = distanceIncrement / bulletSpeed; 
            float t = 0;
            trajectory = new List<Vector3>();
            while(true){
                float hVal = HFunction(angle, t);
                float vVal = VFunction(angle, t);
                t += timeIncrement;
                if((dir.normalized * hVal).sqrMagnitude >= dir.sqrMagnitude){
                    break;
                }
                Vector3 point = startPos + dir.normalized * hVal + Vector3.up * vVal;
                trajectory.Add(point);
                if(Physics.CheckSphere(point, bulletRadius, blockingLayers)){
                    if(Physics.CheckSphere(point, bulletRadius, targettingLayers)){
                        break;
                    }else{
                        return false;
                    }
                }
            }
        }else{
            return false;
        }
        
        return true;
    }

    float HFunction(float angle, float t){
        return bulletSpeed * Mathf.Cos(angle) * t;
    }

    float VFunction(float angle, float t){
        return bulletSpeed * Mathf.Sin(angle) * t + (Physics.gravity.y) * t * t / 2;
    }

    protected override Quaternion AngleToTarget(Vector3 targetPos)
    {
        Vector3 dir = targetPos - targettingCenter.position;
        float? angle = ComputeAngle(dir);
        if(angle == null){
            return Quaternion.identity;
        }
        dir.y = 0;
        return Quaternion.LookRotation( Quaternion.AngleAxis((float)-angle, new Vector3(dir.z, 0, -dir.x)) * dir);
    }

    float? ComputeAngle(Vector3 dir){
        float y = dir.y;
        dir.y = 0;
        float x = dir.magnitude;
        float g = -Physics.gravity.y;
        float squareV = bulletSpeed * bulletSpeed; 
        float unSquaredValue = squareV * squareV - g * (g*x*x + 2*squareV*y);
        if(unSquaredValue < 0){
            return null;
        }
        float squaredValue = Mathf.Sqrt(unSquaredValue);
        float angle1 = Mathf.Atan((squareV + squaredValue) / (g*x)) * Mathf.Rad2Deg; 
        float angle2 = Mathf.Atan((squareV - squaredValue) / (g*x)) * Mathf.Rad2Deg; 
        
        if(Mathf.Clamp(angle1, minVAngle, maxVAngle) == angle1){
            return angle1;
        }else{
            return angle2;
        }

        
    }

    float ComputeRange(float angle){
        float g = -Physics.gravity.y;

        float t = 2 * bulletSpeed * Mathf.Sin(angle * Mathf.Deg2Rad) / g;
        float range = bulletSpeed * t * Mathf.Cos(angle * Mathf.Deg2Rad);

        return range;
    }
}
