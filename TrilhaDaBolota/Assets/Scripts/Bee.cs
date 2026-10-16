using UnityEngine;

namespace TrilhaDaBolota
{
    // Abelha: voa de um lado para o outro em volta do ponto inicial, balançando para cima e para baixo.
    public class Bee : Enemy
    {
        public float patrolDistance = 2.5f;
        public float patrolSpeed = 1.6f;
        public float bobHeight = .45f;

        Vector2 origin;
        float phase;

        void Start()
        {
            origin = body.position;
            phase = Random.value * Mathf.PI * 2f;
        }

        void FixedUpdate()
        {
            float t = Time.time * patrolSpeed / patrolDistance + phase;
            float x = Mathf.Sin(t) * patrolDistance;
            float y = Mathf.Sin(t * 2.3f) * bobHeight;
            body.MovePosition(origin + new Vector2(x, y));
            // Olha para o lado em que está voando (derivada do seno = cosseno).
            spriteRenderer.flipX = Mathf.Cos(t) < 0f;
        }
    }
}
