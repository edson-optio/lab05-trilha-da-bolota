using UnityEngine;

namespace TrilhaDaBolota
{
    // Câmera que segue o esquilo suavemente sem mostrar o lado de fora do mapa.
    // Sem alvo (menu e tela final), ela só desliza para a direita mostrando o parallax.
    [DefaultExecutionOrder(-50)] // move antes do Parallax ler a posição dela
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public float smoothTime = .15f;
        public Vector2 lookAhead = new Vector2(1.5f, 1f);
        public Vector2 min = new Vector2(-100000f, -100000f);
        public Vector2 max = new Vector2(100000f, 100000f);
        public float autoScrollSpeed;

        Vector3 velocity;

        void LateUpdate()
        {
            Vector3 position = transform.position;
            if (target != null)
            {
                float facing = target.GetComponent<SpriteRenderer>().flipX ? -1f : 1f;
                Vector3 goal = target.position + new Vector3(lookAhead.x * facing, lookAhead.y, 0f);
                goal.z = position.z;
                position = Vector3.SmoothDamp(position, goal, ref velocity, smoothTime);
            }
            else
            {
                position.x += autoScrollSpeed * Time.deltaTime;
            }
            position.x = Mathf.Clamp(position.x, min.x, max.x);
            position.y = Mathf.Clamp(position.y, min.y, max.y);
            transform.position = position;
        }

        public void SnapToTarget()
        {
            if (target == null) return;
            velocity = Vector3.zero;
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
            LateUpdate();
        }
    }
}
