using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author: https://github.com/iHateThisName
/// </summary>

public class CalenderUIController : MonoBehaviour {

    // Buttons
    [Header("Day Buttons")]
    [SerializeField] private Button dayOneMondayButton;
    [SerializeField] private Button dayTwoTuesdayButton;
    [SerializeField] private Button dayThreeWednesdayButton;
    [SerializeField] private Button dayFourThursdayButton;
    [SerializeField] private Button dayFiveFridayButton;

    [Header("Information panel")]
    [SerializeField] private TMP_Text titleTextInformation;
    [SerializeField] private TMP_Text descriptionTextInformation;
    [SerializeField] private Button startDayButton;

    private int currentSelectedDay = 0; // 0 means no day selected, 1-5 correspond to Monday-Friday, and -1 means an unknown button was pressed.

    private void OnEnable() {
        this.startDayButton?.onClick.AddListener(() => OnStartDayClicked());

        // Days
        this.dayOneMondayButton?.onClick.AddListener(() => OnDayButtonClicked(dayOneMondayButton));
        this.dayTwoTuesdayButton?.onClick.AddListener(() => OnDayButtonClicked(dayTwoTuesdayButton));
        this.dayThreeWednesdayButton?.onClick.AddListener(() => OnDayButtonClicked(dayThreeWednesdayButton));
        this.dayFourThursdayButton?.onClick.AddListener(() => OnDayButtonClicked(dayFourThursdayButton));
        this.dayFiveFridayButton?.onClick.AddListener(() => OnDayButtonClicked(dayFiveFridayButton));
    }

    private void OnDisable() {
        this.startDayButton?.onClick.RemoveAllListeners();

        // Days
        this.dayOneMondayButton?.onClick.RemoveAllListeners();
        this.dayTwoTuesdayButton?.onClick.RemoveAllListeners();
        this.dayThreeWednesdayButton?.onClick.RemoveAllListeners();
        this.dayFourThursdayButton?.onClick.RemoveAllListeners();
        this.dayFiveFridayButton?.onClick.RemoveAllListeners();
    }

    private void Start() {
        OnDayButtonClicked(this.dayTwoTuesdayButton); // Default to Monday on start.
    }
    private void OnStartDayClicked() {
        switch (currentSelectedDay) {
            case 1:
                Debug.Log("Starting Monday's level...");
                // Load Monday's level here
                GameSceneManager.Instance.LoadScene(GameSceneManager.Instance.levelOneScene);
                break;

            case 2:
                Debug.Log("Starting Tuesday's level...");
                // Load Tuesday's level here
                GameSceneManager.Instance.LoadScene(GameSceneManager.Instance.levelTwoScene);
                break;

            case 3:
                Debug.Log("Starting Wednesday's level...");
                // Load Wednesday's level here
                //GameSceneManager.Instance.LoadScene(GameSceneManager.Instance.levelThreeScene);
                break;

            case 4:
                Debug.Log("Starting Thursday's level...");
                // Load Thursday's level here
                //GameSceneManager.Instance.LoadScene(GameSceneManager.Instance.levelFourScene);
                break;

            case 5:
                Debug.Log("Starting Friday's level...");
                // Load Friday's level here
                //GameSceneManager.Instance.LoadScene(GameSceneManager.Instance.levelFiveScene);
                break;

            default:
                Debug.LogWarning("No day selected or unknown day. Cannot start the level.");
                break;
        }
    }

    private void OnDayButtonClicked(Button dayButton) {
        int dayNumber = GetDayNumberFromButton(dayButton);
        if (dayNumber == -1) return; // Unknown button, do nothing.

        StringBuilder information = new StringBuilder();
        switch (dayNumber) {
            case 1:
                this.titleTextInformation.text = "Monday";
                information.AppendLine("This is the information for Monday.");
                break;
            case 2:
                this.titleTextInformation.text = "Tuesday";
                information.AppendLine("This is the information for Tuesday.");
                break;
            case 3:
                this.titleTextInformation.text = "Wednesday";
                information.AppendLine("This is the information for Wednesday.");
                break;
            case 4:
                this.titleTextInformation.text = "Thursday";
                information.AppendLine("This is the information for Thursday.");
                break;
            case 5:
                this.titleTextInformation.text = "Friday";
                information.AppendLine("This is the information for Friday.");
                break;
            default:
                Debug.LogWarning("Unknown day number: " + dayNumber);
                ResetInformationPanel();
                return;
        }

        this.descriptionTextInformation.text = information.ToString();
        this.currentSelectedDay = dayNumber;

        Debug.Log($"Day button clicked: {dayButton.name}, Day number: {dayNumber}");
    }
    private int GetDayNumberFromButton(Button button) {
        if (button == dayOneMondayButton) return 1;
        else if (button == dayTwoTuesdayButton) return 2;
        else if (button == dayThreeWednesdayButton) return 3;
        else if (button == dayFourThursdayButton) return 4;
        else if (button == dayFiveFridayButton) return 5;

        // Button does not match any known buttons, log a warning and return -1
        Debug.LogWarning("Unknown button pressed: " + button.name);
        return -1; // Return -1 for unknown buttons
    }

    private void ResetInformationPanel() {
        this.titleTextInformation.text = "Select a day to view information.";
        this.descriptionTextInformation.text = "";
    }
}
