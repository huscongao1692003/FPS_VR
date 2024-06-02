using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class Weapon : MonoBehaviour
{
    static RaycastHit[] s_HitInfoBuffer = new RaycastHit[8];

    public enum TriggerType
    {
        Auto,
        Manual
    }

    public enum WeaponType
    {
        Raycast,
        Projectile
    }

    public enum WeaponState
    {
        Idle,
        Firing,
        Reloading
    }

    [System.Serializable]
    public class AdvancedSettings
    {
        public float spreadAngle = 0.0f;
        public int projectilePerShot = 1;
        public float screenShakeMultiplier = 1.0f;
    }

    public TriggerType triggerType = TriggerType.Manual;
    public WeaponType weaponType = WeaponType.Raycast;
    public float fireRate = 0.5f;
    public float reloadTime = 2.0f;
    public int clipSize = 4;
    public float damage = 1.0f;

    public int ammoType = -1;

    public GameObject projectilePrefab;
    public float projectileLaunchForce = 200.0f;

    public Transform EndPoint;

    public AdvancedSettings advancedSettings;


    [Header("Audio Clips")]
    public AudioClip FireAudioClip;
    public AudioClip ReloadAudioClip;

    [Header("Visual Settings")]
    public LineRenderer PrefabRayTrail;

  

    public bool triggerDown
    {
        get { return m_TriggerDown; }
        set
        {
            m_TriggerDown = value;
            if (!m_TriggerDown) m_ShotDone = false;
        }
    }

    public WeaponState CurrentState => m_CurrentState;
    public int ClipContent => m_ClipContent;
    public Controller Owner => m_Owner;

    Controller m_Owner;

    Animator m_Animator;
    WeaponState m_CurrentState;
    bool m_ShotDone;
    float m_ShotTimer = -1.0f;
    bool m_TriggerDown;
    int m_ClipContent;

    AudioSource m_Source;

    Vector3 m_ConvertedMuzzlePos;

    class ActiveTrail
    {
        public LineRenderer renderer;
        public Vector3 direction;
        public float remainingTime;
    }

    List<ActiveTrail> m_ActiveTrails = new List<ActiveTrail>();

    Queue<GameObject> m_ProjectilePool = new Queue<GameObject>();

    int fireNameHash = Animator.StringToHash("fire");
    int reloadNameHash = Animator.StringToHash("reload");

    void Awake()
    {
        m_Animator = GetComponentInChildren<Animator>();
        m_Source = GetComponentInChildren<AudioSource>();
        m_ClipContent = clipSize;

        if (PrefabRayTrail != null)
        {
            const int trailPoolSize = 16;
            PoolSystem.Instance.InitPool(PrefabRayTrail, trailPoolSize);
        }

        if (projectilePrefab != null)
        {
            int size = Mathf.Max(4, clipSize) * advancedSettings.projectilePerShot;
            for (int i = 0; i < size; ++i)
            {
                GameObject p = Instantiate(projectilePrefab);
                p.gameObject.SetActive(false);
                m_ProjectilePool.Enqueue(p);
            }
        }
    }

  

    public void PutAway()
    {
        m_Animator.WriteDefaultValues();

        for (int i = 0; i < m_ActiveTrails.Count; ++i)
        {
            var activeTrail = m_ActiveTrails[i];
            m_ActiveTrails[i].renderer.gameObject.SetActive(false);
        }

        m_ActiveTrails.Clear();
    }

    public void Selected()
    {
       


   

        triggerDown = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger);
        m_ShotDone = false;

        WeaponInfoUI.Instance.UpdateWeaponName(this);
        WeaponInfoUI.Instance.UpdateClipInfo(this);
        WeaponInfoUI.Instance.UpdateAmmoAmount(1000);


 
        m_Animator.SetTrigger("selected");
    }

    public void Fire()
    {
        if (m_CurrentState != WeaponState.Idle || m_ShotTimer > 0 || m_ClipContent == 0)
            return;

        m_ClipContent -= 1;

        m_ShotTimer = fireRate;

    

        WeaponInfoUI.Instance.UpdateClipInfo(this);

        m_CurrentState = WeaponState.Firing;

        m_Animator.SetTrigger("fire");

        m_Source.pitch = Random.Range(0.7f, 1.0f);
        m_Source.PlayOneShot(FireAudioClip);

        CameraShaker.Instance.Shake(0.2f, 0.05f * advancedSettings.screenShakeMultiplier);

        if (weaponType == WeaponType.Raycast)
        {
            for (int i = 0; i < advancedSettings.projectilePerShot; ++i)
            {
                RaycastShot();
            }
        }
        else
        {
            ProjectileShot();
        }
    }

    void RaycastShot()
    {
        //change raycast to gun not camera
        float spreadRatio = advancedSettings.spreadAngle / OVRManager.instance.GetComponent<Camera>().fieldOfView;

        Vector2 spread = spreadRatio * Random.insideUnitCircle;

        RaycastHit hit;
        Ray r = OVRManager.instance.GetComponent<Camera>().ViewportPointToRay(Vector3.one * 0.5f + (Vector3)spread);
        Vector3 hitPosition = r.origin + r.direction * 200.0f;

        if (Physics.Raycast(r, out hit, 1000.0f, ~(1 << 9), QueryTriggerInteraction.Ignore))
        {
            Renderer renderer = hit.collider.GetComponentInChildren<Renderer>();
            ImpactManager.Instance.PlayImpact(hit.point, hit.normal, renderer == null ? null : renderer.sharedMaterial);

            if (hit.distance > 5.0f)
                hitPosition = hit.point;

            if (hit.collider.gameObject.layer == 10)
            {
                Target target = hit.collider.gameObject.GetComponent<Target>();
                target.Got(damage);
            }
        }

        if (PrefabRayTrail != null)
        {
            var pos = new Vector3[] { GetCorrectedMuzzlePlace(), hitPosition };
            var trail = PoolSystem.Instance.GetInstance<LineRenderer>(PrefabRayTrail);
            trail.gameObject.SetActive(true);
            trail.SetPositions(pos);
            m_ActiveTrails.Add(new ActiveTrail()
            {
                remainingTime = 0.3f,
                direction = (pos[1] - pos[0]).normalized,
                renderer = trail
            });
        }
    }

    void ProjectileShot()
    {
        for (int i = 0; i < advancedSettings.projectilePerShot; ++i)
        {
            var projectile = m_ProjectilePool.Dequeue();
            projectile.transform.position = EndPoint.position;
            projectile.gameObject.SetActive(true);
          
        }
    }

    Vector3 GetCorrectedMuzzlePlace()
    {
        m_ConvertedMuzzlePos = OVRManager.instance.GetComponent<Camera>().WorldToScreenPoint(EndPoint.position);
        m_ConvertedMuzzlePos.z = 10.0f;
        return OVRManager.instance.GetComponent<Camera>().ScreenToWorldPoint(m_ConvertedMuzzlePos);
    }

    void Update()
    {
        if (m_ShotTimer >= 0)
        {
            m_ShotTimer -= Time.deltaTime;
        }


        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        {
            if (triggerType == TriggerType.Auto || !m_ShotDone)
            {
                Fire();
                m_ShotDone = true;
            }
        }
    }

  

   
}
