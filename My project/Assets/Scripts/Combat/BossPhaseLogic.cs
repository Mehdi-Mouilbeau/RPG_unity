/// <summary>
/// Logique pure de phase du boss — sans dépendance Unity.
/// BossController (MonoBehaviour) délègue à cette classe.
/// </summary>
public class BossPhaseLogic
{
    private readonly int   _maxHP;
    private readonly float _threshold;

    public bool  Phase2Active       { get; private set; }
    public float Phase2ATKMultiplier => 1.25f;

    public BossPhaseLogic(int maxHP, float phaseThreshold = 0.5f)
    {
        _maxHP     = maxHP;
        _threshold = phaseThreshold;
    }

    /// <summary>
    /// Checks if the boss should transition to phase 2.
    /// Returns true only once (at the transition moment).
    /// </summary>
    public bool CheckAndTransition(int currentHP)
    {
        if (Phase2Active) return false;
        if ((float)currentHP / _maxHP <= _threshold)
        {
            Phase2Active = true;
            return true;
        }
        return false;
    }
}
