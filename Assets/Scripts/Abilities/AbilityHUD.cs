using UnityEngine;
using FishNet.Object;

public class AbilityHUD : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotsContainer;

    // Call this from PlayerController once IsOwner is confirmed
    public void Initialize(GameObject playerObject)
    {
        AbilityBase[] abilities = playerObject.GetComponentsInChildren<AbilityBase>(true);

        foreach (AbilityBase ability in abilities)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotsContainer);
            AbilitySlot slot = slotGO.GetComponent<AbilitySlot>();

            if (slot != null)
                slot.Initialize(ability, ability.AbilityIcon);
        }
    }
}