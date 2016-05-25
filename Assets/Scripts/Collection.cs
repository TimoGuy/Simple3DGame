using UnityEngine;
using System.Collections;

public class Collection : MonoBehaviour {

	public GameObject collectableBox;
	// Use this for initialization
	void Start () {
		Invoke ("ObjectLifeEnd", Random.Range (10.0F, 20.0F));
	}

	void ObjectLifeEnd()
	{
		Rigidbody rb = GetComponent<Rigidbody> ();
		Instantiate (collectableBox, rb.position, rb.rotation);
		Destroy (gameObject);
	}

	void StopObject()
	{
		Rigidbody rb = GetComponent<Rigidbody> ();
		Instantiate (collectableBox, rb.position, rb.rotation);
		Destroy (gameObject);
	}

	void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.CompareTag("Ground")) {
			Invoke ("StopObject", Random.Range (4.0F, 6.0F));
		}
	}
}
