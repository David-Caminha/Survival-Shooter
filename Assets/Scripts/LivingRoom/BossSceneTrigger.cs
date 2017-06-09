using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossSceneTrigger : MonoBehaviour {

    public Text message;
    public string text;

    // Use this for initialization
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            message.text = text;
            Invoke("GoToBossScene", 20f);
        }
    }

    void GoToBossScene()
    {
        SceneManager.LoadSceneAsync(5);
    }
}
