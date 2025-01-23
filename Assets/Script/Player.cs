using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Script
{
    public class Player : MonoBehaviour
    {
        public UnityEvent onDie;

        [SerializeField] Transform startPoint;
        [SerializeField] Transform endPoint;
        [SerializeField] ParticleSystem deathParticle;
        Animator m_Anim;

        [SerializeField] WordFX wordFX;
        [SerializeField] ComboFX comboFX;
        [SerializeField] int health;
        [SerializeField] int damage;
        [SerializeField] int timerAdd;

        [SerializeField] Camera cam;
        [SerializeField] Image[] Hearts;
        [SerializeField] Sprite FullfillHeart;
        [SerializeField] Sprite EmptyHeart;

        Enemy m_Enemy;
        string m_Word;
        int m_CharIndex;
        int combo;

        readonly int m_AttackAnim = Animator.StringToHash("Attack");
        readonly int m_IdleAnim = Animator.StringToHash("Idle");
        readonly int m_HurtAnim = Animator.StringToHash("Hurt");
        readonly int m_DeathAnim = Animator.StringToHash("Death");
        readonly int m_WalkAnim = Animator.StringToHash("Walk");

        public bool IsAttacking => m_Enemy != null;

        void Awake()
        {
            m_Anim = GetComponent<Animator>();
        }

        void Start()
        {
            StartCoroutine(FillHearts());
            m_Anim.SetTrigger(m_WalkAnim);
            transform.position = startPoint.position;
            transform.DOMove(endPoint.position, 2f).SetEase(Ease.Linear).onComplete = () => m_Anim.SetTrigger(m_IdleAnim);
        }

        void OnEnable()
        {
            Keyboard.current.onTextInput += GetKeyInput;
        }
        
        void OnDisable()
        {
            Keyboard.current.onTextInput -= GetKeyInput;
        }

        void GetKeyInput(char input)
        {
            if (m_Word == null) return;

            if (input == m_Word[m_CharIndex])
            {
                ValidChar();
            }
            else
            {
                WrongChar();
            }
        }

        void ValidChar()
        {
            m_CharIndex++;
            wordFX.Press();

            if (m_CharIndex != m_Word.Length) return;
            
            if (m_Enemy != null)
                Attack();
            else if (m_Word == "restart")
            {
                GameManager.Instance.RestartGame();
                wordFX.Finish();
            }
            
        }

        void WrongChar()
        {
            m_CharIndex = 0;
            wordFX.Cancel();
            ResetCombo();
        }

        void Attack()
        {
            wordFX.Finish();
            m_Anim.SetTrigger(m_AttackAnim);
            SoundFX.Instance.PlaySound(SoundType.Attack);

            if (m_Enemy.GetDamage(damage + combo, timerAdd))
            {
                // ENEMY KILLED
                m_Word = null;
                ResetCombo();
                m_Enemy = null;
            }
            else
            {
                StartAttack(m_Enemy);
                AddCombo();
            }
        }

        void AddCombo()
        {
            comboFX.UpdateCombo(combo);
            combo++;
        }

        void ResetCombo()
        {
            combo = 0;
            m_Anim.SetTrigger(m_IdleAnim);
            comboFX.CancelCombo();
        }

        public void StartAttack(Enemy enemy)
        {
            SetWord(GameManager.Instance.GetWord(enemy.difficulty));
            
            m_Enemy = enemy;
            m_CharIndex = 0;
        }

        public void SetWord(string word)
        {
            m_Word = word;
            StartCoroutine(wordFX.DisplayWord(m_Word));
        }

        /// <summary>
        /// Reduce player health
        /// </summary>
        /// <param name="value">Damage dealt</param>
        /// <returns>True if player dies</returns>
        public bool Hurt(int value)
        {
            for (int i = 0; i < value; i++)
            {
                if (health - (i + 1) >= 0)
                    Hearts[health - (i + 1)].sprite = EmptyHeart;
            }
            
            health -= value;
            ResetCombo();
            
            print(health);
            if (health > 0)
                m_Anim.SetTrigger(m_HurtAnim);
            else
            {
                Die();
                return true;
            }

            return false;
        }

        void Die()
        {
            wordFX.Finish();
            m_CharIndex = 0;
            m_Anim.SetTrigger(m_DeathAnim);
            m_Word = null;
            onDie?.Invoke();
        }

        public void HurtFromAnim()
        {
            cam.DOShakePosition(0.3f, .2f);
            SoundFX.Instance.PlaySound(SoundType.PlayerHurt);
        }

        IEnumerator FillHearts()
        {
            foreach (Image heart in Hearts)
            {
                yield return new WaitForSeconds(0.5f);
                heart.sprite = FullfillHeart;
            }
        }

        public void Revive()
        {
            deathParticle.Play();
            SoundFX.Instance.PlaySound(SoundType.Disappear);
            Start();
            health = 3;
        }
    }
}
