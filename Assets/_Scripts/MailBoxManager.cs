using System.Collections.Generic;
using UnityEngine;

public class MailBoxManager : MonoBehaviour {
    [SerializeField] private List<MailData> mailDataList = new List<MailData>();
    [field: SerializeField] private Dictionary<MailData, GameObject> mailGameObjects { get; } = new Dictionary<MailData, GameObject>();
    [SerializeField] private Transform mailContainer;
    [SerializeField] private MailSelectedController mailSelectedController;

    private void OnEnable() {
        this.mailSelectedController.selectedMailDeleted.AddListener(OnDeleteMail);
    }

    private void OnDisable() {
        this.mailSelectedController.selectedMailDeleted.RemoveListener(OnDeleteMail);
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
            MailController mailController = Instantiate(mailPrefab);
            mailController.gameObject.transform.SetParent(this.mailContainer, false);
            mailController.Initilize(mailData);
            mailController.mailButton.onClick.AddListener(() => OnMailSelected(mailData));

            this.mailGameObjects.Add(mailData, mailController.gameObject);
        }

        Debug.Log($"MailBoxManager initialized with {this.mailDataList.Count} mails.");
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
            MailController mailPrefab = AssetReferencesSO.Instance.GetReference<MailController>();
            InstantiateMailUI(newMailData, mailPrefab);
        }
    }

    public void AddMail(MailData newMailData) {
        if (!mailDataList.Contains(newMailData)) {
            mailDataList.Add(newMailData);
        }
        if (!mailGameObjects.ContainsKey(newMailData)) {
            MailController mailPrefab = AssetReferencesSO.Instance.GetReference<MailController>();
            InstantiateMailUI(newMailData, mailPrefab);
        }
    }

    private void InstantiateMailUI(MailData mailData, MailController prefab) {
        MailController mailController = Instantiate(prefab);
        mailController.gameObject.transform.SetParent(this.mailContainer, false);

        mailController.transform.localScale = Vector3.one;

        mailController.Initilize(mailData);
        mailController.mailButton.onClick.AddListener(() => OnMailSelected(mailData));

        this.mailGameObjects.Add(mailData, mailController.gameObject);
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