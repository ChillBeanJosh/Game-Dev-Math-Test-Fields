using UnityEngine;

public class geometricCollision : MonoBehaviour
{
    [Header("Particle Information: ")]
    public GameObject particlePrefab;
    public int numberOfParticles = 10;
    public float Diameter = 1f;
    [Space]

    [Header("Box Information: ")]
    public float boxSize = 10f;
    private LineRenderer boxRenderer;
    [Space]

    [Header("Collision Information: ")]
    Vector2[] pos;
    GameObject[] particles;


    private void Start()
    {
        CreateBoxOutline();

        // Initialize the arrays for positions and particles. The size of the arrays is determined by the number of particles specified in the inspector.
        pos = new Vector2[numberOfParticles];
        particles = new GameObject[numberOfParticles];

        for (int i = 0; i < numberOfParticles; i++)
        {
            // (1) Generate random positions for each particle within the box, ensuring they do not spawn outside the box boundaries by considering their diameter.
            pos[i] = new Vector2
                (
                    Random.Range(Diameter, boxSize - Diameter),
                    Random.Range(Diameter, boxSize - Diameter)
                );

            // (2) Instantiate the particle prefab at the generated position with no rotation (Quaternion.identity). The instantiated particle is stored in the particles array for later reference.
            particles[i] = Instantiate
                (
                    particlePrefab,
                    new Vector3(pos[i].x, pos[i].y, 0),
                    Quaternion.identity
                );

            // (3) Set the local scale of the instantiated particle to be uniform in all dimensions, with the size determined by the Diameter variable. This ensures that all particles have the same size as specified in the inspector.
            particles[i].transform.localScale = Vector3.one * Diameter;
        }
    }

    void CreateBoxOutline()
    {
        // (1) Create a new GameObject named "Simulation Box" and add a LineRenderer component to it. The LineRenderer will be used to draw the outline of the box.
        GameObject box = new GameObject("Simulation Box");
        boxRenderer = box.AddComponent<LineRenderer>();

        // (2) Configure the LineRenderer properties to define how the box outline will be drawn. The position count is set to 5 to create a closed loop (4 corners + 1 to return to the starting point). The width multiplier determines the thickness of the lines, and useWorldSpace is set to true to ensure the lines are drawn in world space.
        boxRenderer.positionCount = 5;
        boxRenderer.loop = false;
        boxRenderer.widthMultiplier = 0.05f;
        boxRenderer.useWorldSpace = true;

        // (3) Set the material and colors for the LineRenderer. The material is set to a simple sprite shader, and both the start and end colors are set to white to create a consistent outline color for the box.
        boxRenderer.material = new Material(Shader.Find("Sprites/Default"));
        boxRenderer.startColor = Color.white;
        boxRenderer.endColor = Color.white;

        // (4) Define the corners of the box using a Vector3 array. The corners are defined in a specific order to create a closed loop when drawn by the LineRenderer. The first corner is at the origin (0, 0, 0), and the subsequent corners are defined based on the box size along the X and Y axes.
        Vector3[] corners = new Vector3[5];
        corners[0] = new Vector3(0, 0, 0);                  // (1) [Origin]
        corners[1] = new Vector3(boxSize, 0, 0);            // (2) [Origin -> X-Axis]
        corners[2] = new Vector3(boxSize, boxSize, 0);      // (3) [X-Axis -> XY-Axis]
        corners[3] = new Vector3(0, boxSize, 0);            // (4) [XY-Axis -> Y-Axis]
        corners[4] = corners[0];                            // (5) [Y-Axis -> Origin]

        // (5) Set the positions of the LineRenderer to the defined corners, which will draw the outline of the box in the scene.
        boxRenderer.SetPositions(corners);
    }

    private void Update()
    {
        DetectCollision();
    }

    void DetectCollision()
    {
        // (1) Reset the color of all particles to white at the beginning of each frame. This ensures that any previous collision indications are cleared before checking for new collisions.
        for (int i = 0; i < numberOfParticles; i++)
        {
            particles[i].GetComponent<Renderer>().material.color = Color.white;
        }
        

        // (2) Nested loop to check for collisions between particles. The outer loop iterates through each particle, while the inner loop checks for collisions with the remaining particles (starting from i + 1 to avoid redundant checks).
        for (int currentParticle = 0; currentParticle < numberOfParticles; currentParticle++)
        {
            for (int nextParticle = currentParticle + 1; nextParticle < numberOfParticles; nextParticle++)
            {
                // (3) Calculate the vector (delta) between the positions of the current particle and the next particle. The squared distance between the two particles is calculated using the sqrMagnitude property of the delta vector, which is more efficient than calculating the actual distance. The squared collision distance is determined by squaring the Diameter variable, which represents the minimum distance at which a collision occurs.
                Vector2 delta = pos[nextParticle] - pos[currentParticle];
                float distanceSquared = delta.sqrMagnitude;
                float collisionDistanceSquared = Diameter * Diameter;

                // (4) If the squared distance between the two particles is less than the squared collision distance, it indicates that a collision has occurred. In this case, the color of both particles involved in the collision is changed to red to visually indicate the collision in the scene.
                if (distanceSquared < collisionDistanceSquared)
                {
                    particles[currentParticle].GetComponent<Renderer>().material.color = Color.red;
                    particles[nextParticle].GetComponent<Renderer>().material.color = Color.red;
                }
            }
        }
    }
}
