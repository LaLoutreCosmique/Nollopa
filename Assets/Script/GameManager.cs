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
        [SerializeField] int easyThreshold;
        [SerializeField] int mediumThreshold;
        [SerializeField] int hardThreshold;

        [Header("Enemy Settings")]
        [SerializeField] Transform spawn;
        [SerializeField] Transform target;
        [SerializeField] Enemy enemy;
        [SerializeField] ParticleSystem enemyDeathParticle;
        [SerializeField] Image fillTimer;

        [Header("End UI")]
        [SerializeField] RectTransform finalScorePanel;
        [SerializeField] TextMeshProUGUI currentScore;
        [SerializeField] TextMeshProUGUI bestScore;
        [SerializeField] float scoreOffset;

        int waveCount;
        int bestWave;

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
            waveCount++;
            Enemy newEnemy = Instantiate(enemy, spawn.position, Quaternion.identity).Init(target.position, player, enemyDeathParticle, fillTimer);
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
                Difficulty.Medium => wordList.mediumWords[Random.Range(0, wordList.shortWords.Length)],
                Difficulty.Hard => wordList.longWords[Random.Range(0, wordList.shortWords.Length)],
                _ => null
            };
        }

        void GameOver()
        {
            if (bestWave < waveCount) bestWave = waveCount;
            StartCoroutine(DisplayEndElements());
        }

        IEnumerator DisplayEndElements()
        {
            yield return new WaitForSeconds(2f);

            bestScore.text = bestWave.ToString();
            currentScore.text = waveCount.ToString();
            finalScorePanel.DOMoveY(Screen.height - finalScorePanel.rect.height/2 - scoreOffset, .5f);
            player.SetWord("restart");
        }

        public void RestartGame()
        {
            waveCount = 0;
            player.Revive();
            finalScorePanel.DOMoveY(Screen.height + finalScorePanel.rect.height/2 + scoreOffset, .5f);
            StartCoroutine(StartGame());
        }
    }

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }
}
