using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Projectile : MonoBehaviour {

	public GameObject explosion;
	public int health;
	public bool explodeOnContact;
	public bool isGuided = false;
	private GameObject target = null;
	private bool explode;
	Quaternion look;
	Rigidbody rb;
	public bool isAllied = false;
	private bool guided = true;

	void Start() {
		explode = false;
		if (isGuided) {
			rb = GetComponent<Rigidbody> ();
			Invoke ("Detonate", 180.0F);
			FindTarget ();
			InvokeRepeating ("FindTarget", 5.0F, 5.0F);
			Invoke ("toggleGuidedMode", Random.Range (8, 12));
		}

	}
		
	/************************************************************
    * SET DETONATION
    *************************************************************/
	public void SetDetonation(float detonation)
	{
		if (detonation < 0) {
			explodeOnContact=true;
		} else {
			explodeOnContact=false;
			Invoke ("Detonate", detonation);
		}
	}

	/************************************************************
    * DETONATE
    *************************************************************/
	void Detonate()
	{
		explode = true;
		Destroy (gameObject);
	}

	/************************************************************
    * ON DESTROY
    *************************************************************/
	void OnDestroy() {
		if (explode) {
			Rigidbody rb = GetComponent<Rigidbody> ();
			Instantiate (explosion, rb.position, rb.rotation);
		}
	}

	/************************************************************
    * ON APPLICATION QUIT
    *************************************************************/
	void OnApplicationQuit()
	{
		explode = false;
		explodeOnContact = false;
	}

	/************************************************************
    * ON COLLISION ENTER
    *************************************************************/
	void OnCollisionEnter(Collision other)
	{
		if (explodeOnContact) { //Projectile explodes no matter what it hits
			explode = true;
			Destroy (gameObject);
			//Projectile only explodes if it hit's an enemy unit.
		} else if (other.gameObject.CompareTag ("EnemyUnit") || other.gameObject.CompareTag ("AlliedUnit") ||
			other.gameObject.CompareTag("Player") || other.gameObject.name.Contains("BreakableCube")) {
			explode = true;
			Destroy (gameObject);
		}
	}
		
	/************************************************************
    * HIT BY POINT DEFENSE
    * DESCRIPTION: Called if the projectile is hit by a point defense laser
    *************************************************************/
	public void HitByPointDefense()
	{
		health--;
		if (health <= 0) {
			explode = true;
			Destroy (gameObject);
		}
	}

	/************************************************************
    * HIT BY LASER
    *************************************************************/
	public void HitByLaser()
	{
		explode = true;
		Destroy (gameObject);
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
				targetList.Add ("EnemyUnit");
			} else {
				targetList.Add ("Player");
				targetList.Add ("AlliedUnit");
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
    * AQUIRE RANDOM TARGET
    *********************************************************/
	void AquireRandomTarget(List<string> targetList)
	{
		List<GameObject> targets = new List<GameObject> ();
		foreach (string unit_target in targetList) {
			GameObject[] foundTargets = GameObject.FindGameObjectsWithTag (unit_target);
			foreach (GameObject foundTarget in foundTargets) {
				if (!foundTarget.name.Contains ("Turret") && !foundTarget.name.Contains("Missile") && !foundTarget.name.Contains("Rocket")) {
					targets.Add (foundTarget);
				}
			}
		}
		int random_target = Random.Range (0, targets.Count - 1);
		if (targets.Count > 0) {
			target = targets [random_target];
		}
	}

	/*********************************************************
    * UPDATE
    *********************************************************/
	void Update () {
		if (isGuided) {
			if (target != null && guided) {
				look = Quaternion.LookRotation (target.transform.position - transform.position);
				transform.rotation = Quaternion.Slerp (transform.rotation, look, Time.deltaTime * 2.0F);
				rb.velocity = transform.forward * 80;
			} else if (target != null && !guided) {
				rb.velocity = transform.forward * 80;
			}
		}

	}
}
