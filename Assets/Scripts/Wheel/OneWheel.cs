using System.Collections.Generic;
using UnityEngine;

public class OneWheel : MonoBehaviour
{
    [System.Serializable]
    public struct TireSystem
    {
        [Header("References")]
        public Transform tireTransform;

        [Header("Steering")]
        public float steerAngle; // Radians

        [Header("Dynamics")]
        public float verticalVelocity; // Track movement over time
        public float damping;         // Stops the wheel from bouncing forever

        public float steerVelocity;
        public float steerAcceleration;
        public float longitudinalAcceleration;
        public float verticalAcceleration;

        public float omegaDot;

        [Header("Tire Geometry")]
        public Vector3 localHubPosition;
        public float unloadedRadius;    // R0

        [Header("Radius & Kinematics")]
        public float loadedRadius;      // RL = R0 - deltaZ
        public float effectiveRadius;   // Re (Effective rolling radius)
        public float omega;             // Angular velocity (rad/s)
        public float rollingVelocity;   // v = omega * Re
        public float rotationAngle; // Track the current visual rotation (degrees)


        [Header("Stiffness Coefficients (N/m)")]
        public float kx;
        public float ky;
        public float kz;

        [Header("Tireprint Parameters")]
        public int n;
        public float a;
        public float b;
        public float sigma_zM;

        [Header("Tangential Stress")]
        public float tau_xM;
        public float tau_yM;

        [Header("Rolling Resistance")]
        [Tooltip("Coefficient of rolling resistance (dimensionless, typical: 0.01-0.02)")]
        public float rollingResistanceCoefficient;
        public float rollingResistanceForce; // Force opposing motion

        [Header("Dynamic Rolling Resistance (Coefficients)")]
        public float c0;   // Base resistance
        public float c1; // Linear scaling
        public float c2; // Quadratic scaling (Keep this very small)

        [Header("Tire Pressure Dynamics")]
        [Tooltip("Current inflation pressure (e.g., in PSI or Bar)")]
        public float inflationPressure;
        [Tooltip("The nominal pressure the tire's constants were measured at")]
        public float nominalPressure;
        [Tooltip("How sensitive the tire is to pressure changes (alpha)")]
        public float pressureSensitivity;

        [Header("Slip & Camber Resistance Coefficients")]
        [Tooltip("Influence of slip angle on rolling resistance (typical: 0.003 - 0.008 per rad^2)")]
        public float cAlpha;
        [Tooltip("Influence of camber angle on rolling resistance (typical: 0.001 - 0.004 per rad^2)")]
        public float cCamber;

        [Header("Longitudinal Dynamics (Pacejka)")]
        public float vx; // Forward velocity of the tire
        public float slipRatio; // Current longitudinal slip (s)
        public float mu_x; // Calculated friction coefficient

        [Header("Pacejka Coefficients")]
        [Tooltip("Stiffness Factor (Slope at origin)")]
        public float pB;
        [Tooltip("Shape Factor (Peak and asymptote positioning)")]
        public float pC;
        [Tooltip("Peak Factor (Maximum friction coefficient)")]
        public float pD;
        [Tooltip("Curvature Factor (Shape near the peak)")]
        public float pE;

        [Header("Lateral Dynamics (Pacejka)")]
        public float pB_y; // Stiffness Factor
        public float pC_y; // Shape Factor
        public float pD_y; // Peak Factor
        public float pE_y; // Curvature Factor

        [Header("Camber Dynamics")]
        [Tooltip("Stiffness of the tire to camber changes (N/rad)")]
        public float camberStiffness;
        public float Fy_camber; // Force resulting from camber
        public float Fy_slip;   // Force resulting from slip angle (Pacejka)

        [Header("Linear Model Coefficients")]
        public float longitudinalStiffness; // Cs (N)
        public float lateralStiffness;      // Ca (N/rad)
        public float pneumaticTrail;        // t (meters) - Distance between geometric center and center of pressure

        [Header("Deflections")]
        public float deltaX;
        public float deltaY;
        public float deltaZ;

        [Header("Forces & Moments")]
        public float Fx;
        public float Fy;
        public float Fz;
        public float Mx;
        public float My;
        public float Mz;

        [Header("Kinematic Angles")]
        public float alpha;
        public float gamma;

        [Header("Static Properties")]
        [Tooltip("The weight this tire is designed to carry (Newtons)")]
        public float designLoad;
    }

    [Header("Setup Parameters")]
    public GameObject tirePrefab;
    public bool formChain = true;
    public int chainCount = 4;
    public float tireSpacing = 1.5f;

    [Header("Input Settings")]
    public KeyCode throttleKey = KeyCode.W;
    public KeyCode brakeKey = KeyCode.S;
    public KeyCode steerLeftKey = KeyCode.A;
    public KeyCode steerRightKey = KeyCode.D;
    [Space]
    private float throttleInput;
    private float brakeInput;
    private float steeringInput;

    public float maxSteerAngle = 35f * Mathf.Deg2Rad;
    public float steeringGain = 150f;
    public float steeringDamping = 20f;

    [Header("Vehicle Dynamics")]
    public float engineTorque = 1000f; // Amount of torque applied
    private float currentThrottle;
    public bool useLinearModel = false;

    [Header("Global Simulation Parameters")]
    public float totalVehicleMass = 1500f;

    [Header("Simulation State")]
    [Range(0.1f, 5f)] public float TimeScale = 1.0f;
    public float globalGravity = 9.81f;
    [Space] 
    public LayerMask groundLayer;
    public List<TireSystem> Tires = new List<TireSystem>();

    [Header("Visualization")]
    public Material lineMaterial; // Assign a simple additive material
    private List<LineRenderer[]> tireVisualizers = new List<LineRenderer[]>();


    private void Start()
    {
        if (tirePrefab == null)
        {
            Debug.LogError("Required Prefabs are unassigned in the inspector.");
            return;
        }
        SetupTireSystems();
        InitializeVisualizers();
    }

    private void Update()
    {
        // Handle Input Polling (Run every frame)
        currentThrottle = 0f;
        if (Input.GetKey(throttleKey)) currentThrottle = 1f;
        if (Input.GetKey(brakeKey)) currentThrottle = -0.5f; // Reverse/Braking
    }

    private void SetupTireSystems()
    {
        if (formChain)
        {
            Tires.Clear();
            for (int i = 0; i < chainCount; i++)
            {
                TireSystem newTire = new TireSystem();

                newTire.verticalVelocity = 0f;
                newTire.damping = 500f; // Adjust this: Higher = less bounce, lower = more bounce

                //Assign Default Physical Properties:
                newTire.unloadedRadius = 0.35f;
                newTire.loadedRadius = newTire.unloadedRadius;
                newTire.effectiveRadius = newTire.unloadedRadius;

                newTire.kx = 150000f;
                newTire.ky = 100000f;
                newTire.kz = 200000f;

                //Assign Tireprint Properties:
                newTire.n = 3;
                newTire.a = 0.05f;
                newTire.b = 0.12f;

                // Assume 1/4 of total car weight (e.g., 1500kg car / 4 = 375kg per tire)
                // 375kg * 9.81 = ~3678 Newtons
                newTire.designLoad = 3678f;

                newTire.rollingResistanceCoefficient = 0.015f;

                newTire.c0 = 0.01f;
                newTire.c1 = 0.0001f;
                newTire.c2 = 0.000005f;

                newTire.inflationPressure = 32f;
                newTire.nominalPressure = 32f;
                newTire.pressureSensitivity = 0.8f; // Typical values range from 0.5 to 1.0

                // New coefficients (assuming alpha and gamma are calculated in radians)
                newTire.cAlpha = 0.005f;
                newTire.cCamber = 0.002f;

                // Pacejka defaults for dry road
                newTire.pB = 10.0f;
                newTire.pC = 1.65f;
                newTire.pD = 1.0f; // Peak friction coeff (mu)
                newTire.pE = 0.01f;

                // Lateral Pacejka (Defining values here)
                newTire.pB_y = 8.0f; // Stiffness (Usually slightly lower than longitudinal)
                newTire.pC_y = 1.3f; // Shape
                newTire.pD_y = 1.0f; // Peak Friction
                newTire.pE_y = 0.0f; // Curvature

                newTire.camberStiffness = 5000f; // Adjust this value based on your tire's physical specs
                newTire.Fy_camber = 0f;
                newTire.Fy_slip = 0f;

                //Setup Pivot:
                float zOffset = (i - (chainCount - 1) / 2f) * tireSpacing;
                newTire.localHubPosition = new Vector3(0f, newTire.unloadedRadius, zOffset);

                GameObject tireInstance = Instantiate(tirePrefab, transform);
                tireInstance.name = $"Tire_{i}";
                tireInstance.transform.localPosition = newTire.localHubPosition;
                newTire.tireTransform = tireInstance.transform;

                Tires.Add(newTire);
            }
        }
    }

    private void FixedUpdate()
    {
        float dt =
            Time.fixedDeltaTime * TimeScale;

        ReadDriverInput();

        for (int i = 0; i < Tires.Count; i++)
        {
            TireSystem tire = Tires[i];

            UpdateSteering(ref tire, dt);

            UpdateVerticalState(ref tire);

            UpdateSlipState(ref tire);

            CalculateNormalForce(ref tire);

            CalculateLongitudinalForce(ref tire);

            CalculateLateralForce(ref tire);

            CalculateMoments(ref tire);

            CalculateAccelerations(ref tire);

            IntegrateState(ref tire, dt);

            UpdateVisualization(ref tire);

            Tires[i] = tire;
        }
    }

    private void ReadDriverInput()
    {
        throttleInput = 0f;
        brakeInput = 0f;
        steeringInput = 0f;

        if (Input.GetKey(throttleKey))
            throttleInput = 1f;

        if (Input.GetKey(brakeKey))
            brakeInput = 1f;

        if (Input.GetKey(steerLeftKey))
            steeringInput = -1f;

        if (Input.GetKey(steerRightKey))
            steeringInput = 1f;
    }

    private void UpdateSteering(ref TireSystem tire, float dt)
    {
        float desiredAngle = steeringInput * maxSteerAngle;

        tire.steerAcceleration =
            steeringGain * (desiredAngle - tire.steerAngle)
            -
            steeringDamping * tire.steerVelocity;

        tire.steerVelocity += tire.steerAcceleration * dt;

        tire.steerAngle += tire.steerVelocity * dt;
    }

    private void UpdateVerticalState(ref TireSystem tire)
    {
        Vector3 worldHub = transform.TransformPoint(tire.localHubPosition);

        float roadY = SampleRoadHeightAtPoint(worldHub.x, worldHub.z);

        float tireBottom = tire.localHubPosition.y - tire.unloadedRadius;

        tire.deltaZ = Mathf.Max(0f, roadY - tireBottom);

        tire.loadedRadius =
            tire.unloadedRadius - tire.deltaZ;

        tire.effectiveRadius =
            tire.loadedRadius + tire.deltaZ / 3f;
    }

    private void UpdateSlipState(ref TireSystem tire)
    {
        tire.rollingVelocity =
            tire.omega * tire.effectiveRadius;

        float cosS = Mathf.Cos(tire.steerAngle);
        float sinS = Mathf.Sin(tire.steerAngle);

        float vLong =
            tire.vx * cosS;

        float vLat =
            tire.vx * sinS;

        tire.alpha =
            -Mathf.Atan2(
                vLat,
                Mathf.Max(Mathf.Abs(vLong), 0.1f));

        tire.slipRatio =
            (tire.rollingVelocity - vLong)
            /
            Mathf.Max(Mathf.Abs(vLong), 0.1f);
    }

    private void CalculateNormalForce(ref TireSystem tire)
    {
        tire.Fz = tire.kz * tire.deltaZ;

        tire.Fz = Mathf.Max(0f, tire.Fz);
    }


    private void CalculateLongitudinalForce(ref TireSystem tire)
    {
        float s = tire.slipRatio;

        tire.mu_x =
            tire.pD *
            Mathf.Sin(
                tire.pC *
                Mathf.Atan(
                    tire.pB * s
                    -
                    tire.pE *
                    (
                        tire.pB * s
                        -
                        Mathf.Atan(tire.pB * s)
                    )
                )
            );

        tire.Fx =
            tire.mu_x *
            tire.Fz;
    }

    private void CalculateLateralForce(ref TireSystem tire)
    {
        float alpha = tire.alpha;

        tire.Fy_slip =
            tire.pD_y *
            Mathf.Sin(
                tire.pC_y *
                Mathf.Atan(
                    tire.pB_y * alpha
                    -
                    tire.pE_y *
                    (
                        tire.pB_y * alpha
                        -
                        Mathf.Atan(tire.pB_y * alpha)
                    )
                )
            );

        tire.Fy_slip *= tire.Fz;

        tire.Fy_camber =
            tire.camberStiffness *
            tire.gamma;

        tire.Fy =
            tire.Fy_slip
            +
            tire.Fy_camber;
    }

    private void CalculateMoments(ref TireSystem tire)
    {
        tire.Mz =
            tire.Fy *
            tire.pneumaticTrail;

        tire.My =
            tire.Fz *
            tire.rollingResistanceCoefficient *
            tire.effectiveRadius;

        tire.Mx =
            tire.gamma *
            tire.camberStiffness *
            0.05f;
    }

    private void CalculateAccelerations(ref TireSystem tire)
    {
        float mass =
            totalVehicleMass / Tires.Count;

        tire.longitudinalAcceleration =
            tire.Fx / mass;

        float spring =
            tire.kz * tire.deltaZ;

        float damping =
            -tire.damping * tire.verticalVelocity;

        float gravity =
            mass * globalGravity;

        tire.verticalAcceleration =
            (spring + damping - gravity)
            / mass;

        float engineAppliedTorque =
            throttleInput * engineTorque;

        float brakeTorque =
            brakeInput * engineTorque;

        float tractionTorque =
            tire.Fx * tire.effectiveRadius;

        float wheelInertia = 1.5f;

        tire.omegaDot =
            (
                engineAppliedTorque
                -
                brakeTorque
                -
                tractionTorque
            )
            /
            wheelInertia;
    }

    private void IntegrateState(ref TireSystem tire, float dt)
    {
        tire.verticalVelocity +=
            tire.verticalAcceleration * dt;

        tire.localHubPosition.y +=
            tire.verticalVelocity * dt;

        tire.vx +=
            tire.longitudinalAcceleration * dt;

        tire.localHubPosition.z +=
            tire.vx * dt;

        tire.omega +=
            tire.omegaDot * dt;

        tire.rotationAngle +=
            tire.omega *
            Mathf.Rad2Deg *
            dt;

        tire.rotationAngle %= 360f;
    }

    private void UpdateVisualization(ref TireSystem tire)
    {
        tire.tireTransform.localPosition =
            tire.localHubPosition;

        tire.tireTransform.localRotation =
            Quaternion.Euler(
                tire.rotationAngle,
                tire.steerAngle * Mathf.Rad2Deg,
                0f);
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

    /// <summary>
    /// Computes the normal stress at a specific local point (x, y) within the tire contact patch.
    /// Used for Equation 3.29.
    /// </summary>
    public float GetNormalStressAtPoint(TireSystem tire, float localX, float localY)
    {
        if (tire.sigma_zM <= 0f) return 0f;

        // Check if the point is outside the theoretical bounds of the contact patch
        if (Mathf.Abs(localX) > tire.a || Mathf.Abs(localY) > tire.b) return 0f;

        float xTerm = Mathf.Pow(localX / tire.a, 2 * tire.n);
        float yTerm = Mathf.Pow(localY / tire.b, 2 * tire.n);

        float stress = tire.sigma_zM * (1f - xTerm - yTerm);

        return Mathf.Max(0f, stress); // Stress cannot be negative
    }

    /// <summary>
    /// Models the tangential stress in the x-direction (longitudinal).
    /// Equation 3.32: tau_x(x,y) = -tau_xM * (x^(2n+1) / a^(2n+1)) * sin^2(x/a * pi) * cos(y/2b * pi)
    /// </summary>
    public float GetTangentialStressX(TireSystem tire, float x, float y)
    {
        // Check bounds of the tireprint area
        if (Mathf.Abs(x) > tire.a || Mathf.Abs(y) > tire.b) return 0f;

        float xRatio = Mathf.Pow(x / tire.a, 2 * tire.n + 1);
        float sinPart = Mathf.Pow(Mathf.Sin((x / tire.a) * Mathf.PI), 2);
        float cosPart = Mathf.Cos((y / (2f * tire.b)) * Mathf.PI);

        return -tire.tau_xM * xRatio * sinPart * cosPart;
    }

    /// <summary>
    /// Models the tangential stress in the y-direction (lateral).
    /// Equation 3.33: tau_y(x,y) = -tau_yM * ((x^(2n) / a^(2n)) - 1) * sin(y/b * pi)
    /// </summary>
    public float GetTangentialStressY(TireSystem tire, float x, float y)
    {
        // Check bounds of the tireprint area
        if (Mathf.Abs(x) > tire.a || Mathf.Abs(y) > tire.b) return 0f;

        float xRatio = Mathf.Pow(x / tire.a, 2 * tire.n);
        float sinPart = Mathf.Sin((y / tire.b) * Mathf.PI);

        return -tire.tau_yM * (xRatio - 1f) * sinPart;
    }

    private void CalculateTireForces(ref TireSystem tire)
    {
        // Linear Model
        // Fx = C_s * slipRatio * Fz
        // Fy = (-C_alpha * alpha) + (C_gamma * gamma)

        // 1. Longitudinal Force
        tire.Fx = tire.longitudinalStiffness * tire.slipRatio * tire.Fz;

        // 2. Lateral Force (Sum of slip and camber effects)
        tire.Fy = (-tire.lateralStiffness * tire.alpha) + (tire.camberStiffness * tire.gamma);

        // 3. Moments
        // Aligning Moment (Mz) = Fy * Pneumatic Trail
        tire.Mz = tire.Fy * tire.pneumaticTrail;

        // Rolling Resistance Moment (My)
        tire.My = tire.Fz * tire.rollingResistanceCoefficient * tire.effectiveRadius;

        // Mx (Camber/Roll Moment)
        tire.Mx = tire.gamma * tire.camberStiffness * 0.05f;
    }

    private void InitializeVisualizers()
    {
        foreach (var tire in Tires)
        {
            // We need 3 arrows: Fz, Fx, Fy. Each arrow takes 3 lines (Body + 2 wings)
            LineRenderer[] renderers = new LineRenderer[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject go = new GameObject("ForceArrow_" + i);
                go.transform.SetParent(tire.tireTransform);
                LineRenderer lr = go.AddComponent<LineRenderer>();
                lr.material = lineMaterial;
                lr.startWidth = 0.02f; lr.endWidth = 0.02f;
                lr.positionCount = 3; // Line body + 2 wings
                renderers[i] = lr;
            }
            tireVisualizers.Add(renderers);
        }
    }

    private void DrawForceArrow(LineRenderer lr, Vector3 start, Vector3 direction, float magnitude, Color color)
    {
        float scale = 0.001f; // Normalize force magnitude to meters
        Vector3 end = start + (direction * magnitude * scale);

        lr.startColor = color; lr.endColor = color;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        // Add logic here to set positions for arrow wings if desired
    }
}
