using Eflatun.SceneReference;
using UnityEngine;
/// <summary>
/// Author: https://github.com/iHateThisName
/// </summary>
public class GameSceneManager : Singleton<GameSceneManager> {
    [field:SerializeField] public SceneReference mainMenuScene { get; private set; }

    [Header("Level/Day Scenes")]
    [field: SerializeField] public SceneReference levelOneScene { get; private set; }
    [field:SerializeField] public SceneReference levelTwoScene { get; private set; }
    [field:SerializeField] public SceneReference levelThreeScene { get; private set; }

    private SceneReference currentScene;

    public void LoadScene(SceneReference newScene) {
        this.currentScene = newScene;
        UnityEngine.SceneManagement.SceneManager.LoadScene(newScene.Path);
    }
}
