using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class DestroyByCollision : MonoBehaviour {

	public int health;
	public bool DestroyOnGroundContact;
	public bool DetectLaserHits;
	public bool destroyAllDrones;
	public bool spawnRockets;
	public bool spawnGuidedRockets;
	public bool spawnLaserBatteries;
	public bool spawnCaptureGunPack;

	public bool SpawnNewOnDeath;
	public int SpawnCount;
	public GameObject spawnObject;

	public bool SpawnAmmoOnDeath;
	public GameObject MachineGunAmmo;
	public GameObject RifleAmmo;
	public GameObject GrenadeAmmo;
	public GameObject LaserBatteries;
	public GameObject RocketPack;
	public GameObject GuidedRocketPack;
	public GameObject AlliedDroneModel;
	public GameObject captureGunPack;

	public bool SpawnHealthOnDeath;
	public GameObject HealthPack;

	public bool IncreaseHealthOnDeath;
	public int HealthIncreaseAmmount;

	public int ScoreValue;
	public int iterations_laser_hit = 0;
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

	public void HitByLaser()
	{
		if (DetectLaserHits) {
			health--;
			iterations_laser_hit = 0;
			if (health < 0) {
				objectWillExplode = true;
				Destroy (gameObject);
			}
		}
		iterations_laser_hit++;
	}
	void OnCollisionEnter(Collision other)
	{
		int remove_value = 0;
		int difficulty = PlayerPrefs.GetInt ("AILevel");
		if (SceneManager.GetActiveScene ().name.Equals("SurvivalMode") && (gameObject.CompareTag ("Portal") || gameObject.CompareTag ("AlliedPortal"))) {
			//Destroy (other.gameObject);
			return;
		}
		if (other.gameObject.CompareTag("CaptureDart")) {
			Destroy (other.gameObject);
			if (CompareTag ("AttackDrone") || CompareTag("TacticalDrone")) {
				Instantiate (AlliedDroneModel, transform.position, transform.rotation);
				objectWillExplode = false;
				Destroy (gameObject);
				return;

			}
		}

		if (other.gameObject.CompareTag("Grenade")) {
			if (difficulty == 0) {
				remove_value = 30;
			} else if (difficulty == 1) {
				remove_value = 20;
			} else {
				remove_value = 15;
			}
		}
		if (other.gameObject.CompareTag("Rocket")) {
			if (difficulty == 0) {
				remove_value = 50;
			} else if (difficulty == 1) {
				remove_value = 40;
			} else if (difficulty == 2) {
				remove_value = 30;
			}
		}
		if (other.gameObject.CompareTag ("RifleBullet")) {
			if (difficulty == 0) {
				remove_value = 8;
			} else if (difficulty == 1) {
				remove_value = 6;
			} else {
				remove_value = 4;
			}
		}
		if (other.gameObject.CompareTag ("MachineGunBullet")) {
			if (difficulty == 0) {
				remove_value = 4;
			} else if (difficulty == 1) {
				remove_value = 3;
			} else {
				remove_value = 2;
			}
		}
		if (remove_value > 0) {
			health -= remove_value;
			Destroy (other.gameObject);
			objectWillExplode = true;
			if (health < 0) {
				objectWillExplode = true;
				Destroy (gameObject);
			}
		}

		if (DestroyOnGroundContact && other.gameObject.CompareTag ("Ground")) {
			Destroy (gameObject);
		}
	}

	void OnTriggerEnter(Collider other) {
		
		if (other.gameObject.CompareTag ("Explosion")) {
			health -= 20;
			if (health < 0)
			{
				objectWillExplode = true;
				Destroy (gameObject);
			}
		}
		
	}

	public void SetExplode()
	{
		objectWillExplode = true;
	}

	void OnDestroy() {
		if (objectWillExplode) {
			Rigidbody rb = GetComponent<Rigidbody> ();
			Vector3 newPosition = rb.position;
			//Object explosion
			Instantiate (explosion, rb.position, rb.rotation);
			if (controller == null) {
				return;
			}
			//Allied units don't add score
			if (!CompareTag ("AlliedDrone") && !CompareTag ("AlliedTacticalDrone") && !CompareTag("AlliedPortal") && health == 0) {
				gameController.SendMessage ("AddToScore", ScoreValue);
			}
			//Increase player health on death
			if (CompareTag("AttackDrone") || CompareTag("TacticalDrone")) {
				gameController.SendMessage ("DroneDestroyed");
			}
			if (IncreaseHealthOnDeath)
			{
				controller.GetComponent ("FirstPersonController").SendMessage ("IncreaseHalth", HealthIncreaseAmmount);
			}
			//Spawn New objects on death
			if (SpawnNewOnDeath)
			{
				newPosition.y += 12;
				for (int i = 0; i < SpawnCount; i++)
				{
					Instantiate (spawnObject, newPosition, rb.rotation);
				}
			}
			//Ammo pack is created when this object is destroyed
			if (SpawnAmmoOnDeath)
			{
				int value = Random.Range (1, 13);
				if (value == 3 && MachineGunAmmo != null) {
					Instantiate (MachineGunAmmo, newPosition, rb.rotation);
				} else if (value == 4 && GrenadeAmmo != null) {
					Instantiate (GrenadeAmmo, newPosition, rb.rotation);
				} else if (value == 5 && spawnLaserBatteries && LaserBatteries != null) {
					Instantiate (LaserBatteries, newPosition, rb.rotation);
				} else if (value == 6 && spawnRockets && RocketPack != null) {
					Instantiate (RocketPack, newPosition, rb.rotation);
				} else if (value == 7 && spawnGuidedRockets && GuidedRocketPack != null) {
					Instantiate (GuidedRocketPack, newPosition, rb.rotation);
				} else if (value == 8 && spawnCaptureGunPack && captureGunPack != null) {
					Instantiate (captureGunPack, newPosition, rb.rotation);
				} else if (value == 9 || value == 10 || value == 11 || value == 12) {
					Instantiate (RifleAmmo, newPosition, rb.rotation);
				}
			}
			//Health is randomly spawned on death
			if (SpawnHealthOnDeath)
			{
				int value = Random.Range (1, 6);
				if (value != 2)
				{
					Instantiate (HealthPack, newPosition, rb.rotation);
				}
			}
			if (destroyAllDrones) {
				GameObject[] attackDrones = GameObject.FindGameObjectsWithTag ("AttackDrone");
				foreach (GameObject drone in attackDrones) {
					drone.GetComponent ("DestroyByCollision").SendMessage ("SetExplode");
					Destroy (drone);
				}
			}
		}
	}

}
