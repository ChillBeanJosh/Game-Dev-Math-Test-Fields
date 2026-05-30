using System.Collections.Generic;
using UnityEngine;

public class DoublePendulumHamiltonian : MonoBehaviour
{
    [System.Serializable]
    public struct DoublePendulumSystem
    {
        [Header("Pendulum 1 Properties: ")]
        public float l1;
        public float m1;
        public float theta1;
        public float p1;


        [Header("Pendulum 2 Properties: ")]
        public float l2;
        public float m2;
        public float theta2;
        public float p2;

        [Header("Global Properties: ")]
        public float gravity;

        [Header("Position: ")]
        public Vector3 pivotPosition;

        [HideInInspector] public Transform mass1Transform;
        [HideInInspector] public LineRenderer lineRenderer1;

        [HideInInspector] public Transform mass2Transform;
        [HideInInspector] public LineRenderer lineRenderer2;
    }
    [Header("--------------------------------------------------------------------------")]

    [Header("Graph Settings: ")]
    public RectTransform graphContainer;
    public float graphScale;

    [Header("Graph Point Settings: ")]
    public PendulumTrail trail1;
    public PendulumTrail trail2;

    [Header("Visual Settings: ")]
    public List<Color> systemColors = new List<Color>() { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta };
    [Space]
    public bool useGradient = false;
    public Gradient trailGradient;

    [Header("--------------------------------------------------------------------------")]

    [Header("Simulation Settings")]
    [Range(0.1f, 5f)] public float TimeScale;

    [Header("Global Properties: ")]
    public float defaultLength;
    public float defaultMass;
    [Space]
    public float defaultGravity = 9.81f;
    [Space]
    public float initialAngle1 = 45f;
    public float initialAngle2 = 45f;

    [Header("--------------------------------------------------------------------------")]

    [Header("Chain Properties: ")]
    public bool formChain = false;
    [Space]
    public int chainCount;
    public float chainPositionSpacing;
    public float initialChainAngle;
    public float chainAngleSpacing;

    [Header("--------------------------------------------------------------------------")]

    [Header("Pendulum Systems Data List: ")]
    public List<DoublePendulumSystem> systems = new List<DoublePendulumSystem>();

    [Header("--------------------------------------------------------------------------")]

    [Header("Prefabs & Materials")]
    public GameObject massPrefab;
    public Material lineMaterial;

    private void Start()
    {
        if (massPrefab == null)
        {
            Debug.LogError("Mass prefab is not assigned!");
            return;
        }

        if (graphContainer == null || trail1 == null || trail2 == null)
        {
            Debug.LogError("Graph container or trails are not assigned!");
            return;
        }

        SetupSystems();
    }

    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime * TimeScale;

        for (int i = 0; i < systems.Count; i++)
        {
            DoublePendulumSystem currentDoublePendulum = systems[i];

            // --------------------------------------------------------

            //Skip Pendulum Systems if Mass 1 OR Mass 2 is Not Assigned:
            if (currentDoublePendulum.mass1Transform == null || currentDoublePendulum.mass2Transform == null) continue;

            // --------------------------------------------------------

            //Assign deltaTheta, cosDelta, and sinDelta to Avoid Redundant Calculations in Both Velocity and Momentum Derivative Evaluations:
            float deltaTheta = currentDoublePendulum.theta1 - currentDoublePendulum.theta2;
            float cosDelta = Mathf.Cos(deltaTheta);
            float sinDelta = Mathf.Sin(deltaTheta);
            float denominatorCommon = currentDoublePendulum.m1 + currentDoublePendulum.m2 * sinDelta * sinDelta;

            // --------------------------------------------------------

            //Angular Velocity For First Pendulum:
            float dTheta1 = (currentDoublePendulum.l2 * currentDoublePendulum.p1 - currentDoublePendulum.l1 * currentDoublePendulum.p2 * cosDelta) /
                             (currentDoublePendulum.l1 * currentDoublePendulum.l1 * currentDoublePendulum.l2 * denominatorCommon);

            //Angular Velocity For Second Pendulum:
            float dTheta2 = (currentDoublePendulum.l1 * (currentDoublePendulum.m1 + currentDoublePendulum.m2) * currentDoublePendulum.p2 - currentDoublePendulum.l2 * currentDoublePendulum.m2 * currentDoublePendulum.p1 * cosDelta) /
                             (currentDoublePendulum.l1 * currentDoublePendulum.l2 * currentDoublePendulum.l2 * currentDoublePendulum.m2 * denominatorCommon);

            // --------------------------------------------------------

            //Gravitational Terms For First Pendulum:
            float gTerm1 = -(currentDoublePendulum.m1 + currentDoublePendulum.m2) * currentDoublePendulum.gravity * currentDoublePendulum.l1 * Mathf.Sin(currentDoublePendulum.theta1);

            //Gravitational Terms For Second Pendulum:
            float gTerm2 = -currentDoublePendulum.m2 * currentDoublePendulum.gravity * currentDoublePendulum.l2 * Mathf.Sin(currentDoublePendulum.theta2);

            // --------------------------------------------------------

            //Hamiltonian Terms:
            float h1 = (currentDoublePendulum.p1 * currentDoublePendulum.p2 * sinDelta) / (currentDoublePendulum.l1 * currentDoublePendulum.l2 * denominatorCommon);
            float h2 = (currentDoublePendulum.m2 * currentDoublePendulum.l2 * currentDoublePendulum.l2 * currentDoublePendulum.p1 * currentDoublePendulum.p1 + (currentDoublePendulum.m1 + currentDoublePendulum.m2) * currentDoublePendulum.l1 * currentDoublePendulum.l1 * currentDoublePendulum.p2 * currentDoublePendulum.p2 - 2f * currentDoublePendulum.m2 * currentDoublePendulum.l1 * currentDoublePendulum.l2 * currentDoublePendulum.p1 * currentDoublePendulum.p2 * cosDelta);
            float h3 = Mathf.Sin(2f * deltaTheta) / (2f * currentDoublePendulum.m2 * currentDoublePendulum.l1 * currentDoublePendulum.l1 * currentDoublePendulum.l2 * currentDoublePendulum.l2 * denominatorCommon * denominatorCommon);

            // --------------------------------------------------------

            //Derivatives of Momentum For First Pendulum:
            float dP1 = gTerm1 - h1 + h2 * h3;

            //Derivatives of Momentum For Second Pendulum:
            float dP2 = gTerm2 + h1 - h2 * h3;

            // --------------------------------------------------------

            // --- Time Integration (Explicit Symplectic Phase Update) ---

            // 1. Update Momenta First
            currentDoublePendulum.p1 += dP1 * deltaTime;
            currentDoublePendulum.p2 += dP2 * deltaTime;

            // 2. CRITICAL: Recalculate geometric conditions using the updated state before position integration
            deltaTheta = currentDoublePendulum.theta1 - currentDoublePendulum.theta2;
            cosDelta = Mathf.Cos(deltaTheta);
            sinDelta = Mathf.Sin(deltaTheta);
            denominatorCommon = currentDoublePendulum.m1 + currentDoublePendulum.m2 * sinDelta * sinDelta;

            // 3. Compute Fresh Angular Velocities with New Momenta
            dTheta1 = (currentDoublePendulum.l2 * currentDoublePendulum.p1 - currentDoublePendulum.l1 * currentDoublePendulum.p2 * cosDelta) / (currentDoublePendulum.l1 * currentDoublePendulum.l1 * currentDoublePendulum.l2 * denominatorCommon);
            dTheta2 = (currentDoublePendulum.l1 * (currentDoublePendulum.m1 + currentDoublePendulum.m2) * currentDoublePendulum.p2 - currentDoublePendulum.l2 * currentDoublePendulum.m2 * currentDoublePendulum.p1 * cosDelta) / (currentDoublePendulum.l1 * currentDoublePendulum.l2 * currentDoublePendulum.l2 * currentDoublePendulum.m2 * denominatorCommon);

            // 4. Update Angular Positions Second
            currentDoublePendulum.theta1 += dTheta1 * deltaTime;
            currentDoublePendulum.theta2 += dTheta2 * deltaTime;

            // --------------------------------------------------------

            //Calculate New Position for Mass 1:
            float x1 = currentDoublePendulum.l1 * Mathf.Sin(currentDoublePendulum.theta1);
            float y1 = -currentDoublePendulum.l1 * Mathf.Cos(currentDoublePendulum.theta1);
            Vector3 pos1 = currentDoublePendulum.pivotPosition + new Vector3(x1, y1, 0);

            //Calculate New Position for Mass 2:
            float x2 = pos1.x + currentDoublePendulum.l2 * Mathf.Sin(currentDoublePendulum.theta2);
            float y2 = pos1.y - currentDoublePendulum.l2 * Mathf.Cos(currentDoublePendulum.theta2);
            Vector3 pos2 = new Vector3(x2, y2, currentDoublePendulum.pivotPosition.z);

            //Apply New Positions to Mass Transforms:
            currentDoublePendulum.mass1Transform.localPosition = pos1;
            currentDoublePendulum.mass2Transform.localPosition = pos2;

            // --------------------------------------------------------

            //Update Line Renderers For First Mass:
            currentDoublePendulum.lineRenderer1.SetPosition(0, currentDoublePendulum.pivotPosition);
            currentDoublePendulum.lineRenderer1.SetPosition(1, currentDoublePendulum.mass1Transform.position);

            //Update Line Renderer For Second Mass:
            currentDoublePendulum.lineRenderer2.SetPosition(0, currentDoublePendulum.mass1Transform.position);
            currentDoublePendulum.lineRenderer2.SetPosition(1, currentDoublePendulum.mass2Transform.position);

            // --------------------------------------------------------

            if (useGradient)
            {
                //Calculate Gradient Color Based on System Index:
                float gradientTime = systems.Count > 1 ? (float)i / (systems.Count - 1) : 0f;
                Color pathColor = trailGradient.Evaluate(gradientTime);

                trail1.AddPoint(i, new Vector2(x1, y1) * graphScale, pathColor);
                trail2.AddPoint(i, new Vector2(x2, y2) * graphScale, pathColor);
            }
            else
            {
                //Use Static System Colors for Trails:
                Color pathColor = systemColors[i % systemColors.Count];

                trail1.AddPoint(i, new Vector2(x1, y1) * graphScale, pathColor);
                trail2.AddPoint(i, new Vector2(x2, y2) * graphScale, pathColor);
            }

            // --------------------------------------------------------

            //Store Updated Pendulum System Back to List:
            systems[i] = currentDoublePendulum;
        }
    }

    private void SetupSystems()
    {
        if (formChain)
        {
            //Clear Existing Systems Before Forming Chain:
            systems.Clear();

            for (int i = 0; i < chainCount; i++)
            {
                DoublePendulumSystem newDoublePendulum = new DoublePendulumSystem();

                // --------------------------------------------------------

                //Assign Default Properties For Mass 1:
                newDoublePendulum.l1 = defaultLength;
                newDoublePendulum.m1 = defaultMass;

                //Assign Default Properties For Mass 2:
                newDoublePendulum.l2 = defaultLength;
                newDoublePendulum.m2 = defaultMass;

                //Assign Default Gravity:
                newDoublePendulum.gravity = defaultGravity;

                // --------------------------------------------------------

                //Calculate Pivot Position:
                float positionOffset = i * chainPositionSpacing;
                newDoublePendulum.pivotPosition = new Vector3(0, 0, positionOffset);

                //Calculate Initial Angles:
                float angleOffset = initialChainAngle + (i * chainAngleSpacing);
                newDoublePendulum.theta1 = initialAngle1;
                newDoublePendulum.theta2 = angleOffset;

                // --------------------------------------------------------

                //Add New Pendulum System to List:
                systems.Add(newDoublePendulum);
            }
        }

        for (int i = 0; i < systems.Count; i++)
        {
            DoublePendulumSystem currentDoublePendulum = systems[i];

            // --------------------------------------------------------

            //Gravity Initialization:
            if (currentDoublePendulum.gravity <= 0) currentDoublePendulum.gravity = defaultGravity;

            //First Pendulum Initialization:
            if (currentDoublePendulum.l1 <= 0) currentDoublePendulum.l1 = defaultLength;
            if (currentDoublePendulum.m1 <= 0) currentDoublePendulum.m1 = defaultMass;
            currentDoublePendulum.theta1 *= Mathf.Deg2Rad;
            currentDoublePendulum.p1 = 0f;

            //Second Pendulum Initialization:
            if (currentDoublePendulum.l2 <= 0) currentDoublePendulum.l2 = defaultLength;
            if (currentDoublePendulum.m2 <= 0) currentDoublePendulum.m2 = defaultMass;
            currentDoublePendulum.theta2 *= Mathf.Deg2Rad;
            currentDoublePendulum.p2 = 0f;

            // --------------------------------------------------------

            //Instantiate Mass 1 Object:
            GameObject massObject1 = Instantiate(massPrefab, transform);
            massObject1.name = $"System {i + 1} - Mass 1";
            currentDoublePendulum.mass1Transform = massObject1.transform;

            //Setup Line Renderer for Mass 1:
            currentDoublePendulum.lineRenderer1 = massObject1.GetComponent<LineRenderer>();
            if (currentDoublePendulum.lineRenderer1 == null)
            {
                currentDoublePendulum.lineRenderer1 = massObject1.AddComponent<LineRenderer>();
            }
            SetupLineRenderer(currentDoublePendulum.lineRenderer1);

            // --------------------------------------------------------

            //Instantiate Mass 2 Object:
            GameObject massObject2 = Instantiate(massPrefab, transform);
            massObject2.name = $"System {i + 1} - Mass 2";
            currentDoublePendulum.mass2Transform = massObject2.transform;

            //Setup Line Renderer for Mass 2:
            currentDoublePendulum.lineRenderer2 = massObject2.GetComponent<LineRenderer>();
            if (currentDoublePendulum.lineRenderer2 == null)
            {
                currentDoublePendulum.lineRenderer2 = massObject2.AddComponent<LineRenderer>();
            }
            SetupLineRenderer(currentDoublePendulum.lineRenderer2);

            // --------------------------------------------------------

            //Store Updated Pendulum System Back to List:
            systems[i] = currentDoublePendulum;
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
