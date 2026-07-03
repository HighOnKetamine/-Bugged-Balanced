using FishNet;
using UnityEngine;

/// <summary>
/// Add to any unit (player, minion, tower) to make it reveal the area around it.
///
/// Vision radius is read from CharacterStats.visionRange.Value when that component
/// is present; otherwise falls back to the inspectable fallback field.
/// </summary>
public class VisionSource : MonoBehaviour
{
    [Tooltip("Used only if CharacterStats is absent (e.g. ward or custom object).")]
    [SerializeField] private float _fallbackRadius = 8f;

    private CharacterStats _stats;
    private TeamComponent  _team;

    /// <summary>World-unit reveal radius, sourced from CharacterStats when available.</summary>
    public float VisionRadius => _stats != null ? _stats.visionRange.Value : _fallbackRadius;

    private void Awake()
    {
        _stats = GetComponent<CharacterStats>();
        _team  = GetComponent<TeamComponent>();
    }

    private void OnEnable()
    {
        FogOfWarManager.RegisterSource(this);           // client: drives the visual fog texture
        if (InstanceFinder.IsServerStarted)
            ServerVisionTracker.RegisterSource(this);   // server: drives FogOfWarCondition checks
    }

    private void OnDisable()
    {
        FogOfWarManager.UnregisterSource(this);
        if (InstanceFinder.IsServerStarted)
            ServerVisionTracker.UnregisterSource(this);
    }

    public sbyte GetTeamId() => _team != null ? _team.teamId.Value : TeamComponent.Neutral;
}
