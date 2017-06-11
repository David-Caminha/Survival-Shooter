using CompleteProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePickup : MonoBehaviour {

    public int damageIncrease = 30;
    public float rotationSpeed = 70;
    public float buffTime = 7.5f;
    Vector3 initialPosition;
    float timer = 0;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerShooting playerShooting = other.GetComponentInChildren<PlayerShooting>();
            playerShooting.DamageAmp(damageIncrease, buffTime);
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    void Start()
    {
        initialPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime / 2f;
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        transform.position = new Vector3(initialPosition.x, initialPosition.y + Mathf.PingPong(timer, 0.5f), initialPosition.z);
    }
}
