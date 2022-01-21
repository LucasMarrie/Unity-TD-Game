using UnityEngine;

[RequireComponent(typeof(TurretRotating))]
[RequireComponent(typeof(TurretTargetting))]
[RequireComponent(typeof(ITurretFiring))]
public class TurretMain : MonoBehaviour
{
    TurretRotating rotatingModule;
    TurretTargetting targettingModule;
    ITurretFiring firingModule;

    // Start is called before the first frame update
    void Start()
    {
        rotatingModule = GetComponent<TurretRotating>();
        targettingModule = GetComponent<TurretTargetting>();
        firingModule = GetComponent<ITurretFiring>();
    }

    // Update is called once per frame
    void Update()
    {
        if(targettingModule.HasTarget()){
            if(rotatingModule.RotateToTarget(targettingModule.GetTargetAngle())){
                firingModule.Fire(targettingModule.Target);
            }
        }
    }
}
