using System.Collections.Generic;
using UnityEngine;

public class ForwardMotion : MonoBehaviour
{
    [System.Serializable]
    public struct FullCarSystem
    {
        [Header("WASD Input Controls:")]
        public bool isPlayerControlled;
        public float motorForceWASD;
        public float brakeForceWASD;
        [Tooltip("Maximum steering angle of the front wheels in degrees")]
        public float maxSteerAngleWASD;

        [Header("Mass Profiles:")]
        public float chassisMass;
        public float chassisInertiaPitch; 
        public float chassisInertiaRoll;  
        [Space]
        public float wheelMassFR; 
        public float wheelMassFL; 
        public float wheelMassRR; 
        public float wheelMassRL; 

        [Header("Chassis Geometry & COM:")]
        public float distToFront_a1; 
        public float distToRear_a2;  
        [Space]
        public float distToRight_b1; 
        public float distToLeft_b2;  
        [Space]

        //Drive Parameters:
        [Tooltip("Height of Center of Mass above axle planes for load transfer torque")]
        public float heightOfCOM;    // h

        [Header("Longitudinal Performance Inputs:")]
        [Tooltip("Total drive force applied to wheels (Overridden if isPlayerControlled = true)")]
        public float driveForceInput;
        public float aerodynamicDragCoefficient; // C_d
        public float frontalArea;                // A
        public float rollingResistanceCoefficient; // f_r

        [Header("Front Right Suspension Profile:")]
        public float suspensionLengthFR;
        public float suspensionConstantK_FR;
        public float dampingCoefficientC_FR;

        [Header("Front Left Suspension Profile:")]
        public float suspensionLengthFL;
        public float suspensionConstantK_FL;
        public float dampingCoefficientC_FL;

        [Header("Rear Right Suspension Profile:")]
        public float suspensionLengthRR;
        public float suspensionConstantK_RR;
        public float dampingCoefficientC_RR;

        [Header("Rear Left Suspension Profile:")]
        public float suspensionLengthRL;
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
        public float antirollBarHeightOffset;

        [Header("Position & State Properties:")]
        public Vector3 chassisPosition;
        public Vector3 chassisVelocity;
        public Vector3 chassisAcceleration;
        [Space(5)]
        public Vector3 chassisAngle;
        public Vector3 chassisAngularVelocity;
        public Vector3 chassisAngularAcceleration;

        //Driving Info:
        [Header("Planar Trajectory States (Read Only):")]
        public Vector2 planarPosition; 
        public float headingAngle;     
        public float forwardVelocity;
        public float forwardAcceleration;

        [Space(10)]
        public Vector3 frWheelPosition;
        public Vector3 frWheelVelocity;
        public Vector3 frWheelAcceleration;
        [Space(5)]
        public Vector3 flWheelPosition;
        public Vector3 flWheelVelocity;
        public Vector3 flWheelAcceleration;
        [Space(5)]
        public Vector3 rrWheelPosition;
        public Vector3 rrWheelVelocity;
        public Vector3 rrWheelAcceleration;
        [Space(5)]
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
        [HideInInspector] public LineRenderer antirollFrontCenterLine;
        [HideInInspector] public LineRenderer antirollRearCenterLine;

        // Vector Force Visualizers
        [HideInInspector] public LineRenderer vectorWeightLine;
        [HideInInspector] public LineRenderer vectorInertialLine;
        [HideInInspector] public LineRenderer vectorDragLine;
        [HideInInspector] public LineRenderer vectorNormalFR;
        [HideInInspector] public LineRenderer vectorNormalFL;
        [HideInInspector] public LineRenderer vectorNormalRR;
        [HideInInspector] public LineRenderer vectorNormalRL;
        [HideInInspector] public LineRenderer vectorTractiveFR;
        [HideInInspector] public LineRenderer vectorTractiveFL;
        [HideInInspector] public LineRenderer vectorTractiveRR;
        [HideInInspector] public LineRenderer vectorTractiveRL;
    }

    [Header("--------------------------------------------------------------------------")]
    [Header("Simulation Settings:")]
    [Range(0.1f, 5f)] public float TimeScale = 1.0f;
    public float globalGravity = 9.81f;
    public float airDensity = 1.225f; 
    public LayerMask groundLayer;

    [Header("Vector Presentation:")]
    public float forceVectorScale = 0.0003f;
    public float vectorLineWidth = 0.06f;

    [Header("--------------------------------------------------------------------------")]
    [Header("Global Default Properties:")]
    public float defaultChassisMass = 1400f;
    public float defaultChassisInertiaPitch = 2000f;
    public float defaultChassisInertiaRoll = 1600f;
    [Space]
    public float defaultWheelMass = 45f;
    [Space]
    public float defaultDistA1 = 1.3f;
    public float defaultDistA2 = 1.5f;
    public float defaultDistB1 = 0.8f;
    public float defaultDistB2 = 0.8f;
    public float defaultHeightCOM = 0.55f;
    [Space]
    public float defaultDriveForce = 3500f;
    public float defaultCd = 0.32f;
    public float defaultFrontalArea = 2.2f;
    public float defaultRollingResistance = 0.015f;
    [Space]
    public float defaultSuspensionLength = 1.5f;
    public float defaultSuspensionK = 32000f;
    public float defaultDampingC = 2500f;
    [Space]
    public float defaultTireLength = 0.4f;
    public float defaultTireKt = 160000f;
    [Space]
    public float defaultAntirollKR = 15000f;
    public float defaultAntirollOffset = 0.4f;

    [Header("--------------------------------------------------------------------------")]
    [Header("Spring Visual Customization:")]
    public int springCoilSegments = 60;
    public float springCoilRadius = 0.12f;
    public float springCoilFrequency = 10f;

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

        for (int i = 0; i < fullCars.Count; i++)
        {
            FullCarSystem car = fullCars[i];
            if (car.chassisTransform == null) continue;

            // --- 1. WASD Input & Kinematic Steering ---
            float yawRate = 0f;
            float lateralAcceleration = 0f;

            if (car.isPlayerControlled)
            {
                float accelInput = Input.GetAxis("Vertical");   // W / S
                float steerInput = Input.GetAxis("Horizontal"); // A / D

                // Map Drive Force
                car.driveForceInput = accelInput > 0 ? accelInput * car.motorForceWASD : accelInput * car.brakeForceWASD;

                // Bicycle Model Kinematic Steering
                float wheelbase = car.distToFront_a1 + car.distToRear_a2;
                float steerRad = steerInput * car.maxSteerAngleWASD * Mathf.Deg2Rad;

                // Protect against divide by zero at a standstill
                if (Mathf.Abs(car.forwardVelocity) > 0.1f)
                {
                    yawRate = (car.forwardVelocity / wheelbase) * Mathf.Tan(steerRad);
                    car.headingAngle += yawRate * dt;
                    lateralAcceleration = car.forwardVelocity * yawRate;
                }
            }

            // Create global yaw rotation matrix for spatial coordinate generation
            Quaternion yawRot = Quaternion.Euler(0, car.headingAngle * Mathf.Rad2Deg, 0);

            // Calculate global wheel planar offsets rotated by heading
            Vector3 offsetFR = yawRot * new Vector3(car.distToRight_b1, 0, car.distToFront_a1);
            Vector3 offsetFL = yawRot * new Vector3(-car.distToLeft_b2, 0, car.distToFront_a1);
            Vector3 offsetRR = yawRot * new Vector3(car.distToRight_b1, 0, -car.distToRear_a2);
            Vector3 offsetRL = yawRot * new Vector3(-car.distToLeft_b2, 0, -car.distToRear_a2);

            Vector3 globalCenter = car.pivotPosition + new Vector3(car.planarPosition.x, 0, car.planarPosition.y);

            // Sample road profiles based on updated rotated world coordinates
            float roadY_FR = SampleRoadHeightAtPoint(globalCenter.x + offsetFR.x, globalCenter.z + offsetFR.z);
            float roadY_FL = SampleRoadHeightAtPoint(globalCenter.x + offsetFL.x, globalCenter.z + offsetFL.z);
            float roadY_RR = SampleRoadHeightAtPoint(globalCenter.x + offsetRR.x, globalCenter.z + offsetRR.z);
            float roadY_RL = SampleRoadHeightAtPoint(globalCenter.x + offsetRL.x, globalCenter.z + offsetRL.z);

            // --- 2. Suspension & Dynamics State Profiles ---
            float x = car.chassisPosition.y; float xDot = car.chassisVelocity.y;
            float theta = car.chassisAngle.x; float thetaDot = car.chassisAngularVelocity.x;
            float phi = car.chassisAngle.z; float phiDot = car.chassisAngularVelocity.z;

            float xFR = car.frWheelPosition.y; float xFRDot = car.frWheelVelocity.y;
            float xFL = car.flWheelPosition.y; float xFLDot = car.flWheelVelocity.y;
            float xRR = car.rrWheelPosition.y; float xRRDot = car.rrWheelVelocity.y;
            float xRL = car.rlWheelPosition.y; float xRLDot = car.rlWheelVelocity.y;

            float suspDeltaPosFR = x - xFR - (car.distToFront_a1 * theta) + (car.distToRight_b1 * phi);
            float suspDeltaVelFR = xDot - xFRDot - (car.distToFront_a1 * thetaDot) + (car.distToRight_b1 * phiDot);

            float suspDeltaPosFL = x - xFL - (car.distToFront_a1 * theta) - (car.distToLeft_b2 * phi);
            float suspDeltaVelFL = xDot - xFLDot - (car.distToFront_a1 * thetaDot) - (car.distToLeft_b2 * phiDot);

            float suspDeltaPosRR = x - xRR + (car.distToRear_a2 * theta) + (car.distToRight_b1 * phi);
            float suspDeltaVelRR = xDot - xRRDot + (car.distToRear_a2 * thetaDot) + (car.distToRight_b1 * phiDot);

            float suspDeltaPosRL = x - xRL + (car.distToRear_a2 * theta) - (car.distToLeft_b2 * phi);
            float suspDeltaVelRL = xDot - xRLDot + (car.distToRear_a2 * thetaDot) - (car.distToLeft_b2 * phiDot);

            float tireDeltaPosFR = xFR - roadY_FR;
            float tireDeltaPosFL = xFL - roadY_FL;
            float tireDeltaPosRR = xRR - roadY_RR;
            float tireDeltaPosRL = xRL - roadY_RL;

            float forceTireFR = (tireDeltaPosFR < 0f) ? car.tireConstantKt_FR * tireDeltaPosFR : 0f;
            float forceTireFL = (tireDeltaPosFL < 0f) ? car.tireConstantKt_FL * tireDeltaPosFL : 0f;
            float forceTireRR = (tireDeltaPosRR < 0f) ? car.tireConstantKt_RR * tireDeltaPosRR : 0f;
            float forceTireRL = (tireDeltaPosRL < 0f) ? car.tireConstantKt_RL * tireDeltaPosRL : 0f;

            float F_FR = (car.suspensionConstantK_FR * suspDeltaPosFR) + (car.dampingCoefficientC_FR * suspDeltaVelFR);
            float F_FL = (car.suspensionConstantK_FL * suspDeltaPosFL) + (car.dampingCoefficientC_FL * suspDeltaVelFL);
            float F_RR = (car.suspensionConstantK_RR * suspDeltaPosRR) + (car.dampingCoefficientC_RR * suspDeltaVelRR);
            float F_RL = (car.suspensionConstantK_RL * suspDeltaPosRL) + (car.dampingCoefficientC_RL * suspDeltaVelRL);

            float M_R_Front = 0f, M_R_Rear = 0f;
            float trackWidth = car.distToRight_b1 + car.distToLeft_b2;
            if (car.useAdvancedModel)
            {
                M_R_Front = -car.antirollStiffnessKR_Front * (phi - ((xFR - xFL) / trackWidth));
                if (!car.useFrontAntirollBarOnly) M_R_Rear = -car.antirollStiffnessKR_Rear * (phi - ((xRR - xRL) / trackWidth));
            }
            else
            {
                M_R_Front = -car.antirollStiffnessKR_Front * phi;
                if (!car.useFrontAntirollBarOnly) M_R_Rear = -car.antirollStiffnessKR_Rear * phi;
            }

            // --- 3. Longitudinal Multi-Force Logic & Aerodynamics ---
            float velocityZ = car.forwardVelocity;
            float dragMag = 0.5f * airDensity * car.aerodynamicDragCoefficient * car.frontalArea * (velocityZ * velocityZ);
            float dragForce = velocityZ >= 0f ? -dragMag : dragMag;

            float normalForceTotal = Mathf.Abs(forceTireFR) + Mathf.Abs(forceTireFL) + Mathf.Abs(forceTireRR) + Mathf.Abs(forceTireRL);
            float rollResistMag = car.rollingResistanceCoefficient * normalForceTotal;
            float rollingResistanceForce = velocityZ > 0.05f ? -rollResistMag : (velocityZ < -0.05f ? rollResistMag : 0f);

            float netForwardForce = car.driveForceInput + dragForce + rollingResistanceForce;
            float totalMass = car.chassisMass + car.wheelMassFR + car.wheelMassFL + car.wheelMassRR + car.wheelMassRL;
            car.forwardAcceleration = netForwardForce / totalMass;

            // --- 4. Dynamic Coupled Equations of Motion ---
            float accBounce = (-F_FR - F_FL - F_RR - F_RL) / car.chassisMass - globalGravity;

            // Pitch coupling (Suspension Moments + Longitudinal Acceleration Inertia)
            float torquePitchSuspension = car.distToFront_a1 * (F_FR + F_FL) - car.distToRear_a2 * (F_RR + F_RL);
            float torquePitchInertial = car.chassisMass * car.forwardAcceleration * car.heightOfCOM;
            float accPitch = (torquePitchSuspension + torquePitchInertial) / car.chassisInertiaPitch;

            // Roll coupling (Suspension Moments + Anti-roll + Lateral Centrifugal Inertia from Steering)
            float torqueRollSuspension = -car.distToRight_b1 * (F_FR + F_RR) + car.distToLeft_b2 * (F_FL + F_RL);
            float torqueRollInertial = car.chassisMass * lateralAcceleration * car.heightOfCOM; // Causes body lean in turns
            float accRoll = (torqueRollSuspension + M_R_Front + M_R_Rear + torqueRollInertial) / car.chassisInertiaRoll;

            float accWheelFR = (F_FR - forceTireFR) / car.wheelMassFR - globalGravity;
            float accWheelFL = (F_FL - forceTireFL) / car.wheelMassFL - globalGravity;
            float accWheelRR = (F_RR - forceTireRR) / car.wheelMassRR - globalGravity;
            float accWheelRL = (F_RL - forceTireRL) / car.wheelMassRL - globalGravity;

            // --- 5. Semi-Implicit Euler Numerical Solvers ---
            car.forwardVelocity += car.forwardAcceleration * dt;

            // Integrate velocity along heading vector for X and Z translation
            car.planarPosition.x += car.forwardVelocity * Mathf.Sin(car.headingAngle) * dt;
            car.planarPosition.y += car.forwardVelocity * Mathf.Cos(car.headingAngle) * dt;

            car.chassisAcceleration = new Vector3(0, accBounce, 0);
            car.chassisVelocity += car.chassisAcceleration * dt;
            car.chassisPosition += car.chassisVelocity * dt;

            car.chassisAngularAcceleration = new Vector3(accPitch, 0, accRoll);
            car.chassisAngularVelocity += car.chassisAngularAcceleration * dt;
            car.chassisAngle += car.chassisAngularVelocity * dt;

            car.frWheelAcceleration = new Vector3(0, accWheelFR, 0); car.frWheelVelocity += car.frWheelAcceleration * dt; car.frWheelPosition += car.frWheelVelocity * dt;
            car.flWheelAcceleration = new Vector3(0, accWheelFL, 0); car.flWheelVelocity += car.flWheelAcceleration * dt; car.flWheelPosition += car.flWheelVelocity * dt;
            car.rrWheelAcceleration = new Vector3(0, accWheelRR, 0); car.rrWheelVelocity += car.rrWheelAcceleration * dt; car.rrWheelPosition += car.rrWheelVelocity * dt;
            car.rlWheelAcceleration = new Vector3(0, accWheelRL, 0); car.rlWheelVelocity += car.rlWheelAcceleration * dt; car.rlWheelPosition += car.rlWheelVelocity * dt;

            // --- 6. Transform Spatial World Mapping ---
            Vector3 translatedChassisPos = new Vector3(globalCenter.x, car.chassisEquilibrium.y + car.chassisPosition.y, globalCenter.z);
            car.chassisTransform.localPosition = translatedChassisPos;

            // Apply heading rotation followed by suspension pitch/roll angles
            car.chassisTransform.localRotation = yawRot * Quaternion.Euler(car.chassisAngle.x * Mathf.Rad2Deg, 0, car.chassisAngle.z * Mathf.Rad2Deg);

            car.wheelTransformFR.localPosition = new Vector3(globalCenter.x + offsetFR.x, car.frWheelEquilibrium.y + car.frWheelPosition.y, globalCenter.z + offsetFR.z);
            car.wheelTransformFL.localPosition = new Vector3(globalCenter.x + offsetFL.x, car.flWheelEquilibrium.y + car.flWheelPosition.y, globalCenter.z + offsetFL.z);
            car.wheelTransformRR.localPosition = new Vector3(globalCenter.x + offsetRR.x, car.rrWheelEquilibrium.y + car.rrWheelPosition.y, globalCenter.z + offsetRR.z);
            car.wheelTransformRL.localPosition = new Vector3(globalCenter.x + offsetRL.x, car.rlWheelEquilibrium.y + car.rlWheelPosition.y, globalCenter.z + offsetRL.z);

            car.wheelTransformFR.localRotation = yawRot;
            car.wheelTransformFL.localRotation = yawRot;
            car.wheelTransformRR.localRotation = yawRot;
            car.wheelTransformRL.localRotation = yawRot;

            // --- 7. Graphics Vector Engine ---
            UpdateFullCarVisuals(car, globalCenter, yawRot, roadY_FR, roadY_FL, roadY_RR, roadY_RL);
            UpdateAnalyticalForceVectors(car, dragForce, forceTireFR, forceTireFL, forceTireRR, forceTireRL);

            fullCars[i] = car;
        }
    }

    private void UpdateAnalyticalForceVectors(FullCarSystem car, float dragForce, float fFR, float fFL, float fRR, float fRL)
    {
        Vector3 comPos = car.chassisTransform.position;
        Vector3 heading = car.chassisTransform.forward;

        float totalMass = car.chassisMass + car.wheelMassFR + car.wheelMassFL + car.wheelMassRR + car.wheelMassRL;
        RenderStraightLine(car.vectorWeightLine, comPos, comPos + Vector3.down * (totalMass * globalGravity * forceVectorScale));

        Vector3 inertialEnd = comPos - heading * (car.chassisMass * car.forwardAcceleration * forceVectorScale);
        RenderStraightLine(car.vectorInertialLine, comPos, inertialEnd);

        Vector3 dragEnd = comPos + Vector3.back * (Mathf.Abs(dragForce) * forceVectorScale);
        RenderStraightLine(car.vectorDragLine, comPos, dragEnd);

        Vector3 pFR = car.wheelTransformFR.position; Vector3 pFL = car.wheelTransformFL.position;
        Vector3 pRR = car.wheelTransformRR.position; Vector3 pRL = car.wheelTransformRL.position;

        RenderStraightLine(car.vectorNormalFR, pFR, pFR + Vector3.up * (Mathf.Abs(fFR) * forceVectorScale));
        RenderStraightLine(car.vectorNormalFL, pFL, pFL + Vector3.up * (Mathf.Abs(fFL) * forceVectorScale));
        RenderStraightLine(car.vectorNormalRR, pRR, pRR + Vector3.up * (Mathf.Abs(fRR) * forceVectorScale));
        RenderStraightLine(car.vectorNormalRL, pRL, pRL + Vector3.up * (Mathf.Abs(fRL) * forceVectorScale));

        float quadDriveForce = car.driveForceInput * 0.25f * forceVectorScale;
        RenderStraightLine(car.vectorTractiveFR, pFR, pFR + heading * quadDriveForce);
        RenderStraightLine(car.vectorTractiveFL, pFL, pFL + heading * quadDriveForce);
        RenderStraightLine(car.vectorTractiveRR, pRR, pRR + heading * quadDriveForce);
        RenderStraightLine(car.vectorTractiveRL, pRL, pRL + heading * quadDriveForce);
    }

    private void SetupCarSystems()
    {
        for (int i = 0; i < fullCars.Count; i++)
        {
            FullCarSystem current = fullCars[i];

            // Assign default fallbacks
            if (current.motorForceWASD <= 0) current.motorForceWASD = 5000f;
            if (current.brakeForceWASD <= 0) current.brakeForceWASD = 8000f;
            if (current.maxSteerAngleWASD <= 0) current.maxSteerAngleWASD = 35f;

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

            current.frWheelEquilibrium = new Vector3(xPos + current.distToRight_b1, current.tireLengthFR, zPos + current.distToFront_a1);
            current.flWheelEquilibrium = new Vector3(xPos - current.distToLeft_b2, current.tireLengthFL, zPos + current.distToFront_a1);
            current.rrWheelEquilibrium = new Vector3(xPos + current.distToRight_b1, current.tireLengthRR, zPos - current.distToRear_a2);
            current.rlWheelEquilibrium = new Vector3(xPos - current.distToLeft_b2, current.tireLengthRL, zPos - current.distToRear_a2);
            current.chassisEquilibrium = new Vector3(xPos, current.tireLengthFR + current.suspensionLengthFR, zPos);

            // Instantiate GameObjects
            GameObject chassisObj = Instantiate(chassisPrefab, transform);
            chassisObj.name = $"Full Car Chassis {i + 1}";
            current.chassisTransform = chassisObj.transform;

            current.wheelTransformFR = Instantiate(wheelPrefabFR, transform).transform; current.wheelTransformFR.name = $"FR Wheel {i + 1}";
            current.wheelTransformFL = Instantiate(wheelPrefabFL, transform).transform; current.wheelTransformFL.name = $"FL Wheel {i + 1}";
            current.wheelTransformRR = Instantiate(wheelPrefabRR, transform).transform; current.wheelTransformRR.name = $"RR Wheel {i + 1}";
            current.wheelTransformRL = Instantiate(wheelPrefabRL, transform).transform; current.wheelTransformRL.name = $"RL Wheel {i + 1}";

            // Generation of Mechanical Structural Lines
            current.suspSpringFRLine = CreateLineVisual(current.chassisTransform, "FR_Spring", Color.red, 0.04f);
            current.suspDamperFRLine = CreateLineVisual(current.chassisTransform, "FR_Damper", Color.green, 0.04f);
            current.tireSpringFRLine = CreateLineVisual(current.wheelTransformFR, "FR_TireLine", Color.blue, 0.04f);

            current.suspSpringFLLine = CreateLineVisual(current.chassisTransform, "FL_Spring", Color.red, 0.04f);
            current.suspDamperFLLine = CreateLineVisual(current.chassisTransform, "FL_Damper", Color.green, 0.04f);
            current.tireSpringFLLine = CreateLineVisual(current.wheelTransformFL, "FL_TireLine", Color.blue, 0.04f);

            current.suspSpringRRLine = CreateLineVisual(current.chassisTransform, "RR_Spring", Color.red, 0.04f);
            current.suspDamperRRLine = CreateLineVisual(current.chassisTransform, "RR_Damper", Color.green, 0.04f);
            current.tireSpringRRLine = CreateLineVisual(current.wheelTransformRR, "RR_TireLine", Color.blue, 0.04f);

            current.suspSpringRLLine = CreateLineVisual(current.chassisTransform, "RL_Spring", Color.red, 0.04f);
            current.suspDamperRLLine = CreateLineVisual(current.chassisTransform, "RL_Damper", Color.green, 0.04f);
            current.tireSpringRLLine = CreateLineVisual(current.wheelTransformRL, "RL_TireLine", Color.blue, 0.04f);

            current.antirollFrontCenterLine = CreateLineVisual(current.chassisTransform, "AR_FrontBar", Color.cyan, 0.04f);
            current.antirollRearCenterLine = CreateLineVisual(current.chassisTransform, "AR_RearBar", Color.cyan, 0.04f);

            // Vector Field Allocation
            current.vectorWeightLine = CreateLineVisual(current.chassisTransform, "V_Weight", Color.green, vectorLineWidth);
            current.vectorInertialLine = CreateLineVisual(current.chassisTransform, "V_Inertial", Color.magenta, vectorLineWidth);
            current.vectorDragLine = CreateLineVisual(current.chassisTransform, "V_Drag", Color.yellow, vectorLineWidth);

            current.vectorNormalFR = CreateLineVisual(current.wheelTransformFR, "V_NormFR", Color.blue, vectorLineWidth);
            current.vectorNormalFL = CreateLineVisual(current.wheelTransformFL, "V_NormFL", Color.blue, vectorLineWidth);
            current.vectorNormalRR = CreateLineVisual(current.wheelTransformRR, "V_NormRR", Color.blue, vectorLineWidth);
            current.vectorNormalRL = CreateLineVisual(current.wheelTransformRL, "V_NormRL", Color.blue, vectorLineWidth);

            current.vectorTractiveFR = CreateLineVisual(current.wheelTransformFR, "V_TracFR", Color.red, vectorLineWidth);
            current.vectorTractiveFL = CreateLineVisual(current.wheelTransformFL, "V_TracFL", Color.red, vectorLineWidth);
            current.vectorTractiveRR = CreateLineVisual(current.wheelTransformRR, "V_TracRR", Color.red, vectorLineWidth);
            current.vectorTractiveRL = CreateLineVisual(current.wheelTransformRL, "V_TracRL", Color.red, vectorLineWidth);

            fullCars[i] = current;
        }
    }

    private void UpdateFullCarVisuals(FullCarSystem car, Vector3 globalCenter, Quaternion yawRot, float roadFR, float roadFL, float roadRR, float roadRL)
    {
        Vector3 nodeFR_World = car.chassisTransform.TransformPoint(new Vector3(car.distToRight_b1, 0, car.distToFront_a1));
        Vector3 nodeFL_World = car.chassisTransform.TransformPoint(new Vector3(-car.distToLeft_b2, 0, car.distToFront_a1));
        Vector3 nodeRR_World = car.chassisTransform.TransformPoint(new Vector3(car.distToRight_b1, 0, -car.distToRear_a2));
        Vector3 nodeRL_World = car.chassisTransform.TransformPoint(new Vector3(-car.distToLeft_b2, 0, -car.distToRear_a2));

        Vector3 hubFR = car.wheelTransformFR.position; Vector3 hubFL = car.wheelTransformFL.position;
        Vector3 hubRR = car.wheelTransformRR.position; Vector3 hubRL = car.wheelTransformRL.position;

        Vector3 offsetFR = yawRot * new Vector3(car.distToRight_b1, 0, car.distToFront_a1);
        Vector3 offsetFL = yawRot * new Vector3(-car.distToLeft_b2, 0, car.distToFront_a1);
        Vector3 offsetRR = yawRot * new Vector3(car.distToRight_b1, 0, -car.distToRear_a2);
        Vector3 offsetRL = yawRot * new Vector3(-car.distToLeft_b2, 0, -car.distToRear_a2);

        Vector3 contactFR = transform.TransformPoint(new Vector3(globalCenter.x + offsetFR.x, roadFR, globalCenter.z + offsetFR.z));
        Vector3 contactFL = transform.TransformPoint(new Vector3(globalCenter.x + offsetFL.x, roadFL, globalCenter.z + offsetFL.z));
        Vector3 contactRR = transform.TransformPoint(new Vector3(globalCenter.x + offsetRR.x, roadRR, globalCenter.z + offsetRR.z));
        Vector3 contactRL = transform.TransformPoint(new Vector3(globalCenter.x + offsetRL.x, roadRL, globalCenter.z + offsetRL.z));

        Vector3 visualOffset = transform.right * 0.12f;

        RenderCoilSpring(car.suspSpringFRLine, nodeFR_World - visualOffset, hubFR - visualOffset);
        RenderStraightLine(car.suspDamperFRLine, nodeFR_World + visualOffset, hubFR + visualOffset);
        RenderStraightLine(car.tireSpringFRLine, hubFR, contactFR);

        RenderCoilSpring(car.suspSpringFLLine, nodeFL_World - visualOffset, hubFL - visualOffset);
        RenderStraightLine(car.suspDamperFLLine, nodeFL_World + visualOffset, hubFL + visualOffset);
        RenderStraightLine(car.tireSpringFLLine, hubFL, contactFL);

        RenderCoilSpring(car.suspSpringRRLine, nodeRR_World - visualOffset, hubRR - visualOffset);
        RenderStraightLine(car.suspDamperRRLine, nodeRR_World + visualOffset, hubRR + visualOffset);
        RenderStraightLine(car.tireSpringRRLine, hubRR, contactRR);

        RenderCoilSpring(car.suspSpringRLLine, nodeRL_World - visualOffset, hubRL - visualOffset);
        RenderStraightLine(car.suspDamperRLLine, nodeRL_World + visualOffset, hubRL + visualOffset);
        RenderStraightLine(car.tireSpringRLLine, hubRL, contactRL);

        Vector3 frontBarCenter = car.chassisTransform.position + car.chassisTransform.forward * car.distToFront_a1 - car.chassisTransform.up * car.antirollBarHeightOffset;
        RenderStraightLine(car.antirollFrontCenterLine, frontBarCenter - car.chassisTransform.right * car.distToLeft_b2, frontBarCenter + car.chassisTransform.right * car.distToRight_b1);

        if (!car.useFrontAntirollBarOnly)
        {
            if (!car.antirollRearCenterLine.enabled) car.antirollRearCenterLine.enabled = true;
            Vector3 rearBarCenter = car.chassisTransform.position - car.chassisTransform.forward * car.distToRear_a2 - car.chassisTransform.up * car.antirollBarHeightOffset;
            RenderStraightLine(car.antirollRearCenterLine, rearBarCenter - car.chassisTransform.right * car.distToLeft_b2, rearBarCenter + car.chassisTransform.right * car.distToRight_b1);
        }
        else if (car.antirollRearCenterLine.enabled)
        {
            car.antirollRearCenterLine.enabled = false;
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

    private LineRenderer CreateLineVisual(Transform parent, string name, Color color, float width)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        LineRenderer line = obj.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = width; line.endWidth = width;
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
        Vector3 direction = end - start;
        float length = direction.magnitude;
        if (length <= 0.01f) return;

        line.positionCount = springCoilSegments + 1;
        Vector3 axisUp = direction.normalized;
        Vector3 axisRight = Vector3.Cross(axisUp, Vector3.forward).magnitude < 0.1f ? Vector3.Cross(axisUp, Vector3.right).normalized : Vector3.Cross(axisUp, Vector3.forward).normalized;
        Vector3 axisForward = Vector3.Cross(axisRight, axisUp).normalized;

        for (int i = 0; i <= springCoilSegments; i++)
        {
            float linearProgress = (float)i / springCoilSegments;
            float angle = linearProgress * springCoilFrequency * 2f * Mathf.PI;
            Vector3 pointOnCoil = start + (axisUp * linearProgress * length) + (axisRight * Mathf.Sin(angle) * springCoilRadius) + (axisForward * Mathf.Cos(angle) * springCoilRadius);
            line.SetPosition(i, pointOnCoil);
        }
    }

    private void VisualizeQuadGraph(float time, float chassisY, float fr, float fl, float rr, float rl)
    {
        // Custom graphing queue logic can be linked here to feed UI layouts or debug canvases.
    }
}