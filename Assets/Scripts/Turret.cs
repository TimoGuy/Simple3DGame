using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Turret : MonoBehaviour {

	public bool fire_on_no_ally_block = false;
	public bool find_random_target = false;
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
	private List<string> targetList;
	private List<string> allyList;

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
		targetList = new List<string> ();
		allyList = new List<string> ();
		if (isAllied) {
			targetList.Add ("AttackDrone");
			targetList.Add ("Turret");
			targetList.Add ("TacticalDrone");
			targetList.Add ("HeavyTurret");
			targetList.Add ("BattleCruiser");
			targetList.Add ("HeavyDrone");
			allyList.Add ("Player");
			allyList.Add ("AlliedDrone");
			allyList.Add ("AlliedTacticalDrone");
			allyList.Add ("HeavyAlliedDrone");

				
		} else {
			targetList.Add ("Player");
			targetList.Add ("AlliedDrone");
			targetList.Add ("AlliedTacticalDrone");
			targetList.Add ("HeavyAlliedDrone");
			allyList.Add ("AttackDrone");
			allyList.Add ("Turret");
			allyList.Add ("TacticalDrone");
			allyList.Add ("HeavyTurret");
			allyList.Add ("BattleCruiser");
			allyList.Add ("HeavyDrone");
			if (!SceneManager.GetActiveScene ().name.Equals ("SurvivalMode")) {
				targetList.Add ("AlliedPortal");
			}
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

	/*********************************************************
    * AQUIRE RANDOM TARGET
    *********************************************************/
	void AquireRandomTarget(List<string> targetList)
	{
		List<GameObject> targets = new List<GameObject> ();
		foreach (string unit_target in targetList) {
			GameObject[] foundTargets = GameObject.FindGameObjectsWithTag (unit_target);
			foreach (GameObject foundTarget in foundTargets) {
				targets.Add (foundTarget);
			}
		}
		int random_target = Random.Range (0, targets.Count - 1);
		if (targets.Count > 0) {
			target = targets [random_target].transform;
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
		foreach (string target_str in targetList) {
			if (!find_random_target) {
				start_dist = FindClosestTarget (target_str, start_dist);
			}
		}
		if (find_random_target) {
			AquireRandomTarget (targetList);
		}
	}

	void FireAtTarget()
	{
		if (target != null) {
			Ray ray = new Ray (transform.position, transform.forward);
			RaycastHit hit;
			bool allyInTheWay = false;
			if (Physics.Raycast (ray, out hit, shotRange)) {
				foreach (string ally in allyList) {
					if (hit.transform.CompareTag (ally)) {
						allyInTheWay = true;
					}
				}
				if ((!fire_on_no_ally_block && hit.transform.name.Contains("BreakableCube") || hit.transform.CompareTag (target.tag)) ||
					(fire_on_no_ally_block && !allyInTheWay)) {
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
