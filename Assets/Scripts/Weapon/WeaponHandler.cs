using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class WeaponHandler : NetworkBehaviour
{
    [Header("Prefabs")] 
    public GrenadeHandler grenadeHandler;
    public RocketHandler rockedHandler;
    
    [Header("ParticleSystem")] 
    public ParticleSystem fireParticleSystem;
    
    [Header("Aim")] 
    public Transform aimPoint;
    
    [Header("Collision")] 
    public LayerMask collisionLayers;

    [Networked(OnChanged = nameof(OnFireChanged))]
    public bool isFiring { get; set; }
    
    float lastTimeFired = 0;
    private TickTimer grenadeTimerFiredDelay = TickTimer.None;
    private TickTimer rockedTimerFiredDelay = TickTimer.None;
    

    //Other components
    private HPHandler hpHandler;
    private NetworkPlayer networkPlayer;
    private NetworkObject networkObject;
    
    private void Awake()
    {
        hpHandler = GetComponent<HPHandler>();
        networkPlayer = GetBehaviour<NetworkPlayer>();
        networkObject = GetComponent<NetworkObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void FixedUpdateNetwork()
    {
        if (hpHandler.isDead)
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
        if (Time.time - lastTimeFired < 0.15f)
            return;

        StartCoroutine(FireEffectCO());

        Runner.LagCompensation.Raycast(aimPoint.position, aimForwardVector, 100, Object.InputAuthority, out var hitinfo, collisionLayers, HitOptions.IgnoreInputAuthority);

        float hitDistance = 100;
        bool isHitOtherPlayer = false;

        if (hitinfo.Distance > 0)
            hitDistance = hitinfo.Distance;

        if (hitinfo.Hitbox != null)
        {
            Debug.Log($"{Time.time} {transform.name} hit hitbox {hitinfo.Hitbox.transform.root.name}");

            if (Object.HasStateAuthority)
                hitinfo.Hitbox.transform.root.GetComponent<HPHandler>().OnTakeDamage(networkPlayer.nickName.ToString());

            isHitOtherPlayer = true;

        }
        else if (hitinfo.Collider != null)
        {
            Debug.Log($"{Time.time} {transform.name} hit PhysX collider {hitinfo.Collider.transform.name}");
        }

        //Debug
        if (isHitOtherPlayer)
            Debug.DrawRay(aimPoint.position, aimForwardVector * hitDistance, Color.red, 1);
        else Debug.DrawRay(aimPoint.position, aimForwardVector * hitDistance, Color.green, 1);

        lastTimeFired = Time.time;
    }
    
    
    void FireGrenade(Vector3 aimForwardVector)
    {
        if (grenadeTimerFiredDelay.ExpiredOrNotRunning(Runner))
        {
            Runner.Spawn(grenadeHandler,
                aimPoint.position + aimForwardVector * 1.5F,
                Quaternion.LookRotation(aimForwardVector),
                Object.InputAuthority, (runner, spwnedGrenade) =>
                {
                    spwnedGrenade.GetComponent<GrenadeHandler>().Throw(aimForwardVector * 15,
                        Object.InputAuthority,
                        networkPlayer.nickName.ToString()
                    );
                });
            
            grenadeTimerFiredDelay = TickTimer.CreateFromSeconds(Runner, 1.0F);
        }
    }
    
    void FireRocked(Vector3 aimForwardVector)
    {
        if (rockedTimerFiredDelay.ExpiredOrNotRunning(Runner))
        {
            Runner.Spawn(rockedHandler,
                aimPoint.position + aimForwardVector * 1.5F,
                Quaternion.LookRotation(aimForwardVector),
                Object.InputAuthority, (runner, spwnedRocked) =>
                {
                    spwnedRocked.GetComponent<RocketHandler>().Fire(Object.InputAuthority,
                        networkObject,
                        networkPlayer.nickName.ToString()
                    );
                });
            
            rockedTimerFiredDelay = TickTimer.CreateFromSeconds(Runner, 3.5F);
        }
    }
    

    IEnumerator FireEffectCO()
    {
        isFiring = true;

        fireParticleSystem.Play();

        yield return new WaitForSeconds(0.09f);

        isFiring = false;
    }


    static void OnFireChanged(Changed<WeaponHandler> changed)
    {
        //Debug.Log($"{Time.time} OnFireChanged value {changed.Behaviour.isFiring}");

        bool isFiringCurrent = changed.Behaviour.isFiring;

        //Load the old value
        changed.LoadOld();

        bool isFiringOld = changed.Behaviour.isFiring;

        if (isFiringCurrent && !isFiringOld)
            changed.Behaviour.OnFireRemote();

    }

    void OnFireRemote()
    {
        if (!Object.HasInputAuthority)
            fireParticleSystem.Play();
    }
}
