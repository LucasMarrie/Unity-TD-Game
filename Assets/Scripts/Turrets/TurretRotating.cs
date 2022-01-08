using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretRotating : MonoBehaviour
{
    public float hRotSpeed = 90;
    public float vRotSpeed = 90;

    public Transform hRotor;
    public Transform vRotor;

    Vector3 targetPos;
    Quaternion targetAngle;

    //returns true if rotation is complete
    public bool RotateToTarget(Quaternion angle){
        //rotation
        Vector3 targetAngle = angle.eulerAngles;

        float hAngle = hRotor.rotation.eulerAngles.y;
        float vAngle = vRotor.rotation.eulerAngles.x;

        float hTargetAngle = targetAngle.y;
        float vTargetAngle = targetAngle.x;

        hAngle = Mathf.MoveTowardsAngle(hAngle, hTargetAngle, hRotSpeed * Time.deltaTime);
        vAngle = Mathf.MoveTowardsAngle(vAngle, vTargetAngle, vRotSpeed * Time.deltaTime);

        vRotor.rotation = Quaternion.Euler(vAngle, vRotor.rotation.eulerAngles.y, vRotor.rotation.eulerAngles.z);
        hRotor.rotation = Quaternion.Euler(hRotor.rotation.eulerAngles.x, hAngle, hRotor.rotation.eulerAngles.z);

        if(hAngle == hTargetAngle && vAngle == vTargetAngle){
            return true;
        }
        return false;
    }

}
