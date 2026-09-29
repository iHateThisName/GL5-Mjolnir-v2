using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PrototypeSceneManager : MonoBehaviour
{
    [SerializeField] private Button MainGameButton;
    [SerializeField] private SceneReference DemoStateMachine;

    public void LoadMainGame()
    {
        SceneManager.LoadScene(DemoStateMachine.Name);
    }
}
