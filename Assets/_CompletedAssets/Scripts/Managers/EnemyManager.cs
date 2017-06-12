using UnityEngine;

namespace CompleteProject
{
    public class EnemyManager : MonoBehaviour
    {
        public PlayerHealth playerHealth;       // Reference to the player's heatlh.
        public GameObject enemy;                // The enemy prefab to be spawned.
        public float spawnInterval = 3f;            // How long between each spawn.
        public Transform[] spawnPoints;         // An array of the spawn points this enemy can spawn from.
        public bool spawning = false;
        public float dropChance = -1;



        void Spawn()
        {
            if (spawning)
            {
                // If the player has no health left...
                if (playerHealth.currentHealth <= 0f)
                {
                    // ... exit the function.
                    return;
                }

                // Find a random index between zero and one less than the number of spawn points.
                int spawnPointIndex = Random.Range(0, spawnPoints.Length);

                // Create an instance of the enemy prefab at the randomly selected spawn point's position and rotation.
                GameObject enemySpawned = Instantiate(enemy, spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);

                if (dropChance != -1)
                    enemySpawned.GetComponent<EnemyHealth>().dropChance = dropChance;

                Invoke("Spawn", spawnInterval);
            }
        }

        public void StartSpawning(float spawnTime)
        {
            Invoke("Spawn", spawnInterval);
            spawning = true;
            Invoke("StopSpawning", spawnTime);
        }

        public void SpawnIndefinitely()
        {
            Invoke("Spawn", spawnInterval);
            spawning = true;
        }

        public void StopSpawning()
        {
            spawning = false;
        }
    }
}