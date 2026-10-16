using UnityEngine;

namespace TrilhaDaBolota
{
    // Parallax scrolling: cada camada de fundo acompanha uma fração do movimento da câmera.
    //   parallaxEffect = 1 -> anda junto com a câmera (parece infinitamente longe, ex.: céu)
    //   parallaxEffect = 0 -> fica parada no mundo (como o chão da fase)
    // A imagem é desenhada repetida (3 cópias lado a lado) e "pula" uma largura quando a
    // câmera passa da borda, então o fundo nunca acaba.
    [RequireComponent(typeof(SpriteRenderer))]
    public class Parallax : MonoBehaviour
    {
        [Range(0f, 1f)] public float parallaxEffect = .5f;
        [Range(0f, 1f)] public float verticalEffect = .5f;

        Transform cameraTransform;
        Vector2 startPosition;
        Vector2 cameraStart;
        float length;

        public float Length => length;

        void Start()
        {
            cameraTransform = Camera.main.transform;
            startPosition = transform.position;
            cameraStart = cameraTransform.position;
            // A imagem desenhada tem 3 larguras; uma repetição é 1/3 disso.
            length = GetComponent<SpriteRenderer>().size.x / 3f;
        }

        void LateUpdate()
        {
            Vector2 cameraMove = (Vector2)cameraTransform.position - cameraStart;
            float distance = cameraMove.x * parallaxEffect;
            float relative = cameraMove.x * (1f - parallaxEffect); // quanto a câmera "andou" sobre esta camada

            transform.position = new Vector3(startPosition.x + distance,
                startPosition.y + cameraMove.y * verticalEffect, transform.position.z);

            if (relative > startPosition.x - cameraStart.x + length) startPosition.x += length;
            else if (relative < startPosition.x - cameraStart.x - length) startPosition.x -= length;
        }
    }
}
