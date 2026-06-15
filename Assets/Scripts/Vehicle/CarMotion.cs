using System.Collections.Generic;
using UnityEngine;

public class CarMotion : MonoBehaviour
{
    [System.Serializable]
    public struct FullCarSystem10DOF
    {
        [Header("Mass & Inertia Profiles:")]
        public float chassisMass;
        public float chassisInertiaPitch; // I_y
        public float chassisInertiaRoll;  // I_x
        public float chassisInertiaYaw;   // I_z (New for 10-DOF Handling)
        [Space]
        public float wheelMassFR;
        public float wheelMassFL;
        public float wheelMassRR;
        public float wheelMassRL;

        [Header("Chassis Geometry (Distance from COM):")]
        public float distToFront_a1;
        public float distToRear_a2;
        [Space]
        public float distToRight_b1;
        public float distToLeft_b2;

        [Header("Tire Handling Characteristics:")]
        public float corneringStiffnessFront; // C_alpha front
        public float corneringStiffnessRear;  // C_alpha rear
        public float maxSteeringAngle;        // In degrees

        [Header("Suspension Stiffness & Damping:")]
        public float suspensionConstantK_FR; public float dampingCoefficientC_FR; public float suspensionLengthFR;
        public float suspensionConstantK_FL; public float dampingCoefficientC_FL; public float suspensionLengthFL;
        public float suspensionConstantK_RR; public float dampingCoefficientC_RR; public float suspensionLengthRR;
        public float suspensionConstantK_RL; public float dampingCoefficientC_RL; public float suspensionLengthRL;

        [Header("Tire Vertical Spring Profiles:")]
        public float tireConstantKt_FR; public float tireLengthFR;
        public float tireConstantKt_FL; public float tireLengthFL;
        public float tireConstantKt_RR; public float tireLengthRR;
        public float tireConstantKt_RL; public float tireLengthRL;

        [Header("Antiroll Bar Profiles:")]
        public bool useAdvancedModel;
        public bool useFrontAntirollBarOnly;
        public float antirollStiffnessKR_Front;
        public float antirollStiffnessKR_Rear;
        public float antirollBarHeightOffset;

        [Header("10-DOF State Profiles:")]
        public Vector3 chassisPosition;           // World Space XYZ (X=Lat, Y=Bounce, Z=Long)
        public Vector3 chassisVelocity;           // World Space XYZ
        public Vector3 chassisAcceleration;       // World Space XYZ
        [Space]
        public Vector3 chassisAngle;              // X = Pitch, Y = Yaw (New), Z = Roll
        public Vector3 chassisAngularVelocity;    // X = PitchRate, Y = YawRate (New), Z = RollRate
        public Vector3 chassisAngularAcceleration;// X = PitchAcc, Y = YawAcc (New), Z = RollAcc
        [Space]
        public Vector3 frWheelPosition;           // Y = Vertical Hop position
        public Vector3 frWheelVelocity;           // Y = Vertical Hop velocity
        public Vector3 frWheelAcceleration;       // Y = Vertical Hop acceleration
        [Space]
        public Vector3 flWheelPosition;
        public Vector3 flWheelVelocity;
        public Vector3 flWheelAcceleration;
        [Space]
        public Vector3 rrWheelPosition;
        public Vector3 rrWheelVelocity;
        public Vector3 rrWheelAcceleration;
        [Space]
        public Vector3 rlWheelPosition;
        public Vector3 rlWheelVelocity;
        public Vector3 rlWheelAcceleration;

        [Header("World Reference Alignments")]
        public Vector3 pivotPosition;
        public Vector3 chassisEquilibrium;
        public Vector3 frWheelEquilibrium;
        public Vector3 flWheelEquilibrium;
        public Vector3 rrWheelEquilibrium;
        public Vector3 rlWheelEquilibrium;

        [HideInInspector] public Transform chassisTransform;
        [HideInInspector] public Transform wheelTransformFR;
        [HideInInspector] public Transform wheelTransformFL;
        [HideInInspector] public Transform wheelTransformRR;
        [HideInInspector] public Transform wheelTransformRL;

        // Visual Render Fields
        [HideInInspector] public LineRenderer suspSpringFRLine; [HideInInspector] public LineRenderer suspDamperFRLine; [HideInInspector] public LineRenderer tireSpringFRLine;
        [HideInInspector] public LineRenderer suspSpringFLLine; [HideInInspector] public LineRenderer suspDamperFLLine; [HideInInspector] public LineRenderer tireSpringFLLine;
        [HideInInspector] public LineRenderer suspSpringRRLine; [HideInInspector] public LineRenderer suspDamperRRLine; [HideInInspector] public LineRenderer tireSpringRRLine;
        [HideInInspector] public LineRenderer suspSpringRLLine; [HideInInspector] public LineRenderer suspDamperRLLine; [HideInInspector] public LineRenderer tireSpringRLLine;
        [HideInInspector] public LineRenderer antirollFrontCenterLine; [HideInInspector] public LineRenderer antirollRearCenterLine;
    }

    [Header("Simulation & Input Channels:")]
    public VehicleInputController inputController;
    [Range(0.1f, 5f)] public float TimeScale = 1.0f;
    public float globalGravity = 9.81f;
    public LayerMask groundLayer;
    [Space]
    public float driverThrottleBrakePower = 12000f; // Driving force scale (N)

    [Header("Global Default Profiles:")]
    public float defaultChassisMass = 1400f;
    public float defaultInertiaPitch = 2000f;
    public float defaultInertiaRoll = 1600f;
    public float defaultInertiaYaw = 2400f;
    public float defaultCorneringStiffness = 45000f;
    public float defaultMaxSteerAngle = 30f;
    public float defaultWheelMass = 45f;
    public float defaultSuspensionK = 32000f;
    public float defaultDampingC = 2500f;
    public float defaultSuspensionLength = 1.2f;
    public float defaultTireKt = 160000f;
    public float defaultTireLength = 0.35f;

    [Header("Chain Config:")]
    public bool formChain = false;
    public int chainCount = 1;
    public float chainSpacingX = 6.0f;

    public List<FullCarSystem10DOF> fullCars = new List<FullCarSystem10DOF>();

    [Header("Prefabs & Materials:")]
    public GameObject chassisPrefab;
    public GameObject wheelPrefabFR; public GameObject wheelPrefabFL;
    public GameObject wheelPrefabRR; public GameObject wheelPrefabRL;
    public Material lineMaterial;

    [Header("Camera Properites:")]
    public CameraLean cameraLean;
    public CameraSpring cameraSpring;

    private void Start()
    {
        if (!chassisPrefab || !wheelPrefabFR || !wheelPrefabFL || !wheelPrefabRR || !wheelPrefabRL)
        {
            Debug.LogError("Prefabs missing execution assignment.");
            return;
        }

        // Try to auto-locate controller on the same GameObject if left empty
        if (inputController == null)
        {
            inputController = GetComponent<VehicleInputController>();
        }

        SetupCarSystems();
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime * TimeScale;
        if (dt <= 0f) return;

        for (int i = 0; i < fullCars.Count; i++)
        {
            FullCarSystem10DOF car = fullCars[i];
            if (!car.chassisTransform) continue;

            // Gather control input variables from the decentralized input controller script
            float inputForward = 0f;
            float inputLateral = 0f;

            if (inputController != null)
            {
                Vector2 systemInput = inputController.GetInput(i);
                inputLateral = systemInput.x;
                inputForward = systemInput.y;
            }
            else
            {
                // Fallback architecture to traditional structural inputs if no script is attached
                inputForward = Input.GetAxis("Vertical");
                inputLateral = Input.GetAxis("Horizontal");
            }

            // 1. Resolve Kinematic Transforms & Dynamic Local Hub Projections
            float yaw = car.chassisAngle.y;
            float pitch = car.chassisAngle.x;
            float roll = car.chassisAngle.z;

            Matrix4x4 headingMatrix = Matrix4x4.Rotate(Quaternion.Euler(0f, yaw * Mathf.Rad2Deg, 0f));

            Vector3 offsetFR = headingMatrix.MultiplyPoint3x4(new Vector3(car.distToRight_b1, 0f, car.distToFront_a1));
            Vector3 offsetFL = headingMatrix.MultiplyPoint3x4(new Vector3(-car.distToLeft_b2, 0f, car.distToFront_a1));
            Vector3 offsetRR = headingMatrix.MultiplyPoint3x4(new Vector3(car.distToRight_b1, 0f, -car.distToRear_a2));
            Vector3 offsetRL = headingMatrix.MultiplyPoint3x4(new Vector3(-car.distToLeft_b2, 0f, -car.distToRear_a2));

            // Extract exact world points to query individual terrain surface elevations
            float roadY_FR = SampleRoadHeightAtPoint(car.chassisPosition.x + offsetFR.x, car.chassisPosition.z + offsetFR.z);
            float roadY_FL = SampleRoadHeightAtPoint(car.chassisPosition.x + offsetFL.x, car.chassisPosition.z + offsetFL.z);
            float roadY_RR = SampleRoadHeightAtPoint(car.chassisPosition.x + offsetRR.x, car.chassisPosition.z + offsetRR.z);
            float roadY_RL = SampleRoadHeightAtPoint(car.chassisPosition.x + offsetRL.x, car.chassisPosition.z + offsetRL.z);

            // 2. Translational & Rotational State Profile Extraction
            float zBounce = car.chassisPosition.y;
            float zBounceDot = car.chassisVelocity.y;

            float thetaPitch = car.chassisAngle.x;
            float thetaPitchDot = car.chassisAngularVelocity.x;

            float phiRoll = car.chassisAngle.z;
            float phiRollDot = car.chassisAngularVelocity.z;

            float wheelFR_z = car.frWheelPosition.y; float wheelFR_zDot = car.frWheelVelocity.y;
            float wheelFL_z = car.flWheelPosition.y; float wheelFL_zDot = car.flWheelVelocity.y;
            float wheelRR_z = car.rrWheelPosition.y; float wheelRR_zDot = car.rrWheelVelocity.y;
            float wheelRL_z = car.rlWheelPosition.y; float wheelRL_zDot = car.rlWheelVelocity.y;
            // 3. Compute High-Fidelity Suspension Deflections
            // Subtracting the suspension length ensures the spring forces equilibrium at the correct physical distance
            float deltaPosFR = (zBounce - wheelFR_z - (car.distToFront_a1 * thetaPitch) + (car.distToRight_b1 * phiRoll)) - car.suspensionLengthFR;
            float deltaPosFL = (zBounce - wheelFL_z - (car.distToFront_a1 * thetaPitch) - (car.distToLeft_b2 * phiRoll)) - car.suspensionLengthFL;
            float deltaPosRR = (zBounce - wheelRR_z + (car.distToRear_a2 * thetaPitch) + (car.distToRight_b1 * phiRoll)) - car.suspensionLengthRR;
            float deltaPosRL = (zBounce - wheelRL_z + (car.distToRear_a2 * thetaPitch) - (car.distToLeft_b2 * phiRoll)) - car.suspensionLengthRL;

            // Velocity deltas remain identical (the derivative of the resting length is 0)
            float deltaVelFR = zBounceDot - wheelFR_zDot - (car.distToFront_a1 * thetaPitchDot) + (car.distToRight_b1 * phiRollDot);
            float deltaVelFL = zBounceDot - wheelFL_zDot - (car.distToFront_a1 * thetaPitchDot) - (car.distToLeft_b2 * phiRollDot);
            float deltaVelRR = zBounceDot - wheelRR_zDot + (car.distToRear_a2 * thetaPitchDot) + (car.distToRight_b1 * phiRollDot);
            float deltaVelRL = zBounceDot - wheelRL_zDot + (car.distToRear_a2 * thetaPitchDot) - (car.distToLeft_b2 * phiRollDot);

            // Suspension forces
            float F_FR = (car.suspensionConstantK_FR * deltaPosFR) + (car.dampingCoefficientC_FR * deltaVelFR);
            float F_FL = (car.suspensionConstantK_FL * deltaPosFL) + (car.dampingCoefficientC_FL * deltaVelFL);
            float F_RR = (car.suspensionConstantK_RR * deltaPosRR) + (car.dampingCoefficientC_RR * deltaVelRR);
            float F_RL = (car.suspensionConstantK_RL * deltaPosRL) + (car.dampingCoefficientC_RL * deltaVelRL);

            // Tire Ground Contact Calculations
            // Subtracting the tire length ensures the upward normal force triggers when the tire *tread* hits the road, not the hub
            float tireDeltaFR = (wheelFR_z - roadY_FR) - car.tireLengthFR;
            float tireDeltaFL = (wheelFL_z - roadY_FL) - car.tireLengthFL;
            float tireDeltaRR = (wheelRR_z - roadY_RR) - car.tireLengthRR;
            float tireDeltaRL = (wheelRL_z - roadY_RL) - car.tireLengthRL;

            float forceTireFR = (tireDeltaFR < 0f) ? car.tireConstantKt_FR * tireDeltaFR : 0f;
            float forceTireFL = (tireDeltaFL < 0f) ? car.tireConstantKt_FL * tireDeltaFL : 0f;
            float forceTireRR = (tireDeltaRR < 0f) ? car.tireConstantKt_RR * tireDeltaRR : 0f;
            float forceTireRL = (tireDeltaRL < 0f) ? car.tireConstantKt_RL * tireDeltaRL : 0f;

            // Antiroll Bar Mechanics
            float M_R_Front = 0f; float M_R_Rear = 0f;
            float trackWidth = car.distToRight_b1 + car.distToLeft_b2;
            if (car.useAdvancedModel)
            {
                M_R_Front = -car.antirollStiffnessKR_Front * (phiRoll - ((wheelFR_z - wheelFL_z) / trackWidth));
                if (!car.useFrontAntirollBarOnly) M_R_Rear = -car.antirollStiffnessKR_Rear * (phiRoll - ((wheelRR_z - wheelRL_z) / trackWidth));
            }
            else
            {
                M_R_Front = -car.antirollStiffnessKR_Front * phiRoll;
                if (!car.useFrontAntirollBarOnly) M_R_Rear = -car.antirollStiffnessKR_Rear * phiRoll;
            }

            // 4. Resolve 10-DOF Planar Handling Equations of Motion
            // Transform world velocities to local chassis frame coordinates (u = forward, v = lateral)
            Vector3 localVel = Quaternion.Inverse(Quaternion.Euler(0f, yaw * Mathf.Rad2Deg, 0f)) * car.chassisVelocity;
            float u = localVel.z; // Longitudinal speed
            float v = localVel.x; // Lateral speed
            float rYawRate = car.chassisAngularVelocity.y;

            float steerAngle = inputLateral * car.maxSteeringAngle * Mathf.Deg2Rad;
            float F_long_drive = inputForward * driverThrottleBrakePower;

            float F_lateral_F = 0f;
            float F_lateral_R = 0f;

            // Avoid low-speed numerical singularities via slip velocity damping threshold
            if (Mathf.Abs(u) > 0.5f)
            {
                float slipAngleF = Mathf.Atan2(v + car.distToFront_a1 * rYawRate, Mathf.Abs(u)) - steerAngle * Mathf.Sign(u);
                float slipAngleR = Mathf.Atan2(v - car.distToRear_a2 * rYawRate, Mathf.Abs(u));

                F_lateral_F = -car.corneringStiffnessFront * slipAngleF;
                F_lateral_R = -car.corneringStiffnessRear * slipAngleR;
            }
            else
            {
                // Low-speed dampening fallback to prevent infinite slip values at rest
                F_lateral_F = -v * 2000f;
                F_lateral_R = -v * 2000f;
            }

            // Transform tire handling profiles into body-fixed local axis allocations
            float F_localX = F_long_drive - F_lateral_F * Mathf.Sin(steerAngle);
            float F_localY = F_lateral_R + F_lateral_F * Mathf.Cos(steerAngle);
            float T_localYaw = (F_lateral_F * Mathf.Cos(steerAngle)) * car.distToFront_a1 - F_lateral_R * car.distToRear_a2 + F_long_drive * Mathf.Sin(steerAngle);

            // Convert accumulated horizontal frame forces into world coordinate space
            Vector3 worldPlanarForce = Quaternion.Euler(0f, yaw * Mathf.Rad2Deg, 0f) * new Vector3(F_localY, 0f, F_localX);

            // 5. Matrix Accel Formulations & Vertical Dynamics Isolation
            float accBounce = (-F_FR - F_FL - F_RR - F_RL) / car.chassisMass - globalGravity;
            float accPitch = (car.distToFront_a1 * (F_FR + F_FL) - car.distToRear_a2 * (F_RR + F_RL)) / car.chassisInertiaPitch;
            float accRoll = (-car.distToRight_b1 * (F_FR + F_RR) + car.distToLeft_b2 * (F_FL + F_RL) + M_R_Front + M_R_Rear) / car.chassisInertiaRoll;
            float accYaw = T_localYaw / car.chassisInertiaYaw;

            float accWheelFR = (F_FR - forceTireFR) / car.wheelMassFR - globalGravity;
            float accWheelFL = (F_FL - forceTireFL) / car.wheelMassFL - globalGravity;
            float accWheelRR = (F_RR - forceTireRR) / car.wheelMassRR - globalGravity;
            float accWheelRL = (F_RL - forceTireRL) / car.wheelMassRL - globalGravity;

            // 6. Semi-Implicit Euler Numerical Integration Loop
            // Planar and vertical translation updates
            car.chassisAcceleration = new Vector3(worldPlanarForce.x / car.chassisMass, accBounce, worldPlanarForce.z / car.chassisMass);
            car.chassisVelocity += car.chassisAcceleration * dt;
            car.chassisPosition += car.chassisVelocity * dt;

            // Rotational orientation updates (Pitch, Yaw, Roll)
            car.chassisAngularAcceleration = new Vector3(accPitch, accYaw, accRoll);
            car.chassisAngularVelocity += car.chassisAngularAcceleration * dt;
            car.chassisAngle += car.chassisAngularVelocity * dt;

            // Independent wheel unsprung vertical hops execution
            car.frWheelAcceleration.y = accWheelFR; car.frWheelVelocity.y += accWheelFR * dt; car.frWheelPosition.y += car.frWheelVelocity.y * dt;
            car.flWheelAcceleration.y = accWheelFL; car.flWheelVelocity.y += accWheelFL * dt; car.flWheelPosition.y += car.flWheelVelocity.y * dt;
            car.rrWheelAcceleration.y = accWheelRR; car.rrWheelVelocity.y += accWheelRR * dt; car.rrWheelPosition.y += car.rrWheelVelocity.y * dt;
            car.rlWheelAcceleration.y = accWheelRL; car.rlWheelVelocity.y += accWheelRL * dt; car.rlWheelPosition.y += car.rlWheelVelocity.y * dt;

            // 7. Spatial Transform Alignments
            car.chassisTransform.position = car.chassisPosition;
            car.chassisTransform.rotation = Quaternion.Euler(car.chassisAngle.x * Mathf.Rad2Deg, car.chassisAngle.y * Mathf.Rad2Deg, car.chassisAngle.z * Mathf.Rad2Deg);

            // Update Hub world tracking (planar position locks to chassis grid offset, vertical locks to wheel hop state)
            car.wheelTransformFR.position = new Vector3(car.chassisPosition.x + offsetFR.x, car.frWheelPosition.y, car.chassisPosition.z + offsetFR.z);
            car.wheelTransformFL.position = new Vector3(car.chassisPosition.x + offsetFL.x, car.flWheelPosition.y, car.chassisPosition.z + offsetFL.z);
            car.wheelTransformRR.position = new Vector3(car.chassisPosition.x + offsetRR.x, car.rrWheelPosition.y, car.chassisPosition.z + offsetRR.z);
            car.wheelTransformRL.position = new Vector3(car.chassisPosition.x + offsetRL.x, car.rlWheelPosition.y, car.chassisPosition.z + offsetRL.z);

            // Local rotation for visuals (steering applied to front wheel gameobjects)
            car.wheelTransformFR.localRotation = Quaternion.Euler(0f, steerAngle * Mathf.Rad2Deg, 0f);
            car.wheelTransformFL.localRotation = Quaternion.Euler(0f, steerAngle * Mathf.Rad2Deg, 0f);

            // 8. Vector Line Link Generation Rendering updates
            UpdateFullCarVisuals(car, roadY_FR, roadY_FL, roadY_RR, roadY_RL);

            fullCars[i] = car;
        }
    }

    private void SetupCarSystems()
    {
        if (formChain)
        {
            fullCars.Clear();
            for (int i = 0; i < chainCount; i++)
            {
                FullCarSystem10DOF newCar = new FullCarSystem10DOF();
                newCar.chassisMass = defaultChassisMass;
                newCar.chassisInertiaPitch = defaultInertiaPitch;
                newCar.chassisInertiaRoll = defaultInertiaRoll;
                newCar.chassisInertiaYaw = defaultInertiaYaw;
                newCar.corneringStiffnessFront = defaultCorneringStiffness;
                newCar.corneringStiffnessRear = defaultCorneringStiffness;
                newCar.maxSteeringAngle = defaultMaxSteerAngle;

                newCar.wheelMassFR = defaultWheelMass; newCar.wheelMassFL = defaultWheelMass;
                newCar.wheelMassRR = defaultWheelMass; newCar.wheelMassRL = defaultWheelMass;
                newCar.distToFront_a1 = 1.4f; newCar.distToRear_a2 = 1.4f;
                newCar.distToRight_b1 = 0.85f; newCar.distToLeft_b2 = 0.85f;

                newCar.suspensionLengthFR = defaultSuspensionLength; newCar.suspensionConstantK_FR = defaultSuspensionK; newCar.dampingCoefficientC_FR = defaultDampingC;
                newCar.suspensionLengthFL = defaultSuspensionLength; newCar.suspensionConstantK_FL = defaultSuspensionK; newCar.dampingCoefficientC_FL = defaultDampingC;
                newCar.suspensionLengthRR = defaultSuspensionLength; newCar.suspensionConstantK_RR = defaultSuspensionK; newCar.dampingCoefficientC_RR = defaultDampingC;
                newCar.suspensionLengthRL = defaultSuspensionLength; newCar.suspensionConstantK_RL = defaultSuspensionK; newCar.dampingCoefficientC_RL = defaultDampingC;

                newCar.tireLengthFR = defaultTireLength; newCar.tireConstantKt_FR = defaultTireKt;
                newCar.tireLengthFL = defaultTireLength; newCar.tireConstantKt_FL = defaultTireKt;
                newCar.tireLengthRR = defaultTireLength; newCar.tireConstantKt_RR = defaultTireKt;
                newCar.tireLengthRL = defaultTireLength; newCar.tireConstantKt_RL = defaultTireKt;

                newCar.antirollStiffnessKR_Front = 15000f;
                newCar.antirollStiffnessKR_Rear = 12000f;
                newCar.antirollBarHeightOffset = 0.3f;

                newCar.pivotPosition = new Vector3(i * chainSpacingX, 0f, 0f);
                // Position vectors store true absolute values down the processing chain loop
                newCar.chassisPosition = new Vector3(newCar.pivotPosition.x, defaultTireLength + defaultSuspensionLength, newCar.pivotPosition.z);

                newCar.frWheelPosition = new Vector3(0f, defaultTireLength, 0f);
                newCar.flWheelPosition = new Vector3(0f, defaultTireLength, 0f);
                newCar.rrWheelPosition = new Vector3(0f, defaultTireLength, 0f);
                newCar.rlWheelPosition = new Vector3(0f, defaultTireLength, 0f);

                fullCars.Add(newCar);
            }
        }

        for (int i = 0; i < fullCars.Count; i++)
        {
            FullCarSystem10DOF current = fullCars[i];

            // Structural Validation Fallbacks
            if (current.chassisInertiaYaw <= 0) current.chassisInertiaYaw = defaultInertiaYaw;
            if (current.corneringStiffnessFront <= 0) current.corneringStiffnessFront = defaultCorneringStiffness;
            if (current.corneringStiffnessRear <= 0) current.corneringStiffnessRear = defaultCorneringStiffness;
            if (current.maxSteeringAngle <= 0) current.maxSteeringAngle = defaultMaxSteerAngle;

            GameObject chassisObj = Instantiate(chassisPrefab, transform);
            chassisObj.name = $"10-DOF Chassis {i + 1}";
            current.chassisTransform = chassisObj.transform;

            current.wheelTransformFR = Instantiate(wheelPrefabFR, transform).transform; current.wheelTransformFR.name = $"FR_Wheel_Node {i + 1}";
            current.wheelTransformFL = Instantiate(wheelPrefabFL, transform).transform; current.wheelTransformFL.name = $"FL_Wheel_Node {i + 1}";
            current.wheelTransformRR = Instantiate(wheelPrefabRR, transform).transform; current.wheelTransformRR.name = $"RR_Wheel_Node {i + 1}";
            current.wheelTransformRL = Instantiate(wheelPrefabRL, transform).transform; current.wheelTransformRL.name = $"RL_Wheel_Node {i + 1}";

            // Instantiation of line rendering connections
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

            fullCars[i] = current;
        }
    }

    private void UpdateFullCarVisuals(FullCarSystem10DOF car, float roadFR, float roadFL, float roadRR, float roadRL)
    {
        Vector3 nodeFR_World = car.chassisTransform.TransformPoint(new Vector3(car.distToRight_b1, 0f, car.distToFront_a1));
        Vector3 nodeFL_World = car.chassisTransform.TransformPoint(new Vector3(-car.distToLeft_b2, 0f, car.distToFront_a1));
        Vector3 nodeRR_World = car.chassisTransform.TransformPoint(new Vector3(car.distToRight_b1, 0f, -car.distToRear_a2));
        Vector3 nodeRL_World = car.chassisTransform.TransformPoint(new Vector3(-car.distToLeft_b2, 0f, -car.distToRear_a2));

        Vector3 hubFR = car.wheelTransformFR.position; Vector3 hubFL = car.wheelTransformFL.position;
        Vector3 hubRR = car.wheelTransformRR.position; Vector3 hubRL = car.wheelTransformRL.position;

        Vector3 contactFR = new Vector3(hubFR.x, roadFR, hubFR.z);
        Vector3 contactFL = new Vector3(hubFL.x, roadFL, hubFL.z);
        Vector3 contactRR = new Vector3(hubRR.x, roadRR, hubRR.z);
        Vector3 contactRL = new Vector3(hubRL.x, roadRL, hubRL.z);

        Vector3 visualOffset = car.chassisTransform.right * 0.1f;

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
        else
        {
            if (car.antirollRearCenterLine.enabled) car.antirollRearCenterLine.enabled = false;
        }
    }

    private float SampleRoadHeightAtPoint(float worldX, float worldZ)
    {
        Vector3 rayOriginWorld = new Vector3(worldX, 50f, worldZ);
        if (Physics.Raycast(rayOriginWorld, Vector3.down, out RaycastHit hit, 100f, groundLayer))
        {
            return hit.point.y;
        }
        return 0f;
    }

    private LineRenderer CreateLineVisual(Transform parent, string name, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        LineRenderer line = obj.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = 0.05f; line.endWidth = 0.05f;
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
        int segments = 45;
        float radius = 0.1f;
        float frequency = 8f;
        line.positionCount = segments;
        for (int j = 0; j < segments; j++)
        {
            float t = (float)j / (segments - 1);
            Vector3 point = Vector3.Lerp(start, end, t);
            if (t > 0.05f && t < 0.95f)
            {
                float wave = Mathf.Sin(t * Mathf.PI * 2f * frequency);
                Vector3 perpendicular = Vector3.Cross((end - start).normalized, Vector3.up).normalized;
                if (perpendicular.sqrMagnitude < 0.1f) perpendicular = Vector3.right;
                point += perpendicular * wave * radius;
            }
            line.SetPosition(j, point);
        }
    }
}
