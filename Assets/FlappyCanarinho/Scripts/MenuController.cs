using UnityEngine;
using UnityEngine.SceneManagement;

namespace FlappyCanarinho
{
    /// <summary>
    /// Botões da cena Menu: Iniciar, Créditos e Voltar.
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        public const string GameSceneName = "Game";

        public GameObject creditsPanel;

        void Start()
        {
            Time.timeScale = 1f;
            if (creditsPanel != null) creditsPanel.SetActive(false);
        }

        public void StartGame()
        {
            SceneManager.LoadScene(GameSceneName);
        }

        public void OpenCredits()
        {
            creditsPanel.SetActive(true);
        }

        public void CloseCredits()
        {
            creditsPanel.SetActive(false);
        }
    }
}
