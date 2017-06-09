using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalScript : MonoBehaviour {

    public GameObject message;
    public GameObject nextCrystal;

	// Use this for initialization
	void OnTriggerEnter (Collider other) {
		if(other.gameObject.CompareTag("Player"))
        {
            message.SetActive(true);
            nextCrystal.SetActive(true);
        }
	}
}