using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MailBoxManager : MonoBehaviour {
    [SerializeField] private List<Mail> mails = new List<Mail>();
    public Dictionary<Mail, GameObject> mailGameObjects { get; } = new Dictionary<Mail, GameObject>();

    public UnityEvent<Mail> onMailSelected = new UnityEvent<Mail>();

    private void Start() {
        MailController mailPrefab = AssetReferencesSO.Instance.GetReference<MailController>();

        foreach (Mail mail in mails) {
            MailController mailController = Instantiate(mailPrefab);
            mailController.gameObject.transform.SetParent(transform, false);
            mailController.Initilize(mail);
            mailController.mailButton.onClick.AddListener(() => OnMailSelected(mail));

            this.mailGameObjects.Add(mail, mailController.gameObject);
        }
    }
    
    /// <summary>
    /// Called by EventManager to deliver a new email to the player.
    /// </summary>
    public void AddMail(Mail newMail)
    {
        if (!mails.Contains(newMail))
        {
            mails.Add(newMail);
        }

        MailController mailPrefab = AssetReferencesSO.Instance.GetReference<MailController>();
        InstantiateMailUI(newMail, mailPrefab);
    }

    private void InstantiateMailUI(Mail mail, MailController prefab)
    {
        MailController mailController = Instantiate(prefab);
        mailController.gameObject.transform.SetParent(transform, false);

        // Force scale to 1 in case the Canvas scaler messes with instantiated prefab scale
        mailController.transform.localScale = Vector3.one;

        mailController.Initilize(mail);
        mailController.mailButton.onClick.AddListener(() => OnMailSelected(mail));

        this.mailGameObjects.Add(mail, mailController.gameObject);
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
