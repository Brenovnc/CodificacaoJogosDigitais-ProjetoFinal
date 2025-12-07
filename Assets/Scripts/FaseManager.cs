using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class FaseManager : MonoBehaviour
{
    [SerializeField] string NomeFase;

    void OnTriggerEnter2D(Collider2D other)
    {
        SceneManager.LoadScene(NomeFase);
    }
}
