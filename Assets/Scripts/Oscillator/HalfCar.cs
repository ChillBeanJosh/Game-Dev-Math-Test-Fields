using System.Collections.Generic;
using UnityEngine;

public class HalfCar : MonoBehaviour
{
    [System.Serializable]
    public struct HalfCarSystem
    {
        [Header("Mass Profiles")]
        public float chassisMass;       // m
        public float chassisInertiaY;   // Iy (Rotational Pitch Inertia)
        public float wheelMassFront;     // m1
        public float wheelMassRear;      // m2

        [Header("Suspension Geometry (Horizontal Offset From COM)")]
        public float a1; // Distance from COM to front axle
        public float a2; // Distance from COM to rear axle

        [Header("Front Suspension Profiles")]
        public float suspensionLengthFront;
        public float suspensionK1;      // k1
        public float dampingC1;         // c1

        [Header("Rear Suspension Profiles")]
        public float suspensionLengthRear;
        public float suspensionK2;      // k2
        public float dampingC2;         // c2

        [Header("Tire Spring Profiles")]
        public float tireLengthFront;
        public float tireK_t1;          // k_t1
        public float tireLengthRear;
        public float tireK_t2;          // k_t2

        [Header("State Properties (Calculated Dynamically)")]
        public float chassisY;          // x
        public float chassisPitch;      // theta (In Radians)
        public float wheelYFront;       // x1
        public float wheelYRear;        // x2

        [HideInInspector] public float chassisVelocityY;      // x_dot
        [HideInInspector] public float chassisVelocityPitch;  // theta_dot
        [HideInInspector] public float wheelVelocityFront;    // x1_dot
        [HideInInspector] public float wheelVelocityRear;     // x2_dot

        [Header("World Space Alignments")]
        public Vector3 pivotPositionCOM;
        public float chassisEquilibriumY;
        public float wheelEquilibriumYFront;
        public float wheelEquilibriumYRear;

        [HideInInspector] public Transform chassisTransform;
        [HideInInspector] public Transform wheelTransformFront;
        [HideInInspector] public Transform wheelTransformRear;

        [HideInInspector] public LineRenderer suspensionLineFront;
        [HideInInspector] public LineRenderer suspensionLineRear;
        [HideInInspector] public LineRenderer tireLineFront;
        [HideInInspector] public LineRenderer tireLineRear;
    }

    [Header("Simulation Settings")]
    [Range(0.1f, 5f)] public float timeScale = 1.0f;
    public float globalGravity = 9.81f;
    public LayerMask groundLayer;

    [Header("Global Configuration Defaults")]
    public float defaultChassisMass = 1000f;
    public float defaultInertiaY = 1500f;
    public float defaultAxleDistance = 1.4f;
    public float defaultWheelMass = 45f;
    public float defaultSuspensionLength = 0.8f;
    public float defaultSuspensionK = 35000f;
    public float defaultDampingC = 2500f;
    public float defaultTireLength = 0.35f;
    public float defaultTireK = 150000f;

    [Header("System Container")]
    public List<HalfCarSystem> halfCars = new List<HalfCarSystem>();

    [Header("Prefabs & Materials")]
    public GameObject chassisPrefab;
    public GameObject wheelPrefab;
    public Material lineMaterial;

    private void Start()
    {
        if (chassisPrefab == null || wheelPrefab == null)
        {
            Debug.LogError("Prefabs are missing on the inspector configuration.");
            return;
        }
        SetupHalfCarSystems();
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime * timeScale;
        if (dt <= 0f) return;

        for (int i = 0; i < halfCars.Count; i++)
        {
            // Pull the temporary value snapshot out of the collection container
            HalfCarSystem car = halfCars[i];

            // 1. Sample Road Conditions
            float roadYFront = SampleRoadHeight(car.pivotPositionCOM.x + car.a1, car.pivotPositionCOM.z);
            float roadYRear = SampleRoadHeight(car.pivotPositionCOM.x - car.a2, car.pivotPositionCOM.z);

            // 2. State Mapping to Local Calculation Workspace Variables
            float x = car.chassisY;
            float theta = car.chassisPitch;
            float x1 = car.wheelYFront;
            float x2 = car.wheelYRear;

            float xDot = car.chassisVelocityY;
            float thetaDot = car.chassisVelocityPitch;
            float x1Dot = car.wheelVelocityFront;
            float x2Dot = car.wheelVelocityRear;

            // 3. Dynamic Tire Contact Force Safety Verification
            // Distance from wheel center to ground vs unstretched tire radius
            float tireDeflectionFront = car.tireLengthFront - (car.wheelEquilibriumYFront + x1 - roadYFront);
            float tireForceFront = (tireDeflectionFront > 0f) ? tireDeflectionFront * car.tireK_t1 : 0f;

            float tireDeflectionRear = car.tireLengthRear - (car.wheelEquilibriumYRear + x2 - roadYRear);
            float tireForceRear = (tireDeflectionRear > 0f) ? tireDeflectionRear * car.tireK_t2 : 0f;

            // 4. Matrix Derivation Calculations mapped directly from System Arrays
            // Row 1: Chassis Vertical Acceleration Mechanics
            float forceC_x = (car.dampingC1 + car.dampingC2) * xDot + (car.a2 * car.dampingC2 - car.a1 * car.dampingC1) * thetaDot - car.dampingC1 * x1Dot - car.dampingC2 * x2Dot;
            float forceK_x = (car.suspensionK1 + car.suspensionK2) * x + (car.a2 * car.suspensionK2 - car.a1 * car.suspensionK1) * theta - car.suspensionK1 * x1 - car.suspensionK2 * x2;
            float accChassisY = (-forceC_x - forceK_x) / car.chassisMass;

            // Row 2: Chassis Pitch Rotational Angular Acceleration
            float forceC_theta = (car.a2 * car.dampingC2 - car.a1 * car.dampingC1) * xDot + (car.dampingC1 * car.a1 * car.a1 + car.dampingC2 * car.a2 * car.a2) * thetaDot + (car.a1 * car.dampingC1) * x1Dot - (car.a2 * car.dampingC2) * x2Dot;
            float forceK_theta = (car.a2 * car.suspensionK2 - car.a1 * car.suspensionK1) * x + (car.suspensionK1 * car.a1 * car.a1 + car.suspensionK2 * car.a2 * car.a2) * theta + (car.a1 * car.suspensionK1) * x1 - (car.a2 * car.suspensionK2) * x2;
            float accChassisPitch = (-forceC_theta - forceK_theta) / car.chassisInertiaY;

            // Row 3: Front Axle Acceleration (Tire push acts in positive upward direction)
            float accWheelFront = (-car.dampingC1 * xDot + car.a1 * car.dampingC1 * thetaDot + car.dampingC1 * x1Dot - car.suspensionK1 * x + car.a1 * car.suspensionK1 * theta + car.suspensionK1 * x1 + tireForceFront) / car.wheelMassFront - globalGravity;

            // Row 4: Rear Axle Acceleration (Tire push acts in positive upward direction)
            float accWheelRear = (-car.dampingC2 * xDot - car.a2 * car.dampingC2 * thetaDot + car.dampingC2 * x2Dot - car.suspensionK2 * x - car.a2 * car.suspensionK2 * theta + car.suspensionK2 * x2 + tireForceRear) / car.wheelMassRear - globalGravity;

            // 5. Numerical Integration (Symplectic Euler Order: Update velocity first, then position)
            car.chassisVelocityY += accChassisY * dt;
            car.chassisY += car.chassisVelocityY * dt;

            car.chassisVelocityPitch += accChassisPitch * dt;
            car.chassisPitch += car.chassisVelocityPitch * dt;

            car.wheelVelocityFront += accWheelFront * dt;
            car.wheelYFront += car.wheelVelocityFront * dt;

            car.wheelVelocityRear += accWheelRear * dt;
            car.wheelYRear += car.wheelVelocityRear * dt;

            // 6. Update Transform Matrices in Engine Environment
            Vector3 chassisLocalPos = new Vector3(car.pivotPositionCOM.x, car.chassisEquilibriumY + car.chassisY, car.pivotPositionCOM.z);
            car.chassisTransform.localPosition = chassisLocalPos;
            car.chassisTransform.localRotation = Quaternion.Euler(0f, 0f, car.chassisPitch * Mathf.Rad2Deg);

            car.wheelTransformFront.localPosition = new Vector3(car.pivotPositionCOM.x + car.a1, car.wheelEquilibriumYFront + car.wheelYFront, car.pivotPositionCOM.z);
            car.wheelTransformRear.localPosition = new Vector3(car.pivotPositionCOM.x - car.a2, car.wheelEquilibriumYRear + car.wheelYRear, car.pivotPositionCOM.z);

            // 7. Re-render Visual Layout Overlays
            UpdateVisualLines(ref car, roadYFront, roadYRear);

            // CRITICAL FIX: Push modified local snapshot states cleanly back to live persistent collection 
            halfCars[i] = car;
        }
    }

    private void SetupHalfCarSystems()
    {
        for (int i = 0; i < halfCars.Count; i++)
        {
            HalfCarSystem car = halfCars[i];

            // Property Sanitization Defaults
            if (car.chassisMass <= 0) car.chassisMass = defaultChassisMass;
            if (car.chassisInertiaY <= 0) car.chassisInertiaY = defaultInertiaY;
            if (car.wheelMassFront <= 0) car.wheelMassFront = defaultWheelMass;
            if (car.wheelMassRear <= 0) car.wheelMassRear = defaultWheelMass;
            if (car.a1 <= 0) car.a1 = defaultAxleDistance;
            if (car.a2 <= 0) car.a2 = defaultAxleDistance;
            if (car.suspensionLengthFront <= 0) car.suspensionLengthFront = defaultSuspensionLength;
            if (car.suspensionLengthRear <= 0) car.suspensionLengthRear = defaultSuspensionLength;
            if (car.suspensionK1 <= 0) car.suspensionK1 = defaultSuspensionK;
            if (car.suspensionK2 <= 0) car.suspensionK2 = defaultSuspensionK;
            if (car.dampingC1 <= 0) car.dampingC1 = defaultDampingC;
            if (car.dampingC2 <= 0) car.dampingC2 = defaultDampingC;
            if (car.tireLengthFront <= 0) car.tireLengthFront = defaultTireLength;
            if (car.tireLengthRear <= 0) car.tireLengthRear = defaultTireLength;
            if (car.tireK_t1 <= 0) car.tireK_t1 = defaultTireK;
            if (car.tireK_t2 <= 0) car.tireK_t2 = defaultTireK;

            car.pivotPositionCOM = new Vector3(0f, 0f, i * 4.0f);

            // Ground-relative structural equilibriums setup
            car.wheelEquilibriumYFront = car.tireLengthFront;
            car.wheelEquilibriumYRear = car.tireLengthRear;
            car.chassisEquilibriumY = Mathf.Max(car.wheelEquilibriumYFront + car.suspensionLengthFront, car.wheelEquilibriumYRear + car.suspensionLengthRear);

            // Component Object Instantiations
            GameObject chassisObj = Instantiate(chassisPrefab, transform);
            chassisObj.name = $"Chassis_Rig_Vehicle_{i + 1}";
            car.chassisTransform = chassisObj.transform;

            GameObject frontWheelObj = Instantiate(wheelPrefab, transform);
            frontWheelObj.name = $"Axle_Front_{i + 1}";
            car.wheelTransformFront = frontWheelObj.transform;

            GameObject rearWheelObj = Instantiate(wheelPrefab, transform);
            rearWheelObj.name = $"Axle_Rear_{i + 1}";
            car.wheelTransformRear = rearWheelObj.transform;

            // Render Component Attaching Logic Blocks
            car.suspensionLineFront = CreateVisualElement("Suspension_Line_Front", chassisObj.transform, Color.red);
            car.suspensionLineRear = CreateVisualElement("Suspension_Line_Rear", chassisObj.transform, Color.red);
            car.tireLineFront = CreateVisualElement("Tire_Line_Front", frontWheelObj.transform, Color.blue);
            car.tireLineRear = CreateVisualElement("Tire_Line_Rear", rearWheelObj.transform, Color.blue);

            // Save initialized object references back to collection
            halfCars[i] = car;
        }
    }

    private void UpdateVisualLines(ref HalfCarSystem car, float roadFrontLocalY, float roadRearLocalY)
    {
        Vector3 frontSuspensionTop = car.chassisTransform.TransformPoint(new Vector3(car.a1, 0f, 0f));
        Vector3 frontSuspensionBottom = car.wheelTransformFront.position;
        car.suspensionLineFront.SetPosition(0, frontSuspensionTop);
        car.suspensionLineFront.SetPosition(1, frontSuspensionBottom);

        Vector3 rearSuspensionTop = car.chassisTransform.TransformPoint(new Vector3(-car.a2, 0f, 0f));
        Vector3 rearSuspensionBottom = car.wheelTransformRear.position;
        car.suspensionLineRear.SetPosition(0, rearSuspensionTop);
        car.suspensionLineRear.SetPosition(1, rearSuspensionBottom);

        Vector3 groundFrontWorld = transform.TransformPoint(new Vector3(car.pivotPositionCOM.x + car.a1, roadFrontLocalY, car.pivotPositionCOM.z));
        car.tireLineFront.SetPosition(0, car.wheelTransformFront.position);
        car.tireLineFront.SetPosition(1, groundFrontWorld);

        Vector3 groundRearWorld = transform.TransformPoint(new Vector3(car.pivotPositionCOM.x - car.a2, roadRearLocalY, car.pivotPositionCOM.z));
        car.tireLineRear.SetPosition(0, car.wheelTransformRear.position);
        car.tireLineRear.SetPosition(1, groundRearWorld);
    }

    private float SampleRoadHeight(float worldX, float worldZ)
    {
        Vector3 rayOrigin = transform.TransformPoint(new Vector3(worldX, 50f, worldZ));
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100f, groundLayer))
        {
            return transform.InverseTransformPoint(hit.point).y;
        }
        return 0f;
    }

    private LineRenderer CreateVisualElement(string name, Transform parent, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        LineRenderer line = obj.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));
        line.startColor = color;
        line.endColor = color;
        line.useWorldSpace = true;
        return line;
    }
}
