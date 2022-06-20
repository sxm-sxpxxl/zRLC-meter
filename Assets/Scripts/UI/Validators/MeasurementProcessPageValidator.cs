using UnityEngine;

public sealed class MeasurementProcessPageValidator : PageValidator
{
    [SerializeField] private CanvasGroup contentCanvasGroup;
    [SerializeField, Range(0f, 1f)] private float disabledAlpha = 0.3f;
    
    [Space]
    [SerializeField] private GameObject errorContainer;
    [SerializeField] private GameObject fixErrorsInfoBox;
    [SerializeField] private GameObject fixWarningsInfoBox;

    protected override void OnDependencyStateChanged(bool isErrorsExist, bool isWarningsExist)
    {
        fixErrorsInfoBox.SetActive(isErrorsExist);
        fixWarningsInfoBox.SetActive(isWarningsExist);
        
        errorContainer.SetActive(isErrorsExist || isWarningsExist);
        
        IsValid = !isErrorsExist;
        contentCanvasGroup.alpha = IsValid ? 1f : disabledAlpha;
        contentCanvasGroup.blocksRaycasts = IsValid;
    }
}
