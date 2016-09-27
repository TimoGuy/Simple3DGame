using UnityEngine;
using System.Collections;

public class FallAgain : MonoBehaviour {

	public GameObject respawnObject;
	// Use this for initialization
	void Start () {
		//Invoke ("RespawnHeavyObject", Random.Range (180, 240));
	}

	//Respawn the original heavy ammo so it can fall (if it got stuck in the air)
	private void RespawnHeavyObject()
	{
		Rigidbody rb = GetComponent<Rigidbody> ();
		Instantiate (respawnObject, rb.position, rb.rotation);
		Destroy (gameObject);
	}
}
