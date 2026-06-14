using System.Collections.Generic;
using UnityEngine;

public class AntirollHalfCar : MonoBehaviour
{
    [System.Serializable]
    public struct RollHalfCarSystem
    {
        [Header("Mass Profiles:")]
        public float chassisMass;
        public float chassisInertiaRoll; // I_x in textbook (Roll Inertia)
        [Space]
        public float wheelMassRight; // m_1
        public float wheelMassLeft;  // m_2

        [Header("Chassis Geometry (Distance from COM):")]
        public float distToRight_b1; // b_1
        public float distToLeft_b2;  // b_2

        [Header("Right Spring Damper Profiles:")]
        public float suspensionLengthRight;
        public float suspensionConstantK1;
        public float dampingCoefficientC1;

        [Header("Left Spring Damper Profiles:")]
        public float suspensionLengthLeft;
        public float suspensionConstantK2;
        public float dampingCoefficientC2;

        [Header("Tire Spring Profiles:")]
        public float tireLengthRight;
        public float tireConstantKt1;
        [Space]
        public float tireLengthLeft;
        public float tireConstantKt2;

        [Header("Antiroll Bar Profiles:")]
        public float antirollStiffnessKR; // k_R
        public float antirollBarHeightOffset;
        public bool useAdvancedModel; // Toggles Eq 13.239 vs Eq 13.238

        [Header("Position Properties: ")]
        public Vector3 chassisPosition;
        public Vector3 chassisVelocity;
        public Vector3 chassisAcceleration;
        [Space(10)]
        public Vector3 chassisAngle;
        public Vector3 chassisAngularVelocity;
        public Vector3 chassisAngularAcceleration;
        [Space(20)]
        public Vector3 rightWheelPosition;
        public Vector3 rightWheelVelocity;
        public Vector3 rightWheelAcceleration;
        [Space(20)]
        public Vector3 leftWheelPosition;
        public Vector3 leftWheelVelocity;
        public Vector3 leftWheelAcceleration;

        [Header("World Reference Alignments")]
        public Vector3 pivotPosition;
        public Vector3 chassisEquilibrium;
        public Vector3 rightWheelEquilibrium;
        public Vector3 leftWheelEquilibrium;

        [HideInInspector] public Transform chassisTransform;
        [HideInInspector] public Transform wheelTransformRight;
        [HideInInspector] public Transform wheelTransformLeft;

        [HideInInspector] public LineRenderer suspensionSpringRightLine;
        [HideInInspector] public LineRenderer suspensionDamperRightLine;
        [HideInInspector] public LineRenderer tireSpringRightLine;
        [Space]
        [HideInInspector] public LineRenderer suspensionSpringLeftLine;
        [HideInInspector] public LineRenderer suspensionDamperLeftLine;
        [HideInInspector] public LineRenderer tireSpringLeftLine;

        [HideInInspector] public LineRenderer antirollLinkLeftLine;
        [HideInInspector] public LineRenderer antirollSpringLeftLine;
        [HideInInspector] public LineRenderer antirollBarCenterLine;
        [HideInInspector] public LineRenderer antirollSpringRightLine;
        [HideInInspector] public LineRenderer antirollLinkRightLine;
    }

    private struct TriGraphPoints
    {
        public GameObject chassisPoint;
        public GameObject wheelPointRight;
        public GameObject wheelPointLeft;
    }

    [Header("--------------------------------------------------------------------------")]

    [Header("Graph Settings:")]
    public RectTransform graphContainer;
    public float graphScale = 100f;
    [Space]
    public GameObject graphPointPrefabChassis;
    public GameObject graphPointPrefabRightWheel;
    public GameObject graphPointPrefabLeftWheel;
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
    public float defaultChassisInertiaRoll = 1200f;
    [Space]
    public float defaultWheelMassRight = 40f;
    public float defaultWheelMassLeft = 40f;
    [Space]
    public float defaultDistB1 = 0.8f;
    public float defaultDistB2 = 0.8f;
    [Space]
    public float defaultSuspensionLengthRight = 1.5f;
    public float defaultSuspensionKRight = 25000f;
    public float defaultDampingCRight = 1800f;
    [Space]
    public float defaultSuspensionLengthLeft = 1.5f;
    public float defaultSuspensionKLeft = 25000f;
    public float defaultDampingCLeft = 1800f;
    [Space]
    public float defaultTireLengthRight = 0.4f;
    public float defaultTireKtRight = 120000f;
    [Space]
    public float defaultTireLengthLeft = 0.4f;
    public float defaultTireKtLeft = 120000f;
    [Space]
    public float defaultAntirollKR = 10000f;
    public float defaultAntirollOffset = 0.4f;

    [Header("--------------------------------------------------------------------------")]

    [Header("Spring Visual Customization:")]
    public int springCoilSegments = 60;
    public float springCoilRadius = 0.12f;
    public float springCoilFrequency = 10f;

    [Header("--------------------------------------------------------------------------")]

    [Header("Chain/Array Properties:")]
    public bool formChain = false;
    public int chainCount = 1;
    public float chainSpacingZ = 4.0f;
    public float initialDisplacementSpacing = 0.2f;

    [Header("--------------------------------------------------------------------------")]

    [Header("Half Car System Data List:")]
    public List<RollHalfCarSystem> halfCars = new List<RollHalfCarSystem>();

    [Header("--------------------------------------------------------------------------")]

    [Header("Prefabs & Materials:")]
    public GameObject chassisPrefab;
    public GameObject wheelPrefabRight;
    public GameObject wheelPrefabLeft;
    public Material lineMaterial;

    private void Start()
    {
        if (chassisPrefab == null || wheelPrefabRight == null || wheelPrefabLeft == null)
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
        float graphWheelRightY = 0f;
        float graphWheelLeftY = 0f;
        bool hasGraphData = false;

        for (int i = 0; i < halfCars.Count; i++)
        {
            RollHalfCarSystem car = halfCars[i];

            if (car.chassisTransform == null || car.wheelTransformRight == null || car.wheelTransformLeft == null) continue;

            // Lateral offset sample (b1 is positive X, b2 is negative X)
            float roadY1 = SampleRoadHeightAtPoint(car.pivotPosition.x + car.distToRight_b1, car.pivotPosition.z);
            float roadY2 = SampleRoadHeightAtPoint(car.pivotPosition.x - car.distToLeft_b2, car.pivotPosition.z);

            // Extract State Variables (Eq 13.245)
            float x = car.chassisPosition.y;
            float xDot = car.chassisVelocity.y;

            // Roll around Z axis
            float phi = car.chassisAngle.z;
            float phiDot = car.chassisAngularVelocity.z;

            float x1 = car.rightWheelPosition.y;
            float x1Dot = car.rightWheelVelocity.y;

            float x2 = car.leftWheelPosition.y;
            float x2Dot = car.leftWheelVelocity.y;

            // Suspension Deflections (Right = Side 1, Left = Side 2)
            float suspDeltaPosRight = x - x1 + (car.distToRight_b1 * phi);
            float suspDeltaVelRight = xDot - x1Dot + (car.distToRight_b1 * phiDot);

            float suspDeltaPosLeft = x - x2 - (car.distToLeft_b2 * phi);
            float suspDeltaVelLeft = xDot - x2Dot - (car.distToLeft_b2 * phiDot);

            float tireDeltaPosRight = x1 - roadY1;
            float tireDeltaPosLeft = x2 - roadY2;

            float forceTireRight = (tireDeltaPosRight < 0f) ? car.tireConstantKt1 * tireDeltaPosRight : 0f;
            float forceTireLeft = (tireDeltaPosLeft < 0f) ? car.tireConstantKt2 * tireDeltaPosLeft : 0f;

            // Forces exerted BY the chassis ON the suspension
            float F1 = (car.suspensionConstantK1 * suspDeltaPosRight) + (car.dampingCoefficientC1 * suspDeltaVelRight);
            float F2 = (car.suspensionConstantK2 * suspDeltaPosLeft) + (car.dampingCoefficientC2 * suspDeltaVelLeft);

            // Antiroll Bar Torque (M_R)
            float M_R = 0f;
            if (car.useAdvancedModel)
            {
                // Eq 13.239
                float w = car.distToRight_b1 + car.distToLeft_b2;
                M_R = -car.antirollStiffnessKR * (phi - ((x1 - x2) / w));
            }
            else
            {
                // Eq 13.238
                M_R = -car.antirollStiffnessKR * phi;
            }

            // Equations of Motion derived from Lagrange (Eq 13.234 - 13.237)
            float accBounce = (-F1 - F2) / car.chassisMass - globalGravity;
            float accRoll = (-car.distToRight_b1 * F1 + car.distToLeft_b2 * F2 + M_R) / car.chassisInertiaRoll;
            float accWheelRight = (F1 - forceTireRight) / car.wheelMassRight - globalGravity;
            float accWheelLeft = (F2 - forceTireLeft) / car.wheelMassLeft - globalGravity;

            // Semi Implicit Euler Integration
            car.chassisAcceleration = new Vector3(0, accBounce, 0);
            car.chassisVelocity += car.chassisAcceleration * dt;
            car.chassisPosition += car.chassisVelocity * dt;

            car.chassisAngularAcceleration = new Vector3(0, 0, accRoll);
            car.chassisAngularVelocity += car.chassisAngularAcceleration * dt;
            car.chassisAngle += car.chassisAngularVelocity * dt;

            car.rightWheelAcceleration = new Vector3(0, accWheelRight, 0);
            car.rightWheelVelocity += car.rightWheelAcceleration * dt;
            car.rightWheelPosition += car.rightWheelVelocity * dt;

            car.leftWheelAcceleration = new Vector3(0, accWheelLeft, 0);
            car.leftWheelVelocity += car.leftWheelAcceleration * dt;
            car.leftWheelPosition += car.leftWheelVelocity * dt;

            // Transform Updates
            car.chassisTransform.localPosition = car.chassisEquilibrium + car.chassisPosition;
            car.chassisTransform.localRotation = Quaternion.Euler(car.chassisAngle.x * Mathf.Rad2Deg, car.chassisAngle.y * Mathf.Rad2Deg, car.chassisAngle.z * Mathf.Rad2Deg);

            car.wheelTransformRight.localPosition = car.rightWheelEquilibrium + car.rightWheelPosition;
            car.wheelTransformLeft.localPosition = car.leftWheelEquilibrium + car.leftWheelPosition;

            // Update Visuals
            UpdateHalfCarVisuals(car, roadY1, roadY2);

            if (i == 0)
            {
                graphTimeX = Time.time;
                graphChassisY = car.chassisPosition.y;
                graphWheelRightY = car.rightWheelPosition.y;
                graphWheelLeftY = car.leftWheelPosition.y;
                hasGraphData = true;
            }

            halfCars[i] = car;
        }

        if (hasGraphData && graphContainer != null && graphPointPrefabChassis != null && graphPointPrefabRightWheel != null && graphPointPrefabLeftWheel != null)
        {
            VisualizeTriGraph(graphTimeX, graphChassisY, graphWheelRightY, graphWheelLeftY);
        }
    }

    private void SetupCarSystems()
    {
        if (formChain)
        {
            halfCars.Clear();

            for (int i = 0; i < chainCount; i++)
            {
                RollHalfCarSystem newCar = new RollHalfCarSystem();

                newCar.chassisMass = defaultChassisMass;
                newCar.chassisInertiaRoll = defaultChassisInertiaRoll;
                newCar.wheelMassRight = defaultWheelMassRight;
                newCar.wheelMassLeft = defaultWheelMassLeft;
                newCar.distToRight_b1 = defaultDistB1;
                newCar.distToLeft_b2 = defaultDistB2;

                newCar.suspensionLengthRight = defaultSuspensionLengthRight;
                newCar.suspensionConstantK1 = defaultSuspensionKRight;
                newCar.dampingCoefficientC1 = defaultDampingCRight;

                newCar.suspensionLengthLeft = defaultSuspensionLengthLeft;
                newCar.suspensionConstantK2 = defaultSuspensionKLeft;
                newCar.dampingCoefficientC2 = defaultDampingCLeft;

                newCar.tireLengthRight = defaultTireLengthRight;
                newCar.tireConstantKt1 = defaultTireKtRight;

                newCar.tireLengthLeft = defaultTireLengthLeft;
                newCar.tireConstantKt2 = defaultTireKtLeft;

                newCar.antirollStiffnessKR = defaultAntirollKR;
                newCar.antirollBarHeightOffset = defaultAntirollOffset;

                // Stack cars along Z axis rather than X for a roll simulation setup
                float longitudinalOffset = i * chainSpacingZ;
                newCar.pivotPosition = new Vector3(0, 0, longitudinalOffset);
                newCar.chassisPosition = new Vector3(0, i * initialDisplacementSpacing, 0);

                halfCars.Add(newCar);
            }
        }

        for (int i = 0; i < halfCars.Count; i++)
        {
            RollHalfCarSystem current = halfCars[i];

            if (current.chassisMass <= 0) current.chassisMass = defaultChassisMass;
            if (current.chassisInertiaRoll <= 0) current.chassisInertiaRoll = defaultChassisInertiaRoll;

            if (current.wheelMassRight <= 0) current.wheelMassRight = defaultWheelMassRight;
            if (current.wheelMassLeft <= 0) current.wheelMassLeft = defaultWheelMassLeft;

            if (current.distToRight_b1 <= 0) current.distToRight_b1 = defaultDistB1;
            if (current.distToLeft_b2 <= 0) current.distToLeft_b2 = defaultDistB2;

            if (current.suspensionLengthRight <= 0) current.suspensionLengthRight = defaultSuspensionLengthRight;
            if (current.suspensionConstantK1 <= 0) current.suspensionConstantK1 = defaultSuspensionKRight;
            if (current.dampingCoefficientC1 <= 0) current.dampingCoefficientC1 = defaultDampingCRight;

            if (current.suspensionLengthLeft <= 0) current.suspensionLengthLeft = defaultSuspensionLengthLeft;
            if (current.suspensionConstantK2 <= 0) current.suspensionConstantK2 = defaultSuspensionKLeft;
            if (current.dampingCoefficientC2 <= 0) current.dampingCoefficientC2 = defaultDampingCLeft;

            if (current.tireLengthRight <= 0) current.tireLengthRight = defaultTireLengthRight;
            if (current.tireConstantKt1 <= 0) current.tireConstantKt1 = defaultTireKtRight;

            if (current.tireLengthLeft <= 0) current.tireLengthLeft = defaultTireLengthLeft;
            if (current.tireConstantKt2 <= 0) current.tireConstantKt2 = defaultTireKtLeft;

            float zPos = current.pivotPosition.z;
            current.rightWheelEquilibrium = new Vector3(current.distToRight_b1, current.tireLengthRight, zPos);
            current.leftWheelEquilibrium = new Vector3(-current.distToLeft_b2, current.tireLengthLeft, zPos);
            current.chassisEquilibrium = new Vector3(0f, current.tireLengthRight + current.suspensionLengthRight, zPos);

            GameObject chassisObj = Instantiate(chassisPrefab, transform);
            chassisObj.name = $"Roll Chassis {i + 1}";
            current.chassisTransform = chassisObj.transform;

            GameObject wheelRightObj = Instantiate(wheelPrefabRight, transform);
            wheelRightObj.name = $"Right Wheel {i + 1}";
            current.wheelTransformRight = wheelRightObj.transform;

            GameObject wheelLeftObj = Instantiate(wheelPrefabLeft, transform);
            wheelLeftObj.name = $"Left Wheel {i + 1}";
            current.wheelTransformLeft = wheelLeftObj.transform;

            current.suspensionSpringRightLine = CreateLineVisual(chassisObj.transform, "RightSpring", Color.red);
            current.suspensionDamperRightLine = CreateLineVisual(chassisObj.transform, "RightDamper", Color.green);
            current.tireSpringRightLine = CreateLineVisual(wheelRightObj.transform, "RightTireLine", Color.blue);

            current.suspensionSpringLeftLine = CreateLineVisual(chassisObj.transform, "LeftSpring", Color.red);
            current.suspensionDamperLeftLine = CreateLineVisual(chassisObj.transform, "LeftDamper", Color.green);
            current.tireSpringLeftLine = CreateLineVisual(wheelLeftObj.transform, "LeftTireLine", Color.blue);

            current.antirollLinkLeftLine = CreateLineVisual(chassisObj.transform, "AR_LinkLeft", Color.cyan);
            current.antirollSpringLeftLine = CreateLineVisual(chassisObj.transform, "AR_SpringLeft", Color.cyan);
            current.antirollBarCenterLine = CreateLineVisual(chassisObj.transform, "AR_CenterBar", Color.cyan);
            current.antirollSpringRightLine = CreateLineVisual(chassisObj.transform, "AR_SpringRight", Color.cyan);
            current.antirollLinkRightLine = CreateLineVisual(chassisObj.transform, "AR_LinkRight", Color.cyan);

            current.chassisTransform.localPosition = current.chassisEquilibrium + current.chassisPosition;
            current.wheelTransformRight.localPosition = current.rightWheelEquilibrium + current.rightWheelPosition;
            current.wheelTransformLeft.localPosition = current.leftWheelEquilibrium + current.leftWheelPosition;

            halfCars[i] = current;
        }
    }

    private void UpdateHalfCarVisuals(RollHalfCarSystem car, float roadRightY, float roadLeftY)
    {
        Vector3 rightSuspChassisWorld = car.chassisTransform.TransformPoint(new Vector3(car.distToRight_b1, 0, 0));
        Vector3 leftSuspChassisWorld = car.chassisTransform.TransformPoint(new Vector3(-car.distToLeft_b2, 0, 0));

        Vector3 hubRightWorld = car.wheelTransformRight.position;
        Vector3 hubLeftWorld = car.wheelTransformLeft.position;

        Vector3 contactRightWorld = transform.TransformPoint(new Vector3(car.distToRight_b1, roadRightY, car.pivotPosition.z));
        Vector3 contactLeftWorld = transform.TransformPoint(new Vector3(-car.distToLeft_b2, roadLeftY, car.pivotPosition.z));

        Vector3 depthOffset = transform.forward * 0.15f;

        RenderCoilSpring(car.suspensionSpringRightLine, rightSuspChassisWorld - depthOffset, hubRightWorld - depthOffset);
        RenderStraightLine(car.suspensionDamperRightLine, rightSuspChassisWorld + depthOffset, hubRightWorld + depthOffset);
        RenderStraightLine(car.tireSpringRightLine, hubRightWorld, contactRightWorld);

        RenderCoilSpring(car.suspensionSpringLeftLine, leftSuspChassisWorld - depthOffset, hubLeftWorld - depthOffset);
        RenderStraightLine(car.suspensionDamperLeftLine, leftSuspChassisWorld + depthOffset, hubLeftWorld + depthOffset);
        RenderStraightLine(car.tireSpringLeftLine, hubLeftWorld, contactLeftWorld);

        // Antiroll Bar 5-Segment Logic
        Vector3 barCenterWorld = car.chassisTransform.position - car.chassisTransform.up * car.antirollBarHeightOffset;
        Vector3 barLeftWorld = barCenterWorld - car.chassisTransform.right * car.distToLeft_b2;
        Vector3 barRightWorld = barCenterWorld + car.chassisTransform.right * car.distToRight_b1;

        Vector3 linkLeftMidpoint = Vector3.Lerp(hubLeftWorld, barLeftWorld, 0.5f);
        Vector3 linkRightMidpoint = Vector3.Lerp(hubRightWorld, barRightWorld, 0.5f);

        RenderStraightLine(car.antirollLinkLeftLine, hubLeftWorld, linkLeftMidpoint);
        RenderCoilSpring(car.antirollSpringLeftLine, linkLeftMidpoint, barLeftWorld);
        RenderStraightLine(car.antirollBarCenterLine, barLeftWorld, barRightWorld);
        RenderCoilSpring(car.antirollSpringRightLine, barRightWorld, linkRightMidpoint);
        RenderStraightLine(car.antirollLinkRightLine, linkRightMidpoint, hubRightWorld);
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

    private void VisualizeTriGraph(float timeX, float chassisY, float wheelRightY, float wheelLeftY)
    {
        GameObject cPoint = Instantiate(graphPointPrefabChassis, graphContainer, false);
        RectTransform cRect = cPoint.GetComponent<RectTransform>();
        cRect.anchoredPosition = new Vector2(timeX * graphScale, chassisY * graphScale);

        GameObject wrPoint = Instantiate(graphPointPrefabRightWheel, graphContainer, false);
        RectTransform wrRect = wrPoint.GetComponent<RectTransform>();
        wrRect.anchoredPosition = new Vector2(timeX * graphScale, wheelRightY * graphScale);

        GameObject wlPoint = Instantiate(graphPointPrefabLeftWheel, graphContainer, false);
        RectTransform wlRect = wlPoint.GetComponent<RectTransform>();
        wlRect.anchoredPosition = new Vector2(timeX * graphScale, wheelLeftY * graphScale);

        TriGraphPoints frame = new TriGraphPoints { chassisPoint = cPoint, wheelPointRight = wrPoint, wheelPointLeft = wlPoint };
        activeGraphPoints.Enqueue(frame);

        if (activeGraphPoints.Count > maxDataPoints)
        {
            TriGraphPoints oldest = activeGraphPoints.Dequeue();
            if (oldest.chassisPoint != null) Destroy(oldest.chassisPoint);
            if (oldest.wheelPointRight != null) Destroy(oldest.wheelPointRight);
            if (oldest.wheelPointLeft != null) Destroy(oldest.wheelPointLeft);
        }
    }
}
