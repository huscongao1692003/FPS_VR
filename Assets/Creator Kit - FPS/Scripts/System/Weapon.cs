using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Oculus.Interaction.Input;

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

    public Projectile projectilePrefab;
    public float projectileLaunchForce = 200.0f;

    public Transform EndPoint;

    public AdvancedSettings advancedSettings;

    [Header("Animation Clips")]
    public AnimationClip FireAnimationClip;
    public AnimationClip ReloadAnimationClip;

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

    Queue<Projectile> m_ProjectilePool = new Queue<Projectile>();

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
                Projectile p = Instantiate(projectilePrefab);
                p.gameObject.SetActive(false);
                m_ProjectilePool.Enqueue(p);
            }
        }
    }

    public void PickedUp(Controller c)
    {
        m_Owner = c;
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
        var ammoRemaining = m_Owner.GetAmmo(ammoType);

        gameObject.SetActive(ammoRemaining != 0 || m_ClipContent != 0);

        if (FireAnimationClip != null)
            m_Animator.SetFloat("fireSpeed", FireAnimationClip.length / fireRate);

        if (ReloadAnimationClip != null)
            m_Animator.SetFloat("reloadSpeed", ReloadAnimationClip.length / reloadTime);

        m_CurrentState = WeaponState.Idle;

        triggerDown = false;
        m_ShotDone = false;

        WeaponInfoUI.Instance.UpdateWeaponName(this);
        WeaponInfoUI.Instance.UpdateClipInfo(this);
        WeaponInfoUI.Instance.UpdateAmmoAmount(m_Owner.GetAmmo(ammoType));


        if (m_ClipContent == 0 && ammoRemaining != 0)
        {
            int chargeInClip = Mathf.Min(ammoRemaining, clipSize);
            m_ClipContent += chargeInClip;
           
            m_Owner.ChangeAmmo(ammoType, -chargeInClip);
            WeaponInfoUI.Instance.UpdateClipInfo(this);
        }

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

        if (m_CurrentState == WeaponState.Firing)
        {
            var info = m_Animator.GetCurrentAnimatorStateInfo(0);
            if (info.normalizedTime >= info.length / 2.0f)
            {
                if (m_ClipContent == 0)
                {
                    StartReload();
                }
                else
                {
                    m_CurrentState = WeaponState.Idle;
                }
            }
        }
        else if (m_CurrentState == WeaponState.Reloading)
        {
            var info = m_Animator.GetCurrentAnimatorStateInfo(0);
            if (info.normalizedTime >= info.length / 2.0f)
            {
                m_CurrentState = WeaponState.Idle;
            }
        }

        for (int i = 0; i < m_ActiveTrails.Count; ++i)
        {
            var activeTrail = m_ActiveTrails[i];
            activeTrail.remainingTime -= Time.deltaTime;
            if (activeTrail.remainingTime < 0)
            {
                activeTrail.renderer.gameObject.SetActive(false);
                m_ActiveTrails.RemoveAt(i);
                --i;
            }
            else
            {
                activeTrail.renderer.SetPosition(0, activeTrail.renderer.GetPosition(0) + activeTrail.direction * 100.0f * Time.deltaTime);
            }
        }

        if (triggerDown)
        {
            if (triggerType == TriggerType.Auto || !m_ShotDone)
            {
                Fire();
                m_ShotDone = true;
            }
        }
    }

    void StartReload()
    {
        if (m_CurrentState != WeaponState.Idle || m_ClipContent == clipSize || m_Owner.GetAmmo(ammoType) == 0)
            return;

        m_CurrentState = WeaponState.Reloading;

        m_Source.PlayOneShot(ReloadAudioClip);

        m_Animator.SetTrigger("reload");

    }

   
}
