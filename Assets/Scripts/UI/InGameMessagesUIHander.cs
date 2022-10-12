using System.Collections;
using UnityEngine;
using TMPro;

public class InGameMessagesUIHander : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] _textMeshProUGUIs;
    
    [SerializeField] private TextMeshProUGUI _textTime;
    [SerializeField] private TextMeshProUGUI _textKill;
    [SerializeField] private TextMeshProUGUI _texDead;

    private Queue _messageQueue = new Queue();
    private int _time = int.MaxValue;

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

    public void OnGameTimeReceived(int time)
    {
        if (_time > (int)time)
        {
            _time = (int)time;

            var s = _time % 60;
            int m = _time / 60;
            var sec = s.ToString();
            var min = m.ToString();
            if (s < 10 )
            {
                sec = "0" + s;
            }
            if (m < 10 )
            {
                min = "0" + m;
            }
            _textTime.text = min+":"+sec;
        }
    }
    
    public void OnGameKillReceived(string value)
    {
        _textKill.text = value;
    }
    
    public void OnGameDeadReceived(string value)
    {
        _texDead.text = value;
    }
    
    
}
