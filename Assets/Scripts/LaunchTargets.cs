using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LaunchTargets : MonoBehaviour {

	public Rigidbody launchSphere;
	public Rigidbody ammoPack;
	int numSpheres = 0;
	void Start () {
		InvokeRepeating ("LaunchTarget", Random.Range (5.0F, 20.0F), Random.Range (5.0F, 10.0F));
	}

	public void LaunchTarget()
	{
		Rigidbody blast_clone;
		Vector3 transform1 = transform.position;

		if (numSpheres == 15)
		{
			transform1.y -=  0.5f;
			transform1.x +=  5;
			blast_clone = Instantiate (ammoPack, transform1, transform.rotation) as Rigidbody;
			numSpheres = 0;
		}
		else
		{
			transform1.y +=  5;
			blast_clone = Instantiate (launchSphere, transform1, transform.rotation) as Rigidbody;

			blast_clone.velocity = new Vector3 (Random.Range(5, 10), Random.Range(15, 30), Random.Range(3, 10));
		}
		numSpheres++;
	}
}
