using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartTrigger : MonoBehaviour
{
    private GameManager manager;
    private RacketScript racket;
    private void Awake()
    {
        manager = FindAnyObjectByType<GameManager>();
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            StartCoroutine(CountDown(other.gameObject));
           
            Debug.Log("It is starting!!!");
        }
    }
    IEnumerator CountDown(GameObject other)
    {
        this.GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(5);
        other.GetComponent<RacketScript>().isStarting = true;
    }
}
