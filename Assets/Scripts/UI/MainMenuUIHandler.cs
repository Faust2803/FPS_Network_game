using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenuUIHandler : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button entryButton;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("PlayerNickname"))
            inputField.text = PlayerPrefs.GetString("PlayerNickname");
    }

    private void OnEnable()
    {
        entryButton.onClick.AddListener(OnJoinGameClicked);
    }

    private void OnDisable()
    {
        entryButton.onClick.RemoveListener(OnJoinGameClicked);
    }

    public void OnJoinGameClicked()
    {
        PlayerPrefs.SetString("PlayerNickname", inputField.text);
        PlayerPrefs.Save();

        GameManager.instance.playerNickName = inputField.text;

        SceneManager.LoadScene("World1");
    }

}
