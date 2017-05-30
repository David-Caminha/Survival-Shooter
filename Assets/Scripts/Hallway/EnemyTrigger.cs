using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTrigger : MonoBehaviour {

    public CompleteProject.EnemyManager[] bunnySpawners;
    public CompleteProject.EnemyManager[] bearSpawners;
    public CompleteProject.EnemyManager[] elephantSpawners;
    public BoxCollider trigger;
    public KitchenEntrance kitchenEntrance;

    EmotionManager emotionManager;
    float timeSpawning;

    void Start()
    {
        emotionManager = EmotionManager.Instance;
        if(emotionManager != null && emotionManager.emotionalMechanics)
        {
            if (emotionManager.tutorialTime > 60)
            {
                timeSpawning = 13f;

                for (int i = 0; i < bunnySpawners.Length; i++)
                    bunnySpawners[i].spawnInterval = 1f;

                for (int i = 0; i < bearSpawners.Length; i++)
                    bearSpawners[i].spawnInterval = 1f;

                for (int i = 0; i < elephantSpawners.Length; i++)
                    elephantSpawners[i].spawnInterval = 3.5f;
            }
            else if (emotionManager.tutorialTime > 40)
            {
                timeSpawning = 16f;

                for (int i = 0; i < bunnySpawners.Length; i++)
                    bunnySpawners[i].spawnInterval = 2f;

                for (int i = 0; i < bearSpawners.Length; i++)
                    bearSpawners[i].spawnInterval = 2f;

                for (int i = 0; i < elephantSpawners.Length; i++)
                    elephantSpawners[i].spawnInterval = 7.5f;
            }
            else if (emotionManager.tutorialTime > 30)
            {
                timeSpawning = 12f;

                for (int i = 0; i < bunnySpawners.Length; i++)
                    bunnySpawners[i].spawnInterval = 2.5f;

                for (int i = 0; i < bearSpawners.Length; i++)
                    bearSpawners[i].spawnInterval = 2.5f;

                for (int i = 0; i < elephantSpawners.Length; i++)
                    elephantSpawners[i].spawnInterval = 10f;
            }
            else
            {
                timeSpawning = 15f;

                for (int i = 0; i < bunnySpawners.Length; i++)
                    bunnySpawners[i].spawnInterval = 4f;

                for (int i = 0; i < bearSpawners.Length; i++)
                    bearSpawners[i].spawnInterval = 4f;

                for (int i = 0; i < elephantSpawners.Length; i++)
                    elephantSpawners[i].spawnInterval = 12f;
            }
        }
        else
        {
            timeSpawning = 12f;
        }
    }

	void OnTriggerEnter (Collider other) {
        if(other.gameObject.tag == "Player")
        {
            for (int i = 0; i < bunnySpawners.Length; i++)
            {
                bunnySpawners[i].StartSpawning(timeSpawning);
            }
            for (int i = 0; i < bearSpawners.Length; i++)
            {
                bearSpawners[i].StartSpawning(timeSpawning);
            }
            for (int i = 0; i < elephantSpawners.Length; i++)
            {
                elephantSpawners[i].StartSpawning(timeSpawning);
            }
            trigger.enabled = false;
            Invoke("EnableKitchen", 30f);
        }
	}

    void EnableKitchen()
    {
        kitchenEntrance.enabled = true;
        Destroy(this.gameObject);
    }
}
