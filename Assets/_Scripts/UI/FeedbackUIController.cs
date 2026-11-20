using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author: https://github.com/iHateThisName
/// </summary>

public class FeedbackUIController : MonoBehaviour {

    [Header("Sprites")]
    [SerializeField] private Sprite PositiveFeedbackSprite;
    [SerializeField] private Sprite NegativeFeedbackSprite;

    [Header("UI Elements")]
    [SerializeField] private Image feedbackImage;
    [SerializeField] private RectTransform feedbackContainer;

    private void OnEnable() {
        SituationManager.Instance.OnSituationStateChange += OnSituationStateChange;
    }

    private void OnDisable() {
        if (SituationManager.Instance != null) {
            SituationManager.Instance.OnSituationStateChange -= OnSituationStateChange;
        }

        this.feedbackImage.DOKill();

    }

    private void Start() {
        
    }

    private void Update() {
        
    }

    private void OnSituationStateChange(SituationData data) {
        if (!data.IsCompleted) return;
        SituationManager.SituationStateEnum situationStateEnum = data.SituationStateEnum;

        this.feedbackImage.DOFade(1f, 0.5f).OnComplete(() => {
            
            this.feedbackImage.DOFade(0f, 0.5f).SetDelay(3f);
        });

        //this.feedbackImage.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), duration: 0.5f, vibrato: 5, elasticity: 5f);
        this.feedbackImage.transform.DOShakePosition(duration: 0.5f, strength: new Vector3(10f, 10f, 0f), vibrato: 10, randomness: 90f);

        if (situationStateEnum == SituationManager.SituationStateEnum.Success) {
            feedbackImage.sprite = PositiveFeedbackSprite;

        } else if (situationStateEnum == SituationManager.SituationStateEnum.Failed) {
            feedbackImage.sprite = NegativeFeedbackSprite;
        }
    }
}
