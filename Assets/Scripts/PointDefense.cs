using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PointDefense : MonoBehaviour {

	private GameObject target = null;
	LineRenderer line;

	// Use this for initialization
	void Start () {
		line = gameObject.GetComponent<LineRenderer> ();
		line.useWorldSpace = true;
	}
		
	// Update is called once per frame
	void Update () {
		if (line.enabled == false) {
			GameObject[] rockets = GameObject.FindGameObjectsWithTag ("AlliedUnit");
			foreach (GameObject rocket in rockets) {
				//Rocket must not be on the same team as the point defense to be a valid target
				if ((rocket.name.Contains("Rocket") || rocket.name.Contains("Missile")) && Vector3.Distance (rocket.transform.position, transform.position) < 40) {
					transform.LookAt (rocket.transform.position);
					target = rocket;
					StartCoroutine ("FirePointBeam");
					break;
				}
			}
		}
	}

	/*********************************************************************
    * FIRE POINT BEAM
    * DESCRIPTION: Fires a point defense beam at an enemy target.
    *********************************************************************/
	IEnumerator FirePointBeam()
	{
		line.enabled = true;
		while (target != null)
		{
			Vector3 positionNew = transform.position;
			Ray ray = new Ray (positionNew, transform.forward);
			RaycastHit hit;
			line.SetPosition (0, ray.origin);
			if (Physics.Raycast (ray, out hit, 1000)) {
				line.SetPosition (1, hit.point);
				if (hit.rigidbody) {
					if (hit.transform.name.Contains ("Rocket") || hit.transform.name.Contains("Missile")){
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
		line.enabled = false;
	}
}
