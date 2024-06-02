using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

[System.Serializable]
public class AmmoInventoryEntry
{
    public int ammoType;
    public int amount = 0;
}

public class Controller : MonoBehaviour
{
    public static Controller Instance { get; protected set; }

    public OVRCameraRig CameraRig;


    public OVRInput.Controller LeftController;
    public OVRInput.Controller RightController;

    public GameObject[] startingWeapons;
  

    [Header("Control Settings")]
    public float PlayerSpeed = 5.0f;
    public float RunningSpeed = 7.0f;
    public float JumpSpeed = 5.0f;

    [Header("Audio")]
    public RandomPlayer FootstepPlayer;
    public AudioClip JumpingAudioClip;
    public AudioClip LandingAudioClip;

    private float m_VerticalSpeed = 0.0f;
    private bool m_IsPaused = false;
    private int m_CurrentWeapon;

    private OVRInput.Controller m_CharacterController;
    private bool m_Grounded;
    private float m_GroundedTimer;
    private float m_SpeedAtJump = 0.0f;

    private List<Weapon> m_Weapons = new List<Weapon>();
    private Dictionary<int, int> m_AmmoInventory = new Dictionary<int, int>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        m_IsPaused = false;
        m_Grounded = true;

        m_CharacterController = GetComponent<OVRInput.Controller>();

        
      
        m_CurrentWeapon = -1;
        ChangeWeapon(0);

    
    }

    void Update()
    {
        bool wasGrounded = m_Grounded;
        bool loosedGrounding = false;

        
        if (!m_IsPaused)
        {
            if (m_Grounded && OVRInput.GetDown(OVRInput.Button.One))
            {
                m_VerticalSpeed = JumpSpeed;
                m_Grounded = false;
                loosedGrounding = true;
                FootstepPlayer.PlayClip(JumpingAudioClip, 0.8f, 1.1f);
            }

            bool running = m_Weapons[m_CurrentWeapon].CurrentState == Weapon.WeaponState.Idle && OVRInput.Get(OVRInput.Button.PrimaryThumbstick);
            float actualSpeed = running ? RunningSpeed : PlayerSpeed;

            if (loosedGrounding)
            {
                m_SpeedAtJump = actualSpeed;
            }

       

            Vector2 secondaryAxis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            transform.Rotate(0, secondaryAxis.x * 45.0f * Time.deltaTime, 0);

            m_Weapons[m_CurrentWeapon].triggerDown = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger);

           
            if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick))
            {
                ChangeWeapon(m_CurrentWeapon + 1);
            }
        }

        m_VerticalSpeed = m_VerticalSpeed - 10.0f * Time.deltaTime;
        if (m_VerticalSpeed < -10.0f)
            m_VerticalSpeed = -10.0f;
      
        if (!wasGrounded && m_Grounded)
        {
            FootstepPlayer.PlayClip(LandingAudioClip, 0.8f, 1.1f);
        }
    }

    

    void ChangeWeapon(int number)
    {
        if (m_CurrentWeapon != -1)
        {
            m_Weapons[m_CurrentWeapon].PutAway();
            m_Weapons[m_CurrentWeapon].gameObject.SetActive(false);
        }

        m_CurrentWeapon = number;

        if (m_CurrentWeapon < 0)
            m_CurrentWeapon = m_Weapons.Count - 1;
        else if (m_CurrentWeapon >= m_Weapons.Count)
            m_CurrentWeapon = 0;

        m_Weapons[m_CurrentWeapon].gameObject.SetActive(true);
        m_Weapons[m_CurrentWeapon].Selected();
    }


    public void PlayFootstep()
    {
        FootstepPlayer.PlayRandom();
    }
}
