using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour {

    public float time = 0;
    public Image fadeOutImage;
    public Color fadeOutColor;
    public float fadeOutTime = 3f;
    public float timer = 0;

    public CompleteProject.PlayerHealth player;
    public CompleteProject.EnemyManager bunnyManager;
    public CompleteProject.EnemyManager bearManager;
    public CompleteProject.EnemyManager elephantManager;

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        time += Time.deltaTime;

        if (time > 60)
        {
            bunnyManager.spawnInterval = 1f;
            bearManager.spawnInterval = 1f;
            elephantManager.spawnInterval = 3.5f;
        }
        else if (time > 40)
        {
            bunnyManager.spawnInterval = 2f;
            bearManager.spawnInterval = 2f;
            elephantManager.spawnInterval = 7.5f;
        }
        else if (time > 30)
        {
            bunnyManager.spawnInterval = 2.5f;
            bearManager.spawnInterval = 2.5f;
            elephantManager.spawnInterval = 10f;
        }

        if(player.currentHealth <= 0)
        {
            if (timer > 4)
                NextLevel();
            fadeOutImage.color = Color.Lerp(Color.clear, fadeOutColor, timer / fadeOutTime);
            timer += Time.deltaTime;
        }
    }

    void NextLevel()
    {
        EmotionManager.Instance.tutorialTime = time;
        SceneManager.LoadScene(2);
    }
}
