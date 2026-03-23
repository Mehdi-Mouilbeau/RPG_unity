using NUnit.Framework;

public class CompanionSelectTests
{
    [Test]
    public void CreatePredefinedCharacter_NullCompanion_CharacterHasNoCompanion()
    {
        var classSO = UnityEngine.ScriptableObject.CreateInstance<ClassSO>();
        classSO.className = "Guerrier";
        classSO.baseHP = 100; classSO.baseMP = 20;
        classSO.baseATK = 10; classSO.baseDEF = 8;
        classSO.baseMAG = 3;  classSO.baseRES = 3;
        classSO.baseAGI = 5;  classSO.baseLCK = 4;

        var raceSO = UnityEngine.ScriptableObject.CreateInstance<RaceSO>();
        raceSO.raceName = "Humain";

        var character = GameSession.CreatePredefinedCharacter(
            "Héros", classSO, raceSO, level: 1, companionSO: null);

        Assert.IsNotNull(character);
        Assert.IsNull(character.Companion);

        UnityEngine.Object.DestroyImmediate(classSO);
        UnityEngine.Object.DestroyImmediate(raceSO);
    }

    [Test]
    public void CreatePredefinedCharacter_WithCompanion_CompanionAssigned()
    {
        var classSO = UnityEngine.ScriptableObject.CreateInstance<ClassSO>();
        classSO.className = "Mage";
        classSO.baseHP = 80; classSO.baseMP = 60;
        classSO.baseATK = 5; classSO.baseDEF = 4;
        classSO.baseMAG = 12; classSO.baseRES = 8;
        classSO.baseAGI = 6;  classSO.baseLCK = 5;

        var raceSO = UnityEngine.ScriptableObject.CreateInstance<RaceSO>();
        raceSO.raceName = "Elfe";

        var companionSO = UnityEngine.ScriptableObject.CreateInstance<CompanionSO>();
        companionSO.companionName = "Esprit";

        var character = GameSession.CreatePredefinedCharacter(
            "Mage", classSO, raceSO, level: 1, companionSO: companionSO);

        Assert.IsNotNull(character.Companion);
        Assert.AreEqual("Esprit", character.Companion.Definition.companionName);

        UnityEngine.Object.DestroyImmediate(classSO);
        UnityEngine.Object.DestroyImmediate(raceSO);
        UnityEngine.Object.DestroyImmediate(companionSO);
    }
}
