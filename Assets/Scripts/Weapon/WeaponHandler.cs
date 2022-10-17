using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.Serialization;

public class WeaponHandler : NetworkBehaviour
{
    private const float  HIT_DISTANCE = 100;
    private const float  FIRE_TIMEOUT = 0.25F;
    private const float  ROCKET_TIMEOUT = 1F;
    private const float  GRENADE_TIMEOUT = 1F;
    
    
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
            {
                Fire(networkInputData.aimForwardVector);
            }
            else
            {
                Runner.LagCompensation.Raycast(_aimPoint.position, networkInputData.aimForwardVector, HIT_DISTANCE, Object.InputAuthority, out var hitinfo, _collisionLayers, HitOptions.IgnoreInputAuthority);
                if (hitinfo.Hitbox != null)
                {
                    AutoFire(hitinfo);
                }
                else if (hitinfo.Collider != null)
                {
                    Debug.Log($"{Time.time} {transform.name} hit PhysX collider {hitinfo.Collider.transform.name}");
                }
            }

            if (networkInputData.isGrenadeFireButtonPressed)
                FireGrenade(networkInputData.aimForwardVector);
            
            if (networkInputData.isRocketFireButtonPressed)
                FireRocked(networkInputData.aimForwardVector);
        }
    }
    
    void AutoFire(LagCompensatedHit hitinfo)
    {
        //Limit fire rate
        if (Time.time - _lastTimeFired < FIRE_TIMEOUT)
            return;

        StartCoroutine(FireEffectCO());

        Debug.Log($"{Time.time} {transform.name} hit hitbox {hitinfo.Hitbox.transform.root.name}");

        if (Object.HasStateAuthority)
            hitinfo.Hitbox.transform.root.GetComponent<HPHandler>().OnTakeDamage(_networkPlayer.nickName.ToString(), 1, _networkPlayer);

        _lastTimeFired = Time.time;
    }

    void Fire(Vector3 aimForwardVector)
    {
        //Limit fire rate
        if (Time.time - _lastTimeFired < FIRE_TIMEOUT)
            return;

        StartCoroutine(FireEffectCO());

        Runner.LagCompensation.Raycast(_aimPoint.position, aimForwardVector, HIT_DISTANCE, Object.InputAuthority, out var hitinfo, _collisionLayers, HitOptions.IgnoreInputAuthority);


        bool isHitOtherPlayer = false;

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
            Debug.DrawRay(_aimPoint.position, aimForwardVector * HIT_DISTANCE, Color.red, 1);
        else Debug.DrawRay(_aimPoint.position, aimForwardVector * HIT_DISTANCE, Color.green, 1);

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
                    spawnedGrenade.GetComponent<GrenadeHandler>().Throw(
                        Object.InputAuthority,
                        _networkPlayer.nickName.ToString(),
                        _networkPlayer, 
                        aimForwardVector * 15
                    );
                });
            
            _grenadeTimerFiredDelay = TickTimer.CreateFromSeconds(Runner, GRENADE_TIMEOUT);
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
                    spawnedRocked.GetComponent<RocketHandler>().Fire(
                        Object.InputAuthority,
                        _networkPlayer.nickName.ToString(),
                        _networkPlayer,
                        aimForwardVector * 15,
                        _networkObject
                    );
                });
            
            _rockedTimerFiredDelay = TickTimer.CreateFromSeconds(Runner, ROCKET_TIMEOUT);
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
