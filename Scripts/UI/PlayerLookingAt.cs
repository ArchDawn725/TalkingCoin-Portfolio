using UnityEngine;

public class PlayerLookingAt : MonoBehaviour
{
    [SerializeField] private float raycastRange = 10f;
    [SerializeField] private LayerMask raycastLayers;
    private CharacterBodyController selectedBodyController;
    private float highlightTimer;
    private const float HighlightTimeoutDuration = 1f;

    private void Update()
    {
        ShootRaycast();
        CheckHighlightTimeout();
        CheckMovementDistance();
    }

    private void ShootRaycast()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastRange, raycastLayers))
        {
            CharacterBodyController bodyController = hit.collider.GetComponent<CharacterBodyController>();
            if (bodyController != null && bodyController != selectedBodyController)
            {
                DeSelectCurrentBody();
                HighlightBody(bodyController);
            }
            else
            {
                ResetHighlightTimer();
            }
        }
        else
        {
            DeSelectCurrentBody();
        }
    }

    private void HighlightBody(CharacterBodyController bodyController)
    {
        bodyController.Highlight();
        selectedBodyController = bodyController;
        Controller.Instance.ActiveInteraction = bodyController.GetComponent<LLMController>();
        ResetHighlightTimer();
    }

    private void DeSelectCurrentBody()
    {
        if (selectedBodyController != null)
        {
            selectedBodyController.UnHighlight();
            selectedBodyController = null;
        }
    }

    private void ResetHighlightTimer()
    {
        highlightTimer = 0f;
    }

    private void CheckHighlightTimeout()
    {
        if (selectedBodyController != null)
        {
            highlightTimer += Time.deltaTime;
            if (highlightTimer > HighlightTimeoutDuration)
            {
                DeSelectCurrentBody();
            }
        }
    }
    private void CheckMovementDistance()
    {
        if (Controller.Instance.ActiveInteraction != null)
        {
            if (Vector3.Distance(transform.position, Controller.Instance.ActiveInteraction.transform.position) > raycastRange)
            {
                DeSelectCurrentBody();
                Controller.Instance.ActiveInteraction = null;
            }
        }
    }
}
