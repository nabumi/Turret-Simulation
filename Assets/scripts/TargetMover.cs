using UnityEngine;

public class TargetMover : MonoBehaviour
{
    [Header("Horizontal Rotation")]
    public float rotationSpeed = 45f;

    [Header("Vertical Movement")]
    public float amplitude = 1.5f; 
    public float frequency = 1.0f; 

    private Transform droneTransform;
    private float initialY;

    void Start()
    {
        droneTransform = transform.GetChild(0);
        if (droneTransform != null)
        {
            initialY = droneTransform.localPosition.y;
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        if (droneTransform != null)
        {
            float newY = initialY + Mathf.Sin(Time.time * frequency) * amplitude;
            droneTransform.localPosition = new Vector3(
                droneTransform.localPosition.x,
                newY,
                droneTransform.localPosition.z
            );
        }
    }
}