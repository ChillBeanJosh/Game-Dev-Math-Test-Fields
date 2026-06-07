using System.Collections.Generic;
using UnityEngine;

public class HalfCar : MonoBehaviour
{
    [System.Serializable]
    public struct HalfCarSystem
    {
        [Header("Mass Profiles:")]
        public float chassisMass;              // m
        public float chassisInertiaZ;          // I_z (Pitch Inertia around local transverse axis)
        public float wheelMassFront;           // m1
        public float wheelMassRear;            // m2

        [Header("Chassis Geometry (Distance from COM):")]
        public float distToFront_a1;           // a1
        public float distToRear_a2;            // a2

        [Header("Front Spring Damper Profiles:")]
        public float suspensionLengthFront;
        public float suspensionConstantK1;     // k1
        public float dampingCoefficientC1;     // c1

        [Header("Rear Spring Damper Profiles:")]
        public float suspensionLengthRear;
        public float suspensionConstantK2;     // k2
        public float dampingCoefficientC2;     // c2

        [Header("Tire Spring Profiles:")]
        public float tireLengthFront;
        public float tireConstantKt1;          // k_t1
        [Space]
        public float tireLengthRear;
        public float tireConstantKt2;          // k_t2

        [Header("Position Properties: ")]
        public Vector3 chassisPosition;
        public Vector3 chassisVelocity;
        public Vector3 chassisAcceleration;
        [Space(10)]
        public Vector3 chassisAngle;                // Stored in Radians. X = Pitch, Y = Yaw, Z = Roll
        public Vector3 chassisAngularVelocity;
        public Vector3 chassisAngularAcceleration;
        [Space(20)]
        public Vector3 frontWheelPosition;
        public Vector3 frontWheelVelocity;
        public Vector3 frontWheelAcceleration;
        [Space(20)]
        public Vector3 rearWheelPosition;
        public Vector3 rearWheelVelocity;
        public Vector3 rearWheelAcceleration;

        [Header("World Reference Alignments")]
        public Vector3 pivotPosition;
        public Vector3 chassisEquilibrium;
        public Vector3 frontWheelEquilibrium;
        public Vector3 rearWheelEquilibrium;

        [HideInInspector] public Transform chassisTransform;
        [HideInInspector] public Transform wheelTransformFront;
        [HideInInspector] public Transform wheelTransformRear;

        [HideInInspector] public LineRenderer suspensionSpringFrontLine;
        [HideInInspector] public LineRenderer suspensionDamperFrontLine;
        [HideInInspector] public LineRenderer tireSpringFrontLine;
        [Space]
        [HideInInspector] public LineRenderer suspensionSpringRearLine;
        [HideInInspector] public LineRenderer suspensionDamperRearLine;
        [HideInInspector] public LineRenderer tireSpringRearLine;
    }

    private struct TriGraphPoints
    {
        public GameObject chassisPoint;
        public GameObject wheelPointFront;
        public GameObject wheelPointRear;
    }

    [Header("--------------------------------------------------------------------------")]

    [Header("Graph Settings:")]
    public RectTransform graphContainer;
    public float graphScale = 100f;
    [Space]
    public GameObject graphPointPrefabChassis;
    public GameObject graphPointPrefabFrontWheel;
    public GameObject graphPointPrefabRearWheel;
    [Space]
    public int maxDataPoints = 200;
    private Queue<TriGraphPoints> activeGraphPoints = new Queue<TriGraphPoints>();

    [Header("--------------------------------------------------------------------------")]

    [Header("Simulation Settings:")]
    [Range(0.1f, 5f)] public float TimeScale = 1.0f;
    public float globalGravity = 9.81f;
    [Space]
    public LayerMask groundLayer;

    [Header("--------------------------------------------------------------------------")]

    [Header("Global Default Properties:")]
    public float defaultChassisMass = 800f;
    public float defaultChassisInertiaZ = 1200f;
    public float defaultWheelMassFront = 40f;
    public float defaultWheelMassRear = 45f;
    [Space]
    public float defaultDistA1 = 1.2f;
    public float defaultDistA2 = 1.4f;
    [Space]
    public float defaultSuspensionLengthFront = 1.5f;
    public float defaultSuspensionKFront = 25000f;
    public float defaultDampingCFront = 1800f;
    [Space]
    public float defaultSuspensionLengthRear = 1.5f;
    public float defaultSuspensionKRear = 28000f;
    public float defaultDampingCRear = 2000f;
    [Space]
    public float defaultTireLengthFront = 0.4f;
    public float defaultTireKtFront = 120000f;
    [Space]
    public float defaultTireLengthRear = 0.4f;
    public float defaultTireKtRear = 130000f;

    [Header("--------------------------------------------------------------------------")]

    [Header("Spring Visual Customization:")]
    public int springCoilSegments = 60;
    public float springCoilRadius = 0.12f;
    public float springCoilFrequency = 10f;

    [Header("--------------------------------------------------------------------------")]

    [Header("Chain/Array Properties:")]
    public bool formChain = false;
    public int chainCount = 1;
    public float chainSpacingX = 4.0f;
    public float initialDisplacementSpacing = 0.2f;

    [Header("--------------------------------------------------------------------------")]

    [Header("Half Car System Data List:")]
    public List<HalfCarSystem> halfCars = new List<HalfCarSystem>();

    [Header("--------------------------------------------------------------------------")]

    [Header("Prefabs & Materials:")]
    public GameObject chassisPrefab;
    public GameObject wheelPrefabFront;
    public GameObject wheelPrefabRear;
    public Material lineMaterial;

    private void Start()
    {
        if (chassisPrefab == null || wheelPrefabFront == null || wheelPrefabRear == null)
        {
            Debug.LogError("Required Prefabs are unassigned in the inspector.");
            return;
        }

        SetupCarSystems();
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime * TimeScale;

        float graphTimeX = 0f;
        float graphChassisY = 0f;
        float graphWheelFrontY = 0f;
        float graphWheelRearY = 0f;
        bool hasGraphData = false;

        for (int i = 0; i < halfCars.Count; i++)
        {
            HalfCarSystem car = halfCars[i];

            if (car.chassisTransform == null || car.wheelTransformFront == null || car.wheelTransformRear == null)
                continue;

            // 1. Sample ground track heights relative to front and rear wheels along the Z axis
            float roadY1 = SampleRoadHeightAtPoint(car.pivotPosition.x, car.pivotPosition.z + car.distToFront_a1);
            float roadY2 = SampleRoadHeightAtPoint(car.pivotPosition.x, car.pivotPosition.z - car.distToRear_a2);

            // 2. Extract translational states and our vectorized pitch elements (X axis)
            float x = car.chassisPosition.y;
            float xDot = car.chassisVelocity.y;

            float theta = car.chassisAngle.x;
            float thetaDot = car.chassisAngularVelocity.x;

            float x1 = car.frontWheelPosition.y;
            float x1Dot = car.frontWheelVelocity.y;
            float x2 = car.rearWheelPosition.y;
            float x2Dot = car.rearWheelVelocity.y;

            // 3. Structural geometry calculations
            float suspDeltaPosFront = x - x1 - (car.distToFront_a1 * theta);
            float suspDeltaVelFront = xDot - x1Dot - (car.distToFront_a1 * thetaDot);

            float suspDeltaPosRear = x - x2 + (car.distToRear_a2 * theta);
            float suspDeltaVelRear = xDot - x2Dot + (car.distToRear_a2 * thetaDot);

            float tireDeltaPosFront = x1 - roadY1;
            float tireDeltaPosRear = x2 - roadY2;

            // 4. One-way constraint contact calculations
            float forceTireFront = (tireDeltaPosFront < 0f) ? car.tireConstantKt1 * tireDeltaPosFront : 0f;
            float forceTireRear = (tireDeltaPosRear < 0f) ? car.tireConstantKt2 * tireDeltaPosRear : 0f;

            // Structural internal suspension loading forces
            float F1 = (car.suspensionConstantK1 * suspDeltaPosFront) + (car.dampingCoefficientC1 * suspDeltaVelFront);
            float F2 = (car.suspensionConstantK2 * suspDeltaPosRear) + (car.dampingCoefficientC2 * suspDeltaVelRear);

            // 5. Analytical acceleration values derived from EOM
            float accBounce = (-F1 - F2) / car.chassisMass - globalGravity;
            float accPitch = (car.distToFront_a1 * F1 - car.distToRear_a2 * F2) / car.chassisInertiaZ;
            float accWheelFront = (F1 - forceTireFront) / car.wheelMassFront - globalGravity;
            float accWheelRear = (F2 - forceTireRear) / car.wheelMassRear - globalGravity;

            // 6. Fully Vectorized Semi-Implicit Integration
            // Chassis Translation
            car.chassisAcceleration = new Vector3(0, accBounce, 0);
            car.chassisVelocity += car.chassisAcceleration * dt;
            car.chassisPosition += car.chassisVelocity * dt;

            // Chassis Rotation (Calculated pitch loaded into Vector3.x)
            car.chassisAngularAcceleration = new Vector3(accPitch, 0, 0);
            car.chassisAngularVelocity += car.chassisAngularAcceleration * dt;
            car.chassisAngle += car.chassisAngularVelocity * dt;

            // Front Wheel Translation
            car.frontWheelAcceleration = new Vector3(0, accWheelFront, 0);
            car.frontWheelVelocity += car.frontWheelAcceleration * dt;
            car.frontWheelPosition += car.frontWheelVelocity * dt;

            // Rear Wheel Translation
            car.rearWheelAcceleration = new Vector3(0, accWheelRear, 0);
            car.rearWheelVelocity += car.rearWheelAcceleration * dt;
            car.rearWheelPosition += car.rearWheelVelocity * dt;

            // 7. Render local spatial transformation properties
            car.chassisTransform.localPosition = car.chassisEquilibrium + car.chassisPosition;
            car.chassisTransform.localRotation = Quaternion.Euler(car.chassisAngle.x * Mathf.Rad2Deg, car.chassisAngle.y * Mathf.Rad2Deg, car.chassisAngle.z * Mathf.Rad2Deg);

            car.wheelTransformFront.localPosition = car.frontWheelEquilibrium + car.frontWheelPosition;
            car.wheelTransformRear.localPosition = car.rearWheelEquilibrium + car.rearWheelPosition;

            // 8. Line Renderer Visual Passes
            UpdateHalfCarVisuals(car, roadY1, roadY2);

            if (i == 0)
            {
                graphTimeX = Time.time;
                graphChassisY = car.chassisPosition.y;
                graphWheelFrontY = car.frontWheelPosition.y;
                graphWheelRearY = car.rearWheelPosition.y;
                hasGraphData = true;
            }

            halfCars[i] = car;
        }

        if (hasGraphData && graphContainer != null && graphPointPrefabChassis != null && graphPointPrefabFrontWheel != null && graphPointPrefabRearWheel != null)
        {
            VisualizeTriGraph(graphTimeX, graphChassisY, graphWheelFrontY, graphWheelRearY);
        }
    }

    private void SetupCarSystems()
    {
        if (formChain)
        {
            halfCars.Clear();
            for (int i = 0; i < chainCount; i++)
            {
                HalfCarSystem newCar = new HalfCarSystem();

                newCar.chassisMass = defaultChassisMass;
                newCar.chassisInertiaZ = defaultChassisInertiaZ;
                newCar.wheelMassFront = defaultWheelMassFront;
                newCar.wheelMassRear = defaultWheelMassRear;
                newCar.distToFront_a1 = defaultDistA1;
                newCar.distToRear_a2 = defaultDistA2;

                newCar.suspensionLengthFront = defaultSuspensionLengthFront;
                newCar.suspensionConstantK1 = defaultSuspensionKFront;
                newCar.dampingCoefficientC1 = defaultDampingCFront;

                newCar.suspensionLengthRear = defaultSuspensionLengthRear;
                newCar.suspensionConstantK2 = defaultSuspensionKRear;
                newCar.dampingCoefficientC2 = defaultDampingCRear;

                newCar.tireLengthFront = defaultTireLengthFront;
                newCar.tireConstantKt1 = defaultTireKtFront;

                newCar.tireLengthRear = defaultTireLengthRear;
                newCar.tireConstantKt2 = defaultTireKtRear;

                float lateralOffset = i * chainSpacingX;
                newCar.pivotPosition = new Vector3(lateralOffset, 0, 0);
                newCar.chassisPosition = new Vector3(0, i * initialDisplacementSpacing, 0);

                halfCars.Add(newCar);
            }
        }

        for (int i = 0; i < halfCars.Count; i++)
        {
            HalfCarSystem current = halfCars[i];

            if (current.chassisMass <= 0) current.chassisMass = defaultChassisMass;
            if (current.chassisInertiaZ <= 0) current.chassisInertiaZ = defaultChassisInertiaZ;
            if (current.wheelMassFront <= 0) current.wheelMassFront = defaultWheelMassFront;
            if (current.wheelMassRear <= 0) current.wheelMassRear = defaultWheelMassRear;
            if (current.distToFront_a1 <= 0) current.distToFront_a1 = defaultDistA1;
            if (current.distToRear_a2 <= 0) current.distToRear_a2 = defaultDistA2;
            if (current.suspensionLengthFront <= 0) current.suspensionLengthFront = defaultSuspensionLengthFront;
            if (current.suspensionLengthRear <= 0) current.suspensionLengthRear = defaultSuspensionLengthRear;
            if (current.tireLengthFront <= 0) current.tireLengthFront = defaultTireLengthFront;
            if (current.tireLengthRear <= 0) current.tireLengthRear = defaultTireLengthRear;

            float xPos = current.pivotPosition.x;
            current.frontWheelEquilibrium = new Vector3(xPos, current.tireLengthFront, current.distToFront_a1);
            current.rearWheelEquilibrium = new Vector3(xPos, current.tireLengthRear, -current.distToRear_a2);

            // Equilibrium baseline for chassis accounts for its height relative to the front axle baseline configuration
            current.chassisEquilibrium = new Vector3(xPos, current.tireLengthFront + current.suspensionLengthFront, 0f);

            GameObject chassisObj = Instantiate(chassisPrefab, transform);
            chassisObj.name = $"Half Car Chassis {i + 1}";
            current.chassisTransform = chassisObj.transform;

            GameObject wheelFrontObj = Instantiate(wheelPrefabFront, transform);
            wheelFrontObj.name = $"Front Wheel {i + 1}";
            current.wheelTransformFront = wheelFrontObj.transform;

            GameObject wheelRearObj = Instantiate(wheelPrefabRear, transform);
            wheelRearObj.name = $"Rear Wheel {i + 1}";
            current.wheelTransformRear = wheelRearObj.transform;

            current.suspensionSpringFrontLine = CreateLineVisual(chassisObj.transform, "FrontSpring", Color.red);
            current.suspensionDamperFrontLine = CreateLineVisual(chassisObj.transform, "FrontDamper", Color.green);
            current.tireSpringFrontLine = CreateLineVisual(wheelFrontObj.transform, "FrontTireLine", Color.blue);

            current.suspensionSpringRearLine = CreateLineVisual(chassisObj.transform, "RearSpring", Color.red);
            current.suspensionDamperRearLine = CreateLineVisual(chassisObj.transform, "RearDamper", Color.green);
            current.tireSpringRearLine = CreateLineVisual(wheelRearObj.transform, "RearTireLine", Color.blue);

            current.chassisTransform.localPosition = current.chassisEquilibrium + current.chassisPosition;
            current.wheelTransformFront.localPosition = current.frontWheelEquilibrium + current.frontWheelPosition;
            current.wheelTransformRear.localPosition = current.rearWheelEquilibrium + current.rearWheelPosition;

            halfCars[i] = current;
        }
    }

    private void UpdateHalfCarVisuals(HalfCarSystem car, float roadFrontY, float roadRearY)
    {
        Vector3 frontSuspChassisWorld = car.chassisTransform.TransformPoint(new Vector3(0, 0, car.distToFront_a1));
        Vector3 rearSuspChassisWorld = car.chassisTransform.TransformPoint(new Vector3(0, 0, -car.distToRear_a2));

        Vector3 hubFrontWorld = car.wheelTransformFront.position;
        Vector3 hubRearWorld = car.wheelTransformRear.position;

        Vector3 contactFrontWorld = transform.TransformPoint(new Vector3(car.pivotPosition.x, roadFrontY, car.distToFront_a1));
        Vector3 contactRearWorld = transform.TransformPoint(new Vector3(car.pivotPosition.x, roadRearY, -car.distToRear_a2));

        Vector3 lateralOffset = transform.right * 0.15f;

        RenderCoilSpring(car.suspensionSpringFrontLine, frontSuspChassisWorld - lateralOffset, hubFrontWorld - lateralOffset);
        RenderStraightLine(car.suspensionDamperFrontLine, frontSuspChassisWorld + lateralOffset, hubFrontWorld + lateralOffset);
        RenderStraightLine(car.tireSpringFrontLine, hubFrontWorld, contactFrontWorld);

        RenderCoilSpring(car.suspensionSpringRearLine, rearSuspChassisWorld - lateralOffset, hubRearWorld - lateralOffset);
        RenderStraightLine(car.suspensionDamperRearLine, rearSuspChassisWorld + lateralOffset, hubRearWorld + lateralOffset);
        RenderStraightLine(car.tireSpringRearLine, hubRearWorld, contactRearWorld);
    }

    private float SampleRoadHeightAtPoint(float worldX, float worldZ)
    {
        Vector3 rayOriginWorld = transform.TransformPoint(new Vector3(worldX, 40f, worldZ));
        if (Physics.Raycast(rayOriginWorld, Vector3.down, out RaycastHit hit, 100f, groundLayer))
        {
            return transform.InverseTransformPoint(hit.point).y;
        }
        return 0f;
    }

    private LineRenderer CreateLineVisual(Transform parent, string name, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        LineRenderer line = obj.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = 0.04f;
        line.endWidth = 0.04f;
        line.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));
        line.startColor = color;
        line.endColor = color;
        line.useWorldSpace = true;
        return line;
    }

    private void RenderStraightLine(LineRenderer line, Vector3 start, Vector3 end)
    {
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    private void RenderCoilSpring(LineRenderer line, Vector3 start, Vector3 end)
    {
        line.positionCount = springCoilSegments;
        for (int j = 0; j < springCoilSegments; j++)
        {
            float t = (float)j / (springCoilSegments - 1);
            Vector3 point = Vector3.Lerp(start, end, t);
            if (t > 0.04f && t < 0.96f)
            {
                float wave = Mathf.Sin(t * Mathf.PI * 2f * springCoilFrequency);
                point += transform.right * wave * springCoilRadius;
            }
            line.SetPosition(j, point);
        }
    }

    private void VisualizeTriGraph(float timeX, float chassisY, float wheelFrontY, float wheelRearY)
    {
        GameObject cPoint = Instantiate(graphPointPrefabChassis, graphContainer, false);
        RectTransform cRect = cPoint.GetComponent<RectTransform>();
        cRect.anchoredPosition = new Vector2(timeX * graphScale, chassisY * graphScale);

        GameObject wfPoint = Instantiate(graphPointPrefabFrontWheel, graphContainer, false);
        RectTransform wfRect = wfPoint.GetComponent<RectTransform>();
        wfRect.anchoredPosition = new Vector2(timeX * graphScale, wheelFrontY * graphScale);

        GameObject wrPoint = Instantiate(graphPointPrefabRearWheel, graphContainer, false);
        RectTransform wrRect = wrPoint.GetComponent<RectTransform>();
        wrRect.anchoredPosition = new Vector2(timeX * graphScale, wheelRearY * graphScale);

        TriGraphPoints frame = new TriGraphPoints { chassisPoint = cPoint, wheelPointFront = wfPoint, wheelPointRear = wrPoint };
        activeGraphPoints.Enqueue(frame);

        if (activeGraphPoints.Count > maxDataPoints)
        {
            TriGraphPoints oldest = activeGraphPoints.Dequeue();
            if (oldest.chassisPoint != null) Destroy(oldest.chassisPoint);
            if (oldest.wheelPointFront != null) Destroy(oldest.wheelPointFront);
            if (oldest.wheelPointRear != null) Destroy(oldest.wheelPointRear);
        }
    }
}
