using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(Image)), DisallowMultipleComponent, ExecuteInEditMode]
public sealed class ValidationIndicator : MonoBehaviour
{
    [SerializeField] private StatusData[] statuses;
    [SerializeField] private Status defaultStatus;

    private Image _target;
    
    [Serializable]
    private struct StatusData
    {
        public Status status;
        public Sprite icon;
        public Color color;
    }

    private enum Status
    {
        Success,
        Warning,
        Error
    }

    private void OnValidate()
    {
        _target = GetComponent<Image>();
        SetStatus(defaultStatus);
    }

    private void Awake()
    {
        _target = GetComponent<Image>();
    }

    private void SetStatus(Status status)
    {
        Assert.IsTrue(statuses.Any(x => x.status == status), "The status doesn't match known statuses.");
        StatusData data = statuses.First(x => x.status == status);
        
        _target.sprite = data.icon;
        _target.color = data.color;
    }
}
