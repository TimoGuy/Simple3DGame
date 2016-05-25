using UnityEngine;
using System.Collections;

public class RandomCrateDrop : MonoBehaviour {

	public GameObject dropCrate;
	// Use this for initialization
	void Start () {
		Invoke ("DropCrate", Random.Range (5.0F, 20.0F));
	}

	void DropCrate()
	{
		Vector3 position = new Vector3(Random.Range(-240.0F, 240.0F), 300, Random.Range(-240.0F, 240.0F));
		Instantiate (dropCrate, position, Random.rotation);
		Invoke ("DropCrate", Random.Range (5.0F, 20.0F));
	}
}
