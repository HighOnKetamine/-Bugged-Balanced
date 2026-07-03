using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Server-only singleton.  Holds every registered VisionSource and answers
/// "can team X see world position P?" for FogOfWarCondition observer checks.
///
/// No networking is needed here — this object only exists on the server process.
/// </summary>
public class ServerVisionTracker : MonoBehaviour
{
    public static ServerVisionTracker Instance { get; private set; }

    private static readonly List<VisionSource> _sources = new();

    public static void RegisterSource  (VisionSource s) { if (!_sources.Contains(s)) _sources.Add(s); }
    public static void UnregisterSource(VisionSource s) => _sources.Remove(s);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        _sources.Clear();
    }

    /// <summary>
    /// Returns true if any vision source belonging to <paramref name="observerTeam"/>
    /// has <paramref name="targetPos"/> inside its radius.
    /// </summary>
    public bool CanSee(sbyte observerTeam, Vector3 targetPos)
    {
        foreach (var src in _sources)
        {
            if (src == null) continue;
            if (src.GetTeamId() != observerTeam) continue;
            float r = src.VisionRadius;
            if ((src.transform.position - targetPos).sqrMagnitude <= r * r)
                return true;
        }
        return false;
    }
}
