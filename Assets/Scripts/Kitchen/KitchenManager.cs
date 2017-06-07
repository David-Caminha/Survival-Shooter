using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenManager : MonoBehaviour {

    public CompleteProject.EnemyManager[] bunnySpawners;
    public CompleteProject.EnemyManager[] bearSpawners;
    public CompleteProject.EnemyManager[] hardBunnySpawners;
    public CompleteProject.EnemyManager[] hardBearSpawners;
    public CompleteProject.EnemyManager[] elephantSpawners;

    EmotionManager emotionManager;

    // Use this for initialization
    void Start () {

        emotionManager = EmotionManager.Instance;
        if (emotionManager != null && emotionManager.emotionalMechanics)
        {
            if (emotionManager.tutorialTime > 60)
            {
                for (int i = 0; i < bunnySpawners.Length; i++)
                    bunnySpawners[i].spawnInterval = 1f;
                for (int i = 0; i < hardBunnySpawners.Length; i++)
                    hardBunnySpawners[i].spawnInterval = 4f;

                for (int i = 0; i < bearSpawners.Length; i++)
                    bearSpawners[i].spawnInterval = 1f;
                for (int i = 0; i < hardBearSpawners.Length; i++)
                    hardBearSpawners[i].spawnInterval = 6f;

                for (int i = 0; i < elephantSpawners.Length; i++)
                    elephantSpawners[i].spawnInterval = 3.5f;
            }
            else if (emotionManager.tutorialTime > 40)
            {
                for (int i = 0; i < bunnySpawners.Length; i++)
                    bunnySpawners[i].spawnInterval = 2f;
                for (int i = 0; i < hardBunnySpawners.Length; i++)
                    hardBunnySpawners[i].spawnInterval = 6f;

                for (int i = 0; i < bearSpawners.Length; i++)
                    bearSpawners[i].spawnInterval = 2f;
                for (int i = 0; i < hardBearSpawners.Length; i++)
                    hardBearSpawners[i].spawnInterval = 8f;

                for (int i = 0; i < elephantSpawners.Length; i++)
                    elephantSpawners[i].spawnInterval = 7.5f;
            }
            else if (emotionManager.tutorialTime > 30)
            {
                for (int i = 0; i < bunnySpawners.Length; i++)
                    bunnySpawners[i].spawnInterval = 2.5f;
                for (int i = 0; i < hardBunnySpawners.Length; i++)
                    hardBunnySpawners[i].spawnInterval = 7f;

                for (int i = 0; i < bearSpawners.Length; i++)
                    bearSpawners[i].spawnInterval = 2.5f;
                for (int i = 0; i < hardBearSpawners.Length; i++)
                    hardBearSpawners[i].spawnInterval = 9f;

                for (int i = 0; i < elephantSpawners.Length; i++)
                    elephantSpawners[i].spawnInterval = 10f;
            }
            else
            {
                for (int i = 0; i < bunnySpawners.Length; i++)
                    bunnySpawners[i].spawnInterval = 3f;
                for (int i = 0; i < hardBunnySpawners.Length; i++)
                    hardBunnySpawners[i].spawnInterval = 10f;

                for (int i = 0; i < bearSpawners.Length; i++)
                    bearSpawners[i].spawnInterval = 3f;
                for (int i = 0; i < hardBearSpawners.Length; i++)
                    hardBearSpawners[i].spawnInterval = 12f;

                for (int i = 0; i < elephantSpawners.Length; i++)
                    elephantSpawners[i].spawnInterval = 12f;
            }
        }
        else
        {
            for (int i = 0; i < bunnySpawners.Length; i++)
                bunnySpawners[i].spawnInterval = 2.5f;
            for (int i = 0; i < hardBunnySpawners.Length; i++)
                hardBunnySpawners[i].spawnInterval = 7f;

            for (int i = 0; i < bearSpawners.Length; i++)
                bearSpawners[i].spawnInterval = 2.5f;
            for (int i = 0; i < hardBearSpawners.Length; i++)
                hardBearSpawners[i].spawnInterval = 9f;

            for (int i = 0; i < elephantSpawners.Length; i++)
                elephantSpawners[i].spawnInterval = 10f;
        }
    }
}
