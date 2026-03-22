using System.Collections.Generic;
using UnityEngine;

public enum BotDifficulty { Easy, Normal }

[CreateAssetMenu(fileName = "NewBot", menuName = "RPG/Bot Brain")]
public class BotBrain : ScriptableObject
{
    public BotDifficulty difficulty = BotDifficulty.Easy;

    public BotAction Decide(CharacterData actor,
        List<CharacterData> allies, List<CharacterData> enemies)
    {
        return difficulty switch
        {
            BotDifficulty.Normal => ActionEvaluator.EvaluateNormal(actor, allies, enemies),
            _                    => ActionEvaluator.EvaluateEasy(actor, allies, enemies),
        };
    }
}
