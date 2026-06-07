using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[System.Serializable]
public class RecipeStep
{
    public string targetImageName;

    [Header("Text Content")]
    public string stepTitle;
    [TextArea(3, 5)] public string stepDescription; 

    [Header("Optional Media")]
    public int timerSeconds; 

    public UnityEngine.Video.VideoClip videoFile; 
}

public class PestoTutorialARManager : MonoBehaviour
{
    [Header("AR Setup")]
    [SerializeField] private GameObject stepCardPrefab; 

    [Header("UI Appearance & Scaling")]
    [SerializeField] private float cardScale = 1.0f;    
    
    [Header("Wall Offsets")]
    [SerializeField] private float shiftLeftAmount = 0.35f; 
    
    [SerializeField] private float floatOutFromWall = 0.2f;  
    
    [SerializeField] private float verticalOffset = 0.0f;

    [Header("Pesto Recipe Steps")]
    [SerializeField] private List<RecipeStep> recipeSteps = new List<RecipeStep>();

    private ARTrackedImageManager _trackedImageManager;
    private Dictionary<string, RecipeStep> _stepDataMap;
    private Dictionary<string, int> _stepIndexMap;
    private Dictionary<TrackableId, GameObject> _spawnedCards;

    private void Start()
    {
        _trackedImageManager = GetComponent<ARTrackedImageManager>();
        _spawnedCards = new Dictionary<TrackableId, GameObject>();
        _stepDataMap = new Dictionary<string, RecipeStep>();
        _stepIndexMap = new Dictionary<string, int>();

        for (int i = 0; i < recipeSteps.Count; i++)
        {
            var step = recipeSteps[i];
            if (!_stepDataMap.ContainsKey(step.targetImageName))
            {
                _stepDataMap.Add(step.targetImageName, step);
                _stepIndexMap.Add(step.targetImageName, i); 
            }
        }

        _trackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
    }

    private void OnDestroy()
    {
        if (_trackedImageManager != null)
            _trackedImageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
    }

    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var added in eventArgs.added) UpdateCard(added);
        foreach (var updated in eventArgs.updated) UpdateCard(updated);
        
        foreach (var removedPair in eventArgs.removed)
        {
            TrackableId id = removedPair.Key;
            if (_spawnedCards.ContainsKey(id))
            {
                Destroy(_spawnedCards[id]);
                _spawnedCards.Remove(id);
            }
        }
    }

    private void UpdateCard(ARTrackedImage trackedImage)
    {
        if (trackedImage.referenceImage == null) return;
        string imageName = trackedImage.referenceImage.name;

        // Spawn and position: Relies on image transform as parent to inherit wall angle
        if (!_spawnedCards.ContainsKey(trackedImage.trackableId))
        {
            GameObject newCard = Instantiate(stepCardPrefab, trackedImage.transform);
            
            // Apply wall offsets to local coordinates:
            newCard.transform.localPosition = new Vector3(-shiftLeftAmount, verticalOffset, floatOutFromWall);
            
            newCard.transform.localRotation = Quaternion.identity;
            
            // Apply scale safely with the base millimeter conversion factor
            newCard.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f) * cardScale; 
            
            Vector3 worldPos = newCard.transform.position;
            worldPos.y = Camera.main.transform.position.y;
            newCard.transform.position = worldPos;
            _spawnedCards.Add(trackedImage.trackableId, newCard);
        }

        GameObject activeCard = _spawnedCards[trackedImage.trackableId];

        // Determine if this image belongs to the current step
        bool isCurrentStep =
            _stepIndexMap.ContainsKey(imageName) &&
            _stepIndexMap[imageName] == PestoPipelineManager.Instance.GetCurrentStepIndex();

        bool isTracked = trackedImage.trackingState is TrackingState.Tracking;

        if (!isCurrentStep || !isTracked)
        {
            activeCard.SetActive(false);
            return;
        }

        activeCard.SetActive(true);

        if (_stepDataMap.ContainsKey(imageName))
        {
            RecipeStep stepData = _stepDataMap[imageName];

            StepCardUI uiScript = activeCard.GetComponentInChildren<StepCardUI>();

            if (uiScript != null)
            {
                uiScript.UpdateStepContent(stepData.stepTitle, stepData.stepDescription, stepData.timerSeconds, stepData.videoFile);
            }
        }
    }

    public void RefreshActiveCards()
    {
        foreach (var trackedImage in _trackedImageManager.trackables)
        {
            UpdateCard(trackedImage);
        }
    }
}