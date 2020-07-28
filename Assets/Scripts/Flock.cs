using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class Flock : MonoBehaviour
{
    private const float AWARENESS_RADIUS = 4.0f;
    private const float ALIGNMENT_RADIUS = 4.0f;
    private const float COHESION_RADIUS = 4.0f;
    private const float SEPARATION_RADIUS = 1.0f;

    [SerializeField, Tooltip("Other boids which fall inside this FOV are neighbors. FOV is in degrees.")]
    private float BoidFOV;

    private List<GameObject> _boids;

    private GameObject _boidPrefab;

    private Octree _tree;
    public void Intialize(GameObject boidPrefab)
    {
        _boidPrefab = boidPrefab;
        _boids = new List<GameObject>();

        for (int i = 0; i < 100; ++i)
        {
            string name = $"Boid{i}";

            Vector3 position = new Vector3(Random.Range(-15.0f,15.0f), Random.Range(1.0f,8.0f), Random.Range(-15.0f,15.0f));

            var boid = Instantiate(boidPrefab, position, Quaternion.identity);
            boid.name = name;
            boid.SetActive(true);
            _boids.Add(boid);
        }

        _tree = new Octree(new Vector3(-20.0f, 0.0f, -20.0f),new Vector3(20.0f, 10.0f, 20.0f) , new List<GameObject>(_boids), 10);
        _tree.BuildTree();
        int j = 0;
    }

    public void UpdateFlock(float deltaTime)
    {
        if(_boids.Count == 1)
        {
            _boids[0].GetComponent<Boid>().UpdateBoid(deltaTime, Vector3.zero,Vector3.zero,Vector3.zero);
            return;
        }
        foreach(var boid in _boids)
        {
            var boidPosition = boid.transform.position;
            var name = boid.name;
            var currentNode = _tree.GetOctreeNode(boidPosition);

            if(currentNode == null)
            {
                Debug.Log($"current node is null for boid:: {name}");
            }

            var neighbors = _tree.GetNeighbors(boidPosition, currentNode, AWARENESS_RADIUS);

            Vector3 alignment = Vector3.zero;
            Vector3 separation = Vector3.zero;
            Vector3 cohesion = Vector3.zero;
            int alignmentCount = 0;
            int cohesionCount = 0;
            int separationCount = 0;
            
            if(neighbors != null)
            {
                foreach (var neighbor in neighbors)
                {
                    if (string.Compare(boid.name, neighbor.name) == 0) continue;

                    var neighborPosition = neighbor.position;
                    float distance = Vector3.Distance(boidPosition, neighborPosition);

                    //check if the neighbor is visible by the boid
                    var angle = Mathf.Rad2Deg * (Mathf.Acos(Vector3.Dot(boid.transform.forward, Vector3.Normalize(neighborPosition - boidPosition))));

                    if (angle <= BoidFOV && distance <= AWARENESS_RADIUS)
                    {
                        if (distance <= ALIGNMENT_RADIUS)
                        {
                            alignment += neighbor.velocity;
                            alignmentCount++;
                        }
                        if (distance <= COHESION_RADIUS)
                        {
                            cohesion += neighborPosition;
                            cohesionCount++;
                        }
                        if (distance <= SEPARATION_RADIUS)
                        {
                            var separationVector = boidPosition - neighborPosition;
                            separationVector.Normalize();
                            separationVector /= distance;
                            separation += separationVector;
                            separationCount++;
                        }
                    }
                }
            }
            
            if(alignmentCount > 0)
            {
                alignment /= alignmentCount;
            }

            if(cohesionCount > 0)
            {
                cohesion /= cohesionCount;
            }

            if(separationCount > 0)
            {
                separation /= separationCount;
            }

            boid.GetComponent<Boid>().UpdateBoid(deltaTime, alignment, cohesion, separation);

            _tree.UpdateBoidPositionInTree(boid, currentNode);
        }
    }
}
