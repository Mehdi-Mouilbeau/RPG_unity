using System.Collections.Generic;

/// <summary>
/// Données d'une rencontre en attente, passées par GameSession à la scène Battle.
/// </summary>
public class CampaignEncounter
{
    public List<EnemySO>   Enemies        { get; }
    public bool            IsBoss         { get; }
    public CampaignZoneSO  Zone           { get; }
    /// <summary>Si non-vide, ce flag est mis à true dans ProgressionFlags quand le joueur gagne.</summary>
    public string          DefeatedFlagKey { get; set; }

    public CampaignEncounter(List<EnemySO> enemies, CampaignZoneSO zone, bool isBoss = false)
    {
        Enemies = enemies;
        Zone    = zone;
        IsBoss  = isBoss;
    }
}
