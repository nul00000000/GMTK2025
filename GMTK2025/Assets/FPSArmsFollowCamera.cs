using UnityEngine;

public class FPSArmsFollowCamera : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;

    Quaternion baseOffset;
    Vector3 posOffset;

    void Start()
    {
        baseOffset = Quaternion.Inverse(cameraTransform.rotation) * transform.rotation;
        posOffset  = cameraTransform.InverseTransformPoint(transform.position);
    }
    
    void LateUpdate()
    {
        transform.rotation = cameraTransform.rotation * baseOffset;
        transform.position = cameraTransform.TransformPoint(posOffset);
    }
}
