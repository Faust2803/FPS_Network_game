using UnityEngine;
using UnityEngine.UI;

public class CharacterInputHandler : MonoBehaviour
{
    [SerializeField] private VariableJoystick _variableJoystick;
    [SerializeField] private Button _fireButton;
    [SerializeField] private Button _jumpButton;
    [SerializeField] private Button _rockedutton;
    [SerializeField] private Button _grenadeButton;

    
    private Vector2 _moveInputVector = Vector2.zero;
    private Vector2 _viewInputVector = Vector2.zero;
    private bool _isJumpButtonPressed = false;
    private bool _isFireButtonPressed = false;
    private bool _isGrenadeFireButtonPressed = false;
    private bool _isRockedFireButtonPressed = false;

    //Other components
    private LocalCameraHandler _localCameraHandler;
    private CharacterMovementHandler _characterMovementHandler;

    private void Awake()
    {
        _localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        _characterMovementHandler = GetComponent<CharacterMovementHandler>();
    }

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        //View input
        PCInput(false);

#elif UNITY_STANDALONE_WIN
        PCInput(true);
#else
         PCInput(false);
#endif
    }

    private void PCInput(bool value)
    {
        if (value)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _variableJoystick.gameObject.SetActive(false);
            _fireButton.gameObject.SetActive(false);
            _jumpButton.gameObject.SetActive(false);
            _rockedutton.gameObject.SetActive(false);
            _grenadeButton.gameObject.SetActive(false);
        }
        else
        {
            _variableJoystick.gameObject.SetActive(true);
            _fireButton.gameObject.SetActive(true);
            _jumpButton.gameObject.SetActive(true);
            _rockedutton.gameObject.SetActive(true);
            _grenadeButton.gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!_characterMovementHandler.Object.HasInputAuthority)
            return;

        
        
        for (var i = 0; i < Input.touchCount; i++)
        {
            var ray = Camera.main.ScreenPointToRay (Input.GetTouch(i).position);
            if (Physics.Raycast (ray, out RaycastHit hitInfo)) {
                // Create a particle if hit
                Debug.Log("!!!!!!!   " +hitInfo.transform.gameObject.name);
            }
        }
        
#if UNITY_EDITOR
        if (Input.touchCount > 0 )
        {
            
        }
        
        //View input
        _viewInputVector.x = Input.GetAxis("Mouse X");
        _viewInputVector.y = Input.GetAxis("Mouse Y") * -1; //Invert the mouse look
        
        //Move input
        Vector3 direction = Vector3.forward * _variableJoystick.Vertical + Vector3.right * _variableJoystick.Horizontal;
        _moveInputVector.x = Input.GetAxis("Horizontal");
        _moveInputVector.y = Input.GetAxis("Vertical");
        if (direction != Vector3.zero)
        {
            _moveInputVector.x = direction.x;
            _moveInputVector.y = direction.z;
        }
        
        //Jump
        if (Input.GetKeyDown(KeyCode.Space))
            _isJumpButtonPressed = true;

        //Fire
        if (Input.GetMouseButtonDown(0))
            _isFireButtonPressed = true;
        if (Input.GetMouseButtonDown(1))
            _isRockedFireButtonPressed = true;

        if (Input.GetKeyDown(KeyCode.G))
            _isGrenadeFireButtonPressed = true;
        
#elif UNITY_STANDALONE_WIN

        //View input
        _viewInputVector.x = Input.GetAxis("Mouse X");
        _viewInputVector.y = Input.GetAxis("Mouse Y") * -1; //Invert the mouse look

        //Move input
        _moveInputVector.x = Input.GetAxis("Horizontal");
        _moveInputVector.y = Input.GetAxis("Vertical");

        //Jump
        if (Input.GetKeyDown(KeyCode.Space))
            _isJumpButtonPressed = true;

        //Fire
        if (Input.GetMouseButtonDown(0))
            _isFireButtonPressed = true;
        if (Input.GetMouseButtonDown(1))
            _isRockedFireButtonPressed = true;

        if (Input.GetKeyDown(KeyCode.G))
            _isGrenadeFireButtonPressed = true;
        
#else
        //View input
        _viewInputVector.x = Input.GetAxis("Mouse X");
        _viewInputVector.y = Input.GetAxis("Mouse Y") * -1; //Invert the mouse look
        
        Vector3 direction = Vector3.forward * _variableJoystick.Vertical + Vector3.right * _variableJoystick.Horizontal;
        _moveInputVector.x = direction.x;
         _moveInputVector.y = direction.z;
#endif
        //Set view
        _localCameraHandler.SetViewInputVector(_viewInputVector);

    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        //Aim data
        networkInputData.aimForwardVector = _localCameraHandler.transform.forward;

        //Move data
        networkInputData.movementInput = _moveInputVector;

        //Jump data
        networkInputData.isJumpPressed = _isJumpButtonPressed;

        //Fire data
        networkInputData.isFireButtonPressed = _isFireButtonPressed;
        networkInputData.isGrenadeFireButtonPressed = _isGrenadeFireButtonPressed;
        networkInputData.isRocketFireButtonPressed = _isRockedFireButtonPressed;

        //Reset variables now that we have read their states
        _isJumpButtonPressed = false;
        _isFireButtonPressed = false;
        _isGrenadeFireButtonPressed = false;
        _isRockedFireButtonPressed = false;
        return networkInputData;
    }
    
    private void OnJump()
    {
        _isJumpButtonPressed = true;
    }
    
    private void OnFire()
    {
        _isFireButtonPressed = true;
    }
    
    private void OnRockedFire()
    {
        _isRockedFireButtonPressed = true;
    }
    
    private void OnGrenadeFire()
    {
        _isGrenadeFireButtonPressed = true;
    }
    
    private void OnEnable()
    {
        _fireButton.onClick.AddListener(OnFire);
        _jumpButton.onClick.AddListener(OnJump);
        _rockedutton.onClick.AddListener(OnRockedFire);
        _grenadeButton.onClick.AddListener(OnGrenadeFire);
    }

    private void OnDisable()
    {
        _fireButton.onClick.RemoveListener(OnFire);
        _jumpButton.onClick.RemoveListener(OnJump);
        _rockedutton.onClick.RemoveListener(OnRockedFire);
        _grenadeButton.onClick.RemoveListener(OnGrenadeFire);
    }
}
