using CompleteProject;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoredomManager : MonoBehaviour
{

    EmotionManager emotionManager;

    public Transform player;
    public GameObject[] zones;
    public float mtth = 60; //Mean Time To Happen in seconds
    public float spawnTime = 10f;

    float spawnProb;
    float timeSinceLastEvent;

    // Use this for initialization
    void Start()
    {
        emotionManager = EmotionManager.Instance;

        int numChecks = (int)(mtth / 5);
        spawnProb = 1 - (Mathf.Pow(0.5f, 1f / numChecks));

        timeSinceLastEvent = 0;
        InvokeRepeating("CheckSpawn", 5, 5);
    }

    void CheckSpawn()
    {
        timeSinceLastEvent += 5f;
        if (timeSinceLastEvent > 10f)
        {
            float timeMult = timeSinceLastEvent / 45f + 1f / 6f;
            float emotionMult = 1f;
            if (emotionManager)
                emotionMult = GetEmotionMult();
            float rand = UnityEngine.Random.Range(0f, 1f);
            if (rand <= spawnProb * timeMult * emotionMult)
            {
                SpawnEnemies();
            }
        }
    }

    private void SpawnEnemies()
    {
        float minDist = float.MaxValue;
        GameObject closestZone = null;
        for (int i = 0; i < zones.Length; i++)
        {
            float dist = Vector3.Distance(player.position, zones[i].transform.position);
            if (dist >= 25 && dist < minDist)
            {
                closestZone = zones[i];
            }
        }
        if (closestZone)
        {
            EnemyManager[] spawners = closestZone.GetComponentsInChildren<EnemyManager>();
            for (int i = 0; i < spawners.Length; i++)
            {
                spawners[i].StartSpawning(spawnTime);
            }
            ResetTimer();
        }
    }

    private float GetEmotionMult()
    {
        float emotionMult = 1f;

        if (emotionManager.emotionalMechanics)
        {
            double avgArousal = emotionManager.AverageArousal();
            double avgValence = emotionManager.AverageValence();

            if (avgArousal >= 2.5)
                emotionMult = 0.2f;
            else if (avgArousal >= 2 && avgValence >= 2.5)
                emotionMult = 0.3f;
            else if (avgArousal >= 2 && avgValence <= 1.5)
                emotionMult = 0.3f;
            else if (avgArousal >= 1 && avgValence >= 2.5)
                emotionMult = 0.8f;
            else if (avgArousal >= 1 && avgValence <= 1.5)
                emotionMult = 1f;
            else if (avgArousal >= 1.8)
                emotionMult = 0.6f;
            else if (avgArousal >= 1)
                emotionMult = 1.2f;
            else if (avgArousal >= 0 && avgValence <= 2.5)
                emotionMult = 1.5f;
            else if (avgArousal >= 0 && avgValence >= 2.5)
                emotionMult = 1.35f;
        }

        return emotionMult;
    }

    public void ResetTimer()
    {
        CancelInvoke();
        timeSinceLastEvent = 0;
        InvokeRepeating("CheckSpawn", 5, 5);
    }
}
