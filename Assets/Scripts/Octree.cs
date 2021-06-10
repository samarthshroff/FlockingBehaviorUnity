using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class Octree 
{
    public OctreeNode root;

    public Octree( Vector3 boundsMin, Vector3 boundsMax, List<GameObject> boids, int boidsCapacity)
    {
        this.root = new OctreeNode(boundsMin, boundsMax, null, boids, boidsCapacity);
    }

    public void BuildTree()
    {
        root.BuildTree();
    }

    public List<NeighborMetaData> GetNeighbors(Vector3 boidPosition, OctreeNode currentNode, float awarenessRadius)
    {
        //traverse up until we reach a node that completely encloses the radius of awareness for the given boid
        var current = currentNode;
        var previous = currentNode.parent;

        while(previous != current)
        {
            if(current == null) return null;

            //if boid position is inside the box
            if(!current.region.Contains(boidPosition)) break;
            
            //if boid's awareness radius is inside the node then break. node found
            if ((boidPosition.x - awarenessRadius >= current.region.min.x) &&
                (boidPosition.x + awarenessRadius <= current.region.max.x) &&
                (boidPosition.y - awarenessRadius >= current.region.min.y) &&
                (boidPosition.y + awarenessRadius <= current.region.max.y) &&
                (boidPosition.z - awarenessRadius >= current.region.min.z) &&
                (boidPosition.z + awarenessRadius <= current.region.max.z))
            {
                break;
            }

            previous = current;
            current = current.parent;
        }

        return current.GetNeighborsFromChildren();
    }

    public OctreeNode GetOctreeNode(Vector3 boidPosition)
    {
        var current = root;
        var previous = current.parent;

        while( current != null && previous != current)
        {
            previous = current;

            for (int i = 0; i < OctreeNode.CHILDREN_CAPACITY;++i)
            {
                if(current.child[i] == null) continue;

                if(!current.child[i].region.Contains(boidPosition)) continue;

                //if the chlid's region contains the boid position then traverse that child's subtree
                current = current.child[i];
                break;
            }
        }

        return current;
    }

    public void UpdateBoidPositionInTree(GameObject boid, OctreeNode node)
    {
        var current = node;
        var previous = node.parent;

        Vector3 boidPosition = boid.transform.position;

        //traverse up to find the node that encloses the boid. this can be the node or parent of the node which encloses the boid
        while(current != null && !current.region.Contains(boidPosition))
        {
            previous = current;
            current = current.parent;
        }

        if(current == null && previous == root) current = root;

        //current is the node the encloses the boid, now traverse down it's subtree to find which node encloses the boid completely
        while( current != null && current != previous)
        {
            previous = current;

            for (int i = 0; i < OctreeNode.CHILDREN_CAPACITY;++i)
            {
                if(current.child[i] == null) continue;
                if(!current.child[i].region.Contains(boidPosition)) continue;

                current = current.child[i];
                break;
            }
        }

        if(node != current)
        {
            node.boids.Remove(boid);
            current.boids.Add(boid);
            current.BuildTree();
        }
    }
}
