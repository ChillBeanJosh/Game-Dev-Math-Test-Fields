using System.Collections.Generic;
using UnityEngine;

public class FullCar : MonoBehaviour
{
    [System.Serializable]
    public struct FullCarSystem
    {
        [Header("Mass Profiles:")]
        public float chassisMass;
        public float chassisInertiaPitch; // I_y or I_z depending on axis (Pitch Inertia)
        public float chassisInertiaRoll;  // I_x (Roll Inertia)
        [Space]
        public float wheelMassFR; // m_fr
        public float wheelMassFL; // m_fl
        public float wheelMassRR; // m_rr
        public float wheelMassRL; // m_rl

        [Header("Chassis Geometry (Distance from COM):")]
        public float distToFront_a1; // a_1 (Longitudinal Front)
        public float distToRear_a2;  // a_2 (Longitudinal Rear)
        [Space]
        public float distToRight_b1; // b_1 (Lateral Right)
        public float distToLeft_b2;  // b_2 (Lateral Left)

        [Header("Front Right Suspension Profile:")]
        public float suspensionLengthFR;
        [Space]
        public float suspensionConstantK_FR;
        public float dampingCoefficientC_FR;

        [Header("Front Left Suspension Profile:")]
        public float suspensionLengthFL;
        [Space]
        public float suspensionConstantK_FL;
        public float dampingCoefficientC_FL;

        [Header("Rear Right Suspension Profile:")]
        public float suspensionLengthRR;
        [Space]
        public float suspensionConstantK_RR;
        public float dampingCoefficientC_RR;

        [Header("Rear Left Suspension Profile:")]
        public float suspensionLengthRL;
        [Space]
        public float suspensionConstantK_RL;
        public float dampingCoefficientC_RL;

        [Header("Tire Spring Profiles:")]
        public float tireLengthFR;
        public float tireConstantKt_FR;
        [Space]
        public float tireLengthFL;
        public float tireConstantKt_FL;
        [Space]
        public float tireLengthRR;
        public float tireConstantKt_RR;
        [Space]
        public float tireLengthRL;
        public float tireConstantKt_RL;

        [Header("Antiroll Bar Profiles:")]
        public bool useAdvancedModel;
        public bool useFrontAntirollBarOnly;
        [Space]
        public float antirollStiffnessKR_Front;
        public float antirollStiffnessKR_Rear;
        [Space]
        public float antirollBarHeightOffset;

        [Header("Position Properties: ")]
        public Vector3 chassisPosition;
        public Vector3 chassisVelocity;
        public Vector3 chassisAcceleration;
        [Space(10)]
        public Vector3 chassisAngle; // .x = Pitch, .z = Roll
        public Vector3 chassisAngularVelocity;
        public Vector3 chassisAngularAcceleration;
        [Space(20)]
        public Vector3 frWheelPosition;
        public Vector3 frWheelVelocity;
        public Vector3 frWheelAcceleration;
        [Space(10)]
        public Vector3 flWheelPosition;
        public Vector3 flWheelVelocity;
        public Vector3 flWheelAcceleration;
        [Space(10)]
        public Vector3 rrWheelPosition;
        public Vector3 rrWheelVelocity;
        public Vector3 rrWheelAcceleration;
        [Space(10)]
        public Vector3 rlWheelPosition;
        public Vector3 rlWheelVelocity;
        public Vector3 rlWheelAcceleration;

        [Header("World Reference Alignments")]
        public Vector3 pivotPosition;
        [Space]
        public Vector3 chassisEquilibrium;
        [Space]
        public Vector3 frWheelEquilibrium;
        public Vector3 flWheelEquilibrium;
        public Vector3 rrWheelEquilibrium;
        public Vector3 rlWheelEquilibrium;

        [HideInInspector] public Transform chassisTransform;
        [HideInInspector] public Transform wheelTransformFR;
        [HideInInspector] public Transform wheelTransformFL;
        [HideInInspector] public Transform wheelTransformRR;
        [HideInInspector] public Transform wheelTransformRL;

        // Visuals fields
        [HideInInspector] public LineRenderer suspSpringFRLine;
        [HideInInspector] public LineRenderer suspDamperFRLine;
        [HideInInspector] public LineRenderer tireSpringFRLine;

        [HideInInspector] public LineRenderer suspSpringFLLine;
        [HideInInspector] public LineRenderer suspDamperFLLine;
        [HideInInspector] public LineRenderer tireSpringFLLine;

        [HideInInspector] public LineRenderer suspSpringRRLine;
        [HideInInspector] public LineRenderer suspDamperRRLine;
        [HideInInspector] public LineRenderer tireSpringRRLine;

        [HideInInspector] public LineRenderer suspSpringRLLine;
        [HideInInspector] public LineRenderer suspDamperRLLine;
        [HideInInspector] public LineRenderer tireSpringRLLine;

        // Antiroll Bar Center Render Lines
        [HideInInspector] public LineRenderer antirollFrontCenterLine;
        [HideInInspector] public LineRenderer antirollRearCenterLine;
    }

    private struct QuadGraphPoints
    {
        public GameObject chassisPoint;
        public GameObject wheelPointFR;
        public GameObject wheelPointFL;
        public GameObject wheelPointRR;
        public GameObject wheelPointRL;
    }

    [Header("--------------------------------------------------------------------------")]

    [Header("Graph Settings:")]
    public RectTransform graphContainer;
    public float graphScale = 100f;
    [Space]
    public GameObject graphPointPrefabChassis;
    public GameObject graphPointPrefabFRWheel;
    public GameObject graphPointPrefabFLWheel;
    public GameObject graphPointPrefabRRWheel;
    public GameObject graphPointPrefabRLWheel;
    [Space]
    public int maxDataPoints = 200;
    private Queue<QuadGraphPoints> activeGraphPoints = new Queue<QuadGraphPoints>();

    [Header("--------------------------------------------------------------------------")]

    [Header("Simulation Settings:")]
    [Range(0.1f, 5f)] public float TimeScale = 1.0f;
    public float globalGravity = 9.81f;
    [Space]
    public LayerMask groundLayer;

    [Header("--------------------------------------------------------------------------")]

    [Header("Global Default Properties:")]
    public float defaultChassisMass = 1400f;
    public float defaultChassisInertiaPitch = 2000f;
    public float defaultChassisInertiaRoll = 1600f;
    [Space]
    public float defaultWheelMass = 45f;
    [Space]
    public float defaultDistA1 = 1.3f; // Front
    public float defaultDistA2 = 1.5f; // Rear
    public float defaultDistB1 = 0.8f; // Right
    public float defaultDistB2 = 0.8f; // Left
    [Space]
    public float defaultSuspensionLength = 1.5f;
    public float defaultSuspensionK = 30000f;
    public float defaultDampingC = 2200f;
    [Space]
    public float defaultTireLength = 0.4f;
    public float defaultTireKt = 150000f;
    [Space]
    public float defaultAntirollKR = 12000f;
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
    public float chainSpacingX = 5.0f;
    public float initialDisplacementSpacing = 0.2f;

    [Header("--------------------------------------------------------------------------")]

    [Header("Full Car System Data List:")]
    public List<FullCarSystem> fullCars = new List<FullCarSystem>();

    [Header("--------------------------------------------------------------------------")]

    [Header("Prefabs & Materials:")]
    public GameObject chassisPrefab;
    public GameObject wheelPrefabFR;
    public GameObject wheelPrefabFL;
    public GameObject wheelPrefabRR;
    public GameObject wheelPrefabRL;
    public Material lineMaterial;

    private void Start()
    {
        if (chassisPrefab == null || wheelPrefabFR == null || wheelPrefabFL == null || wheelPrefabRR == null || wheelPrefabRL == null)
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
        float graphFR = 0f, graphFL = 0f, graphRR = 0f, graphRL = 0f;
        bool hasGraphData = false;

        for (int i = 0; i < fullCars.Count; i++)
        {
            FullCarSystem car = fullCars[i];

            if (car.chassisTransform == null || car.wheelTransformFR == null || car.wheelTransformFL == null || car.wheelTransformRR == null || car.wheelTransformRL == null) continue;

            // Sample road surface underneath all 4 wheel nodes
            float roadY_FR = SampleRoadHeightAtPoint(car.pivotPosition.x + car.distToRight_b1, car.pivotPosition.z + car.distToFront_a1);
            float roadY_FL = SampleRoadHeightAtPoint(car.pivotPosition.x - car.distToLeft_b2, car.pivotPosition.z + car.distToFront_a1);
            float roadY_RR = SampleRoadHeightAtPoint(car.pivotPosition.x + car.distToRight_b1, car.pivotPosition.z - car.distToRear_a2);
            float roadY_RL = SampleRoadHeightAtPoint(car.pivotPosition.x - car.distToLeft_b2, car.pivotPosition.z - car.distToRear_a2);

            // Translational state profiles
            float x = car.chassisPosition.y;
            float xDot = car.chassisVelocity.y;

            // Rotational profiles (Pitch = X, Roll = Z)
            float theta = car.chassisAngle.x;
            float thetaDot = car.chassisAngularVelocity.x;

            float phi = car.chassisAngle.z;
            float phiDot = car.chassisAngularVelocity.z;

            // Wheel state extractions
            float xFR = car.frWheelPosition.y; float xFRDot = car.frWheelVelocity.y;
            float xFL = car.flWheelPosition.y; float xFLDot = car.flWheelVelocity.y;
            float xRR = car.rrWheelPosition.y; float xRRDot = car.rrWheelVelocity.y;
            float xRL = car.rlWheelPosition.y; float xRLDot = car.rlWheelVelocity.y;

            // Combined Deflections (Maintaining script sign conventions)
            float suspDeltaPosFR = x - xFR - (car.distToFront_a1 * theta) + (car.distToRight_b1 * phi);
            float suspDeltaVelFR = xDot - xFRDot - (car.distToFront_a1 * thetaDot) + (car.distToRight_b1 * phiDot);

            float suspDeltaPosFL = x - xFL - (car.distToFront_a1 * theta) - (car.distToLeft_b2 * phi);
            float suspDeltaVelFL = xDot - xFLDot - (car.distToFront_a1 * thetaDot) - (car.distToLeft_b2 * phiDot);

            float suspDeltaPosRR = x - xRR + (car.distToRear_a2 * theta) + (car.distToRight_b1 * phi);
            float suspDeltaVelRR = xDot - xRRDot + (car.distToRear_a2 * thetaDot) + (car.distToRight_b1 * phiDot);

            float suspDeltaPosRL = x - xRL + (car.distToRear_a2 * theta) - (car.distToLeft_b2 * phi);
            float suspDeltaVelRL = xDot - xRLDot + (car.distToRear_a2 * thetaDot) - (car.distToLeft_b2 * phiDot);

            // Tire Ground Contact Checks
            float tireDeltaPosFR = xFR - roadY_FR;
            float tireDeltaPosFL = xFL - roadY_FL;
            float tireDeltaPosRR = xRR - roadY_RR;
            float tireDeltaPosRL = xRL - roadY_RL;

            float forceTireFR = (tireDeltaPosFR < 0f) ? car.tireConstantKt_FR * tireDeltaPosFR : 0f;
            float forceTireFL = (tireDeltaPosFL < 0f) ? car.tireConstantKt_FL * tireDeltaPosFL : 0f;
            float forceTireRR = (tireDeltaPosRR < 0f) ? car.tireConstantKt_RR * tireDeltaPosRR : 0f;
            float forceTireRL = (tireDeltaPosRL < 0f) ? car.tireConstantKt_RL * tireDeltaPosRL : 0f;

            // Suspension forces acting between masses
            float F_FR = (car.suspensionConstantK_FR * suspDeltaPosFR) + (car.dampingCoefficientC_FR * suspDeltaVelFR);
            float F_FL = (car.suspensionConstantK_FL * suspDeltaPosFL) + (car.dampingCoefficientC_FL * suspDeltaVelFL);
            float F_RR = (car.suspensionConstantK_RR * suspDeltaPosRR) + (car.dampingCoefficientC_RR * suspDeltaVelRR);
            float F_RL = (car.suspensionConstantK_RL * suspDeltaPosRL) + (car.dampingCoefficientC_RL * suspDeltaVelRL);

            // Antiroll Bar Auxiliary Calculations
            float M_R_Front = 0f;
            float M_R_Rear = 0f;
            float w = car.distToRight_b1 + car.distToLeft_b2;

            if (car.useAdvancedModel)
            {
                M_R_Front = -car.antirollStiffnessKR_Front * (phi - ((xFR - xFL) / w));

                if (!car.useFrontAntirollBarOnly)
                {
                    M_R_Rear = -car.antirollStiffnessKR_Rear * (phi - ((xRR - xRL) / w));
                }
            }
            else
            {
                M_R_Front = -car.antirollStiffnessKR_Front * phi;

                if (!car.useFrontAntirollBarOnly)
                {
                    M_R_Rear = -car.antirollStiffnessKR_Rear * phi;
                }
            }

            // Coupled 7-DOF Equations of Motion (Lagrangian Matrix Breakdown)
            float accBounce = (-F_FR - F_FL - F_RR - F_RL) / car.chassisMass - globalGravity;
            float accPitch = (car.distToFront_a1 * (F_FR + F_FL) - car.distToRear_a2 * (F_RR + F_RL)) / car.chassisInertiaPitch;
            float accRoll = (-car.distToRight_b1 * (F_FR + F_RR) + car.distToLeft_b2 * (F_FL + F_RL) + M_R_Front + M_R_Rear) / car.chassisInertiaRoll;

            float accWheelFR = (F_FR - forceTireFR) / car.wheelMassFR - globalGravity;
            float accWheelFL = (F_FL - forceTireFL) / car.wheelMassFL - globalGravity;
            float accWheelRR = (F_RR - forceTireRR) / car.wheelMassRR - globalGravity;
            float accWheelRL = (F_RL - forceTireRL) / car.wheelMassRL - globalGravity;

            // Semi-Implicit Euler Integration
            car.chassisAcceleration = new Vector3(0, accBounce, 0);
            car.chassisVelocity += car.chassisAcceleration * dt;
            car.chassisPosition += car.chassisVelocity * dt;

            car.chassisAngularAcceleration = new Vector3(accPitch, 0, accRoll);
            car.chassisAngularVelocity += car.chassisAngularAcceleration * dt;
            car.chassisAngle += car.chassisAngularVelocity * dt;

            car.frWheelAcceleration = new Vector3(0, accWheelFR, 0);
            car.frWheelVelocity += car.frWheelAcceleration * dt;
            car.frWheelPosition += car.frWheelVelocity * dt;

            car.flWheelAcceleration = new Vector3(0, accWheelFL, 0);
            car.flWheelVelocity += car.flWheelAcceleration * dt;
            car.flWheelPosition += car.flWheelVelocity * dt;

            car.rrWheelAcceleration = new Vector3(0, accWheelRR, 0);
            car.rrWheelVelocity += car.rrWheelAcceleration * dt;
            car.rrWheelPosition += car.rrWheelVelocity * dt;

            car.rlWheelAcceleration = new Vector3(0, accWheelRL, 0);
            car.rlWheelVelocity += car.rlWheelAcceleration * dt;
            car.rlWheelPosition += car.rlWheelVelocity * dt;

            // Transform Spatial Mapping
            car.chassisTransform.localPosition = car.chassisEquilibrium + car.chassisPosition;
            car.chassisTransform.localRotation = Quaternion.Euler(car.chassisAngle.x * Mathf.Rad2Deg, car.chassisAngle.y * Mathf.Rad2Deg, car.chassisAngle.z * Mathf.Rad2Deg);

            car.wheelTransformFR.localPosition = car.frWheelEquilibrium + car.frWheelPosition;
            car.wheelTransformFL.localPosition = car.flWheelEquilibrium + car.flWheelPosition;
            car.wheelTransformRR.localPosition = car.rrWheelEquilibrium + car.rrWheelPosition;
            car.wheelTransformRL.localPosition = car.rlWheelEquilibrium + car.rlWheelPosition;

            // Visual Frame Rendering
            UpdateFullCarVisuals(car, roadY_FR, roadY_FL, roadY_RR, roadY_RL);

            if (i == 0)
            {
                graphTimeX = Time.time;
                graphChassisY = car.chassisPosition.y;
                graphFR = car.frWheelPosition.y; graphFL = car.flWheelPosition.y;
                graphRR = car.rrWheelPosition.y; graphRL = car.rlWheelPosition.y;
                hasGraphData = true;
            }

            fullCars[i] = car;
        }

        if (hasGraphData && graphContainer != null && graphPointPrefabChassis != null)
        {
            VisualizeQuadGraph(graphTimeX, graphChassisY, graphFR, graphFL, graphRR, graphRL);
        }
    }

    private void SetupCarSystems()
    {
        if (formChain)
        {
            fullCars.Clear();
            for (int i = 0; i < chainCount; i++)
            {
                FullCarSystem newCar = new FullCarSystem();
                newCar.chassisMass = defaultChassisMass;
                newCar.chassisInertiaPitch = defaultChassisInertiaPitch;
                newCar.chassisInertiaRoll = defaultChassisInertiaRoll;

                newCar.wheelMassFR = defaultWheelMass; newCar.wheelMassFL = defaultWheelMass;
                newCar.wheelMassRR = defaultWheelMass; newCar.wheelMassRL = defaultWheelMass;

                newCar.distToFront_a1 = defaultDistA1; newCar.distToRear_a2 = defaultDistA2;
                newCar.distToRight_b1 = defaultDistB1; newCar.distToLeft_b2 = defaultDistB2;

                newCar.suspensionLengthFR = defaultSuspensionLength; newCar.suspensionConstantK_FR = defaultSuspensionK; newCar.dampingCoefficientC_FR = defaultDampingC;
                newCar.suspensionLengthFL = defaultSuspensionLength; newCar.suspensionConstantK_FL = defaultSuspensionK; newCar.dampingCoefficientC_FL = defaultDampingC;
                newCar.suspensionLengthRR = defaultSuspensionLength; newCar.suspensionConstantK_RR = defaultSuspensionK; newCar.dampingCoefficientC_RR = defaultDampingC;
                newCar.suspensionLengthRL = defaultSuspensionLength; newCar.suspensionConstantK_RL = defaultSuspensionK; newCar.dampingCoefficientC_RL = defaultDampingC;

                newCar.tireLengthFR = defaultTireLength; newCar.tireConstantKt_FR = defaultTireKt;
                newCar.tireLengthFL = defaultTireLength; newCar.tireConstantKt_FL = defaultTireKt;
                newCar.tireLengthRR = defaultTireLength; newCar.tireConstantKt_RR = defaultTireKt;
                newCar.tireLengthRL = defaultTireLength; newCar.tireConstantKt_RL = defaultTireKt;

                newCar.antirollStiffnessKR_Front = defaultAntirollKR;
                newCar.antirollStiffnessKR_Rear = defaultAntirollKR;
                newCar.antirollBarHeightOffset = defaultAntirollOffset;

                float lateralOffset = i * chainSpacingX;
                newCar.pivotPosition = new Vector3(lateralOffset, 0, 0);
                newCar.chassisPosition = new Vector3(0, i * initialDisplacementSpacing, 0);

                fullCars.Add(newCar);
            }
        }

        for (int i = 0; i < fullCars.Count; i++)
        {
            FullCarSystem current = fullCars[i];

            // Structural Validation Fallbacks
            if (current.chassisMass <= 0) current.chassisMass = defaultChassisMass;
            if (current.chassisInertiaPitch <= 0) current.chassisInertiaPitch = defaultChassisInertiaPitch;
            if (current.chassisInertiaRoll <= 0) current.chassisInertiaRoll = defaultChassisInertiaRoll;

            if (current.wheelMassFR <= 0) current.wheelMassFR = defaultWheelMass;
            if (current.wheelMassFL <= 0) current.wheelMassFL = defaultWheelMass;
            if (current.wheelMassRR <= 0) current.wheelMassRR = defaultWheelMass;
            if (current.wheelMassRL <= 0) current.wheelMassRL = defaultWheelMass;

            if (current.distToFront_a1 <= 0) current.distToFront_a1 = defaultDistA1;
            if (current.distToRear_a2 <= 0) current.distToRear_a2 = defaultDistA2;

            if (current.distToRight_b1 <= 0) current.distToRight_b1 = defaultDistB1;
            if (current.distToLeft_b2 <= 0) current.distToLeft_b2 = defaultDistB2;

            if (current.suspensionLengthFR <= 0) current.suspensionLengthFR = defaultSuspensionLength;
            if (current.suspensionConstantK_FR <= 0) current.suspensionConstantK_FR = defaultSuspensionK;
            if (current.dampingCoefficientC_FR <= 0) current.dampingCoefficientC_FR = defaultDampingC;

            if (current.suspensionLengthFL <= 0) current.suspensionLengthFL = defaultSuspensionLength;
            if (current.suspensionConstantK_FL <= 0) current.suspensionConstantK_FL = defaultSuspensionK;
            if (current.dampingCoefficientC_FL <= 0) current.dampingCoefficientC_FL = defaultDampingC;

            if (current.suspensionLengthRR <= 0) current.suspensionLengthRR = defaultSuspensionLength;
            if (current.suspensionConstantK_RR <= 0) current.suspensionConstantK_RR = defaultSuspensionK;
            if (current.dampingCoefficientC_RR <= 0) current.dampingCoefficientC_RR = defaultDampingC;

            if (current.suspensionLengthRL <= 0) current.suspensionLengthRL = defaultSuspensionLength;
            if (current.suspensionConstantK_RL <= 0) current.suspensionConstantK_RL = defaultSuspensionK;
            if (current.dampingCoefficientC_RL <= 0) current.dampingCoefficientC_RL = defaultDampingC;

            if (current.tireLengthFR <= 0) current.tireLengthFR = defaultTireLength;
            if (current.tireConstantKt_FR <= 0) current.tireConstantKt_FR = defaultTireKt;

            if (current.tireLengthFL <= 0) current.tireLengthFL = defaultTireLength;
            if (current.tireConstantKt_FL <= 0) current.tireConstantKt_FL = defaultTireKt;

            if (current.tireLengthRR <= 0) current.tireLengthRR = defaultTireLength;
            if (current.tireConstantKt_RR <= 0) current.tireConstantKt_RR = defaultTireKt;

            if (current.tireLengthRL <= 0) current.tireLengthRL = defaultTireLength;
            if (current.tireConstantKt_RL <= 0) current.tireConstantKt_RL = defaultTireKt;

            if (current.antirollStiffnessKR_Front <= 0) current.antirollStiffnessKR_Front = defaultAntirollKR;
            if (current.antirollStiffnessKR_Rear <= 0) current.antirollStiffnessKR_Rear = defaultAntirollKR;

            float xPos = current.pivotPosition.x;
            float zPos = current.pivotPosition.z;

            // Establish 3D Spatial Equilibriums
            current.frWheelEquilibrium = new Vector3(xPos + current.distToRight_b1, current.tireLengthFR, zPos + current.distToFront_a1);
            current.flWheelEquilibrium = new Vector3(xPos - current.distToLeft_b2, current.tireLengthFL, zPos + current.distToFront_a1);
            current.rrWheelEquilibrium = new Vector3(xPos + current.distToRight_b1, current.tireLengthRR, zPos - current.distToRear_a2);
            current.rlWheelEquilibrium = new Vector3(xPos - current.distToLeft_b2, current.tireLengthRL, zPos - current.distToRear_a2);
            current.chassisEquilibrium = new Vector3(xPos, current.tireLengthFR + current.suspensionLengthFR, zPos);

            // Objects Allocation
            GameObject chassisObj = Instantiate(chassisPrefab, transform);
            chassisObj.name = $"Full Car Chassis {i + 1}";
            current.chassisTransform = chassisObj.transform;

            current.wheelTransformFR = Instantiate(wheelPrefabFR, transform).transform; current.wheelTransformFR.name = $"FR Wheel {i + 1}";
            current.wheelTransformFL = Instantiate(wheelPrefabFL, transform).transform; current.wheelTransformFL.name = $"FL Wheel {i + 1}";
            current.wheelTransformRR = Instantiate(wheelPrefabRR, transform).transform; current.wheelTransformRR.name = $"RR Wheel {i + 1}";
            current.wheelTransformRL = Instantiate(wheelPrefabRL, transform).transform; current.wheelTransformRL.name = $"RL Wheel {i + 1}";

            // Visual Links Generation
            current.suspSpringFRLine = CreateLineVisual(chassisObj.transform, "FR_Spring", Color.red);
            current.suspDamperFRLine = CreateLineVisual(chassisObj.transform, "FR_Damper", Color.green);
            current.tireSpringFRLine = CreateLineVisual(current.wheelTransformFR, "FR_TireLine", Color.blue);

            current.suspSpringFLLine = CreateLineVisual(chassisObj.transform, "FL_Spring", Color.red);
            current.suspDamperFLLine = CreateLineVisual(chassisObj.transform, "FL_Damper", Color.green);
            current.tireSpringFLLine = CreateLineVisual(current.wheelTransformFL, "FL_TireLine", Color.blue);

            current.suspSpringRRLine = CreateLineVisual(chassisObj.transform, "RR_Spring", Color.red);
            current.suspDamperRRLine = CreateLineVisual(chassisObj.transform, "RR_Damper", Color.green);
            current.tireSpringRRLine = CreateLineVisual(current.wheelTransformRR, "RR_TireLine", Color.blue);

            current.suspSpringRLLine = CreateLineVisual(chassisObj.transform, "RL_Spring", Color.red);
            current.suspDamperRLLine = CreateLineVisual(chassisObj.transform, "RL_Damper", Color.green);
            current.tireSpringRLLine = CreateLineVisual(current.wheelTransformRL, "RL_TireLine", Color.blue);

            current.antirollFrontCenterLine = CreateLineVisual(chassisObj.transform, "AR_FrontBar", Color.cyan);
            current.antirollRearCenterLine = CreateLineVisual(chassisObj.transform, "AR_RearBar", Color.cyan);

            // Frame Alignments
            current.chassisTransform.localPosition = current.chassisEquilibrium + current.chassisPosition;
            current.wheelTransformFR.localPosition = current.frWheelEquilibrium + current.frWheelPosition;
            current.wheelTransformFL.localPosition = current.flWheelEquilibrium + current.flWheelPosition;
            current.wheelTransformRR.localPosition = current.rrWheelEquilibrium + current.rrWheelPosition;
            current.wheelTransformRL.localPosition = current.rlWheelEquilibrium + current.rlWheelPosition;

            fullCars[i] = current;
        }
    }

    private void UpdateFullCarVisuals(FullCarSystem car, float roadFR, float roadFL, float roadRR, float roadRL)
    {
        // 4-Quadrant Chassis Attachment Reference Nodes
        Vector3 nodeFR_World = car.chassisTransform.TransformPoint(new Vector3(car.distToRight_b1, 0, car.distToFront_a1));
        Vector3 nodeFL_World = car.chassisTransform.TransformPoint(new Vector3(-car.distToLeft_b2, 0, car.distToFront_a1));
        Vector3 nodeRR_World = car.chassisTransform.TransformPoint(new Vector3(car.distToRight_b1, 0, -car.distToRear_a2));
        Vector3 nodeRL_World = car.chassisTransform.TransformPoint(new Vector3(-car.distToLeft_b2, 0, -car.distToRear_a2));

        Vector3 hubFR = car.wheelTransformFR.position; Vector3 hubFL = car.wheelTransformFL.position;
        Vector3 hubRR = car.wheelTransformRR.position; Vector3 hubRL = car.wheelTransformRL.position;

        Vector3 contactFR = transform.TransformPoint(new Vector3(car.pivotPosition.x + car.distToRight_b1, roadFR, car.pivotPosition.z + car.distToFront_a1));
        Vector3 contactFL = transform.TransformPoint(new Vector3(car.pivotPosition.x - car.distToLeft_b2, roadFL, car.pivotPosition.z + car.distToFront_a1));
        Vector3 contactRR = transform.TransformPoint(new Vector3(car.pivotPosition.x + car.distToRight_b1, roadRR, car.pivotPosition.z - car.distToRear_a2));
        Vector3 contactRL = transform.TransformPoint(new Vector3(car.pivotPosition.x - car.distToLeft_b2, roadRL, car.pivotPosition.z - car.distToRear_a2));

        Vector3 visualOffset = transform.right * 0.12f;

        // Front Right Quad Rendering
        RenderCoilSpring(car.suspSpringFRLine, nodeFR_World - visualOffset, hubFR - visualOffset);
        RenderStraightLine(car.suspDamperFRLine, nodeFR_World + visualOffset, hubFR + visualOffset);
        RenderStraightLine(car.tireSpringFRLine, hubFR, contactFR);

        // Front Left Quad Rendering
        RenderCoilSpring(car.suspSpringFLLine, nodeFL_World - visualOffset, hubFL - visualOffset);
        RenderStraightLine(car.suspDamperFLLine, nodeFL_World + visualOffset, hubFL + visualOffset);
        RenderStraightLine(car.tireSpringFLLine, hubFL, contactFL);

        // Rear Right Quad Rendering
        RenderCoilSpring(car.suspSpringRRLine, nodeRR_World - visualOffset, hubRR - visualOffset);
        RenderStraightLine(car.suspDamperRRLine, nodeRR_World + visualOffset, hubRR + visualOffset);
        RenderStraightLine(car.tireSpringRRLine, hubRR, contactRR);

        // Rear Left Quad Rendering
        RenderCoilSpring(car.suspSpringRLLine, nodeRL_World - visualOffset, hubRL - visualOffset);
        RenderStraightLine(car.suspDamperRLLine, nodeRL_World + visualOffset, hubRL + visualOffset);
        RenderStraightLine(car.tireSpringRLLine, hubRL, contactRL);

        // Antiroll Bar Torsion Rod Indicators
        Vector3 frontBarCenter = car.chassisTransform.position + car.chassisTransform.forward * car.distToFront_a1 - car.chassisTransform.up * car.antirollBarHeightOffset;
        RenderStraightLine(car.antirollFrontCenterLine, frontBarCenter - car.chassisTransform.right * car.distToLeft_b2, frontBarCenter + car.chassisTransform.right * car.distToRight_b1);

        // Toggle rear antiroll bar visibility dynamically based on user customize configuration
        if (!car.useFrontAntirollBarOnly)
        {
            if (!car.antirollRearCenterLine.enabled) car.antirollRearCenterLine.enabled = true;

            Vector3 rearBarCenter = car.chassisTransform.position - car.chassisTransform.forward * car.distToRear_a2 - car.chassisTransform.up * car.antirollBarHeightOffset;
            RenderStraightLine(car.antirollRearCenterLine, rearBarCenter - car.chassisTransform.right * car.distToLeft_b2, rearBarCenter + car.chassisTransform.right * car.distToRight_b1);
        }
        else
        {
            if (car.antirollRearCenterLine.enabled) car.antirollRearCenterLine.enabled = false;
        }
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
        line.startWidth = 0.04f; line.endWidth = 0.04f;
        line.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));
        line.startColor = color; line.endColor = color;
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

    private void VisualizeQuadGraph(float timeX, float chassisY, float frY, float flY, float rrY, float rlY)
    {
        GameObject cPoint = Instantiate(graphPointPrefabChassis, graphContainer, false);
        cPoint.GetComponent<RectTransform>().anchoredPosition = new Vector2(timeX * graphScale, chassisY * graphScale);

        GameObject frPoint = Instantiate(graphPointPrefabFRWheel, graphContainer, false);
        frPoint.GetComponent<RectTransform>().anchoredPosition = new Vector2(timeX * graphScale, frY * graphScale);

        GameObject flPoint = Instantiate(graphPointPrefabFLWheel, graphContainer, false);
        flPoint.GetComponent<RectTransform>().anchoredPosition = new Vector2(timeX * graphScale, flY * graphScale);

        GameObject rrPoint = Instantiate(graphPointPrefabRRWheel, graphContainer, false);
        rrPoint.GetComponent<RectTransform>().anchoredPosition = new Vector2(timeX * graphScale, rrY * graphScale);

        GameObject rlPoint = Instantiate(graphPointPrefabRLWheel, graphContainer, false);
        rlPoint.GetComponent<RectTransform>().anchoredPosition = new Vector2(timeX * graphScale, rlY * graphScale);

        QuadGraphPoints frame = new QuadGraphPoints
        {
            chassisPoint = cPoint,
            wheelPointFR = frPoint,
            wheelPointFL = flPoint,
            wheelPointRR = rrPoint,
            wheelPointRL = rlPoint
        };
        activeGraphPoints.Enqueue(frame);

        if (activeGraphPoints.Count > maxDataPoints)
        {
            QuadGraphPoints oldest = activeGraphPoints.Dequeue();
            if (oldest.chassisPoint != null) Destroy(oldest.chassisPoint);
            if (oldest.wheelPointFR != null) Destroy(oldest.wheelPointFR);
            if (oldest.wheelPointFL != null) Destroy(oldest.wheelPointFL);
            if (oldest.wheelPointRR != null) Destroy(oldest.wheelPointRR);
            if (oldest.wheelPointRL != null) Destroy(oldest.wheelPointRL);
        }
    }
}
