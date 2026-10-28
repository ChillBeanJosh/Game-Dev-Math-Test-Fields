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


    [Header("Collision Settings: ")]
    [SerializeField] private float capsuleHeight = 2.0f;
    [SerializeField] private float capsuleRadius = 0.5f;
    [SerializeField] private LayerMask collisionMask;
    [Tooltip("Prevents the player from getting mathematically stuck in walls")]
    [SerializeField] private float skinWidth = 0.015f;
    [Tooltip("How many walls can we slide against in a single frame?")]
    [SerializeField] private int maxBounces = 3;
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
    [SerializeField] private Color hitPointColor = Color.white;
    [SerializeField] private Color hitNormalColor = Color.magenta;
    [Space]


    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;

    // List to capture individual acceleration vectors for gizmo drawing each frame
    List<Vector3> _activeAccelerations = new List<Vector3>();

    private List<CollisionHitData> _collisionHits = new List<CollisionHitData>();
    private struct CollisionHitData
    {
        public Vector3 Point;
        public Vector3 Normal;
    }

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
        _collisionHits.Clear();
        totalAcceleration = Vector3.zero;

        //Player Owned Acceleration Terms:
        Vector3 appliedAcceleration = _requestedMovement * moveAcceleration;
        Vector3 dragAcceleration = -currentVelocity * dragCoefficient;
        Vector3 gravityAcceleration = new Vector3(0f, -98f, 0f);

        AddAcceleration(appliedAcceleration);
        AddAcceleration(dragAcceleration);
        AddAcceleration(gravityAcceleration);
    }

    public void ApplyMovement()
    {
        float dt = Time.fixedDeltaTime;

        //Store Previous Position For Interpolation:
        previousPosition = currentPosition;

        //Semi Implicit Euler Integration:
        currentVelocity += totalAcceleration * dt;
        Vector3 positionDisplacement = currentVelocity * dt;

        //Collision Detection: 
        currentPosition = PerformCollideAndSlide(currentPosition, positionDisplacement, ref currentVelocity, 0);

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

    private Vector3 PerformCollideAndSlide(Vector3 position, Vector3 movement, ref Vector3 velocity, int depth)
    {
        // Base case: Stop if we've bounced too many times (corners) or movement is microscopic
        if (depth >= maxBounces || movement.magnitude < 0.001f)
            return position;

        GetCapsulePoints(position, out Vector3 p1, out Vector3 p2);
        Vector3 direction = movement.normalized;
        float distance = movement.magnitude;

        // Sweep cast our capsule forward along the movement path
        if (Physics.CapsuleCast(p1, p2, capsuleRadius, direction, out RaycastHit hit, distance + skinWidth, collisionMask))
        {
            // Record hit data for Gizmo visualization
            _collisionHits.Add(new CollisionHitData { Point = hit.point, Normal = hit.normal });

            // Move safely up to the hit point, keeping 'skinWidth' away to prevent getting stuck
            float safeDistance = Mathf.Max(0f, hit.distance - skinWidth);
            Vector3 safePosition = position + (direction * safeDistance);

            // Calculate the movement we still have left to complete
            Vector3 remainingMovement = movement - (direction * safeDistance);

            // Slide remaining movement vector along the surface we just hit
            remainingMovement = Vector3.ProjectOnPlane(remainingMovement, hit.normal);

            // IMPORTANT: We also must project the global velocity onto the wall plane.
            // If we don't, our velocity will keep building up *into* the wall over multiple frames.
            velocity = Vector3.ProjectOnPlane(velocity, hit.normal);

            // Recurse to handle sliding along this wall (and potentially hitting another wall in the corner)
            return PerformCollideAndSlide(safePosition, remainingMovement, ref velocity, depth + 1);
        }

        // If no collision, just return the remaining desired position
        return position + movement;
    }

    private void GetCapsulePoints(Vector3 basePosition, out Vector3 p1, out Vector3 p2)
    {
        // Pivot is on the feet, so move up by 1 to center the capsule
        Vector3 center = basePosition + Vector3.up * 1.0f;

        float halfHeight = capsuleHeight / 2f;
        float distToSphereCenters = halfHeight - capsuleRadius;

        p1 = center - (Vector3.up * distToSphereCenters); // Bottom sphere
        p2 = center + (Vector3.up * distToSphereCenters); // Top sphere
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || !showDebugGizmos) return;

        // CAPSULE VISUALIZATION (shows the Y+1 offset in action)
        Gizmos.color = velocityColor;
        GetCapsulePoints(currentPosition, out Vector3 p1, out Vector3 p2);
        Gizmos.DrawWireSphere(p1, capsuleRadius);
        Gizmos.DrawWireSphere(p2, capsuleRadius);
        Gizmos.DrawLine(p1 + Vector3.left * capsuleRadius, p2 + Vector3.left * capsuleRadius);
        Gizmos.DrawLine(p1 + Vector3.right * capsuleRadius, p2 + Vector3.right * capsuleRadius);
        Gizmos.DrawLine(p1 + Vector3.forward * capsuleRadius, p2 + Vector3.forward * capsuleRadius);
        Gizmos.DrawLine(p1 + Vector3.back * capsuleRadius, p2 + Vector3.back * capsuleRadius);

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

        //COLLISION HITS & NORMALS:
        foreach (var hit in _collisionHits)
        {
            Gizmos.color = hitPointColor;
            Gizmos.DrawSphere(hit.Point, 0.25f); // Small sphere at the exact point of impact

            Gizmos.color = hitNormalColor;
            Gizmos.DrawRay(hit.Point, hit.Normal * 0.75f); // Ray showing the direction the wall is facing
        }
    }
}
