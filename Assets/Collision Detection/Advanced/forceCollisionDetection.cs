using UnityEngine;
using UnityEngine.VFX;

public class forceCollisionDetection : MonoBehaviour
{
    [Header("Particle Information: ")]
    public int numberOfParticles = 10;                 
    public float Diameter = 1f;
    public float mass = 3f;
    [Space]

    [Header("Collision Parameters: ")]
    [Tooltip("Sping constant used as a force scalar for both particle-particle and particle-wall interactions. Higher values create stiffer collisions, while lower values result in softer interactions.")]
    public float springConstant = 100f;
    [Tooltip("Size of the box is determined by this multiplier times the particle diameter.")]
    public float boxMultiplier = 10f;
    [Space]

    [Header("Physical Parameters: ")]
    public float gravity = 0f;
    [Space]

    [Header("Time Parameters: ")]
    public float totalTime = 100f;
    [Range(0.001f, 0.1f)] public float dt = 0.01f;
    [Space]

    [Header("Visualization: ")]
    [Tooltip("Toggle to visualize particles and their velocities. Disabling can improve performance for large particle counts.")]
    public bool visualize = true;
    public GameObject particlePrefab;
    private LineRenderer boxRenderer;

    // Internal simulation variables:
    float Lx, Ly;
    int Nt;
    int currentStep;

    Vector2[] pos;
    Vector2[] vel;
    Vector2[] accOld;

    GameObject[] particleObjects;

    void Start()
    {
        CreateBoxOutline();

        // (1) Set box dimensions based on the multiplier and particle diameter
        Lx = boxMultiplier * Diameter;
        Ly = boxMultiplier * Diameter;

        // (2) Calculate the number of time steps based on total simulation time and time step size
        Nt = Mathf.FloorToInt(totalTime / dt);

        InitializeParticles();
    }

    void InitializeParticles()
    {
        // (0.1) Initialize arrays to store particle positions, velocities, accelerations, and visual representations, ensuring that all necessary data structures are set up for the simulation
        pos = new Vector2[numberOfParticles];
        vel = new Vector2[numberOfParticles];
        accOld = new Vector2[numberOfParticles];
        particleObjects = new GameObject[numberOfParticles];

        // (0.2) Initialize a counter to keep track of how many particles have been initialized, ensuring that the loop continues until the specified number of particles has been created without exceeding the limits of the box dimensions
        int count = 0;
        float spacing = Diameter;

        // (1.1) Initialize particle positions in a grid-like pattern, ensuring they are spaced at least one diameter apart to avoid initial overlaps
        for (float x = Diameter / 2; x < Lx - Diameter / 2 && count < numberOfParticles; x += spacing)
        {
            for (float y = Diameter / 2; y < Ly - Diameter / 2 && count < numberOfParticles; y += spacing)
            {
                // (1.2) Apply the spacing logic to the particle positions, ensuring they are placed in a grid pattern with at least one diameter of space between them to prevent initial overlaps
                pos[count] = new Vector2(x, y);

                // (2) Assign small random initial velocities to create dynamic behavior from the start
                vel[count] = new Vector2
                    (
                        Random.Range(-0.33f, 0.33f),
                        Random.Range(-0.33f, 0.33f)
                    );

                // (3) Set initial acceleration to account for gravity, ensuring particles start with the correct initial conditions for the Verlet integration
                accOld[count] = new Vector2(0, -gravity);

                // (4.1) If visualization is enabled and a particle prefab is assigned, instantiate a visual representation of the particle at its initial position, allowing for real-time observation of the simulation
                if (visualize && particlePrefab != null)
                {
                    // (4.2) Instantiate the particle prefab at the calculated position with no rotation, creating a visual representation of the particle in the Unity scene
                    particleObjects[count] = Instantiate
                        (
                            particlePrefab,
                            new Vector3(x, y, 0),
                            Quaternion.identity
                        );

                    // (4.3) Set the local scale of the instantiated particle object to match the specified diameter, ensuring that the visual representation accurately reflects the physical size of the particle in the simulation
                    particleObjects[count].transform.localScale = Vector3.one * Diameter;
                }

                // (5) Increment the particle count to keep track of how many particles have been initialized, ensuring that the loop continues until the specified number of particles has been created without exceeding the limits of the box dimensions
                count++;
            }
        }
    }

    void Update()
    {
        // (1) Check if the current simulation step has reached or exceeded the total number of time steps, ensuring that the simulation runs for the specified duration and then stops to prevent unnecessary computations
        if (currentStep >= Nt) return;

        // (2) Call the SimulateStep method to perform the physics calculations for the current time step, ensuring that the positions, velocities, and forces are updated according to the Verlet integration method and collision detection logic
        SimulateStep();

        // (3) Increment the current step counter to keep track of the simulation progress, ensuring that the loop continues until the specified number of time steps has been completed
        currentStep++;
    }

    void SimulateStep()
    {
        // Initialize an array to store the net forces acting on each particle, ensuring that all forces are reset to zero at the start of the simulation step
        Vector2[] force = new Vector2[numberOfParticles];

        // (1) First Verlet Step — Update Positions
        for (int currentParticle = 0; currentParticle < numberOfParticles; currentParticle++)
        {
            pos[currentParticle] += (0.5f * accOld[currentParticle] * (dt * dt)) + (vel[currentParticle] * dt);
        }
        

        for (int currentParticle = 0; currentParticle < numberOfParticles; currentParticle++)
        {
            for (int nextParticle = currentParticle + 1; nextParticle < numberOfParticles; nextParticle++)
            {
                Vector2 delta = pos[nextParticle] - pos[currentParticle];
                float distanceSquared = delta.sqrMagnitude;
                float collisionDistanceSquared = Diameter * Diameter;

                if (distanceSquared < collisionDistanceSquared)
                {
                    float distance = Mathf.Sqrt(distanceSquared);
                    Vector2 normal = delta / distance;
                    float penetrationDepth = Diameter - distance;

                    // (2.1) Hooke's Law: F = k * x
                    // F is the Force.
                    // k is the Spring Constant.
                    // x is the Penetration Depth.
                    // This step calculates the force exerted on each particle due to the collision, allowing for a more realistic response to collisions by applying forces that can cause particles to bounce off each other rather than just correcting their positions.
                    float forceMagnitude = springConstant * penetrationDepth;
                    Vector2 F = forceMagnitude * normal;

                    // (2.2) Apply equal and opposite forces to the colliding particles, ensuring that the total momentum of the system is conserved and that the particles respond appropriately to the collision by accelerating in opposite directions based on the calculated forces.
                    force[currentParticle] -= F;
                    force[nextParticle]    += F;
                }
            }
        }


        // (3.0) Wall Forces (Soft Walls)
        // Apply forces to particles that penetrate the walls of the box, simulating soft collisions with the boundaries by using Hooke's law to calculate the force based on the penetration depth.
        for (int currentParticle = 0; currentParticle < numberOfParticles; currentParticle++)
        {
            float particleRadius = Diameter / 2f;

            // (3.1) Left wall
            if (pos[currentParticle].x < particleRadius)
            {
                float overlap = particleRadius - pos[currentParticle].x;
                force[currentParticle].x += springConstant * overlap;
            }

            // (3.2) Right wall
            if (pos[currentParticle].x > Lx - particleRadius)
            {
                float overlap = pos[currentParticle].x - (Lx - particleRadius);
                force[currentParticle].x -= springConstant * overlap;
            }

            // (3.3) Bottom wall
            if (pos[currentParticle].y < particleRadius)
            {
                float overlap = particleRadius - pos[currentParticle].y;
                force[currentParticle].y += springConstant * overlap;
            }

            // (3.4) Top wall
            if (pos[currentParticle].y > Ly - particleRadius)
            {
                float overlap = pos[currentParticle].y - (Ly - particleRadius);
                force[currentParticle].y -= springConstant * overlap;
            }
        }


        // (4.0) Second Verlet Step — Update Velocities
        for (int currentParticle = 0; currentParticle < numberOfParticles; currentParticle++)
        {
            // (4.1) Calculate acceleration based on the net force and mass [F = m * a  =>  a = F / m] (Newton's Second Law)
            Vector2 acc = force[currentParticle] / mass;
            acc.y -= gravity;


            // (4.2) Update velocity using the average of the current and previous accelerations, ensuring that the Verlet integration method is correctly applied to update the velocities based on the forces acting on the particles during the current time step, allowing for accurate simulation of particle dynamics and interactions.
            vel[currentParticle] += 0.5f * (accOld[currentParticle] + acc) * dt;

            // (4.3) Store the current acceleration for use in the next time step, ensuring that the Verlet integration method can correctly calculate the new positions and velocities in the subsequent simulation step by keeping track of the previous acceleration values.
            accOld[currentParticle] = acc;

            // (4.4) If visualization is enabled and the particle has a corresponding visual object, update the position of the visual representation to match the new calculated position of the particle, allowing for real-time observation of the simulation's dynamics.
            if (visualize && particleObjects[currentParticle] != null)
            {
                particleObjects[currentParticle].transform.position = new Vector3(pos[currentParticle].x, pos[currentParticle].y, 0);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (pos == null || vel == null) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < pos.Length; i++)
        {
            Vector3 start = new Vector3(pos[i].x, pos[i].y, 0);
            Vector3 end = start + new Vector3(vel[i].x, vel[i].y, 0);
            Gizmos.DrawLine(start, end);
        }
    }

    void CreateBoxOutline()
    {
        float boxSize = boxMultiplier * Diameter;
        GameObject box = new GameObject("Simulation Box");
        boxRenderer = box.AddComponent<LineRenderer>();

        boxRenderer.positionCount = 5;
        boxRenderer.loop = false;
        boxRenderer.widthMultiplier = 0.05f;
        boxRenderer.useWorldSpace = true;

        boxRenderer.material = new Material(Shader.Find("Sprites/Default"));
        boxRenderer.startColor = Color.white;
        boxRenderer.endColor = Color.white;

        Vector3[] corners = new Vector3[5];
        corners[0] = new Vector3(0, 0, 0);
        corners[1] = new Vector3(boxSize, 0, 0);
        corners[2] = new Vector3(boxSize, boxSize, 0);
        corners[3] = new Vector3(0, boxSize, 0);
        corners[4] = corners[0];

        boxRenderer.SetPositions(corners);
    }
}
