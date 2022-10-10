using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DestroyGameObject : MonoBehaviour
{
    [SerializeField] private float _lifeTime = 1.5f;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(_lifeTime);

        Destroy(gameObject);
    }
}
