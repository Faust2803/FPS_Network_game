using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class LocalCameraHandler : MonoBehaviour
{
    [SerializeField] private Transform cameraAnchorPoint;
    public Camera LocalCamera {  get; private set;}
    //Input
    private Vector2 _viewInput;

    //Rotation
    private float _cameraRotationX = 0;
    private float _cameraRotationY = 0;

    //Other components
    private NetworkCharacterControllerPrototypeCustom _networkCharacterControllerPrototypeCustom;
   
    [Inject] private GameManager _gameManager;
    
    private void Awake()
    {
        LocalCamera = GetComponent<Camera>();
        _networkCharacterControllerPrototypeCustom = GetComponentInParent<NetworkCharacterControllerPrototypeCustom>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _cameraRotationX = _gameManager.cameraViewRotation.x;
        _cameraRotationY = _gameManager.cameraViewRotation.y;
    }

    void LateUpdate()
    {
        if (cameraAnchorPoint == null)
            return;

        if (!LocalCamera.enabled)
            return;

        //Move the camera to the position of the player
        LocalCamera.transform.position = cameraAnchorPoint.position;

        //Calculate rotation
        _cameraRotationX += _viewInput.y * Time.deltaTime * _networkCharacterControllerPrototypeCustom.viewUpDownRotationSpeed;
        _cameraRotationX = Mathf.Clamp(_cameraRotationX, -90, 90);

        _cameraRotationY += _viewInput.x * Time.deltaTime * _networkCharacterControllerPrototypeCustom.rotationSpeed;

        //Apply rotation
        LocalCamera.transform.rotation = Quaternion.Euler(_cameraRotationX, _cameraRotationY, 0);

    }
    public void SetViewInputVector(Vector2 viewInput)
    {
        this._viewInput = viewInput;
    }

    private void OnDestroy()
    {
        if (_cameraRotationX != 0 && _cameraRotationY != 0)
        {
            _gameManager.cameraViewRotation.x = _cameraRotationX;
            _gameManager.cameraViewRotation.y = _cameraRotationY;
        }
    }
}
