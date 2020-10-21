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

    //private Vector3 _newPosAfterWallCollision;
    //private LineRenderer lineRenderer;
    // Start is called before the first frame update
    private void Start() 
    {
        var angle = Random.Range(-40.0f, 40.0f);

        _velocity.x = Mathf.Cos(Mathf.Deg2Rad * angle);
        _velocity.y = Mathf.Sin(Mathf.Deg2Rad * angle);

        _velocity /= _velocity.magnitude;
        _velocity *= MaxSpeed;

        transform.rotation = Quaternion.LookRotation(_velocity);

        // lineRenderer = GetComponent<LineRenderer>();
        // lineRenderer.positionCount = 2;
        // lineRenderer.startWidth = lineRenderer.endWidth = 0.25f;
        // lineRenderer.SetPosition(0, transform.position);
        // lineRenderer.SetPosition(1, transform.forward * FarSightness + transform.position);
  
        _extent = GetComponent<Renderer>().bounds.extents;
    }

    public Vector3 GetVelocity()
    {
        return _velocity;
    }

    // Update is called once per frame
    public void UpdateBoid(float deltaTime, Vector3 alignmentSteering , Vector3 cohesionSteering , Vector3 separationSteering )
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
            _velocity /= _velocity.magnitude;
            _velocity *= MaxSpeed;
        }
        
        //Debug.Log($"velocity for {gameObject.name} is {_velocity}");

        transform.rotation = Quaternion.LookRotation(_velocity);
        transform.position = transform.position + (_velocity * deltaTime);// Translate(_velocity * deltaTime, Space.World);
        
        // if(_newPosAfterWallCollision == Vector3.zero)
        //     transform.Translate(_velocity * deltaTime, Space.World);
        // else
        //     transform.position = Vector3.Slerp(transform.position, _newPosAfterWallCollision, 0.5f);

        // lineRenderer.SetPosition(0, transform.position);
        // lineRenderer.SetPosition(1, transform.forward * FarSightness + transform.position);

        //transform.SetPositionAndRotation(location, Quaternion.LookRotation(location+transform.position));
        // transform.position = location;
        // transform.rotation = Quaternion.LookRotation(_velocity);
        //_newPosAfterWallCollision = Vector3.zero;
        _acceleration = Vector3.zero;
    }

    // private void OnGUI() {
    //     var forwardVector = transform.forward;
    //     var newVector = new Vector3(_extent.x * forwardVector.x, _extent.y * forwardVector.y, _extent.z * forwardVector.z);
    //     var origin = transform.position + newVector;
    //     var end = origin * FarSightness + forwardVector;
        
    // }

    private void FixedUpdate() 
    {
        var forwardVector = transform.forward;
        var newVector = new Vector3(_extent.x * forwardVector.x, _extent.y * forwardVector.y, _extent.z * forwardVector.z);
        var origin = transform.position;// + newVector;
        var end = origin * 1.0f + forwardVector;

        Debug.DrawLine(origin, end, Color.red);

        if (Physics.Raycast(origin, forwardVector, out var hit, FarSightness, 1 << 8))
        {
            //to find the new velocity
            // the dot product is to find the angle between the velocity and the normal of the plane the boid is going to collide with
            // we multiply it by 2 because ray of incidence = ray of reflection
            // the above result has to move in the direction of the plane's normal thus we multiply the result by the normal
            // _velocity - above result to et the subtraction vector which is the direction and the magnitude in which the boiid has to move
            //_velocity = _velocity - 2*Vector3.Scale(Vector3.Scale(_velocity, hit.normal),hit.normal);
            _velocity = _velocity - 2 * (Vector3.Dot(_velocity, hit.normal)) * hit.normal;
            //_newPosAfterWallCollision = transform.position + _velocity + 2 * (Vector3.Dot(-1 * _velocity, hit.normal) - Vector3.Dot((transform.position - hit.point), hit.normal)) * hit.normal;
        }
    }

    // private void OnCollisionEnter(Collision other) 
    // {
    //     if (string.Compare(other.gameObject.tag, "Walls") == 0)
    //     {
    //         Debug.Log($"this boid {gameObject.name} has collied with {other.gameObject.name} at boid position {transform.position}");
    //         _velocity = _velocity - 2 * Vector3.Scale(Vector3.Scale(_velocity, other.contacts[0].normal), other.contacts[0].normal);
    //         _velocity = _velocity - 2 * (Vector3.Dot(_velocity, other.contacts[0].normal)) * other.contacts[0].normal;
    //     }
    // }

    // public class SteeringVectors
    // {
    //     public Vector3 alignment = Vector3.zero;
    //     public Vector3 separation = Vector3.zero;
    //     public Vector3 cohesion = Vector3.zero;

    //     public SteeringVectors()
    //     {

    //     }

    //     public SteeringVectors(Vector3 alignment, Vector3 cohesion, Vector3 separation)
    //     {
    //         this.alignment = alignment;
    //         this.cohesion = cohesion;
    //         this.separation = separation;
    //     }
    // }
}
