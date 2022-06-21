using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Image)), DisallowMultipleComponent, ExecuteInEditMode]
public sealed class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public event Action<TabButton> OnClick = delegate { };
    
    [SerializeField] private State defaultState = State.Idle;

    [Header("Content")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image underscore;

    [Header("States config")]
    [SerializeField] private Color idleColor;
    [SerializeField] private Color hoverColor;
    [SerializeField] private Color pressColor;
    [SerializeField] private Color disableColor;
    [SerializeField] private Color selectColor;

    [Space]
    [SerializeField] private TMP_FontAsset idleFontAsset;
    [SerializeField] private TMP_FontAsset selectFontAsset;

    private State _currentState;
    
    public enum State
    {
        Idle,
        Hover,
        Press,
        Disable,
        Select
    }

    private void OnValidate()
    {
        SetState(defaultState);
    }

    private void Awake()
    {
        SetState(defaultState);
    }

    public void SetState(State state)
    {
        Color stateColor = GetStateColor(state);

        icon.color = stateColor;
        label.color = stateColor;
        underscore.color = stateColor;
        
        underscore.gameObject.SetActive(state == State.Select);
        label.font = state == State.Select ? selectFontAsset : idleFontAsset;
        
        _currentState = state;
    }
    
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (_currentState == State.Idle)
        {
            SetState(State.Hover);
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (_currentState == State.Hover)
        {
            SetState(State.Idle);
        }
    }
    
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (_currentState == State.Hover)
        {
            SetState(State.Press);
        }
    }
    
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if (_currentState == State.Press)
        {
            SetState(State.Hover);
        }
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (_currentState != State.Select && _currentState != State.Disable)
        {
            OnClick.Invoke(this);
        }
    }

    private Color GetStateColor(State state) => state switch
    {
        State.Idle => idleColor,
        State.Hover => hoverColor,
        State.Press => pressColor,
        State.Disable => disableColor,
        State.Select => selectColor,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
    };
}
