using UnityEngine;

namespace TrilhaDaBolota
{
    // Mantém a área de jogo em 16:9 com barras pretas se a janela tiver outra proporção.
    [RequireComponent(typeof(Camera))]
    public class FixedViewport : MonoBehaviour
    {
        Camera targetCamera;

        void Awake()
        {
            targetCamera = GetComponent<Camera>();
            ApplyViewport();
        }

        void OnPreCull() => ApplyViewport();

        void ApplyViewport()
        {
            float windowAspect = Screen.width / (float)Mathf.Max(1, Screen.height);
            float scale = windowAspect / GameRules.TargetAspect;
            if (scale < 1f)
                targetCamera.rect = new Rect(0f, (1f - scale) * .5f, 1f, scale);
            else
            {
                float width = 1f / scale;
                targetCamera.rect = new Rect((1f - width) * .5f, 0f, width, 1f);
            }
        }
    }
}
