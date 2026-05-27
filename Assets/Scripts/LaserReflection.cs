using UnityEditor;
using UnityEngine;

public class LaserReflection : MonoBehaviour
{
    [Header("Laser Settings: ")]
    public float totalDistance = 50f;
    public int maxBounces = 10;
    public LayerMask reflectionLayer;
    public LayerMask refractionLayer;
    private LineRenderer lineRenderer;

    [Header("Debug Settings: ")]
    [Range(0, 10)] public int debugBounceNumber;
    [Range(0f, 10f)] public float debugVectorLength = 2f;
    [Space]
    private Vector3 debugHitPoint;
    private Vector3 debugNormal;
    private Vector3 debugIncident;
    private Vector3 debugRotationAxis;
    [Space]
    private float debugTheta1; 
    private float debugTheta2;
    private bool isRefractionBounce = false;
    [Space]
    private bool hasDebugData = false;

    [Header("Refraction Settings: ")]
    private float n_air = 1.0003f;

    void Start() => lineRenderer = GetComponent<LineRenderer>();
    void Update() => CalculateLaser();

    void CalculateLaser()
    {
        //Initial Parameters:
        Vector3 position = transform.position;
        Vector3 direction = transform.forward;
        float remainingDistance = totalDistance;

        //Line Renderer Setup:
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, position);
        hasDebugData = false;

        for (int i = 0; i < maxBounces; i++)
        {
            LayerMask combinedMask = reflectionLayer | refractionLayer;
            if (Physics.Raycast(position, direction, out RaycastHit hit, remainingDistance, combinedMask))
            {
                //Set Line Renderer Position at Hit Point and Reduce Remaining Distance:
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, hit.point);
                remainingDistance -= hit.distance;

                //Declare and Normalize Incident and Normal Vectors:
                Vector3 incident = direction.normalized;
                Vector3 normal = hit.normal.normalized;

                //Calculate Angle of Incidence and Rotation Axis:
                float dot = Vector3.Dot(-incident, normal);
                dot = Mathf.Clamp(dot, -1f, 1f);
                float theta1Rad = Mathf.Acos(dot);
                Vector3 rotationAxis = Vector3.Cross(normal, -incident).normalized;

                float currentTheta1 = theta1Rad * Mathf.Rad2Deg;
                float currentTheta2 = 0f;
                bool isRefracting = false;

                // --- BRANCH: REFRACTION ---
                if (((1 << hit.collider.gameObject.layer) & refractionLayer) != 0)
                {
                    //Declare A New Index of Refraction Based on Material Tag:
                    isRefracting = true;
                    float n_material = GetIndexByTag(hit.collider.tag);

                    //Determine If Entering or Exiting Material, Then Set Proper Index of Refraction, Then Set Correct Direction For Normal:
                    bool entering = Vector3.Dot(incident, normal) < 0;
                    float n1 = entering ? n_air : n_material;
                    float n2 = entering ? n_material : n_air;
                    Vector3 normalForMath = entering ? normal : -normal;

                    // Calculate Refraction Using Snell's Law:
                    float sinTheta2 = (n1 * Mathf.Sin(theta1Rad)) / n2;

                    //If (Within Sin Range), Calculate Refraction Direction: 
                    if (sinTheta2 <= 1.0f)
                    {
                        //Calculate Refraction Angle:
                        float theta2Rad = Mathf.Asin(sinTheta2);
                        currentTheta2 = theta2Rad * Mathf.Rad2Deg;

                        //Rotate Incident Vector by Refraction Angle Around Rotation Axis to Get New Direction:
                        Quaternion refractionRotation = Quaternion.AngleAxis(currentTheta2, rotationAxis);
                        direction = refractionRotation * -normalForMath;
                        position = hit.point + (direction * 0.001f);
                    }
                    //Else Handle Total Internal Reflection:
                    else 
                    {
                        // Total Internal Reflection Occurs, Treat As Reflection:
                        currentTheta2 = currentTheta1;
                        direction = Quaternion.AngleAxis(180f, normal) * -incident;
                        position = hit.point + (direction * 0.001f);
                    }
                }
                // --- BRANCH: REFLECTION ---
                else
                {
                    currentTheta2 = currentTheta1;
                    direction = Quaternion.AngleAxis(2 * currentTheta2, rotationAxis) * -incident;
                    position = hit.point;
                }

                // Debug capture
                if (i == debugBounceNumber)
                {
                    debugHitPoint = hit.point;
                    debugNormal = normal;
                    debugIncident = -incident;
                    debugRotationAxis = rotationAxis;
                    debugTheta1 = currentTheta1;
                    debugTheta2 = currentTheta2;
                    isRefractionBounce = isRefracting;
                    hasDebugData = true;
                }
            }
            else
            {
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, position + direction * remainingDistance);
                break;
            }
            if (remainingDistance <= 0) break;
        }
    }

    float GetIndexByTag(string tag)
    {
        return tag switch
        {
            "Vacuum" => 1.0000f,
            "Air" => 1.0003f,
            "Ice" => 1.309f,
            "Water" => 1.333f,
            "Ethanol Alcohol" => 1.361f,
            "Teflon" => 1.38f,
            "Flourite" => 1.434f,
            "Glycerin" => 1.473f,
            "Benzene" => 1.501f,
            "Plexiglas" => 1.51f,
            "Crown Glass" => 1.52f,
            "Light Flint Glass" => 1.58f,
            "Polycarbonate Glass" => 1.59f,
            "Dense Flint Glass" => 1.66f,
            "Sapphire Gemstone" => 1.77f,
            "Zircon" => 1.923f,
            "Diamond" => 2.417f,
            "Rutile" => 2.907f,
            "Gallium Phosphide" => 3.50f,
        };
    }

    private void OnDrawGizmos()
    {
        if (!hasDebugData) return;

        //Normal Vector:
        Gizmos.color = Color.white;
        Gizmos.DrawRay(debugHitPoint, debugNormal * debugVectorLength);

        //Incident Vector:
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(debugHitPoint, debugIncident * debugVectorLength);

        //Rotation Axis:
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(debugHitPoint, debugRotationAxis * debugVectorLength);

        // ------------------------------------------------------------------------------------

        // Angle Text:
#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.green;
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;

        string angleText = isRefractionBounce
            ? $"θ1 (Inc): {debugTheta1:F2}°\nθ2 (Refr): {debugTheta2:F2}°"
            : $"θ1 (Inc): {debugTheta1:F2}°\nθ2 (Refl): {debugTheta2:F2}°";

        Handles.Label(debugHitPoint + (Vector3.up * 0.5f), angleText, style);
#endif  
    }
}
