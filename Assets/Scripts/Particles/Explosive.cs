using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Explosive : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] _particles;
    void Start()
    {
        float longestTime = 0;
        foreach (var t in _particles)
        {
            
            var duration = t.main.duration;
            if (longestTime < duration)
            {
                longestTime = duration;
            }
        }
        StartCoroutine(Clear(longestTime));
    }

    IEnumerator Clear(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

}
