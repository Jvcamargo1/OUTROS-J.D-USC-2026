using UnityEngine;

namespace FlappyCanarinho
{
    /// <summary>
    /// Par de canos (cima + baixo). Anda para a esquerda, dá 1 ponto quando o pássaro passa
    /// e se destrói depois de sair pela borda esquerda da câmera (em qualquer proporção de tela).
    /// </summary>
    public class Pipe : MonoBehaviour
    {
        [Tooltip("Folga (unidades) além da borda esquerda da tela antes de destruir o cano.")]
        public float despawnMargin = 2f;

        bool scored;
        Transform bird;
        Camera cam;

        void Start()
        {
            cam = Camera.main;
            var b = FindFirstObjectByType<BirdController>();
            if (b != null) bird = b.transform;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing) return;

            transform.position += Vector3.left * (gm.pipeSpeed * Time.deltaTime);

            if (!scored && bird != null && transform.position.x < bird.position.x)
            {
                scored = true;
                gm.AddScore();
            }

            float leftEdge = cam.transform.position.x - cam.orthographicSize * cam.aspect;
            if (transform.position.x < leftEdge - despawnMargin) Destroy(gameObject);
        }
    }
}
