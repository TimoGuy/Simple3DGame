using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class DestroyByCollision : MonoBehaviour {

	public int health;
	public bool DestroyOnGroundContact;
	public bool DetectLaserHits;
	public bool destroyAllDrones;
	public int SpawnCount;
	public bool spawn_ammo = false;
	public GameObject spawnObject = null;
	public GameObject MachineGunAmmo = null;
	public GameObject RifleAmmo = null;
	public GameObject GrenadeAmmo = null;
	public GameObject LaserBatteries = null;
	public GameObject RocketPack = null;
	public GameObject GuidedRocketPack = null;
	public GameObject AlliedDroneModel = null;
	public GameObject captureGunPack = null;
	public GameObject HealthPack = null;
	public int HealthIncreaseAmmount;

	public int ScoreValue;
	public GameObject explosion;
	
	private bool objectWillExplode;
	private GameObject gameController;
	private UnityStandardAssets.Characters.FirstPerson.FirstPersonController controller;

	// Use this for initialization
	void Start () {
		objectWillExplode = false;
		gameController = GameObject.FindGameObjectWithTag ("GameController");
		controller = GameObject.FindObjectOfType<UnityStandardAssets.Characters.FirstPerson.FirstPersonController> ();	
	}

	/********************************************************
    * HIT BY LASER
    ********************************************************/
	public void HitByLaser()
	{
		if (gameObject.name.Contains ("Portal") && SceneManager.GetActiveScene ().name.Equals ("SurvivalMode"))
		{
			return;
		}
		if (DetectLaserHits) {
			health--;
			if (health < 0) {
				objectWillExplode = true;
				Destroy (gameObject);
			}
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
			objectWillExplode = true;
			Destroy (gameObject);
		}
	}

	/********************************************************
    * ON COLLISION ENTER
    ********************************************************/
	void OnCollisionEnter(Collision other)
	{
		int remove_value = 0;
		int difficulty = PlayerPrefs.GetInt ("AILevel");

		if (other.gameObject.name.Contains("CaptureDart")) {
			Destroy (other.gameObject);
			if (CompareTag("EnemyUnit") && (name.Contains("AttackDrone") || name.Contains("TacticalDrone") || name.Contains("HeavyDrone"))) {
				Instantiate (AlliedDroneModel, transform.position, transform.rotation);
				objectWillExplode = false;
				Destroy (gameObject);
				return;

			}
		}

		if (other.gameObject.name.Contains("Grenade")) {
			if (difficulty == 0) {
				remove_value = 30;
			} else if (difficulty == 1) {
				remove_value = 20;
			} else {
				remove_value = 15;
			}
		}
		if (other.gameObject.name.Contains("Rocket") || other.gameObject.name.Contains("Missile")) {
			if (difficulty == 0) {
				remove_value = 50;
			} else if (difficulty == 1) {
				remove_value = 40;
			} else if (difficulty == 2) {
				remove_value = 30;
			}
		}
		if (other.gameObject.name.Contains ("HighVelocityRound")) {
			if (difficulty == 0) {
				remove_value = 14;
			} else if (difficulty == 1) {
				remove_value = 8;
			} else {
				remove_value = 6;
			}
		}
		if (other.gameObject.name.Contains ("MachineGunBullet")) {
			if (difficulty == 0) {
				remove_value = 4;
			} else if (difficulty == 1) {
				remove_value = 3;
			} else {
				remove_value = 2;
			}
		}

		if (other.gameObject.name.Contains ("PlasmaBall")) {
			if (difficulty == 0) {
				remove_value = 80;
			} else if (difficulty == 1) {
				remove_value = 70;
			} else {
				remove_value = 60;
			}
		}

		if (gameObject.name.Contains("Portal") && SceneManager.GetActiveScene ().name.Equals ("SurvivalMode")) {
			remove_value = 0;
		}

		if (remove_value > 0) {
			health -= remove_value;
			//Destroy (other.gameObject);
			if (health < 0) {
				objectWillExplode = true;
				Destroy (gameObject);
			}
		}

		if (DestroyOnGroundContact && other.gameObject.CompareTag ("Ground")) {
			Destroy (gameObject);
		}
	}

	/********************************************************
    * ON TRIGGER ENTER
    ********************************************************/
	void OnTriggerEnter(Collider other) {

		if ((gameObject.name.Contains("Portal")) && SceneManager.GetActiveScene ().name.Equals ("SurvivalMode"))
		{
			return;
		}
		if (other.gameObject.name.Contains ("ExplosionPlasma")) {
			health -= 40;
		}
		if (other.gameObject.name.Contains ("ExplosionLarge")) {
			health -= 10;
		}
		if (other.gameObject.name.Contains ("ExplosionMassive")) {
			health -= 20;
		}
		if (health < 0)
		{
			objectWillExplode = true;
			Destroy (gameObject);
		}
	}

	/********************************************************
    * SET EXPLODE
    ********************************************************/
	public void SetExplode()
	{
		objectWillExplode = true;
	}

	/********************************************************
    * ON APPLICATION QUIT
    ********************************************************/
	void OnApplicationQuit()
	{
		objectWillExplode = false;
	}

	/********************************************************
    * ON DESTROY
    ********************************************************/
	void OnDestroy() {
		if (objectWillExplode) {
			Rigidbody rb = GetComponent<Rigidbody> ();
			Vector3 newPosition = rb.position;
			//Object explosion
			Instantiate (explosion, rb.position, rb.rotation);
			if (controller == null || gameController == null) {
				return;
			}
			//Allied units don't add score
			if (!name.Contains ("AlliedDrone") && !name.Contains ("TacticalAlliedDrone") && !name.Contains("AlliedPortal") && health < 0) {
				gameController.SendMessage ("AddToScore", ScoreValue);
			}
			//Destroy the enemy drones
			if (name.Contains("AttackDrone") || name.Contains("TacticalDrone")) {
				gameController.SendMessage ("DroneDestroyed");
			}
			if (HealthIncreaseAmmount > 0)
			{
				controller.GetComponent ("FirstPersonController").SendMessage ("IncreaseHalth", HealthIncreaseAmmount);
			}
			//Spawn New objects on death
			if (spawnObject != null)
			{
				newPosition.y += 12;
				for (int i = 0; i < SpawnCount; i++)
				{
					Instantiate (spawnObject, newPosition, rb.rotation);
				}
			}
			if (spawn_ammo) {
				int value = Random.Range (1, 13);
				if (value == 3 && MachineGunAmmo != null) {
					Instantiate (MachineGunAmmo, newPosition, rb.rotation);
				} else if (value == 4 && GrenadeAmmo != null) {
					Instantiate (GrenadeAmmo, newPosition, rb.rotation);
				} else if (value == 5 && LaserBatteries != null) {
					Instantiate (LaserBatteries, newPosition, rb.rotation);
				} else if (value == 6 && RocketPack != null) {
					Instantiate (RocketPack, newPosition, rb.rotation);
				} else if (value == 7 && GuidedRocketPack != null) {
					Instantiate (GuidedRocketPack, newPosition, rb.rotation);
				} else if (value == 8 && captureGunPack != null) {
					Instantiate (captureGunPack, newPosition, rb.rotation);
				} else if (RifleAmmo != null && value == 9 || value == 10 || value == 11 || value == 12) {
					Instantiate (RifleAmmo, newPosition, rb.rotation);
				}
			}
			//Health is randomly spawned on death
		    if (HealthPack != null)
			{
				int value = Random.Range (1, 6);
				if (value != 2)
				{
					Instantiate (HealthPack, newPosition, rb.rotation);
				}
			}
			if (destroyAllDrones) {
				GameObject[] attackDrones = GameObject.FindGameObjectsWithTag ("EnemyUnit");
				foreach (GameObject drone in attackDrones) {
					if (drone.name.Contains ("AttackDrone")) {
						drone.GetComponent ("DestroyByCollision").SendMessage ("SetExplode");
						Destroy (drone);
					}
				}
			}
		}
	}

}
