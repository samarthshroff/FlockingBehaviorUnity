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
        _flock.Initialize(_boidPrefab);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.G))
        {
            _startAI = !_startAI;
        }
        if(_startAI)
        {
            //Debug.Log("Update started.");
            _flock.UpdateFlock(Time.deltaTime);
        }           
    }
}
