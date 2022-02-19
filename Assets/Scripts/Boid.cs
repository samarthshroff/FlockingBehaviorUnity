using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    //private readonly Vector3 zeroVec = new Vector3(0, 0, 0);
    private Vector3 _velocity;
    private Vector3 _acceleration;
    private Vector3 _extent;

    [SerializeField]
    private float MaxSpeed;

    [SerializeField, Tooltip("Length of Ray cast (in cms) for checking forward collision with World Static objects, so that the boid can turn accordingly.")]
    private float FarSightness;

    [SerializeField]
    private float MaxSteeringForce;
    
    [SerializeField]
    private float AlignmentWeight = 1.0f;

    [SerializeField]
    private float CohesionWeight = 1.0f;

    [SerializeField]
    private float SeparationWeight = 1.0f;

    //private Vector3 _newPosAfterWallCollision;
    private LineRenderer lineRenderer;

    private float _maxSteeringForceSquared;
    
    // Start is called before the first frame update
    private void Start() 
    {
        var angle = Random.Range(-80.0f, 80.0f);

        _velocity.x = Mathf.Cos(Mathf.Deg2Rad * angle);
        _velocity.y = Mathf.Sin(Mathf.Deg2Rad * angle);

        _velocity /= _velocity.magnitude;
        _velocity *= MaxSpeed;

        _maxSteeringForceSquared = MaxSteeringForce * MaxSteeringForce;
        
        transform.localRotation = Quaternion.LookRotation(_velocity);
        _extent = GetComponent<Renderer>().bounds.extents;
    }

    public Vector3 GetVelocity()
    {
        return _velocity;
    }

    // Update is called once per frame
    public void UpdateBoid(Vector3 alignmentSteering , Vector3 cohesionSteering , Vector3 separationSteering )
    {
        if(alignmentSteering != Vector3.zero)
        {
            alignmentSteering.Normalize();
            alignmentSteering *= MaxSpeed;

            alignmentSteering = alignmentSteering - _velocity;

            alignmentSteering = Vector3.ClampMagnitude(alignmentSteering, MaxSteeringForce) * AlignmentWeight;
        }

        if(cohesionSteering != Vector3.zero)
        {
            cohesionSteering -= transform.position;
            cohesionSteering.Normalize();
            cohesionSteering *= MaxSpeed;
            cohesionSteering -= _velocity;

            cohesionSteering = Vector3.ClampMagnitude(cohesionSteering, MaxSteeringForce) * CohesionWeight;
        }

        if(separationSteering != Vector3.zero)
        {
            separationSteering.Normalize();
            separationSteering *= MaxSpeed;
            separationSteering -= _velocity;

            separationSteering = Vector3.ClampMagnitude(separationSteering, MaxSteeringForce) * SeparationWeight;
        }

        _acceleration = alignmentSteering + cohesionSteering + separationSteering;
        
        _velocity += _acceleration;

        if(_velocity.magnitude > MaxSpeed)
        {
            _velocity /= _velocity.magnitude;
            _velocity *= MaxSpeed;
        }

        gameObject.transform.position = transform.position + (_velocity * (1.0f/30.0f));
        transform.localRotation = Quaternion.LookRotation(_velocity);
        _acceleration = Vector3.zero;
    }

    private void FixedUpdate() 
    {
        var origin = transform.position;

        if (Physics.Raycast(origin, transform.forward, out var hit, FarSightness, 1 << 8))
        {
            // To find the new velocity
            // The dot product is to find the angle between the velocity and the normal of the plane the boid is going to collide with
            // we multiply it by 2 because ray of incidence = ray of reflection
            // The result has to move in the direction of the plane's normal thus we multiply the result by the normal
            // _velocity - the above result to get the subtraction vector which is the direction and the magnitude in which the boiid has to move
            _velocity = _velocity - 2 * (Vector3.Dot(_velocity, hit.normal)) * hit.normal;
        }
    }

}
