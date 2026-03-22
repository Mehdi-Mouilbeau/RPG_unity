#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility to auto-generate all starter RPG ScriptableObject assets.
/// Run via menu: RPG > Create Starter Assets
/// </summary>
public static class RPGAssetCreator
{
    [MenuItem("RPG/Create Starter Assets")]
    public static void CreateAllAssets()
    {
        CreateFolders();
        CreateSkills();
        CreateRaces();
        CreateClasses();
        CreateBotBrains();
        CreateArenaRoster();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("RPG: All starter assets created successfully!");
    }

    private static void CreateFolders()
    {
        EnsureFolder("Assets/_Data");
        EnsureFolder("Assets/_Data/Skills");
        EnsureFolder("Assets/_Data/Races");
        EnsureFolder("Assets/_Data/Classes");
        EnsureFolder("Assets/_Data/AI");
        EnsureFolder("Assets/_Data/Arena");
    }

    // ────────────────────────────── SKILLS ──────────────────────────────

    private static void CreateSkills()
    {
        // ─── Shared ───
        CreateSkill("AttaqueBasique",
            skillName: "Attaque Basique",
            description: "Une attaque physique standard.",
            damageType: SkillDamageType.Physical,
            targetType: SkillTargetType.SingleEnemy,
            element: ElementType.None,
            mpCost: 0,
            cooldown: 0,
            power: 1.0f,
            statusEffect: StatusEffectType.None,
            statusChance: 0f);

        // ─── Guerrier ───
        CreateSkill("FrappeVigoureuse",
            skillName: "Frappe Vigoureuse",
            description: "Une attaque physique puissante.",
            damageType: SkillDamageType.Physical,
            targetType: SkillTargetType.SingleEnemy,
            element: ElementType.None,
            mpCost: 6,
            cooldown: 2,
            power: 1.8f,
            statusEffect: StatusEffectType.None,
            statusChance: 0f);

        // ─── Mage ───
        CreateSkill("BouleDeFeu",
            skillName: "Boule de Feu",
            description: "Lance une boule de feu sur l'ennemi.",
            damageType: SkillDamageType.Magical,
            targetType: SkillTargetType.SingleEnemy,
            element: ElementType.Fire,
            mpCost: 10,
            cooldown: 1,
            power: 1.2f,
            statusEffect: StatusEffectType.Burn,
            statusChance: 0.2f);

        CreateSkill("GlaceCristal",
            skillName: "Glace Cristal",
            description: "Projette des éclats de glace qui peuvent geler la cible.",
            damageType: SkillDamageType.Magical,
            targetType: SkillTargetType.SingleEnemy,
            element: ElementType.Water,
            mpCost: 12,
            cooldown: 2,
            power: 1.3f,
            statusEffect: StatusEffectType.Freeze,
            statusChance: 0.25f);

        // ─── Soigneur ───
        CreateSkill("SoinBasique",
            skillName: "Soin",
            description: "Restaure des HP à un allié.",
            damageType: SkillDamageType.Healing,
            targetType: SkillTargetType.SingleAlly,
            element: ElementType.Light,
            mpCost: 8,
            cooldown: 0,
            power: 1.5f,
            statusEffect: StatusEffectType.None,
            statusChance: 0f);

        CreateSkill("SoinZone",
            skillName: "Soin de Zone",
            description: "Soigne légèrement toute l'équipe.",
            damageType: SkillDamageType.Healing,
            targetType: SkillTargetType.AllAllies,
            element: ElementType.Light,
            mpCost: 20,
            cooldown: 3,
            power: 0.8f,
            statusEffect: StatusEffectType.None,
            statusChance: 0f);
    }

    private static void CreateSkill(string assetName, string skillName, string description,
        SkillDamageType damageType, SkillTargetType targetType, ElementType element,
        int mpCost, int cooldown, float power,
        StatusEffectType statusEffect, float statusChance)
    {
        string path = $"Assets/_Data/Skills/{assetName}.asset";
        if (AssetDatabase.LoadAssetAtPath<SkillSO>(path) != null) return;
        var so = ScriptableObject.CreateInstance<SkillSO>();
        so.skillName = skillName;
        so.description = description;
        so.damageType = damageType;
        so.targetType = targetType;
        so.element = element;
        so.mpCost = mpCost;
        so.cooldownTurns = cooldown;
        so.powerMultiplier = power;
        so.statusEffect = statusEffect;
        so.statusChance = statusChance;
        AssetDatabase.CreateAsset(so, path);
    }

    // ────────────────────────────── RACES ───────────────────────────────

    private static void CreateRaces()
    {
        CreateRace("Humain",
            raceName: "Humain",
            description: "Polyvalents et adaptables. Gagnent 5% d'XP supplémentaire.",
            hp: 0f, mp: 0f, atk: 0f, def: 0f, mag: 0f, res: 0f, agi: 0f, lck: 0f,
            affinity: ElementType.None,
            immunities: new StatusEffectType[0],
            passive: "+5% XP (géré dans le système de progression)");
    }

    private static void CreateRace(string assetName, string raceName, string description,
        float hp, float mp, float atk, float def, float mag, float res, float agi, float lck,
        ElementType affinity, StatusEffectType[] immunities, string passive)
    {
        string path = $"Assets/_Data/Races/{assetName}.asset";
        if (AssetDatabase.LoadAssetAtPath<RaceSO>(path) != null) return;
        var so = ScriptableObject.CreateInstance<RaceSO>();
        so.raceName = raceName;
        so.description = description;
        so.hpModifier = hp; so.mpModifier = mp;
        so.atkModifier = atk; so.defModifier = def;
        so.magModifier = mag; so.resModifier = res;
        so.agiModifier = agi; so.lckModifier = lck;
        so.elementalAffinity = affinity;
        so.statusImmunities = immunities;
        so.passiveDescription = passive;
        AssetDatabase.CreateAsset(so, path);
    }

    // ────────────────────────────── CLASSES ─────────────────────────────

    private static void CreateClasses()
    {
        var attaqueBasique    = Load<SkillSO>("Assets/_Data/Skills/AttaqueBasique.asset");
        var frappeVigoureuse  = Load<SkillSO>("Assets/_Data/Skills/FrappeVigoureuse.asset");
        var bouleDeFeu        = Load<SkillSO>("Assets/_Data/Skills/BouleDeFeu.asset");
        var glaceCristal      = Load<SkillSO>("Assets/_Data/Skills/GlaceCristal.asset");
        var soinBasique       = Load<SkillSO>("Assets/_Data/Skills/SoinBasique.asset");
        var soinZone          = Load<SkillSO>("Assets/_Data/Skills/SoinZone.asset");

        // ─── Guerrier ───
        CreateClass("Guerrier",
            className: "Guerrier",
            role: ClassRole.DPS,
            description: "Combattant physique robuste. Spé: Berserker / Chevalier.",
            baseHP: 120, baseMP: 30, baseATK: 14, baseDEF: 12,
            baseMAG: 5,  baseRES: 8,  baseAGI: 9,  baseLCK: 6,
            hpGrowth: 18, mpGrowth: 4, atkGrowth: 3, defGrowth: 2,
            magGrowth: 1, resGrowth: 1, agiGrowth: 1, lckGrowth: 1,
            affinity: ElementType.None,
            skills: new SkillSO[] { attaqueBasique, frappeVigoureuse });

        // ─── Mage ───
        CreateClass("Mage",
            className: "Mage",
            role: ClassRole.DPS,
            description: "Lanceur de sorts élémentaires. Spé: Élémentaliste / Arcaniste.",
            baseHP: 70, baseMP: 80, baseATK: 5, baseDEF: 6,
            baseMAG: 16, baseRES: 12, baseAGI: 11, baseLCK: 8,
            hpGrowth: 10, mpGrowth: 12, atkGrowth: 1, defGrowth: 1,
            magGrowth: 4, resGrowth: 2, agiGrowth: 1, lckGrowth: 1,
            affinity: ElementType.Fire,
            skills: new SkillSO[] { attaqueBasique, bouleDeFeu, glaceCristal });

        // ─── Soigneur ───
        CreateClass("Soigneur",
            className: "Soigneur",
            role: ClassRole.Healer,
            description: "Soutien vital, soins et résurrection. Spé: Prêtre / Chaman.",
            baseHP: 90, baseMP: 100, baseATK: 7, baseDEF: 8,
            baseMAG: 14, baseRES: 14, baseAGI: 8, baseLCK: 7,
            hpGrowth: 12, mpGrowth: 15, atkGrowth: 1, defGrowth: 2,
            magGrowth: 3, resGrowth: 2, agiGrowth: 1, lckGrowth: 1,
            affinity: ElementType.Light,
            skills: new SkillSO[] { attaqueBasique, soinBasique, soinZone });
    }

    private static void CreateClass(string assetName, string className, ClassRole role,
        string description,
        int baseHP, int baseMP, int baseATK, int baseDEF,
        int baseMAG, int baseRES, int baseAGI, int baseLCK,
        int hpGrowth, int mpGrowth, int atkGrowth, int defGrowth,
        int magGrowth, int resGrowth, int agiGrowth, int lckGrowth,
        ElementType affinity, SkillSO[] skills)
    {
        string path = $"Assets/_Data/Classes/{assetName}.asset";
        if (AssetDatabase.LoadAssetAtPath<ClassSO>(path) != null) return;
        var so = ScriptableObject.CreateInstance<ClassSO>();
        so.className = className;
        so.role = role;
        so.description = description;
        so.baseHP = baseHP; so.baseMP = baseMP;
        so.baseATK = baseATK; so.baseDEF = baseDEF;
        so.baseMAG = baseMAG; so.baseRES = baseRES;
        so.baseAGI = baseAGI; so.baseLCK = baseLCK;
        so.hpGrowth = hpGrowth; so.mpGrowth = mpGrowth;
        so.atkGrowth = atkGrowth; so.defGrowth = defGrowth;
        so.magGrowth = magGrowth; so.resGrowth = resGrowth;
        so.agiGrowth = agiGrowth; so.lckGrowth = lckGrowth;
        so.elementalAffinity = affinity;
        so.startingSkills = skills;
        AssetDatabase.CreateAsset(so, path);
    }

    // ────────────────────────────── BOT BRAINS ─────────────────────────────

    private static void CreateBotBrains()
    {
        string easyPath   = "Assets/_Data/AI/EasyBot.asset";
        string normalPath = "Assets/_Data/AI/NormalBot.asset";

        if (AssetDatabase.LoadAssetAtPath<BotBrain>(easyPath) == null)
        {
            var easy = ScriptableObject.CreateInstance<BotBrain>();
            easy.difficulty = BotDifficulty.Easy;
            AssetDatabase.CreateAsset(easy, easyPath);
        }

        if (AssetDatabase.LoadAssetAtPath<BotBrain>(normalPath) == null)
        {
            var normal = ScriptableObject.CreateInstance<BotBrain>();
            normal.difficulty = BotDifficulty.Normal;
            AssetDatabase.CreateAsset(normal, normalPath);
        }
    }

    // ────────────────────────────── ARENA ROSTER ─────────────────────────────

    private static void CreateArenaRoster()
    {
        string path = "Assets/_Data/Arena/ArenaRoster.asset";
        if (AssetDatabase.LoadAssetAtPath<ArenaRoster>(path) != null) return;

        var roster = ScriptableObject.CreateInstance<ArenaRoster>();
        roster.entries.Add(new ArenaRoster.RosterEntry {
            displayName    = "Guerrier Humain",
            characterClass = Load<ClassSO>("Assets/_Data/Classes/Guerrier.asset"),
            characterRace  = Load<RaceSO>("Assets/_Data/Races/Humain.asset")
        });
        roster.entries.Add(new ArenaRoster.RosterEntry {
            displayName    = "Mage Humain",
            characterClass = Load<ClassSO>("Assets/_Data/Classes/Mage.asset"),
            characterRace  = Load<RaceSO>("Assets/_Data/Races/Humain.asset")
        });
        roster.entries.Add(new ArenaRoster.RosterEntry {
            displayName    = "Soigneur Humain",
            characterClass = Load<ClassSO>("Assets/_Data/Classes/Soigneur.asset"),
            characterRace  = Load<RaceSO>("Assets/_Data/Races/Humain.asset")
        });
        AssetDatabase.CreateAsset(roster, path);
    }

    // ─────────────────────────── HELPERS ────────────────────────────────

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    private static T Load<T>(string path) where T : Object
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null) Debug.LogError($"RPGAssetCreator: asset not found at {path}");
        return asset;
    }
}
#endif
