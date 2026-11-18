using UnityEngine;

namespace FlappyCanarinho
{
    /// <summary>
    /// Camada de fundo infinita: filhos (tiles) andam para a esquerda e dão a volta, criando rolagem sem fim.
    /// Os tiles ficam centralizados na câmera (ex.: -1, 0, +1 larguras), cobrindo também telas bem largas.
    /// </summary>
    public class ScrollingBackground : MonoBehaviour
    {
        [Tooltip("Unidades por segundo (parallax: camadas mais distantes são mais lentas).")]
        public float speed = 1f;
        [Tooltip("Se ligado, usa a velocidade dos canos do GameManager (usado no chão).")]
        public bool useWorldSpeed;
        [Tooltip("Largura de cada tile em unidades.")]
        public float tileWidth = 19.2f;
        [Tooltip("Rola mesmo sem GameManager (cena do menu).")]
        public bool alwaysScroll;

        Transform[] tiles;

        void Awake()
        {
            tiles = new Transform[transform.childCount];
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = transform.GetChild(i);

                // Centraliza os tiles: com 3 tiles ficam em -W, 0 e +W
                var p = tiles[i].localPosition;
                p.x = (i - (tiles.Length - 1) * 0.5f) * tileWidth;
                tiles[i].localPosition = p;
            }
        }

        void Update()
        {
            var gm = GameManager.Instance;
            float s = speed;

            if (gm != null)
            {
                if (gm.State == GameState.Dead) return;
                if (useWorldSpeed) s = gm.pipeSpeed;
            }
            else if (!alwaysScroll)
            {
                return;
            }

            float dx = s * Time.deltaTime;
            float wrap = tileWidth * tiles.Length;
            float limit = -wrap * 0.5f;

            for (int i = 0; i < tiles.Length; i++)
            {
                var p = tiles[i].localPosition;
                p.x -= dx;
                if (p.x < limit) p.x += wrap;
                tiles[i].localPosition = p;
            }
        }
    }
}
