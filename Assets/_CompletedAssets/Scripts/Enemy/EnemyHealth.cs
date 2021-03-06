﻿using UnityEngine;

namespace CompleteProject
{
    public class EnemyHealth : MonoBehaviour
    {
        EmotionManager emotionManager;

        public GameObject healthDrop;
        public GameObject boostDrop;
        public float dropChance = 10f;

        public int startingHealth = 100;            // The amount of health the enemy starts the game with.
        public int currentHealth;                   // The current health the enemy has.
        public float sinkSpeed = 2.5f;              // The speed at which the enemy sinks through the floor when dead.
        public int scoreValue = 10;                 // The amount added to the player's score when the enemy dies.
        public AudioClip deathClip;                 // The sound to play when the enemy dies.


        Animator anim;                              // Reference to the animator.
        AudioSource enemyAudio;                     // Reference to the audio source.
        ParticleSystem hitParticles;                // Reference to the particle system that plays when the enemy is damaged.
        CapsuleCollider capsuleCollider;            // Reference to the capsule collider.
        bool isDead;                                // Whether the enemy is dead.
        bool isSinking;                             // Whether the enemy has started sinking through the floor.


        void Awake ()
        {
            emotionManager = EmotionManager.Instance;

            // Setting up the references.
            anim = GetComponent <Animator> ();
            enemyAudio = GetComponent <AudioSource> ();
            hitParticles = GetComponentInChildren <ParticleSystem> ();
            capsuleCollider = GetComponent <CapsuleCollider> ();

            // Setting the current health when the enemy first spawns.
            currentHealth = startingHealth;
        }


        public void TakeDamage (int amount, Vector3 hitPoint)
        {
            // If the enemy is dead...
            if(isDead)
                // ... no need to take damage so exit the function.
                return;

            // Play the hurt sound effect.
            enemyAudio.Play ();

            // Reduce the current health by the amount of damage sustained.
            currentHealth -= amount;
            
            // Set the position of the particle system to where the hit was sustained.
            hitParticles.transform.position = hitPoint;

            // And play the particles.
            hitParticles.Play();

            // If the current health is less than or equal to zero...
            if(currentHealth <= 0)
            {
                // ... the enemy is dead.
                Death ();
            }
        }


        void Death ()
        {
            if(emotionManager)
                emotionManager.AddEvent("Kill " + gameObject.name);
            
            if (dropChance > 0)
                DropItem();
            
            // The enemy is dead.
            isDead = true;

            // Turn the collider into a trigger so shots can pass through it.
            capsuleCollider.isTrigger = true;

            // Tell the animator that the enemy is dead.
            anim.SetTrigger ("Dead");

            // Change the audio clip of the audio source to the death clip and play it (this will stop the hurt clip playing).
            enemyAudio.clip = deathClip;
            enemyAudio.Play ();
        }

        void DropItem()
        {
            float emotionMult = GetEmotionMult();
            float rand = Random.Range(0f, 100f);
            if(rand < dropChance * emotionMult)
            {
                float item = Random.Range(0f, 100f);
                if(item < 20)
                {
                    Vector3 pickupPosition = new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z);
                    Instantiate(boostDrop, pickupPosition, Quaternion.identity);
                    if (emotionManager)
                        emotionManager.AddEvent("Drop boost pack");
                }
                else
                {
                    Vector3 pickupPosition = new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z);
                    Instantiate(healthDrop, pickupPosition, Quaternion.identity);
                    if (emotionManager)
                        emotionManager.AddEvent("Drop health pack");
                }
            }
        }

        float GetEmotionMult()
        {
            float emotionMult = 1f;

            if (emotionManager && emotionManager.emotionalMechanics)
            {
                double avgArousal = emotionManager.AverageArousal();
                double avgValence = emotionManager.AverageValence();

                if (avgArousal >= 3)
                {
                    if (avgValence >= 3)
                        emotionMult = 1;
                    else if (avgValence >= 2)
                        emotionMult = 1;
                    else if (avgValence >= 1)
                        emotionMult = 3;
                    else
                        emotionMult = 5;
                }
                else if (avgArousal >= 2)
                {
                    if (avgValence >= 3)
                        emotionMult = 1;
                    else if (avgValence >= 2)
                        emotionMult = 1;
                    else if (avgValence >= 1)
                        emotionMult = 2;
                    else
                        emotionMult = 3;
                }
                else if (avgArousal >= 1)
                {
                    if (avgValence >= 3)
                        emotionMult = 0.75f;
                    else if (avgValence >= 2)
                        emotionMult = 0.5f;
                    else if (avgValence >= 1)
                        emotionMult = 0.4f;
                    else
                        emotionMult = 0.2f;
                }
                else
                {
                    if (avgValence >= 2)
                        emotionMult = 0.6f;
                    else
                        emotionMult = 0.2f;
                }
            }

            return emotionMult;
        }


        public void StartSinking ()
        {
            // Find and disable the Nav Mesh Agent.
            GetComponent <UnityEngine.AI.NavMeshAgent> ().enabled = false;

            // Find the rigidbody component and make it kinematic (since we use Translate to sink the enemy).
            GetComponent <Rigidbody> ().isKinematic = true;

            // The enemy should no sink.
            isSinking = true;

            // Increase the score by the enemy's score value.
            ScoreManager.score += scoreValue;

            // After 2 seconds destory the enemy.
            Destroy (gameObject, 2f);
        }
    }
}