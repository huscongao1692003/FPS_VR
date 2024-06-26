﻿using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Weapon : MonoBehaviour
{
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject bulletPosision;
    [SerializeField] private float shotDelay = 0.2f;
    [Range(0,3000),SerializeField] private float bulletSpeed;
    [Space,SerializeField] private AudioSource audioSource;

    private float lastShot;

    public void Shoot()
    {

        if (lastShot > Time.time) return;
        lastShot = Time.time + shotDelay;
        var bulletPrefab = Instantiate(bullet,bulletPosision.transform.position, bulletPosision.transform.rotation);
        var bulletRB = bulletPrefab.GetComponent<Rigidbody>();
        var direction = bulletPrefab.transform.TransformDirection(Vector3.forward);
        bulletRB.AddForce(direction*bulletSpeed);
        Destroy(bulletPrefab, 5f);

    }
    private void Update()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        {
            Shoot();
            GunShotAudio();
        }
    }
    private void GunShotAudio()
    {
        var random = Random.Range(0.8f, 1.2f);
        audioSource.pitch = random;
        audioSource.Play();
    }




























    //static RaycastHit[] s_HitInfoBuffer = new RaycastHit[8];

    //public enum TriggerType
    //{
    //    Auto,
    //    Manual
    //}

    //public enum WeaponType
    //{
    //    Raycast,
    //    Projectile
    //}



    //[System.Serializable]
    //public class AdvancedSettings
    //{
    //    public float spreadAngle = 0.0f;
    //    public int projectilePerShot = 1;
    //    public float screenShakeMultiplier = 1.0f;
    //}

    //public TriggerType triggerType = TriggerType.Manual;
    //public WeaponType weaponType = WeaponType.Raycast;
    //public float fireRate = 0.5f;
    //public int clipSize = 4;
    //public float damage = 1.0f;
    //public int ammoType = -1;
    //public Transform EndPoint;
    //public AdvancedSettings advancedSettings;

    //[Header("Audio Clips")]
    //public AudioClip FireAudioClip;
    //public AudioClip ReloadAudioClip;

    //[Header("Visual Settings")]
    //public LineRenderer PrefabRayTrail;

    //private bool m_TriggerDown;
    //private bool m_ShotDone;
    //private int m_ClipContent;
    //private Animator m_Animator;
    //private AudioSource m_Source;
    //private float m_ShotTimer = -1.0f;
    //private List<ActiveTrail> m_ActiveTrails = new List<ActiveTrail>();
    //private Queue<GameObject> m_ProjectilePool = new Queue<GameObject>();

    //private OVRGrabbable m_Grabbable;

    //class ActiveTrail
    //{
    //    public LineRenderer renderer;
    //    public Vector3 direction;
    //    public float remainingTime;
    //}

    //void Awake()
    //{
    //    m_Animator = GetComponentInChildren<Animator>();
    //    m_Source = GetComponentInChildren<AudioSource>();
    //    m_ClipContent = clipSize;

    //    m_Grabbable = GetComponent<OVRGrabbable>();

    //    if (PrefabRayTrail != null)
    //    {
    //        const int trailPoolSize = 16;
    //        PoolSystem.Instance.InitPool(PrefabRayTrail, trailPoolSize);
    //    }

    //}

    //public void PutAway()
    //{
    //    m_Animator.WriteDefaultValues();
    //    foreach (var activeTrail in m_ActiveTrails)
    //    {
    //        activeTrail.renderer.gameObject.SetActive(false);
    //    }
    //    m_ActiveTrails.Clear();
    //}

    //public void Selected()
    //{
       
    //    m_ShotDone = false;

    //    WeaponInfoUI.Instance.UpdateWeaponName(this);
    //    WeaponInfoUI.Instance.UpdateAmmoAmount(1000);

    //    m_Animator.SetTrigger("selected");
    //}

    //public void Fire()
    //{
    //    if (!m_Grabbable.isGrabbed || m_ShotTimer > 0 || m_ClipContent == 0)
    //        return;

    //    m_ClipContent -= 1;
    //    m_ShotTimer = fireRate;
    //    m_Animator.SetTrigger("fire");
    //    m_Source.pitch = Random.Range(0.7f, 1.0f);
    //    m_Source.PlayOneShot(FireAudioClip);

    //        for (int i = 0; i < advancedSettings.projectilePerShot; ++i)
    //        {
    //            RaycastShot();
    //        }
        
     
    //}

    //void RaycastShot()
    //{
    //    float spreadRatio = advancedSettings.spreadAngle / OVRManager.instance.GetComponent<Camera>().fieldOfView;
    //    Vector2 spread = spreadRatio * Random.insideUnitCircle;
    //    RaycastHit hit;
    //    Ray r = new Ray(EndPoint.position, EndPoint.forward + new Vector3(spread.x, spread.y, 0));
    //    Vector3 hitPosition = r.origin + r.direction * 200.0f;

    //    if (Physics.Raycast(r, out hit, 1000.0f, ~(1 << 9), QueryTriggerInteraction.Ignore))
    //    {
    //        Renderer renderer = hit.collider.GetComponentInChildren<Renderer>();
    //        ImpactManager.Instance.PlayImpact(hit.point, hit.normal, renderer == null ? null : renderer.sharedMaterial);

    //        if (hit.distance > 5.0f)
    //            hitPosition = hit.point;

    //        if (hit.collider.gameObject.layer == 10)
    //        {
    //            Target target = hit.collider.gameObject.GetComponent<Target>();
    //            target.Got(damage);
    //        }
    //    }

    //    if (PrefabRayTrail != null)
    //    {
    //        var pos = new Vector3[] { EndPoint.position, hitPosition };
    //        var trail = PoolSystem.Instance.GetInstance<LineRenderer>(PrefabRayTrail);
    //        trail.gameObject.SetActive(true);
    //        trail.SetPositions(pos);
    //        m_ActiveTrails.Add(new ActiveTrail()
    //        {
    //            remainingTime = 0.3f,
    //            direction = (pos[1] - pos[0]).normalized,
    //            renderer = trail
    //        });
    //    }
    //}

  
    //void Update()
    //{
    //    if (m_ShotTimer >= 0)
    //    {
    //        m_ShotTimer -= Time.deltaTime;
    //    }

    //    if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
    //    {
    //        if (!m_ShotDone)
    //        {
    //            Fire();
    //            m_ShotDone = true;
    //        }
    //    }
    //}

    //public void ReturnProjectileToPool(GameObject projectile)
    //{
    //    projectile.SetActive(false);
    //    m_ProjectilePool.Enqueue(projectile);
    //}
}
