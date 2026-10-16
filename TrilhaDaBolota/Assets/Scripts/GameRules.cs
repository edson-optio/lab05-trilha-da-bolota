using UnityEngine;

namespace TrilhaDaBolota
{
    // Números do jogo num lugar só, para ajustar a dificuldade sem caçar valores pelo código.
    public static class GameRules
    {
        public const string MenuScene = "Menu";
        public const string EndScene = "Fim";
        public static readonly string[] PhaseScenes = { "Fase1", "Fase2" };
        public static readonly string[] PhaseNames = { "Bosque do Girassol", "Colina das Nuvens" };

        public const int InitialLives = 3;
        public const int MaxLives = 5;
        public const int AcornPoints = 10;
        public const int StrawberryPoints = 50;
        public const int HeartPoints = 20;
        public const int StompPoints = 100;
        public const int PhaseBonus = 200;

        public const float InvulnerableTime = 1.5f;
        public const float FallMarginBelowMap = 3f;

        // Câmera: 64 pixels de tela por bloco de 16 px (escala 4x, sem borrar a pixel art).
        public const float CameraSize = 5.625f;
        public const float TargetAspect = 16f / 9f;

        public static readonly Color Sky = new Color32(136, 194, 246, 255);
        public static readonly Color Ink = new Color32(44, 26, 18, 255);
        public static readonly Color Cream = new Color32(255, 241, 208, 255);
        public static readonly Color Gold = new Color32(255, 212, 92, 255);
        public static readonly Color Leaf = new Color32(92, 168, 70, 255);
    }
}
