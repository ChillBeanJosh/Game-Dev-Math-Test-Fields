using System.Collections.Generic;
using UnityEngine;

public class VehicleInputController : MonoBehaviour
{
    [System.Serializable]
    public struct KeyMapping
    {
        public KeyCode forwardKey;
        public KeyCode backwardKey;
        public KeyCode leftKey;
        public KeyCode rightKey;
    }

    [Header("Global Default Mapping")]
    public KeyMapping defaultMapping = new KeyMapping
    {
        forwardKey = KeyCode.W,
        backwardKey = KeyCode.S,
        leftKey = KeyCode.A,
        rightKey = KeyCode.D
    };

    [Header("Per-Car Configuration Overrides")]
    [Tooltip("Assign unique control sets for specific cars in the simulation chain (e.g., P1, P2, P3). If empty or unassigned, the default mapping is used.")]
    public List<KeyMapping> carOverrides = new List<KeyMapping>();

    // Internal input buffer to capture frame-accurate keystrokes inside Update
    private List<Vector2> processedInputs = new List<Vector2>();
    private CarMotion carMotionComponent;

    private void Awake()
    {
        carMotionComponent = GetComponent<CarMotion>();
    }

    private void Update()
    {
        // Establish the target length based on the active CarMotion configuration
        int targetCount = 1;
        if (carMotionComponent != null)
        {
            targetCount = Mathf.Max(carMotionComponent.chainCount, carMotionComponent.fullCars.Count);
        }
        targetCount = Mathf.Max(targetCount, carOverrides.Count);

        // Maintain the state matrix buffer size
        while (processedInputs.Count < targetCount)
        {
            processedInputs.Add(Vector2.zero);
        }

        // Poll keyboard state transforms for every system instance
        for (int i = 0; i < processedInputs.Count; i++)
        {
            KeyMapping currentMapping = (carOverrides != null && i < carOverrides.Count) ? carOverrides[i] : defaultMapping;

            float forward = 0f;
            float lateral = 0f;

            if (Input.GetKey(currentMapping.forwardKey)) forward += 1f;
            if (Input.GetKey(currentMapping.backwardKey)) forward -= 1f;
            if (Input.GetKey(currentMapping.rightKey)) lateral += 1f;
            if (Input.GetKey(currentMapping.leftKey)) lateral -= 1f;

            // x = Lateral (Steering), y = Forward (Throttle/Brake)
            processedInputs[i] = new Vector2(lateral, forward);
        }
    }

    /// <returns>Returns Vector2 where X = Lateral Input (-1 to 1) and Y = Forward Input (-1 to 1)</returns>
    public Vector2 GetInput(int carIndex)
    {
        if (carIndex >= 0 && carIndex < processedInputs.Count)
        {
            return processedInputs[carIndex];
        }
        return Vector2.zero;
    }
}
