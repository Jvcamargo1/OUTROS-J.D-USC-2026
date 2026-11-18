using UnityEngine;

namespace FlappyCanarinho
{
    /// <summary>
    /// Pássaro controlado só pela barra de espaço. Antes do primeiro toque ele paira no lugar.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class BirdController : MonoBehaviour
    {
        [Header("Movimento")]
        public float flapVelocity = 8f;
        public float gravityScale = 2.6f;
        public float maxFallSpeed = 14f;
        [Tooltip("Altura máxima (o pássaro não sai por cima da tela).")]
        public float ceilingY = 5.1f;

        [Header("Visual")]
        public Transform visual;
        public float hoverAmplitude = 0.25f;
        public float hoverSpeed = 4f;

        Rigidbody2D rb;
        Animator anim;
        float startY;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponentInChildren<Animator>();
            startY = transform.position.y;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            switch (gm.State)
            {
                case GameState.Ready:
                    var p = transform.position;
                    p.y = startY + Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
                    transform.position = p;
                    if (FlapPressed())
                    {
                        gm.StartGame();
                        rb.gravityScale = gravityScale;
                        Flap();
                    }
                    break;

                case GameState.Playing:
                    if (FlapPressed()) Flap();
                    LimitMovement();
                    break;

                case GameState.Dead:
                    LimitMovement();
                    break;
            }

            UpdateTilt();
        }

        static bool FlapPressed()
        {
            return Input.GetKeyDown(KeyCode.Space);
        }

        void Flap()
        {
            rb.linearVelocity = new Vector2(0f, flapVelocity);

            var gm = GameManager.Instance;
            if (gm != null) gm.PlayFlap();
        }

        void LimitMovement()
        {
            var v = rb.linearVelocity;
            if (v.y < -maxFallSpeed) v.y = -maxFallSpeed;

            if (transform.position.y > ceilingY)
            {
                var p = transform.position;
                p.y = ceilingY;
                transform.position = p;
                if (v.y > 0f) v.y = 0f;
            }
            rb.linearVelocity = v;
        }

        void UpdateTilt()
        {
            if (visual == null) return;
            float target = Mathf.Clamp(rb.linearVelocity.y * 5f, -80f, 30f);
            float z = Mathf.LerpAngle(visual.localEulerAngles.z, target, Time.deltaTime * 10f);
            visual.localRotation = Quaternion.Euler(0f, 0f, z);
        }

        void Die()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State == GameState.Dead) return;

            gm.GameOver();
            if (anim != null) anim.speed = 0f;
            rb.linearVelocity = new Vector2(0f, Mathf.Min(rb.linearVelocity.y, 0f));
        }

        // Canos são triggers
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<Pipe>() != null) Die();
        }

        // Chão é colisor sólido
        void OnCollisionEnter2D(Collision2D collision)
        {
            Die();
        }
    }
}
