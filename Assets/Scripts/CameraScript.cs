using UnityEngine;
using FishNet.Object;

public class CameraScript : NetworkBehaviour
{


    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            Camera cam = GetComponent<Camera>();
            cam.enabled = true;
        }
    }


}
