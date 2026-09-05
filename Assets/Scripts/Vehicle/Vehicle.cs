using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    [System.Serializable]
    public struct FullCarSystem
    {
        [Header("Road Properties")]
        public float slopeAngle;

        [Header("Steering Dynamics")]
        public float steerAngleFR;              // rad
        public float steerAngleFL;

        public float steerAngularVelocityFR;    // rad/s
        public float steerAngularVelocityFL;

      

        [Header("Input States")]
        public float throttleInput; // -1 to 1
        public float steeringInput; // -1 to 1 (left to right)
        public float maxSteerAngle; // degrees
        public float engineTorque; // Nm

        [Header("Chassis Mass Profiles:")]
        public float chassisMass;
        public float chassisInertiaPitch;
        public float chassisInertiaRoll;
        public float chassisInertiaYaw;
        [Space]

        [Header("Wheel Mass Profiles:")]
        public float wheelRotationalInertia;
        public float wheelMassFR;
        public float wheelMassFL;
        public float wheelMassRR;
        public float wheelMassRL;
        [Header("--------------------------------------------------------------------------")]


        [Header("Chassis Geometry:")]
        public float distToFront_a1;
        public float distToRear_a2;
        [Space]
        public float distToRight_b1;
        public float distToLeft_b2;

        [Header("Tire Geometry (For Tire Load  [x^(2*n) / a^(2*n) + y^(2*n) / b^(2*n) = 1]):")]
        public float superEllipseFactor_n;
        [Space]
        public float tireprintHalfLength_a;
        public float tireprintHalfWidth_b;
        [Header("--------------------------------------------------------------------------")]


        [Header("Front-Right Suspension Profile:")]
        public float suspensionLengthFR;
        [Space]
        public float suspensionConstantK_FR;
        public float dampingCoefficientC_FR;

        [Header("Front-Left Suspension Profile:")]
        public float suspensionLengthFL;
        [Space]
        public float suspensionConstantK_FL;
        public float dampingCoefficientC_FL;

        [Header("Rear-Right Suspension Profile:")]
        public float suspensionLengthRR;
        [Space]
        public float suspensionConstantK_RR;
        public float dampingCoefficientC_RR;

        [Header("Rear-Left Suspension Profile:")]
        public float suspensionLengthRL;
        [Space]
        public float suspensionConstantK_RL;
        public float dampingCoefficientC_RL;
        [Header("--------------------------------------------------------------------------")]

        [Header("Front-Right Tire Profile:")]
        public float freeRadiusR0_FR;
        [Space]
        public float tireConstantKt_FR;
        public float rollingFrictionCoefficientU_FR;

        [Header("Front-Left Tire Profile:")]
        public float freeRadiusR0_FL;
        [Space]
        public float tireConstantKt_FL;
        public float rollingFrictionCoefficientU_FL;

        [Header("Rear-Right Tire Profile:")]
        public float freeRadiusR0_RR;
        [Space]
        public float tireConstantKt_RR;
        public float rollingFrictionCoefficientU_RR;

        [Header("Rear-Left Tire Profile:")]
        public float freeRadiusR0_RL;
        [Space]
        public float tireConstantKt_RL;
        public float rollingFrictionCoefficientU_RL;
        [Header("--------------------------------------------------------------------------")]


        [Header("Antiroll Bar Profiles:")]
        public bool useAdvancedModel;
        public bool useFrontAntirollBarOnly;
        [Space]
        public float antirollStiffnessKR_Front;
        public float antirollStiffnessKR_Rear;
        [Space]
        public float antirollBarHeightOffset;
        [Header("--------------------------------------------------------------------------")]


        [Header("Chassis Properties: ")]
        public Vector3 chassisPosition;
        public Vector3 chassisVelocity;
        public Vector3 chassisAcceleration;
        [Space(10)]
        public Vector3 chassisAngle;
        public Vector3 chassisAngularVelocity;
        public Vector3 chassisAngularAcceleration;
        [Space(20)]
        [Header("--------------------------------------------------------------------------")]


        [Header("Front-Right Wheel Properties: ")]
        public float suspensionForceFR;
        [Space(10)]
        public Vector3 frWheelPosition;
        public Vector3 frWheelVelocity;
        public Vector3 frWheelAcceleration;
        [Space(10)]
        public Vector3 frWheelAngle;
        public Vector3 frWheelAngularVelocity;
        public Vector3 frWheelAngularAcceleration;
        [Space(20)]

        [Header("Front-Left Wheel Properties: ")]
        public float suspensionForceFL;
        [Space(10)]
        public Vector3 flWheelPosition;
        public Vector3 flWheelVelocity;
        public Vector3 flWheelAcceleration;
        [Space(10)]
        public Vector3 flWheelAngle;
        public Vector3 flWheelAngularVelocity;
        public Vector3 flWheelAngularAcceleration;
        [Space(20)]

        [Header("Rear-Right Wheel Properties: ")]
        public float suspensionForceRR;
        [Space(10)]
        public Vector3 rrWheelPosition;
        public Vector3 rrWheelVelocity;
        public Vector3 rrWheelAcceleration;
        [Space(10)]
        public Vector3 rrWheelAngle;
        public Vector3 rrWheelAngularVelocity;
        public Vector3 rrWheelAngularAcceleration;
        [Space(20)]

        [Header("Rear-Left Wheel Properties: ")]
        public float suspensionForceRL;
        [Space(10)]
        public Vector3 rlWheelPosition;
        public Vector3 rlWheelVelocity;
        public Vector3 rlWheelAcceleration;
        [Space(10)]
        public Vector3 rlWheelAngle;
        public Vector3 rlWheelAngularVelocity;
        public Vector3 rlWheelAngularAcceleration;

        [Header("Antiroll Bar Properties: ")]
        public float antirollMomentFront;
        public float antirollMomentRear;
        [Header("--------------------------------------------------------------------------")]


        [Header("Front-Right Tire Properties: ")]
        public float frEffectiveTireRadius;
        public float frRollingResistanceForce;
        [Space(10)]
        public float frSlipAngle;
        public float frSlipRatio;
        public float frLongitudinalFrictionCoefficient;
        [Space(10)]
        public float frTireNormalForce;
        public float frTireLongitudinalForce;
        public float frTireLateralForce;
        [Space(10)]
        public float frRollMoment;
        public float frPitchMoment;
        public float frYawMoment;
        [Space(20)]

        [Header("Front-Left Tire Properties: ")]
        public float flEffectiveTireRadius;
        public float flRollingResistanceForce;
        [Space(10)]
        public float flSlipAngle;
        public float flSlipRatio;
        public float flLongitudinalFrictionCoefficient;
        [Space(10)]
        public float flTireNormalForce;
        public float flTireLongitudinalForce;
        public float flTireLateralForce;
        [Space(10)]
        public float flRollMoment;
        public float flPitchMoment;
        public float flYawMoment;
        [Space(20)]

        [Header("Rear-Right Tire Properties: ")]
        public float rrEffectiveTireRadius;
        public float rrRollingResistanceForce;
        [Space(10)]
        public float rrSlipAngle;
        public float rrSlipRatio;
        public float rrLongitudinalFrictionCoefficient;
        [Space(10)]
        public float rrTireNormalForce;
        public float rrTireLongitudinalForce;
        public float rrTireLateralForce;
        [Space(10)]
        public float rrRollMoment;
        public float rrPitchMoment;
        public float rrYawMoment;
        [Space(20)]

        [Header("Rear-Left Tire Properties: ")]
        public float rlEffectiveTireRadius;
        public float rlRollingResistanceForce;
        [Space(10)]
        public float rlSlipAngle;
        public float rlSlipRatio;
        public float rlLongitudinalFrictionCoefficient;
        [Space(10)]
        public float rlTireNormalForce;
        public float rlTireLongitudinalForce;
        public float rlTireLateralForce;
        [Space(10)]
        public float rlRollMoment;
        public float rlPitchMoment;
        public float rlYawMoment;

        [Header("Longitudinal Tire Model")]
        public float peakLongitudinalFriction;
        public float slipStiffness;

        [Header("Lateral Tire Model (Jazar)")]
        public float corneringStiffnessC_alpha; // C_a in the text
        public float peakLateralFriction;       // mu_y in the text
        public float pneumaticTrail_a_x;        // a_xa in the text
        public float lateralShift_a_y;          // a_ya in the text
        [Header("--------------------------------------------------------------------------")]


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
    }

    private struct WheelDynamics
    {
        public float omega;
        public float effectiveRadius;

        public float engineTorque;
        public float gravityTorque;

        public float tireReactionTorque;
        public float rollingResistanceTorque;
        public float bearingDragTorque;

        public float totalTorque;
        public float angularAcceleration;
    }

    [Header("--------------------------------------------------------------------------")]

    [Header("Simulation Settings:")]
    [Range(0.1f, 5f)] public float TimeScale = 1.0f;
    public float globalGravity = 9.81f;
    [Space]
    public LayerMask groundLayer;

    [Header("--------------------------------------------------------------------------")]

    [Header("Global Default Properties:")]
    public float defaultMaxSteeringAngle = 35f;
    public float defaultEngineTorque = 500f;
    [Space]
    public float defaultChassisMass = 1400f;
    public float defaultChassisInertiaPitch = 2000f;
    public float defaultChassisInertiaRoll = 1600f;
    public float defaultChassisInertiaYaw = 2400f;
    [Space]
    public float defaultWheelRotationalInertia = 1.8f;
    public float defaultWheelMass = 45f;
    [Space]
    public float defaultDistA1 = 1.3f;
    public float defaultDistA2 = 1.5f;
    public float defaultDistB1 = 0.8f;
    public float defaultDistB2 = 0.8f;
    [Space]
    public float defaultSuspensionLength = 1.5f;
    public float defaultSuspensionK = 30000f;
    public float defaultDampingC = 2200f;
    [Space]
    public float defaultTireRadius = 0.4f;
    public float defaultTireKt = 150000f;
    public float defaultTireU = 0.012f;
    [Space]
    public float defaultAntirollKR = 12000f;
    public float defaultAntirollOffset = 0.4f;
    [Space]
    public float defaultPeakLongitudinalFriction = 1.0f;
    public float defaultSlipStiffness = 20f;
    public float minimumForwardVelocity = 0.5f;

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

        for (int i = 0; i < fullCars.Count; i++)
        {
            FullCarSystem car = fullCars[i];

            //User Input:
            ReadDriverInputs(ref car);

            //Wheel Displacement From Ground:
            var road = SampleRoad(ref car);
            car.slopeAngle = road.slopeAngle;

            //Normal Force Data:
            UpdateVerticalTireState(ref car, road.roadY_FR, road.roadY_FL, road.roadY_RR, road.roadY_RL);

            //Vertical Force Data:
            float vehicleForwardVelocity = UpdateLongitudinalTireForces(ref car);

            //Wheel Vertical Rotation Data:
            UpdateWheelRotationalDynamics(ref car, road.roadY_FR, road.roadY_FL, road.roadY_RR, road.roadY_RL, vehicleForwardVelocity, dt);

            //Horizontal Force Data:
            UpdateLateralTireForces(ref car);

            //Suspension Force Data:
            UpdateSuspensionForces(ref car);

            //Derive Equations of Motion For The System:
            UpdateEquationsOfMotion(ref car);

            //Apply Semi-Implicit Euler Integration:
            IntegrateDynamics(ref car, dt);

            //Update Vehicle Positions:
            UpdateTransforms(ref car, dt);

            //Update Line Renders:
            UpdateFullCarVisuals(ref car, road.roadY_FR, road.roadY_FL, road.roadY_RR, road.roadY_RL);

            //Store Updated Car Data:
            fullCars[i] = car;
        }
    }
    private void ReadDriverInputs(ref FullCarSystem car)
    {
        // Read raw player input
        car.throttleInput = Input.GetAxis("Vertical");
        car.steeringInput = Input.GetAxis("Horizontal");

        // Convert steering command to radians
        float steerAngle = car.steeringInput * car.maxSteerAngle * Mathf.Deg2Rad;

        car.steerAngleFR = steerAngle;
        car.steerAngleFL = steerAngle;
    }

    private (float roadY_FR, float roadY_FL, float roadY_RR, float roadY_RL, float slopeAngle) SampleRoad(ref FullCarSystem car)
    {
        float frontZ = car.pivotPosition.z + car.distToFront_a1;
        float rearZ = car.pivotPosition.z - car.distToRear_a2;

        float rightX = car.pivotPosition.x + car.distToRight_b1;
        float leftX = car.pivotPosition.x - car.distToLeft_b2;

        float roadY_FR = SampleRoadHeightAtPoint(rightX, frontZ);
        float roadY_FL = SampleRoadHeightAtPoint(leftX, frontZ);
        float roadY_RR = SampleRoadHeightAtPoint(rightX, rearZ);
        float roadY_RL = SampleRoadHeightAtPoint(leftX, rearZ);

        float frontHeight = (roadY_FR + roadY_FL) * 0.5f;
        float rearHeight = (roadY_RR + roadY_RL) * 0.5f;

        float wheelbase = car.distToFront_a1 + car.distToRear_a2;

        float slopeAngle = Mathf.Atan2(frontHeight - rearHeight, wheelbase);

        return (roadY_FR, roadY_FL, roadY_RR, roadY_RL, slopeAngle);
    }

    private void UpdateVerticalTireState(ref FullCarSystem car, float roadY_FR, float roadY_FL, float roadY_RR, float roadY_RL)
    {
        //Calculate Loaded Radius: (distance from wheel center to ground)
        float loadedRadiusFR = (car.freeRadiusR0_FR + car.frWheelPosition.y) - roadY_FR;
        float loadedRadiusFL = (car.freeRadiusR0_FL + car.flWheelPosition.y) - roadY_FL;
        float loadedRadiusRR = (car.freeRadiusR0_RR + car.rrWheelPosition.y) - roadY_RR;
        float loadedRadiusRL = (car.freeRadiusR0_RL + car.rlWheelPosition.y) - roadY_RL;

        //Calculate Tire Deflection: (Compression amount)
        float deflectionFR = Mathf.Max(0f, car.freeRadiusR0_FR - loadedRadiusFR);
        float deflectionFL = Mathf.Max(0f, car.freeRadiusR0_FL - loadedRadiusFL);
        float deflectionRR = Mathf.Max(0f, car.freeRadiusR0_RR - loadedRadiusRR);
        float deflectionRL = Mathf.Max(0f, car.freeRadiusR0_RL - loadedRadiusRL);

        //Vertical Normal Force: (Fz = Kt * deflection)
        car.frTireNormalForce = car.tireConstantKt_FR * deflectionFR;
        car.flTireNormalForce = car.tireConstantKt_FL * deflectionFL;
        car.rrTireNormalForce = car.tireConstantKt_RR * deflectionRR;
        car.rlTireNormalForce = car.tireConstantKt_RL * deflectionRL;

        //Calculate Effective Rolling Radius: (Re = R0 - 1/3*deflection)
        car.frEffectiveTireRadius = car.freeRadiusR0_FR - (deflectionFR / 3f);
        car.flEffectiveTireRadius = car.freeRadiusR0_FL - (deflectionFL / 3f);
        car.rrEffectiveTireRadius = car.freeRadiusR0_RR - (deflectionRR / 3f);
        car.rlEffectiveTireRadius = car.freeRadiusR0_RL - (deflectionRL / 3f);

        //Rolling Resistance: (Fr = u * Fz)
        car.frRollingResistanceForce = car.rollingFrictionCoefficientU_FR * car.frTireNormalForce;
        car.flRollingResistanceForce = car.rollingFrictionCoefficientU_FL * car.flTireNormalForce;
        car.rrRollingResistanceForce = car.rollingFrictionCoefficientU_RR * car.rrTireNormalForce;
        car.rlRollingResistanceForce = car.rollingFrictionCoefficientU_RL * car.rlTireNormalForce;

    }

    private float UpdateLongitudinalTireForces(ref FullCarSystem car)
    {
        //Vehicle forward velocity
        float vehicleForwardVelocity = Vector3.Dot(car.chassisVelocity, car.chassisTransform.forward);
        float Vx = Mathf.Max(Mathf.Abs(vehicleForwardVelocity), minimumForwardVelocity);

        //Wheel Angular Velocities:
        float omegaFR = car.frWheelAngularVelocity.z;
        float omegaFL = car.flWheelAngularVelocity.z;
        float omegaRR = car.rrWheelAngularVelocity.z;
        float omegaRL = car.rlWheelAngularVelocity.z;

        //Slip Ratio
        car.frSlipRatio = (car.frEffectiveTireRadius * omegaFR - vehicleForwardVelocity) / Vx;
        car.flSlipRatio = (car.flEffectiveTireRadius * omegaFL - vehicleForwardVelocity) / Vx;
        car.rrSlipRatio = (car.rrEffectiveTireRadius * omegaRR - vehicleForwardVelocity) / Vx;
        car.rlSlipRatio = (car.rlEffectiveTireRadius * omegaRL - vehicleForwardVelocity) / Vx;

        //Longitudinal Friction Coefficient:
        car.frLongitudinalFrictionCoefficient = car.peakLongitudinalFriction * (2f / (1f + Mathf.Exp(-2f * car.slipStiffness * car.frSlipRatio)) - 1f);
        car.flLongitudinalFrictionCoefficient = car.peakLongitudinalFriction * (2f / (1f + Mathf.Exp(-2f * car.slipStiffness * car.flSlipRatio)) - 1f);
        car.rrLongitudinalFrictionCoefficient = car.peakLongitudinalFriction * (2f / (1f + Mathf.Exp(-2f * car.slipStiffness * car.rrSlipRatio)) - 1f);
        car.rlLongitudinalFrictionCoefficient = car.peakLongitudinalFriction * (2f / (1f + Mathf.Exp(-2f * car.slipStiffness * car.rlSlipRatio)) - 1f);

        // Determine the direction of travel (use a small deadzone to prevent jitter)
        float velocitySign = Mathf.Abs(vehicleForwardVelocity) > 0.05f ? Mathf.Sign(vehicleForwardVelocity) : 0f;

        //Longitudinal Tire Force
        car.frTireLongitudinalForce = (car.frLongitudinalFrictionCoefficient * car.frTireNormalForce) - (velocitySign * car.frRollingResistanceForce);
        car.flTireLongitudinalForce = (car.flLongitudinalFrictionCoefficient * car.flTireNormalForce) - (velocitySign * car.flRollingResistanceForce);
        car.rrTireLongitudinalForce = (car.rrLongitudinalFrictionCoefficient * car.rrTireNormalForce) - (velocitySign * car.rrRollingResistanceForce);
        car.rlTireLongitudinalForce = (car.rlLongitudinalFrictionCoefficient * car.rlTireNormalForce) - (velocitySign * car.rlRollingResistanceForce);

        bool parked =
        Mathf.Abs(vehicleForwardVelocity) < 0.05f &&
        Mathf.Abs(car.frWheelAngularVelocity.z) < 0.05f &&
        Mathf.Abs(car.flWheelAngularVelocity.z) < 0.05f &&
        Mathf.Abs(car.rrWheelAngularVelocity.z) < 0.05f &&
        Mathf.Abs(car.rlWheelAngularVelocity.z) < 0.05f &&
        Mathf.Abs(car.throttleInput) < 0.01f;

        if (parked)
        {
            car.frSlipRatio = 0f;
            car.flSlipRatio = 0f;
            car.rrSlipRatio = 0f;
            car.rlSlipRatio = 0f;

            car.frLongitudinalFrictionCoefficient = 0f;
            car.flLongitudinalFrictionCoefficient = 0f;
            car.rrLongitudinalFrictionCoefficient = 0f;
            car.rlLongitudinalFrictionCoefficient = 0f;

            car.frTireLongitudinalForce = 0f;
            car.flTireLongitudinalForce = 0f;
            car.rrTireLongitudinalForce = 0f;
            car.rlTireLongitudinalForce = 0f;

            return vehicleForwardVelocity;
        }

        return vehicleForwardVelocity;
    }

    void UpdateWheelRotationalDynamics(ref FullCarSystem car, float roadY_FR, float roadY_FL, float roadY_RR, float roadY_RL, float vehicleForwardVelocity, float dt)
    {
        // Engine Torque:
        float requestedEngineTorque = car.engineTorque * car.throttleInput;
        float rearBias = 1.0f;
        float frontBias = 0.0f;

        float engineTorqueFR = requestedEngineTorque * frontBias * 0.5f;
        float engineTorqueFL = requestedEngineTorque * frontBias * 0.5f;
        float engineTorqueRR = requestedEngineTorque * rearBias * 0.5f;
        float engineTorqueRL = requestedEngineTorque * rearBias * 0.5f;

        // Rolling Resistance Torque (Opposes wheel rotation):
        // Use a small deadzone to prevent infinite micro-oscillations when stopped
        float signOmegaFR = Mathf.Abs(car.frWheelAngularVelocity.z) > 0.01f ? Mathf.Sign(car.frWheelAngularVelocity.z) : 0f;
        float signOmegaFL = Mathf.Abs(car.flWheelAngularVelocity.z) > 0.01f ? Mathf.Sign(car.flWheelAngularVelocity.z) : 0f;
        float signOmegaRR = Mathf.Abs(car.rrWheelAngularVelocity.z) > 0.01f ? Mathf.Sign(car.rrWheelAngularVelocity.z) : 0f;
        float signOmegaRL = Mathf.Abs(car.rlWheelAngularVelocity.z) > 0.01f ? Mathf.Sign(car.rlWheelAngularVelocity.z) : 0f;

        float rollingTorqueFR = signOmegaFR * car.frRollingResistanceForce * car.frEffectiveTireRadius;
        float rollingTorqueFL = signOmegaFL * car.flRollingResistanceForce * car.flEffectiveTireRadius;
        float rollingTorqueRR = signOmegaRR * car.rrRollingResistanceForce * car.rrEffectiveTireRadius;
        float rollingTorqueRL = signOmegaRL * car.rlRollingResistanceForce * car.rlEffectiveTireRadius;

        // Bearing Drag (Natural spin-down when in the air or coasting):
        const float bearingDragCoefficient = 0.35f;

        float bearingTorqueFR = bearingDragCoefficient * car.frWheelAngularVelocity.z;
        float bearingTorqueFL = bearingDragCoefficient * car.flWheelAngularVelocity.z;
        float bearingTorqueRR = bearingDragCoefficient * car.rrWheelAngularVelocity.z;
        float bearingTorqueRL = bearingDragCoefficient * car.rlWheelAngularVelocity.z;

        /// 1. Calculate Target Angular Velocities (The speed required for 0 slip)
        float targetOmegaFR = vehicleForwardVelocity / car.frEffectiveTireRadius;
        float targetOmegaFL = vehicleForwardVelocity / car.flEffectiveTireRadius;
        float targetOmegaRR = vehicleForwardVelocity / car.rrEffectiveTireRadius;
        float targetOmegaRL = vehicleForwardVelocity / car.rlEffectiveTireRadius;

        // 2. Calculate the exact Torque needed to reach that target in ONE frame (T = I * dW / dt)
        float maxStabilizingTorqueFR = (car.wheelRotationalInertia * (targetOmegaFR - car.frWheelAngularVelocity.z)) / dt;
        float maxStabilizingTorqueFL = (car.wheelRotationalInertia * (targetOmegaFL - car.flWheelAngularVelocity.z)) / dt;
        float maxStabilizingTorqueRR = (car.wheelRotationalInertia * (targetOmegaRR - car.rrWheelAngularVelocity.z)) / dt;
        float maxStabilizingTorqueRL = (car.wheelRotationalInertia * (targetOmegaRL - car.rlWheelAngularVelocity.z)) / dt;

        // 3. Calculate Raw Reaction Torque from the tire forces
        // Positive Fx (pushing car forward) means the ground pushes back on the wheel (negative torque)
        float rawReactionFR = -car.frTireLongitudinalForce * car.frEffectiveTireRadius;
        float rawReactionFL = -car.flTireLongitudinalForce * car.flEffectiveTireRadius;
        float rawReactionRR = -car.rrTireLongitudinalForce * car.rrEffectiveTireRadius;
        float rawReactionRL = -car.rlTireLongitudinalForce * car.rlEffectiveTireRadius;

        // 4. Clamp the Raw Reaction Torque
        // If the raw reaction is pushing us toward the target, but is STRONGER than the required torque, cap it.
        float tireReactionFR = ClampTorque(rawReactionFR, maxStabilizingTorqueFR);
        float tireReactionFL = ClampTorque(rawReactionFL, maxStabilizingTorqueFL);
        float tireReactionRR = ClampTorque(rawReactionRR, maxStabilizingTorqueRR);
        float tireReactionRL = ClampTorque(rawReactionRL, maxStabilizingTorqueRL);

        // Grounded checks
        bool frGrounded = car.frTireNormalForce > 1f;
        bool flGrounded = car.flTireNormalForce > 1f;
        bool rrGrounded = car.rrTireNormalForce > 1f;
        bool rlGrounded = car.rlTireNormalForce > 1f;

        // --- FRONT RIGHT ---
        float totalTorqueFR = engineTorqueFR - bearingTorqueFR;
        if (frGrounded)
        {
            totalTorqueFR += tireReactionFR; // Note we ADD it here because we flipped the sign in step 3
            totalTorqueFR -= rollingTorqueFR;
        }
        else
        {
            car.frSlipRatio = 0f; 
            car.frTireLongitudinalForce = 0f;
        }

        // --- FRONT LEFT ---
        float totalTorqueFL = engineTorqueFL - bearingTorqueFL;
        if (flGrounded)
        {
            totalTorqueFL += tireReactionFL;
            totalTorqueFL -= rollingTorqueFL;
        }
        else
        {
            car.flSlipRatio = 0f;
            car.flTireLongitudinalForce = 0f;
        }

        // --- REAR RIGHT ---
        float totalTorqueRR = engineTorqueRR - bearingTorqueRR;
        if (rrGrounded)
        {
            totalTorqueRR += tireReactionRR;
            totalTorqueRR -= rollingTorqueRR;
        }
        else
        {
            car.rrSlipRatio = 0f;
            car.rrTireLongitudinalForce = 0f;
        }

        // --- REAR LEFT ---
        float totalTorqueRL = engineTorqueRL - bearingTorqueRL;
        if (rlGrounded)
        {
            totalTorqueRL += tireReactionRL;
            totalTorqueRL -= rollingTorqueRL;
        }
        else
        {
            car.rlSlipRatio = 0f;
            car.rlTireLongitudinalForce = 0f;
        }

        // Angular Acceleration:
        car.frWheelAngularAcceleration.z = totalTorqueFR / car.wheelRotationalInertia;
        car.flWheelAngularAcceleration.z = totalTorqueFL / car.wheelRotationalInertia;
        car.rrWheelAngularAcceleration.z = totalTorqueRR / car.wheelRotationalInertia;
        car.rlWheelAngularAcceleration.z = totalTorqueRL / car.wheelRotationalInertia;
    }

    private float ClampTorque(float rawTorque, float requiredTorque)
    {
        // If the torque is acting in the same direction as the required stabilization...
        if (Mathf.Sign(rawTorque) == Mathf.Sign(requiredTorque))
        {
            // Clamp it so it doesn't exceed the required stabilization torque
            return Mathf.Clamp(rawTorque, -Mathf.Abs(requiredTorque), Mathf.Abs(requiredTorque));
        }
        return rawTorque;
    }

    private void UpdateLateralTireForces(ref FullCarSystem car)
    {
        // already updated by steering dynamics
        float deltaFR = car.steerAngleFR;
        float deltaFL = car.steerAngleFL;

        // Planar velocities of the chassis center
        float vSway = car.chassisVelocity.x;
        float vPush = car.chassisVelocity.z;
        float yawRate = car.chassisAngularVelocity.y; // rad/s

        // Calculate planar velocities at each wheel hub due to vehicle translation + yaw rotation
        float localVelX_FR = vSway + (yawRate * car.distToFront_a1);
        float localVelZ_FR = vPush - (yawRate * car.distToRight_b1);

        float localVelX_FL = vSway + (yawRate * car.distToFront_a1);
        float localVelZ_FL = vPush + (yawRate * car.distToLeft_b2);

        float localVelX_RR = vSway - (yawRate * car.distToRear_a2);
        float localVelZ_RR = vPush - (yawRate * car.distToRight_b1);

        float localVelX_RL = vSway - (yawRate * car.distToRear_a2);
        float localVelZ_RL = vPush + (yawRate * car.distToLeft_b2);

        const float minimumVelocity = 0.1f;

        // Slip Angles (Now processing actual ground track velocities)
        car.frSlipAngle = Mathf.Atan2(localVelX_FR, Mathf.Max(Mathf.Abs(localVelZ_FR), minimumVelocity)) - deltaFR;
        car.flSlipAngle = Mathf.Atan2(localVelX_FL, Mathf.Max(Mathf.Abs(localVelZ_FL), minimumVelocity)) - deltaFL;
        car.rrSlipAngle = Mathf.Atan2(localVelX_RR, Mathf.Max(Mathf.Abs(localVelZ_RR), minimumVelocity));
        car.rlSlipAngle = Mathf.Atan2(localVelX_RL, Mathf.Max(Mathf.Abs(localVelZ_RL), minimumVelocity));

        //Maximum Lateral Force:
        float maxFyFR = car.peakLateralFriction * car.frTireNormalForce;
        float maxFyFL = car.peakLateralFriction * car.flTireNormalForce;
        float maxFyRR = car.peakLateralFriction * car.rrTireNormalForce;
        float maxFyRL = car.peakLateralFriction * car.rlTireNormalForce;

        //Linear Tire Model:
        car.frTireLateralForce = Mathf.Clamp(-car.corneringStiffnessC_alpha * car.frSlipAngle, -maxFyFR, maxFyFR);
        car.flTireLateralForce = Mathf.Clamp(-car.corneringStiffnessC_alpha * car.flSlipAngle, -maxFyFL, maxFyFL);
        car.rrTireLateralForce = Mathf.Clamp(-car.corneringStiffnessC_alpha * car.rrSlipAngle, -maxFyRR, maxFyRR);
        car.rlTireLateralForce = Mathf.Clamp(-car.corneringStiffnessC_alpha * car.rlSlipAngle, -maxFyRL, maxFyRL);

        //Aligning Moments:
        car.frYawMoment = car.frTireLateralForce * car.pneumaticTrail_a_x;
        car.flYawMoment = car.flTireLateralForce * car.pneumaticTrail_a_x;
        car.rrYawMoment = car.rrTireLateralForce * car.pneumaticTrail_a_x;
        car.rlYawMoment = car.rlTireLateralForce * car.pneumaticTrail_a_x;

        //Overturning Moments:
        car.frRollMoment = car.frTireNormalForce * car.lateralShift_a_y;
        car.flRollMoment = car.flTireNormalForce * car.lateralShift_a_y;
        car.rrRollMoment = car.rrTireNormalForce * car.lateralShift_a_y;
        car.rlRollMoment = car.rlTireNormalForce * car.lateralShift_a_y;
    }

    private void UpdateSuspensionForces(ref FullCarSystem car)
    {
        //Store Position and Velocity Variables For Chassis:
        float x = car.chassisPosition.y;
        float xDot = car.chassisVelocity.y;

        //Store Angular Position and Velocity Variables For Chassis:
        float theta = car.chassisAngle.x;
        float thetaDot = car.chassisAngularVelocity.x;

        float phi = car.chassisAngle.z;
        float phiDot = car.chassisAngularVelocity.z;

        //Store Position and Velocity Variables For Each Wheel:
        float xFR = car.frWheelPosition.y; float xFRDot = car.frWheelVelocity.y;
        float xFL = car.flWheelPosition.y; float xFLDot = car.flWheelVelocity.y;
        float xRR = car.rrWheelPosition.y; float xRRDot = car.rrWheelVelocity.y;
        float xRL = car.rlWheelPosition.y; float xRLDot = car.rlWheelVelocity.y;

        //Calculate Change in Position and Velocity For The Spring-Damper Systems For Front-Right Suspension:
        float suspDeltaPosFR = x - xFR - (car.distToFront_a1 * theta) + (car.distToRight_b1 * phi);
        float suspDeltaVelFR = xDot - xFRDot - (car.distToFront_a1 * thetaDot) + (car.distToRight_b1 * phiDot);

        //Calculate Change in Position and Velocity For The Spring-Damper Systems For Front-Left Suspension:
        float suspDeltaPosFL = x - xFL - (car.distToFront_a1 * theta) - (car.distToLeft_b2 * phi);
        float suspDeltaVelFL = xDot - xFLDot - (car.distToFront_a1 * thetaDot) - (car.distToLeft_b2 * phiDot);

        //Calculate Change in Position and Velocity For The Spring-Damper Systems For Rear-Right Suspension:
        float suspDeltaPosRR = x - xRR + (car.distToRear_a2 * theta) + (car.distToRight_b1 * phi);
        float suspDeltaVelRR = xDot - xRRDot + (car.distToRear_a2 * thetaDot) + (car.distToRight_b1 * phiDot);

        //Calculate Change in Position and Velocity For The Spring-Damper Systems For Rear-Left Suspension:
        float suspDeltaPosRL = x - xRL + (car.distToRear_a2 * theta) - (car.distToLeft_b2 * phi);
        float suspDeltaVelRL = xDot - xRLDot + (car.distToRear_a2 * thetaDot) - (car.distToLeft_b2 * phiDot);

        //Proportional-Derivative Control Law on Suspension Forces:
        car.suspensionForceFR = (car.suspensionConstantK_FR * suspDeltaPosFR) + (car.dampingCoefficientC_FR * suspDeltaVelFR);
        car.suspensionForceFL = (car.suspensionConstantK_FL * suspDeltaPosFL) + (car.dampingCoefficientC_FL * suspDeltaVelFL);
        car.suspensionForceRR = (car.suspensionConstantK_RR * suspDeltaPosRR) + (car.dampingCoefficientC_RR * suspDeltaVelRR);
        car.suspensionForceRL = (car.suspensionConstantK_RL * suspDeltaPosRL) + (car.dampingCoefficientC_RL * suspDeltaVelRL);

        //Antiroll Bar Torque Calculation:
        car.antirollMomentFront = 0f;
        car.antirollMomentRear = 0f;
        float w = car.distToRight_b1 + car.distToLeft_b2;

        if (car.useAdvancedModel)
        {
            car.antirollMomentFront = -car.antirollStiffnessKR_Front * (phi - ((xFR - xFL) / w));

            if (!car.useFrontAntirollBarOnly)
            {
                car.antirollMomentRear = -car.antirollStiffnessKR_Rear * (phi - ((xRR - xRL) / w));
            }
        }
        else
        {
            car.antirollMomentFront = -car.antirollStiffnessKR_Front * phi;

            if (!car.useFrontAntirollBarOnly)
            {
                car.antirollMomentRear = -car.antirollStiffnessKR_Rear * phi;
            }
        }
    }

    private void UpdateEquationsOfMotion(ref FullCarSystem car)
    {
        //Chassis Position Movement:
        float accBounce = (-car.suspensionForceFR - car.suspensionForceFL - car.suspensionForceRR - car.suspensionForceRL) / car.chassisMass - globalGravity;
        float totalLateralForce = car.frTireLateralForce + car.flTireLateralForce + car.rrTireLateralForce + car.rlTireLateralForce;
        float accSway = totalLateralForce / car.chassisMass;
        float totalLongitudinalForce = car.frTireLongitudinalForce + car.flTireLongitudinalForce + car.rrTireLongitudinalForce + car.rlTireLongitudinalForce;
        float gravityAlongSlope = car.chassisMass * globalGravity * Mathf.Sin(car.slopeAngle);
        float accPush = (totalLongitudinalForce - gravityAlongSlope) / car.chassisMass;

        //Chassis Angular Movement:
        float accPitch = (car.distToFront_a1 * (car.suspensionForceFR + car.suspensionForceFL) - car.distToRear_a2 * (car.suspensionForceRR + car.suspensionForceRL)) / car.chassisInertiaPitch;
        float accRoll = (-car.distToRight_b1 * (car.suspensionForceFR + car.suspensionForceRR) + car.distToLeft_b2 * (car.suspensionForceFL + car.suspensionForceRL) + car.antirollMomentFront + car.antirollMomentRear + car.frRollMoment + car.flRollMoment + car.rrRollMoment + car.rlRollMoment) / car.chassisInertiaRoll;
        float yawTorque = (car.distToFront_a1 * (car.frTireLateralForce + car.flTireLateralForce)) - (car.distToRear_a2 * (car.rrTireLateralForce + car.rlTireLateralForce)) + (car.frYawMoment + car.flYawMoment + car.rrYawMoment + car.rlYawMoment);
        float accYaw = yawTorque / car.chassisInertiaYaw;

        //Wheel Vertical Motion:
        float accWheelFR = (car.suspensionForceFR + car.frTireNormalForce) / car.wheelMassFR - globalGravity;
        float accWheelFL = (car.suspensionForceFL + car.flTireNormalForce) / car.wheelMassFL - globalGravity;
        float accWheelRR = (car.suspensionForceRR + car.rrTireNormalForce) / car.wheelMassRR - globalGravity;
        float accWheelRL = (car.suspensionForceRL + car.rlTireNormalForce) / car.wheelMassRL - globalGravity;

        //Motion Assignment:
        car.chassisAcceleration = new Vector3(accSway, accBounce, accPush);
        car.chassisAngularAcceleration = new Vector3(accPitch, accYaw, accRoll);

        car.frWheelAcceleration = new Vector3(0f, accWheelFR, 0f);
        car.flWheelAcceleration = new Vector3(0f, accWheelFL, 0f);
        car.rrWheelAcceleration = new Vector3(0f, accWheelRR, 0f);
        car.rlWheelAcceleration = new Vector3(0f, accWheelRL, 0f);
    }

    private void IntegrateDynamics(ref FullCarSystem car, float dt)
    {
        //Semi Implicit Euler Integration on Chassis Position:
        car.chassisVelocity += car.chassisAcceleration * dt;
        car.chassisPosition += car.chassisVelocity * dt;

        //Semi Implicit Euler Integration on Chassis Rotation:
        car.chassisAngularVelocity += car.chassisAngularAcceleration * dt;
        car.chassisAngle += car.chassisAngularVelocity * dt;

        //Semi Implicit Euler Integration on Front-Right Wheel Position:
        car.frWheelVelocity += car.frWheelAcceleration * dt;
        car.frWheelPosition += car.frWheelVelocity * dt;

        //Semi Implicit Euler Integration on Front-Left Wheel Position:
        car.flWheelVelocity += car.flWheelAcceleration * dt;
        car.flWheelPosition += car.flWheelVelocity * dt;

        //Semi Implicit Euler Integration on Rear-Right Wheel Position:
        car.rrWheelVelocity += car.rrWheelAcceleration * dt;
        car.rrWheelPosition += car.rrWheelVelocity * dt;

        //Semi Implicit Euler Integration on Rear-Left Wheel Position:
        car.rlWheelVelocity += car.rlWheelAcceleration * dt;
        car.rlWheelPosition += car.rlWheelVelocity * dt;


        car.frWheelAngularVelocity.z += car.frWheelAngularAcceleration.z * dt;
        car.flWheelAngularVelocity.z += car.flWheelAngularAcceleration.z * dt;
        car.rrWheelAngularVelocity.z += car.rrWheelAngularAcceleration.z * dt;
        car.rlWheelAngularVelocity.z += car.rlWheelAngularAcceleration.z * dt;

        //Angular Clamping:
        const float omegaDeadZone = 0.01f;

        if (Mathf.Abs(car.frWheelAngularVelocity.z) < omegaDeadZone)
        {
            car.frWheelAngularVelocity.z = 0f;
            car.frWheelAngularAcceleration.z = 0f;
        }

        if (Mathf.Abs(car.flWheelAngularVelocity.z) < omegaDeadZone)
        {
            car.flWheelAngularVelocity.z = 0f;
            car.flWheelAngularAcceleration.z = 0f;
        }

        if (Mathf.Abs(car.rrWheelAngularVelocity.z) < omegaDeadZone)
        {
            car.rrWheelAngularVelocity.z = 0f;
            car.rrWheelAngularAcceleration.z = 0f;
        }

        if (Mathf.Abs(car.rlWheelAngularVelocity.z) < omegaDeadZone)
        {
            car.rlWheelAngularVelocity.z = 0f;
            car.rlWheelAngularAcceleration.z = 0f;
        }
    }

    private void UpdateTransforms(ref FullCarSystem car, float dt)
    {
        //Calculate New Chassis Position and Rotation:
        car.chassisTransform.localPosition = car.chassisEquilibrium + car.chassisPosition;
        car.chassisTransform.localRotation = Quaternion.Euler(car.chassisAngle.x * Mathf.Rad2Deg, car.chassisAngle.y * Mathf.Rad2Deg, car.chassisAngle.z * Mathf.Rad2Deg);

        //Calculate New Wheel Positions:
        car.wheelTransformFR.localPosition = car.frWheelEquilibrium + car.frWheelPosition;
        car.wheelTransformFR.Rotate(Vector3.right, car.frWheelAngularVelocity.z * Mathf.Rad2Deg * dt, Space.Self);

        car.wheelTransformFL.localPosition = car.flWheelEquilibrium + car.flWheelPosition;
        car.wheelTransformFL.Rotate(Vector3.right, car.flWheelAngularVelocity.z * Mathf.Rad2Deg * dt, Space.Self);

        car.wheelTransformRR.localPosition = car.rrWheelEquilibrium + car.rrWheelPosition;
        car.wheelTransformRR.Rotate(Vector3.right, car.rrWheelAngularVelocity.z * Mathf.Rad2Deg * dt, Space.Self);

        car.wheelTransformRL.localPosition = car.rlWheelEquilibrium + car.rlWheelPosition;
        car.wheelTransformRL.Rotate(Vector3.right, car.rlWheelAngularVelocity.z * Mathf.Rad2Deg * dt, Space.Self);
    }

    private void SetupCarSystems()
    {
        if (formChain)
        {
            //Clear Existing Inspected Full Car Data:
            fullCars.Clear();

            for (int i = 0; i < chainCount; i++)
            {
                FullCarSystem newCar = new FullCarSystem();

                //Assign Default Physical Properties:
                newCar.chassisMass = defaultChassisMass;
                newCar.chassisInertiaPitch = defaultChassisInertiaPitch;
                newCar.chassisInertiaRoll = defaultChassisInertiaRoll;
                newCar.chassisInertiaYaw = defaultChassisInertiaYaw;

                newCar.wheelMassFR = defaultWheelMass; newCar.wheelMassFL = defaultWheelMass;
                newCar.wheelMassRR = defaultWheelMass; newCar.wheelMassRL = defaultWheelMass;

                newCar.distToFront_a1 = defaultDistA1; newCar.distToRear_a2 = defaultDistA2;
                newCar.distToRight_b1 = defaultDistB1; newCar.distToLeft_b2 = defaultDistB2;

                newCar.suspensionLengthFR = defaultSuspensionLength; newCar.suspensionConstantK_FR = defaultSuspensionK; newCar.dampingCoefficientC_FR = defaultDampingC;
                newCar.suspensionLengthFL = defaultSuspensionLength; newCar.suspensionConstantK_FL = defaultSuspensionK; newCar.dampingCoefficientC_FL = defaultDampingC;
                newCar.suspensionLengthRR = defaultSuspensionLength; newCar.suspensionConstantK_RR = defaultSuspensionK; newCar.dampingCoefficientC_RR = defaultDampingC;
                newCar.suspensionLengthRL = defaultSuspensionLength; newCar.suspensionConstantK_RL = defaultSuspensionK; newCar.dampingCoefficientC_RL = defaultDampingC;

                newCar.freeRadiusR0_FR = defaultTireRadius; newCar.tireConstantKt_FR = defaultTireKt;
                newCar.freeRadiusR0_FL = defaultTireRadius; newCar.tireConstantKt_FL = defaultTireKt;
                newCar.freeRadiusR0_RR = defaultTireRadius; newCar.tireConstantKt_RR = defaultTireKt;
                newCar.freeRadiusR0_RL = defaultTireRadius; newCar.tireConstantKt_RL = defaultTireKt;

                newCar.peakLongitudinalFriction = defaultPeakLongitudinalFriction;
                newCar.slipStiffness = defaultSlipStiffness;

                newCar.antirollStiffnessKR_Front = defaultAntirollKR;
                newCar.antirollStiffnessKR_Rear = defaultAntirollKR;
                newCar.antirollBarHeightOffset = defaultAntirollOffset;

                //Calculate Pivot & Equilibrium Positions:
                float lateralOffset = i * chainSpacingX;
                newCar.pivotPosition = new Vector3(lateralOffset, 0, 0);
                newCar.chassisPosition = new Vector3(0, i * initialDisplacementSpacing, 0);

                //Add New Car To The List:
                fullCars.Add(newCar);
            }
        }

        for (int i = 0; i < fullCars.Count; i++)
        {
            FullCarSystem current = fullCars[i];

            //Assign Default Values if Null or Invalid:
            if (current.maxSteerAngle <= 0) current.maxSteerAngle = defaultMaxSteeringAngle;
            if (current.engineTorque <= 0) current.engineTorque = defaultEngineTorque;

            if (current.chassisMass <= 0) current.chassisMass = defaultChassisMass;
            if (current.chassisInertiaPitch <= 0) current.chassisInertiaPitch = defaultChassisInertiaPitch;
            if (current.chassisInertiaRoll <= 0) current.chassisInertiaRoll = defaultChassisInertiaRoll;
            if (current.chassisInertiaYaw <= 0) current.chassisInertiaYaw = defaultChassisInertiaYaw;

            if (current.wheelRotationalInertia <= 0) current.wheelRotationalInertia = defaultWheelRotationalInertia;
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

            if (current.freeRadiusR0_FR <= 0) current.freeRadiusR0_FR = defaultTireRadius;
            if (current.tireConstantKt_FR <= 0) current.tireConstantKt_FR = defaultTireKt;
            if (current.rollingFrictionCoefficientU_FR <= 0) current.rollingFrictionCoefficientU_FR = defaultTireU;

            if (current.freeRadiusR0_FL <= 0) current.freeRadiusR0_FL = defaultTireRadius;
            if (current.tireConstantKt_FL <= 0) current.tireConstantKt_FL = defaultTireKt;
            if (current.rollingFrictionCoefficientU_FL <= 0) current.rollingFrictionCoefficientU_FL = defaultTireU;

            if (current.freeRadiusR0_RR <= 0) current.freeRadiusR0_RR = defaultTireRadius;
            if (current.tireConstantKt_RR <= 0) current.tireConstantKt_RR = defaultTireKt;
            if (current.rollingFrictionCoefficientU_RR <= 0) current.rollingFrictionCoefficientU_RR = defaultTireU;

            if (current.freeRadiusR0_RL <= 0) current.freeRadiusR0_RL = defaultTireRadius;
            if (current.tireConstantKt_RL <= 0) current.tireConstantKt_RL = defaultTireKt;
            if (current.rollingFrictionCoefficientU_RL <= 0) current.rollingFrictionCoefficientU_RL = defaultTireU;

            if (current.peakLongitudinalFriction <= 0) current.peakLongitudinalFriction = defaultPeakLongitudinalFriction;
            if (current.slipStiffness <= 0) current.slipStiffness = defaultSlipStiffness;

            if (current.antirollStiffnessKR_Front <= 0) current.antirollStiffnessKR_Front = defaultAntirollKR;
            if (current.antirollStiffnessKR_Rear <= 0) current.antirollStiffnessKR_Rear = defaultAntirollKR;

            //Assign Equilibrium Positions Based On Pivot Point:
            float xPos = current.pivotPosition.x;
            float zPos = current.pivotPosition.z;

            current.frWheelEquilibrium = new Vector3(xPos + current.distToRight_b1, current.freeRadiusR0_FR, zPos + current.distToFront_a1);
            current.flWheelEquilibrium = new Vector3(xPos - current.distToLeft_b2, current.freeRadiusR0_FL, zPos + current.distToFront_a1);
            current.rrWheelEquilibrium = new Vector3(xPos + current.distToRight_b1, current.freeRadiusR0_RR, zPos - current.distToRear_a2);
            current.rlWheelEquilibrium = new Vector3(xPos - current.distToLeft_b2, current.freeRadiusR0_RL, zPos - current.distToRear_a2);
            current.chassisEquilibrium = new Vector3(xPos, current.freeRadiusR0_FR + current.suspensionLengthFR, zPos);

            //Instantiate Chassis Mass Object:
            GameObject chassisObj = Instantiate(chassisPrefab, transform);
            chassisObj.name = $"Full Car Chassis {i + 1}";
            current.chassisTransform = chassisObj.transform;

            //Instantiate Wheel Mass Objects:
            current.wheelTransformFR = Instantiate(wheelPrefabFR, transform).transform; current.wheelTransformFR.name = $"FR Wheel {i + 1}";
            current.wheelTransformFL = Instantiate(wheelPrefabFL, transform).transform; current.wheelTransformFL.name = $"FL Wheel {i + 1}";
            current.wheelTransformRR = Instantiate(wheelPrefabRR, transform).transform; current.wheelTransformRR.name = $"RR Wheel {i + 1}";
            current.wheelTransformRL = Instantiate(wheelPrefabRL, transform).transform; current.wheelTransformRL.name = $"RL Wheel {i + 1}";

            //Create Line Renderers for Suspension and Tire Visuals:
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

            //Create Line Renderers for Antiroll Bar Visuals:
            current.antirollFrontCenterLine = CreateLineVisual(chassisObj.transform, "AR_FrontBar", Color.cyan);
            current.antirollRearCenterLine = CreateLineVisual(chassisObj.transform, "AR_RearBar", Color.cyan);

            //Initial Position:
            current.chassisTransform.localPosition = current.chassisEquilibrium + current.chassisPosition;
            current.wheelTransformFR.localPosition = current.frWheelEquilibrium + current.frWheelPosition;
            current.wheelTransformFL.localPosition = current.flWheelEquilibrium + current.flWheelPosition;
            current.wheelTransformRR.localPosition = current.rrWheelEquilibrium + current.rrWheelPosition;
            current.wheelTransformRL.localPosition = current.rlWheelEquilibrium + current.rlWheelPosition;

            //Store Updated Car Back Into The List:
            fullCars[i] = current;
        }
    }

    private void UpdateFullCarVisuals(ref FullCarSystem car, float roadFR, float roadFL, float roadRR, float roadRL)
    {
        //Store World Positions For The Suspension Pivot Points On The Chassis:
        Vector3 nodeFR_World = car.chassisTransform.TransformPoint(new Vector3(car.distToRight_b1, 0, car.distToFront_a1));
        Vector3 nodeFL_World = car.chassisTransform.TransformPoint(new Vector3(-car.distToLeft_b2, 0, car.distToFront_a1));
        Vector3 nodeRR_World = car.chassisTransform.TransformPoint(new Vector3(car.distToRight_b1, 0, -car.distToRear_a2));
        Vector3 nodeRL_World = car.chassisTransform.TransformPoint(new Vector3(-car.distToLeft_b2, 0, -car.distToRear_a2));

        //Store World Positions For Wheel Hubs:
        Vector3 hubFR = car.wheelTransformFR.position; Vector3 hubFL = car.wheelTransformFL.position;
        Vector3 hubRR = car.wheelTransformRR.position; Vector3 hubRL = car.wheelTransformRL.position;

        //Store World Positions For The Contact Points Between The Tires and The Road Surface:
        Vector3 contactFR = transform.TransformPoint(new Vector3(car.pivotPosition.x + car.distToRight_b1, roadFR, car.pivotPosition.z + car.distToFront_a1));
        Vector3 contactFL = transform.TransformPoint(new Vector3(car.pivotPosition.x - car.distToLeft_b2, roadFL, car.pivotPosition.z + car.distToFront_a1));
        Vector3 contactRR = transform.TransformPoint(new Vector3(car.pivotPosition.x + car.distToRight_b1, roadRR, car.pivotPosition.z - car.distToRear_a2));
        Vector3 contactRL = transform.TransformPoint(new Vector3(car.pivotPosition.x - car.distToLeft_b2, roadRL, car.pivotPosition.z - car.distToRear_a2));

        //Offset For Line Render Visibility:
        Vector3 visualOffset = transform.right * 0.12f;

        //Line Renders for Front-Right Suspension:
        RenderCoilSpring(car.suspSpringFRLine, nodeFR_World - visualOffset, hubFR - visualOffset);
        RenderStraightLine(car.suspDamperFRLine, nodeFR_World + visualOffset, hubFR + visualOffset);
        RenderStraightLine(car.tireSpringFRLine, hubFR, contactFR);

        //Line Render for Front-Left Suspension:
        RenderCoilSpring(car.suspSpringFLLine, nodeFL_World - visualOffset, hubFL - visualOffset);
        RenderStraightLine(car.suspDamperFLLine, nodeFL_World + visualOffset, hubFL + visualOffset);
        RenderStraightLine(car.tireSpringFLLine, hubFL, contactFL);

        //Line Render for Rear-Right Suspension:
        RenderCoilSpring(car.suspSpringRRLine, nodeRR_World - visualOffset, hubRR - visualOffset);
        RenderStraightLine(car.suspDamperRRLine, nodeRR_World + visualOffset, hubRR + visualOffset);
        RenderStraightLine(car.tireSpringRRLine, hubRR, contactRR);

        //Line Render for Rear-Left Suspension:
        RenderCoilSpring(car.suspSpringRLLine, nodeRL_World - visualOffset, hubRL - visualOffset);
        RenderStraightLine(car.suspDamperRLLine, nodeRL_World + visualOffset, hubRL + visualOffset);
        RenderStraightLine(car.tireSpringRLLine, hubRL, contactRL);

        //Line Renders for Antiroll Bar:
        Vector3 frontBarCenter = car.chassisTransform.position + car.chassisTransform.forward * car.distToFront_a1 - car.chassisTransform.up * car.antirollBarHeightOffset;
        RenderStraightLine(car.antirollFrontCenterLine, frontBarCenter - car.chassisTransform.right * car.distToLeft_b2, frontBarCenter + car.chassisTransform.right * car.distToRight_b1);

        //
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
}
