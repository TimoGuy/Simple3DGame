using UnityEngine;
using System.Collections;

public class DestroyByBoundary : MonoBehaviour {

	void OnTriggerExit(Collider other) {
		if (other.gameObject.CompareTag ("Player")) {
			other.gameObject.SendMessage ("ReduceHealth", 100); //If it is the player then his health is gone.
		} else {
			Destroy (other.gameObject);
		}
	}
}
