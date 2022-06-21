using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class TestGenerationProcessView : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TestGenerationProcessController testGenerationProcessController;
    
    [Header("Test generation")]
    [SerializeField] private Button testGenerateButton;
    [SerializeField] private InputFieldController leftChannelRmsInputField;
    [SerializeField] private InputFieldController rightChannelRmsInputField;
    [SerializeField] private InputFieldController phaseShiftInputField;

    private void Awake()
    {
        testGenerateButton.onClick.AddListener(OnTestGenerationButtonClick);
        testGenerationProcessController.OnTestGenerationFinished += OnTestGenerationFinished;
    }

    private void OnDestroy()
    {
        testGenerateButton.onClick.RemoveListener(OnTestGenerationButtonClick);
        testGenerationProcessController.OnTestGenerationFinished -= OnTestGenerationFinished;
    }

    private void OnTestGenerationButtonClick()
    {
        testGenerateButton.interactable = false;
        testGenerationProcessController.TestGenerate();
    }

    private void OnTestGenerationFinished(float leftChannelRms, float rightChannelRms, float phaseShiftInRad)
    {
        leftChannelRmsInputField.SetValue(leftChannelRms);
        rightChannelRmsInputField.SetValue(rightChannelRms);
        phaseShiftInputField.SetValue(phaseShiftInRad * Mathf.Rad2Deg);
            
        testGenerateButton.interactable = true;
    }
}
