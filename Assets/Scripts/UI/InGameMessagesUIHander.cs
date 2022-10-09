using System.Collections;
using UnityEngine;
using TMPro;

public class InGameMessagesUIHander : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] _textMeshProUGUIs;

    private Queue _messageQueue = new Queue();

    public void OnGameMessageReceived(string message)
    {
        Debug.Log($"InGameMessagesUIHander {message}");

        _messageQueue.Enqueue(message);

        if (_messageQueue.Count > 3)
            _messageQueue.Dequeue();

        int queueIndex = 0;
        foreach (string messageInQueue in _messageQueue)
        {
            _textMeshProUGUIs[queueIndex].text = messageInQueue;
            queueIndex++;
        }

    }
}
