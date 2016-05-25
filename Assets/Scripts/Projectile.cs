using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	public GameObject explosion;
	public bool explodeOnContact;
	private float detonation_time;
	private bool explode;
	private bool explodeOnDestroy;
	public int health;
	void Start() {
		explode = false;

	}

	public void SetDetonation(float detonation)
	{
		if (detonation < 0) {
			explodeOnContact=true;
		} else {
			detonation_time = detonation;
			explodeOnContact=false;
			explodeOnDestroy = true;
			Destroy (gameObject, detonation_time);
		}
	}

	void OnDestroy() {
		if (explode || explodeOnDestroy) {
			Rigidbody rb = GetComponent<Rigidbody> ();
			Instantiate (explosion, rb.position, rb.rotation);
		}
	}

	public void SetExplodeOnContact()
	{
		explodeOnContact = true;
	}

	void OnCollisionEnter(Collision other)
	{
		if (explodeOnContact) {
			explode = true;
			Destroy (gameObject);
		}
	}

	public void HitByPointDefense()
	{
		health--;
		if (health <= 0) {
			explode = true;
			Destroy (gameObject);
		}
	}
}
