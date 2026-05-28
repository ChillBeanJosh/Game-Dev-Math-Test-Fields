using UnityEngine;
using System.Collections.Generic;

public class SimplePendulum : MonoBehaviour
{
    [System.Serializable]
    public struct Pendulum
    {
        [Header("Physical Properties: ")]
        public float length;
        public float gravity;

        [Header("Angular Properties: ")]
        public Vector3 angularPosition;
        public Vector3 angularVelocity;
        public Vector3 angularAcceleration;

        [Header("Position: ")]
        public Vector3 pivotPosition;

        [HideInInspector] public Transform massTransform;
        [HideInInspector] public LineRenderer lineRenderer;
    }
    [Header("Simulation Settings: ")]
    [Range(0.1f, 5f)] public float TimeScale;

    [Header("Global Properties: ")]
    public float defaultLength = 2f;
    public float defaultGravity = 9.81f;
    [Space]
    public GameObject massPrefab;
    public Material lineMaterial;

    [Header("Chain Properties: ")]
    public bool formChain = false;
    [Space]
    public int chainCount;
    public float chainPositionSpacing;
    public float initialChainAngle;
    public float chainAngleSpacing;

    [Header("Pendulum Data List: ")]
    [Space]
    public List<Pendulum> pendulums = new List<Pendulum>();

    private void Start()
    {
        if (massPrefab == null)
        {
            Debug.LogError("Mass prefab is not assigned. Please assign a mass prefab in the inspector.");
            return;
        }
        SetupPendulums();
    }

    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime * TimeScale;

        for (int i = 0; i < pendulums.Count; i++)
        {
            Pendulum currentPendulum = pendulums[i];

            //Skip Pendulum if Mass Transform is Not Assigned:
            if (currentPendulum.massTransform == null) continue;

            //Assign Angular Acceleration Derived From Euler-Lagrange Equation for Simple Pendulum:
            float angle = currentPendulum.angularPosition.z;
            float acceleration = (-currentPendulum.gravity / currentPendulum.length) * Mathf.Sin(angle);
            currentPendulum.angularAcceleration = new Vector3(0, 0, acceleration);

            //Semi-Implicit Euler Integration:
            currentPendulum.angularVelocity += currentPendulum.angularAcceleration * deltaTime;
            currentPendulum.angularPosition += currentPendulum.angularVelocity * deltaTime;

            //Calculate New Position:
            float x = currentPendulum.length * Mathf.Sin(currentPendulum.angularPosition.z);
            float y = -currentPendulum.length * Mathf.Cos(currentPendulum.angularPosition.z);
            Vector3 relativePosition = new Vector3(x, y, 0);
            currentPendulum.massTransform.localPosition = currentPendulum.pivotPosition + relativePosition;

            //Update Line Renderer:
            Vector3 worldPivot = currentPendulum.pivotPosition;
            currentPendulum.lineRenderer.SetPosition(0, worldPivot);
            currentPendulum.lineRenderer.SetPosition(1, currentPendulum.massTransform.position);

            //Store Updated Pendulum Data:
            pendulums[i] = currentPendulum;
        }
    }

    private void SetupPendulums()
    {
        if (formChain)
        {
            //Clear Existing Inspected Pendulum Data:
            pendulums.Clear();

            for (int i = 0; i < chainCount; i++)
            {
               Pendulum newPendulum = new Pendulum();

                //Assign Default Physical Properties:
                newPendulum.length = defaultLength;
                newPendulum.gravity = defaultGravity;

                //Calculate Pivot Position:
                float positionOffset = i * chainPositionSpacing;
                newPendulum.pivotPosition = new Vector3(0, 0, positionOffset);

                //Calculate Initial Angular Position:
                float angleOffset = initialChainAngle + (i * chainAngleSpacing);
                newPendulum.angularPosition = new Vector3(0, 0, angleOffset);

                //Add New Pendulum to List:
                pendulums.Add(newPendulum);
            }
        }

        for (int i = 0; i < pendulums.Count; i++)
        {
            Pendulum currentPendulum = pendulums[i];

            //Assign Default Values if Null or Invalid:
            if (currentPendulum.length <= 0) currentPendulum.length = defaultLength;
            if (currentPendulum.gravity <= 0) currentPendulum.gravity = defaultGravity;

            //Assign Initial Angular Data:
            currentPendulum.angularPosition = new Vector3(0, 0, (currentPendulum.angularPosition.z * Mathf.Deg2Rad));
            currentPendulum.angularVelocity = Vector3.zero;
            currentPendulum.angularAcceleration = Vector3.zero;

            //Instantiate Mass Object:
            GameObject massObject = Instantiate(massPrefab, transform);
            massObject.name = $"Pendulum Mass {i + 1}";
            currentPendulum.massTransform = massObject.transform;

            //Setup Line Renderer:
            currentPendulum.lineRenderer = massObject.GetComponent<LineRenderer>();
            if (currentPendulum.lineRenderer == null)
            {
                currentPendulum.lineRenderer = massObject.AddComponent<LineRenderer>();
            }
            SetupLineRenderer(currentPendulum.lineRenderer);

            //Calculate Initial Position:
            float x = currentPendulum.length * Mathf.Sin(currentPendulum.angularPosition.z);
            float y = -currentPendulum.length * Mathf.Cos(currentPendulum.angularPosition.z);
            Vector3 relativePosition = new Vector3(x, y, 0);
            currentPendulum.massTransform.localPosition = currentPendulum.pivotPosition + relativePosition;

            //Store Updated Pendulum Data:
            pendulums[i] = currentPendulum;
        }
    }

    private void SetupLineRenderer(LineRenderer line)
    {
        line.positionCount = 2;
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.material = lineMaterial;

        line.useWorldSpace = true;
    }
}
