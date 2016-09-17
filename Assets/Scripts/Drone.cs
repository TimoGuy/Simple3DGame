using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Drone : MonoBehaviour {

	private Transform target = null;
	private float speed;
	public GameObject bullet;
	public GameObject secondary;
	public float easySpeed;
	public float normalSpeed;
	public float hardSpeed;
	public float easyRange;
	public float normSecRange;
	public float easySecRange;
	public float hardSecRange;
	public float normalRange;
	public float hardRange;
	public float easyROF;
	public float normalROF;
	public float hardROF;
	private float rateOfFire;
	private float shotRange;
	public float shotOffset;
	public float shotSpeed;
	private float standoffPositionx = 0;
	private float standoffPositionz = 0;
	public string lastRaycast;
	private Vector3 standoffPosition;
	public bool isAllied;
	public bool isTactical;
	//private bool rightSide = false;
	private int fireCount = 0;
	private float secondaryRange;
	private int attempted_fires = 0;
	private List<Transform> invalidTargets = new List<Transform>();


	// Use this for initialization
	void Start () {
		//Drones are faster and have more range on higher diffulties
		int difficulty = PlayerPrefs.GetInt ("AILevel");
		if (difficulty == 0) {
			speed = easySpeed;
			shotRange = easyRange;
			rateOfFire = easyROF;
			secondaryRange = easySecRange;
		}
		else if (difficulty == 1) {
			speed = normalSpeed;
			shotRange = normalRange;
			rateOfFire = normalROF;
			secondaryRange = normSecRange;
		} else if (difficulty == 2) {
			speed = hardSpeed;
			shotRange = hardRange;
			rateOfFire = hardROF;
			secondaryRange = hardSecRange;
		}

		InvokeRepeating ("AquireTargets", Random.Range(0.1F, 1.0F), 10.0F);
		if (isTactical) {
			//InvokeRepeating ("FireSecondaryAtTarget", Random.Range (1.0F, 3.0F), 1.0F);
			Invoke("FireAtTarget", Random.Range(0.5F, 1.0F));
		} else {
			InvokeRepeating ("FireAtTarget", Random.Range (1.0F, 3.0F), rateOfFire);
		}
	}

	bool isInvalid(Transform target)
	{
		foreach (Transform invalid in invalidTargets)
		{
			if (target == invalid) {
				return true;
			}
		}
		return false;
	}
	/*************************************************************
    * AQUIRE TARGET
    * DESCRIPTION: Finds which of the objects with the input tag is closer
    * than the start distance, saves the target reference and distance.
    *************************************************************/
	float AquireTarget(string tag, float start_dist)
	{
		//First target enemy drones
		GameObject[] enemyDrones = GameObject.FindGameObjectsWithTag (tag);
		foreach (GameObject enemyDrone in enemyDrones) {
			if (Vector3.Distance (enemyDrone.transform.position, transform.position) < start_dist && !isInvalid(enemyDrone.transform)) {
				target = enemyDrone.transform;
				start_dist = Vector3.Distance (enemyDrone.transform.position, transform.position);
			}
		}
		return start_dist;
	}

	/*************************************************************
    * AQUIRE TARGETS
    * DESCRIPTION: Finds a target to go after in the game.
    *************************************************************/
	void AquireTargets()
	{
		float start_dist = 10000.0F;
		if (isAllied) {
			start_dist = AquireTarget ("AttackDrone", start_dist);
			start_dist = AquireTarget ("TacticalDrone", start_dist);
			start_dist = AquireTarget ("Turret", start_dist);
			start_dist = AquireTarget ("HeavyTurret", start_dist);
			start_dist = AquireTarget ("BattleCruiser", start_dist);
			if (!SceneManager.GetActiveScene ().name.Equals ("SurvivalMode")) {
				start_dist = AquireTarget ("Portal", start_dist);
			}

		} else {
			target = GameObject.FindGameObjectWithTag ("Player").transform;//GameObject.FindObjectOfType<UnityStandardAssets.Characters.FirstPerson.FirstPersonController> ().transform;
			start_dist = Vector3.Distance (target.position, transform.position);
			start_dist = AquireTarget ("AlliedDrone", start_dist);
			start_dist = AquireTarget ("AlliedTacticalDrone", start_dist);
			if (!SceneManager.GetActiveScene ().name.Equals ("SurvivalMode")) {
				start_dist = AquireTarget ("AlliedPortal", start_dist);
			}
		}
		AquirePositionTarget ();
	}

	/*************************************************************
    * AQUIRE POSITION TARGET
    * DESCRIPTION: Calculates an appropriate standoff position to the
    * currently selected target.
    *************************************************************/
	void AquirePositionTarget()
	{
		if (target != null) {
			if (isTactical) {
				standoffPositionx = Random.Range (-30, 30);
				standoffPositionz = Random.Range (-30, 30);
				standoffPosition = target.position;
				standoffPosition.y += Random.Range (25, 70);
				standoffPosition.z += standoffPositionz;
				standoffPosition.x += standoffPositionx;
			} else {
				standoffPositionx = Random.Range (-30, 30);
				standoffPositionz = Random.Range (-30, 30);
				standoffPosition = target.position;
				standoffPosition.y += Random.Range (5, 50);
				standoffPosition.z += standoffPositionz;
				standoffPosition.x += standoffPositionx;
			}
		}
	}

	void FireSecondaryAtTarget()
	{
		if (target != null) {
			transform.LookAt (target.position);
			Ray ray = new Ray (transform.position, transform.forward);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, secondaryRange)) {
				//Only fire if target is in range
				lastRaycast = hit.transform.tag;
				if (hit.transform.name.Contains ("BreakableCube") || hit.transform.CompareTag (target.tag)) {
					float dist = Vector3.Distance (target.position, transform.position);
					if (dist < secondaryRange) {
						GameObject blast_clone;
						Vector3 position = transform.position;
						position = transform.position + transform.forward * 15;// + transform.right * 5;
						//position = transform.position + transform.right * 5;
						blast_clone = Instantiate (secondary, position, transform.rotation) as GameObject;
						blast_clone.GetComponent<Rigidbody> ().velocity = transform.forward * 80;	
					}
				} else {
					attempted_fires++;
					if (attempted_fires == 10) {
						attempted_fires = 0;
						invalidTargets.Add (target);
					}
				}
			}
		}
	}
	/*************************************************************
    * FIRE AT TARGET
    * DESCRIPTION: Fires a shot at the selected target object.
    *************************************************************/
	void FireAtTarget()
	{
		bool fired = false;
		if (target != null) {
			transform.LookAt (target.position);
			Ray ray = new Ray (transform.position, transform.forward);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, shotRange)) {
				lastRaycast = hit.transform.tag;
				if (hit.transform.name.Contains ("BreakableCube") || hit.transform.CompareTag (target.tag)) {
					float dist = Vector3.Distance (target.position, transform.position);
					if (dist < shotRange) {
						GameObject blast_clone;
						Vector3 position = transform.position;
						position = transform.position + transform.forward * shotOffset;
						blast_clone = Instantiate (bullet, position, transform.rotation) as GameObject;
						blast_clone.GetComponent<Rigidbody> ().velocity = transform.forward * shotSpeed;
						fired = true;
					}
				} else {
					attempted_fires++;
					if (attempted_fires == 10) {
						attempted_fires = 0;
						invalidTargets.Add (target);
					}
				}
			}
		}
		if (isTactical) {
			if (fireCount < 10 && fired) {
				fireCount++;
				Invoke ("FireAtTarget", 0.05F);
			} else {
				fireCount = 0;
				Invoke ("FireAtTarget", rateOfFire + 1.0F);
				Invoke ("FireSecondaryAtTarget", 1.0F);
			}
		}
	}

	void Update()
	{
		float step = speed * Time.deltaTime;
		transform.position = Vector3.MoveTowards (transform.position, standoffPosition, step);
	}
}
