using UnityEngine;

public class velocityCollisionDetection : MonoBehaviour
{
    [Header("Particle Information: ")]
    public GameObject particlePrefab;
    public int numberOfParticles = 10;
    public float Diameter = 1f;
    public float mass = 1f;         // Mass of each particle, used for calculating collision responses 
    [Space]
    [Tooltip("Coefficient of restitution (bounciness) for collisions, where 1 means perfectly elastic and 0 means perfectly inelastic")]
    [Range(0f, 1f)] public float restitution = 1f; 

    [Header("Box Information: ")]
    public float boxSize = 10f;
    [Space]

    [Header("Time Parameters: ")]
    [Range(0.1f, 10f)] public float timeScale = 1f;    // Time scale factor to control the speed of the simulation.
    private LineRenderer boxRenderer;
    [Space]

    [Header("Collision Information: ")]
    Vector2[] pos;
    Vector2[] vel;                  // Array to store the velocity of each particle, which will be updated based on collisions and used to move the particles in the simulation.
    GameObject[] particles;

    private void Start()
    {
        CreateBoxOutline();

        pos = new Vector2[numberOfParticles];
        vel = new Vector2[numberOfParticles];  // Initialize the velocity array to store the velocity of each particle. The size of the array is determined by the number of particles specified in the inspector.
        particles = new GameObject[numberOfParticles];

        for (int i = 0; i < numberOfParticles; i++)
        {
            pos[i] = new Vector2
                (
                    Random.Range(Diameter, boxSize - Diameter),
                    Random.Range(Diameter, boxSize - Diameter)
                );

            vel[i] = Random.insideUnitCircle; // Generates a random x and y value for the velocity of each particle, where the values are between -1 and 1. This gives each particle a random initial velocity in a random direction.

            particles[i] = Instantiate
                (
                    particlePrefab,
                    new Vector3(pos[i].x, pos[i].y, 0),
                    Quaternion.identity
                );

            particles[i].transform.localScale = Vector3.one * Diameter;
        }
    }

    void CreateBoxOutline()
    {
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

    private void Update()
    {
        // (1) Calculate the time step for the simulation by multiplying Time.deltaTime (the time elapsed since the last frame) with the timeScale variable. This allows for controlling the speed of the simulation, where a timeScale of 1 means normal speed, values greater than 1 will speed up the simulation, and values less than 1 will slow it down.
        float deltaTime = Time.deltaTime * timeScale;

        // (2) Update the position of each particle based on its velocity and the calculated time step. The new position is determined by adding the product of velocity and deltaTime to the current position. After updating the position, the WallCollisions method is called for each particle to check for collisions with the walls of the box and adjust the velocity accordingly.
        for (int i = 0; i < numberOfParticles; i++)
        {
            pos[i] += vel[i] * deltaTime; 
            WallCollisions(i);
        }

        // (3) After updating the positions and handling wall collisions, the ParticleCollisions method is called to check for collisions between particles and update their velocities based on the collision response.
        ParticleCollisions();

        // (4) Update the position of each particle GameObject in the scene to match the new positions calculated in the simulation. This is done by iterating through each particle and setting its transform.position to the corresponding position in the pos array, ensuring that the visual representation of the particles in the scene matches their simulated positions.
        for (int i = 0; i < numberOfParticles; i++)
        {
            particles[i].transform.position = new Vector3(pos[i].x, pos[i].y, 0);
        }
    }

    void ParticleCollisions()
    {
        for (int currentParticle = 0; currentParticle < numberOfParticles; currentParticle++)
        {
            for (int nextParticle = currentParticle + 1; nextParticle < numberOfParticles; nextParticle++)
            {
                Vector2 delta = pos[nextParticle] - pos[currentParticle];
                float distanceSquared = delta.sqrMagnitude;
                float collisionDistanceSquared = Diameter * Diameter;

                if (distanceSquared < collisionDistanceSquared)
                {
                    // (1) Calculate the actual distance between the two particles by taking the square root of the distanceSquared variable. The normal vector is then calculated by dividing the delta vector by the distance, which gives a unit vector pointing from the current particle to the next particle. This normal vector will be used to determine the direction of the collision response.
                    float distance = Mathf.Sqrt(distanceSquared);
                    Vector2 normal = delta / distance;

                    // (2) Calculate the relative velocity between the two particles by subtracting the velocity of the current particle from the velocity of the next particle. The velocity along the normal is then calculated by taking the dot product of the relative velocity and the normal vector. This value indicates how much of the relative velocity is directed along the line connecting the centers of the two particles, which is crucial for determining how they will respond to the collision.
                    Vector2 relativeVelocity = vel[nextParticle] - vel[currentParticle];
                    float velocityAlongNormal = Vector2.Dot(relativeVelocity, normal);

                    // (3) If the velocity along the normal is greater than 0, it means that the particles are moving away from each other and there is no need to resolve the collision. In this case, the loop continues to the next iteration without applying any collision response, as the particles will not collide if they are already moving apart.
                    if (velocityAlongNormal > 0) continue;

                    // (4) Calculate the impulse scalar using the formula for elastic collisions, which takes into account the restitution coefficient and the relative velocity along the normal. The impulse is then calculated by multiplying the impulse scalar with the normal vector. This impulse will be applied to both particles to change their velocities according to the collision response, where the current particle's velocity is decreased by the impulse divided by its mass, and the next particle's velocity is increased by the same amount.
                    float impulseScalar = -(1 + restitution) * velocityAlongNormal / (2f / mass);
                    Vector2 impulse = impulseScalar * normal;

                    vel[currentParticle] -= impulse / mass;
                    vel[nextParticle]    += impulse / mass;

                    // (5) To prevent particles from overlapping after a collision, the penetration depth is calculated by subtracting the distance between the particles from the diameter. A correction vector is then calculated by multiplying the normal vector by half of the penetration depth (to distribute the correction equally between both particles). The positions of both particles are then adjusted by subtracting the correction from the current particle's position and adding it to the next particle's position, ensuring that they are no longer overlapping after the collision response is applied.
                    float penetrationDepth = Diameter - distance;
                    Vector2 correction = normal * (penetrationDepth * 0.5f);

                    pos[currentParticle] -= correction;
                    pos[nextParticle]    += correction;
                }
            }
        }
    }



    void WallCollisions(int currentParticle)
    {
        // (1) Check for collisions with the vertical walls of the box. 
        if ((pos[currentParticle].x < Diameter / 2f) || (pos[currentParticle].x > boxSize - Diameter / 2f))
        {
            // If a collision is detected with the vertical walls, the x-component of the velocity for the current particle is multiplied by -restitution. This effectively reverses the direction of the velocity along the x-axis and scales it by the restitution coefficient, which determines how bouncy the collision is. A restitution value of 1 means a perfectly elastic collision (no energy loss), while a value less than 1 means some energy is lost in the collision, resulting in a less bouncy response.
            vel[currentParticle].x *= -restitution;
        }

        // (2) Check for collisions with the horizontal walls of the box.
        if ((pos[currentParticle].y < Diameter / 2f) || (pos[currentParticle].y > boxSize - Diameter / 2f))
        {
            // If a collision is detected with the horizontal walls, the y-component of the velocity for the current particle is multiplied by -restitution. Similar to the vertical wall collision, this reverses the direction of the velocity along the y-axis and scales it by the restitution coefficient to determine the bounciness of the collision.
            vel[currentParticle].y *= -restitution;
        }
    }


    private void OnDrawGizmos()
    {
        if (pos == null || vel == null) return;

        for (int i = 0; i < numberOfParticles; i++)
        {
            Gizmos.color = Color.green;

            // (1) Start Point [Particle Position]
            Vector3 start = new Vector3(pos[i].x, pos[i].y, 0);

            // (2) End Point [Particle Position + Velocity]
            Vector3 end = start + new Vector3(vel[i].x, vel[i].y, 0);

            // (3) Line between the start and end points to visualize the velocity of each particle.
            Gizmos.DrawLine(start, end);
        }
    }


    /* TLDR:
     * - This script simulates a 2D particle system with collision detection and response, where particles can collide with each other and the walls of a box.
     * - The script includes parameters for particle properties (mass, restitution), box properties (size, time scale), and handles the physics of collisions using basic principles of mechanics.
     * - The OnDrawGizmos method is used to visualize the velocity of each particle in the Unity editor, allowing for a better understanding of the simulation dynamics.
     * 
     * NOTE: 
     * - This is using discrete collision detection
     * - For more accurate collision detection, especially at higher velocities, continuous collision detection techniques can be implemented to prevent tunneling issues where particles may pass through each other or the walls without detecting a collision.
     * - Additionally, more complex collision responses can be implemented to account for factors such as friction, angular momentum, and more realistic physics interactions between particles.
     * - The current implementation assumes that all particles have the same mass and diameter, but this can be modified to allow for varying properties among particles for a more diverse simulation.
     */
}
