using UnityEngine.SceneManagement;

namespace TrilhaDaBolota
{
    // Dados que atravessam as cenas: pontos, vidas e bolotas da partida inteira.
    // Também concentra a troca de cenas (menu -> fase 1 -> fase 2 -> fim).
    public static class GameSession
    {
        public static bool Started { get; private set; }
        public static int Score { get; private set; }
        public static int Lives { get; private set; }
        public static int AcornsCollected { get; private set; }
        public static int PhaseIndex { get; private set; }
        public static bool Victory { get; private set; }

        public static void NewGame()
        {
            Started = true;
            Score = 0;
            Lives = GameRules.InitialLives;
            AcornsCollected = 0;
            PhaseIndex = 0;
            Victory = false;
        }

        // Permite apertar Play direto numa fase dentro do editor.
        public static void EnsureStarted(int phaseIndex)
        {
            if (Started) return;
            NewGame();
            PhaseIndex = phaseIndex;
        }

        public static void AddScore(int points) => Score += points;
        public static void AddAcorn() => AcornsCollected++;
        public static void AddLife() { if (Lives < GameRules.MaxLives) Lives++; }
        public static void LoseLife() { if (Lives > 0) Lives--; }

        public static void StartNewGame()
        {
            NewGame();
            SceneManager.LoadScene(GameRules.PhaseScenes[0]);
        }

        // Fase concluída: vai para a próxima ou, se era a última, para a vitória.
        public static void FinishPhase()
        {
            PhaseIndex++;
            if (PhaseIndex < GameRules.PhaseScenes.Length)
                SceneManager.LoadScene(GameRules.PhaseScenes[PhaseIndex]);
            else
                EndGame(true);
        }

        public static void EndGame(bool victory)
        {
            Victory = victory;
            SceneManager.LoadScene(GameRules.EndScene);
        }

        public static void GoToMenu()
        {
            Started = false;
            SceneManager.LoadScene(GameRules.MenuScene);
        }
    }
}
