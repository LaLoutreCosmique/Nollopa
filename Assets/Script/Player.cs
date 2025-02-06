using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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

        const int EscapeKeyCode = 1769499;

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
            if (input.GetHashCode() == EscapeKeyCode)
            {
                print("OK");
                GameManager.Instance.TogglePause();
                return;
            }
            
            if (m_Word == null || GameManager.Instance.GamePaused) return;

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
            else if (m_Word == "rejouer")
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

        public void SetWord(string word, bool withNerf = true)
        {
            if (!withNerf)
            {
                m_Word = word;
                StartCoroutine(wordFX.DisplayWord(word, new Dictionary<int, NerfColor>()));
                return;
            }
            
            m_Word = ApplyNerf(word, out Dictionary<int, NerfColor> nerfData);

            StartCoroutine(wordFX.DisplayWord(word, nerfData));
            
            print(m_Word);
        }

        string ApplyNerf(string word, out Dictionary<int, NerfColor> nerfData)
        {
            nerfData = new Dictionary<int, NerfColor>();
            PhaseData currPhase = GameManager.Instance.CurrentPhase;
            StringBuilder wordToWrite = new StringBuilder(word);
            int nerfIndex;

            // Red nerf
            for (int i = 0; i < currPhase.redNerf; i++)
            {
                if (word.Length <= nerfData.Count + i) break;

                nerfIndex = DrawIndex(nerfData.Keys.ToArray(), word.Length);
                wordToWrite.Remove(GetIndexToModif(nerfIndex, nerfData), 1);
                nerfData.Add(nerfIndex, NerfColor.Red);
                
                print("draw" + nerfIndex);
            }
            
            // Green nerf
            for (int i = 0; i < currPhase.greenNerf; i++)
            {
                if (word.Length <= nerfData.Count) break;
                
                nerfIndex = DrawIndex(nerfData.Keys.ToArray(), word.Length);
                int indexToModif = GetIndexToModif(nerfIndex, nerfData);
                wordToWrite.Insert(indexToModif, wordToWrite[indexToModif]);
                nerfData.Add(nerfIndex, NerfColor.Green);
                
                print("draw : " + nerfIndex);
            }
            
            return wordToWrite.ToString();
        }
        
        int DrawIndex(int[] bannedIndexes, int maxIndex)
        {
            int index;
            while (true)
            {
                index = Random.Range(0, maxIndex);
                if (bannedIndexes.All(x => x != index)) return index;
            }
        }

        int GetIndexToModif(int indexDrew, Dictionary<int, NerfColor> nerfData)
        {
            foreach (var nerf in nerfData.Where(nerf => indexDrew > nerf.Key))
            {
                switch (nerf.Value)
                {
                    case NerfColor.Green:
                        indexDrew++;
                        break;
                    case NerfColor.Red:
                        indexDrew--;
                        break;
                }
            }

            print("to modif : " + indexDrew);
            return indexDrew;
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

        public enum NerfColor
        {
            Black,
            Green,
            Red,
            Orange
        }
    }
}
