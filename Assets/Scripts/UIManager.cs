using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TMP_InputField widthInput;
    public TMP_InputField heightInput;
    public Button generateButton;
    public Button cancelButton;
    public Slider progressBar;
    public TextMeshProUGUI logText;
    public Slider obstacleSlider;
    public TextMeshProUGUI obstacleLabel;
    public Slider resourceSlider;
    public TextMeshProUGUI resourceLabel;


    public MapManager mapManager;

    private void Start()
    {
        generateButton.onClick.AddListener(OnGenerateClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);

        obstacleSlider.onValueChanged.AddListener(value => obstacleLabel.text = $"Przeszkody: {value:F0}%");
        resourceSlider.onValueChanged.AddListener(value => resourceLabel.text = $"Zasoby: {value:F0}%");

        // Ustaw domy?lne etykiety
        obstacleLabel.text = $"Przeszkody: {obstacleSlider.value:F0}%";
        resourceLabel.text = $"Zasoby: {resourceSlider.value:F0}%";
    }

    private void OnGenerateClicked()
    {
        int.TryParse(widthInput.text, out int width);
        int.TryParse(heightInput.text, out int height);
        
        if (width <= 0 || height <= 0)
        {
            LogMessage("Nieprawid?owe wymiary mapy.");
            return;
        }

        mapManager.width = width;
        mapManager.height = height;

        float obstaclePercent = obstacleSlider.value / 100f;
        float resourcePercent = resourceSlider.value / 100f;

        progressBar.value = 0;
        logText.text = "";

        mapManager.StartGeneration(this, obstaclePercent, resourcePercent);
    }

    private void OnCancelClicked()
    {
        mapManager.CancelGeneration();
    }

    public void UpdateProgress(float value)
    {
        progressBar.value = value;
    }

    public void LogMessage(string msg)
    {
        logText.text += msg + "\n";
    }
}
