using UnityEngine;

public class PestoPipelineManager : MonoBehaviour
{
    public static PestoPipelineManager Instance { get; private set; }

    [Header("Current Pipeline State")]
    [SerializeField] private int currentStepIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public int GetCurrentStepIndex()
    {
        return currentStepIndex;
    }

    public void AdvancePipeline()
    {
        currentStepIndex++;

        // Force AR cards to instantly re-evaluate their visibility
        var arManager = FindFirstObjectByType<PestoTutorialARManager>();
        if (arManager != null)
        {
            arManager.RefreshActiveCards();
        }

        Debug.Log($"Pipeline advanced to step index: {currentStepIndex}");
    }
}