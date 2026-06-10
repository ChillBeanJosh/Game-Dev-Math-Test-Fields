using System.Collections.Generic;
using UnityEngine;

public class HalfCar : MonoBehaviour
{
    [System.Serializable]
    public struct HalfCarSystem
    {
        [Header("Mass Profiles:")]
        public float chassisMass;              
        public float chassisInertiaZ;
        [Space]
        public float wheelMassFront;           
        public float wheelMassRear;           

        [Header("Chassis Geometry (Distance from COM):")]
        public float distToFront_a1;          
        public float distToRear_a2;            

        [Header("Front Spring Damper Profiles:")]
        public float suspensionLengthFront;
        public float suspensionConstantK1;     
        public float dampingCoefficientC1;     

        [Header("Rear Spring Damper Profiles:")]
        public float suspensionLengthRear;
        public float suspensionConstantK2;     
        public float dampingCoefficientC2;     

        [Header("Tire Spring Profiles:")]
        public float tireLengthFront;
        public float tireConstantKt1;         
        [Space]
        public float tireLengthRear;
        public float tireConstantKt2;         

        [Header("Position Properties: ")]
        public Vector3 chassisPosition;
        public Vector3 chassisVelocity;
        public Vector3 chassisAcceleration;
        [Space(10)]
        public Vector3 chassisAngle;                
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
    [Space]
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

            //Skip Car if Mass Transforms Are Not Assigned:
            if (car.chassisTransform == null || car.wheelTransformFront == null || car.wheelTransformRear == null) continue;

            //Calculate The Distance From The Wheel To The Road Surface Using A Raycast:
            float roadY1 = SampleRoadHeightAtPoint(car.pivotPosition.x, car.pivotPosition.z + car.distToFront_a1);
            float roadY2 = SampleRoadHeightAtPoint(car.pivotPosition.x, car.pivotPosition.z - car.distToRear_a2);

            //Store Position and Velocity Variables For Chassis:
            float x = car.chassisPosition.y;
            float xDot = car.chassisVelocity.y;

            //Store Angular Position and Velocity Variables For Chassis:
            float theta = car.chassisAngle.x;
            float thetaDot = car.chassisAngularVelocity.x;

            //Store Position and Velocity Variables For Front Wheel:
            float x1 = car.frontWheelPosition.y;
            float x1Dot = car.frontWheelVelocity.y;

            //Store Position and Velocity Variables For Rear Wheel:
            float x2 = car.rearWheelPosition.y;
            float x2Dot = car.rearWheelVelocity.y;

            //Calculate Change in Position and Velocity For The Spring-Damper Systems For Front Suspension:
            float suspDeltaPosFront = x - x1 - (car.distToFront_a1 * theta);
            float suspDeltaVelFront = xDot - x1Dot - (car.distToFront_a1 * thetaDot);

            //Calculate Change in Position and Velocity For The Spring-Damper Systems For Rear Suspension:
            float suspDeltaPosRear = x - x2 + (car.distToRear_a2 * theta);
            float suspDeltaVelRear = xDot - x2Dot + (car.distToRear_a2 * thetaDot);

            //Change in Position For The Tire Spring Based On Contact With The Road Surface:
            float tireDeltaPosFront = x1 - roadY1;
            float tireDeltaPosRear = x2 - roadY2;

            //Apply Tire Force Only When The Tire Is Compressed Against The Road Surface, Otherwise It Should Not Exert Any Force:
            float forceTireFront = (tireDeltaPosFront < 0f) ? car.tireConstantKt1 * tireDeltaPosFront : 0f;
            float forceTireRear = (tireDeltaPosRear < 0f) ? car.tireConstantKt2 * tireDeltaPosRear : 0f;

            //Equations of Motion For Half Car System, Derived Through Lagrangian Mechanics:
            float F1 = (car.suspensionConstantK1 * suspDeltaPosFront) + (car.dampingCoefficientC1 * suspDeltaVelFront);
            float F2 = (car.suspensionConstantK2 * suspDeltaPosRear) + (car.dampingCoefficientC2 * suspDeltaVelRear);

            float accBounce = (-F1 - F2) / car.chassisMass - globalGravity;
            float accPitch = (car.distToFront_a1 * F1 - car.distToRear_a2 * F2) / car.chassisInertiaZ;
            float accWheelFront = (F1 - forceTireFront) / car.wheelMassFront - globalGravity;
            float accWheelRear = (F2 - forceTireRear) / car.wheelMassRear - globalGravity;

            //Semi Implicit Euler Integration on Chassis Position:
            car.chassisAcceleration = new Vector3(0, accBounce, 0);
            car.chassisVelocity += car.chassisAcceleration * dt;
            car.chassisPosition += car.chassisVelocity * dt;

            //Semi Implicit Euler Integration on Chassis Rotation:
            car.chassisAngularAcceleration = new Vector3(accPitch, 0, 0);
            car.chassisAngularVelocity += car.chassisAngularAcceleration * dt;
            car.chassisAngle += car.chassisAngularVelocity * dt;

            //Semi Implicit Euler Integration on Front Wheel Translation:
            car.frontWheelAcceleration = new Vector3(0, accWheelFront, 0);
            car.frontWheelVelocity += car.frontWheelAcceleration * dt;
            car.frontWheelPosition += car.frontWheelVelocity * dt;

            //Semi Implicit Euler Integration on Rear Wheel Translation:
            car.rearWheelAcceleration = new Vector3(0, accWheelRear, 0);
            car.rearWheelVelocity += car.rearWheelAcceleration * dt;
            car.rearWheelPosition += car.rearWheelVelocity * dt;

            //Calculate New Chassis Position and Rotation:
            car.chassisTransform.localPosition = car.chassisEquilibrium + car.chassisPosition;
            car.chassisTransform.localRotation = Quaternion.Euler(car.chassisAngle.x * Mathf.Rad2Deg, car.chassisAngle.y * Mathf.Rad2Deg, car.chassisAngle.z * Mathf.Rad2Deg);

            //Calculate New Wheel Positions:
            car.wheelTransformFront.localPosition = car.frontWheelEquilibrium + car.frontWheelPosition;
            car.wheelTransformRear.localPosition = car.rearWheelEquilibrium + car.rearWheelPosition;

            //Update Line Renderer:
            UpdateHalfCarVisuals(car, roadY1, roadY2);

            //Capture Graph Data for First Car:
            if (i == 0)
            {
                graphTimeX = Time.time;
                graphChassisY = car.chassisPosition.y;
                graphWheelFrontY = car.frontWheelPosition.y;
                graphWheelRearY = car.rearWheelPosition.y;
                hasGraphData = true;
            }

            //Store Updated Car Data:
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
            //Clear Existing Inspected Half Car Data:
            halfCars.Clear();

            for (int i = 0; i < chainCount; i++)
            {
                HalfCarSystem newCar = new HalfCarSystem();

                //Assign Default Physical Properties:
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

                //Calculate Pivot & Equilibrium Positions:
                float lateralOffset = i * chainSpacingX;
                newCar.pivotPosition = new Vector3(lateralOffset, 0, 0);
                newCar.chassisPosition = new Vector3(0, i * initialDisplacementSpacing, 0);

                //Add New Car To The List:
                halfCars.Add(newCar);
            }
        }

        for (int i = 0; i < halfCars.Count; i++)
        {
            HalfCarSystem current = halfCars[i];

            //Assign Default Values if Null or Invalid:
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

            //Assign Equilibrium Positions Based On Pivot Point:
            float xPos = current.pivotPosition.x;
            current.frontWheelEquilibrium = new Vector3(xPos, current.tireLengthFront, current.distToFront_a1);
            current.rearWheelEquilibrium = new Vector3(xPos, current.tireLengthRear, -current.distToRear_a2);
            current.chassisEquilibrium = new Vector3(xPos, current.tireLengthFront + current.suspensionLengthFront, 0f);

            //Instantiate Chassis Mass Object:
            GameObject chassisObj = Instantiate(chassisPrefab, transform);
            chassisObj.name = $"Half Car Chassis {i + 1}";
            current.chassisTransform = chassisObj.transform;

            //Instantiate Front Wheel Object:
            GameObject wheelFrontObj = Instantiate(wheelPrefabFront, transform);
            wheelFrontObj.name = $"Front Wheel {i + 1}";
            current.wheelTransformFront = wheelFrontObj.transform;

            //Instantiate Rear Wheel Object:
            GameObject wheelRearObj = Instantiate(wheelPrefabRear, transform);
            wheelRearObj.name = $"Rear Wheel {i + 1}";
            current.wheelTransformRear = wheelRearObj.transform;

            //Setup Front Suspension & Front Tire Line Render: 
            current.suspensionSpringFrontLine = CreateLineVisual(chassisObj.transform, "FrontSpring", Color.red);
            current.suspensionDamperFrontLine = CreateLineVisual(chassisObj.transform, "FrontDamper", Color.green);
            current.tireSpringFrontLine = CreateLineVisual(wheelFrontObj.transform, "FrontTireLine", Color.blue);

            //Setup Rear Suspension & Rear Tire Line Render:
            current.suspensionSpringRearLine = CreateLineVisual(chassisObj.transform, "RearSpring", Color.red);
            current.suspensionDamperRearLine = CreateLineVisual(chassisObj.transform, "RearDamper", Color.green);
            current.tireSpringRearLine = CreateLineVisual(wheelRearObj.transform, "RearTireLine", Color.blue);

            //Initial Position:
            current.chassisTransform.localPosition = current.chassisEquilibrium + current.chassisPosition;
            current.wheelTransformFront.localPosition = current.frontWheelEquilibrium + current.frontWheelPosition;
            current.wheelTransformRear.localPosition = current.rearWheelEquilibrium + current.rearWheelPosition;

            //Store Updated Car Back Into The List:
            halfCars[i] = current;
        }
    }

    private void UpdateHalfCarVisuals(HalfCarSystem car, float roadFrontY, float roadRearY)
    {
        //Store World Positions For The Front and Rear Suspension Pivot Points On The Chassis:
        Vector3 frontSuspChassisWorld = car.chassisTransform.TransformPoint(new Vector3(0, 0, car.distToFront_a1));
        Vector3 rearSuspChassisWorld = car.chassisTransform.TransformPoint(new Vector3(0, 0, -car.distToRear_a2));

        //Store World Positions For The Front and Rear Wheel Hubs:
        Vector3 hubFrontWorld = car.wheelTransformFront.position;
        Vector3 hubRearWorld = car.wheelTransformRear.position;

        //Store World Positions For The Contact Points Between The Tires and The Road Surface:
        Vector3 contactFrontWorld = transform.TransformPoint(new Vector3(car.pivotPosition.x, roadFrontY, car.distToFront_a1));
        Vector3 contactRearWorld = transform.TransformPoint(new Vector3(car.pivotPosition.x, roadRearY, -car.distToRear_a2));

        //Offset For Line Render Visibility:
        Vector3 lateralOffset = transform.right * 0.15f;

        //Line Renders for Front Suspension:
        RenderCoilSpring(car.suspensionSpringFrontLine, frontSuspChassisWorld - lateralOffset, hubFrontWorld - lateralOffset);
        RenderStraightLine(car.suspensionDamperFrontLine, frontSuspChassisWorld + lateralOffset, hubFrontWorld + lateralOffset);
        RenderStraightLine(car.tireSpringFrontLine, hubFrontWorld, contactFrontWorld);

        //Line Renders for Rear Suspension:
        RenderCoilSpring(car.suspensionSpringRearLine, rearSuspChassisWorld - lateralOffset, hubRearWorld - lateralOffset);
        RenderStraightLine(car.suspensionDamperRearLine, rearSuspChassisWorld + lateralOffset, hubRearWorld + lateralOffset);
        RenderStraightLine(car.tireSpringRearLine, hubRearWorld, contactRearWorld);
    }

    private float SampleRoadHeightAtPoint(float worldX, float worldZ)
    {
        //Vector At the Car's Pivot Point, With a Vertical Length:
        Vector3 rayOriginWorld = transform.TransformPoint(new Vector3(worldX, 40f, worldZ));

        //If Hit Detected, Return The Local Y Height Relative To The Car's Transform:
        if (Physics.Raycast(rayOriginWorld, Vector3.down, out RaycastHit hit, 100f, groundLayer))
        {
            return transform.InverseTransformPoint(hit.point).y;
        }

        //If No Hit Detected, Assume Flat Ground At Y=0:
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
        //Damper & Tire Compression Line:
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    private void RenderCoilSpring(LineRenderer line, Vector3 start, Vector3 end)
    {
        //Suspension Spring Line:
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
        //Instantiate, Transform, and Anchor Chassis Position Based on The Provided x and y Values Multiplied by The Graph Scale:
        GameObject cPoint = Instantiate(graphPointPrefabChassis, graphContainer, false);
        RectTransform cRect = cPoint.GetComponent<RectTransform>();
        cRect.anchoredPosition = new Vector2(timeX * graphScale, chassisY * graphScale);

        //Instantiate, Transform, and Anchor Front Wheel Position Based on The Provided x and y Values Multiplied by The Graph Scale:
        GameObject wfPoint = Instantiate(graphPointPrefabFrontWheel, graphContainer, false);
        RectTransform wfRect = wfPoint.GetComponent<RectTransform>();
        wfRect.anchoredPosition = new Vector2(timeX * graphScale, wheelFrontY * graphScale);

        //Instantiate, Transform, and Anchor Rear Wheel Position Based on The Provided x and y Values Multiplied by The Graph Scale:
        GameObject wrPoint = Instantiate(graphPointPrefabRearWheel, graphContainer, false);
        RectTransform wrRect = wrPoint.GetComponent<RectTransform>();
        wrRect.anchoredPosition = new Vector2(timeX * graphScale, wheelRearY * graphScale);

        //Store Both Points Together In A Small Structure and Enqueue It For Management:
        TriGraphPoints frame = new TriGraphPoints { chassisPoint = cPoint, wheelPointFront = wfPoint, wheelPointRear = wrPoint };
        activeGraphPoints.Enqueue(frame);

        //If The Number Of Active Graph Points Exceeds The Maximum Allowed, Dequeue The Oldest Frame and Destroy Its GameObjects To Prevent Memory Bloat:
        if (activeGraphPoints.Count > maxDataPoints)
        {
            TriGraphPoints oldest = activeGraphPoints.Dequeue();
            if (oldest.chassisPoint != null) Destroy(oldest.chassisPoint);
            if (oldest.wheelPointFront != null) Destroy(oldest.wheelPointFront);
            if (oldest.wheelPointRear != null) Destroy(oldest.wheelPointRear);
        }
    }
}
