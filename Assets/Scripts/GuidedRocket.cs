using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GuidedRocket : MonoBehaviour {

	private GameObject target = null;
	Quaternion look;
	Rigidbody rb;
	public bool isAllied;
	private bool guided = true;
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		Invoke ("Detonate", 180.0F);
		FindTarget ();
		InvokeRepeating ("FindTarget", 5.0F, 5.0F);
		Invoke ("toggleGuidedMode", Random.Range (8, 12));
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
				targetList.Add ("Carrier");
				targetList.Add ("RocketEvil");
			} else {
				targetList.Add ("Player");
				targetList.Add ("AlliedDrone");
				targetList.Add ("AlliedTacticalDrone");
				targetList.Add ("HeavyAlliedDrone");
				targetList.Add ("GoodRocket");
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

	/***********************************************************
    * TOGGLE GUIDED MODE
    * DESCRIPTION: If the rocket gets stuck going around and around
    * this will periodically break the loop and allow the rocket to
    * re-approach the target.
    ***********************************************************/
	void toggleGuidedMode()
	{
		if (guided)
		{
			guided = false;
			Invoke ("toggleGuidedMode", Random.Range (1.5F, 3F));
		}
		else
		{
			guided = true;
			Invoke ("toggleGuidedMode", Random.Range (8F, 12F));
		}
	}

	/*********************************************************
    * UPDATE
    *********************************************************/
	void Update () {
		if (target != null && guided) {
			look = Quaternion.LookRotation (target.transform.position - transform.position);
			transform.rotation = Quaternion.Slerp (transform.rotation, look, Time.deltaTime * 2.0F);
			rb.velocity = transform.forward * 80;
		} else if (target != null && !guided) {
			rb.velocity = transform.forward * 80;
		}
			
	}
}
