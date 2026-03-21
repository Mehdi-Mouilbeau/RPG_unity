[System.Serializable]
public class StatusEffect
{
    public StatusEffectType type;
    public int remainingTurns;
    public float value;

    public StatusEffect(StatusEffectType type, int turns, float value = 0f)
    {
        this.type = type;
        this.remainingTurns = turns;
        this.value = value;
    }
}
