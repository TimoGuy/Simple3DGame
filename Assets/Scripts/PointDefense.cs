using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PointDefense : MonoBehaviour {

	private bool firing = false;
	private GameObject target = null;
	LineRenderer line;
	List<int> ignoreList = new List<int>();

	// Use this for initialization
	void Start () {
		line = gameObject.GetComponent<LineRenderer> ();
		line.useWorldSpace = true;
	}

	public void AddIgnoreId(int input)
	{
		ignoreList.Add (input);
	}

	private bool ignoreRocket(int instanceId)
	{
		foreach (int id in ignoreList) {
			if (id == instanceId) {
				return true;
			}
		}
		return false;
	}

	// Update is called once per frame
	void Update () {
		if (firing == false) {
			GameObject[] rockets = GameObject.FindGameObjectsWithTag ("Rocket");
			foreach (GameObject enemyDrone in rockets) {
				//Rocket must not be on the same team as the point defense to be a valid target
				if (!ignoreRocket(enemyDrone.GetInstanceID()) && Vector3.Distance (enemyDrone.transform.position, transform.position) < 30) {
					transform.LookAt (enemyDrone.transform.position);
					target = enemyDrone;
					StartCoroutine ("FirePointBeam");
					break;
				}
			}
		}
	}

	IEnumerator FirePointBeam()
	{
		firing = true;
		line.enabled = true;

		while (target != null)
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
					if (hit.transform.CompareTag ("Rocket")){
						hit.transform.SendMessage ("HitByPointDefense");
					} else {
						hit.rigidbody.AddForceAtPosition (transform.forward * 100, hit.point);
					}
				}
			} else {		
				line.SetPosition (1, ray.GetPoint (1000));
			}
			transform.LookAt (target.transform.position);
			yield return null;
		}
		firing = false;
		line.enabled = false;
	}
}
