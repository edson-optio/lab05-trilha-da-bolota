using System.Collections;
using UnityEngine;

namespace TrilhaDaBolota
{
    // Regras de uma fase: conta as bolotas, aplica pontos e vidas, trata quedas,
    // pausa e decide quando a fase acaba (vitória da fase ou derrota).
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        public int phaseIndex;
        public PlayerController player;
        public GameHUD hud;
        public GameAudio sound;
        public Sprite[] sparkleFrames;
        [Tooltip("Abaixo desta altura o esquilo caiu num buraco.")]
        public float fallLimitY = -3f;

        public GameAudio Sound => sound;
        public int AcornsTotal { get; private set; }
        public int AcornsCollected { get; private set; }
        public bool IsEnding { get; private set; }
        public bool Paused { get; private set; }
        public Vector2 Checkpoint { get; private set; }

        void Awake()
        {
            Instance = this;
            GameSession.EnsureStarted(phaseIndex);
            Time.timeScale = 1f;
            hud.Build(phaseIndex);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            foreach (var item in FindObjectsByType<Collectible>(FindObjectsSortMode.None))
                if (item.Required) AcornsTotal++;
            Checkpoint = player.transform.position;
            hud.ShowBanner("FASE " + (phaseIndex + 1), GameRules.PhaseNames[phaseIndex], 2f);
            RefreshHud();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape)) TogglePause();
            // Usa a posição do corpo físico: logo após o Respawn o transform interpolado ainda
            // mostra a posição antiga por um quadro, e a queda seria contada duas vezes.
            if (!IsEnding && player.Body.position.y < fallLimitY) PlayerFell();
        }

        public void TogglePause()
        {
            if (IsEnding) return;
            Paused = !Paused;
            Time.timeScale = Paused ? 0f : 1f;
            hud.SetPaused(Paused);
        }

        public void Collect(Collectible item)
        {
            switch (item.type)
            {
                case CollectibleType.Bolota:
                    AcornsCollected++;
                    GameSession.AddAcorn();
                    GameSession.AddScore(GameRules.AcornPoints);
                    sound.PlayCoin();
                    break;
                case CollectibleType.Morango:
                    GameSession.AddScore(GameRules.StrawberryPoints);
                    sound.PlayFruit();
                    break;
                case CollectibleType.Coracao:
                    GameSession.AddLife();
                    GameSession.AddScore(GameRules.HeartPoints);
                    sound.PlayHeart();
                    break;
            }
            Sparkle(item.transform.position);
            RefreshHud();
            if (AcornsCollected >= AcornsTotal && !IsEnding) StartCoroutine(CompletePhase());
        }

        public void EnemyDefeated(Enemy enemy)
        {
            GameSession.AddScore(enemy.points);
            sound.PlayStomp();
            Sparkle(enemy.transform.position + Vector3.up * .4f);
            RefreshHud();
        }

        public void PlayerHurt()
        {
            sound.PlayHurt();
            LoseLife();
        }

        public void PlayerFell()
        {
            sound.PlayHurt();
            LoseLife();
            if (!IsEnding) player.Respawn(Checkpoint);
        }

        public void SetCheckpoint(Vector2 position)
        {
            if (position.x > Checkpoint.x) Checkpoint = position + Vector2.up * .2f;
        }

        void LoseLife()
        {
            GameSession.LoseLife();
            RefreshHud();
            if (GameSession.Lives <= 0) StartCoroutine(GameOver());
        }

        IEnumerator CompletePhase()
        {
            IsEnding = true;
            GameSession.AddScore(GameRules.PhaseBonus);
            RefreshHud();
            sound.PlayWin();
            bool last = phaseIndex == GameRules.PhaseScenes.Length - 1;
            hud.ShowBanner(last ? "TODAS AS BOLOTAS!" : "FASE CONCLUÍDA!", "+" + GameRules.PhaseBonus + " pontos de bônus", 2f);
            yield return new WaitForSecondsRealtime(1.8f);
            GameSession.FinishPhase();
        }

        IEnumerator GameOver()
        {
            IsEnding = true;
            sound.PlayLose();
            player.gameObject.SetActive(false);
            hud.ShowBanner("SEM VIDAS", "", 2f);
            yield return new WaitForSecondsRealtime(1.4f);
            GameSession.EndGame(false);
        }

        void RefreshHud() => hud.Refresh(GameSession.Score, GameSession.Lives, AcornsCollected, AcornsTotal);

        void Sparkle(Vector3 position)
        {
            var sparkle = new GameObject("Brilho", typeof(SpriteRenderer));
            sparkle.transform.position = position;
            sparkle.GetComponent<SpriteRenderer>().sortingOrder = 30;
            var animator = sparkle.AddComponent<SpriteAnimator>();
            animator.loop = false;
            animator.destroyWhenDone = true;
            animator.Play(sparkleFrames, 14f);
        }
    }
}
