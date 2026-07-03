using UnityEngine;

/// <summary>
/// Add to any unit that should be hidden when outside vision (players and minions).
/// Towers are always visible in LoL style — don't add this to them.
///
/// The teamId SyncVar defaults to 0 before the server message arrives, so an
/// enemy team-1 unit would briefly appear as a team-0 ally.  We fix this by
/// listening to OnChange: the moment the correct team arrives we re-evaluate
/// visibility immediately rather than waiting for the next 100 ms FoW tick.
/// </summary>
public class VisibilityTarget : MonoBehaviour
{
    private Renderer[]    _renderers;
    private TeamComponent _team;
    private bool          _visible = true;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
        _team      = GetComponent<TeamComponent>();
    }

    private void Start()
    {
        if (_team != null)
            _team.teamId.OnChange += OnTeamChanged;
    }

    private void OnDestroy()
    {
        if (_team != null)
            _team.teamId.OnChange -= OnTeamChanged;
    }

    private void OnTeamChanged(sbyte prev, sbyte next, bool asServer)
    {
        // Immediately correct visibility when the team SyncVar is first set.
        FogOfWarManager.Instance?.EvaluateTarget(this);
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
