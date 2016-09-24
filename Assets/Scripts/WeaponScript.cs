using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;

public class WeaponScript : MonoBehaviour {

	public string weaponName;
	public int ammoPackValue;
	public float shotSpeed;
	public float maxRateOfFire;
	public bool isFullAuto;
	public bool ammoDetonates;
	public GameObject bullet;
	public GameObject barrelFlash;
	public GameObject sparks;
	public int DefaultAmmo;
	public AudioClip ammoPickup;
	public bool isLaser = false;
	public AudioClip fireSound;
	private int ammo;
	private float detonation_time;
	private float nextFireTime;
	private bool ammo_pulled = false;
	private AudioSource m_AudioSource;
	private LineRenderer line;
	private int charge;
	private GameObject gameController;

	void Start () {
		m_AudioSource = GetComponent<AudioSource>();
		gameController = GameObject.FindGameObjectWithTag ("GameController");
		if (isLaser) {
			line = gameObject.GetComponent<LineRenderer> ();
			line.enabled = false;
			charge = 100;
			//firing = false;
		}
		nextFireTime = 0;
		detonation_time = 5;
		GetAmmoLevel ();
		InvokeRepeating ("ScanTarget", Random.Range(0.5F, 1.0F), 0.5F);
	}

	public void PlayPickupSound()
	{
		if (ammoPickup != null && m_AudioSource != null) {
			m_AudioSource.PlayOneShot (ammoPickup);
		}
	}
		
	public void DeleteAmmoLevel()
	{
		string save_string = SceneManager.GetActiveScene ().name + "_" + weaponName + "_ammo";
		PlayerPrefs.DeleteKey (save_string);
	}

	public void ResetAmmoLevel()
	{
		string save_string = SceneManager.GetActiveScene ().name + "_" + weaponName + "_ammo";
		PlayerPrefs.SetInt (save_string, DefaultAmmo);
	}

	public void ScanTarget()
	{
		if (gameObject.activeSelf) {
			Ray ray = new Ray (transform.position, transform.forward);
			RaycastHit hit;
			string text = "";
			if (Physics.Raycast (ray, out hit, 9999)) {
				if (hit.transform.name.Contains("BreakableCube")) {
					text = hit.distance.ToString() + "\nAmmo Crate\nDestroyable";
				} else if (hit.transform.name.Contains ("AlliedDrone")) {
					text = hit.distance.ToString() + "\nAllied Drone\nDO NOT FIRE!";
				}
				else if (hit.transform.CompareTag ("AlliedPortal")) {
					text = hit.distance.ToString() + "\nAllied Portal\nDO NOT FIRE!";
				}
				else if (hit.transform.name.Contains ("AttackDrone")) {
					text = hit.distance.ToString() + "\nAttack Drone\nDestroy with\nExtreme Prejudice";
				}
				else if (hit.transform.CompareTag ("BattleCruiser")) {
					text = hit.distance.ToString() + "\nBattleCruiser\nWARNING\nUnit is heavily armed!";
				}
				else if (hit.transform.CompareTag ("HeavyTurret")) {
					text = hit.distance.ToString() + "\nHeavy Turret\nWARNING\nDon't allow it to fire!";
				}
				else if (hit.transform.CompareTag ("PointDefense")) {
					text = hit.distance.ToString() + "\nPoint Defense\nWARNING\nRockets are ineffective\nAgainst target";
				}
				else if (hit.transform.CompareTag ("Portal")) {
					text = hit.distance.ToString() + "\nPortal\nWARNING\nDestroy before more\nDrones come through!";
				}
				else if (hit.transform.name.Contains ("TacticalAlliedDrone")) {
					text = hit.distance.ToString() + "\nAllied Tactical Drone\nDO NOT FIRE!";
				}
				else if (hit.transform.name.Contains ("HeavyDrone")) {
					text = hit.distance.ToString() + "\nHeavy Drone\nDANGER!\nDrone is heavily armed!";
				}
				else if (hit.transform.name.Contains ("HeavyAlliedDrone")) {
					text = hit.distance.ToString() + "\nAllied Heavy Drone\nDO NOT FIRE!";
				}
				else if (hit.transform.CompareTag ("TacticalDrone")) {
					text = hit.distance.ToString() + "\nTactical Drone\nWARNING\nDestroy with\nExtreme Prejudice";
				}
				else if (hit.transform.CompareTag ("Turret")) {
					text = hit.distance.ToString() + "\nTurret\nWARNING\nDestroy with\nExtreme Prejudice";
				}
				else if (hit.transform.name.Contains ("LevelMarker")) {
					text = "Level Marker\nWalk through\nThis to save\tthe game";
				}
				else if (hit.transform.name.Contains ("BreakableWall")) {
					text = hit.distance.ToString() + "\nWall is destroyable";
				}
				else if (hit.transform.name.Contains ("CaptureGunPickup")) {
					text = hit.distance.ToString() + "\nCapture Gun\nPick this up\nAnd you can\nCapture enemy drones";
				}
				else if (hit.transform.name.Contains ("GrenadeLauncherPickup")) {
					text = hit.distance.ToString() + "\nGrenade Launcher\nPick this up\nto fire grenades\n";
				}
				else if (hit.transform.name.Contains ("HealthPack")) {
					text = hit.distance.ToString() + "\nHealth Pack\nPick this up\nto gain more\nhealth";
				}
				else if (hit.transform.name.Contains ("LaserPickup")) {
					text = hit.distance.ToString() + "\nLaser Gun\nPick this up\nto fire a\ndirected energy\nbeam at targets";
				}
				else if (hit.transform.name.Contains ("MachineGunPickup")) {
					text = hit.distance.ToString() + "\nMachine Gun\nPick this up\nto fire bullets\nrapidly at targets";
				}
				else if (hit.transform.name.Contains("RocketLauncherPickup")) {
					text = hit.distance.ToString() + "\nRocket Launcher\nPick this up\nto fire rockets\nat targets";
				}
				else if (hit.transform.name.Contains ("GuidedMissileLauncherPickup")) {
					text = hit.distance.ToString() + "\nGuided Rocket Launcher\nPick this up\nto fire guided rockets\nat targets";
				}
				else if (hit.transform.name.Contains ("PlasmaLauncherPickup")) {
					text = hit.distance.ToString() + "\nPlasma Launcher\nPick this up\nto fire plasma rockets\nat targets";
				}
				else if (hit.transform.CompareTag ("RocketTurret")) {
					text = hit.distance.ToString() + "\nRocket Turret\nWARNING\nLaunches guided rockets\nAt targets";
				}
				gameController.SendMessage ("SetTargetText", text);
			}
		}
	}


	public void GetAmmoLevel()
	{
		if (!ammo_pulled) {
			if (SceneManager.GetActiveScene ().name.Equals ("CaptureMode") && weaponName.Equals ("Capture Gun")) {
				ammo = 999999;
			} else {
				ammo_pulled = true;
				string save_string = SceneManager.GetActiveScene ().name + "_" + weaponName + "_ammo";
				if (PlayerPrefs.HasKey (save_string)) {
					ammo = PlayerPrefs.GetInt (save_string);
				} else {
					PlayerPrefs.SetInt (save_string, DefaultAmmo);
					ammo = DefaultAmmo;
				}
			}
		}
	}

	public void WeaponActive()
	{
		GetAmmoLevel ();
		DisplayWeaponText ();
	}

	public void ResetAmmo()
	{
		PlayerPrefs.DeleteKey (SceneManager.GetActiveScene ().name + "_" + weaponName + "_ammo");
	}

	public void SaveAmmo()
	{
		string save_string = SceneManager.GetActiveScene ().name + "_" + weaponName + "_ammo";
		PlayerPrefs.SetInt (save_string, ammo);
	}

	public void CollectAmmo(GameObject ammoPack)
	{
		if (isLaser) {
			if (ammoPack.name.Contains ("LaserAmmo")) {
				GetAmmoLevel ();
				if (ammo < 10000) {
					ammo += 500;
					Destroy (ammoPack);
					DisplayWeaponText ();
				}
			}
		} else {
			GetAmmoLevel ();
			Destroy (ammoPack.gameObject);
			ammo += ammoPackValue;
			DisplayWeaponText ();
		}
	}

	void DisplayWeaponText()
	{
		if (isLaser) {
			string weaponText = "Weapon: Laser\n";
			weaponText = "Weapon: Laser" + "\n";
			weaponText += "Battery: " + ammo.ToString () + "kwh\n";
			weaponText += "Charge: " + charge.ToString () + "%";
			if (gameController != null) {
				gameController.SendMessage ("SetWeaponText", weaponText);
			}
		} else {
			string weaponText = "Weapon: " + weaponName + "\n";
			weaponText += "Ammo: " + ammo.ToString () + "\n";
			if (ammoDetonates) {
				weaponText += "Detonation Time: " + detonation_time.ToString ();
			}
#if !MOBILE_INPUT
			weaponText += "('b' to switch)";
#endif
			if (gameController != null) {
				gameController.SendMessage ("SetWeaponText", weaponText);
			}
		}
	}

	void FireWeapon()
	{
		if (isLaser && !line.enabled && nextFireTime < Time.time && charge > 0) {
			StopCoroutine ("FireLaser");
			StartCoroutine ("FireLaser");
		}
		else if (!isLaser && ammo > 0 && nextFireTime < Time.time)
		{
			GameObject blast_clone;
			if (SceneManager.GetActiveScene ().name.Equals ("CaptureMode") && weaponName.Equals ("Capture Gun")) {

			} else {
				ammo--;
			}
			DisplayWeaponText();
			Vector3 position = transform.position;
			if (barrelFlash != null) {
				Instantiate(barrelFlash, position, transform.rotation);
			}
			blast_clone = Instantiate(bullet, position, transform.rotation) as GameObject;
			blast_clone.GetComponent<Rigidbody>().velocity = transform.forward * shotSpeed;
			if (ammoDetonates)
			{
				blast_clone.GetComponent("Projectile").SendMessage("SetDetonation", detonation_time);
			}
			GameObject[] pointdefenses = GameObject.FindGameObjectsWithTag ("PointDefenseLaser");
			foreach (GameObject point in pointdefenses)
			{
				point.SendMessage("AddIgnoreId", blast_clone.GetInstanceID());
			}
			nextFireTime = Time.time + maxRateOfFire;
		}
	}

	bool is_holding_fire()
	{
		int ySelectionZone = Screen.height / 6;
		int xSelectionZone = Screen.width / 4;
		for (int i = 0; i < Input.touchCount; i++)
		{
			if (Input.GetTouch(i).position.y < ySelectionZone * 1 && Input.GetTouch(i).position.x > xSelectionZone)
			{
				return true;
			}
		}
		return false;
	}
#if MOBILE_INPUT
	bool last_time_holding_fire = false;
#endif
	// Update is called once per frame
	void Update () {
#if !MOBILE_INPUT
		if (Input.GetButton("Fire1") && gameObject.activeSelf && Time.timeScale == 1){
			FireWeapon();

		}
#else
		if (isFullAuto && is_holding_fire())
		{
			FireWeapon();
		}
		else if (is_holding_fire() && !last_time_holding_fire)
		{
			FireWeapon();
			last_time_holding_fire = true;
		}
		else if (!is_holding_fire() && last_time_holding_fire)
		{
			last_time_holding_fire = false;
		}
			
#endif
		if (ammoDetonates)
		{
		   if (Input.GetKeyDown (KeyCode.N)) {
			  if (detonation_time > 0)
			  {
				  detonation_time--;
				  DisplayWeaponText();
			  }

		   }
		  if (Input.GetKeyDown (KeyCode.M)) {
			  if (detonation_time < 10)
			  {
				 detonation_time++;
				 DisplayWeaponText();
			  }
		   }
		}
		if (isLaser) {
			if (charge < 100 && line.enabled == false) {
				if (ammo > 0) {
					charge++;
					ammo--;
					DisplayWeaponText();
				}
			}
		}
	}

	IEnumerator FireLaser()
	{
		line.enabled = true;
		//firing = true;
		m_AudioSource.PlayOneShot (fireSound);
#if !MOBILE_INPUT
		while (Input.GetButton ("Fire1") && charge > 0 && gameObject.activeSelf)
#else
		while (is_holding_fire() && charge > 0 && gameObject.activeSelf)
#endif
		{
			float randomJump = Random.Range(-0.01F, 0.01F);
			Vector3 positionNew = transform.position;
			positionNew.x += randomJump;
			Ray ray = new Ray (positionNew, transform.forward);
			RaycastHit hit;
			line.SetPosition (0, ray.origin);
			if (Physics.Raycast (ray, out hit, 1000)) {
				line.SetPosition (1, hit.point);
				if (hit.rigidbody) {
					if (hit.transform.name.Contains ("BreakableCube") || hit.transform.name.Contains ("AttackDrone") ||
						hit.transform.name.Contains("TargetSphere") || hit.transform.CompareTag("Portal") ||
						hit.transform.name.Contains("TacticalDrone")) {
						hit.transform.SendMessage ("HitByLaser");
					} else {
						hit.rigidbody.AddForceAtPosition (transform.forward * 100, hit.point);
					}
					Instantiate (sparks, hit.point, hit.transform.rotation);
				}
			} else {		
				line.SetPosition (1, ray.GetPoint (1000));
			}
			charge--;
			DisplayWeaponText ();
			yield return null;
		}
		line.enabled = false;
		if (charge == 0) {
			nextFireTime = Time.time + 1.0F;
		}
		line.enabled = false;
	}

}
