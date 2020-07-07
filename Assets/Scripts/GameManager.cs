using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _boidPrefab;

    private Flock _flock;

    private bool _startAI;

    void Awake()
    {
        _flock = gameObject.GetComponent<Flock>();
    }

    private void Start() 
    {
        _flock.Intialize(_boidPrefab);
    }

    // Update is called once per frame
    void Update()
    {
        //_flock.UpdateFlock(Time.deltaTime);
    }
}
