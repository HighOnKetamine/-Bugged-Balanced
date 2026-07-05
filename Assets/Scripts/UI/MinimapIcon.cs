using FishNet.Object;
using UnityEngine;

/// <summary>
/// Add to every unit that should appear on the minimap.
/// Set IconType in the Inspector (Player / Minion / Structure).
/// For players, assign MinimapSprite to show a portrait icon instead of a dot.
/// The component self-registers with MinimapManager on enable/disable.
/// </summary>
public class MinimapIcon : MonoBehaviour
{
    public enum IconType { Player, Minion, Structure }

    [SerializeField] private IconType _type = IconType.Minion;

    [Tooltip("Optional portrait sprite for Player icons. When set, shown as a small image instead of a dot.")]
    public Sprite MinimapSprite;

    private TeamComponent    _team;
    private VisibilityTarget _visibility;
    private NetworkObject    _nob;

    private void Awake()
    {
        _team       = GetComponent<TeamComponent>();
        _visibility = GetComponent<VisibilityTarget>();
        _nob        = GetComponent<NetworkObject>();
    }

    private void OnEnable()  => MinimapManager.Register(this);
    private void OnDisable() => MinimapManager.Unregister(this);

    public IconType Type          => _type;
    public sbyte    TeamId        => _team != null ? _team.teamId.Value : TeamComponent.Neutral;
    public bool     IsLocalPlayer => _nob  != null && _nob.IsOwner;

    /// <summary>
    /// True when this icon should be drawn this frame.
    /// Allies and structures are always visible; enemy units only when in vision.
    /// </summary>
    public bool ShouldShow(sbyte localTeam)
    {
        if (localTeam == TeamComponent.Neutral) return true;
        if (TeamId == localTeam || TeamId == TeamComponent.Neutral) return true;

        // Structures (towers, inhibitors, nexus) are fixed map features — always shown.
        if (_type == IconType.Structure) return true;

        // Enemy unit: only show when inside local team's vision.
        if (_visibility != null) return _visibility.IsCurrentlyVisible;
        return FogOfWarManager.Instance == null ||
               FogOfWarManager.Instance.IsPositionVisible(transform.position);
    }
}
