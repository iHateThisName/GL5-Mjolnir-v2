// --- ScamFailureFeedback.cs ---

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ScamFailureFeedback : MonoBehaviour
{
    [SerializeField] private AudioClip failureSfx;
    [SerializeField] private Image redLightImage;
    [SerializeField] private float blinkDuration = 0.2f;
    [SerializeField] private int totalBlinks = 3;

    private bool isBlinking;

    private void OnEnable()
    {
        if (ConditionTracker.Instance != null)
        {
            ConditionTracker.Instance.OnConditionStateChanged += this.OnConditionStateChanged;
        }
    }

    private void OnDisable()
    {
        if (ConditionTracker.Instance != null)
        {
            ConditionTracker.Instance.OnConditionStateChanged -= this.OnConditionStateChanged;
        }
    }

    private void OnConditionStateChanged(ConditionTracker.ConditionState state)
    {
        // Check for either the Scam condition OR the False Report condition!
        bool isScam = state.Condition == ConditionTracker.ConditionEnum.HasBeenScammed;
        bool isFalseReport = state.Condition == ConditionTracker.ConditionEnum.HasFalselyReported;

        if ((isScam || isFalseReport) && state.ExpectedState)
        {
            Debug.Log("RED BLINK TRIGGERED!");
            this.PlayFeedback();
        }
    }

    private void PlayFeedback()
    {
        if (this.isBlinking) return;
        this.isBlinking = true;

        if (this.failureSfx != null)
        {
            AudioManager.Instance.PlaySFX(this.failureSfx);
        }

        if (this.redLightImage != null)
        {
            Debug.Log("BLINK BLINK BLINK");
            int loops = this.totalBlinks * 2;

            this.redLightImage.gameObject.SetActive(true);
            this.redLightImage.DOFade(0.5f, this.blinkDuration)
                .SetLoops(loops, LoopType.Yoyo)
                .OnComplete(() => {
                    this.redLightImage.color = new Color(this.redLightImage.color.r, this.redLightImage.color.g, this.redLightImage.color.b, 0f);
                    this.redLightImage.gameObject.SetActive(false);
                    this.isBlinking = false;
                });
        }
        else
        {
            this.isBlinking = false;
        }
    }
}