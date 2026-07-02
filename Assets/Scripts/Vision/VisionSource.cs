using UnityEngine;

/// <summary>
/// Add to any unit (player, minion, tower) to make it reveal the area around it.
/// FogOfWarManager only uses sources whose team matches the local player's team.
/// </summary>
public class VisionSource : MonoBehaviour
{
    [Tooltip("World-unit radius this unit reveals around itself.")]
    [SerializeField] public float visionRadius = 12f;

    private TeamComponent _team;

    private void Awake()
    {
        _team = GetComponent<TeamComponent>();
    }

    private void OnEnable()  => FogOfWarManager.RegisterSource(this);
    private void OnDisable() => FogOfWarManager.UnregisterSource(this);

    public sbyte GetTeamId() => _team != null ? _team.teamId.Value : TeamComponent.Neutral;
}
