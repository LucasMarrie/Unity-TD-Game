using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardShooting : MonoBehaviour, ITurretFiring
{
    public GameObject bullet;
    public Transform firingPoint;
    [Tooltip("Time between shots")]
    public float firingCooldown;
    float nextTimeToFire;

    [Header ("Bullet Stats")]
    public float speed;
    public int damage;

    [Header("Ammo and Reloading")]
    public bool infiniteAmmo = true;
    public int maxAmmo;
    int ammo;
    public float reloadTime;
    [Tooltip("Time before autoreloading")]
    public float autoReloadTime;
    //to-do: add a check within a radius bigger than range to know when it's safe to reload

    void Update(){
        if(!infiniteAmmo && nextTimeToFire + autoReloadTime > Time.time){
            Reload();
        }
    }

    public void Fire(Transform target){
        if(nextTimeToFire > Time.time){
            return;
        }
        if(!infiniteAmmo && ammo == 0) {
            Reload();
            return;
        }
    
        nextTimeToFire = Time.time + firingCooldown;

        GameObject newBullet = Instantiate(bullet, firingPoint.position, firingPoint.rotation);
        newBullet.GetComponent<IProjectile>().SetStats(speed, damage);
        Destroy(newBullet, 20f);
    }

    void Reload(){
        nextTimeToFire = Time.time + reloadTime;
    }
}
