using System;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public static Controller Instance { get; protected set; }

    public OVRCameraRig CameraRig;
    private Weapon _weapon;
    public Controller(Weapon weapon)
    {
        _weapon = weapon;
    }


    public OVRInput.Controller LeftController;
    public OVRInput.Controller RightController;
  

    [Header("Control Settings")]
    public float PlayerSpeed = 5.0f;
    public float RunningSpeed = 7.0f;
    public float JumpSpeed = 5.0f;

    [Header("Audio")]
    public RandomPlayer FootstepPlayer;
    public AudioClip JumpingAudioClip;
    public AudioClip LandingAudioClip;

   
    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        {
            _weapon.Shoot();
        }
    }




    public void PlayFootstep()
    {
        FootstepPlayer.PlayRandom();
    }
}
