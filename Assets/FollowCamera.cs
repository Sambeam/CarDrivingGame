using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -10);
    public float smoothSpeed = 5f;

    private bool followRotation = true; // Toggle state

    void Update()
    {
        // Toggle with C key
        if (Input.GetKeyDown(KeyCode.C))
        {
            followRotation = !followRotation;
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desiredPosition;

        if (followRotation)
        {
            // Rotated offset (chase camera style)
            Vector3 rotatedOffset = target.rotation * offset;
            desiredPosition = target.position + rotatedOffset;
        }
        else
        {
            // Fixed offset (free camera style)
            desiredPosition = target.position + offset;
        }

        // Smooth camera movement
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Always look at the car
        transform.LookAt(target.position + target.forward * 5f);
    }
}
