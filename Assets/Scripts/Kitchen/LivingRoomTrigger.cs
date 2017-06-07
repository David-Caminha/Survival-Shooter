using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LivingRoomTrigger : MonoBehaviour {

    public GameObject message;
    
	void OnTriggerEnter (Collider other) {
		if(other.gameObject.CompareTag("Player"))
        {
            message.SetActive(true);
            SceneManager.LoadSceneAsync(4);
        }
	}
}
