using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Header("References: ")]
    [SerializeField] private Gameplay _inputActions;
    [Space]
    [SerializeField] private PlayerMovement _movementLogic;
    [SerializeField] private PlayerCamera _cameraLogic;

    [Header("Movement Parameters: ")]
    [SerializeField] private float moveAcceleration = 50.0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _inputActions = new Gameplay();
        _inputActions.Enable();

        _cameraLogic.Initialize(_movementLogic.GetCameraTarget());
    }

    void OnDestroy()
    {
        if (_inputActions != null)
        {
            _inputActions.Player.Disable();
            _inputActions.Dispose();
        }
    }

    void Update()
    {
        var input = _inputActions.Player;
        var deltaTime = Time.deltaTime;
        

        var cameraInput = new CameraInput()
        {
            Look = input.Look.ReadValue<Vector2>()
        };
        _cameraLogic.UpdateRotation(cameraInput);
        

        var characterInput = new CharacterInput
        {
            Move = input.Move.ReadValue<Vector2>(),
            Rotation = _cameraLogic.transform.rotation
        };
        _movementLogic.UpdateInput(characterInput);
        _movementLogic.UpdateBody(deltaTime);
    }

    private void LateUpdate()
    {
        _cameraLogic.UpdatePosition(_movementLogic.GetCameraTarget());
    }

    void FixedUpdate()
    {
        _movementLogic.GatherIntrinsicAccelerations(moveAcceleration);
        _movementLogic.ApplyMovement();
    }
}
