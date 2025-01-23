using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Script;
using Unity.Mathematics;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public UnityEvent OnPosition;
    public UnityEvent OnDie;

    [SerializeField] float moveDuration;
    public Difficulty difficulty;
    [SerializeField] int health;
    [SerializeField] float timeBeforeAttack;
    [SerializeField] SpriteRenderer healthPrefab;
    [SerializeField] Sprite emptyHealth;
    [SerializeField] Transform healthPos;

    Player m_Player;
    Vector3 m_MoveTarget;
    Cooldown m_AttackCooldown;
    Animator m_Anim;
    ParticleSystem m_DeathParticle;
    Image m_FillTimer;
    List<SpriteRenderer> m_HealthVisuals = new ();

    public int Health => health;
    public bool IsDead => Health <= 0;

    void Awake()
    {
        m_Anim = GetComponent<Animator>();
    }

    public Enemy Init(Vector3 moveTo, Player player, ParticleSystem deathParticle, Image fillTimer)
    {
        m_Player = player;
        m_DeathParticle = deathParticle;
        m_FillTimer = fillTimer;
        
        transform.DOMove(moveTo, moveDuration).SetEase(Ease.Linear).onComplete = SetReady;
        m_AttackCooldown = new Cooldown(timeBeforeAttack, Attack, UpdateFillTimer);
        DisplayHealth();

        return this;
    }

    void DisplayHealth()
    {
        const float offset = 0.24f;
        for (int i = 0; i < health; i++)
        {
            Vector3 spawnPosition = new Vector3(healthPos.position.x + (i - health / 2) * offset, healthPos.position.y);
            m_HealthVisuals.Add(Instantiate(healthPrefab, spawnPosition, quaternion.identity, healthPos));
        }
    }
    
    void SetReady()
    {
        OnPosition?.Invoke();
        m_AttackCooldown.Start();
        m_Anim.SetTrigger("Idle");
    }

    void Update()
    {
        if (!IsDead)
            m_AttackCooldown?.Update();
    }

    void Attack()
    {
        if (m_Player.Hurt(1))
            StartCoroutine(Die(2, false));
        m_Anim.SetTrigger("Attack");
        m_AttackCooldown.Start();
    }

    void UpdateFillTimer()
    {
        m_FillTimer.fillAmount = m_AttackCooldown.Progress;
    }

    /// <summary>
    ///  Reduce health value
    /// </summary>
    /// <param name="value">Damage amount</param>
    /// <returns>True if died</returns>
    public bool GetDamage(int value, float timerAdd)
    {
        for (int i = 0; i < value; i++)
        {
            if (health - (i + 1) >= 0)
                m_HealthVisuals[health - (i + 1)].sprite = emptyHealth;
        }
        
        health -= value;
        m_AttackCooldown.TimeLeft += timerAdd;
        m_Anim.SetTrigger("Hurt");

        if (!IsDead) return false;

        StartCoroutine(Die());
        return true;
    }

    IEnumerator Die(float pauseDuration = 0.5f, bool killedByPlayer = true)
    {
        if (killedByPlayer)
            OnDie?.Invoke();
        else
            health = 0;

        yield return new WaitForSeconds(pauseDuration);

        if (!killedByPlayer)
        {
            OnHurtAnim();
            yield return new WaitForSeconds(pauseDuration/3);
        }
        
        Destroy(gameObject);
    }

    // Called by anim
    public void OnHurtAnim()
    {
        //Play sound

        SoundFX.Instance.PlaySound(SoundType.EnemyHurt);
        if (IsDead)
        {
            m_DeathParticle.Play();
            SoundFX.Instance.PlaySound(SoundType.Disappear);
        }
    }
}
