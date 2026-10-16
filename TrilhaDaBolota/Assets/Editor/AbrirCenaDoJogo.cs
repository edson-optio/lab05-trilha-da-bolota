using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TrilhaDaBolota.Editor
{
    // Ao abrir o projeto no Unity, se o editor estiver numa cena vazia (Untitled),
    // abre o Menu para que o Play comece pela tela inicial.
    [InitializeOnLoad]
    static class AbrirCenaDoJogo
    {
        const string Cena = "Assets/Scenes/Menu.unity";

        static AbrirCenaDoJogo()
        {
            EditorApplication.delayCall += Abrir;
        }

        static void Abrir()
        {
            if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode) return;
            var atual = EditorSceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(atual.path) || atual.isDirty) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Cena) == null) return;
            EditorSceneManager.OpenScene(Cena);
        }
    }
}
