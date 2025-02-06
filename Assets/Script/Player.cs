using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Script
{
    public class Player : MonoBehaviour
    {
        public enum NerfColor
        {
            Black,
            Green,
            Red,
            Orange
        }

        const int EscapeKeyCode = 1769499;
        public UnityEvent onDie;

        [SerializeField] Transform startPoint;
        [SerializeField] Transform endPoint;
        [SerializeField] ParticleSystem deathParticle;

        [SerializeField] WordFX wordFX;
        [SerializeField] ComboFX comboFX;
        [SerializeField] int health;
        [SerializeField] int damage;
        [SerializeField] int timerAdd;

        [SerializeField] Camera cam;
        [SerializeField] Image[] Hearts;
        [SerializeField] Sprite FullfillHeart;
        [SerializeField] Sprite EmptyHeart;

        readonly int m_AttackAnim = Animator.StringToHash("Attack");
        readonly int m_DeathAnim = Animator.StringToHash("Death");
        readonly int m_HurtAnim = Animator.StringToHash("Hurt");
        readonly int m_IdleAnim = Animator.StringToHash("Idle");
        readonly int m_WalkAnim = Animator.StringToHash("Walk");
        int combo;
        Animator m_Anim;
        int m_CharIndex;

        Enemy m_Enemy;
        string m_Word;

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
            transform.DOMove(endPoint.position, 2f).SetEase(Ease.Linear).onComplete =
                () => m_Anim.SetTrigger(m_IdleAnim);
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
                GameManager.Instance.TogglePause();
                return;
            }

            if (m_Word == null || GameManager.Instance.GamePaused) return;

            if (input == m_Word[m_CharIndex])
                ValidChar();
            else
                WrongChar();
        }

        void ValidChar()
        {
            m_CharIndex++;
            wordFX.Press();

            if (m_CharIndex != m_Word.Length) return;

            if (m_Enemy != null)
            {
                Attack();
            }
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

            m_Word = ApplyNerf(word, out var nerfData);

            StartCoroutine(wordFX.DisplayWord(word, nerfData));
        }

        string ApplyNerf(string word, out Dictionary<int, NerfColor> nerfData)
        {
            nerfData = new Dictionary<int, NerfColor>();
            var currPhase = GameManager.Instance.CurrentPhase;
            var wordToWrite = new StringBuilder(word);

            // Orange nerf
            if (currPhase.orangeNerf)
            {
                var nerfIndex1 = DrawIndex(nerfData.Keys.ToArray(), word.Length);
                var char1 = wordToWrite[nerfIndex1];
                nerfData.Add(nerfIndex1, NerfColor.Orange);

                var nerfIndex2 = DrawIndex(nerfData.Keys.ToArray(), word.Length);
                var char2 = wordToWrite[nerfIndex2];
                nerfData.Add(nerfIndex2, NerfColor.Orange);

                wordToWrite.Replace(char1, char2, nerfIndex1, 1);
                wordToWrite.Replace(char2, char1, nerfIndex2, 1);
            }

            // Red nerf
            for (var i = 0; i < currPhase.redNerf; i++)
            {
                if (word.Length <= nerfData.Count + i) break;

                var nerfIndex = DrawIndex(nerfData.Keys.ToArray(), word.Length);
                wordToWrite.Remove(GetIndexToModif(nerfIndex, nerfData), 1);
                nerfData.Add(nerfIndex, NerfColor.Red);
            }

            // Green nerf
            for (var i = 0; i < currPhase.greenNerf; i++)
            {
                if (word.Length <= nerfData.Count) break;

                var nerfIndex = DrawIndex(nerfData.Keys.ToArray(), word.Length);
                var indexToModif = GetIndexToModif(nerfIndex, nerfData);
                wordToWrite.Insert(indexToModif, wordToWrite[indexToModif]);
                nerfData.Add(nerfIndex, NerfColor.Green);
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
                switch (nerf.Value)
                {
                    case NerfColor.Green:
                        indexDrew++;
                        break;
                    case NerfColor.Red:
                        indexDrew--;
                        break;
                }
            
            return indexDrew;
        }

        /// <summary>
        ///     Reduce player health
        /// </summary>
        /// <param name="value">Damage dealt</param>
        /// <returns>True if player dies</returns>
        public bool Hurt(int value)
        {
            for (var i = 0; i < value; i++)
                if (health - (i + 1) >= 0)
                    Hearts[health - (i + 1)].sprite = EmptyHeart;

            health -= value;
            ResetCombo();

            if (health > 0)
            {
                m_Anim.SetTrigger(m_HurtAnim);
            }
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
            foreach (var heart in Hearts)
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