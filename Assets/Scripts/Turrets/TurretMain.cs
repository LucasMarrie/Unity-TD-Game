using UnityEngine;

[RequireComponent(typeof(TurretRotating))]
[RequireComponent(typeof(ITurretTargetting))]
[RequireComponent(typeof(ITurretFiring))]
public class TurretMain : MonoBehaviour
{
    TurretRotating rotatingModule;
    ITurretTargetting targettingModule;
    ITurretFiring firingModule;

    // Start is called before the first frame update
    void Start()
    {
        rotatingModule = GetComponent<TurretRotating>();
        targettingModule = GetComponent<ITurretTargetting>();
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
