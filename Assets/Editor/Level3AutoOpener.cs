#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class Level3AutoOpener
{
    static Level3AutoOpener()
    {
        EditorApplication.delayCall += CheckAndOpenLevel3;
    }

    private static void CheckAndOpenLevel3()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.Log("[Level3AutoOpener] Loading/Reloading Assets/Scenes/Level3.unity in Unity Editor...");
            EditorSceneManager.OpenScene("Assets/Scenes/Level3.unity", OpenSceneMode.Single);
        }
    }

    [MenuItem("TimeLoop/Open Level 3 Scene")]
    public static void OpenLevel3()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Level3.unity", OpenSceneMode.Single);
    }

    [MenuItem("TimeLoop/Force Asset Refresh")]
    public static void RefreshAssets()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }
}
#endif
