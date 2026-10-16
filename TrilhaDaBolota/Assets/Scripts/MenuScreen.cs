using UnityEngine;
using UnityEngine.UI;

namespace TrilhaDaBolota
{
    // Tela inicial: história, instruções de como jogar e o botão para começar.
    public class MenuScreen : MonoBehaviour
    {
        public Sprite squirrel, acorn, strawberry, heart, snail, bee;

        public Button PlayButton { get; private set; }
        public string TitleText { get; private set; }
        public string InstructionsText { get; private set; }

        void Start()
        {
            Time.timeScale = 1f;
            var root = UiKit.Canvas("Menu");
            Vector2 top = new Vector2(.5f, 1f), center = new Vector2(.5f, .5f);

            UiKit.Panel(root, center, new Vector2(0f, 0f), new Vector2(1180f, 660f), new Color(.17f, .1f, .07f, .72f));
            TitleText = "A TRILHA DA BOLOTA";
            UiKit.Label(root, TitleText, 76, top, new Vector2(0f, -48f), new Vector2(1100f, 96f), color: GameRules.Gold);
            UiKit.Picture(root, squirrel, top, new Vector2(-470f, -48f), new Vector2(96f, 96f));
            UiKit.Picture(root, acorn, top, new Vector2(470f, -56f), new Vector2(72f, 72f));
            UiKit.Label(root, "O inverno está chegando e o vento espalhou as bolotas do esquilo Tito pela floresta.\n" +
                              "Ajude Tito a juntar todas antes que a neve caia!", 26, top, new Vector2(0f, -160f), new Vector2(1060f, 80f));

            InstructionsText =
                "COMO JOGAR\n" +
                "Setas (esquerda / direita) ou A / D : andar\n" +
                "Espaço, W, seta para cima ou Z : pular\n(segure o botão para pular mais alto)\n" +
                "P ou Esc : pausar\n\n" +
                "Junte TODAS as bolotas de cada fase para avançar.\n" +
                "Pule em cima das lesmas e abelhas para derrotá-las.\n" +
                "Encostar nelas ou cair num buraco tira uma vida.";
            UiKit.Label(root, InstructionsText, 25, top, new Vector2(-170f, -248f), new Vector2(720f, 300f), TextAnchor.UpperLeft);

            // Legenda dos itens e inimigos com os próprios sprites do jogo.
            Vector2 legend = new Vector2(330f, -262f);
            Legend(root, acorn, "Bolota  +10  (obrigatória)", legend);
            Legend(root, strawberry, "Morango  +50", legend + new Vector2(0f, -52f));
            Legend(root, heart, "Coração  +1 vida", legend + new Vector2(0f, -104f));
            Legend(root, snail, "Lesma  +100 no pisão", legend + new Vector2(0f, -156f));
            Legend(root, bee, "Abelha  +100 no pisão", legend + new Vector2(0f, -208f));

            PlayButton = UiKit.Button(root, "JOGAR  (Enter)", new Vector2(.5f, 0f), new Vector2(0f, 56f), GameSession.StartNewGame);
        }

        static void Legend(Transform root, Sprite icon, string text, Vector2 position)
        {
            Vector2 top = new Vector2(.5f, 1f);
            UiKit.Picture(root, icon, top, position + new Vector2(-150f, 0f), new Vector2(44f, 44f));
            UiKit.Label(root, text, 24, top, position + new Vector2(70f, 0f), new Vector2(380f, 44f), TextAnchor.MiddleLeft);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) GameSession.StartNewGame();
        }
    }
}
