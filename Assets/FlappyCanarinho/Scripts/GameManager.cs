using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FlappyCanarinho
{
    public enum GameState { Ready, Playing, Dead }

    /// <summary>
    /// Controla o estado da partida (aguardando, jogando, game over), a pontuação, os sons e a UI da cena Game.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public const string MenuSceneName = "Menu";
        const string BestScoreKey = "FlappyCanarinho.MelhorPontuacao";

        public static GameManager Instance { get; private set; }

        [Header("Mundo")]
        [Tooltip("Velocidade (unidades/s) com que canos e chão andam para a esquerda.")]
        public float pipeSpeed = 4f;

        [Header("UI")]
        public TMP_Text scoreText;
        public GameObject readyPanel;
        public GameObject gameOverPanel;
        public TMP_Text finalScoreText;
        public TMP_Text bestScoreText;

        [Header("Áudio")]
        public AudioClip flapClip;
        public AudioClip scoreClip;
        public AudioClip gameOverClip;
        [Range(0f, 1f)] public float sfxVolume = 1f;

        public GameState State { get; private set; } = GameState.Ready;
        public int Score { get; private set; }

        AudioSource sfx;

        void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;

            sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            sfx.spatialBlend = 0f; // som 2D
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            scoreText.text = "0";
            readyPanel.SetActive(true);
            gameOverPanel.SetActive(false);
        }

        public void StartGame()
        {
            if (State != GameState.Ready) return;
            State = GameState.Playing;
            readyPanel.SetActive(false);
        }

        public void AddScore()
        {
            if (State != GameState.Playing) return;
            Score++;
            scoreText.text = Score.ToString();
            Play(scoreClip);
        }

        public void GameOver()
        {
            if (State == GameState.Dead) return;
            State = GameState.Dead;

            int best = PlayerPrefs.GetInt(BestScoreKey, 0);
            if (Score > best)
            {
                best = Score;
                PlayerPrefs.SetInt(BestScoreKey, best);
                PlayerPrefs.Save();
            }

            finalScoreText.text = "Pontos: " + Score;
            bestScoreText.text = "Melhor: " + best;
            gameOverPanel.SetActive(true);
            Play(gameOverClip);
        }

        // Chamado pelo pássaro a cada pulo
        public void PlayFlap()
        {
            Play(flapClip);
        }

        void Play(AudioClip clip)
        {
            if (clip != null) sfx.PlayOneShot(clip, sfxVolume);
        }

        // Ligado ao botão "Reiniciar"
        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Ligado ao botão "Menu"
        public void BackToMenu()
        {
            SceneManager.LoadScene(MenuSceneName);
        }
    }
}
