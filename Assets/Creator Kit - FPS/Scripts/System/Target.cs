using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Target : MonoBehaviour
{
    public float health = 5.0f;
    public int pointValue;

    public ParticleSystem DestroyedEffect;

    [Header("Audio")]
    public RandomPlayer HitPlayer;
    public AudioSource IdleSource;

    public bool Destroyed => m_Destroyed;

    bool m_Destroyed = false;
    float m_CurrentHealth;

    private bool _isDisable;

    void Awake()
    {
        Helpers.RecursiveLayerChange(transform, LayerMask.NameToLayer("Target"));
    }

    void Start()
    {
        if (DestroyedEffect)
            PoolSystem.Instance.InitPool(DestroyedEffect, 16);

        m_CurrentHealth = health;
        if (IdleSource != null)
            IdleSource.time = Random.Range(0.0f, IdleSource.clip.length);
    }

    public void Got(float damage)
    {
        m_CurrentHealth -= damage;

        if (HitPlayer != null)
            HitPlayer.PlayRandom();

        if (m_CurrentHealth > 0)
            return;

        Vector3 position = transform.position;

        if (HitPlayer != null)
        {
            var source = WorldAudioPool.GetWorldSFXSource();
            source.transform.position = position;
            source.pitch = HitPlayer.source.pitch;
            source.PlayOneShot(HitPlayer.GetRandomClip());
        }

        if (DestroyedEffect != null)
        {
            var effect = PoolSystem.Instance.GetInstance<ParticleSystem>(DestroyedEffect);
            effect.transform.position = position;
            effect.time = 0.0f;
            effect.Play();
        }

        m_Destroyed = true;
        gameObject.SetActive(false);
        GameSystem.Instance.TargetDestroyed(pointValue);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!_isDisable && other.gameObject.CompareTag("Bullet"))
        {
            m_CurrentHealth = 0; // Set health to 0 to ensure it gets destroyed
            Got(0); // Call Got to handle destruction
        }
    }
}
