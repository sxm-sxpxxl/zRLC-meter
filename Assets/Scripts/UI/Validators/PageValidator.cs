using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class PageValidator : MonoBehaviour
{
    public event Action<ValidationState> OnValidationStateChanged = delegate { };

    [SerializeField] private bool isWarning = false;
    [SerializeField] private List<PageValidator> dependencies = new List<PageValidator>();

    private void Awake()
    {
        foreach (var dependency in dependencies)
        {
            dependency.OnValidationStateChanged += OnDependencyStateChanged;
        }

        Init();
    }

    private void OnDestroy()
    {
        foreach (var dependency in dependencies)
        {
            dependency.OnValidationStateChanged -= OnDependencyStateChanged;
        }

        Deinit();
    }

    protected virtual void Init() { }
    
    protected virtual void Deinit() { }

    protected virtual void OnDependencyStateChanged(bool isErrorsExist, bool isWarningsExist) { }
    
    private void OnDependencyStateChanged(ValidationState state)
    {
        bool isErrorsExist = dependencies.Any(x => x.ValidationState == ValidationState.NotValidError);
        bool isWarningsExist = dependencies.Any(x => x.ValidationState == ValidationState.NotValidWarning);

        OnDependencyStateChanged(isErrorsExist, isWarningsExist);
    }

    protected bool IsValid
    {
        get => _isValid;
        set
        {
            _isValid = value;
            OnValidationStateChanged.Invoke(ValidationState);
        }
    }

    private ValidationState ValidationState => _isValid 
        ? ValidationState.Valid
        : isWarning ? ValidationState.NotValidWarning : ValidationState.NotValidError;

    private bool _isValid = false;
}
