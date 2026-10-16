using UnityEngine;

namespace TrilhaDaBolota
{
    // Esquilo controlado pelo jogador: anda, pula, toma dano e escolhe a animação.
    // A colisão com as plataformas é feita pela física 2D (Rigidbody2D + BoxCollider2D
    // contra o Tilemap Collider 2D + Composite Collider 2D do chão).
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteAnimator))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movimento")]
        public float moveSpeed = 7f;
        public float acceleration = 70f;
        public float jumpSpeed = 15f;
        public float maxFallSpeed = 18f;
        [Tooltip("Tempo em que ainda dá para pular depois de sair da beirada.")]
        public float coyoteTime = .1f;
        [Tooltip("Tempo em que o aperto do pulo fica guardado antes de tocar o chão.")]
        public float jumpBufferTime = .12f;

        [Header("Animações")]
        public Sprite[] idleFrames;
        public Sprite[] runFrames;
        public Sprite[] jumpFrames;
        public Sprite[] fallFrames;

        [Header("Controle pelo teste automático")]
        public bool useManualInput;
        public float manualMove;
        public bool manualJump;

        public Rigidbody2D Body { get; private set; }
        public Collider2D Hitbox { get; private set; }
        public bool Grounded { get; private set; }
        public bool Invulnerable => Time.time < invulnerableUntil;
        public bool FacingRight => !spriteRenderer.flipX;

        SpriteRenderer spriteRenderer;
        SpriteAnimator animator;
        ContactFilter2D groundFilter;
        float lastGroundedTime = -10f;
        float lastJumpPressedTime = -10f;
        float invulnerableUntil;
        float knockbackUntil;
        float moveInput;
        bool jumpHeld;

        void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
            Hitbox = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<SpriteAnimator>();
            // "Chão" = contato com a camada Chao cuja normal aponta para cima (45° a 135°).
            groundFilter = new ContactFilter2D();
            groundFilter.SetLayerMask(LayerMask.GetMask("Chao"));
            groundFilter.SetNormalAngle(45f, 135f);
        }

        void Update()
        {
            ReadInput();
            Animate();
            // Pisca enquanto está invulnerável depois de um dano.
            spriteRenderer.enabled = !Invulnerable || Mathf.Repeat(Time.time * 12f, 1f) > .35f;
        }

        void ReadInput()
        {
            if (useManualInput)
            {
                moveInput = manualMove;
                jumpHeld = manualJump;
                if (manualJump) { lastJumpPressedTime = Time.time; manualJump = false; jumpHeld = true; }
                return;
            }
            moveInput = Input.GetAxisRaw("Horizontal");
            bool pressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)
                || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Z);
            if (pressed) lastJumpPressedTime = Time.time;
            jumpHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W)
                || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Z);
        }

        void FixedUpdate()
        {
            Grounded = Body.IsTouching(groundFilter) && Body.linearVelocity.y <= .01f;
            if (Grounded) lastGroundedTime = Time.time;

            Vector2 velocity = Body.linearVelocity;
            if (Time.time >= knockbackUntil)
                velocity.x = Mathf.MoveTowards(velocity.x, moveInput * moveSpeed, acceleration * Time.fixedDeltaTime);

            bool canJump = Time.time - lastGroundedTime <= coyoteTime;
            bool wantsJump = Time.time - lastJumpPressedTime <= jumpBufferTime;
            if (canJump && wantsJump)
            {
                velocity.y = jumpSpeed;
                lastGroundedTime = lastJumpPressedTime = -10f;
                LevelManager.Instance?.Sound.PlayJump();
            }
            // Soltar o botão no meio da subida corta o pulo (pulo curto x pulo alto).
            if (!jumpHeld && velocity.y > jumpSpeed * .4f && !useManualInput)
                velocity.y = jumpSpeed * .4f;
            velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
            Body.linearVelocity = velocity;
        }

        void Animate()
        {
            if (moveInput > .01f) spriteRenderer.flipX = false;
            else if (moveInput < -.01f) spriteRenderer.flipX = true;

            if (!Grounded)
                animator.Play(Body.linearVelocity.y > 0f ? jumpFrames : fallFrames, 4f);
            else if (Mathf.Abs(Body.linearVelocity.x) > .3f)
                animator.Play(runFrames, 12f);
            else
                animator.Play(idleFrames, 2.5f);
        }

        // Dano de inimigo: perde vida, é empurrado para longe e fica invulnerável por um tempo.
        public void Hurt(Vector2 source)
        {
            if (Invulnerable || LevelManager.Instance == null || LevelManager.Instance.IsEnding) return;
            invulnerableUntil = Time.time + GameRules.InvulnerableTime;
            knockbackUntil = Time.time + .25f;
            float direction = transform.position.x >= source.x ? 1f : -1f;
            Body.linearVelocity = new Vector2(direction * 6f, 8f);
            LevelManager.Instance.PlayerHurt();
        }

        // Quicar depois de pisar num inimigo.
        public void Bounce()
        {
            Body.linearVelocity = new Vector2(Body.linearVelocity.x, jumpSpeed * .75f);
        }

        public void Respawn(Vector2 position)
        {
            Body.position = position;
            transform.position = position;
            Body.linearVelocity = Vector2.zero;
            invulnerableUntil = Time.time + GameRules.InvulnerableTime;
        }

        public void TeleportTo(Vector2 position)
        {
            Body.position = position;
            transform.position = position;
            Body.linearVelocity = Vector2.zero;
        }

        public void ClearInvulnerability() => invulnerableUntil = 0f;
        public void MakeInvulnerable(float seconds) => invulnerableUntil = Time.time + seconds;
    }
}
