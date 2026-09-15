using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MailBoxManager : MonoBehaviour {
    [SerializeField] private List<Mail> mails = new List<Mail>();
    public Dictionary<Mail, GameObject> mailGameObjects { get; } = new Dictionary<Mail, GameObject>();
    private AssetReferencesSO assetReferencesSO;

    public UnityEvent<Mail> onMailSelected = new UnityEvent<Mail>();

    private void Start() {
        this.assetReferencesSO = AssetReferencesSO.Instance;

        MailController mailPrefab = assetReferencesSO.GetReference<MailController>();
        foreach (Mail mail in mails) {
            MailController mailController = Instantiate(mailPrefab);
            mailController.gameObject.transform.SetParent(transform, false);
            mailController.Initilize(mail);
            mailController.mailButton.onClick.AddListener(() => OnMailSelected(mail));

            this.mailGameObjects.Add(mail, mailController.gameObject);
        }
    }

    private void OnMailSelected(Mail mail) {
        this.onMailSelected.Invoke(mail);
    }

    public void OnDeleteMail(Mail mail) {
        if (this.mailGameObjects.TryGetValue(mail, out GameObject goMail)) {
            goMail.SetActive(false);
        } else {
            Debug.LogWarning($"Mail {mail.name} not found in mailGameObjects dictionary.");
        }
    }
}
