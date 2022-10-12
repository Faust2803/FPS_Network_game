using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.Serialization;

public class WeaponHandler : NetworkBehaviour
{
    
    [Header("Prefabs")] 
    [SerializeField] private GrenadeHandler _grenadeHandler;
    [SerializeField] private RocketHandler _rockedHandler;
    
    [Header("ParticleSystem")] 
    [SerializeField] private ParticleSystem _fireParticleSystem;
    
    [Header("Aim")] 
    [SerializeField] private Transform _aimPoint;
    
    [Header("Collision")] 
    [SerializeField] private LayerMask _collisionLayers;

    [Networked(OnChanged = nameof(OnFireChanged))]
    public bool IsFiring { get; set; }
    
    private float _lastTimeFired = 0;
    private TickTimer _grenadeTimerFiredDelay = TickTimer.None;
    private TickTimer _rockedTimerFiredDelay = TickTimer.None;
    

    //Other components
    private HPHandler _hpHandler;
    private NetworkPlayer _networkPlayer;
    private NetworkObject _networkObject;
    
    private void Awake()
    {
        _hpHandler = GetComponent<HPHandler>();
        _networkPlayer = GetBehaviour<NetworkPlayer>();
        _networkObject = GetComponent<NetworkObject>();
    }

    public override void FixedUpdateNetwork()
    {
        if (_hpHandler.IsDead)
            return;

        //Get the input from the network
        if (GetInput(out NetworkInputData networkInputData))
        {
            if (networkInputData.isFireButtonPressed)
                Fire(networkInputData.aimForwardVector);
            
            if (networkInputData.isGrenadeFireButtonPressed)
                FireGrenade(networkInputData.aimForwardVector);
            
            if (networkInputData.isRocketFireButtonPressed)
                FireRocked(networkInputData.aimForwardVector);
        }
    }

    void Fire(Vector3 aimForwardVector)
    {
        //Limit fire rate
        if (Time.time - _lastTimeFired < 0.15f)
            return;

        StartCoroutine(FireEffectCO());

        Runner.LagCompensation.Raycast(_aimPoint.position, aimForwardVector, 100, Object.InputAuthority, out var hitinfo, _collisionLayers, HitOptions.IgnoreInputAuthority);

        float hitDistance = 100;
        bool isHitOtherPlayer = false;

        if (hitinfo.Distance > 0)
            hitDistance = hitinfo.Distance;

        if (hitinfo.Hitbox != null)
        {
            Debug.Log($"{Time.time} {transform.name} hit hitbox {hitinfo.Hitbox.transform.root.name}");

            if (Object.HasStateAuthority)
                hitinfo.Hitbox.transform.root.GetComponent<HPHandler>().OnTakeDamage(_networkPlayer.nickName.ToString(), 1, _networkPlayer);

            isHitOtherPlayer = true;

        }
        else if (hitinfo.Collider != null)
        {
            Debug.Log($"{Time.time} {transform.name} hit PhysX collider {hitinfo.Collider.transform.name}");
        }

        //Debug
        if (isHitOtherPlayer)
            Debug.DrawRay(_aimPoint.position, aimForwardVector * hitDistance, Color.red, 1);
        else Debug.DrawRay(_aimPoint.position, aimForwardVector * hitDistance, Color.green, 1);

        _lastTimeFired = Time.time;
    }
    
    
    void FireGrenade(Vector3 aimForwardVector)
    {
        if (_grenadeTimerFiredDelay.ExpiredOrNotRunning(Runner))
        {
            Runner.Spawn(_grenadeHandler,
                _aimPoint.position + aimForwardVector * 1.5F,
                Quaternion.LookRotation(aimForwardVector),
                Object.InputAuthority, (runner, spawnedGrenade) =>
                {
                    spawnedGrenade.GetComponent<GrenadeHandler>().Throw(aimForwardVector * 15,
                        Object.InputAuthority,
                        _networkPlayer.nickName.ToString(),
                        _networkPlayer
                    );
                });
            
            _grenadeTimerFiredDelay = TickTimer.CreateFromSeconds(Runner, 2.0F);
        }
    }
    
    void FireRocked(Vector3 aimForwardVector)
    {
        if (_rockedTimerFiredDelay.ExpiredOrNotRunning(Runner))
        {
            Runner.Spawn(_rockedHandler,
                _aimPoint.position + aimForwardVector * 1.5F,
                Quaternion.LookRotation(aimForwardVector),
                Object.InputAuthority, (runner, spawnedRocked) =>
                {
                    spawnedRocked.GetComponent<RocketHandler>().Fire(Object.InputAuthority,
                        _networkObject,
                        _networkPlayer.nickName.ToString(),
                        _networkPlayer
                    );
                });
            
            _rockedTimerFiredDelay = TickTimer.CreateFromSeconds(Runner, 3.5F);
        }
    }
    

    IEnumerator FireEffectCO()
    {
        IsFiring = true;

        _fireParticleSystem.Play();

        yield return new WaitForSeconds(0.09f);

        IsFiring = false;
    }


    static void OnFireChanged(Changed<WeaponHandler> changed)
    {
        //Debug.Log($"{Time.time} OnFireChanged value {changed.Behaviour.IsFiring}");

        bool isFiringCurrent = changed.Behaviour.IsFiring;

        //Load the old value
        changed.LoadOld();

        bool isFiringOld = changed.Behaviour.IsFiring;

        if (isFiringCurrent && !isFiringOld)
            changed.Behaviour.OnFireRemote();

    }

    void OnFireRemote()
    {
        if (!Object.HasInputAuthority)
            _fireParticleSystem.Play();
    }
}
