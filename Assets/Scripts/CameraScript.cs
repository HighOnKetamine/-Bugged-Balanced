using UnityEngine;
using FishNet.Object;

public class CameraScript : NetworkBehaviour
{
    private Quaternion initialRotation;
    private Vector3 initialOffset;
    private Transform parentTransform;
    private Camera _cam;

    public override void OnStartClient()
    {
        base.OnStartClient();

        _cam = GetComponent<Camera>();
        _cam.enabled = false; // always disable first

        if (IsOwner)
        {
            _cam.enabled = true;
            initialRotation = transform.rotation;
            parentTransform = transform.parent;
            if (parentTransform != null)
                initialOffset = transform.position - parentTransform.position;
        }
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;
        if (parentTransform == null) return;

        transform.rotation = initialRotation;
        transform.position = parentTransform.position + initialOffset;
    }
}