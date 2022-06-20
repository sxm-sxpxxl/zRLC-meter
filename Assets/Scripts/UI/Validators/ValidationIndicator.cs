using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(Image)), DisallowMultipleComponent, ExecuteInEditMode]
public sealed class ValidationIndicator : MonoBehaviour
{
    [SerializeField] private StatusData[] statuses;
    [SerializeField] private ValidationState defaultStatus;

    [Space]
    [SerializeField] private PageValidator targetPageValidator;

    private Image _target;
    
    [Serializable]
    private struct StatusData
    {
        public ValidationState state;
        public Sprite icon;
        public Color color;
    }

    private void OnValidate()
    {
        _target = GetComponent<Image>();
        SetStatus(defaultStatus);
    }

    private void Awake()
    {
        _target = GetComponent<Image>();
        targetPageValidator.OnValidationStateChanged += SetStatus;
    }

    private void OnDestroy()
    {
        targetPageValidator.OnValidationStateChanged -= SetStatus;
    }

    private void SetStatus(ValidationState state)
    {
        Assert.IsTrue(statuses.Any(x => x.state == state), "The status doesn't match known statuses.");
        StatusData data = statuses.First(x => x.state == state);
        
        _target.sprite = data.icon;
        _target.color = data.color;
    }
}
