/*************************************************************
* SPHERE BREAKER
* DESCRIPTION: Sphere breaker is a simple 3D unity game. In the
* game you move through a simple course in an "All Range Mode"
* You can go whereever you like. Enemy spheres move in at random
* and begin to attack, you must destroy as many of them as you
* can before you are overwhelmed and destroyed.
* Author: Jonathan L Clark
* Date: 3/8/2016
* Update: 9/29/2016, Reved to version 1.3.10. Preformed a minor
* refactor on the FirstPersonController, modified the LoadWeapons function
* to use a loop, now new weapons can be added seemlessly. Now using a weapon
* class instead of a firepoint dictionary, weapon dictionary and int array (for user having a weapon).
* Now using a bool instead of an int to indicate when the player has a weapon. Fixed an issue
* where the cluster launcher ammo was spawning the GuidedMissileCollect object. Added hit by laser to
* the projectile script so rockets and other objects can be hit by the laser.
* Update: 9/30/2016, Reved to version 1.3.11, Preformed a minor refactor of the first person controller
* added code to generate a proper pickup string message when a weapon is picked up. Combined the
* guided rocket and Projectile scripts. fixed an issue where the grenade was not exploding when it hit
* enemy units. Cleaned up the Projectile and DestroyByCollision scripts. Added hitByPoint defense to the 
* DestroyByCollision script. Fixed an issue where grenades were not destroying crates. Reduced the walk
* speed sound rate (as it was way too fast).
* Update: 10/1/2016, Reved to version 1.3.12, Reduced the ammo levels for the plasma cannon and
* capture gun. Performed a refactor on the drone script. Now the target and aquire target functions
* take params instead of using globals. The target is now a GameObject rather than a transform. Added
* the assistant drone which helps protect the player from enemies.
************************************************************/
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
	public class Weapon
	{
		public bool has;
		public GameObject firePoint;
		public GameObject weapon;
		public Weapon (GameObject weapon_in, GameObject firePoint_in, bool inHas)
		{
			weapon = weapon_in;
			firePoint = firePoint_in;
			has = inHas;
		}
	};

    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private bool m_IsWalking;
        [SerializeField] private float m_WalkSpeed;
        [SerializeField] private float m_RunSpeed;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;
        [SerializeField] private MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;
        [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

		public enum WeaponType { None=-1, Rifle=0, GrenadeLauncher=1, MachineGun=2, Laser=3, RocketLauncher=4, GuidedMissileLauncher=5, CaptureGun=6, PlasmaLauncher=7, ClusterLauncher=8};
        private Camera m_Camera;
		public GameObject assistant_drone;
        private bool m_Jump;
        private float m_YRotation;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;

		private Dictionary<WeaponType, Weapon> weapons = new Dictionary<WeaponType, Weapon> ();
	
        private AudioSource m_AudioSource;
		private bool gameOver = false;

		private GameObject gameController;
		private WeaponType current = WeaponType.Rifle;
		private int health = 100;
		private int lives = 1;
		private bool lives_updated = false;
		private float lastSaveTime;
#if MOBILE_INPUT
		private float pitch = 0x0f;
		private float yaw = 0x0f;
#endif
        // Use this for initialization
        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
			m_NextStep = m_StepCycle/2f;
            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();
			m_MouseLook.Init(transform , m_Camera.transform);
			LoadWeapons ();

			if (!SceneManager.GetActiveScene ().name.Equals ("SurvivalMode")) {
				LoadPrefs ();
			}
			else if (SceneManager.GetActiveScene ().name.Equals ("CaptureMode")) {
				weapons [WeaponType.Rifle].has = false;
				weapons [WeaponType.CaptureGun].has = true;
				current = WeaponType.CaptureGun;
			}
			else
			{
				weapons [WeaponType.Rifle].has = true;
			}

			lastSaveTime = Random.Range(5.00F, 30.0F);
			Invoke ("InitialDisplay", 0.5F);
			SwitchWeapons(current, true, false);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
        }

		/********************************************
        * SAVE PREFS
        ********************************************/
		private void SavePrefs()
		{
			PlayerPrefs.SetFloat ("StartLocationX" + "_" + SceneManager.GetActiveScene ().name, transform.position.x);
			PlayerPrefs.SetFloat ("StartLocationY" + "_" + SceneManager.GetActiveScene ().name, transform.position.y);
			PlayerPrefs.SetFloat ("StartLocationZ" + "_" + SceneManager.GetActiveScene ().name, transform.position.z);
			PlayerPrefs.SetInt ("health" +  "_" + SceneManager.GetActiveScene ().name, health);
			PlayerPrefs.SetInt ("lives" +  "_" + SceneManager.GetActiveScene ().name, lives);
			PlayerPrefs.SetInt ("curWeapon" + "_" + SceneManager.GetActiveScene ().name, (int)current);
			PlayerPrefs.Save ();
		}

		/************************************************ 
        * SAFE LOAD INT PREF
        ************************************************/
		private int SafeLoadPref(string key, int default_value)
		{
			if (PlayerPrefs.HasKey (key + "_" + SceneManager.GetActiveScene ().name)) {
				return PlayerPrefs.GetInt (key + "_" + SceneManager.GetActiveScene ().name);
			} else {
				PlayerPrefs.SetInt (key + "_" + SceneManager.GetActiveScene ().name, default_value);
				return default_value;
			}
		}

		/************************************************ 
        * SAFE LOAD FLOAT PREF
        ************************************************/
		private float SafeLoadPref(string key, float default_value)
		{
			if (PlayerPrefs.HasKey (key + "_" + SceneManager.GetActiveScene ().name)) {
				return PlayerPrefs.GetFloat (key + "_" + SceneManager.GetActiveScene ().name);
			} else {
				PlayerPrefs.SetFloat (key + "_" + SceneManager.GetActiveScene ().name, default_value);
				return default_value;
			}
		}

		/********************************************
        * LOAD PREFS
        * DESCRIPTION: Load all prefs related to the fps controller
        ********************************************/
		private void LoadPrefs()
		{
			float startX = SafeLoadPref ("StartLocationX", 0.0F);
			float startY = SafeLoadPref ("StartLocationY", 0.0F);
			float startZ = SafeLoadPref ("StartLocationZ", 0.0F);
			current = (WeaponType)SafeLoadPref ("curWeapon", 0);
			health = SafeLoadPref ("health", 100);
			lives = SafeLoadPref ("lives", 5);
			if (startX == 0.0F && startY == 0.0F && startZ == 0.0F) {
			} else {
				transform.position = new Vector3(startX, startY, startZ + 5.0F);
			}
		}

		/********************************************
        * LOAD WEAPONS
        * DESCRIPTION: Loads all the weapons from the fps
        ********************************************/
		private void LoadWeapons()
		{
			string local = "FirstPersonCharacter";
			var values = Enum.GetValues (typeof(WeaponType));
			foreach (var value in values) {
				WeaponType type = (WeaponType)value;
				if (type != WeaponType.None) {
					bool hasWeapon = Convert.ToBoolean(SafeLoadPref (((int)type).ToString() + "_" + SceneManager.GetActiveScene ().name, 0));
					Weapon targetWeapon = new Weapon (GameObject.Find (local + "/" + type.ToString ()), GameObject.Find(local + "/" + type.ToString () + "/FirePoint"), hasWeapon);
					weapons.Add (type, targetWeapon);
				}
			}
			gameController = GameObject.FindGameObjectWithTag ("GameController");
		}

		/********************************************
        * INITILAL DISPLAY
        * DESCRIPTION: Displays the player's initial health.
        ********************************************/
		private void InitialDisplay()
		{
			ReduceHealth (0);
		}

		/********************************************
        * INCREASE HEALTH
        ********************************************/
		public void IncreaseHalth(int amount)
		{
			health += amount;
			if (health > 100) {
				health = 100;
			}
			ReduceHealth(0);
		}

		/********************************************
        * CLEAR WEAPON SAVES
        ********************************************/
		private void ClearWeaponSaves()
		{
			foreach (WeaponType weapon in weapons.Keys) {
				weapons [weapon].weapon.SetActive (true);
				weapons[weapon].firePoint.SendMessage ("DeleteAmmoLevel");
				weapons [weapon].weapon.SetActive (false);
			}
		}

		/*******************************************
        * CLEAR FIRST PERSON CONTROLLER
        *******************************************/
		private void ClearFirstPersonController()
		{
			PlayerPrefs.DeleteKey("StartLocationX" + "_" + SceneManager.GetActiveScene ().name);
			PlayerPrefs.DeleteKey ("StartLocationY" + "_" + SceneManager.GetActiveScene ().name);
			PlayerPrefs.DeleteKey ("StartLocationZ" + "_" + SceneManager.GetActiveScene ().name);
			PlayerPrefs.DeleteKey ("health" +  "_" + SceneManager.GetActiveScene ().name);
			PlayerPrefs.DeleteKey ("lives" +  "_" + SceneManager.GetActiveScene ().name);
			PlayerPrefs.DeleteKey ("curWeapon" + "_" + SceneManager.GetActiveScene ().name);
			foreach (WeaponType weapon in weapons.Keys) {
				PlayerPrefs.DeleteKey ("Weapon_" + ((int)weapon).ToString() + "_" + SceneManager.GetActiveScene ().name);
				weapons [weapon].weapon.SetActive (true);
				weapons[weapon].firePoint.SendMessage ("ResetAmmo");
				weapons [weapon].weapon.SetActive (false);
			}
		}

		/********************************************
        * REDUCE HEALTH
        * DESCRIPTION: Reduce the players health by
        ********************************************/
		public void ReduceHealth(int amount)
		{
			if (health > 0) {
				health -= amount;
				if (health <= 0)
				{
					lives--;
					if (SceneManager.GetActiveScene ().name.Equals ("SurvivalMode")) {
						gameController.SendMessage ("GameOver");
						gameOver = true;
						Invoke ("LoadMainMenu", 5.0F);

					}
					else if (lives > 0)
					{
						health = 100;
						gameController.SendMessage ("SaveDrones");
						PlayerPrefs.SetInt ("health" + "_" + SceneManager.GetActiveScene ().name, health);
						PlayerPrefs.SetInt ("lives" + "_" + SceneManager.GetActiveScene ().name, lives);
						PlayerPrefs.Save ();
						SceneManager.LoadScene ("MiniGame");
					}
					else
					{
						gameController.SendMessage ("SetDefaults");
						gameController.SendMessage ("GameOver");
						Invoke ("LoadMainMenu", 5.0F);
						gameOver = true;
						ClearWeaponSaves ();
						ClearFirstPersonController ();
						PlayerPrefs.Save ();
					}
				}
			}
			gameController.SendMessage ("UpdateHealth", health);
			if (!lives_updated) {
				lives_updated = true;
				gameController.SendMessage ("UpdateLives", lives); 
			}
		}

		/********************************************
        * LOAD MAIN MENU
        ********************************************/
		private void LoadMainMenu()
		{
			SceneManager.LoadScene ("MainMenu");
		}

		/********************************************
        * NEXT WEAPON HEALTH
        * DESCRIPTION: Switches to the next available weapon
        ********************************************/
		private void NextWeapon()
		{
			bool foundNewWeapon = false;
			current++; //Move to the first new weapon
			for (int i = (int)current; i < weapons.Keys.Count; i++) {
				WeaponType type = (WeaponType)i;
				if (weapons[type].has) {
					current = type;
					foundNewWeapon = true;
					break;
				}
			}
			//We have no other weapons, switch back to the rifle.
			if (foundNewWeapon == false) {
				current = WeaponType.Rifle;
			}
		}

		/********************************************
        * SWITCH WEAPONS
        * DESCRIPTION: Switches the players weapon
        ********************************************/
		public void SwitchWeapons(WeaponType newWeapon, bool save, bool playReload)
		{
			foreach (WeaponType weaponType in weapons.Keys) {
				weapons [weaponType].weapon.SetActive (false);
			}

			current = newWeapon;

			weapons [newWeapon].weapon.SetActive (true);
			weapons[newWeapon].firePoint.SendMessage ("WeaponActive");
			if (playReload) {
				weapons[newWeapon].firePoint.SendMessage ("PlayPickupSound");
			}
		}

		/*****************************************************
        * IS HOLDING JUMP (MOBILE ONLY)
        *****************************************************/
		bool is_holding_jump()
		{
			int ySelectionZone = Screen.height / 6;
			for (int i = 0; i < Input.touchCount; i++)
			{
				//Second 6th of the screen from the top
				if (Input.GetTouch(i).position.y < ySelectionZone * 5 &&  Input.GetTouch(i).position.y > ySelectionZone * 4)
				{
					return true;
				}
			}
			return false;
		}

		bool last_time_holding_jump = false;
		bool pressed_jump_area()
		{
			if (is_holding_jump() && !last_time_holding_jump)
			{
				last_time_holding_jump = true;
				return true;
			}
			else if (!is_holding_jump() && last_time_holding_jump)
			{
				last_time_holding_jump = false;
			}
			return false;
		}

        // Update is called once per frame
        private void Update()
        {
			if ((Input.GetKeyDown ("b") || CrossPlatformInputManager.GetButtonDown("Fire2")) && gameOver == false) {
				NextWeapon ();
				SwitchWeapons(current, true, false);
			}
			if (gameOver == false) {
				RotateView ();
			}
            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump)
            {
#if MOBILE_INPUT
				m_Jump = pressed_jump_area ();
#else
				m_Jump = CrossPlatformInputManager.GetButton("Jump");
#endif
            }

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }
            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }
			
        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        private void FixedUpdate()
        {
            float speed;
            GetInput(out speed);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward*m_Input.y + transform.right*m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               m_CharacterController.height/2f);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x*speed;
            m_MoveDir.z = desiredMove.z*speed;


            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else
            {
                m_MoveDir += Physics.gravity*m_GravityMultiplier*Time.fixedDeltaTime;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir*Time.fixedDeltaTime);

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);
        }


        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }

		private bool lastStep = false;
        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

			if (!lastStep) {
				PlayFootStepAudio ();
				lastStep = true;
			} else {
				lastStep = false;
			}
        }


        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }


        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");

            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#else
			m_IsWalking = false; //Always run on mobile platforms
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }

        }

        private void RotateView()
        {
#if !MOBILE_INPUT
			if (Time.timeScale > 0) {
               m_MouseLook.LookRotation (transform, m_Camera.transform);
			}

#else
			   int xSectionSize = Screen.width / 4;
			   int ySectionSize = Screen.height / 4;

			   //Find the first touch that is within range of the screen
			   for (int i = 0; i < Input.touchCount; i++)
			   {
				  if (Input.GetTouch(i).position.y > ySectionSize && Input.GetTouch(i).position.x > xSectionSize)
				  {
			       
					 float rotateYSpeed = 200.0F * PlayerPrefs.GetFloat("lookYSensitivity");
					 float rotateXSpeed = 200.0F * PlayerPrefs.GetFloat("lookXSensitivity");
			         pitch = Input.GetTouch (i).deltaPosition.y * rotateYSpeed * -1 * Time.deltaTime;
			         yaw = Input.GetTouch (i).deltaPosition.x * rotateXSpeed * Time.deltaTime;

			         if ((m_Camera.transform.eulerAngles.x < 400 && m_Camera.transform.eulerAngles.x + pitch > 280) ||
				         (m_Camera.transform.eulerAngles.x > -1 && m_Camera.transform.eulerAngles.x + pitch < 60))
			             {
			                 m_Camera.transform.localRotation *= Quaternion.Euler(pitch, 0, 0);
			             }
			             transform.localRotation *= Quaternion.Euler(0, yaw, 0);
				         break; //Don't need to continue in this loop
			       }
			    }
#endif
        }

		/*************************************************
        * PICKUP WEAPON
        *************************************************/
		void PickupWeapon(GameObject weapon, WeaponType weaponType, string weaponMessage, string ammoMessage)
		{
			if (!weapons[weaponType].has) {
				weapons[weaponType].has = true;
				Destroy (weapon);
				gameController.SendMessage("SetTempText", weaponMessage); 
			} else {
				bool isActive = weapons [weaponType].weapon.activeSelf;
				weapons [weaponType].weapon.SetActive (true);
				weapons[weaponType].firePoint.SendMessage ("CollectAmmo", weapon);
				weapons [weaponType].weapon.SetActive (isActive);
				gameController.SendMessage("ammoMessage", weaponMessage); 
			}
			SwitchWeapons (current, true, true);
		}

		/***********************************************
        * PICKUP AMMO
        ***********************************************/
		void PickupAmmo(GameObject inputPack, WeaponType weaponType, string message)
		{
			bool isActive = weapons [weaponType].weapon.activeSelf;
			weapons [weaponType].weapon.SetActive (true);
			weapons[weaponType].firePoint.SendMessage ("CollectAmmo", inputPack);
			weapons [weaponType].weapon.SetActive (isActive);
			SwitchWeapons (current, true, true);
			gameController.SendMessage("SetTempText", message); 

		}

		/************************************************
        * ON TRIGGER ENTER
        ************************************************/
		void OnTriggerEnter(Collider other) {
			//Is it a weapon or ammo?
			foreach (WeaponType weaponType in weapons.Keys) {
				string weaponName = Regex.Replace (weaponType.ToString (), "(\\B[A-Z])", " $1");
				if (other.gameObject.name.Contains (weaponType.ToString () + "AmmoCollect")) {
					PickupAmmo (other.gameObject, weaponType, "Picked up " + weaponName + " ammo");
					break;
				} else if (other.gameObject.name.Contains (weaponType.ToString () + "PickupCollect")) {
					PickupWeapon(other.gameObject, weaponType, "Picked up " + weaponName, "Picked up " + weaponName + " ammo");
					break;
				}
			}
			if (other.gameObject.name.Contains ("ExplosionPlasma")) {
				ReduceHealth (15);
			}
			if (other.gameObject.name.Contains ("ExplosionLarge")) {
				ReduceHealth(5);
			}
			if (other.gameObject.name.Contains ("ExplosionMassive")) {
				ReduceHealth(10);
			}
			if (other.gameObject.name.Contains ("HealthPack")) {
				if (health < 100)
				{
					Destroy(other.gameObject);
					health += 20;
					if (health > 100)
					{
						health = 100;
					}
					ReduceHealth (0);
					gameController.SendMessage("SetTempText", "Picked up health"); 
				}
			}
			if (other.gameObject.name.Contains ("DroneSpawnBox")) {
				Destroy (other.gameObject);
				Instantiate (assistant_drone, transform.position, transform.rotation);
				gameController.SendMessage("SetTempText", "Assistant drone will now help you"); 
			}
			if (other.gameObject.name.Contains ("LevelMarker") && lastSaveTime < Time.time) {
				foreach (WeaponType weapon in weapons.Keys) {
					weapons [weapon].weapon.SetActive (true);
					weapons[weapon].firePoint.SendMessage ("SaveAmmo");
					weapons [weapon].weapon.SetActive (false);
				}
				foreach (WeaponType weaponType in weapons.Keys) {
					PlayerPrefs.SetInt (((int)weaponType).ToString() + "_" + SceneManager.GetActiveScene ().name, Convert.ToInt32(weapons[weaponType].has));
				}
				SavePrefs ();
				SwitchWeapons (current, false, false);
				gameController.SendMessage ("SaveGame");
				gameController.SendMessage("SetTempText", "Game Saved");
				IncreaseHalth (20);

				lastSaveTime = Time.time + 10; //Prevents too many rapid save operations at a time.
				
			}
		}

		void OnCollisionEnter(Collision other)
		{
			if (other.gameObject.name.Contains ("Rocket")) {
#if !MOBILE_INPUT
				ReduceHealth(20);
#else
				ReduceHealth(10);
#endif
				Destroy(other.gameObject);
			}
			if (other.gameObject.name.Contains ("HighVelocityRound")) {
#if !MOBILE_INPUT
				ReduceHealth(5);
#else
				ReduceHealth(2);
#endif
				Destroy(other.gameObject);
			}
			if (other.gameObject.name.Contains ("HighExplosiveRoun")) {
#if !MOBILE_INPUT
				ReduceHealth(10);
#else
				ReduceHealth(5);
#endif
				Destroy(other.gameObject);
			}
		}

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
			//Fall damage
			if (hit.gameObject.CompareTag ("Ground") && m_CharacterController.velocity.y < -15) {
				float healthReduce = m_CharacterController.velocity.y * -1.0F;
				int healthReduction = (int)healthReduce;
				ReduceHealth(healthReduction);
			}
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        }
    }
}
