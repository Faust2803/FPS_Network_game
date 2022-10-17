using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

public class GrenadeHandler : NetworkBehaviour
{
   private const byte SECOND = 2;
   
   [Header("Prefabs")] 
   [SerializeField] private GameObject _explosionParticleSystemPrefab;
   [Header("Particles")] 
   [SerializeField] private ParticleSystem _landingParticleSystem;
   [Header("Collision detection")] 
   [SerializeField] private LayerMask _collisionLayers;
   
   private PlayerRef _playerRef;
   private string _ownerPlayerName;
   
   private TickTimer _explodeTickTimer = TickTimer.None;

   private List<LagCompensatedHit> _hits = new List<LagCompensatedHit>();

   private NetworkObject _networkObject;
   private NetworkRigidbody _networkRigidbody;
   private NetworkPlayer _networkPlayer;
   
   public void Throw(PlayerRef playerRef, string ownerPlayerName, NetworkPlayer networkPlayer, Vector3 throwForce)
   {
      _networkObject = GetComponent<NetworkObject>();
      _networkRigidbody = GetComponent<NetworkRigidbody>();
      
      _networkRigidbody.Rigidbody.AddForce(throwForce, ForceMode.Impulse);

      _playerRef = playerRef;
      _ownerPlayerName = ownerPlayerName;
      _networkPlayer = networkPlayer;
      
      _explodeTickTimer = TickTimer.CreateFromSeconds(Runner, SECOND);
   }

   public override void FixedUpdateNetwork()
   {
      if (Runner.IsServer)
      {
         
         if (_explodeTickTimer.Expired(Runner))
         {
            Debug.LogWarning("!!!!!"+_explodeTickTimer);
            int hitCounter =
               Runner.LagCompensation.OverlapSphere(transform.position,
                  10,
                  _playerRef, 
                  _hits, 
                  _collisionLayers
                  );

            for (var i = 0; i < hitCounter; i++)
            {
               HPHandler hpHandler = _hits[i].Hitbox.transform.root.GetComponent<HPHandler>();

               if (hpHandler != null)
               {
                  hpHandler.OnTakeDamage(_ownerPlayerName, 3, _networkPlayer);
               }
            }
           
            Runner.Despawn(_networkObject);
            
            _explodeTickTimer = TickTimer.None;
         }
      }
   }

   public override void Despawned(NetworkRunner runner, bool hasState)
   {
      MeshRenderer grenadeMesh = GetComponentInChildren<MeshRenderer>();

      Instantiate(_explosionParticleSystemPrefab, grenadeMesh.transform.position, Quaternion.identity);
   }

   private void OnCollisionEnter(Collision collision)
   {
      _landingParticleSystem.Play();
   }
}
