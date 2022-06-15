using UnityEngine;
using TMPro;
using UnityEngine.Events;

[DisallowMultipleComponent]
public sealed class InputFieldController : MonoBehaviour
{
    [SerializeField] private TMP_InputField selfInputField;
    [SerializeField] private TMP_Text prefixText;

    [Space]
    [SerializeField] private string defaultValue = string.Empty;
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
        selfInputField.onEndEdit.AddListener(OnTextEndEdit);
    }

    private void OnDestroy()
    {
        selfInputField.onEndEdit.RemoveListener(OnTextEndEdit);
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void SetValue(float value)
    {
        Value = value;
        
        (float convertedValue, string prefix) = value.AutoConvertNormalValue();

        selfInputField.SetTextWithoutNotify($"{convertedValue:N}");
        prefixText.SetText($"{prefix}{units}");
    }

    public void ResetValue()
    {
        selfInputField.text = defaultValue;
        prefixText.text = $"{units}";
    }

    private void OnTextEndEdit(string text)
    {
        SetValue(float.Parse(text));
        OnValueEndEdit.Invoke(Value);
    }
}
