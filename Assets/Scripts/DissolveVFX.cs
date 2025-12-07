using System;
using System.Collections;
using UnityEngine;

public class DissolveVFX : MonoBehaviour
{

    Material _mat;
    int dissolveID = Shader.PropertyToID("_DissolveAmount");

    [SerializeField] float totalTime;
    void Awake()
    {
        _mat = GetComponent<SpriteRenderer>().material;
    }

    void Start()
    {
        StartCoroutine(IEDissolve(0, 1));
    }

    IEnumerator IEDissolve(float origin, float target)
    {
        float _elapsedime = 0;
        while (_elapsedime < totalTime)
        {
            _elapsedime += Time.deltaTime;
            float dissolveAmount = Mathf.Lerp(origin, target, _elapsedime / totalTime);
            
            _mat.SetFloat(dissolveID,dissolveAmount);

            yield return null;
        }
        Destroy(gameObject);
    }
}