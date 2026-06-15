using UnityEngine;

public class SmoothVehicleCamera : MonoBehaviour
{
    [Header("Target Tracking")]
    [Tooltip("Assign the moving Chassis child object here.")]
    public Transform target;

    [Header("Position Tuning")]
    public float distance = 6.0f;     // Distance behind the car
    public float height = 2.0f;       // Height above the car
    public float positionDamping = 5.0f;

    [Header("Rotation Tuning")]
    public float rotationDamping = 3.0f;

    [Header("Bespoke Physics Settings")]
    [Tooltip("If true, filters out suspension bounce/pitch/roll from the camera's position calculation to prevent jitter.")]
    public bool filterHighFrequencyDynamics = true;

    private void LateUpdate()
    {
        // Early out if no moving chassis target is assigned
        if (!target) return;

        // 1. Calculate Target Orientation
        float targetRotationAngle = target.eulerAngles.y;
        float targetHeight = target.position.y + height;

        // If we want to ignore suspension pitch/roll for camera positioning:
        if (filterHighFrequencyDynamics)
        {
            // We isolate the horizontal angle (yaw) so the camera doesn't whip up/down during hard braking or body roll
            targetRotationAngle = Mathf.Atan2(target.forward.x, target.forward.z) * Mathf.Rad2Deg;
        }

        // 2. Smoothly Interpolate Angles & Height
        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        // Damped linear interpolation over time
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, targetRotationAngle, rotationDamping * Time.deltaTime);
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, positionDamping * Time.deltaTime);

        // 3. Convert the smoothed angle into a rotation quaternion
        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        // 4. Calculate Final Camera Position
        // Start at the target's position, pull back along the rotated forward vector, and set the smoothed height
        Vector3 targetPosition = target.position;
        targetPosition -= currentRotation * Vector3.forward * distance;
        targetPosition.y = currentHeight;

        // Apply position
        transform.position = targetPosition;

        // 5. Look at the Target
        // We look slightly ahead or slightly above the chassis origin to keep the car framed perfectly
        Vector3 lookTarget = target.position + (target.forward * 1.0f) + (Vector3.up * 0.5f);
        transform.LookAt(lookTarget);
    }
}
