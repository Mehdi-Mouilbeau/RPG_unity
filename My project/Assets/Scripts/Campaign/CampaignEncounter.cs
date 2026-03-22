using System.Collections.Generic;

/// <summary>
/// Données d'une rencontre en attente, passées par GameSession à la scène Battle.
/// </summary>
public class CampaignEncounter
{
    public List<EnemySO>   Enemies  { get; }
    public bool            IsBoss   { get; }
    public CampaignZoneSO  Zone     { get; }

    public CampaignEncounter(List<EnemySO> enemies, CampaignZoneSO zone, bool isBoss = false)
    {
        Enemies = enemies;
        Zone    = zone;
        IsBoss  = isBoss;
    }
}
