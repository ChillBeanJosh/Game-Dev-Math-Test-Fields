using System.Collections.Generic;
using UnityEngine;

public class QuarterCar : MonoBehaviour
{
    [System.Serializable]
    public struct QuarterCarSystem
    {
        [Header("Mass Profiles: ")]
        public float chassisMass;              
        public float wheelMass;             

        [Header("Spring Damper System Profiles: ")]
        public float suspensionLength;
        public float suspensionConstantK2;     
        public float dampingCoefficientC2;    

        [Header("Tire Spring Profiles: ")]
        public float tireLength;
        public float tireConstantK1;           

        [Header("Position Properties: ")]
        public Vector3 chassisPosition;        
        public Vector3 chassisVelocity;
        public Vector3 chassisAcceleration;
        [Space(20)]
        public Vector3 wheelPosition;          
        public Vector3 wheelVelocity;
        public Vector3 wheelAcceleration;

        [Header("World Reference Alignments")]
        public Vector3 pivotPosition;          
        public Vector3 chassisEquilibrium;     
        public Vector3 wheelEquilibrium;       

        [HideInInspector] public Transform chassisTransform; 
        [HideInInspector] public Transform wheelTransform;   

        [HideInInspector] public LineRenderer suspensionSpringLine; 
        [HideInInspector] public LineRenderer suspensionDamperLine; 
        [HideInInspector] public LineRenderer tireSpringLine;       
    }
    private struct DualGraphPoints
    {
        public GameObject chassisPoint;
        public GameObject wheelPoint;
    }

    [Header("--------------------------------------------------------------------------")]

    [Header("Graph Settings: ")]
    public RectTransform graphContainer;
    public float graphScale = 100f;
    [Space]
    public GameObject graphPointPrefabM2;
    public GameObject graphPointPrefabM1;
    [Space]
    public int maxDataPoints = 200;
    private Queue<DualGraphPoints> activeGraphPoints = new Queue<DualGraphPoints>();

    [Header("--------------------------------------------------------------------------")]

    [Header("Simulation Settings: ")]
    [Range(0.1f, 5f)] public float TimeScale = 1.0f;
    public float globalGravity = 9.81f;
    [Space]
    public LayerMask groundLayer;

    [Header("--------------------------------------------------------------------------")]

    [Header("Global Properties: ")]
    public float defaultChassisMass = 400f;
    public float defaultWheelMass = 40f;
    [Space]
    public float defaultSuspensionLength = 2.0f;
    public float defaultSuspensionK = 15000f;
    public float defaultDampingC = 1200f;
    [Space]
    public float defaultTireLength = 0.5f;
    public float defaultTireK = 100000f;

    [Header("--------------------------------------------------------------------------")]

    [Header("Spring Visual Customization: ")]
    public int springCoilSegments = 60;
    public float springCoilRadius = 0.12f;
    public float springCoilFrequency = 10f;

    [Header("--------------------------------------------------------------------------")]

    [Header("Chain/Array Properties: ")]
    public bool formChain = false;
    [Space]
    public int chainCount = 1;
    public float chainSpacingZ = 2.5f;
    public float initialDisplacementSpacing = 0.2f;

    [Header("--------------------------------------------------------------------------")]

    [Header("Quarter Car System Data List: ")]
    public List<QuarterCarSystem> quarterCars = new List<QuarterCarSystem>();

    [Header("--------------------------------------------------------------------------")]

    [Header("Prefabs & Materials: ")]
    public GameObject chassisPrefab;
    public GameObject wheelPrefab;
    public Material lineMaterial;

    private void Start()
    {
        if (chassisPrefab == null || wheelPrefab == null)
        {
            Debug.LogError("Mass Prefabs are not assigned. Please assign a mass prefab in the inspector.");
            return;
        }

        SetupCarSystems();
    }

    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime * TimeScale;

        float graphTimeX = 0f;
        float graphChassisY = 0f;
        float graphWheelY = 0f;
        bool hasGraphData = false;

        for (int i = 0; i < quarterCars.Count; i++)
        {
            QuarterCarSystem currentCar = quarterCars[i];

            //Skip Car if Mass Transforms Are Not Assigned:
            if (currentCar.chassisTransform == null || currentCar.wheelTransform == null) continue;

            //Calculate The Distance From The Wheel To The Road Surface Using A Raycast:
            float roadSurfaceY = SampleRoadHeightProfile(currentCar);

            //Store Position and Velocity Variables For Mass 2:
            float y2 = currentCar.chassisPosition.y;
            float y2Dot = currentCar.chassisVelocity.y;

            //Store Position and Velocity Variables For Mass 1:
            float y1 = currentCar.wheelPosition.y;
            float y1Dot = currentCar.wheelVelocity.y;

            //Calculate Changes In Position and Velocity For The Spring-Damper System:
            float suspensionDeltaPos = y2 - y1;
            float suspensionDeltaVel = y2Dot - y1Dot;

            //Change In Position For The Tire Spring Based On The Contact Point With The Road Surface:
            float tireDeltaPos = y1 - roadSurfaceY;
            float tireForce = 0f;

            //Apply Tire Force Only When The Tire Is Compressed Against The Road Surface, Otherwise It Should Not Exert Any Force:
            if (tireDeltaPos < 0f)
            {
                tireForce = currentCar.tireConstantK1 * tireDeltaPos;
            }
            else
            {
                tireForce = 0f;
            }

            //Equations of Motion For Mass 2 and Mass 1, Derived Through Lagrangian Mechanics:
            float accChassisY = (-currentCar.suspensionConstantK2 * suspensionDeltaPos - currentCar.dampingCoefficientC2 * suspensionDeltaVel) / currentCar.chassisMass - globalGravity;
            float accWheelY = (currentCar.suspensionConstantK2 * suspensionDeltaPos + currentCar.dampingCoefficientC2 * suspensionDeltaVel - tireForce) / currentCar.wheelMass - globalGravity;

            //Apply Calculated Accelerations To The Car System:
            currentCar.chassisAcceleration = new Vector3(0, accChassisY, 0);
            currentCar.wheelAcceleration = new Vector3(0, accWheelY, 0);

            //Semi Implicit Euler Integration on Mass 2:
            currentCar.chassisVelocity += currentCar.chassisAcceleration * deltaTime;
            currentCar.chassisPosition += currentCar.chassisVelocity * deltaTime;

            //Semi Implicit Euler Integration on Mass 1:
            currentCar.wheelVelocity += currentCar.wheelAcceleration * deltaTime;
            currentCar.wheelPosition += currentCar.wheelVelocity * deltaTime;

            //Calculate New Positions:
            currentCar.chassisTransform.localPosition = currentCar.chassisEquilibrium + currentCar.chassisPosition;
            currentCar.wheelTransform.localPosition = currentCar.wheelEquilibrium + currentCar.wheelPosition;

            //Update Line Renderer:
            UpdateQuarterCarVisuals(currentCar, roadSurfaceY);

            //Capture Graph Data for First Car:
            if (i == 0)
            {
                graphTimeX = Time.time;
                graphChassisY = currentCar.chassisPosition.y;
                graphWheelY = currentCar.wheelPosition.y;
                hasGraphData = true;
            }

            //Store Updated Car Data:
            quarterCars[i] = currentCar;
        }

        if (hasGraphData && graphContainer != null && graphPointPrefabM2 != null && graphPointPrefabM1 != null)
        {
            VisualizeDualGraph(graphTimeX, graphChassisY, graphWheelY);
        }
    }

    private void SetupCarSystems()
    {
        if (formChain)
        {
            //Clear Existing Inspected Quarter Car Data:
            quarterCars.Clear();

            for (int i = 0; i < chainCount; i++)
            {
                QuarterCarSystem newCar = new QuarterCarSystem();

                //Assign Default Physical Properties:
                newCar.chassisMass = defaultChassisMass;
                newCar.wheelMass = defaultWheelMass;
                newCar.suspensionLength = defaultSuspensionLength;
                newCar.suspensionConstantK2 = defaultSuspensionK;
                newCar.dampingCoefficientC2 = defaultDampingC;
                newCar.tireLength = defaultTireLength;
                newCar.tireConstantK1 = defaultTireK;

                //Calculate Pivot & Equilibrium Positions:
                float positionOffset = i * chainSpacingZ;
                newCar.pivotPosition = new Vector3(0, 0, positionOffset);
                newCar.wheelEquilibrium = new Vector3(0, newCar.tireLength, positionOffset);
                newCar.chassisEquilibrium = new Vector3(0, newCar.tireLength + newCar.suspensionLength, positionOffset);

                //Apply Offset Displacement To Each Car In The Chain For Visual Clarity:
                newCar.chassisPosition = new Vector3(0, i * initialDisplacementSpacing, 0);
                newCar.wheelPosition = Vector3.zero;

                //Add New Car To The List:
                quarterCars.Add(newCar);
            }
        }

        for (int i = 0; i < quarterCars.Count; i++)
        {
            QuarterCarSystem current = quarterCars[i];

            //Assign Default Values if Null or Invalid:
            if (current.chassisMass <= 0) current.chassisMass = defaultChassisMass;
            if (current.wheelMass <= 0) current.wheelMass = defaultWheelMass;
            if (current.suspensionLength <= 0) current.suspensionLength = defaultSuspensionLength;
            if (current.suspensionConstantK2 <= 0) current.suspensionConstantK2 = defaultSuspensionK;
            if (current.dampingCoefficientC2 <= 0) current.dampingCoefficientC2 = defaultDampingC;
            if (current.tireLength <= 0) current.tireLength = defaultTireLength;
            if (current.tireConstantK1 <= 0) current.tireConstantK1 = defaultTireK;

            //Assign Equilibrium Positions Based On Pivot Point:
            if (!formChain)
            {
                float zPos = current.pivotPosition.z;
                current.wheelEquilibrium = new Vector3(current.pivotPosition.x, current.tireLength, zPos);
                current.chassisEquilibrium = new Vector3(current.pivotPosition.x, current.tireLength + current.suspensionLength, zPos);
            }

            //Instantiate Mass1 Object:
            GameObject chassisObj = Instantiate(chassisPrefab, transform);
            chassisObj.name = $"Quarter Car Chassis {i + 1}";
            current.chassisTransform = chassisObj.transform;

            //Instantiate Mass2 Object:
            GameObject wheelObj = Instantiate(wheelPrefab, transform);
            wheelObj.name = $"Quarter Car Wheel {i + 1}";
            current.wheelTransform = wheelObj.transform;

            //Setup SpringConstantK2 Line Render:
            GameObject springK2Obj = new GameObject("SuspensionSpringVisual");
            springK2Obj.transform.SetParent(chassisObj.transform, false);
            current.suspensionSpringLine = springK2Obj.AddComponent<LineRenderer>();
            SetupLineRenderer(current.suspensionSpringLine, Color.red);

            //Setup DampingCoefficientC2 Line Render:
            GameObject damperC2Obj = new GameObject("SuspensionDamperVisual");
            damperC2Obj.transform.SetParent(chassisObj.transform, false);
            current.suspensionDamperLine = damperC2Obj.AddComponent<LineRenderer>();
            SetupLineRenderer(current.suspensionDamperLine, Color.green);

            //Setup TireConstantK1 Line Render:
            GameObject tireK1Obj = new GameObject("TireSpringVisual");
            tireK1Obj.transform.SetParent(wheelObj.transform, false);
            current.tireSpringLine = tireK1Obj.AddComponent<LineRenderer>();
            SetupLineRenderer(current.tireSpringLine, Color.blue);

            //Initial Position:
            current.chassisTransform.localPosition = current.chassisEquilibrium + current.chassisPosition;
            current.wheelTransform.localPosition = current.wheelEquilibrium + current.wheelPosition;

            //Store Updated Car Back Into The List:
            quarterCars[i] = current;
        }
    }

    private void UpdateQuarterCarVisuals(QuarterCarSystem car, float roadSurfaceLocalY)
    {
        //Store World Positions For The Car's Body, Wheel Hub, and Contact Point With The Road Surface:
        Vector3 bodyPos = car.chassisTransform.position;
        Vector3 hubPos = car.wheelTransform.position;
        Vector3 contactPointWorld = transform.TransformPoint(new Vector3(car.pivotPosition.x, roadSurfaceLocalY, car.pivotPosition.z));

        //Calculate A Side Offset Direction Perpendicular To The Travel Direction For Clear Visual Separation Of Spring and Damper Lines:
        Vector3 travelDir = (bodyPos - hubPos).normalized;
        Vector3 sideOffset = Vector3.Cross(travelDir, transform.forward).normalized * 0.12f;

        //Suspension Spring Line:
        Vector3 springStart = bodyPos - sideOffset;
        Vector3 springEnd = hubPos - sideOffset;
        car.suspensionSpringLine.positionCount = springCoilSegments;
        for (int j = 0; j < springCoilSegments; j++)
        {
            float t = (float)j / (springCoilSegments - 1);
            Vector3 point = Vector3.Lerp(springStart, springEnd, t);
            if (t > 0.04f && t < 0.96f)
            {
                float wave = Mathf.Sin(t * Mathf.PI * 2f * springCoilFrequency);
                point += transform.right * wave * springCoilRadius;
            }
            car.suspensionSpringLine.SetPosition(j, point);
        }

        //Damper Line:
        car.suspensionDamperLine.positionCount = 2;
        car.suspensionDamperLine.SetPosition(0, bodyPos + sideOffset);
        car.suspensionDamperLine.SetPosition(1, hubPos + sideOffset);

        //Tire Spring Line:
        car.tireSpringLine.positionCount = 2;
        car.tireSpringLine.SetPosition(0, hubPos);
        car.tireSpringLine.SetPosition(1, contactPointWorld);
    }

    private float SampleRoadHeightProfile(QuarterCarSystem car)
    {
        //Vector At the Car's Pivot Point, With a Vertical Length:
        Vector3 rayOriginWorld = transform.TransformPoint(new Vector3(car.pivotPosition.x, 40f, car.pivotPosition.z));

        //If Hit Detected, Return The Local Y Height Relative To The Car's Transform:
        if (Physics.Raycast(rayOriginWorld, Vector3.down, out RaycastHit hit, 100f, groundLayer))
        {
            return transform.InverseTransformPoint(hit.point).y;
        }

        //If No Hit Detected, Assume Flat Ground At Y=0:
        return 0f;
    }

    private void SetupLineRenderer(LineRenderer line, Color debugColor)
    {
        line.positionCount = 2;
        line.startWidth = 0.04f;
        line.endWidth = 0.04f;

        line.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));

        line.startColor = debugColor;
        line.endColor = debugColor;

        line.useWorldSpace = true;
    }

    private void VisualizeDualGraph(float timeX, float chassisY, float wheelY)
    {
        //Instantiate, Transform, and Anchor Mass 2 Position Based on The Provided x and y Values Multiplied by The Graph Scale:
        GameObject cPoint = Instantiate(graphPointPrefabM2, graphContainer, false);
        RectTransform cRect = cPoint.GetComponent<RectTransform>();
        cRect.anchoredPosition = new Vector2(timeX * graphScale, chassisY * graphScale);

        //Instantiate, Transform, and Anchor Mass 1 Position Based on The Provided x and y Values Multiplied by The Graph Scale:
        GameObject wPoint = Instantiate(graphPointPrefabM1, graphContainer, false);
        RectTransform wRect = wPoint.GetComponent<RectTransform>();
        wRect.anchoredPosition = new Vector2(timeX * graphScale, wheelY * graphScale);

        //Store Both Points Together In A Small Structure and Enqueue It For Management:
        DualGraphPoints combinedFrame = new DualGraphPoints { chassisPoint = cPoint, wheelPoint = wPoint };
        activeGraphPoints.Enqueue(combinedFrame);

        //If The Number Of Active Graph Points Exceeds The Maximum Allowed, Dequeue The Oldest Frame and Destroy Its GameObjects To Prevent Memory Bloat:
        if (activeGraphPoints.Count > maxDataPoints)
        {
            DualGraphPoints oldestFrame = activeGraphPoints.Dequeue();
            if (oldestFrame.chassisPoint != null) Destroy(oldestFrame.chassisPoint);
            if (oldestFrame.wheelPoint != null) Destroy(oldestFrame.wheelPoint);
        }
    }
}
