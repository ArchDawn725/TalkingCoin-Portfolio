using UnityEngine;

public class CharacterHighlightController
{
    private readonly CharacterBodyController bodyController;
    public SkinnedMeshRenderer skinnedMeshRenderer;

    public CharacterHighlightController(CharacterBodyController controller)
    {
        bodyController = controller;
        skinnedMeshRenderer = bodyController.GetComponentInChildren<SkinnedMeshRenderer>();
    }

    public void Highlight()
    {
        SetRenderingLayer(3); // Set to highlight layer
    }

    public void UnHighlight()
    {
        SetRenderingLayer(1); // Reset to default layer
    }

    private void SetRenderingLayer(uint layer)
    {
        if (skinnedMeshRenderer != null)
        {
            skinnedMeshRenderer.renderingLayerMask = layer;
        }
    }
}
