using UnityEngine;
using System.Collections;

public class Carrier : MonoBehaviour {

	private Transform target;
	private Vector3 standoffPosition;
	private float speed = 25;
	private UnityStandardAssets.Characters.FirstPerson.FirstPersonController controller;
	// Use this for initialization
	void Start () {
		GameObject[] destinations = GameObject.FindGameObjectsWithTag ("FlightPoint");
		controller = GameObject.FindObjectOfType<UnityStandardAssets.Characters.FirstPerson.FirstPersonController> ();
		//If we have a flight point move toward that, otherwise target the player
		if (destinations.Length > 0) {
			int destination = Random.Range (0, destinations.Length - 1);
			target = destinations [destination].transform;

		} else {
			target = controller.transform;
		}
		standoffPosition = target.position;
		standoffPosition.y = transform.position.y;
	}
	
	// Update is called once per frame
	void Update () {
		float step = speed * Time.deltaTime;
		standoffPosition = target.position;
		standoffPosition.y = transform.position.y;
		transform.position = Vector3.MoveTowards (transform.position, standoffPosition, step);
	}
}
