/*************************************************************
* SPHERE BREAKER
* DESCRIPTION: Sphere breaker is a simple 3D unity game. In the
* game you move through a simple course in an "All Range Mode"
* You can go whereever you like. Enemy spheres move in at random
* and begin to attack, you must destroy as many of them as you
* can before you are overwhelmed and destroyed.
* Author: Jonathan L Clark
* Date: 3/8/2016
* Update: 5/28/2016, Reved to version 1.1.44 (Alpha 12), Improved the guided rocket
* now the rocket sends a raycast to find any target directly in front of it before
* attempting to aquire another target. Rocket turn radius has been decreased for better
* accuracy. Modified the rocket trails to look better when the rockets turn and curve.
* Update: 5/30/2016, Reved to version 1.1.45 (Alpha 12), Replaced the health text with a
* health bar. This bar indicates how much health the user has. Fixed an issue on exit
* wher the guided rocket is attempting to access the target game object. Added
* the option to rotate the view using the accelerometor.
* Update: 5/31/2016, Reved to version 1.1.46 (Alpha 12), Added target distance to active
* targeting. Modifed the tactical drone to shoot a little ahead of the target, modified the
* guided rocket to explode once it is close to a target. Increased guided rocket turn radius. Added code that
* flashes the screen red when the player is hit. Added smoke effect to the rifle. Modified the switch weapon
* button to be smaller and just say "Weapon"
* Update: 6/1/2016, Reved to version 1.1.47 (Alpha 12), Preformed a minor refactor of the GameController
* added tactical drone counts to the screen. Fixed an issue where allies add to the players score when
* destroyed. Fixed another issue where tactical drones were not being counted as destroyed. Decreased
* the crate drop rate of the battle cruiser. Fixed some issues with how and when battle cruisers are launched
* and how the counts are handled for Survival Mode. Cruiser speed decreases as more cruisers are launched. This makes the game
* more difficult as it becomes more difficult to fight off the drones they produce. No longer updating the screen text
* when points are scored (as the tick timer updates this for us). Added code to launch cruisers in waves after
* a certain number of them have passed by. Only incrementing the score if the objects health is at 0; this fixes
* an issue where the player score increments when a battlecruiser passes the screen. Added the game over screen overlay
* this will now display "Game Over" when the player is defeated. Fixed an issue in the projectile script where projectiles
* were Instantiating explosions when destroyed at the end of the game. Optimized this script so objects that detonate
* only make an explosion when they call their detonate function. Modified the menu script to enable or disable the
* gyroscope depeninding on the setting. Added a few new game elements to capture mode to make it operational, capture mode
* has now been added to the main menu.
************************************************************/
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]


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

        private Camera m_Camera;
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
	
        private AudioSource m_AudioSource;

		private GameObject gatlingFirePoint;
		private GameObject gatlingGun;

		private GameObject grenadeFirePoint;
		private GameObject grenadeLauncher;

		private GameObject rifle;
		private GameObject rifleFirePoint;

		private GameObject laser;
		private GameObject laserFirePoint;

		private GameObject rocketLauncher;
		private GameObject rocketLaunchPoint;

		private GameObject guidedRocketLauncher;
		private GameObject guidedRocketLaunchPoint;

		private GameObject captureGun;
		private GameObject captureGunFirePoint;
		private bool gameOver = false;
		public int team = 1;

		private GameObject gameController;
		public AudioClip ammoPickup;
		private int curWeapon = 0;
		private int health = 100;
		private int lives = 1;
		private float lastSaveTime;
		private int[] currentWeapons;
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
			currentWeapons = new int[25];
			for (int i = 0; i < currentWeapons.Length; i++) {
				currentWeapons[i] = SafeLoadPref (i.ToString () + "_" + SceneManager.GetActiveScene ().name, 0);
			}
			//for (int i = 0; i < currentWeapons.Length; i++) {
			//	currentWeapons [i] = 0;
			//}
			LoadWeapons ();
			if (!SceneManager.GetActiveScene ().name.Equals ("SurvivalMode")) {
				LoadPrefs ();
			}
			if (SceneManager.GetActiveScene ().name.Equals ("CaptureMode")) {
				currentWeapons [6] = 1; //Always have the capture gun
				currentWeapons [0] = 0;
				curWeapon = 6;
			}
			else
			{
				currentWeapons [0] = 1; //Always have the rifle
			}

			lastSaveTime = Random.Range(5.00F, 30.0F);
			Invoke ("InitialDisplay", 0.5F);
			SwitchWeapons(curWeapon, true);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
        }

		/********************************************
        * SAVE PREFS
        * DESCRIPTION: Save all prefs related to the first
        * person controller.
        ********************************************/
		private void SavePrefs()
		{
			PlayerPrefs.SetFloat ("StartLocationX" + "_" + SceneManager.GetActiveScene ().name, transform.position.x);
			PlayerPrefs.SetFloat ("StartLocationY" + "_" + SceneManager.GetActiveScene ().name, transform.position.y);
			PlayerPrefs.SetFloat ("StartLocationZ" + "_" + SceneManager.GetActiveScene ().name, transform.position.z);
			PlayerPrefs.SetInt ("health" +  "_" + SceneManager.GetActiveScene ().name, health);
			PlayerPrefs.SetInt ("lives" +  "_" + SceneManager.GetActiveScene ().name, lives);
			PlayerPrefs.SetInt ("curWeapon" + "_" + SceneManager.GetActiveScene ().name, curWeapon);
			PlayerPrefs.Save ();
		}

		/************************************************ 
        * SAFE LOAD INT PREF
        * DESCRIPTION: Loads the desired preference, if that 
        * preference is not available it sets it to the
        * default value.
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
        * DESCRIPTION: Loads the desired preference, if that 
        * preference is not available it sets it to the
        * default value.
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
			float startX = SafeLoadPref ("StartLocationX", 0.0F);//float startX = PlayerPrefs.GetFloat ("StartLocationX");
			float startY = SafeLoadPref ("StartLocationY", 0.0F);//PlayerPrefs.GetFloat ("StartLocationY");
			float startZ = SafeLoadPref ("StartLocationZ", 0.0F);//PlayerPrefs.GetFloat ("StartLocationZ");
			curWeapon = SafeLoadPref ("curWeapon", 0);
			health = SafeLoadPref ("health", 100);
			lives = SafeLoadPref ("lives", 5);
			currentWeapons [6] = SafeLoadPref ("Weapon_6", 0);//PlayerPrefs.GetInt ("hasCaptureGun");
			currentWeapons[5] = SafeLoadPref ("Weapon_5", 0);//PlayerPrefs.GetInt("hasGuidedRocket");
			currentWeapons[4] = SafeLoadPref ("Weapon_4", 0);//PlayerPrefs.GetInt("hasRocket");
			currentWeapons[3] = SafeLoadPref ("Weapon_3", 0);//PlayerPrefs.GetInt ("hasLaser");
			currentWeapons[2] = SafeLoadPref ("Weapon_2", 0);//PlayerPrefs.GetInt ("hasMachineGun");
			currentWeapons[1] = SafeLoadPref ("Weapon_1", 0);//PlayerPrefs.GetInt ("hasGrenadeLauncher");
			if (startX == 0.0F && startY == 0.0F && startZ == 0.0F) {
			} else {
				transform.position = new Vector3(startX, startY, startZ + 5.0F);
			}
		}

		/********************************************
        * LOAD WEAPONS
        * DESCRIPTION: Loads all the weapons from the fps
        * game object.
        ********************************************/
		private void LoadWeapons()
		{
			rifle = GameObject.Find ("FirstPersonCharacter/Rifle");
			rifleFirePoint = GameObject.Find ("FirstPersonCharacter/Rifle/RifleFirePoint");
			grenadeLauncher = GameObject.Find ("FirstPersonCharacter/GrenadeLauncher");
			grenadeFirePoint = GameObject.Find ("FirstPersonCharacter/GrenadeLauncher/GrenadeFirePoint");
			laser = GameObject.Find ("FirstPersonCharacter/LaserGun");
			laserFirePoint = GameObject.Find ("FirstPersonCharacter/LaserGun/LaserFirePoint");
			rocketLauncher = GameObject.Find ("FirstPersonCharacter/RocketLauncher");
			rocketLaunchPoint = GameObject.Find ("FirstPersonCharacter/RocketLauncher/LaunchPoint");
			guidedRocketLauncher = GameObject.Find ("FirstPersonCharacter/GuidedRocketLauncher");
			guidedRocketLaunchPoint = GameObject.Find ("FirstPersonCharacter/GuidedRocketLauncher/LaunchPoint");
			captureGun = GameObject.Find ("FirstPersonCharacter/DroneCaptureRifle");
			captureGunFirePoint = GameObject.Find ("FirstPersonCharacter/DroneCaptureRifle/LaunchPoint");
			gatlingGun = GameObject.Find ("FirstPersonCharacter/MachineGun");
			gatlingFirePoint = GameObject.Find ("FirstPersonCharacter/MachineGun/MachineGunFirePoint");
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
        * DESCRIPTION: Increase the players health by
        * the input ammount.
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
        * DESCRIPTION: Deletes all the ammo levels for
        * each weapon.
        ********************************************/
		private void ClearWeaponSaves()
		{
			rifle.SetActive (true);
			rifleFirePoint.SendMessage ("DeleteAmmoLevel");
			rifle.SetActive (false);
			laser.SetActive (true);
			laserFirePoint.SendMessage ("DeleteAmmoLevel");
			laser.SetActive (false);
			grenadeLauncher.SetActive (true);
			grenadeFirePoint.SendMessage ("DeleteAmmoLevel");
			grenadeLauncher.SetActive (false);
			gatlingGun.SetActive (true);
			gatlingFirePoint.SendMessage ("DeleteAmmoLevel");
			gatlingGun.SetActive (false);
			guidedRocketLauncher.SetActive (true);
			guidedRocketLaunchPoint.SendMessage ("DeleteAmmoLevel");
			guidedRocketLauncher.SetActive (false);
			captureGun.SetActive (true);
			captureGunFirePoint.SendMessage ("DeleteAmmoLevel");
			captureGun.SetActive (false);
		}

		private void ClearFirstPersonController()
		{
			PlayerPrefs.DeleteKey("StartLocationX" + "_" + SceneManager.GetActiveScene ().name);
			PlayerPrefs.DeleteKey ("StartLocationY" + "_" + SceneManager.GetActiveScene ().name);
			PlayerPrefs.DeleteKey ("StartLocationZ" + "_" + SceneManager.GetActiveScene ().name);
			PlayerPrefs.DeleteKey ("health" +  "_" + SceneManager.GetActiveScene ().name);
			PlayerPrefs.DeleteKey ("lives" +  "_" + SceneManager.GetActiveScene ().name);
			PlayerPrefs.DeleteKey ("curWeapon" + "_" + SceneManager.GetActiveScene ().name);
			PlayerPrefs.DeleteKey ("Weapon_1" + "_" + SceneManager.GetActiveScene ().name);
			PlayerPrefs.DeleteKey ("Weapon_2" + "_" + SceneManager.GetActiveScene ().name);
			PlayerPrefs.DeleteKey ("Weapon_3" + "_" + SceneManager.GetActiveScene ().name);
			PlayerPrefs.DeleteKey ("Weapon_4" + "_" + SceneManager.GetActiveScene ().name);
			PlayerPrefs.DeleteKey ("Weapon_5" + "_" + SceneManager.GetActiveScene ().name);
			PlayerPrefs.DeleteKey ("Weapon_6" + "_" + SceneManager.GetActiveScene ().name);
		}

		/********************************************
        * REDUCE HEALTH
        * DESCRIPTION: Reduce the players health by
        * the input ammount.
        ********************************************/
		public void ReduceHealth(int amount)
		{
			if (health > 0) {
				health -= amount;
				if (health <= 0)
				{
					lives--;
					if (SceneManager.GetActiveScene ().name.Equals ("SurvivalMode")) {
						//SceneManager.LoadScene ("MainMenu");
						gameController.SendMessage ("GameOver");
						SwitchWeapons (-1, false); //deactivate weapons
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
						SwitchWeapons (-1, false); //deactivate weapons
						Invoke ("LoadMainMenu", 5.0F);
						gameOver = true;
						ClearWeaponSaves ();
						ClearFirstPersonController ();
						PlayerPrefs.Save ();
						//SceneManager.LoadScene ("MiniGame");
					}
				}
			}
			gameController.SendMessage ("UpdateHealth", health);
			gameController.SendMessage ("UpdateLives", lives); 
		}

		private void LoadMainMenu()
		{
			SceneManager.LoadScene ("MainMenu");
		}

		/********************************************
        * NEXT WEAPON HEALTH
        * DESCRIPTION: Switches the players weapon to the next
        * available one.
        ********************************************/
		private void NextWeapon()
		{
			bool foundNewWeapon = false;
			curWeapon++; //Move to the first new weapon
			for (int i = curWeapon; i < currentWeapons.Length; i++) {
				if (currentWeapons [i] == 1) {
					curWeapon = i;
					foundNewWeapon = true;
					break;
				}
			}
			//We have no other weapons, switch back to the rifle.
			if (foundNewWeapon == false) {
				curWeapon = 0;
			}
		}

		/********************************************
        * SWITCH WEAPONS
        * DESCRIPTION: Switches the players weapon object to the next
        * available one.
        ********************************************/
		public void SwitchWeapons(int newWeapon, bool save)
		{
			rifle.SetActive (false);
			gatlingGun.SetActive (false);
			grenadeLauncher.SetActive (false);
			laser.SetActive (false);
			rocketLauncher.SetActive (false);
			guidedRocketLauncher.SetActive (false);
			captureGun.SetActive (false);
			curWeapon = newWeapon;
			if (newWeapon == 0) {
				rifle.SetActive (true);
				rifleFirePoint.SendMessage ("WeaponActive");
			} else if (newWeapon == 1) {
				grenadeLauncher.SetActive (true);
				grenadeFirePoint.SendMessage ("WeaponActive");
			} else if (newWeapon == 2) {
				gatlingGun.SetActive (true);
				gatlingFirePoint.SendMessage ("WeaponActive");
			} else if (newWeapon == 3) {
				laser.SetActive (true);
				laserFirePoint.SendMessage ("WeaponActive");
			} else if (newWeapon == 4) {
				rocketLauncher.SetActive (true);
				rocketLaunchPoint.SendMessage ("WeaponActive");
			} else if (newWeapon == 5) {
				guidedRocketLauncher.SetActive (true);
				guidedRocketLaunchPoint.SendMessage ("WeaponActive");
			} else if (newWeapon == 6) {
				captureGun.SetActive (true);
				captureGunFirePoint.SendMessage ("WeaponActive");
			}
		}

        // Update is called once per frame
        private void Update()
        {
			if ((Input.GetKeyDown ("b") || CrossPlatformInputManager.GetButtonDown("Fire2")) && gameOver == false) {
				NextWeapon ();
				SwitchWeapons(curWeapon, true);
				//DisplayStatusText();
			}
			if (gameOver == false) {
				RotateView ();
			}
            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
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

            PlayFootStepAudio();
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
			if (PlayerPrefs.GetInt("UseGyro") == 1)// && Input.gyro.enabled)
			{
				float rotateYSpeed = 200.0F * PlayerPrefs.GetFloat("lookYSensitivity");
				float rotateXSpeed = 200.0F * PlayerPrefs.GetFloat("lookXSensitivity");
			   pitch = Input.gyro.rotationRateUnbiased.y;
			    yaw = Input.gyro.rotationRateUnbiased.x;

				if ((m_Camera.transform.eulerAngles.x < 400 && m_Camera.transform.eulerAngles.x + pitch > 280) ||
					(m_Camera.transform.eulerAngles.x > -1 && m_Camera.transform.eulerAngles.x + pitch < 60))
				{
					m_Camera.transform.localEulerAngles += new Vector3(pitch, m_Camera.transform.localEulerAngles.y,
						m_Camera.transform.localEulerAngles.z);
				}
				transform.localEulerAngles += new Vector3(0, yaw, 0);
			}
			else
			{
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
			                 m_Camera.transform.localEulerAngles += new Vector3(pitch, m_Camera.transform.localEulerAngles.y,
				             m_Camera.transform.localEulerAngles.z);
			             }
			             transform.localEulerAngles += new Vector3(0, yaw, 0);
				         break; //Don't need to continue in this loop
			       }
			    }
			}
#endif
        }

		int PickupWeapon(GameObject weapon, GameObject attachedWeapon, GameObject attachedFirePoint, int hasWeapon, string weaponPref, string weaponMessage, string ammoMessage)
		{
			if (hasWeapon == 0) {
				hasWeapon = 1;
				Destroy (weapon);
				m_AudioSource.PlayOneShot (ammoPickup);
				gameController.SendMessage("SetTempText", weaponMessage); 
			} else {
				bool isActive = attachedWeapon.activeSelf;
				attachedWeapon.SetActive (true);
				m_AudioSource.PlayOneShot (ammoPickup);
				attachedFirePoint.SendMessage ("CollectAmmo", weapon);
				attachedWeapon.SetActive (isActive);
				SwitchWeapons (curWeapon, true);
				gameController.SendMessage("ammoMessage", weaponMessage); 
			}
			return hasWeapon;
		}

		/***********************************************
        * PICKUP AMMO
        * DESCRIPTION: Handles picking up an ammo item.
        ***********************************************/
		void PickupAmmo(GameObject inputPack, GameObject weapon, GameObject firePoint, string message)
		{
			bool isActive = weapon.activeSelf;
			weapon.SetActive (true);
			m_AudioSource.PlayOneShot (ammoPickup);
			firePoint.SendMessage ("CollectAmmo", inputPack);
			weapon.SetActive (isActive);
			SwitchWeapons (curWeapon, true);
			gameController.SendMessage("SetTempText", message); 
		}
		void OnTriggerEnter(Collider other) {

			if (other.gameObject.CompareTag ("RifleAmmo")) {
				PickupAmmo(other.gameObject, rifle, rifleFirePoint, "Picked up rifle ammo");
			}
			if (other.gameObject.CompareTag ("MachineGunAmmo")) {
				PickupAmmo(other.gameObject, gatlingGun, gatlingFirePoint, "Picked up machine gun ammo");
			}
			if (other.gameObject.CompareTag ("GrenadeAmmo")) {
				PickupAmmo(other.gameObject, grenadeLauncher, grenadeFirePoint, "Picked up grenades");
			}
			if (other.gameObject.CompareTag ("RocketAmmo")) {
				PickupAmmo(other.gameObject, rocketLauncher, rocketLaunchPoint, "Picked up rockets");
			}
			if (other.gameObject.CompareTag ("GuidedRocketAmmo")) {
				PickupAmmo(other.gameObject, guidedRocketLauncher, guidedRocketLaunchPoint, "Picked up guided rockets");
			}
			if (other.gameObject.CompareTag ("CaptureGunAmmo")) {
				PickupAmmo(other.gameObject, captureGun, captureGunFirePoint, "Picked up capture darts");
			}
			if (other.gameObject.CompareTag ("LaserBattery")) {
				PickupAmmo(other.gameObject, laser, laserFirePoint, "Picked up laser battery");
			}
			if (other.gameObject.CompareTag ("LaserGunPickup")) {
				if (currentWeapons[3] == 0) {
					currentWeapons[3] = 1;
					Destroy (other.gameObject);
					gameController.SendMessage("SetTempText", "Picked up laser gun"); 
					PlayerPrefs.SetInt ("hasLaser" + "_" + SceneManager.GetActiveScene ().name, currentWeapons[3]);
				} else {
					bool isActive = laser.activeSelf;
					laser.SetActive (true);
					laserFirePoint.SendMessage ("CollectAmmo", other.gameObject);
					laser.SetActive (isActive);
					SwitchWeapons (curWeapon, true);
					gameController.SendMessage("SetTempText", "Picked up laser gun"); 
				}
			}
			if (other.gameObject.CompareTag ("GuidedRocketLauncherPickup")) {
				currentWeapons[5] = PickupWeapon(other.gameObject, guidedRocketLauncher, guidedRocketLaunchPoint, currentWeapons[5], "hasGuidedRocket", "Picked up guided rocket launcher", "Picked up guided rockets");

			}
			if (other.gameObject.CompareTag ("RocketLauncherPickup")) {
				currentWeapons[4] = PickupWeapon(other.gameObject, rocketLauncher, rocketLaunchPoint, currentWeapons[4], "hasRocket", "Picked up rocket launcher", "Picked up rockets");

			}
			if (other.gameObject.CompareTag ("MachineGunPickup")) {
				currentWeapons[2] = PickupWeapon(other.gameObject, gatlingGun, gatlingFirePoint, currentWeapons[2], "hasMachineGun", "Picked up machine gun", "Picked up machine gun ammo");

			}
			if (other.gameObject.CompareTag ("GrenadeLauncherPickup")) {
				currentWeapons[1] = PickupWeapon(other.gameObject, grenadeLauncher, grenadeFirePoint, currentWeapons[1], "hasGrenadeLauncher", "Picked up grenade launcher", "Picked up grenades");
			}
			if (other.gameObject.CompareTag ("CaptureGunPickup")) {
				currentWeapons[6] = PickupWeapon(other.gameObject, captureGun, captureGunFirePoint, currentWeapons[6], "hasCaptureGun", "Picked up capture gun", "Picked up capture darts");
			}
			if (other.gameObject.CompareTag ("Explosion")) {
				ReduceHealth(5);
				Destroy(other.gameObject);
			}
			if (other.gameObject.CompareTag ("HealthPack")) {
				if (health < 100)
				{
					Destroy(other.gameObject);
					health += 10;
					if (health > 100)
					{
						health = 100;
					}
					ReduceHealth (0);
					GetComponent<AudioSource>().PlayOneShot(ammoPickup, 3.0F);
					gameController.SendMessage("SetTempText", "Picked up health"); 
				}
			}
			if (other.gameObject.CompareTag ("LevelMarker") && lastSaveTime < Time.time) {
				laser.SetActive (true);
				laserFirePoint.SendMessage ("SaveAmmo");
				laser.SetActive (false);
				grenadeLauncher.SetActive (true);
				grenadeFirePoint.SendMessage ("SaveAmmo");
				grenadeLauncher.SetActive (false);
				rifle.SetActive (false);
				gatlingGun.SetActive (true);
				gatlingFirePoint.SendMessage ("SaveAmmo");
				gatlingGun.SetActive (false);
				rifle.SetActive (true);
				rifleFirePoint.SendMessage ("SaveAmmo");
				rifle.SetActive (false);
				guidedRocketLauncher.SetActive (true);
				guidedRocketLaunchPoint.SendMessage ("SaveAmmo");
				guidedRocketLauncher.SetActive (false);
				captureGun.SetActive (true);
				captureGunFirePoint.SendMessage ("SaveAmmo");
				captureGun.SetActive (false);
				//Save weapons
				for (int i = 0; i < currentWeapons.Length; i++) {
					PlayerPrefs.SetInt ("Weapon_" + i.ToString() + "_" + SceneManager.GetActiveScene ().name, currentWeapons[i]);
				}
				SavePrefs ();
				SwitchWeapons (curWeapon, false);
				gameController.SendMessage ("SaveGame");
				gameController.SendMessage("SetTempText", "Game Saved");

				lastSaveTime = Time.time + 10; //Prevents too many rapid save operations at a time.
				
			}
		}

		void OnCollisionEnter(Collision other)
		{
			if (other.gameObject.tag.Contains ("Rocket")) {
#if !MOBILE_INPUT
				ReduceHealth(20);
#else
				ReduceHealth(10);
#endif
				Destroy(other.gameObject);
			}
			if (other.gameObject.tag.Contains ("Explosion")) {
				ReduceHealth(5);
				Destroy(other.gameObject);
			}
			if (other.gameObject.tag.Contains ("RifleBullet")) {
#if !MOBILE_INPUT
				ReduceHealth(5);
#else
				ReduceHealth(2);
#endif
				Destroy(other.gameObject);
			}
			if (other.gameObject.tag.Contains ("Shell")) {
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
