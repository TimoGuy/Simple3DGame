using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Drone : MonoBehaviour {

	private GameObject move_target = null;
	private GameObject attack_target = null;
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
	public float standoffCurX = 0;
	public float standoffCurY = 0;
	//private float standoffPositionx = 0;
	//private float standoffPositionz = 0;
	public string lastRaycast;
	private Vector3 standoffPosition;
	public bool isAllied;
	public bool isTactical;
	public bool isAssistant = false;
	//private bool rightSide = false;
	private int fireCount = 0;
	private float secondaryRange;
	private int attempted_fires = 0;
	private float next_random_move = 0F;
	private List<GameObject> invalidTargets = new List<GameObject>();


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
		if (!isAssistant) {
			InvokeRepeating ("AquireTarget", Random.Range (0.1F, 1.0F), 10.0F);
			if (isTactical) {
				Invoke ("FireAtTarget", Random.Range (0.5F, 1.0F));
			} else {
				InvokeRepeating ("FireAtTarget", Random.Range (1.0F, 3.0F), rateOfFire);
			}
		} else {
			//InvokeRepeating ("AquirePlayerToFollow", Random.Range (0.1F, 1.0F), 0.5F);
		}
	}

	bool isInvalid(GameObject target)
	{
		foreach (GameObject invalid in invalidTargets)
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
	private GameObject FindClosestTarget(string tag, float start_dist_in, out float start_dist_out)
	{
		GameObject closestTarget = null;
		GameObject[] targetList = GameObject.FindGameObjectsWithTag (tag);
		foreach (GameObject targetUnit in targetList) {
			if (!targetUnit.name.Contains("Rocket") && !targetUnit.name.Contains("Missile") && Vector3.Distance (targetUnit.transform.position, transform.position) < start_dist_in && !isInvalid(targetUnit)) {
				closestTarget = targetUnit;//.transform;
				start_dist_in = Vector3.Distance (targetUnit.transform.position, transform.position);
			}
		}
		start_dist_out = start_dist_in;
		return closestTarget;
	}

	void AquireTarget()
	{
		move_target = AquireClosestTarget (isAllied);
		standoffPosition = AquirePositionTarget (move_target, 30F);
		attack_target = move_target;
	}

	void AquirePlayerToFollow()
	{
		move_target = GameObject.FindGameObjectWithTag ("Player");
		attack_target = AquireClosestTarget (isAllied);
		standoffPosition = AquirePositionTarget (move_target, 5F);
	}

	/*************************************************************
    * AQUIRE TARGETS
    * DESCRIPTION: Finds a target to go after in the game.
    *************************************************************/
	GameObject AquireClosestTarget(bool in_is_allied)
	{
		GameObject closest_target = null;
		float start_dist = 10000.0F;
		if (isAllied) {
			closest_target = FindClosestTarget ("EnemyUnit", start_dist, out start_dist);
			if (!SceneManager.GetActiveScene ().name.Equals ("SurvivalMode")) {
				closest_target = FindClosestTarget ("Portal", start_dist, out start_dist);
			}

		} else {
			GameObject player_target = GameObject.FindGameObjectWithTag ("Player");
			float player_distance = Vector3.Distance (player_target.transform.position, transform.position);
			closest_target = FindClosestTarget ("AlliedUnit", start_dist, out start_dist);
			if (player_distance < start_dist) {
				closest_target = player_target;
			}
			if (!SceneManager.GetActiveScene ().name.Equals ("SurvivalMode")) {
				closest_target = FindClosestTarget ("AlliedPortal", start_dist, out start_dist);
			}
		}

		return closest_target;
	}

	/*************************************************************
    * AQUIRE POSITION TARGET
    * DESCRIPTION: Calculates an appropriate standoff position to the
    * currently selected target.
    *************************************************************/
	Vector3 AquirePositionTarget(GameObject closest_target, float range)
	{
		float standoffPositionx = Random.Range (range*-1F, range);
		float standoffPositionz = Random.Range (range*-1F, range);
		Vector3 standOffVector = transform.position;

		if (closest_target != null) {
			standOffVector = closest_target.transform.position;
			if (isTactical) {
				standOffVector.y += Random.Range (25, 70);
				standOffVector.z += standoffPositionz;
				standOffVector.x += standoffPositionx;
			} else if (isAssistant) {

				if (Time.time > next_random_move) {
					standoffCurX = Random.Range (-5, 5);
					standoffCurY = Random.Range (-5, 5);
					next_random_move = Time.time + 3F;
				}
				standOffVector.y += 2;
				standOffVector.z += standoffCurX;
				standOffVector.x += standoffCurY;

			}else {
				standOffVector.y += Random.Range (5, 50);
				standOffVector.z += standoffPositionz;
				standOffVector.x += standoffPositionx;
			}
		}
		return standOffVector;
	}

	bool Fire(GameObject active_bullet, float max_range, float position_offset, float shot_speed)
	{
		float dist = Vector3.Distance (attack_target.transform.position, transform.position);
		if (dist < max_range) {
			GameObject shot;
			Vector3 position = transform.position;
			position = transform.position + transform.forward * position_offset;
			shot = Instantiate (active_bullet, position, transform.rotation) as GameObject;
			shot.GetComponent<Rigidbody> ().velocity = transform.forward * shot_speed;
			return true;
		}
		return false;
	}

	/*************************************************************
    * FIRE SECONDARY AT TARGET
    * DESCRIPTION: Fires the secondary weapon at the enemy target
    *************************************************************/
	void FireSecondaryAtTarget()
	{
		if (attack_target != null) {
			transform.LookAt (attack_target.transform.position);
			Ray ray = new Ray (transform.position, transform.forward);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, secondaryRange)) {
				//Only fire if target is in range
				lastRaycast = hit.transform.tag;
				if (hit.transform.name.Contains ("BreakableCube") || hit.transform.CompareTag (attack_target.tag)) {
					Fire(secondary, secondaryRange, 15.0F, 80F);
				} else {
					attempted_fires++;
					if (attempted_fires == 10) {
						attempted_fires = 0;
						invalidTargets.Add (attack_target);
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
		if (attack_target != null) {
			transform.LookAt (attack_target.transform.position);
			Ray ray = new Ray (transform.position, transform.forward);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, shotRange)) {
				lastRaycast = hit.transform.tag;
				if (hit.transform.name.Contains ("BreakableCube") || hit.transform.CompareTag (attack_target.tag)) {
					fired = Fire (bullet, shotRange, shotOffset, shotSpeed);
				} else {
					attempted_fires++;
					if (attempted_fires == 10) {
						attempted_fires = 0;
						invalidTargets.Add (attack_target);
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
		if (isAssistant) {
			move_target = GameObject.FindGameObjectWithTag ("Player");
			standoffPosition = AquirePositionTarget (move_target, 5F);
		}
		float step = speed * Time.deltaTime;
		transform.position = Vector3.MoveTowards (transform.position, standoffPosition, step);
	}
}
