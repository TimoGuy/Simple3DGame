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
		InvokeRepeating ("FindTarget", Random.Range (0.2F, 0.3F), 5.0F);
		InvokeRepeating ("AimAtTarget", Random.Range (0.5F, 0.6F), 0.3F);
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
			start_dist = AquireTarget ("AttackDrone", start_dist);
			start_dist = AquireTarget ("BattleCruiser", start_dist);
			start_dist = AquireTarget ("HeavyTurret", start_dist);
			start_dist = AquireTarget ("Turret", start_dist);
			start_dist = AquireTarget ("TacticalDrone", start_dist);
		} else {
			start_dist = AquireTarget ("Player", start_dist);
			start_dist = AquireTarget ("AlliedDrone", start_dist);
			start_dist = AquireTarget ("AlliedTacticalDrone", start_dist);
		}
	}

	// Update is called once per frame
	void Update () {
		//Target is available so aim for it.
		if (target != null) {
			transform.rotation = Quaternion.Slerp (transform.rotation, look, Time.deltaTime * Random.Range (0.3F, 0.5F));
			rb.velocity = transform.forward * 80;
		} else {
			FindTarget ();
		}
	}
}
