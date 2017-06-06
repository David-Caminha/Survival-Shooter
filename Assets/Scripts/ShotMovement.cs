using CompleteProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotMovement : MonoBehaviour {

    public float speed = 12f;
    public int damage;
    public LayerMask mask;

    Rigidbody shotRigidbody;
    BulletManager bulletManager;

	// Use this for initialization
	void Start () {
        shotRigidbody = GetComponent<Rigidbody>();
        bulletManager = BulletManager.Instance;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        shotRigidbody.MovePosition(transform.position + transform.forward * speed * Time.fixedDeltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(damage);
            bulletManager.StopBullet(gameObject);
        }
        else if(!(other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Spectator")) && (1<<other.gameObject.layer & mask) != 0)
        {
            bulletManager.StopBullet(gameObject);
        }
    }
}
