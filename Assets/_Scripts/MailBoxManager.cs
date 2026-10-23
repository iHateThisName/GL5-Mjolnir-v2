using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Author: https://github.com/iHateThisName
/// </summary>
public class MailBoxManager : MonoBehaviour
{
    [SerializeField] private List<MailData> mailDataList = new List<MailData>();
    [SerializeField] private Transform mailContainer;
    [SerializeField] private MailSelectedController mailSelectedController;
    [Header("SFX References")]
    [SerializeField] private AudioClip mailSuccessSFX;
    [SerializeField] private AudioClip mailFailedSFX;

    private readonly Dictionary<MailData, MailController> mailViews = new Dictionary<MailData, MailController>();
    private SituationManager manager;
    private bool started;

    private void Awake()
    {
        if (mailSelectedController == null && transform.parent != null)
            mailSelectedController = transform.parent.GetComponentInChildren<MailSelectedController>(true);
    }

    private void OnEnable()
    {
        if (started) SubscribeAndRefresh();
    }

    private void Start()
    {
        started = true;
        if (mailSelectedController == null || mailContainer == null || SituationManager.Instance == null)
        {
            Debug.LogError("Mailbox requires its controller, mail container, and SituationManager.", this);
            enabled = false;
            return;
        }
        SubscribeAndRefresh();
    }

    private void SubscribeAndRefresh()
    {
        manager = SituationManager.Instance;
        if (manager == null || mailSelectedController == null) return;
        // Pair and deduplicate only our own listeners.
        mailSelectedController.selectedMailFinished.RemoveListener(OnMailFinished);
        mailSelectedController.selectedMailFinished.AddListener(OnMailFinished);
        manager.OnSituationStateChange -= OnSituationStateChange;
        manager.OnSituationStateChange += OnSituationStateChange;
        manager.OnMailPresentationCompleted -= OnPresentationCompleted;
        manager.OnMailPresentationCompleted += OnPresentationCompleted;
        manager.RuntimeReady -= RefreshAll;
        manager.RuntimeReady += RefreshAll;
        foreach (MailData mail in mailDataList.Where(mail => mail != null).Distinct().ToArray())
        {
            manager.AddSituation(mail);
            EnsureView(mail);
        }
        RefreshAll();
    }

    private void OnDisable()
    {
        if (mailSelectedController != null)
            mailSelectedController.selectedMailFinished.RemoveListener(OnMailFinished);
        if (manager != null)
        {
            manager.OnSituationStateChange -= OnSituationStateChange;
            manager.OnMailPresentationCompleted -= OnPresentationCompleted;
            manager.RuntimeReady -= RefreshAll;
        }
    }

    public void AddMail(MailData mail)
    {
        if (mail == null) return;
        if (!mailDataList.Contains(mail)) mailDataList.Add(mail);
        if (!started || manager == null) return;
        manager.AddSituation(mail);
        EnsureView(mail);
        RefreshAll();
    }

    private void EnsureView(MailData mail)
    {
        if (mailViews.ContainsKey(mail)) return;
        MailController prefab = AssetReferencesSO.Instance.GetReference<MailController>();
        if (prefab == null)
        {
            Debug.LogError("AssetReferencesSO has no MailController prefab.", this);
            return;
        }
        MailController view = Instantiate(prefab, mailContainer, false);
        view.transform.localScale = Vector3.one;
        view.Initialize(mail);
        if (view.mailButton == null)
        {
            Debug.LogError("Mail row prefab needs a Button.", view);
            Destroy(view.gameObject);
            return;
        }
        view.mailButton.onClick.AddListener(() => mailSelectedController.SelectMail(mail));
        mailViews.Add(mail, view);
    }

    private void OnSituationStateChange(SituationData situation)
    {
        if (!(situation is MailData mail)) return;
        // Refresh every row to apply the global sequence lock, including other mailboxes.
        RefreshAll();
        if (!mailDataList.Contains(mail) || AudioManager.Instance == null) return;
        var state = manager.GetSituationState(mail);
        if (state == SituationManager.SituationStateEnum.Success && mailSuccessSFX != null)
            AudioManager.Instance.PlaySFX(mailSuccessSFX);
        else if (state == SituationManager.SituationStateEnum.Failed && mailFailedSFX != null)
            AudioManager.Instance.PlaySFX(mailFailedSFX, 0.25f);
    }

    private void OnPresentationCompleted(MailResolution result) => RefreshAll();

    private void RefreshAll()
    {
        if (manager == null) return;
        foreach (var pair in mailViews)
        {
            bool pending = manager.PendingMailPresentation != null &&
                           manager.PendingMailPresentation.Mail == pair.Key;
            bool active = manager.GetSituationState(pair.Key) == SituationManager.SituationStateEnum.Active;
            pair.Value.gameObject.SetActive(active || pending);
            pair.Value.mailButton.interactable = manager.IsReady && active && !manager.HasPendingMailPresentation;
        }
    }

    private void OnMailFinished(MailData finished)
    {
        if (!mailDataList.Contains(finished))
            return;

        RefreshAll();

        // Do not select the next email automatically.
        // The player chooses it from the inbox.
    }
}
