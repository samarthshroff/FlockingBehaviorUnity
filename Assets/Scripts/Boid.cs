using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    private Vector3 _velocity;
    private Vector3 _acceleration;
    private Vector3 _extent;

    [SerializeField]
    private float MaxSpeed;

    [SerializeField, Tooltip("Length of Ray cast (in cms) for checking forward collision with World Static objects, so that the boid can turn accordingly.")]
    private float FarSightness;

    [SerializeField]
    private float MaxSteeringForce;

    // Start is called before the first frame update
    void Start()
    {
        var angle = Random.Range(-40.0f, 40.0f);

        _velocity.x = Mathf.Cos(Mathf.Deg2Rad * angle);
        _velocity.x = Mathf.Sin(Mathf.Deg2Rad * angle);

        _velocity.Normalize();
        _velocity *= MaxSpeed;

        transform.rotation = Quaternion.LookRotation(_velocity);        

        _extent = GetComponent<Renderer>().bounds.extents;
    }

    public Vector3 GetVelocity()
    {
        return _velocity;
    }

    // Update is called once per frame
    public void UpdateBoid(Vector3 alignmentSteering, Vector3 cohesionSteering, Vector3 separationSteering, float deltaTime)
    {
        if(alignmentSteering != Vector3.zero)
        {
            alignmentSteering.Normalize();
            alignmentSteering *= MaxSpeed;

            alignmentSteering = alignmentSteering - _velocity;

            if(alignmentSteering.sqrMagnitude >= (MaxSteeringForce*MaxSteeringForce))
            {
                alignmentSteering.Normalize();
                alignmentSteering *= MaxSteeringForce;
            }
        }

        if(cohesionSteering != Vector3.zero)
        {
            cohesionSteering = cohesionSteering - transform.position;
            cohesionSteering.Normalize();
            cohesionSteering *= MaxSpeed;
            cohesionSteering = cohesionSteering - _velocity;

            if(cohesionSteering.sqrMagnitude > (MaxSteeringForce*MaxSteeringForce))
            {
                cohesionSteering.Normalize();
                cohesionSteering *= MaxSteeringForce;
            }
        }

        if(separationSteering != Vector3.zero)
        {
            separationSteering.Normalize();
            separationSteering *= MaxSpeed;
            separationSteering = separationSteering - _velocity;

            if (separationSteering.sqrMagnitude > (MaxSteeringForce * MaxSteeringForce))
            {
                separationSteering.Normalize();
                separationSteering *= MaxSteeringForce;
            }
            separationSteering *= 1.5f;
        }

        _acceleration = alignmentSteering + cohesionSteering + separationSteering;

        _velocity = _velocity + _acceleration;

        if(_velocity.magnitude > MaxSpeed)
        {
            _velocity.Normalize();
            _velocity *= MaxSpeed;
        }

        Vector3 location = transform.position;
        location += _velocity;

        transform.position = location;
        transform.rotation = Quaternion.LookRotation(_velocity);

        _acceleration = Vector3.zero;

        var forwardVector = transform.forward;
        var newVector = new Vector3( _extent.x*forwardVector.x, _extent.y*forwardVector.y,_extent.z*forwardVector.z );
        var origin = transform.position + newVector;
        var end = origin * FarSightness + forwardVector;

        if(Physics.Raycast(origin,forwardVector, out var hit, FarSightness, 1<<8))
        {
            //to find the new velocity
            // the dot product is to find the angle between the velocity and the normal of the plane the boid is going to collide with
            // we multiply it by 2 because ray of incidence = ray of reflection
            // the above result has to move in the direction of the plane's normal thus we multiply the result by the normal
            // _velocity - above result to et the subtraction vector which is the direction and the magnitude in which the boiid has to move
            _velocity = _velocity - 2 * ( Vector3.Dot(_velocity, hit.normal)) * hit.normal;
        }

    }
}
