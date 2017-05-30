using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class StartGame : MonoBehaviour {

    public Text playerName;
    public GameObject confirmPanel;
    public Text confirmText;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void PlayGame()
    {
        if (playerName.text.Length != 0)
            ConfirmName();
    }

    void ConfirmName()
    {
        EmotionManager.Instance.playerName = playerName.text;
        confirmText.text = "Is " + playerName.text + " the name you want?";
        confirmPanel.SetActive(true);
    }

    public void Confirm()
    {
        EmotionManager.Instance.enabled = true;
        SceneManager.LoadScene(1);
    }

    public void Cancel()
    {
        confirmPanel.SetActive(false);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }
}
