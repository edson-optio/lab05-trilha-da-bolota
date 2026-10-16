using UnityEngine;
using UnityEngine.UI;

namespace TrilhaDaBolota
{
    // Cena final: informa vitória (todas as bolotas) ou derrota (sem vidas) e oferece reinício.
    public class EndScreen : MonoBehaviour
    {
        public Sprite victoryIcon, defeatIcon;
        public int totalAcorns;

        public Button RestartButton { get; private set; }
        public Button MenuButton { get; private set; }
        public string TitleText { get; private set; }
        public string SummaryText { get; private set; }

        void Start()
        {
            Time.timeScale = 1f;
            bool victory = GameSession.Victory;
            var root = UiKit.Canvas("Fim");
            Vector2 center = new Vector2(.5f, .5f);

            UiKit.Panel(root, center, Vector2.zero, new Vector2(900f, 560f), new Color(.17f, .1f, .07f, .78f));
            TitleText = victory ? "VITÓRIA!" : "FIM DE JOGO";
            UiKit.Label(root, TitleText, 92, center, new Vector2(0f, 190f), new Vector2(860f, 110f),
                color: victory ? GameRules.Gold : new Color32(255, 120, 110, 255));
            UiKit.Picture(root, victory ? victoryIcon : defeatIcon, center, new Vector2(0f, 70f), new Vector2(112f, 112f));

            string message = victory
                ? "Tito juntou todas as bolotas e passará o inverno quentinho!"
                : "As vidas acabaram... as lesmas e abelhas venceram desta vez.";
            SummaryText = message + "\n\nPONTOS  " + GameSession.Score.ToString("D6") +
                          "\nBOLOTAS  " + GameSession.AcornsCollected + " de " + totalAcorns;
            UiKit.Label(root, SummaryText, 30, center, new Vector2(0f, -60f), new Vector2(840f, 160f));

            RestartButton = UiKit.Button(root, "JOGAR NOVAMENTE", center, new Vector2(-190f, -205f), GameSession.StartNewGame);
            MenuButton = UiKit.Button(root, "MENU", center, new Vector2(190f, -205f), GameSession.GoToMenu);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) GameSession.StartNewGame();
        }
    }
}
