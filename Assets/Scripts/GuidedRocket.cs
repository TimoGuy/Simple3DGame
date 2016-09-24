using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GuidedRocket : MonoBehaviour {

	private GameObject target = null;
	Quaternion look;
	Rigidbody rb;
	public bool isAllied;
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		Invoke ("Detonate", 60.0F);
		FindTarget ();
		InvokeRepeating ("FindTarget", 5.0F, 5.0F);
	}
    
	/*********************************************************
    * DETONATE: Destroys the rocket after a designated time period
    *********************************************************/
	private void Detonate()
	{
		GetComponent("Projectile").SendMessage("SetDetonation", 2.0F);
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
			target = targets [random_target];
		}
	}

	/*********************************************************
    * FIND TARGET: Find a target to move towards
    *********************************************************/
	void FindTarget()
	{
		if (target == null) {
			bool direct_target_found = false;
			List<string> targetList = new List<string> ();
			if (isAllied) {
				targetList.Add ("AttackDrone");
				targetList.Add ("BattleCruiser");
				targetList.Add ("TacticalDrone");
				targetList.Add ("HeavyDrone");
			} else {
				targetList.Add ("Player");
				targetList.Add ("AlliedDrone");
				targetList.Add ("AlliedTacticalDrone");
				targetList.Add ("HeavyAlliedDrone");
			}
			Ray ray = new Ray (transform.position, transform.forward);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, 9999)) {
				foreach (string target_string in targetList) {
					if (hit.transform.name.Contains (target_string)) {
						direct_target_found = true;
					}
				}
			}
			if (!direct_target_found) {
				AquireRandomTarget (targetList);
			}
		}
	}

	/*********************************************************
    * UPDATE
    *********************************************************/
	void Update () {
		if (target != null) {
			look = Quaternion.LookRotation (target.transform.position - transform.position);
			transform.rotation = Quaternion.Slerp (transform.rotation, look, Time.deltaTime * 2.0F);
			rb.velocity = transform.forward * 80;
		}
	}
}
