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

        Camera cam = GetComponent<Camera>();
        if (IsOwner)
        {
            cam.enabled = true;
            cam.depth = 1; // Render above any scene cameras
            initialRotation = transform.rotation;
            parentTransform = transform.parent;
            if (parentTransform != null)
            {
                initialOffset = transform.position - parentTransform.position;
            }
        }
        else
        {
            cam.enabled = false;
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
