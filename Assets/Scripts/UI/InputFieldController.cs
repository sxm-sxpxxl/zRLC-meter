using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

[DisallowMultipleComponent]
public sealed class InputFieldController : MonoBehaviour
{
    [SerializeField] private TMP_InputField selfInputField;
    [SerializeField] private CustomRaycaster customRaycaster;
    [SerializeField] private TMP_Text prefixText;

    [Space]
    [SerializeField] private bool isReadonly = false;
    [SerializeField] private string defaultValue = string.Empty;
    [Space]
    [SerializeField] private bool isFixedUnits = false;
    [SerializeField] private string units = "Ohm";

    public float Value { get; private set; }
    public UnityEvent<float> OnValueEndEdit { get; } = new UnityEvent<float>();

    private void OnValidate()
    {
        if (selfInputField == null || prefixText == null)
        {
            Debug.LogWarning($"Assign {nameof(selfInputField)} and {nameof(prefixText)} refs to update the units!");
            return;
        }

        ResetValue();
    }

    private void Awake()
    {
        customRaycaster.OnPointerClick += OnInputFieldClick;
        selfInputField.onEndEdit.AddListener(OnTextEndEdit);
    }

    private void OnDestroy()
    {
        customRaycaster.OnPointerClick -= OnInputFieldClick;
        selfInputField.onEndEdit.RemoveListener(OnTextEndEdit);
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void SetValue(float value)
    {
        Value = value;

        if (isFixedUnits)
        {
            selfInputField.SetTextWithoutNotify($"{value:F1}");
            prefixText.SetText($"{units}");
            return;
        }
        
        (float convertedValue, string prefix) = value.AutoConvertNormalValue();

        selfInputField.SetTextWithoutNotify($"{convertedValue:F1}");
        prefixText.SetText($"{prefix}{units}");
    }

    public void ResetValue()
    {
        selfInputField.text = defaultValue;
        selfInputField.readOnly = isReadonly;
        prefixText.text = $"{units}";
    }
    
    private void OnInputFieldClick(PointerEventData eventData)
    {
        if (isReadonly)
        {
            return;
        }
        
        string valueText = $"{Value}";
        
        selfInputField.SetTextWithoutNotify(valueText);
        prefixText.SetText($"{units}");

        selfInputField.selectionStringAnchorPosition = valueText.Length;
    }

    private void OnTextEndEdit(string text)
    {
        if (isReadonly)
        {
            return;
        }
        
        SetValue(float.Parse(text));
        OnValueEndEdit.Invoke(Value);
    }
}
