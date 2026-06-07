using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Video;
using UnityEngine.UI; 

public class StepCardUI : MonoBehaviour
{
    [Header("Text Elements")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI timerText;
    
    [Header("Media and Layout Elements")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoDisplay; 
    
    [Header("Buttons")]
    [SerializeField] private Button timerButton;    
    [SerializeField] private Button nextStepButton; 
    [SerializeField] private TextMeshProUGUI timerButtonLabel;  

    [SerializeField] private GameObject timerGroup;

    private int _totalSeconds;
    private int _currentSeconds;
    private bool _isTimerRunning = false;
    private Coroutine _timerCoroutine;
    private string _loadedStepTitle = "";

    private void OnEnable()
    {
        _loadedStepTitle = ""; 

        if (nextStepButton != null)
        {
            nextStepButton.onClick.RemoveListener(OnNextButtonClicked);
            nextStepButton.onClick.AddListener(OnNextButtonClicked);
        }
    }

    private void Awake()
    {
        // Automatically assign the camera so clicks register in AR space
        Canvas myCanvas = GetComponentInParent<Canvas>();
        if (myCanvas != null && myCanvas.worldCamera == null)
        {
            myCanvas.worldCamera = Camera.main;
        }

        // Setup the timer button listener 
        if (timerButton != null)
        {
            timerButton.onClick.RemoveListener(ToggleTimer);
            timerButton.onClick.AddListener(ToggleTimer);
        }
    }

    private void OnNextButtonClicked()
    {
        if (PestoPipelineManager.Instance != null)
        {
            PestoPipelineManager.Instance.AdvancePipeline();
        }
    }

    public void UpdateStepContent(string stepTitle, string stepDescription, int timerSeconds, UnityEngine.Video.VideoClip videoFile)
    {
        if (_loadedStepTitle == stepTitle) return;
        _loadedStepTitle = stepTitle;

        if (titleText != null) titleText.text = stepTitle;
        if (descriptionText != null) descriptionText.text = stepDescription;

        if (videoDisplay != null)
        {
            if (videoFile != null && videoPlayer != null)
            {
                videoDisplay.gameObject.SetActive(true);
                videoPlayer.gameObject.SetActive(true);

                // Create a RenderTexture sized to the clip and wire both ends to it
                if (videoPlayer.targetTexture == null)
                {
                    var rt = new RenderTexture(1920, 1080, 0);
                    rt.Create();
                    videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                    videoPlayer.targetTexture = rt;
                    videoDisplay.texture = rt;
                }

                videoPlayer.clip = videoFile;
                videoPlayer.Play();
            }
            else
            {
                videoDisplay.gameObject.SetActive(false); 
                if (videoPlayer != null) videoPlayer.gameObject.SetActive(false);
            }
        }

        if (timerText != null)
        {
            if (timerSeconds > 0)
            {
                timerGroup.SetActive(true);
                timerText.gameObject.SetActive(true);
                if (timerButton != null) timerButton.gameObject.SetActive(true);

                _totalSeconds = timerSeconds;
                _currentSeconds = _totalSeconds;
                _isTimerRunning = false;
                UpdateTimerDisplay();
                UpdateTimerButtonLabel();

                if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);
            }
            else
            {
                timerGroup.SetActive(false);
                timerText.gameObject.SetActive(false);
                if (timerButton != null) timerButton.gameObject.SetActive(false);
            }
        }
    }

    private void ToggleTimer()
    {
        if (_currentSeconds <= 0) return; 
        _isTimerRunning = !_isTimerRunning; 

        UpdateTimerButtonLabel();
        if (_isTimerRunning)
        {
            _timerCoroutine = StartCoroutine(RunTimer());
        }
        else
        {
            if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);
            UpdateTimerDisplay(); 
        }
    }

    private IEnumerator RunTimer()
    {
        UpdateTimerDisplay();
        while (_currentSeconds > 0)
        {
            yield return new WaitForSeconds(1f);
            _currentSeconds--;
            UpdateTimerDisplay();
        }
        _isTimerRunning = false;
        UpdateTimerButtonLabel();
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        if (_currentSeconds <= 0)
        {
            timerText.text = "00:00 - DONE!";
        }
        else
        {
            timerText.text = string.Format("{0:00}:{1:00}", _currentSeconds / 60, _currentSeconds % 60);
        }
    }
    private void UpdateTimerButtonLabel()
    {
        if (timerButtonLabel == null) return;

        if (_currentSeconds <= 0)
            timerButtonLabel.text = "Start";
        else
            timerButtonLabel.text = _isTimerRunning ? "Stop" : "Start";
    }
}