using System.Collections.Generic;

public static class ElementSystem
{
    private static readonly Dictionary<(ElementType, ElementType), float> _matrix
        = new()
    {
        { (ElementType.Fire,      ElementType.Nature),    1.25f },
        { (ElementType.Fire,      ElementType.Water),     0.75f },
        { (ElementType.Nature,    ElementType.Lightning), 1.25f },
        { (ElementType.Nature,    ElementType.Fire),      0.75f },
        { (ElementType.Lightning, ElementType.Water),     1.25f },
        { (ElementType.Lightning, ElementType.Nature),    0.75f },
        { (ElementType.Water,     ElementType.Fire),      1.25f },
        { (ElementType.Water,     ElementType.Lightning), 0.75f },
        { (ElementType.Light,     ElementType.Dark),      1.25f },
        { (ElementType.Dark,      ElementType.Light),     1.25f },
    };

    public static float GetModifier(ElementType attackerElement, ElementType defenderAffinity)
    {
        if (attackerElement == ElementType.None || defenderAffinity == ElementType.None)
            return 1.0f;
        if (attackerElement == defenderAffinity)
            return 1.0f;
        if (_matrix.TryGetValue((attackerElement, defenderAffinity), out float mod))
            return mod;
        return 1.0f;
    }
}
