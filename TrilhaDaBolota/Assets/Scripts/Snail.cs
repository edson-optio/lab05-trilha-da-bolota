using UnityEngine;

namespace TrilhaDaBolota
{
    // Lesma: anda devagar no chão e dá meia-volta na parede ou na beirada.
    public class Snail : Enemy
    {
        public float speed = 1.2f;
        public float direction = -1f;

        int groundMask;

        protected override void Awake()
        {
            base.Awake();
            groundMask = LayerMask.GetMask("Chao");
        }

        void FixedUpdate()
        {
            Vector2 position = body.position;
            // Sensores: um raio para a frente (parede) e um para baixo, logo à frente (beirada).
            Vector2 ahead = position + new Vector2(direction * .5f, .3f);
            bool wall = Physics2D.Raycast(ahead, Vector2.right * direction, .1f, groundMask);
            bool floor = Physics2D.Raycast(ahead, Vector2.down, .6f, groundMask);
            if (wall || !floor) direction = -direction;

            body.MovePosition(position + Vector2.right * (direction * speed * Time.fixedDeltaTime));
            // O desenho olha para a direita; vira quando anda para a esquerda.
            spriteRenderer.flipX = direction < 0f;
        }
    }
}
