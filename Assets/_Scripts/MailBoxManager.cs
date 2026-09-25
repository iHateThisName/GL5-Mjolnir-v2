using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MailBoxManager : MonoBehaviour
{
    [SerializeField] private List<Mail> mails = new List<Mail>();
    public Dictionary<Mail, GameObject> mailGameObjects { get; } = new Dictionary<Mail, GameObject>();

    public UnityEvent<Mail> onMailSelected = new UnityEvent<Mail>();


    public void AddMail(Mail newMail)
    {
        if (!mails.Contains(newMail))
        {
            mails.Add(newMail);
        }

        if (!mailGameObjects.ContainsKey(newMail))
        {
            MailController mailPrefab = AssetReferencesSO.Instance.GetReference<MailController>();
            InstantiateMailUI(newMail, mailPrefab);
        }
    }

    private void InstantiateMailUI(Mail mail, MailController prefab)
    {
        MailController mailController = Instantiate(prefab);
        mailController.gameObject.transform.SetParent(transform, false);

        mailController.transform.localScale = Vector3.one;

        mailController.Initilize(mail);
        mailController.mailButton.onClick.AddListener(() => OnMailSelected(mail));

        this.mailGameObjects.Add(mail, mailController.gameObject);
    }

    private void OnMailSelected(Mail mail)
    {
        this.onMailSelected.Invoke(mail);
    }

    public void OnDeleteMail(Mail mail)
    {
        if (this.mailGameObjects.TryGetValue(mail, out GameObject goMail))
        {
            goMail.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Mail {mail.name} not found in mailGameObjects dictionary.");
        }
    }
}