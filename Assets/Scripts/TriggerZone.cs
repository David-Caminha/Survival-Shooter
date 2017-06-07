using CompleteProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerZone : MonoBehaviour {

    public GameObject[] zones;
    public float spawnTimeMult = 1;

    EmotionManager emotionManager;
    float spawnTime;

    // Use this for initialization
    void Start () {

        emotionManager = EmotionManager.Instance;
        if (emotionManager != null && emotionManager.emotionalMechanics)
        {
            if (emotionManager.tutorialTime > 60)
            {
                spawnTime = 14f;
            }
            else if (emotionManager.tutorialTime > 40)
            {
                spawnTime = 17f;
            }
            else if (emotionManager.tutorialTime > 30)
            {
                spawnTime = 17f;
            }
            else
            {
                spawnTime = 15f;
            }
        }
        else
        {
            spawnTime = 17f;
        }
    }
	
	// Update is called once per frame
	void OnTriggerEnter (Collider other) {

        if(other.gameObject.CompareTag("Player"))
        {
            for(int i = 0; i < zones.Length; i++)
            {
                EnemyManager[] spawners = zones[i].GetComponentsInChildren<EnemyManager>();
                for (int j = 0; j < spawners.Length; j++)
                {
                    spawners[j].StartSpawning(spawnTime * spawnTimeMult);
                }
            }
        }
    }
}
