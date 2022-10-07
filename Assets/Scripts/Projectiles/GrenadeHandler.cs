using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class GrenadeHandler : NetworkBehaviour
{
   private const byte second = 2;

   [Header("Prefabs")] 
   public GameObject explosionParticleSystemPrefab;
   [Header("Particles")] 
   public ParticleSystem landingParticleSystem;
   [Header("Collision detection")] 
   public LayerMask collisionLayers;
   
   private PlayerRef throwByPlayerRef;
   private string throwByPlayerName;
   
   TickTimer explodeTickTimer = TickTimer.None;

   private List<LagCompensatedHit> hits = new List<LagCompensatedHit>();

   private NetworkObject networkObject;
   private NetworkRigidbody networkRigidbody;
   
   public void Throw(Vector3 throwForce, PlayerRef throwByPlayerRef, string throwByPlayerName)
   {
      networkObject = GetComponent<NetworkObject>();
      networkRigidbody = GetComponent<NetworkRigidbody>();
      
      networkRigidbody.Rigidbody.AddForce(throwForce, ForceMode.Impulse);

      this.throwByPlayerRef = throwByPlayerRef;
      this.throwByPlayerName = throwByPlayerName;

      explodeTickTimer = TickTimer.CreateFromSeconds(Runner, second);
      
   }

   public override void FixedUpdateNetwork()
   {
      // if (Object.HasInputAuthority)
      // {
      if (explodeTickTimer.Expired(Runner))
         {
            int hitCounter =
               Runner.LagCompensation.OverlapSphere(transform.position,
                  10,
                  throwByPlayerRef, 
                  hits, 
                  collisionLayers
                  );

            for (var i = 0; i < hitCounter; i++)
            {
               HPHandler hpHandler = hits[i].Hitbox.transform.root.GetComponent<HPHandler>();

               if (hpHandler != null)
               {
                  hpHandler.OnTakeDamage(throwByPlayerName, 3);
               }
            }
            
            Runner.Despawn(networkObject);
            
            explodeTickTimer = TickTimer.None;
         }
      //}
   }

   public override void Despawned(NetworkRunner runner, bool hasState)
   {
      MeshRenderer grenadeMesh = GetComponentInChildren<MeshRenderer>();

      Instantiate(explosionParticleSystemPrefab, grenadeMesh.transform.position, Quaternion.identity);
   }

   private void OnCollisionEnter(Collision collision)
   {
      landingParticleSystem.Play();
   }
}
