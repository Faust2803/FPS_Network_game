using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

public class MainMenuUIHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Button _entryButton;
    [SerializeField] private GameObject _legenda;

    private GameManager _gameManager;
    
    
    [Inject]
    private void Construct(GameManager gameManager)
    {
        _gameManager = gameManager;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("PlayerNickname"))
            _inputField.text = PlayerPrefs.GetString("PlayerNickname");
        
#if UNITY_EDITOR
        _legenda.SetActive(true);
#elif UNITY_STANDALONE_WIN
        _legenda.SetActive(true);
#else
       _legenda.SetActive(false);
#endif
    }

    private void OnEnable()
    {
        _entryButton.onClick.AddListener(OnJoinGameClicked);
    }

    private void OnDisable()
    {
        _entryButton.onClick.RemoveListener(OnJoinGameClicked);
    }

    public void OnJoinGameClicked()
    {
        PlayerPrefs.SetString("PlayerNickname", _inputField.text);
        PlayerPrefs.Save();

        _gameManager.playerNickName = _inputField.text;

        SceneManager.LoadScene("World1");
        //SceneManager.LoadScene("Test");
    }

}
