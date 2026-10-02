using System.Collections.Generic;
using UnityEngine;

public class MailBoxManager : MonoBehaviour {
    [SerializeField] private List<MailData> mailDataList = new List<MailData>();
    [field: SerializeField] private Dictionary<MailData, GameObject> mailGameObjects { get; } = new Dictionary<MailData, GameObject>();
    [SerializeField] private Transform mailContainer;
    [SerializeField] private MailSelectedController mailSelectedController;

    private void OnEnable() {
        this.mailSelectedController.selectedMailDeleted.AddListener(OnDeleteMail);
        SituationManager.Instance.OnSituationStateChange += OnSituationStateChange;
    }

    private void OnDisable() {
        this.mailSelectedController.selectedMailDeleted.RemoveListener(OnDeleteMail);
        SituationManager.Instance.OnSituationStateChange -= OnSituationStateChange;
    }

    private void Awake() {
        if (this.mailSelectedController == null) {
            this.mailSelectedController = transform.parent.GetComponentInChildren<MailSelectedController>();
        }
    }

    private void Start() {
        MailController mailPrefab = AssetReferencesSO.Instance.GetReference<MailController>();

        if (this.mailDataList.Count == 0) return;

        foreach (MailData mailData in this.mailDataList) {
            InstantiateMailUI(mailData);
        }

        Debug.Log($"MailBoxManager initialized with {this.mailDataList.Count} mails.");
    }

    private void OnSituationStateChange(SituationData data) {

        // Check if the sitaution is of type MailData
        if (data is MailData mailData) {

            // Check if this MailBox containst the mailData
            if (this.mailDataList.Contains(mailData)) {

                // This mailbox contains this mail, the SituationStateEnum has been changed and needs to be reflected

                switch (data.SituationStateEnum) {
                    case SituationManager.SituationStateEnum.Inactive:
                        // The mail is Inactive make it not show up
                        this.mailGameObjects[mailData].SetActive(false);

                        break;
                    case SituationManager.SituationStateEnum.Active: {
                            // The mail have been activated need to display it in the mailbox
                            GameObject go = this.mailGameObjects[mailData];
                            go.SetActive(true);
                            break;
                        }

                    case SituationManager.SituationStateEnum.Success:
                        
                        break;
                    case SituationManager.SituationStateEnum.Failed:
                        break;
                }

                Debug.Log($"MailBoxManager: {mailData.SituationName}, Mail is {data.SituationStateEnum}");

            }

        }
    }

    /// <summary>
    /// Called by EventManager to deliver a new email to the player.
    /// </summary>
    public void AddMail(Mail newMail) {

        MailData newMailData = new MailData(newMail.EmailAddress, newMail.EmailSubject, new string[] { newMail.EmailBody }, newMail.EmailType);

        if (!mailDataList.Contains(newMailData)) {
            mailDataList.Add(newMailData);
        }

        if (!mailGameObjects.ContainsKey(newMailData)) {
            InstantiateMailUI(newMailData);
        }
    }

    public void AddMail(MailData newMailData) {
        if (!mailDataList.Contains(newMailData)) {
            mailDataList.Add(newMailData);
        }
        if (!mailGameObjects.ContainsKey(newMailData)) {
            InstantiateMailUI(newMailData);
        }
    }

    private void InstantiateMailUI(MailData mailData) {
        MailController mailPrefab = AssetReferencesSO.Instance.GetReference<MailController>();

        MailController mailController = Instantiate(mailPrefab);
        mailController.gameObject.transform.SetParent(this.mailContainer, false);

        mailController.transform.localScale = Vector3.one;

        mailController.Initilize(mailData);
        mailController.mailButton.onClick.AddListener(() => OnMailSelected(mailData));

        if (mailData.SituationStateEnum == SituationManager.SituationStateEnum.Inactive) {
            mailController.gameObject.SetActive(false);
        }

        this.mailGameObjects.Add(mailData, mailController.gameObject);

        Debug.Log($"{mailData.name} was instantiate and is {mailController.gameObject.activeSelf}");
    }

    private void OnMailSelected(MailData mailData) {
        this.mailSelectedController.SelectMail(mailData);
    }

    private void OnDeleteMail(MailData mailData) {
        if (this.mailGameObjects.TryGetValue(mailData, out GameObject goMail)) {
            goMail.SetActive(false);
        } else {
            Debug.LogWarning($"Mail {mailData.EmailHeader} not found in mailGameObjects dictionary.");
        }
    }
}