using System.Collections;
using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HPHandler : NetworkBehaviour
{
     public const byte STARTING_HP = 5;
     
    [Networked(OnChanged = nameof(OnStateChanged))]
    public bool IsDead { get; set; }
    
    [SerializeField] private Color _uiOnHitColor;
    [SerializeField] private Image _uiOnHitImage;
    [SerializeField] private MeshRenderer _bodyMeshRenderer;
    [SerializeField] private GameObject _playerModel;
    [SerializeField] private GameObject _deathGameObjectPrefab;
    [Space]
    [SerializeField] private TextMeshProUGUI _textHP;
    [SerializeField] private Slider _hpSlider;
    [Space]
    [SerializeField] private bool _skipSettingStartValues = false;
    [SerializeField] private TextMeshProUGUI _textHPEnemy;
    [SerializeField] private Slider _HPEnemy;
   

    public bool SkipSettingStartValues
    {
        get => _skipSettingStartValues;
        set => _skipSettingStartValues = value;
    }
    
    [Networked(OnChanged = nameof(OnHPChanged))]
    private byte HP { get; set; }
    private bool _isInitialized = false;
    private Color _defaultMeshBodyColor;
    
    //Other components
    private HitboxRoot _hitboxRoot;
    private CharacterMovementHandler _characterMovementHandler;
    private NetworkInGameMessages _networkInGameMessages;
    private NetworkPlayer _networkPlayer;

    private void Awake()
    {
        _characterMovementHandler = GetComponent<CharacterMovementHandler>();
        _hitboxRoot = GetComponentInChildren<HitboxRoot>();
        _networkInGameMessages = GetComponent<NetworkInGameMessages>();
        _networkPlayer = GetComponent<NetworkPlayer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _HPEnemy.maxValue = STARTING_HP;
        _hpSlider.maxValue = STARTING_HP;
        if (!_skipSettingStartValues)
        {
            HP = STARTING_HP;
            IsDead = false;
        }

        _defaultMeshBodyColor = _bodyMeshRenderer.material.color;

        _isInitialized = true;
        SetHp();
    }

    IEnumerator OnHitCO()
    {
        _bodyMeshRenderer.material.color = Color.white;

        if (Object.HasInputAuthority)
            _uiOnHitImage.color = _uiOnHitColor;
        //SetHp();
        yield return new WaitForSeconds(0.2f);

        _bodyMeshRenderer.material.color = _defaultMeshBodyColor;
        
        if (Object.HasInputAuthority && !IsDead)
        {
            _uiOnHitImage.color = new Color(0, 0, 0, 0);
        }
        SetHp();
    }

    IEnumerator ServerReviveCO()
    {
        yield return new WaitForSeconds(2.0f);

        _characterMovementHandler.RequestRespawn();
    }


    //Function only called on the server
    public void OnTakeDamage(string damageCausedByPlayer, byte damage, NetworkPlayer networkPlayer)
    {
        //Only take damage while alive
        if (IsDead)
            return;

        if (damage > HP)
            damage = HP;
        
        HP -= damage;

        Debug.Log($"{Time.time} {transform.name} took damage got {HP} left ");

        //Player died
        if (HP == 0)
        {
            _networkInGameMessages.SendInGameRPCMessage(damageCausedByPlayer, $"Killed <b>{_networkPlayer.nickName.ToString()}</b>");

            Debug.Log($"{Time.time} {transform.name} died");

            StartCoroutine(ServerReviveCO());

            IsDead = true;
            
            _networkPlayer.Dead++;


            
            if (networkPlayer != _networkPlayer)
            {
                networkPlayer.Kill++;  
            }
        }
    }

    static void OnHPChanged(Changed<HPHandler> changed)
    {
        Debug.Log($"{Time.time} OnHPChanged value {changed.Behaviour.HP}");

        // byte newHP = changed.Behaviour.HP;
        //
        // //Load the old value
        // changed.LoadOld();
        //
        // byte oldHP = changed.Behaviour.HP;
        //
        // //Check if the HP has been decreased
        // if (newHP < oldHP)

        
        changed.Behaviour.OnHPReduced();
        
        
    }

    private void OnHPReduced()
    {
        if (!_isInitialized)
            return;

        StartCoroutine(OnHitCO());
    }

    static void OnStateChanged(Changed<HPHandler> changed)
    {
        Debug.Log($"{Time.time} OnStateChanged IsDead {changed.Behaviour.IsDead}");

        bool isDeadCurrent = changed.Behaviour.IsDead;

        //Load the old value
        changed.LoadOld();

        bool isDeadOld = changed.Behaviour.IsDead;

        //Handle on death for the player. Also check if the player was dead but is now alive in that case revive the player.
        if (isDeadCurrent)
            changed.Behaviour.OnDeath();
        else if (!isDeadCurrent && isDeadOld)
            changed.Behaviour.OnRevive();
    }

    private void OnDeath()
    {
        Debug.Log($"{Time.time} OnDeath");

        _playerModel.gameObject.SetActive(false);
        _hitboxRoot.HitboxRootActive = false;
        _characterMovementHandler.SetCharacterControllerEnabled(false);

        Instantiate(_deathGameObjectPrefab, transform.position, Quaternion.identity);
    }

    private void OnRevive()
    {
        Debug.Log($"{Time.time} OnRevive");

        if (Object.HasInputAuthority)
            _uiOnHitImage.color = new Color(0, 0, 0, 0);

        _playerModel.gameObject.SetActive(true);
        _hitboxRoot.HitboxRootActive = true;
        _characterMovementHandler.SetCharacterControllerEnabled(true);
    }

    public void OnRespawned()
    {
        //Reset variables
        HP = STARTING_HP;
        SetHp();
        IsDead = false;
    }

    private void SetHp()
    {
         _textHP.text = "HP "+HP;
         _textHPEnemy.text = HP.ToString();
         _HPEnemy.value = HP;
         _hpSlider.value = HP;
    }
}
