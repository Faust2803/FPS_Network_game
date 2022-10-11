using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.Serialization;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public static NetworkPlayer Local { get; set;}
    [SerializeField] private TextMeshProUGUI _playerNickNameTM;
    [SerializeField] private Transform _playerModel;
    [SerializeField] private LocalCameraHandler _localCameraHandler;
    [SerializeField] private GameObject _localUI;
    
    [SerializeField] private TextMeshProUGUI _textKill;
    [SerializeField] private TextMeshProUGUI _texDead;
        
    [SerializeField] private TextMeshProUGUI _textTime;
    
    [Networked(OnChanged = nameof(OnNickNameChanged))]
    public NetworkString<_16> nickName { get; set;}
    [Networked(OnChanged = nameof(OnKillChanged))]
    public byte Kill { get; set; }
    [Networked(OnChanged = nameof(OnDeadChanged))]
    public byte Dead { get; set; }
    [Networked(OnChanged = nameof(OnStateChanged))]
    public bool IsEnd { get; set; }
    [Networked(OnChanged = nameof(OnGameTimeChanged))]
    private TickTimer GameTime { get; set; }
    
    [Networked] public int token { get; set;}
    
    private bool _isPublicJoinMessageSent = false;
    //Other components
    private NetworkInGameMessages _networkInGameMessages;
    
    public LocalCameraHandler LocalCameraHandler { get; set;}

    void Awake()
    {
        _networkInGameMessages = GetComponent<NetworkInGameMessages>();
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;

            //Sets the layer of the local players model
            Utils.SetRenderLayerInChildren(_playerModel, LayerMask.NameToLayer("LocalPlayerModel"));

            //Disable main camera
            if (Camera.main != null)
                Camera.main.gameObject.SetActive(false);

            //Enable 1 audio listner
            AudioListener audioListener = GetComponentInChildren<AudioListener>(true);
            audioListener.enabled = true;


            //Enable the local camera
            _localCameraHandler.LocalCamera.enabled = true;

            //Detach camera if enabled
            _localCameraHandler.transform.parent = null;

            //Enable UI for local player
            _localUI.SetActive(true);

            RPC_SetNickName(GameManager.instance.playerNickName);

            Debug.Log("Spawned local player");
        }
        else
        {
            //Disable the local camera for remote players
            _localCameraHandler.LocalCamera.enabled = false;

            //Disable UI for remote player
            _localUI.SetActive(false);

            //Only 1 audio listner is allowed in the scene so disable remote players audio listner
            AudioListener audioListener = GetComponentInChildren<AudioListener>();
            audioListener.enabled = false;

            Debug.Log($"{Time.time} Spawned remote player");  
        }

        //Set the Player as a player object
        Runner.SetPlayerObject(Object.InputAuthority, Object);

        //Make it easier to tell which player is which.
        transform.name = $"P_{Object.Id}";
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (Object.HasStateAuthority)
        {
            if (Runner.TryGetPlayerObject(player, out NetworkObject playerLeftNetworkObject))
            {
                if (playerLeftNetworkObject == Object)
                    Local.GetComponent<NetworkInGameMessages>().SendInGameRPCMessage(playerLeftNetworkObject.GetComponent<NetworkPlayer>().nickName.ToString(), "left");
            }
               
        }


        if (player == Object.InputAuthority)
            Runner.Despawn(Object);

    }
    static void OnNickNameChanged(Changed<NetworkPlayer> changed)
    {
        Debug.Log($"{Time.time} OnHPChanged value {changed.Behaviour.nickName}");

        changed.Behaviour.OnNickNameChanged();
    }

    private void OnNickNameChanged()
    {
        Debug.Log($"Nickname changed for player to {nickName} for player {gameObject.name}");

        _playerNickNameTM.text = nickName.ToString();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickName(string nickName, RpcInfo info = default)
    {
        Debug.Log($"[RPC] SetNickName {nickName}");
        this.nickName = nickName;

        if(!_isPublicJoinMessageSent)
        {
            _networkInGameMessages.SendInGameRPCMessage(nickName, "joined");

            _isPublicJoinMessageSent = true;
        }
    }
    
    static void OnKillChanged(Changed<NetworkPlayer> changed)
    {
        Debug.Log($"{Time.time} OnKillChanged value {changed.Behaviour.Kill}");

        byte newKill = changed.Behaviour.Kill;

        //Load the old value
        changed.LoadOld();

        byte oldKill = changed.Behaviour.Kill;

        //Check if the Kill has been increased
        if (newKill > oldKill)
            changed.Behaviour.OnKillIncreased();
    }
        
    private void OnKillIncreased()
    {
        _textKill.text = Kill.ToString();
    }
        
    static void OnDeadChanged(Changed<NetworkPlayer> changed)
    {
        Debug.Log($"{Time.time} OnoldDeadChanged value {changed.Behaviour.Dead}");

        byte newDead = changed.Behaviour.Dead;

        //Load the old value
        changed.LoadOld();

        byte oldDead = changed.Behaviour.Dead;

        //Check if the Dead has been increased
        if (newDead > oldDead)
            changed.Behaviour.OnDeadIncreased();
    }
        
    private void OnDeadIncreased()
    {
        _texDead.text = Dead.ToString();
    }
    
    static void OnGameTimeChanged(Changed<NetworkPlayer> changed)
    {
        Debug.Log($"{Time.time} OnGameTimeChanged value {changed.Behaviour.GameTime}");

        TickTimer newGameTime = changed.Behaviour.GameTime;

        //Load the old value
        changed.LoadOld();

        TickTimer oldGameTime = changed.Behaviour.GameTime;

        //Check if the GameTime has been decreased
        if (newGameTime.TargetTick < oldGameTime.TargetTick)
            changed.Behaviour.OnGameTimeReduced();
    }
        
    private void OnGameTimeReduced()
    {
        _textTime.text = GameTime.RemainingTicks(Runner).ToString();
        IsEnd = true;
    }
        
    static void OnStateChanged(Changed<NetworkPlayer> changed)
    {
        Debug.Log($"{Time.time} OnStateChanged IsEnd");

        bool isEndCurrent = changed.Behaviour.IsEnd;

        //Load the old value
        changed.LoadOld();

        bool isEndOld = changed.Behaviour.IsEnd;

            
        if (isEndCurrent)
            changed.Behaviour.OnEnd();
            
    }
        
    private void OnEnd()
    {
        Debug.Log($"{Time.time} OnEndTime");
    }
    
    void OnDestroy()
    {
        //Get rid of the local camera if we get destroyed as a new one will be spawned with the new Network player
        if (_localCameraHandler != null)
            Destroy(_localCameraHandler.gameObject);
    }
}
