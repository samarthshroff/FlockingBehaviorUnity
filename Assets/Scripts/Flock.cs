using System.Collections.Generic;
using UnityEngine;

internal class Flock : MonoBehaviour
{
    private const float AWARENESS_RADIUS = 15.0f;
    private const float SEPARATION_RADIUS = 5.0f;

    [SerializeField, Tooltip("Total number of boids.")]
    private float TotalBoids;

    private List<GameObject> _boids;

    private GameObject _boidPrefab;
   
    public void Initialize(GameObject boidPrefab)
    {
        _boidPrefab = boidPrefab;
        _boids = new List<GameObject>();

        for (int i = 0; i < TotalBoids; ++i)
        {
            string name = $"Boid{i}";

            Vector3 position = new Vector3(Random.Range(-15.0f,15.0f), Random.Range(1.0f,8.0f), Random.Range(-15.0f,15.0f));

            var boid = Instantiate(boidPrefab, position, Quaternion.identity);
            boid.name = name;
            boid.SetActive(true);
            _boids.Add(boid);
        }
    }

    public void UpdateFlock(float deltaTime)
    {
        foreach(var boid in _boids)
        {
            var boidPosition = boid.transform.position;

            var alignment = Vector3.zero;
            var separation = Vector3.zero;
            var cohesion = Vector3.zero;
            var neighborCount = 0;
            var separationCount = 0;

            foreach (var neighbor in _boids)
            {
                if (boid == neighbor) continue;

                var neighborPosition = neighbor.transform.position;
                var distance = Vector3.Distance(neighborPosition, boidPosition);
                
                //check if the neighbor is visible by the boid
                if (distance < AWARENESS_RADIUS)
                {
                    alignment += neighbor.GetComponent<Boid>().GetVelocity();
                    cohesion += neighborPosition;
                    neighborCount++;

                    if (distance < SEPARATION_RADIUS)
                    {
                        separation -= (neighborPosition - boidPosition)/(distance*distance);
                        separationCount++;
                    }
                }
            }
            
            if(neighborCount > 0)
            {
                alignment /= neighborCount;
                cohesion /= neighborCount;
                
                if(separationCount > 0)
                {
                    separation /= separationCount;
                }
            }

            boid.GetComponent<Boid>().UpdateBoid( alignment, cohesion, separation);
        }
    }
}
