using UnityEngine;
using System.Collections;

public class GuidedRocket : MonoBehaviour {

	//private int team; Future use
	private GameObject target = null;
	Quaternion look;
	Rigidbody rb;
	public bool isAllied;
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		Invoke ("detonate", 60.0F);
		FindTarget ();
		InvokeRepeating ("FindTarget", 5.0F, 5.0F);
	}
    
	private void detonate()
	{
		GetComponent("Projectile").SendMessage("SetDetonation", 2.0F);
	}


	/*****************************************************************   
    * AIM AT TARGET
    * DESCRIPTION: Finds the new location of the target object to ensure
    * it is up to date.
    *****************************************************************/
	void AimAtTarget()
	{
		if (target != null) {
			look = Quaternion.LookRotation (target.transform.position - transform.position);
		}
	}

	float AquireTarget(string objectTag, float start_dist)
	{
		GameObject[] targets = GameObject.FindGameObjectsWithTag (objectTag);
		if (targets.Length > 0) {
			foreach (GameObject drone in targets) {
				//Find the closest drone
				if (Vector3.Distance (drone.transform.position, transform.position) < start_dist) {
					target = drone;
					start_dist = Vector3.Distance (drone.transform.position, transform.position);
				}
			}
		}
		return start_dist;
	}

	void FindTarget()
	{
		float start_dist = 10000;
		if (isAllied) {
			//Cast a ray and see if a target is right in front
			Ray ray = new Ray (transform.position, transform.forward);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, 9999))
			{
				if (hit.transform.CompareTag ("AttackDrone") || hit.transform.CompareTag ("BattleCruiser") ||
				    hit.transform.CompareTag ("HeavyTurret") || hit.transform.CompareTag ("Turret") ||
				    hit.transform.CompareTag ("TacticalDrone")) {
					target = hit.transform.gameObject;
					start_dist = 0;
				}
			}
			if (start_dist > 0) {
				start_dist = AquireTarget ("AttackDrone", start_dist);
				start_dist = AquireTarget ("BattleCruiser", start_dist);
				//start_dist = AquireTarget ("HeavyTurret", start_dist);
				//start_dist = AquireTarget ("Turret", start_dist);
				start_dist = AquireTarget ("TacticalDrone", start_dist);
			}
		} else {
			Ray ray = new Ray (transform.position, transform.forward);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, 9999))
			{
				if (hit.transform.CompareTag ("Player") || hit.transform.CompareTag ("AlliedDrone") ||
					hit.transform.CompareTag ("AlliedTacticalDrone")) {
					target = hit.transform.gameObject;
					start_dist = 0;
				}
			}
			if (start_dist > 0) {
				start_dist = AquireTarget ("Player", start_dist);
				start_dist = AquireTarget ("AlliedDrone", start_dist);
				start_dist = AquireTarget ("AlliedTacticalDrone", start_dist);
			}
		}
	}

	// Update is called once per frame
	void Update () {
		if (target != null) {
			look = Quaternion.LookRotation (target.transform.position - transform.position);
			transform.rotation = Quaternion.Slerp (transform.rotation, look, Time.deltaTime * 2.0F);
			float dist = Vector3.Distance (target.transform.position, transform.position);
			//Close enough, blow them up!
			if (dist < 10) {
				GetComponent ("Projectile").SendMessage ("SetDetonation", 0.0F);
			}
			rb.velocity = transform.forward * 80;
		}
	}
}
