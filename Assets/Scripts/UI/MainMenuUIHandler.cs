using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainMenuUIHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Button _entryButton;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("PlayerNickname"))
            _inputField.text = PlayerPrefs.GetString("PlayerNickname");
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

        GameManager.instance.playerNickName = _inputField.text;

        SceneManager.LoadScene("World1");
    }

}
