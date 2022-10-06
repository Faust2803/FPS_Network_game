using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class RocketHandler : NetworkBehaviour
{
    private const byte second = 5;
    
    [Header("Prefabs")] 
    public GameObject explosionParticleSystemPrefab;

    [Header("Collision detection")] 
    public Transform checkForImpactPoint;
    public LayerMask collisionLayers;
    
    TickTimer explodeTickTimer = TickTimer.None;

    private int rockedSpeed = 20;
    
    private List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
    
    private PlayerRef fireByPlayerRef;
    private string fireByPlayerName;
    private NetworkObject fireBynetworkObject;
    
    private NetworkObject networkObject;
    
    public void Fire(PlayerRef fireByPlayerRef, NetworkObject fireBynetworkObject,  string fireByPlayerName)
    {
        
        this.fireByPlayerRef = fireByPlayerRef;
        this.fireByPlayerName = fireByPlayerName;
        this.fireBynetworkObject = fireBynetworkObject;
        networkObject = GetComponent<NetworkObject>();

        explodeTickTimer = TickTimer.CreateFromSeconds(Runner, second);
    }

    public override void FixedUpdateNetwork()
    {
        transform.position += transform.forward * Runner.DeltaTime * rockedSpeed;
        
        if (Object.HasInputAuthority)
        {
            if (explodeTickTimer.Expired(Runner))
            {
                Runner.Despawn(networkObject);
                return;
            }
            
            var hitCounter =
                Runner.LagCompensation.OverlapSphere(checkForImpactPoint.position,
                    0.5F,
                    fireByPlayerRef, 
                    hits, 
                    collisionLayers,
                    HitOptions.IncludePhysX
                );

            var isValidHit = false;

            
            if (hitCounter > 0)
            {
                isValidHit = true;
            }
               
            
            for (var i = 0; i < hitCounter; i++)
            {
                if (hits[i].Hitbox != null)
                {
                    if (hits[i].Hitbox.Root.GetBehaviour<NetworkObject>() == fireBynetworkObject)
                    {
                        isValidHit = false;
                    }
                }
            }
            
            
            if (isValidHit)
            {
                hitCounter = Runner.LagCompensation.OverlapSphere(checkForImpactPoint.position,
                    5,
                    fireByPlayerRef,
                    hits,
                    collisionLayers,
                    HitOptions.None);
                
                for (var i = 0; i < hitCounter; i++)
                {
                    HPHandler hpHandler = hits[i].Hitbox.transform.root.GetComponent<HPHandler>();

                    if (hpHandler != null)
                    {
                        hpHandler.OnTakeDamage(fireByPlayerName, 5);
                    }
                }  
                
                Runner.Despawn(networkObject);
            }
        }
    }
    
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        //MeshRenderer grenadeMesh = GetComponentInChildren<MeshRenderer>();

        Instantiate(explosionParticleSystemPrefab, checkForImpactPoint.position, Quaternion.identity);
    }
}
