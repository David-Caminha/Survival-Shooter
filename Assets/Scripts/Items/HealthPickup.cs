using CompleteProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int healthAmount = 5;
    public float rotationSpeed = 70;
    public ItemMagnet itemMagnet;
    Vector3 initialPosition;
    float timer = 0;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth.currentHealth < playerHealth.maxHealth)
            {
                playerHealth.Heal(healthAmount);
                Destroy(gameObject);
            }
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
        if (itemMagnet.moving)
            enabled = false;
        timer += Time.deltaTime / 2f;
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        transform.position = new Vector3(initialPosition.x, initialPosition.y + Mathf.PingPong(timer, 0.5f), initialPosition.z);
    }
}
