using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BattleCruiser : MonoBehaviour {

	public GameObject droneModel;
	public GameObject crateDrop;
	public GameObject tacticalDrone;
	private Transform target;
	private float speed = 3;
	private UnityStandardAssets.Characters.FirstPerson.FirstPersonController controller;
	public GameObject rocketModel;
	private int droneCount = 0;
	private Vector3 standoffPosition;
	// Use this for initialization
	void Start () {
		controller = GameObject.FindObjectOfType<UnityStandardAssets.Characters.FirstPerson.FirstPersonController> ();
		GameObject[] destinations = GameObject.FindGameObjectsWithTag ("FlightPoint");
		if (destinations.Length > 0) {
			int destination = Random.Range (0, destinations.Length - 1);
			target = destinations [destination].transform;
			InvokeRepeating ("DropCrate", Random.Range (5.00F, 10.0F), 5.0F); 

		} else {
			target = controller.transform;
		}
		standoffPosition = target.position;
		standoffPosition.y = transform.position.y;
		Invoke ("StartDroneWave", Random.Range (1.00F, 5.0F)); 
	}

	public void SetSpeed(int in_Speed)
	{
		speed = in_Speed;
	}
	public void TargetPlayer()
	{
		if (controller != null) {
			target = controller.transform;
			standoffPosition = target.position;
			standoffPosition.y = transform.position.y;
		}
	}


	private void DropCrate()
	{
		Vector3 droneLaunchPoint = transform.position;
		droneLaunchPoint.y -= 5;
		Instantiate (crateDrop, droneLaunchPoint, transform.rotation);
	}

	private void StartDroneWave()
	{
		droneCount = 1;
		Vector3 droneLaunchPoint = transform.position;
		droneLaunchPoint.y -= 5;
		Instantiate (droneModel, droneLaunchPoint, transform.rotation);
		int difficulty = PlayerPrefs.GetInt ("AILevel");
		if (difficulty == 0) {
			Invoke ("LaunchNextDrone", 3.0F);
		} else if (difficulty == 1) {
			Invoke ("LaunchNextDrone", 2.0F);
		} else if (difficulty == 2) {
			Invoke ("LaunchNextDrone", 1.0F);
		}
	}

	private void LaunchNextDrone()
	{
		Vector3 droneLaunchPoint = transform.position;
		droneLaunchPoint.y -= 5;
		if (droneCount == 4) {
			Instantiate (tacticalDrone, droneLaunchPoint, transform.rotation);
		} else {
			Instantiate (droneModel, droneLaunchPoint, transform.rotation);
		}
		int difficulty = PlayerPrefs.GetInt ("AILevel");
		droneCount++;
		if (droneCount == 5) {
			droneCount = 0;
			Invoke ("StartDroneWave", Random.Range (1.00F, 5.0F));
		} else {
			if (difficulty == 0) {
				Invoke ("LaunchNextDrone", 3.0F);
			} else if (difficulty == 1) {
				Invoke ("LaunchNextDrone", 2.0F);
			} else if (difficulty == 2) {
				Invoke ("LaunchNextDrone", 1.0F);
			}
		}

	}

	// Update is called once per frame
	void Update () {
		float step = speed * Time.deltaTime;
		standoffPosition = target.position;
		standoffPosition.y = transform.position.y;
		transform.position = Vector3.MoveTowards (transform.position, standoffPosition, step);
	}
}
