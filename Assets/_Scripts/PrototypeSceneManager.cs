using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PrototypeSceneManager : MonoBehaviour
{
    [SerializeField] private Button MainGameButton;

    public void LoadMainGame()
    {
        SceneManager.LoadScene("Demo_StateMachine");
    }
}
