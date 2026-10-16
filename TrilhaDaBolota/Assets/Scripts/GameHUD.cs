using UnityEngine;
using UnityEngine.UI;

namespace TrilhaDaBolota
{
    // HUD da fase: pontos, vidas (corações), bolotas coletadas, nome da fase e avisos no centro.
    public class GameHUD : MonoBehaviour
    {
        public Sprite heartIcon;
        public Sprite acornIcon;

        Text scoreLabel, acornLabel, bannerTitle, bannerSubtitle;
        Image[] hearts;
        GameObject banner, pausePanel;
        float bannerUntil;

        public string ScoreText => scoreLabel.text;
        public string AcornText => acornLabel.text;
        public string BannerTitle => banner.activeSelf ? bannerTitle.text : "";
        public bool PauseVisible => pausePanel.activeSelf;
        public int HeartsShown
        {
            get
            {
                int count = 0;
                foreach (var heart in hearts) if (heart.enabled) count++;
                return count;
            }
        }

        public void Build(int phaseIndex)
        {
            var root = UiKit.Canvas("Interface");
            root.SetParent(transform, false);
            Vector2 topLeft = new Vector2(0f, 1f), topRight = new Vector2(1f, 1f), center = new Vector2(.5f, .5f);

            UiKit.Panel(root, topLeft, new Vector2(16f, -16f), new Vector2(300f, 128f), new Color(.17f, .1f, .07f, .55f));
            scoreLabel = UiKit.Label(root, "", 30, topLeft, new Vector2(32f, -22f), new Vector2(280f, 40f), TextAnchor.MiddleLeft);

            hearts = new Image[GameRules.MaxLives];
            for (int i = 0; i < hearts.Length; i++)
                hearts[i] = UiKit.Picture(root, heartIcon, topLeft, new Vector2(30f + i * 42f, -66f), new Vector2(40f, 40f));

            UiKit.Picture(root, acornIcon, topLeft, new Vector2(28f, -104f), new Vector2(40f, 40f));
            acornLabel = UiKit.Label(root, "", 28, topLeft, new Vector2(74f, -104f), new Vector2(220f, 40f), TextAnchor.MiddleLeft);

            UiKit.Label(root, "FASE " + (phaseIndex + 1) + "  ·  " + GameRules.PhaseNames[phaseIndex], 26, topRight,
                new Vector2(-24f, -22f), new Vector2(600f, 40f), TextAnchor.MiddleRight);
            UiKit.Label(root, "P / Esc: pausa", 20, topRight, new Vector2(-24f, -58f), new Vector2(400f, 30f), TextAnchor.MiddleRight);

            banner = UiKit.Box("Aviso", root, center, new Vector2(0f, 120f), new Vector2(900f, 160f)).gameObject;
            bannerTitle = UiKit.Label(banner.transform, "", 64, center, new Vector2(0f, 24f), new Vector2(900f, 80f), color: GameRules.Gold);
            bannerSubtitle = UiKit.Label(banner.transform, "", 32, center, new Vector2(0f, -40f), new Vector2(900f, 50f));
            banner.SetActive(false);

            pausePanel = UiKit.Panel(root, center, Vector2.zero, new Vector2(4000f, 4000f), new Color(0f, 0f, 0f, .5f)).gameObject;
            UiKit.Label(pausePanel.transform, "PAUSA", 72, center, new Vector2(0f, 30f), new Vector2(600f, 90f), color: GameRules.Gold);
            UiKit.Label(pausePanel.transform, "Aperte P ou Esc para continuar", 30, center, new Vector2(0f, -40f), new Vector2(700f, 50f));
            pausePanel.SetActive(false);
        }

        public void Refresh(int score, int lives, int acorns, int acornsTotal)
        {
            scoreLabel.text = "PONTOS  " + score.ToString("D6");
            for (int i = 0; i < hearts.Length; i++) hearts[i].enabled = i < lives;
            acornLabel.text = acorns + " / " + acornsTotal;
        }

        public void ShowBanner(string title, string subtitle, float seconds)
        {
            bannerTitle.text = title;
            bannerSubtitle.text = subtitle;
            banner.SetActive(true);
            bannerUntil = Time.unscaledTime + seconds;
        }

        public void SetPaused(bool paused) => pausePanel.SetActive(paused);

        void Update()
        {
            if (banner != null && banner.activeSelf && Time.unscaledTime > bannerUntil) banner.SetActive(false);
        }
    }
}
