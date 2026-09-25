using UnityEngine;

public class IntroController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject introPanel;

    [Tooltip("Drag your 3 text GameObjects here in the order you want them to appear.")]
    [SerializeField] private GameObject[] introTextObjects;

    private int currentIndex = 0;

    private void Start()
    {
        if (introPanel != null) introPanel.SetActive(true);

        // Hide all text objects first, then turn on ONLY the first one
        for (int i = 0; i < introTextObjects.Length; i++)
        {
            if (introTextObjects[i] != null)
            {
                introTextObjects[i].SetActive(i == 0); // True for index 0, False for the rest
            }
        }

        currentIndex = 0;
    }

    /// <summary>
    /// Link this to your "Next" button on the Intro Panel
    /// </summary>
    public void OnNextButtonClicked()
    {
        // Hide the text we were just looking at
        if (currentIndex < introTextObjects.Length && introTextObjects[currentIndex] != null)
        {
            introTextObjects[currentIndex].SetActive(false);
        }

        currentIndex++;

        // Check if there is another text object to show
        if (currentIndex < introTextObjects.Length)
        {
            if (introTextObjects[currentIndex] != null)
            {
                introTextObjects[currentIndex].SetActive(true);
            }
        }
        else
        {
            // We have reached the end of the texts, hide the entire panel!
            if (introPanel != null) introPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}