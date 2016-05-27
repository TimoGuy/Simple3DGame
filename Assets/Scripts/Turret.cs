using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Turret : MonoBehaviour {

	public float targetingSpeed;
	public float shotVelocity;
	public float easyROF;
	public float normalROF;
	public float hardROF;
	public float easyRange;
	public float normalRange;
	public float hardRange;
	private float rateOfFire;
	private float shotRange;
	public float forwardOffset;
	public GameObject bullet;
	private Transform target;
	public bool isAllied;
	private Quaternion look;


	// Use this for initialization
	void Start () {
		int difficulty = PlayerPrefs.GetInt ("AILevel");
		if (difficulty == 0) {
			shotRange = easyRange;
			rateOfFire = easyROF;
		}
		else if (difficulty == 1) {
			shotRange = normalRange;
			rateOfFire = normalROF;
		} else if (difficulty == 2) {
			shotRange = hardRange;
			rateOfFire = hardROF;
		}
		InvokeRepeating ("FireAtTarget", Random.Range(1.0F, 5.0F), rateOfFire);
		InvokeRepeating ("AimAtTarget", Random.Range (1.0F, 5.0F), 0.3F);
		InvokeRepeating ("AquireTarget", Random.Range (0.3F, 0.5F), 5.0F);
	}

	/*****************************************************************
    * FIND CLOSEST TARGET
    * DESCRIPTION: Finds the target closest to the object
    *****************************************************************/
	float FindClosestTarget(string tag, float start_dist)
	{
		//First target enemy drones
		GameObject[] targets = GameObject.FindGameObjectsWithTag (tag);
		foreach (GameObject targetUnit in targets) {
			if (Vector3.Distance (targetUnit.transform.position, transform.position) < start_dist) {
				target = targetUnit.transform;
				start_dist = Vector3.Distance (targetUnit.transform.position, transform.position);
			}
		}
		return start_dist;
	}

	/*****************************************************************   
    * AIM AT TARGET
    * DESCRIPTION: Finds the new location of the target object to ensure
    * it is up to date.
    *****************************************************************/
	void AimAtTarget()
	{
		if (target != null) {
			look = Quaternion.LookRotation (target.position - transform.position);
		}
	}

	/***************************************************************
    * AQUIRE TARGET
    * DESCRIPTION: Locates the closest enemy target to the current 
    * object.
    ***************************************************************/
	void AquireTarget()
	{
		float start_dist = 10000.0F;
		if (isAllied) {
			start_dist = FindClosestTarget ("AttackDrone", start_dist);
			start_dist = FindClosestTarget ("Turret", start_dist);
			start_dist = FindClosestTarget ("TacticalDrone", start_dist);
			start_dist = FindClosestTarget ("HeavyTurret", start_dist);
			start_dist = FindClosestTarget ("BattleCruiser", start_dist);
			if (!SceneManager.GetActiveScene ().name.Equals ("Portal")) {
				start_dist = FindClosestTarget ("AlliedPortal", start_dist);
			}
		} else {
			target = GameObject.FindGameObjectWithTag ("Player").transform;
			start_dist = Vector3.Distance (target.position, transform.position);
			start_dist = FindClosestTarget ("AlliedDrone", start_dist);
			start_dist = FindClosestTarget ("AlliedTacticalDrone", start_dist);
			if (!SceneManager.GetActiveScene ().name.Equals ("SurvivalMode")) {
				start_dist = FindClosestTarget ("AlliedPortal", start_dist);
			}
		}
	}

	void FireAtTarget()
	{
		if (target != null) {
			Ray ray = new Ray (transform.position, transform.forward);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, shotRange)) {
				if (hit.transform.CompareTag ("Crate") || hit.transform.CompareTag (target.tag)) {
					float dist = Vector3.Distance (target.position, transform.position);
					if (dist < shotRange) {
						GameObject blast_clone;
						Vector3 position = transform.position;
						position = transform.position + transform.forward * forwardOffset;
						blast_clone = Instantiate (bullet, position, transform.rotation) as GameObject;
						blast_clone.GetComponent<Rigidbody> ().velocity = transform.forward * shotVelocity;
					}
				}
			}
		}
	}

	// Update is called once per frame
	void Update () {
		 transform.rotation = Quaternion.Slerp (transform.rotation, look, Time.deltaTime * targetingSpeed);
	}
}
