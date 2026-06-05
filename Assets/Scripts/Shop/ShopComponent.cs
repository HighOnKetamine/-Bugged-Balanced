using FishNet.Object;
using UnityEngine;

public class ShopComponent : NetworkBehaviour
{
    [ServerRpc]
    public void ServerRequestPurchase(string itemId)
    {
        if (!IsOwner)
            return;

        if (ShopManager.Instance == null)
        {
            Debug.LogWarning("[ShopComponent] No ShopManager instance found.");
            return;
        }

        if (!ShopManager.Instance.TryPurchaseItem(gameObject, itemId, out string reason))
        {
            Debug.LogWarning($"[ShopComponent] Purchase failed for {itemId}: {reason}");
        }
    }
}
