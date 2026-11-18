using UnityEngine;

namespace FlappyCanarinho
{
    /// <summary>
    /// Gera pares de canos sem parar (mapa infinito) enquanto a partida estiver rolando.
    /// Os canos nascem sempre logo depois da borda direita da câmera (fora da tela), em qualquer proporção de tela.
    /// </summary>
    public class PipeSpawner : MonoBehaviour
    {
        public GameObject pipePairPrefab;
        public Sprite[] pipeSprites;

        [Header("Posição / ritmo")]
        [Tooltip("Folga (unidades) além da borda direita da tela onde o cano nasce, para nunca aparecer 'do nada'.")]
        public float spawnMargin = 1.5f;
        [Tooltip("Largura do cano em unidades (sprite do kit = 1,44).")]
        public float pipeWidth = 1.44f;
        public float spawnInterval = 1.9f;
        [Tooltip("Tempo até o primeiro par nascer depois do primeiro toque (0 = imediatamente).")]
        public float firstSpawnDelay = 0f;

        [Header("Abertura (buraco)")]
        public float gapSize = 3.4f;
        public float minGapCenter = -0.8f;
        public float maxGapCenter = 2.9f;
        [Tooltip("Variação máxima da altura do buraco entre dois pares seguidos (mantém o jogo justo).")]
        public float maxStep = 2.2f;

        [Header("Sprite do cano")]
        public float pipeHeight = 5.62f;

        float timer;
        float lastCenter;
        Camera cam;

        void Start()
        {
            cam = Camera.main;
            timer = firstSpawnDelay;
            lastCenter = (minGapCenter + maxGapCenter) * 0.5f;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing) return;

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Spawn();
                timer = spawnInterval;
            }
        }

        /// <summary>X de nascimento: borda direita da câmera + meia largura do cano + folga.</summary>
        float GetSpawnX()
        {
            float halfWidth = cam.orthographicSize * cam.aspect;
            return cam.transform.position.x + halfWidth + pipeWidth * 0.5f + spawnMargin;
        }

        void Spawn()
        {
            float lo = Mathf.Max(minGapCenter, lastCenter - maxStep);
            float hi = Mathf.Min(maxGapCenter, lastCenter + maxStep);
            float center = Random.Range(lo, hi);
            lastCenter = center;

            var go = Instantiate(pipePairPrefab, new Vector3(GetSpawnX(), 0f, 0f), Quaternion.identity, transform);

            float offset = gapSize * 0.5f + pipeHeight * 0.5f;
            var top = go.transform.Find("Top");
            var bottom = go.transform.Find("Bottom");
            top.localPosition = new Vector3(0f, center + offset, 0f);
            bottom.localPosition = new Vector3(0f, center - offset, 0f);

            if (pipeSprites != null && pipeSprites.Length > 0)
            {
                var sprite = pipeSprites[Random.Range(0, pipeSprites.Length)];
                top.GetComponent<SpriteRenderer>().sprite = sprite;
                bottom.GetComponent<SpriteRenderer>().sprite = sprite;
            }
        }
    }
}
