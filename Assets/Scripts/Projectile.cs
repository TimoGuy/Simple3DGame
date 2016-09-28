using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	public GameObject explosion;
	public bool explodeOnContact;
	private bool explode;
	public int health;
	void Start() {
		explode = false;

	}

	public void SetDetonation(float detonation)
	{
		if (detonation < 0) {
			explodeOnContact=true;
		} else {
			explodeOnContact=false;
			Invoke ("Detonate", detonation);
		}
	}

	void Detonate()
	{
		explode = true;
		Destroy (gameObject);
	}

	void OnDestroy() {
		
		if (explode) {
			Rigidbody rb = GetComponent<Rigidbody> ();
			Instantiate (explosion, rb.position, rb.rotation);
		}
	}

	void OnApplicationQuit()
	{
		explode = false;
		explodeOnContact = false;
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
		} else if (other.gameObject.name.Contains ("EnemyUnit") || other.gameObject.CompareTag ("AlliedUnit") ||
			other.gameObject.CompareTag("Player")) {
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
