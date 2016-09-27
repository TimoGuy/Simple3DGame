/*************************************************************
* SPHERE BREAKER
* DESCRIPTION: Sphere breaker is a simple 3D unity game. In the
* game you move through a simple course in an "All Range Mode"
* You can go whereever you like. Enemy spheres move in at random
* and begin to attack, you must destroy as many of them as you
* can before you are overwhelmed and destroyed.
* Author: Jonathan L Clark
* Date: 3/8/2016
* Update: 9/15/2016, Reved Sphere Crusher to version 1.3.0 and removed the Beta tag
* as this game is now in release. Removed the fire buttons, replaced with touch sensitive
* fire area of screen.
* Update: 9/16/2016, Reved Sphere Crusher to version 1.3.1 Modified fire button to require
* user to let off to fire. Modified the joystick to have a larger movement range. Added the
* new fire area to the laser weapon, removed the 'jump' button, tapping high on the
* screen now allows the character to jump. Removed several items that were throwing 
* warnings. Pulled all weapon information into Dicationary structures, now using an enum
* to access which weapon is which instead of integers. This will improve code clarity and make
* code simplification easier.
* Update: 9/17/2016, Reved Sphere Crusher to version 1.3.2 Fixed an issue where the
* jump button was not working on Windows. Replaced large sections of repeat code with loops
* reducing the code by about 200 lines. Now calling 'WeaponStrings' weapon type. Removed
* all unused params from pickup weapon. Cleaned up the code that handles picking up
* ammo. Removed all tags from ammo, we now detect an ammo pickup by name. Removed tags
* from all weapon pickups, now using the names to identify each weapon. GuidedRocketLauncher
* was changed to GuidedMissileLauncher to prevent name matching issues in the future. Modified
* the health pack to also use the name instead of the tag. Removed the tag from the level marker.
* Modified all bullets to be identified by name (not tag). Removed the tag from all crates, now
* accessing with just the name.
* Update: 9/19/2016, Reved Sphere Crusher to version 1.3.3. Removed the remaining tags from the game
* except for tags on allied and enemy units (these must be handled seperatly). Fixed an issue where
* guided missiles were not doing any damage to targets. Fixed an issue where jump wasn't working
* on mobile platforms. Fixed an issue where new game objects were spawning on exit.
* Update: 9/21/2016, Reved Sphere Crusher to version 1.3.4 Fixed an issue where explosions were
* not affecting the player. Added the new plasma rifle and plasma explosion. Added the barrel 
* flash special effect to the machine gun and rifle. Fixed an issue where there was no explosion
* assigned to one of the battle cruiser's point defenses. Modified the guided rocket to only explode
* on contact, not just when it is close. Added barrel flashes to each weapon type. Fixed an issue
* where WeaponType.None was being accessed. Increased rocket rate of fire. Added code that allows
* the rocket to randomly select a target.
* Update: 9/22/2016, Modified the bullet explosion to be smaller with less particals and a
* more accurate special effect. Modified the machine gun round and rifle round to no longer
* be affected by gravity as these objects would appear to be like that in real life. Fixed an issue
* where explosions were being 'eaten' by the target game object. Preformed a minor refactor on the
* DestroyByCollision script. Now we detect implicitly weather an object is to be spawned based on
* if that object is null or not. This eliminates the need for checkboxes in the script. Added sound
* to the plasma cannon. Added plasma cannon ammo packs.
* Update: 9/23/2016, Reved to version 1.3.6, Preformed a major refactor of the guided rocket
* script. Fixed an issue where the laser was not making any sound when firing. Added the plasma launcher
* gun pickup. Added the plasma gun pickup item to the game SurvivalMode in a hidden place. Combinde
* the Laser Script into the Weapon script. Now only one weapon script is require for all weapons. There
* is no longer a specal case for lasers. Fixed a few issues with the object identification system (HUD). Also
* added the plasma launcher to the HUD.
* Update: 9/24/2016, Reved to version 1.3.7, Increased the speed of the drones and first person controller
* to be more responsive. Decreasesed the rifle ROF slightly but increased how powerful each shot is. Now a single
* rifle shot can destroy a drone. Added a new unit to the game, the HeavyDrone. This unit fills the gap between the
* battlecruiser and the tactical drone. Added code to launch the heavy drone regularly. Reduced the power of the
* explosions. Fixed an issue where ammo counts were staying the same. Added the heavy drone to the HUD. Doubled the 
* health pack value.
* Update: 9/26-9/27/2016, Reved to version 1.3.8, Started adding the carrier unit. This is the heaviest unit in the game
* capable of untold damage. Improved the rocket launch script to handle various modes. Finished the carrier AI unit.
* Fixed an issue where the battlecruiser, heavy drone and carrier were are not affected by the laser. Added more intellegance
* to the guided rockets so they don't get stuck circiling their target. Modified the game controller to launch the battle cruiser
* on strictly a random basis. Added the evil guided rocket to the target list as well as the guided rocket. Now rockets can hit each other.
* Added the carrier to the game controller. Decreased rifle pack value. Increased the capture gun pack value. Fixed an issue
* where the enemy point defenses were not working. Added a new script that will eventually delete ammo packs after a designated
* time frame. Disabled the 'FallAgain' script code.
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

		public enum WeaponType { None=-1, Rifle=0, GrenadeLauncher=1, MachineGun=2, Laser=3, RocketLauncher=4, GuidedMissileLauncher=5, CaptureGun=6, PlasmaLauncher=7};
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

		private Dictionary<WeaponType, GameObject> weapons = new Dictionary<WeaponType, GameObject> ();
		private Dictionary<WeaponType, GameObject> firePoints = new Dictionary<WeaponType, GameObject> ();
	
        private AudioSource m_AudioSource;
		private bool gameOver = false;
		public int team = 1;

		private GameObject gameController;
		private int curWeapon = 0;
		private int health = 100;
		private int lives = 1;
		private bool lives_updated = false;
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
			LoadWeapons ();
			if (!SceneManager.GetActiveScene ().name.Equals ("SurvivalMode")) {
				LoadPrefs ();
			}
			if (SceneManager.GetActiveScene ().name.Equals ("CaptureMode")) {
				currentWeapons [(int) WeaponType.CaptureGun] = 1; //Always have the capture gun
				currentWeapons [(int) WeaponType.Rifle] = 0;
				curWeapon = 6;
			}
			else
			{
				currentWeapons [(int)WeaponType.Rifle] = 1; //Always have the rifle
			}

			lastSaveTime = Random.Range(5.00F, 30.0F);
			Invoke ("InitialDisplay", 0.5F);
			SwitchWeapons((WeaponType)curWeapon, true, false);
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
			float startX = SafeLoadPref ("StartLocationX", 0.0F);
			float startY = SafeLoadPref ("StartLocationY", 0.0F);
			float startZ = SafeLoadPref ("StartLocationZ", 0.0F);
			curWeapon = SafeLoadPref ("curWeapon", 0);
			health = SafeLoadPref ("health", 100);
			lives = SafeLoadPref ("lives", 5);
			for (int i = 0; i < weapons.Count; i++) {
				currentWeapons [i] = SafeLoadPref ("Weapon_" + i.ToString(), 0);
			}
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
			//weapons.Add (WeaponType.None, null);
			//firePoints.Add (WeaponType.None, null);
			weapons.Add (WeaponType.Rifle, GameObject.Find ("FirstPersonCharacter/Rifle"));
			firePoints.Add(WeaponType.Rifle, GameObject.Find ("FirstPersonCharacter/Rifle/RifleFirePoint"));
			weapons.Add(WeaponType.GrenadeLauncher, GameObject.Find ("FirstPersonCharacter/GrenadeLauncher"));
			firePoints.Add(WeaponType.GrenadeLauncher, GameObject.Find ("FirstPersonCharacter/GrenadeLauncher/GrenadeFirePoint"));
			weapons.Add(WeaponType.Laser, GameObject.Find ("FirstPersonCharacter/LaserGun"));
			firePoints.Add(WeaponType.Laser, GameObject.Find ("FirstPersonCharacter/LaserGun/LaserFirePoint"));
			weapons.Add(WeaponType.RocketLauncher, GameObject.Find ("FirstPersonCharacter/RocketLauncher"));
			firePoints.Add(WeaponType.RocketLauncher, GameObject.Find ("FirstPersonCharacter/RocketLauncher/LaunchPoint"));
			weapons.Add(WeaponType.GuidedMissileLauncher, GameObject.Find ("FirstPersonCharacter/GuidedRocketLauncher"));
			firePoints.Add(WeaponType.GuidedMissileLauncher, GameObject.Find ("FirstPersonCharacter/GuidedRocketLauncher/LaunchPoint"));
			weapons.Add(WeaponType.CaptureGun, GameObject.Find ("FirstPersonCharacter/DroneCaptureRifle"));
			firePoints.Add(WeaponType.CaptureGun, GameObject.Find ("FirstPersonCharacter/DroneCaptureRifle/LaunchPoint"));
			weapons.Add(WeaponType.MachineGun, GameObject.Find ("FirstPersonCharacter/MachineGun"));
			firePoints.Add(WeaponType.MachineGun, GameObject.Find ("FirstPersonCharacter/MachineGun/MachineGunFirePoint"));
			weapons.Add(WeaponType.PlasmaLauncher, GameObject.Find("FirstPersonCharacter/PlasmaLauncher"));
			firePoints.Add(WeaponType.PlasmaLauncher, GameObject.Find("FirstPersonCharacter/PlasmaLauncher/PlasmaFirePoint"));
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
				weapons [weapon].SetActive (true);
				firePoints[weapon].SendMessage ("DeleteAmmoLevel");
				weapons [weapon].SetActive (false);
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
				weapons [weapon].SetActive (true);
				firePoints[weapon].SendMessage ("ResetAmmo");
				weapons [weapon].SetActive (false);
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
						//SwitchWeapons (WeaponType.None, false, false); //deactivate weapons
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
						//SwitchWeapons (WeaponType.None, false, false); //deactivate weapons
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
        * DESCRIPTION: Switches the players weapon
        ********************************************/
		public void SwitchWeapons(WeaponType newWeapon, bool save, bool playReload)
		{
			//Deactive all weapons
			foreach (WeaponType weapon in weapons.Keys) {
				weapons [weapon].SetActive (false);
			}

			curWeapon = (int)newWeapon;

			weapons [newWeapon].SetActive (true);
			firePoints[newWeapon].SendMessage ("WeaponActive");
			if (playReload) {
				firePoints[newWeapon].SendMessage ("PlayPickupSound");
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
				SwitchWeapons((WeaponType)curWeapon, true, false);
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
			                 m_Camera.transform.localEulerAngles += new Vector3(pitch, m_Camera.transform.localEulerAngles.y,
				             m_Camera.transform.localEulerAngles.z);
			             }
			             transform.localEulerAngles += new Vector3(0, yaw, 0);
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
			if (currentWeapons[(int)weaponType] == 0) {
				currentWeapons[(int)weaponType] = 1;
				Destroy (weapon);
				gameController.SendMessage("SetTempText", weaponMessage); 
				//SwitchWeapons ((WeaponType)curWeapon, true, true);
			} else {
				bool isActive = weapons [weaponType];
				weapons [weaponType].SetActive (true);
				firePoints[weaponType].SendMessage ("CollectAmmo", weapon);
				weapons [weaponType].SetActive (isActive);
				//SwitchWeapons ((WeaponType)curWeapon, true, true);
				gameController.SendMessage("ammoMessage", weaponMessage); 
			}
			SwitchWeapons ((WeaponType)curWeapon, true, true);
		}

		/***********************************************
        * PICKUP AMMO
        ***********************************************/
		void PickupAmmo(GameObject inputPack, WeaponType weaponType, string message)
		{
			bool isActive = weapons [weaponType].activeSelf;
			weapons [weaponType].SetActive (true);
			firePoints[weaponType].SendMessage ("CollectAmmo", inputPack);
			weapons [weaponType].SetActive (isActive);
			SwitchWeapons ((WeaponType)curWeapon, true, true);
			gameController.SendMessage("SetTempText", message); 

		}

		void OnTriggerEnter(Collider other) {
			//Is it a weapon or ammo?
			foreach (WeaponType weaponType in weapons.Keys) {
				if (other.gameObject.name.Contains (weaponType.ToString () + "AmmoCollect")) {
					PickupAmmo (other.gameObject, weaponType, "Picked up " + weaponType.ToString ().ToLower () + " ammo");
					break;
				} else if (other.gameObject.name.Contains (weaponType.ToString () + "PickupCollect") && !other.gameObject.name.Contains("Laser")) {
					PickupWeapon(other.gameObject, weaponType, "Picked up " + weaponType.ToString(), "Picked up " + weaponType.ToString ().ToLower () + " ammo");
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
			if (other.gameObject.name.Contains ("LaserPickup")) {
				if (currentWeapons[(int)WeaponType.Laser] == 0) {
					currentWeapons[(int)WeaponType.Laser] = 1;
					Destroy (other.gameObject);
					gameController.SendMessage("SetTempText", "Picked up laser gun"); 
					PlayerPrefs.SetInt ("hasLaser" + "_" + SceneManager.GetActiveScene ().name, currentWeapons[3]);
				} else {
					bool isActive = weapons [WeaponType.Laser].activeSelf;
					weapons [WeaponType.Laser].SetActive (true);
					firePoints[WeaponType.Laser].SendMessage ("CollectAmmo", other.gameObject);
					weapons [WeaponType.Laser].SetActive (isActive);
					SwitchWeapons ((WeaponType)curWeapon, true, true);
					gameController.SendMessage("SetTempText", "Picked up laser gun"); 
				}
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
			if (other.gameObject.name.Contains ("LevelMarker") && lastSaveTime < Time.time) {
				foreach (WeaponType weapon in weapons.Keys) {
					weapons [weapon].SetActive (true);
					firePoints[weapon].SendMessage ("SaveAmmo");
					weapons [weapon].SetActive (false);
				}

				//Save weapons
				for (int i = 0; i < currentWeapons.Length; i++) {
					PlayerPrefs.SetInt ("Weapon_" + i.ToString() + "_" + SceneManager.GetActiveScene ().name, currentWeapons[i]);
				}
				SavePrefs ();
				SwitchWeapons ((WeaponType)curWeapon, false, false);
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
