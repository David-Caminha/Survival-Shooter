using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalScript : MonoBehaviour {

    public GameObject message;
    public GameObject nextCrystal;
    public GameObject arrow;

	// Use this for initialization
	void OnTriggerEnter (Collider other) {
		if(other.gameObject.CompareTag("Player"))
        {
            arrow.SetActive(false);
            message.SetActive(true);
            nextCrystal.SetActive(true);
        }
	}
}