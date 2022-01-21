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

    [Header("Effect")]
    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] AudioSource shootingSFX;

    [Header("Bullet Stats")]
    public float speed;
    public float radius;
    public int damage;
    public float BulletSpeed { get => speed;}
    public float BulletRadius { get => radius;}

    [Header("Ammo and Reloading")]
    public bool infiniteAmmo = true;
    public int maxAmmo;
    int ammo;
    public float reloadTime;
    [Tooltip("Time before autoreloading")]
    public float autoReloadTime;
    //to-do: add a check within a radius bigger than range to know when it's safe to reload

    void Start(){
        Reload();
    }

    void Update(){
        if(!infiniteAmmo && ammo != maxAmmo && nextTimeToFire + autoReloadTime < Time.time){
            Reload();
        }
    }

    public void Fire(Transform target){
        if(nextTimeToFire > Time.time){
            return;
        }
        if(!infiniteAmmo) {
            if(ammo == 0){
                Reload();
                return;
            }
            ammo--;
        }
        muzzleFlash.Play();
        shootingSFX.Play();
        nextTimeToFire = Time.time + firingCooldown;

        GameObject newBullet = Instantiate(bullet, firingPoint.position, firingPoint.rotation);
        newBullet.GetComponent<IProjectile>().SetStats(speed, damage);
        Destroy(newBullet, 20f);
    }

    void Reload(){
        nextTimeToFire = Time.time + reloadTime;
        ammo = maxAmmo;
    }
}
