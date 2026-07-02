using UnityEngine;

/// <summary>
/// Add to any unit that should be hidden when outside vision (players and minions).
/// FogOfWarManager calls SetVisible each update cycle.
/// Towers don't need this — they're always visible in LoL style.
/// </summary>
public class VisibilityTarget : MonoBehaviour
{
    private Renderer[]  _renderers;
    private TeamComponent _team;
    private bool _visible = true;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
        _team      = GetComponent<TeamComponent>();
    }

    private void OnEnable()  => FogOfWarManager.RegisterTarget(this);
    private void OnDisable() => FogOfWarManager.UnregisterTarget(this);

    public sbyte GetTeamId() => _team != null ? _team.teamId.Value : TeamComponent.Neutral;

    public void SetVisible(bool visible)
    {
        if (_visible == visible) return;
        _visible = visible;
        foreach (var r in _renderers)
            if (r != null) r.enabled = visible;
    }
}
