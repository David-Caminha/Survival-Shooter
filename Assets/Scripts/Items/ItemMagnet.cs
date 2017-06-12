using CompleteProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMagnet : MonoBehaviour {
    
    public float speed;

    EmotionManager emotionManager;
    public Transform player;
    public PlayerHealth playerHealth;
    public bool moving;

    // Use this for initialization
    void Start ()
    {
        emotionManager = EmotionManager.Instance;
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject.transform;
        playerHealth = playerObject.GetComponent<PlayerHealth>();
    }
	
	// Update is called once per frame
	void Update () {
        Debug.Log(playerHealth.currentHealth);
        if(!moving)
        {
            if (emotionManager && emotionManager.emotionalMechanics)
            {
                if (emotionManager.AverageArousal() > 3 && emotionManager.AverageValence() < 2)
                {
                    moving = true;
                }
            }
            else
            {
                if (playerHealth.currentHealth < 25)
                {
                    moving = true;
                }
            }
        }
        else
        {
            Vector3 movement = player.position - transform.position;
            movement = movement.normalized * speed * Time.unscaledDeltaTime;
            transform.Translate(movement, Space.World);
        }
	}
}
