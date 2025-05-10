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
    public RawImage mapPreview;


    private bool _isAdjusting = false;


    public MapManager mapManager;

    private void Start()
    {
        generateButton.onClick.AddListener(OnGenerateClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);

        obstacleSlider.onValueChanged.AddListener(OnObstacleSliderChanged);
        resourceSlider.onValueChanged.AddListener(OnResourceSliderChanged);

        // Ustaw domyślne etykiety
        obstacleLabel.text = $"Przeszkody: {obstacleSlider.value:F0}%";
        resourceLabel.text = $"Zasoby: {resourceSlider.value:F0}%";
    }

    private void OnGenerateClicked()
    {
        Debug.Log("Kliknięto przycisk Generuj.");
        int.TryParse(widthInput.text, out int width);
        int.TryParse(heightInput.text, out int height);
        
        if (width <= 0 || height <= 0)
        {
            LogMessage("Nieprawidłowe wymiary mapy.");
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
        Debug.Log("Kliknięto przycisk Anuluj.");
        mapManager.CancelGeneration();
    }

    public void UpdateProgress(float value)
    {
        progressBar.value = value;
    }

    public void LogMessage(string msg)
    {
        //logText.text += msg + "\n";
        logText.text = msg;
        Debug.Log(msg);

    }

    private void OnObstacleSliderChanged(float value)
    {
        if (_isAdjusting) return;
        _isAdjusting = true;

        float total = value + resourceSlider.value;
        if (total > 100)
        {
            resourceSlider.value = 100 - value;
        }

        UpdateLabels();
        _isAdjusting = false;
    }

    private void OnResourceSliderChanged(float value)
    {
        if (_isAdjusting) return;
        _isAdjusting = true;

        float total = value + obstacleSlider.value;
        if (total > 100)
        {
            obstacleSlider.value = 100 - value;
        }

        UpdateLabels();
        _isAdjusting = false;
    }

    private void UpdateLabels()
    {
        obstacleLabel.text = $"Przeszkody: {(int)obstacleSlider.value}%";
        resourceLabel.text = $"Zasoby: {(int)resourceSlider.value}%";
    }

    public void ShowMapTexture(Texture2D texture)
    {
        if (texture == null) return;

        mapPreview.texture = texture;
        mapPreview.rectTransform.sizeDelta = new Vector2(texture.width, texture.height);
    }


}
