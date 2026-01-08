using UnityEngine;
using FishNet.Object;

public class CameraScript : NetworkBehaviour
{


    private Quaternion initialRotation;
    private Vector3 initialOffset;
    private Transform parentTransform;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            Camera cam = GetComponent<Camera>();
            cam.enabled = true;
            initialRotation = transform.rotation;
            parentTransform = transform.parent;
            if (parentTransform != null)
            {
                initialOffset = transform.position - parentTransform.position;
            }
        }
    }

    private void LateUpdate()
    {
        if (IsOwner)
        {
            transform.rotation = initialRotation;
            if (parentTransform != null)
            {
                transform.position = parentTransform.position + initialOffset;
            }
        }
    }


}
