using UnityEngine;

public class WheelRotator : MonoBehaviour
{
    public float rotationSpeed = 500f;
    public CarController car;

    void Update()
    {
        if (!car) return;

        float input = Input.GetAxis("Vertical");
        if (input != 0)
        {
            transform.Rotate(Vector3.right * input * rotationSpeed * Time.deltaTime);
        }
    }
}
