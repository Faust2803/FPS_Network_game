using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

public class RocketHandler : NetworkBehaviour
{
    private const byte SECOND = 10;
    private const int ROCKED_SPEED = 23;
    
    [Header("Prefabs")] 
    [SerializeField] private GameObject _explosionParticleSystemPrefab;

    [Header("Collision detection")] 
    [SerializeField] private Transform _checkForImpactPoint;
    [SerializeField] private LayerMask _collisionLayers;
    
    private TickTimer _explodeTickTimer = TickTimer.None;

    private List<LagCompensatedHit> _hits = new List<LagCompensatedHit>();
    
    private PlayerRef _playerRef;
    private string _ownerPlayerName;
    private NetworkObject _fireBynetworkObject;
    
    private NetworkObject _networkObject;
    private NetworkRigidbody _networkRigidbody;
    private NetworkPlayer _networkPlayer;
    
    public void Fire(PlayerRef playerRef, string ownerPlayerName, NetworkPlayer networkPlayer, Vector3 throwForce, NetworkObject fireBynetworkObject)
    {
        _networkObject = GetComponent<NetworkObject>();
        _networkRigidbody = GetComponent<NetworkRigidbody>();
        
        //_networkRigidbody.Rigidbody.AddForce(throwForce, ForceMode.Force);
        
        _playerRef = playerRef;
        _ownerPlayerName = ownerPlayerName;
        _networkPlayer = networkPlayer;
        
        _fireBynetworkObject = fireBynetworkObject;

        _explodeTickTimer = TickTimer.CreateFromSeconds(Runner, SECOND);
    }

    public override void FixedUpdateNetwork()
    {
        transform.position += transform.forward * Runner.DeltaTime * ROCKED_SPEED;
        
         if (Runner.IsServer)
         {
            if (_explodeTickTimer.Expired(Runner))
            {
                Runner.Despawn(_networkObject);
                return;
            }
            
            var hitCounter =
                Runner.LagCompensation.OverlapSphere(_checkForImpactPoint.position,
                    0.5F,
                    _playerRef, 
                    _hits, 
                    _collisionLayers,
                    HitOptions.IncludePhysX
                );

            var isValidHit = false;
            if (hitCounter > 0)
            {
                isValidHit = true;
            }
            for (var i = 0; i < hitCounter; i++)
            {
                if (_hits[i].Hitbox != null)
                {
                    if (_hits[i].Hitbox.Root.GetBehaviour<NetworkObject>() == _fireBynetworkObject)
                    {
                        isValidHit = false;
                    }
                }
            }
            if (isValidHit)
            {
                hitCounter = Runner.LagCompensation.OverlapSphere(_checkForImpactPoint.position,
                    5,
                    _playerRef,
                    _hits,
                    _collisionLayers,
                    HitOptions.None);
                
                for (var i = 0; i < hitCounter; i++)
                {
                    HPHandler hpHandler = _hits[i].Hitbox.transform.root.GetComponent<HPHandler>();

                    if (hpHandler != null)
                    {
                        
                        hpHandler.OnTakeDamage(_ownerPlayerName, 5, _networkPlayer);
                    }
                }

                Runner.Despawn(_networkObject);
             
                _explodeTickTimer = TickTimer.None;
            }
        }
    }
    
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        //MeshRenderer grenadeMesh = GetComponentInChildren<MeshRenderer>();

        //Instantiate(_explosionParticleSystemPrefab, _checkForImpactPoint.position, Quaternion.identity);
        
        MeshRenderer grenadeMesh = GetComponentInChildren<MeshRenderer>();

        Instantiate(_explosionParticleSystemPrefab, grenadeMesh.transform.position, Quaternion.identity);
    }
}
