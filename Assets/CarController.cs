using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Driving Settings")]
    public float maxMotorTorque = 1500f;
    public float topSpeed = 50f;
    public float accelerationRate = 10f;
    public float brakingForce = 3000f;
    public float drag = 0.5f;

    [Header("Steering Settings")]
    public float maxSteeringAngle = 30f;
    public float steeringSensitivity = 0.6f;
    public float steerSpeed = 5f;

    [Header("Drifting Settings")]
    public bool enableDrift = true;
    public float driftGripLoss = 0.5f;
    public float driftAngularDrag = 0.5f;
    public float normalAngularDrag = 2f;
    public KeyCode driftKey = KeyCode.Space;

    [Header("Boost Settings")]
    public float boostMultiplier = 1.5f;
    public KeyCode boostKey = KeyCode.LeftShift;

    [Header("Explosion Settings")]
    public GameObject explosionPrefab;
    public KeyCode explodeKey = KeyCode.E;
    private bool isDestroyed = false;

    [Header("Reset Settings")]
    public KeyCode resetKey = KeyCode.R;

    private Rigidbody rb;
    private float steerInput;
    private float currentSpeed;
    private bool isDrifting;

    // Store starting position and rotation
    private Vector3 startingPosition;
    private Quaternion startingRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.6f, 0);
        rb.drag = drag;
        rb.angularDrag = normalAngularDrag;

        // Save starting position and rotation
        startingPosition = transform.position;
        startingRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        // Always allow reset
        if (Input.GetKeyDown(resetKey))
        {
            ResetCar();
        }
        if (isDestroyed) return;

        HandleInput();
        HandleSteering();
        HandleDrive();
        HandleDrift();

        if (Input.GetKeyDown(explodeKey))
        {
            ExplodeCar();
        }

        if (Input.GetKeyDown(resetKey))
        {
            ResetCar();
        }
    }

    void HandleInput()
    {
        steerInput = Input.GetAxis("Horizontal");
        currentSpeed = rb.velocity.magnitude * 3.6f; // Convert to km/h
        isDrifting = enableDrift && Input.GetKey(driftKey);
    }

    void HandleSteering()
    {
        float speedFactor = Mathf.Clamp01(currentSpeed / topSpeed);
        float steerAngle = maxSteeringAngle * (1 - speedFactor * steeringSensitivity);
        float targetSteer = steerInput * steerAngle;

        Quaternion steerRotation = Quaternion.Euler(0, targetSteer * Time.fixedDeltaTime * steerSpeed, 0);
        rb.MoveRotation(rb.rotation * steerRotation);
    }

    void HandleDrive()
    {
        float motorInput = Input.GetAxis("Vertical");

        float speedPercent = Mathf.Clamp01(currentSpeed / topSpeed);
        float targetForce = motorInput * maxMotorTorque * (1 - speedPercent) * Time.fixedDeltaTime;

        if (Input.GetKey(boostKey))
        {
            targetForce *= boostMultiplier;
        }

        rb.AddForce(transform.forward * targetForce, ForceMode.Force);

        if (Mathf.Abs(motorInput) < 0.1f)
        {
            rb.velocity *= 0.99f; // Auto-brake when not accelerating
        }
    }

    void HandleDrift()
    {
        if (!isDrifting)
        {
            rb.angularDrag = normalAngularDrag;
            return;
        }

        rb.angularDrag = driftAngularDrag;

        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        localVelocity.x *= driftGripLoss;
        rb.velocity = transform.TransformDirection(localVelocity);
    }

    void ExplodeCar()
    {
        isDestroyed = true;

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        rb.AddExplosionForce(2500f, transform.position - transform.forward * 2f, 5f, 2f, ForceMode.Impulse);

        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<MeshRenderer>(out var mr))
            {
                mr.enabled = false;
            }
            foreach (Transform grandchild in child)
            {
                if (grandchild.TryGetComponent<MeshRenderer>(out var gmr))
                {
                    gmr.enabled = false;
                }
            }
        }

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    void ResetCar()
    {
        isDestroyed = false;

        // Unfreeze rigidbody
        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startingPosition;
        transform.rotation = startingRotation;

        // Restore car visuals
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<MeshRenderer>(out var mr))
            {
                mr.enabled = true;
            }
            foreach (Transform grandchild in child)
            {
                if (grandchild.TryGetComponent<MeshRenderer>(out var gmr))
                {
                    gmr.enabled = true;
                }
            }
        }
    }

}
