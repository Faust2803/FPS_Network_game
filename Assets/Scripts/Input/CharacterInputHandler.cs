using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CharacterInputHandler : MonoBehaviour
{
    [SerializeField] private VariableJoystick _joystickM;
    [SerializeField] private VariableJoystick _joystickR;
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
    
    private enum InputsType
    {
        Unity,
        Window,
        Mobile
    }

    private InputsType _myInput;

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
        _myInput = InputsType.Unity;
#elif UNITY_STANDALONE_WIN
       _myInput = InputsType.Window;
#else
         _myInput = InputsType.Mobile;
#endif
        PCInput();
    }

    private void PCInput()
    {
        if (_myInput != InputsType.Window)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _joystickM.gameObject.SetActive(false);
            _joystickR.gameObject.SetActive(false);
            _fireButton.gameObject.SetActive(false);
            _jumpButton.gameObject.SetActive(false);
            _rockedutton.gameObject.SetActive(false);
            _grenadeButton.gameObject.SetActive(false);
        }
        else if  (_myInput != InputsType.Unity)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _joystickM.gameObject.SetActive(false);
            _joystickR.gameObject.SetActive(false);
            _fireButton.gameObject.SetActive(false);
            _jumpButton.gameObject.SetActive(false);
            _rockedutton.gameObject.SetActive(false);
            _grenadeButton.gameObject.SetActive(false);
        }
        else
        {
            _joystickM.gameObject.SetActive(true);
            _joystickR.gameObject.SetActive(true);
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

        if (_myInput != InputsType.Window)
        {
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
        }
        else if  (_myInput != InputsType.Unity)
        {
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
        }
        else
        {
            //View input
            Vector3 direction1 = Vector3.forward * _joystickR.Vertical + Vector3.right * _joystickR.Horizontal;
            _viewInputVector.x = direction1.x;
            _viewInputVector.y = direction1.z * -1; //Invert the mouse look
        
            Vector3 direction = Vector3.forward * _joystickM.Vertical + Vector3.right * _joystickM.Horizontal;
            _moveInputVector.x = direction.x;
            _moveInputVector.y = direction.z;
        }
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
