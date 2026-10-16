using UnityEngine;

namespace TrilhaDaBolota
{
    // Efeitos sonoros sintetizados no código (sem arquivos de áudio externos).
    [RequireComponent(typeof(AudioSource))]
    public class GameAudio : MonoBehaviour
    {
        AudioSource source;
        AudioClip jump, coin, fruit, heart, stomp, hurt, win, lose;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = .3f;
            jump = MakeTone("Pulo", 330f, 660f, .12f, .22f);
            coin = MakeTone("Bolota", 880f, 1320f, .09f, .25f);
            fruit = MakeTone("Morango", 660f, 1100f, .16f, .25f);
            heart = MakeTone("Coração", 520f, 1560f, .35f, .25f);
            stomp = MakeTone("Pisão", 240f, 120f, .12f, .35f);
            hurt = MakeTone("Dano", 200f, 70f, .3f, .35f);
            win = MakeTone("Vitória", 520f, 1560f, .6f, .25f);
            lose = MakeTone("Derrota", 300f, 90f, .7f, .3f);
        }

        public void PlayJump() => source.PlayOneShot(jump);
        public void PlayCoin() => source.PlayOneShot(coin);
        public void PlayFruit() => source.PlayOneShot(fruit);
        public void PlayHeart() => source.PlayOneShot(heart);
        public void PlayStomp() => source.PlayOneShot(stomp);
        public void PlayHurt() => source.PlayOneShot(hurt);
        public void PlayWin() => source.PlayOneShot(win);
        public void PlayLose() => source.PlayOneShot(lose);

        // Onda quadrada suave com a frequência deslizando do início ao fim (som de "8 bits").
        static AudioClip MakeTone(string name, float startFrequency, float endFrequency, float duration, float amplitude)
        {
            const int sampleRate = 44100;
            int count = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[count];
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                float progress = i / (float)count;
                phase += Mathf.PI * 2f * Mathf.Lerp(startFrequency, endFrequency, progress) / sampleRate;
                float square = Mathf.Clamp(Mathf.Sin(phase) * 3f, -1f, 1f);
                float envelope = Mathf.Min(1f, progress * 30f) * (1f - progress);
                samples[i] = square * envelope * amplitude;
            }
            var clip = AudioClip.Create(name, count, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
