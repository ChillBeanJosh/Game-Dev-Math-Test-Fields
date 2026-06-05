using System.Collections.Generic;
using UnityEngine;

public class DampedVerticalOscillator : MonoBehaviour
{

    [System.Serializable]
    public struct SpringDampedOscillator
    {
        [Header("Physical Properties: ")]
        public float mass;
        public float springLength;
        public float springConstant;
        public float dampingCoefficient;
        public float initialDisplacement;
        public float gravity;

        [Header("Position Properties: ")]
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 acceleration;

        [Header("Position: ")]
        public Vector3 pivotPosition;
        public Vector3 equilibriumPosition;

        [HideInInspector] public Transform massTransform;
        [HideInInspector] public LineRenderer springLineRenderer;
        [HideInInspector] public LineRenderer dampingLineRenderer;
    }
    [Header("--------------------------------------------------------------------------")]

    [Header("Graph Settings: ")]
    public RectTransform graphContainer;
    public float graphScale;

    [Header("Graph Point Settings: ")]
    public GameObject graphPointObjectPrefab;
    public int maxDataPoints;
    private Queue<GameObject> activeGraphPoints = new Queue<GameObject>();

    [Header("--------------------------------------------------------------------------")]

    [Header("Simulation Settings: ")]
    [Range(0.1f, 5f)] public float TimeScale = 1.0f;

    [Header("Global Properties: ")]
    public float defaultMass = 1.0f;
    public float defaultSpringLength = 3.0f;
    public float defaultSpringConstant = 12.0f;
    public float defaultDampingCoefficient = 0.5f;
    public float defaultInitialDisplacement = 0.5f;
    public float defaultGravity = 9.81f;

    [Header("--------------------------------------------------------------------------")]

    [Header("Spring Visual Customization: ")]
    public int springCoilSegments = 100;
    public float springCoilRadius = 0.2f;
    public float springCoilFrequency = 12f;

    [Header("--------------------------------------------------------------------------")]

    [Header("Chain/Array Properties: ")]
    public bool formChain = false;
    [Space]
    public int chainCount;
    public float chainPositionSpacing;
    public float initialDisplacementSpacing;

    [Header("--------------------------------------------------------------------------")]

    [Header("Oscillator Data List: ")]
    public List<SpringDampedOscillator> oscillators = new List<SpringDampedOscillator>();

    [Header("--------------------------------------------------------------------------")]

    [Header("Prefabs & Materials: ")]
    public GameObject massPrefab;
    public Material lineMaterial;

    private void Start()
    {
        if (massPrefab == null)
        {
            Debug.LogError("Mass prefab is not assigned. Please assign a mass prefab in the inspector.");
            return;
        }

        if (graphContainer == null || graphPointObjectPrefab == null)
        {
            Debug.LogError("Graph container or graph point prefab is not assigned. Please assign them in the inspector.");
            return;
        }

        SetupOscillators();
    }

    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime * TimeScale;

        float graphX = 0f;
        float graphY = 0f;
        bool hasGraphData = false;

        for (int i = 0; i < oscillators.Count; i++)
        {
            SpringDampedOscillator currentOscillator = oscillators[i];

            //Skip Oscillator if Mass Transform is Not Assigned:
            if (currentOscillator.massTransform == null) continue;

            //Assign Acceleration Derived From Euler-Lagrange Equation for Oscillator:
            float currentDisplacementFromEquilibrium = -currentOscillator.position.y;
            float velocityDamping = -currentOscillator.velocity.y;
            float acceleration = -((currentOscillator.springConstant / currentOscillator.mass) * currentDisplacementFromEquilibrium) - ((currentOscillator.dampingCoefficient / currentOscillator.mass) * velocityDamping) + currentOscillator.gravity;
            currentOscillator.acceleration = new Vector3(0, -acceleration, 0);

            //Semi-Implicit Euler Integration:
            currentOscillator.velocity += currentOscillator.acceleration * deltaTime;
            currentOscillator.position += currentOscillator.velocity * deltaTime;

            //Calculate New Position:
            currentOscillator.massTransform.localPosition = currentOscillator.position + currentOscillator.equilibriumPosition;

            //Update Line Renderer:
            UpdateSpringVisual(currentOscillator);

            //Capture Graph Data for First Oscillator:
            if (i == 0)
            {
                graphX = Time.time;
                graphY = currentOscillator.position.y;
                hasGraphData = true;
            }

            //Store Updated Oscillator Data:
            oscillators[i] = currentOscillator;
        }

        if (hasGraphData && graphContainer != null && graphPointObjectPrefab != null)
        {
            VisualizeGraph(graphX, graphY);
        }
    }

    private void SetupOscillators()
    {
        if (formChain)
        {
            //Clear Existing Inspected Oscillator Data:
            oscillators.Clear();

            for (int i = 0; i < chainCount; i++)
            {
                SpringDampedOscillator newOscillator = new SpringDampedOscillator();

                //Assign Default Physical Properties:
                newOscillator.mass = defaultMass;
                newOscillator.springLength = defaultSpringLength;
                newOscillator.springConstant = defaultSpringConstant;
                newOscillator.dampingCoefficient = defaultDampingCoefficient;
                newOscillator.gravity = defaultGravity;

                //Calculate Pivot & Equilibrium Positions:
                float positionOffset = i * chainPositionSpacing;
                newOscillator.pivotPosition = new Vector3(0, 0, positionOffset);
                newOscillator.equilibriumPosition = new Vector3(0, -newOscillator.springLength, positionOffset);

                //Calculate Initial Displacement for Each Oscillator in the Chain:
                float displacementOffset = i * initialDisplacementSpacing;
                newOscillator.initialDisplacement = defaultInitialDisplacement + displacementOffset;

                //Add New Oscillator to List:
                oscillators.Add(newOscillator);
            }
        }

        for (int i = 0; i < oscillators.Count; i++)
        {
            SpringDampedOscillator current = oscillators[i];

            //Assign Default Values if Null or Invalid:
            if (current.mass <= 0) current.mass = defaultMass;
            if (current.springLength <= 0) current.springLength = defaultSpringLength;
            if (current.springConstant <= 0) current.springConstant = defaultSpringConstant;
            if (current.dampingCoefficient <= 0) current.dampingCoefficient = defaultDampingCoefficient; 
            if (current.gravity <= 0) current.gravity = defaultGravity;
            if (current.initialDisplacement == 0) current.initialDisplacement = defaultInitialDisplacement;

            //Assign Equilibrium Position Based on Pivot and Spring Length if Not Forming a Chain:
            if (!formChain)
            {
                float equilibriumY = current.pivotPosition.y - current.springLength;
                current.equilibriumPosition = new Vector3(current.pivotPosition.x, equilibriumY, current.pivotPosition.z);
            }

            //Initial Mass Position Based on Initial Displacement:
            current.position = new Vector3(0, -current.initialDisplacement, 0);
            current.velocity = Vector3.zero;
            current.acceleration = Vector3.zero;

            //Instantiate Mass Object:
            GameObject massObject = Instantiate(massPrefab, transform);
            massObject.name = $"Oscillator Mass {i + 1}";
            current.massTransform = massObject.transform;

            // Create a Child GameObject dedicated to the Spring Line Renderer
            GameObject springObj = new GameObject("SpringVisual");
            springObj.transform.SetParent(massObject.transform, false);
            current.springLineRenderer = springObj.AddComponent<LineRenderer>();
            SetupLineRenderer(current.springLineRenderer, Color.red);

            // Create a Child GameObject dedicated to the Damper Line Renderer
            GameObject damperObj = new GameObject("DamperVisual");
            damperObj.transform.SetParent(massObject.transform, false);
            current.dampingLineRenderer = damperObj.AddComponent<LineRenderer>();
            SetupLineRenderer(current.dampingLineRenderer, Color.green);

            //Initial Position:
            current.massTransform.localPosition = current.position + current.equilibriumPosition;

            //Store Updated Pendulum Data:
            oscillators[i] = current;
        }
    }

    private void UpdateSpringVisual(SpringDampedOscillator oscillator)
    {
        Vector3 startPoint = transform.TransformPoint(oscillator.pivotPosition);
        Vector3 endPoint = oscillator.massTransform.position;

        // Calculate a side vector perpendicular to the suspension travel axis to push the lines apart
        Vector3 travelDirection = (endPoint - startPoint).normalized;
        Vector3 lateralOffset = Vector3.Cross(travelDirection, transform.forward).normalized * 0.15f;

        // --- 1. RENDER SPRING (Red Coil) ---
        Vector3 springStart = startPoint - lateralOffset;
        Vector3 springEnd = endPoint - lateralOffset;
        oscillator.springLineRenderer.positionCount = springCoilSegments;

        for (int i = 0; i < springCoilSegments; i++)
        {
            float t = (float)i / (springCoilSegments - 1);
            Vector3 pathPoint = Vector3.Lerp(springStart, springEnd, t);

            if (t > 0.05f && t < 0.95f)
            {
                float wave = Mathf.Sin(t * Mathf.PI * 2f * springCoilFrequency);
                pathPoint += transform.right * wave * springCoilRadius;
            }
            oscillator.springLineRenderer.SetPosition(i, pathPoint);
        }

        // --- 2. RENDER DAMPER (Green Straight Shock Strut) ---
        Vector3 damperStart = startPoint + lateralOffset;
        Vector3 damperEnd = endPoint + lateralOffset;

        // A damper is a straight piston telescopic cylinder, so it only needs 2 points!
        oscillator.dampingLineRenderer.positionCount = 2;
        oscillator.dampingLineRenderer.SetPosition(0, damperStart);
        oscillator.dampingLineRenderer.SetPosition(1, damperEnd);
    }

    private void VisualizeGraph(float x, float y)
    {
        //Instantiate a New Graph Point Inside the Graph Container:
        GameObject point = Instantiate(graphPointObjectPrefab, graphContainer, false);

        //Get The Transform of The Graph Point Inside The Graph Container:
        RectTransform pointRect = point.GetComponent<RectTransform>();

        //Anchor Its Position Based on The Provided x and y Values Multiplied by The Graph Scale:
        pointRect.anchoredPosition = new Vector2(x * graphScale, y * graphScale);

        //Store The New Graph Point in The Queue:
        activeGraphPoints.Enqueue(point);

        //If The Queue Reaches The Maximum Number of Data Points, Dequeue the Oldest Point, Then Destroy It to Free Up Resources:
        if (activeGraphPoints.Count > maxDataPoints)
        {
            GameObject oldPoint = activeGraphPoints.Dequeue();
            Destroy(oldPoint);
        }
    }

    private void SetupLineRenderer(LineRenderer line, Color debugColor)
    {
        line.positionCount = springCoilSegments;
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));

        line.startColor = debugColor;
        line.endColor = debugColor;

        line.useWorldSpace = true;
    }
}
