using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nickname : MonoBehaviour
{
    // Start is called before the first frame update
    private Transform _mainCamera;
    
    void Start()
    {
        //_mainCamera = Camera.main.transform;

        var cam = Camera.allCameras;
        _mainCamera = cam[0].transform;
//Debug.Log("!!!!!!!!!!!!!!!!!!!!!"+cam.Length);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(_mainCamera);
        transform.rotation = _mainCamera.rotation;
    }
}
