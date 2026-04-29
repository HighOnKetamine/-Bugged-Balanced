using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;

public class ManaComponent : NetworkBehaviour
{
    private CharacterStats _stats;

    public readonly SyncVar<float> currentMana = new SyncVar<float>();

    public event Action<float, float> OnManaChanged; // current, max

    public float Current => currentMana.Value;
    public float Max => _stats.maxMana.Value;

    private void Awake()
    {
        _stats = GetComponent<CharacterStats>();
        if (_stats == null)
            Debug.LogError($"[ManaComponent] No CharacterStats found on {gameObject.name}!");

        currentMana.OnChange += HandleManaChanged;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (IsServerInitialized)
        {
            currentMana.Value = Max;
            if (_stats.resourceType != ResourceType.None)
                StartCoroutine(RegenTick());
        }
    }

    [Server]
    public bool UseMana(float cost)
    {
        if (currentMana.Value < cost) return false;
        currentMana.Value -= cost;
        return true;
    }

    [Server]
    public void RestoreMana(float amount)
    {
        currentMana.Value = Mathf.Min(Max, currentMana.Value + amount);
    }

    private IEnumerator RegenTick()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (_stats.resourceType != ResourceType.None)
                RestoreMana(_stats.manaRegen.Value);
        }
    }

    private void HandleManaChanged(float oldValue, float newValue, bool asServer)
    {
        OnManaChanged?.Invoke(newValue, Max);
    }


    [Server]
    public void ResetToFull()
    {
        currentMana.Value = Max;
    }
}