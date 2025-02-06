using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Random = UnityEngine.Random;

namespace Script
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        
        [SerializeField] Player player;
        [SerializeField] WordList wordList;
        [SerializeField] PhaseDatabase phases;

        [Header("Enemy Settings")]
        [SerializeField] Transform spawn;
        [SerializeField] Transform target;
        [SerializeField] ParticleSystem enemyDeathParticle;
        [SerializeField] Image fillTimer;

        [Header("End UI")]
        [SerializeField] RectTransform finalScorePanel;
        [SerializeField] TextMeshProUGUI currentScore;
        [SerializeField] TextMeshProUGUI bestScore;
        [SerializeField] float scoreOffset;

        int m_WaveCount;
        int m_BestWave;
        bool m_GamePaused;

        public PhaseData CurrentPhase => phases.data[m_WaveCount - 1];
        public bool FreezeTimer => CurrentPhase.freezeTimer;
        public bool GamePaused => m_GamePaused;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        void Start()
        {
            StartCoroutine(StartGame());
            player.onDie.AddListener(GameOver);
        }

        IEnumerator StartGame()
        {
            yield return new WaitForSeconds(1f);
            StartWave();
        }

        void Update()
        {
            if (!player.IsAttacking && fillTimer.fillAmount > 0f)
                fillTimer.fillAmount = Mathf.Lerp(fillTimer.fillAmount, 0f, Time.deltaTime * 3);
        }

        void StartWave()
        {
            m_WaveCount++;
            Enemy newEnemy = Instantiate(CurrentPhase.enemy, spawn.position, Quaternion.identity).Init(target.position, player, enemyDeathParticle, fillTimer);
            newEnemy.OnPosition.AddListener(delegate{StartAttack(newEnemy);});
            newEnemy.OnDie.AddListener(OnEnemyKilled);
        }

        void OnEnemyKilled()
        {
            StartWave();
        }

        void StartAttack(Enemy target)
        {
            player.StartAttack(target);
        }

        public string GetWord(Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Easy => wordList.shortWords[Random.Range(0, wordList.shortWords.Length)],
                Difficulty.Medium => wordList.mediumWords[Random.Range(0, wordList.mediumWords.Length)],
                Difficulty.Hard => wordList.longWords[Random.Range(0, wordList.longWords.Length)],
                _ => null
            };
        }

        void GameOver()
        {
            if (m_BestWave < m_WaveCount) m_BestWave = m_WaveCount;
            StartCoroutine(DisplayEndElements());
        }

        IEnumerator DisplayEndElements()
        {
            yield return new WaitForSeconds(2f);

            bestScore.text = m_BestWave.ToString();
            currentScore.text = m_WaveCount.ToString();
            finalScorePanel.DOMoveY(Screen.height - finalScorePanel.rect.height/2 - scoreOffset, .5f);
            player.SetWord("rejouer", false);
        }

        public void RestartGame()
        {
            m_WaveCount = 0;
            player.Revive();
            finalScorePanel.DOMoveY(Screen.height + finalScorePanel.rect.height/2 + scoreOffset, .5f);
            StartCoroutine(StartGame());
        }

        public void TogglePause()
        {
            if (m_GamePaused)
            {
                Time.timeScale = 1f;
                m_GamePaused = false;
            }
            else
            {
                Time.timeScale = 0f;
                m_GamePaused = true;
            }
        }
    }

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }
}
