using UnityEngine;
using FishNet.Object;

public class AbilityHUD : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotsContainer;

    // Call this from PlayerController once IsOwner is confirmed
    public void Initialize(GameObject playerObject)
    {
        AbilityBase[] abilities = playerObject.GetComponents<AbilityBase>();

        foreach (AbilityBase ability in abilities)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotsContainer);
            AbilitySlot slot = slotGO.GetComponent<AbilitySlot>();

            if (slot != null)
                slot.Initialize(ability, null); // pass icon sprite here later
        }
    }
}