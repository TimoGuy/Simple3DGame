using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.SceneManagement;

public class Laser : MonoBehaviour {

	LineRenderer line;
	private int charge;
	private int batteryCharge;
	private bool firing;
	private float chargeTime;
	public AudioClip fireSound;
	public GameObject sparks;
	public GameObject gameController;
	private bool ammo_pulled = false;
	// Use this for initialization

	public void SaveAmmo()
	{
		string save_string = SceneManager.GetActiveScene ().name + "_" + "batteryCharge";
		PlayerPrefs.SetInt (save_string, batteryCharge);
	}
	public void WeaponActive()
	{
		GetAmmoLevel ();
		DisplayWeaponStatus ();
	}
	public void CollectAmmo(GameObject battery)
	{
		if (battery.CompareTag ("LaserBattery")) {
			GetAmmoLevel ();
			if (batteryCharge < 10000) {
				batteryCharge += 500;
				Destroy (battery);
				DisplayWeaponStatus ();
				//SaveAmmo ();
			}
		}

	}

	public void DeleteAmmoLevel()
	{
		string save_string = SceneManager.GetActiveScene ().name + "_" + "batteryCharge";
		PlayerPrefs.DeleteKey (save_string);
	}

	public void ResetAmmoLevel()
	{
		string save_string = SceneManager.GetActiveScene ().name + "_" + "batteryCharge";
		PlayerPrefs.SetInt (save_string, 3000);
	}

	public void GetAmmoLevel()
	{
		if (!ammo_pulled) {
			ammo_pulled = true;
			string save_string = SceneManager.GetActiveScene ().name + "_" + "batteryCharge";
			if (PlayerPrefs.HasKey (save_string)) {
				batteryCharge = PlayerPrefs.GetInt (save_string);
			} else {
				PlayerPrefs.SetInt (save_string, 3000);
				batteryCharge = 3000;
			}
		}
	}

	void DisplayWeaponStatus()
	{
		string weaponText = "Weapon: Laser\n";
		weaponText = "Weapon: Laser" + "\n";
		weaponText += "Battery: " + batteryCharge.ToString () + "kwh\n";
		weaponText += "Charge: " + charge.ToString () + "%";
		if (gameController != null) {
			gameController.SendMessage ("SetWeaponText", weaponText);
		}
	}

	void Start () {
		gameController = GameObject.FindGameObjectWithTag ("GameController");
		line = gameObject.GetComponent<LineRenderer> ();
		line.enabled = false;
		charge = 100;
		firing = false;
		chargeTime = 0;
		GetAmmoLevel ();
	}
		
	// Update is called once per frame
	void Update () {
#if !MOBILE_INPUT
		if (Input.GetButtonDown("Fire1") && gameObject.activeSelf && chargeTime < Time.time && charge > 0 && Time.timeScale == 1) {
#else
		if (CrossPlatformInputManager.GetButtonDown("FireBtn") && gameObject.activeSelf && chargeTime < Time.time && charge > 0 && Time.timeScale == 1){
#endif
			GetComponent<AudioSource> ().PlayOneShot (fireSound, 5.0F);
			StopCoroutine ("FireLaser");
			StartCoroutine ("FireLaser");
		}
		if (charge < 100 && firing == false) {
			if (batteryCharge > 0)
			{
			   charge++;
			   batteryCharge--;
				if (charge == 100)
				{

				}
			    DisplayWeaponStatus ();
			}
		}
	}

	IEnumerator FireLaser()
	{
		line.enabled = true;
		firing = true;
#if !MOBILE_INPUT
		while (Input.GetButton ("Fire1") && charge > 0)
#else
		while (CrossPlatformInputManager.GetButton("FireBtn") && charge > 0)
#endif
		{
			//line.renderer.
			float randomJump = Random.Range(-0.01F, 0.01F);
			Vector3 positionNew = transform.position;
			positionNew.x += randomJump;
			Ray ray = new Ray (positionNew, transform.forward);
			RaycastHit hit;
			line.SetPosition (0, ray.origin);
			if (Physics.Raycast (ray, out hit, 1000)) {
				line.SetPosition (1, hit.point);
				if (hit.rigidbody) {
					if (hit.transform.CompareTag ("Crate") || hit.transform.CompareTag ("AttackDrone") ||
						hit.transform.CompareTag ("Explodeable") || hit.transform.CompareTag("Portal")) {
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
			DisplayWeaponStatus ();
			yield return null;
		}
		firing = false;
		if (charge == 0) {
			chargeTime = Time.time + 1.0F;
		}
		line.enabled = false;
	}
}
