using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, IProjectile
{
    public bool hitscan = false;
    public float radius;
    public LayerMask CollisionLayer = 0b_0001_1100_0000;

    [Header("Effects")]
    [Tooltip("Audio + Particle Effect")]
    [SerializeField] GameObject destructionEffect;
    [SerializeField] float effectLifetime = 10f;

    float speed;
    int damage;

    Vector3 prevPosition;
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        if(hitscan){
            RaycastHit hit;
            if(Physics.Raycast(transform.position, transform.forward, out hit, CollisionLayer)){
                //add damage stuff here
                Explode(hit.point, hit.normal);
            }
        }else{
            prevPosition = transform.position;
            rb = GetComponent<Rigidbody>();
            rb.velocity = transform.forward * speed;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = transform.position - prevPosition;
        RaycastHit hit;
        if(Physics.SphereCast(prevPosition, radius, dir, out hit, dir.magnitude, CollisionLayer)){

            Explode(hit.point, hit.normal);
        }
        prevPosition = transform.position;
    }

    void Explode(Vector3 hitPosition, Vector3 hitNormal){
        GameObject destructionObj = Instantiate(destructionEffect, hitPosition, Quaternion.identity);
        destructionObj.SetActive(true);
        Destroy(destructionObj, effectLifetime);
        Destroy(gameObject);
    }

    public void SetStats(float speed, int damage){
        this.speed = speed;
        this.damage = damage;   
    }
}
