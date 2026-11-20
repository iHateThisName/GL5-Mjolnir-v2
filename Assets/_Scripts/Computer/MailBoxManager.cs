using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
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

    [Header("New Mail Notification")]
    [Tooltip("Small panel with icon + text on the computer desktop. Stays visible until the inbox is opened.")]
    [SerializeField] private GameObject newMailNotificationPopup;
    [SerializeField] private TMP_Text notificationTitleText;
    [SerializeField] private TMP_Text notificationAddressText;
    [SerializeField] private SFXController newMailSFXController;
    [SerializeField] private float popupTweenDuration = 0.35f;

    [Header("Unread Badge")]
    [Tooltip("Badge object above the inbox icon. Hides automatically when 0.")]
    [SerializeField] private GameObject unreadBadgeRoot;
    [SerializeField] private TMP_Text unreadCountText;

    private readonly Dictionary<MailData, MailController> mailViews = new Dictionary<MailData, MailController>();
    private readonly HashSet<MailData> notifiedMails = new HashSet<MailData>();
    private SituationManager manager;
    private bool started;
    private Vector3 popupDefaultScale = Vector3.one;

    private void Awake()
    {
        if (this.mailSelectedController == null && this.transform.parent != null)
            this.mailSelectedController = this.transform.parent.GetComponentInChildren<MailSelectedController>(true);

        if (this.newMailNotificationPopup != null)
        {
            this.popupDefaultScale = this.newMailNotificationPopup.transform.localScale;
            this.newMailNotificationPopup.SetActive(false);
        }

        if (this.unreadBadgeRoot != null)
            this.unreadBadgeRoot.SetActive(false);
    }

    private void OnEnable()
    {
        if (this.started) this.SubscribeAndRefresh();
    }

    private void Start()
    {
        this.started = true;
        if (this.mailSelectedController == null || this.mailContainer == null || SituationManager.Instance == null)
        {
            Debug.LogError("Mailbox requires its controller, mail container, and SituationManager.", this);
            this.enabled = false;
            return;
        }
        this.SubscribeAndRefresh();
    }

    private void SubscribeAndRefresh()
    {
        this.manager = SituationManager.Instance;
        if (this.manager == null || this.mailSelectedController == null) return;

        // Pair and deduplicate only our own listeners.
        this.mailSelectedController.selectedMailFinished.RemoveListener(this.OnMailFinished);
        this.mailSelectedController.selectedMailFinished.AddListener(this.OnMailFinished);
        this.manager.OnSituationStateChange -= this.OnSituationStateChange;
        this.manager.OnSituationStateChange += this.OnSituationStateChange;
        this.manager.OnMailPresentationCompleted -= this.OnPresentationCompleted;
        this.manager.OnMailPresentationCompleted += this.OnPresentationCompleted;
        this.manager.RuntimeReady -= this.OnRuntimeReady;
        this.manager.RuntimeReady += this.OnRuntimeReady;

        foreach (MailData mail in this.mailDataList.Where(mail => mail != null).Distinct().ToArray())
        {
            this.manager.AddSituation(mail);
            this.EnsureView(mail);
        }

        this.OnRuntimeReady();
    }

    private void OnDisable()
    {
        if (this.mailSelectedController != null)
            this.mailSelectedController.selectedMailFinished.RemoveListener(this.OnMailFinished);
        if (this.manager != null)
        {
            this.manager.OnSituationStateChange -= this.OnSituationStateChange;
            this.manager.OnMailPresentationCompleted -= this.OnPresentationCompleted;
            this.manager.RuntimeReady -= this.OnRuntimeReady;
        }

        if (this.newMailNotificationPopup != null)
            this.newMailNotificationPopup.transform.DOKill();
    }

    public void AddMail(MailData mail)
    {
        if (mail == null) return;
        if (!this.mailDataList.Contains(mail)) this.mailDataList.Add(mail);
        if (!this.started || this.manager == null) return;
        this.manager.AddSituation(mail);
        this.EnsureView(mail);
        this.RefreshAll();
    }

    public void ClearNotification()
    {
        if (this.newMailNotificationPopup != null)
        {
            this.newMailNotificationPopup.transform.DOKill();
            this.newMailNotificationPopup.SetActive(false);
        }
    }

    private void OnRuntimeReady()
    {
        this.RefreshAll();

        if (this.manager == null) return;

        // Show the popup for any mail already active at scene start (without blasting SFX on frame 1).
        foreach (MailData mail in this.mailDataList)
        {
            if (mail == null) continue;

            if (this.manager.GetSituationState(mail) == SituationManager.SituationStateEnum.Active)
            {
                this.notifiedMails.Add(mail);
                this.ShowNotificationPopup(mail, animate: false);
            }
        }
    }

    private void EnsureView(MailData mail)
    {
        if (this.mailViews.ContainsKey(mail)) return;
        MailController prefab = AssetReferencesSO.Instance.GetReference<MailController>();
        if (prefab == null)
        {
            Debug.LogError("AssetReferencesSO has no MailController prefab.", this);
            return;
        }
        MailController view = Instantiate(prefab, this.mailContainer, false);
        view.transform.localScale = Vector3.one;
        view.Initialize(mail);
        if (view.mailButton == null)
        {
            Debug.LogError("Mail row prefab needs a Button.", view);
            Destroy(view.gameObject);
            return;
        }
        view.mailButton.onClick.AddListener(() => this.mailSelectedController.SelectMail(mail));
        this.mailViews.Add(mail, view);
    }

    private void OnSituationStateChange(SituationData situation)
    {
        if (!(situation is MailData mail)) return;

        // Refresh every row to apply the global sequence lock, including other mailboxes.
        this.RefreshAll();

        if (!this.mailDataList.Contains(mail)) return;

        var state = this.manager.GetSituationState(mail);

        if (state == SituationManager.SituationStateEnum.Active && this.notifiedMails.Add(mail))
        {
            this.ShowNotificationPopup(mail, animate: true);
            this.newMailSFXController?.PlaySFX();
            return;
        }

        if (AudioManager.Instance == null) return;

        if (state == SituationManager.SituationStateEnum.Success && this.mailSuccessSFX != null)
            AudioManager.Instance.PlaySFX(this.mailSuccessSFX);
        else if (state == SituationManager.SituationStateEnum.Failed && this.mailFailedSFX != null)
            AudioManager.Instance.PlaySFX(this.mailFailedSFX, 0.25f);
    }

    private void ShowNotificationPopup(MailData mail, bool animate)
    {
        if (this.notificationTitleText != null)
            this.notificationTitleText.text = mail.EmailHeader;

        if (this.notificationAddressText != null)
            this.notificationAddressText.text = string.IsNullOrWhiteSpace(mail.EmailAddress)
                ? mail.DisplayName
                : mail.EmailAddress;

        if (this.newMailNotificationPopup == null)
            return;

        Transform popupTransform = this.newMailNotificationPopup.transform;
        popupTransform.DOKill();
        this.newMailNotificationPopup.SetActive(true);

        if (animate)
        {
            popupTransform.localScale = Vector3.zero;
            popupTransform.DOScale(this.popupDefaultScale, this.popupTweenDuration).SetEase(Ease.OutBack);
        }
        else
        {
            popupTransform.localScale = this.popupDefaultScale;
        }
    }

    private void OnPresentationCompleted(MailResolution result) => this.RefreshAll();

    private void RefreshAll()
    {
        if (this.manager == null) return;

        int activeCount = 0;

        foreach (var pair in this.mailViews)
        {
            bool pending = this.manager.PendingMailPresentation != null &&
                           this.manager.PendingMailPresentation.Mail == pair.Key;
            bool active = this.manager.GetSituationState(pair.Key) == SituationManager.SituationStateEnum.Active;

            if (active)
                activeCount++;

            pair.Value.gameObject.SetActive(active || pending);
            pair.Value.mailButton.interactable = this.manager.IsReady && active && !this.manager.HasPendingMailPresentation;
        }

        this.UpdateUnreadBadge(activeCount);
    }

    private void UpdateUnreadBadge(int count)
    {
        if (this.unreadBadgeRoot != null)
            this.unreadBadgeRoot.SetActive(count > 0);

        if (this.unreadCountText != null)
            this.unreadCountText.text = count.ToString();
    }

    private void OnMailFinished(MailData finished)
    {
        if (!this.mailDataList.Contains(finished))
            return;

        this.RefreshAll();
    }
}