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
	public int DefaultAmmo;
	private int ammo;
	private float detonation_time;
	private float temporaryTextTime;
	private bool notActive;
	private float nextFireTime;
	private bool ammo_pulled = false;

	private GameObject gameController;

	void Start () {
		ParticleSystem system = GetComponent<ParticleSystem>();
		if (system != null) {
			system.Pause ();
		}
		gameController = GameObject.FindGameObjectWithTag ("GameController");
		nextFireTime = 0;
		detonation_time = 5;
		GetAmmoLevel ();
		//Invoke ("DisplayWeaponText", 0.5F);
		InvokeRepeating ("ScanTarget", Random.Range(0.5F, 1.0F), 0.5F);
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
				if (hit.transform.CompareTag ("Crate")) {
					text = hit.distance.ToString() + "\nAmmo Crate\nDestroyable";
				} else if (hit.transform.CompareTag ("AlliedDrone")) {
					text = hit.distance.ToString() + "\nAllied Drone\nDO NOT FIRE!";
				}
				else if (hit.transform.CompareTag ("AlliedPortal")) {
					text = hit.distance.ToString() + "\nAllied Portal\nDO NOT FIRE!";
				}
				else if (hit.transform.CompareTag ("AttackDrone")) {
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
				//else if (hit.transform.CompareTag ("RocketTurret")) {
				//	text = "Rocket Turret\nWARNING\nDestroy before it can fire!";
				//}
				else if (hit.transform.CompareTag ("AlliedTacticalDrone")) {
					text = hit.distance.ToString() + "\nAllied Tactical Drone\nDO NOT FIRE!";
				}
				else if (hit.transform.CompareTag ("TacticalDrone")) {
					text = hit.distance.ToString() + "\nTactical Drone\nWARNING\nDestroy with\nExtreme Prejudice";
				}
				else if (hit.transform.CompareTag ("Turret")) {
					text = hit.distance.ToString() + "\nTurret\nWARNING\nDestroy with\nExtreme Prejudice";
				}
				else if (hit.transform.CompareTag ("LevelMarker")) {
					text = "Level Marker\nWalk through\nThis to save\tthe game";
				}
				else if (hit.transform.CompareTag ("DestroyableWall")) {
					text = hit.distance.ToString() + "\nWall is destroyable";
				}
				else if (hit.transform.CompareTag ("CaptureGunPickup")) {
					text = hit.distance.ToString() + "\nCapture Gun\nPick this up\nAnd you can\nCapture enemy drones";
				}
				else if (hit.transform.CompareTag ("GrenadeLauncherPickup")) {
					text = hit.distance.ToString() + "\nGrenade Launcher\nPick this up\nto fire grenades\n";
				}
				else if (hit.transform.CompareTag ("HealthPack")) {
					text = hit.distance.ToString() + "\nHealth Pack\nPick this up\nto gain more\nhealth";
				}
				else if (hit.transform.CompareTag ("LaserGunPickup")) {
					text = hit.distance.ToString() + "\nLaser Gun\nPick this up\nto fire a\ndirected energy\nbeam at targets";
				}
				else if (hit.transform.CompareTag ("MachineGunPickup")) {
					text = hit.distance.ToString() + "\nMachine Gun\nPick this up\nto fire bullets\nrapidly at targets";
				}
				else if (hit.transform.CompareTag ("RocketLauncherPickup")) {
					text = hit.distance.ToString() + "\nRocket Launcher\nPick this up\nto fire rockets\nat targets";
				}
				else if (hit.transform.CompareTag ("GuidedRocketLauncherPickup")) {
					text = hit.distance.ToString() + "\nGuided Rocket Launcher\nPick this up\nto fire guided rockets\nat targets";
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
		GetAmmoLevel ();
		Destroy(ammoPack.gameObject);
		ammo+=ammoPackValue;
		DisplayWeaponText();
	}

	void DisplayWeaponText()
	{
		string weaponText = "Weapon: " + weaponName + "\n";
		weaponText += "Ammo: " + ammo.ToString() + "\n";
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


	// Update is called once per frame
	void Update () {
#if !MOBILE_INPUT
		if (isFullAuto && Input.GetButton("Fire1") && gameObject.activeSelf && Time.timeScale == 1){
#else
		if (isFullAuto && CrossPlatformInputManager.GetButton("FireBtn") && gameObject.activeSelf && Time.timeScale == 1){
#endif
			if (ammo > 0 && nextFireTime < Time.time)
			{
				GameObject blast_clone;
				if (SceneManager.GetActiveScene ().name.Equals ("CaptureMode") && weaponName.Equals ("Capture Gun")) 
				{
					
				}
				else
				{
				   ammo--;
				}
				DisplayWeaponText();
				Vector3 position = transform.position;

				ParticleSystem system = GetComponent<ParticleSystem>();
				if (system != null) {
					system.Emit(1);
				}
				blast_clone = Instantiate(bullet, position, transform.rotation) as GameObject;
				blast_clone.GetComponent<Rigidbody>().velocity = transform.forward * shotSpeed;
				nextFireTime = Time.time + maxRateOfFire; 
				GameObject[] pointdefenses = GameObject.FindGameObjectsWithTag ("PointDefenseLaser");
				foreach (GameObject point in pointdefenses)
				{
					point.SendMessage("AddIgnoreId", blast_clone.GetInstanceID());
				}
			}
		}
#if !MOBILE_INPUT
		else if (Input.GetButton("Fire1") && gameObject.activeSelf && Time.timeScale == 1){
#else
		else if (CrossPlatformInputManager.GetButtonDown("FireBtn") && gameObject.activeSelf && Time.timeScale == 1){
#endif
			if (ammo > 0 && nextFireTime < Time.time)
			{
				GameObject blast_clone;

				if (SceneManager.GetActiveScene ().name.Equals ("CaptureMode") && weaponName.Equals ("Capture Gun")) 
				{

				}
				else
				{
					ammo--;
				}
				DisplayWeaponText();
				Vector3 position = transform.position;
				ParticleSystem system = GetComponent<ParticleSystem>();
				if (system != null) {
					system.Emit(1);
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
	}

}
