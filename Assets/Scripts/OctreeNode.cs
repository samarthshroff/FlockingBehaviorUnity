using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class OctreeNode 
{
    public const int CHILDREN_CAPACITY = 8;

    public List<GameObject> boids;
    public OctreeNode parent;

    public OctreeNode[] child;

    public Bounds region;

    public int boidsCapacity;

    public OctreeNode()
    {
        this.parent = null;
    }

    public OctreeNode(Vector3 boundsMin, Vector3 boundsMax, OctreeNode parent, List<GameObject> boids, int boidsCapacity)
    {
        child = new OctreeNode[CHILDREN_CAPACITY];

        this.region = CreateRegion(boundsMin, boundsMax);
        this.parent = parent;
        this.boids = boids;
        this.boidsCapacity = boidsCapacity;
    }

    public void BuildTree()
    {
        //if true then this is the leaf node which will hold not more than 100 boids in it's space
        if(boids.Count <= boidsCapacity)
        {
            return;
        }

        var center = region.center;
        
        Bounds[] octant = new Bounds[CHILDREN_CAPACITY];

        //if the dimensions of this node is greater then the min required then this is not a leaf node; subdivide

        octant[0] = CreateRegion(region.min, center);
        octant[1] = CreateRegion(new Vector3(center.x, region.min.y, region.min.z), new Vector3(region.max.x, center.y, center.z));
        octant[2] = CreateRegion(new Vector3(center.x, region.min.y, center.z), new Vector3(region.max.x, center.y, region.max.z));
        octant[3] = CreateRegion(new Vector3(region.min.x, region.min.y, center.z), new Vector3(center.x, center.y, region.max.z));
        octant[4] = CreateRegion(new Vector3(region.min.x, center.y, region.min.z), new Vector3(center.x, region.max.y, center.z));
        octant[5] = CreateRegion(new Vector3(center.x, center.y, region.min.z), new Vector3(region.max.x, region.max.y, center.z));
        octant[6] = CreateRegion(center, region.max);
        octant[7] = CreateRegion(new Vector3(region.min.x, center.y, center.z), new Vector3(center.x, region.max.y, region.max.z));

        //list of boids to be distributed amongst the children
        List<GameObject>[] boidDistribution = new List<GameObject>[CHILDREN_CAPACITY];
        for (int i = 0; i < CHILDREN_CAPACITY;++i)
        {
            boidDistribution[i] = new List<GameObject>();
        }

        //list of boids that are to be moved down to the children and needs to be removed from this node
        List<GameObject> boidsToRemove = new List<GameObject>();

        //boids distribution
        foreach( var boid in boids)
        {
            Vector3 boidPosition = boid.transform.position;

            for (int i = 0; i < CHILDREN_CAPACITY;++i)
            {
                if(octant[i].Contains(boidPosition))
                {
                    boidDistribution[i].Add(boid);
                    boidsToRemove.Add(boid);
                }
            }
        }

        //remove boids from current node
        foreach(var boid in boidsToRemove)
            boids.Remove(boid);

        //distribute the boids amongst children
        for (int i = 0; i < CHILDREN_CAPACITY;++i)
        {
            if(child[i] == null)
            {
                child[i] = new OctreeNode(octant[i].min,octant[i].max, this, boidDistribution[i], boidsCapacity);
            }
            else
            {
                foreach( var boid in boidDistribution[i])
                {
                    child[i].boids.Add(boid);
                }
            }
            child[i].BuildTree();
        }
    }

    public List<NeighborMetaData> GetNeighborsFromChildren()
    {
        Stack<OctreeNode> dfsStack = new Stack<OctreeNode>();

        List<NeighborMetaData> neighbors = new List<NeighborMetaData>();

        dfsStack.Push(this);

        while(dfsStack.Count > 0)
        {
            var visited = dfsStack.Pop();

            if(visited == null) continue;

            //visit current and store neighbors
            foreach(var neighbor in visited.boids)
            {
                if(neighbor != null)
                {
                    //neighbors.Add(neighbor);
                    neighbors.Add(new NeighborMetaData
                    {
                        name = neighbor.name,
                        position = neighbor.transform.position,
                        velocity = neighbor.GetComponent<Boid>().GetVelocity()
                    });
                } 
            }

            foreach(var child in visited.child)
            {
                if(child != null) dfsStack.Push(child);
            }
        }

        return (neighbors.Count > 0 )?neighbors:null;
    }

    private Bounds CreateRegion(Vector3 boundsMin, Vector3 boundsMax)
    {
        Bounds bound = new Bounds();
        bound.SetMinMax(boundsMin, boundsMax);
        return bound;
    }
}

public class NeighborMetaData
{
    public string name;
    public Vector3 position;
    public Vector3 velocity;
}
