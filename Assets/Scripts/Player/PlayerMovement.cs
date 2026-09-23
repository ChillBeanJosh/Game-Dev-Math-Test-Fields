using System.Collections.Generic;
using UnityEngine;

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
}

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Setup: ")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Transform root;
    [SerializeField] private Transform cameraTarget;
    [Space]


    [Header("Global Physics State")]
    public Vector3 previousPosition;
    public Vector3 currentPosition;
    public Vector3 currentVelocity;
    public Vector3 totalAcceleration;
    [Space]


    [Header("Physics Settings")]
    public float meshRotationSpeed = 15f;
    public float dragCoefficient = 8.0f;
    [Space]


    [Header("Vector Visualization: ")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color individualAccelerationColor = Color.blue;
    [SerializeField] private Color totalAccelerationColor = Color.red;
    [Space]
    [SerializeField] private Color velocityColor = Color.green;
    [Space]
    [SerializeField] private Color trajectoryColor = Color.yellow;
    [SerializeField] private int trajectorySteps = 30;
    [Space]


    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;

    // List to capture individual acceleration vectors for gizmo drawing each frame
    List<Vector3> _activeAccelerations = new List<Vector3>();

    void Awake()
    {
        _rb.freezeRotation = true;
        _rb.useGravity = false;
        _rb.isKinematic = true;

        //Initialize Position State:
        currentPosition = transform.position;
        previousPosition = currentPosition;
    }

    public void UpdateInput(CharacterInput input)
    {
        _requestedRotation = input.Rotation;

        //Remap 2D Vector -> Y-Plane For Proper Input Direction:
        _requestedMovement = new Vector3(input.Move.x, 0f, input.Move.y);
        _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);
        _requestedMovement = _requestedRotation * _requestedMovement;
    }

    public void AddAcceleration(Vector3 acceleration)
    {
        totalAcceleration += acceleration;
        _activeAccelerations.Add(acceleration);
    }

    public void GatherIntrinsicAccelerations(float moveAcceleration)
    {
        //Clear Last TimeStep Data:
        _activeAccelerations.Clear();
        totalAcceleration = Vector3.zero;

        //Player Owned Acceleration Terms:
        Vector3 appliedAcceleration = _requestedMovement * moveAcceleration;
        Vector3 dragAcceleration = -currentVelocity * dragCoefficient;

        AddAcceleration(appliedAcceleration);
        AddAcceleration(dragAcceleration);
    }

    public void ApplyMovement()
    {
        float dt = Time.fixedDeltaTime;

        //Store Previous Position For Interpolation:
        previousPosition = currentPosition;

        //Semi Implicit Euler Integration:
        currentVelocity += totalAcceleration * dt;
        currentPosition += currentVelocity * dt;

        //Apply Position Update:
        transform.position = currentPosition;
    }

    public void UpdateBody(float deltaTime)
    {
        //Player Position Update Interpolation:
        float interpolationFactor = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
        root.position = Vector3.Lerp(previousPosition, currentPosition, interpolationFactor);

        //Player Rotation Update:
        Quaternion targetYaw = Quaternion.Euler(0, _requestedRotation.eulerAngles.y, 0);
        root.rotation = Quaternion.Slerp(root.rotation, targetYaw, meshRotationSpeed * deltaTime);
    }

    public Transform GetCameraTarget() => cameraTarget;

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || !showDebugGizmos) return;

        //INDEPENDENT ACCELERATION VECTORS:
        Gizmos.color = individualAccelerationColor;
        foreach (var acc in _activeAccelerations)
        {
            Gizmos.DrawRay(currentPosition, acc);
        }

        //TOTAL ACCELERATION VECTOR:
        Gizmos.color = totalAccelerationColor;
        Gizmos.DrawRay(currentPosition, totalAcceleration);

        //CURRENT VELOCITY VECTOR:
        Gizmos.color = velocityColor;
        Gizmos.DrawRay(currentPosition, currentVelocity);

        //POSITION TRAJECTORY PATH:
        Gizmos.color = trajectoryColor;
        Vector3 simPosition = currentPosition;
        Vector3 simVelocity = currentVelocity;
        float dt = Time.fixedDeltaTime;

        for (int i = 0; i < trajectorySteps; i++)
        {
            // Simulate steps ahead assuming total acceleration remains constant
            simVelocity += totalAcceleration * dt;
            Vector3 nextSimPosition = simPosition + simVelocity * dt;

            Gizmos.DrawLine(simPosition, nextSimPosition);
            simPosition = nextSimPosition;
        }
    }
}
