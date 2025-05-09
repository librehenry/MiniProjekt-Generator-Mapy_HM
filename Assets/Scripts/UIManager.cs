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

    public MapManager mapManager;

    private void Start()
    {
        generateButton.onClick.AddListener(OnGenerateClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    private void OnGenerateClicked()
    {
        int.TryParse(widthInput.text, out int width);
        int.TryParse(heightInput.text, out int height);

        mapManager.width = width;
        mapManager.height = height;
        mapManager.StartGeneration(this);
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
    /*
    // Update is called once per frame
    void Update()
    {
        
    }
    */
}
