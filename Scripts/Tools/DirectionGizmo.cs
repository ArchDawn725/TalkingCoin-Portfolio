using UnityEngine;

public class DirectionGizmo : MonoBehaviour
{
    // Set the color and length of the arrow
    public Color gizmoColor = Color.red;
    public float arrowLength = 0.5f;

    // This method is called by Unity to draw Gizmos in the Scene view
    void OnDrawGizmos()
    {
        // Set the color of the gizmo
        Gizmos.color = gizmoColor;

        // Draw a ray to indicate the direction the GameObject is facing
        Gizmos.DrawRay(transform.position, transform.forward * arrowLength);

        // Optionally, draw an arrowhead to make it more visible
        DrawArrowHead();
    }

    // Method to draw an arrowhead at the end of the gizmo
    void DrawArrowHead()
    {
        Vector3 arrowTip = transform.position + transform.forward * arrowLength;
        Vector3 rightWing = arrowTip + (-transform.forward + transform.right) * 0.1f;
        Vector3 leftWing = arrowTip + (-transform.forward - transform.right) * 0.1f;

        Gizmos.DrawLine(arrowTip, rightWing);
        Gizmos.DrawLine(arrowTip, leftWing);
    }
}
