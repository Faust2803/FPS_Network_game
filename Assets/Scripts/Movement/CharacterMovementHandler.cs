using UnityEngine;
using Fusion;

public class CharacterMovementHandler : NetworkBehaviour
{
    private bool _isRespawnRequested = false;

    //Other components
    private NetworkCharacterControllerPrototypeCustom _networkCharacterControllerPrototypeCustom;
    private HPHandler _hpHandler;
    private NetworkInGameMessages _networkInGameMessages;
    private NetworkPlayer _networkPlayer;

    private void Awake()
    {
        _networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        _hpHandler = GetComponent<HPHandler>();
        _networkInGameMessages = GetComponent<NetworkInGameMessages>();
        _networkPlayer = GetComponent<NetworkPlayer>();
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            if (_isRespawnRequested)
            {
                Respawn();
                return;
            }

            //Don't update the clients position when they are dead
            if (_hpHandler.IsDead)
                return;
        }

        //Get the input from the network
        if (GetInput(out NetworkInputData networkInputData))
        {
            //Rotate the transform according to the client aim vector
            transform.forward = networkInputData.aimForwardVector;

            //Cancel out rotation on X axis as we don't want our character to tilt
            Quaternion rotation = transform.rotation;
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, rotation.eulerAngles.z);
            transform.rotation = rotation;

            //Move
            Vector3 moveDirection = transform.forward * networkInputData.movementInput.y + transform.right * networkInputData.movementInput.x;
            moveDirection.Normalize();

            _networkCharacterControllerPrototypeCustom.Move(moveDirection);

            //Jump
            if(networkInputData.isJumpPressed)
                _networkCharacterControllerPrototypeCustom.Jump();

            //Check if we've fallen off the world.
            CheckFallRespawn();
        }

    }

    void CheckFallRespawn()
    {
        if (transform.position.y < -12)
        {
            if (Object.HasStateAuthority)
            {
                Debug.Log($"{Time.time} Respawn due to fall outside of map at position {transform.position}");

                _networkInGameMessages.SendInGameRPCMessage(_networkPlayer.nickName.ToString(), "fell off the world");

                Respawn();
            }

        }
    }

    public void RequestRespawn()
    {
        _isRespawnRequested = true;
    }

    void Respawn()
    {
        _networkCharacterControllerPrototypeCustom.TeleportToPosition(Utils.GetRandomSpawnPoint());

        _hpHandler.OnRespawned();

        _isRespawnRequested = false;
    }

    public void SetCharacterControllerEnabled(bool isEnabled)
    {
        _networkCharacterControllerPrototypeCustom.Controller.enabled = isEnabled;
    }

}
