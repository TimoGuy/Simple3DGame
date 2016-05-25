using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	private Rigidbody rb;
	public float speed;
	public GameObject projectile;
	public float shootForce;
	void Update()
	{
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");

		Vector3 movement = new Vector3 (moveHorizontal, 0, moveVertical);
		movement *= speed;
		rb.AddForce (movement);






	}
	void FixedUpdate()
	{

	}

	void Start() 
	{
		rb = GetComponent<Rigidbody> ();
	}


}
