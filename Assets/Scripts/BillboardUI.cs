using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Camera _mainCamera;

    private void Start()
    {
        // Find the AR phone camera automatically
        _mainCamera = Camera.main; 

        // Automatically assign the Event Camera for UI interactions
        Canvas myCanvas = GetComponent<Canvas>();
        if (myCanvas != null)
        {
            myCanvas.worldCamera = _mainCamera;
        }
    }

    private void LateUpdate()
    {
        if (_mainCamera == null) return;

        // Force the UI to face the camera
        Vector3 directionAwayFromCamera = transform.position - _mainCamera.transform.position;
        directionAwayFromCamera.y = 0;

        transform.rotation = Quaternion.LookRotation(directionAwayFromCamera);
    }
}