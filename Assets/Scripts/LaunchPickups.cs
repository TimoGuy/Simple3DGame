using UnityEngine;
using System.Collections;

public class LaunchPickups : MonoBehaviour {

	public GameObject rifleAmmo;
	public GameObject laserBattery;
	public GameObject healthPack;
	public GameObject machineGunAmmo;
	public GameObject guidedRockets;
	public GameObject rockets;
	public GameObject grenades;
	public GameObject captureDarts;
	public bool health_only;

	// Use this for initialization
	void Start () {
		Invoke("LaunchAmmo", Random.Range(5.0F, 15.0F));
	}
	
	private void LaunchAmmo()
	{
		int randomObject = Random.Range (0, 20);
		Rigidbody launch_object = healthPack.GetComponent<Rigidbody>();

		if (health_only) {
			launch_object = healthPack.GetComponent<Rigidbody> ();
		}
		else if (randomObject == 3 && laserBattery != null) {
			launch_object = laserBattery.GetComponent<Rigidbody>();
		}
		else if ((randomObject == 2 || randomObject == 1) && healthPack != null) {
			launch_object = healthPack.GetComponent<Rigidbody>();
		}
		else if ((randomObject == 4 || randomObject == 11 || randomObject == 12) && machineGunAmmo != null) {
			launch_object = machineGunAmmo.GetComponent<Rigidbody>();
		}
		else if (randomObject == 5 && guidedRockets != null) {
			launch_object = guidedRockets.GetComponent<Rigidbody>();
		}
		else if (randomObject == 6 && guidedRockets != null) {
			launch_object = guidedRockets.GetComponent<Rigidbody>();
		}
		else if (randomObject == 7 && rockets != null) {
			launch_object = rockets.GetComponent<Rigidbody>();
		}
		else if (randomObject == 13 && captureDarts != null) {
			launch_object = captureDarts.GetComponent<Rigidbody>();
		}
		else if ((randomObject == 8 || randomObject == 9 || randomObject == 10) && grenades != null) {
			launch_object = grenades.GetComponent<Rigidbody>();
		}
		else
		{
			launch_object = rifleAmmo.GetComponent<Rigidbody>();
		}

		Rigidbody blast_clone = Instantiate (launch_object, transform.position, transform.rotation) as Rigidbody;
		blast_clone.velocity = new Vector3 (Random.Range(5, 20), Random.Range(25, 50), Random.Range(3, 15));

		if (health_only) {
			Invoke ("LaunchAmmo", Random.Range (60.0F, 120.0F));
		} else {
			Invoke ("LaunchAmmo", Random.Range (20.0F, 25.0F));
		}

	}
}
