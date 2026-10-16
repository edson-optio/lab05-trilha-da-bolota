using UnityEngine;

namespace TrilhaDaBolota
{
    // Animação quadro a quadro: troca o sprite do SpriteRenderer numa taxa fixa.
    // Usado pelo esquilo, inimigos, itens e pelo brilho de coleta.
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnimator : MonoBehaviour
    {
        public Sprite[] frames;
        public float framesPerSecond = 6f;
        public bool loop = true;
        public bool destroyWhenDone;

        SpriteRenderer spriteRenderer;
        float clock;

        public int FrameIndex { get; private set; }

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            // Começa num instante aleatório para objetos iguais não piscarem em sincronia.
            if (loop) clock = Random.value * 10f;
        }

        // Troca de animação sem reiniciar se ela já estiver tocando.
        public void Play(Sprite[] newFrames, float fps)
        {
            framesPerSecond = fps;
            if (frames == newFrames) return;
            frames = newFrames;
            clock = 0f;
            Apply(0);
        }

        void Update()
        {
            if (frames == null || frames.Length == 0) return;
            clock += Time.deltaTime;
            int index = Mathf.FloorToInt(clock * framesPerSecond);
            if (!loop && index >= frames.Length)
            {
                if (destroyWhenDone) Destroy(gameObject);
                return;
            }
            Apply(index % frames.Length);
        }

        void Apply(int index)
        {
            FrameIndex = index;
            spriteRenderer.sprite = frames[index];
        }
    }
}
