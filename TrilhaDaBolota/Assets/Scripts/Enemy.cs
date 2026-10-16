using UnityEngine;

namespace TrilhaDaBolota
{
    // Base dos inimigos. O collider é um gatilho (trigger): encostar no esquilo causa dano,
    // mas se o esquilo cair por cima o inimigo é derrotado e o esquilo quica.
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public abstract class Enemy : MonoBehaviour
    {
        public int points = GameRules.StompPoints;

        protected Rigidbody2D body;
        protected SpriteRenderer spriteRenderer;
        Collider2D hitbox;

        protected virtual void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            hitbox = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void OnTriggerEnter2D(Collider2D other) => Touch(other);
        void OnTriggerStay2D(Collider2D other) => Touch(other);

        void Touch(Collider2D other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player == null || !enabled) return;

            // Pisão: o esquilo está descendo e os pés estão acima do meio do inimigo.
            bool falling = player.Body.linearVelocity.y <= .5f;
            bool above = player.Hitbox.bounds.min.y > hitbox.bounds.center.y;
            if (falling && above)
            {
                player.Bounce();
                Defeat();
            }
            else
            {
                player.Hurt(transform.position);
            }
        }

        public void Defeat()
        {
            enabled = false;
            LevelManager.Instance.EnemyDefeated(this);
            Destroy(gameObject);
        }
    }
}
