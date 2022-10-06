using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    public ParticleSystem[] particles;
    void Start()
    {
        float longestTime = 0;
        foreach (var t in particles)
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
