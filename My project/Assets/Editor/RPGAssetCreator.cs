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
        CreateStarterEquipment();
        CreateStarterSkillTrees();
        CreateStarterCompanions();
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
        EnsureFolder("Assets/_Data/Equipment");
        EnsureFolder("Assets/_Data/SkillTrees");
        EnsureFolder("Assets/_Data/Companions");
        EnsureFolder("Assets/_Data/Companions/Skills");
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

    private static T CreateOrLoad<T>(string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;
        var so = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(so, path);
        return so;
    }

    // ────────────────────────────── EQUIPMENT ───────────────────────────────

    private static void CreateStarterEquipment()
    {
        // ── Warrior starter gear (physical focus) ──
        CreateEquipment("EpeeEnFer",
            itemName: "Épée en Fer", description: "Une lame solide, taillée pour le combat.",
            slot: EquipmentSlot.MainWeapon, rarity: EquipmentRarity.Common,
            atkBonus: 6, defBonus: 0, hpBonus: 0, mpBonus: 0,
            magBonus: 0, resBonus: 0, agiBonus: 0, lckBonus: 0,
            materials: new[] { "FerBrut", "Charbon" }, goldCost: 80,
            effectPool: new[] { "atk_3", "crit_5", "damage_on_hit" });

        CreateEquipment("BouclierEnBois",
            itemName: "Bouclier en Bois", description: "Protection basique en bois épais.",
            slot: EquipmentSlot.Offhand, rarity: EquipmentRarity.Common,
            atkBonus: 0, defBonus: 4, hpBonus: 10, mpBonus: 0,
            magBonus: 0, resBonus: 0, agiBonus: 0, lckBonus: 0,
            materials: new[] { "BoisDur" }, goldCost: 50,
            effectPool: new[] { "def_2", "hp_regen", "elemental_resist" });

        CreateEquipment("CasqueEnCuir",
            itemName: "Casque en Cuir", description: "Un casque léger en cuir tanné.",
            slot: EquipmentSlot.Helmet, rarity: EquipmentRarity.Common,
            atkBonus: 0, defBonus: 2, hpBonus: 15, mpBonus: 0,
            magBonus: 0, resBonus: 1, agiBonus: 0, lckBonus: 0,
            materials: new[] { "CuirBrut" }, goldCost: 40,
            effectPool: new[] { "def_2", "res_2", "hp_regen" });

        CreateEquipment("ArmureEnCuir",
            itemName: "Armure en Cuir", description: "Armure légère offrant une bonne mobilité.",
            slot: EquipmentSlot.Armor, rarity: EquipmentRarity.Common,
            atkBonus: 0, defBonus: 5, hpBonus: 25, mpBonus: 0,
            magBonus: 0, resBonus: 2, agiBonus: 0, lckBonus: 0,
            materials: new[] { "CuirBrut", "FerBrut" }, goldCost: 100,
            effectPool: new[] { "def_3", "hp_regen", "elemental_resist" });

        CreateEquipment("BottesEnCuir",
            itemName: "Bottes en Cuir", description: "Des bottes confortables pour les longs voyages.",
            slot: EquipmentSlot.Boots, rarity: EquipmentRarity.Common,
            atkBonus: 0, defBonus: 1, hpBonus: 0, mpBonus: 0,
            magBonus: 0, resBonus: 0, agiBonus: 3, lckBonus: 0,
            materials: new[] { "CuirBrut" }, goldCost: 30,
            effectPool: new[] { "agi_2", "lck_2", "crit_3" });

        CreateEquipment("AnneauBasique1",
            itemName: "Anneau Basique", description: "Un anneau simple qui porte chance.",
            slot: EquipmentSlot.Ring1, rarity: EquipmentRarity.Common,
            atkBonus: 0, defBonus: 0, hpBonus: 0, mpBonus: 5,
            magBonus: 0, resBonus: 0, agiBonus: 0, lckBonus: 2,
            materials: new[] { "PierreGemme" }, goldCost: 60,
            effectPool: new[] { "lck_3", "crit_5", "mp_regen" });

        CreateEquipment("AnneauBasique2",
            itemName: "Anneau de Vigueur", description: "Un anneau qui renforce la résistance.",
            slot: EquipmentSlot.Ring2, rarity: EquipmentRarity.Common,
            atkBonus: 0, defBonus: 0, hpBonus: 5, mpBonus: 0,
            magBonus: 0, resBonus: 2, agiBonus: 0, lckBonus: 0,
            materials: new[] { "PierreGemme" }, goldCost: 60,
            effectPool: new[] { "res_2", "hp_regen", "elemental_resist" });

        CreateEquipment("BatonArcanique",
            itemName: "Bâton Arcanique", description: "Un bâton chargé de puissance magique.",
            slot: EquipmentSlot.MainWeapon, rarity: EquipmentRarity.Common,
            atkBonus: 0, defBonus: 0, hpBonus: 0, mpBonus: 10,
            magBonus: 7, resBonus: 0, agiBonus: 0, lckBonus: 0,
            materials: new[] { "CristalArcanique", "BoisDur" }, goldCost: 90,
            effectPool: new[] { "mag_4", "mp_regen", "crit_5" });

        // ── Rare example (unlocked via Forge) ──
        CreateEquipment("EpeeRunique",
            itemName: "Épée Runique", description: "Lame gravée de runes anciennement oubliées.",
            slot: EquipmentSlot.MainWeapon, rarity: EquipmentRarity.Rare,
            atkBonus: 10, defBonus: 0, hpBonus: 0, mpBonus: 0,
            magBonus: 0, resBonus: 0, agiBonus: 2, lckBonus: 0,
            materials: new[] { "FerRunique", "Charbon", "CristalArcanique" }, goldCost: 250,
            effectPool: new[] { "atk_5", "crit_8", "damage_on_hit", "elemental_fire" });
    }

    private static void CreateEquipment(string assetName, string itemName, string description,
        EquipmentSlot slot, EquipmentRarity rarity,
        int atkBonus, int defBonus, int hpBonus, int mpBonus,
        int magBonus, int resBonus, int agiBonus, int lckBonus,
        string[] materials, int goldCost, string[] effectPool)
    {
        string path = $"Assets/_Data/Equipment/{assetName}.asset";
        if (AssetDatabase.LoadAssetAtPath<EquipmentSO>(path) != null) return;
        var so = ScriptableObject.CreateInstance<EquipmentSO>();
        so.itemName          = itemName;
        so.description       = description;
        so.slot              = slot;
        so.rarity            = rarity;
        so.atkBonus          = atkBonus;
        so.defBonus          = defBonus;
        so.hpBonus           = hpBonus;
        so.mpBonus           = mpBonus;
        so.magBonus          = magBonus;
        so.resBonus          = resBonus;
        so.agiBonus          = agiBonus;
        so.lckBonus          = lckBonus;
        so.craftingMaterials = materials;
        so.craftingGoldCost  = goldCost;
        so.effectPool        = effectPool;
        AssetDatabase.CreateAsset(so, path);
    }
    // ────────────────────────────── SKILL TREES ─────────────────────────────

    private static void CreateStarterSkillTrees()
    {
        var tree = CreateOrLoad<SkillTreeSO>("Assets/_Data/SkillTrees/GuerrierSkillTree.asset");
        tree.specAName = "Chevalier";
        tree.specBName = "Berserker";
        tree.nodes = new SkillNode[]
        {
            // ── Tronc commun (L1-10, 5 nœuds) ───────────────────────────────
            new SkillNode { nodeId = "g_coup_puissant",     branch = SkillBranch.Common, pointCost = 1, unlockLevel = 1,  prerequisiteNodeIds = new string[0] },
            new SkillNode { nodeId = "g_cri_de_guerre",     branch = SkillBranch.Common, pointCost = 1, unlockLevel = 2,  prerequisiteNodeIds = new[] { "g_coup_puissant" } },
            new SkillNode { nodeId = "g_posture_de_garde",  branch = SkillBranch.Common, pointCost = 1, unlockLevel = 3,  prerequisiteNodeIds = new[] { "g_coup_puissant" } },
            new SkillNode { nodeId = "g_attaque_en_chaine", branch = SkillBranch.Common, pointCost = 1, unlockLevel = 5,  prerequisiteNodeIds = new[] { "g_cri_de_guerre" } },
            new SkillNode { nodeId = "g_endurance",         branch = SkillBranch.Common, pointCost = 1, unlockLevel = 7,  prerequisiteNodeIds = new[] { "g_posture_de_garde" } },

            // ── Branche Chevalier (SpecA, L10-30, 8 nœuds) ──────────────────
            new SkillNode { nodeId = "c_bouclier_sacre",    branch = SkillBranch.SpecA,  pointCost = 1, unlockLevel = 10, prerequisiteNodeIds = new[] { "g_endurance" } },
            new SkillNode { nodeId = "c_mur_de_fer",        branch = SkillBranch.SpecA,  pointCost = 1, unlockLevel = 11, prerequisiteNodeIds = new[] { "c_bouclier_sacre" } },
            new SkillNode { nodeId = "c_frappe_divine",     branch = SkillBranch.SpecA,  pointCost = 1, unlockLevel = 12, prerequisiteNodeIds = new[] { "c_bouclier_sacre" } },
            new SkillNode { nodeId = "c_benediction",       branch = SkillBranch.SpecA,  pointCost = 1, unlockLevel = 14, prerequisiteNodeIds = new[] { "c_frappe_divine" } },
            new SkillNode { nodeId = "c_aura_de_lumiere",   branch = SkillBranch.SpecA,  pointCost = 1, unlockLevel = 16, prerequisiteNodeIds = new[] { "c_benediction" } },
            new SkillNode { nodeId = "c_armure_sacree",     branch = SkillBranch.SpecA,  pointCost = 1, unlockLevel = 18, prerequisiteNodeIds = new[] { "c_mur_de_fer" } },
            new SkillNode { nodeId = "c_jugement",          branch = SkillBranch.SpecA,  pointCost = 1, unlockLevel = 22, prerequisiteNodeIds = new[] { "c_aura_de_lumiere" } },
            new SkillNode { nodeId = "c_paladin_divin",     branch = SkillBranch.SpecA,  pointCost = 1, unlockLevel = 27, prerequisiteNodeIds = new[] { "c_jugement", "c_armure_sacree" } },

            // ── Branche Berserker (SpecB, L10-30, 8 nœuds) ──────────────────
            new SkillNode { nodeId = "b_rage",              branch = SkillBranch.SpecB,  pointCost = 1, unlockLevel = 10, prerequisiteNodeIds = new[] { "g_attaque_en_chaine" } },
            new SkillNode { nodeId = "b_peau_epaisse",      branch = SkillBranch.SpecB,  pointCost = 1, unlockLevel = 11, prerequisiteNodeIds = new[] { "b_rage" } },
            new SkillNode { nodeId = "b_frenésie",          branch = SkillBranch.SpecB,  pointCost = 1, unlockLevel = 12, prerequisiteNodeIds = new[] { "b_rage" } },
            new SkillNode { nodeId = "b_cri_primitif",      branch = SkillBranch.SpecB,  pointCost = 1, unlockLevel = 14, prerequisiteNodeIds = new[] { "b_frenésie" } },
            new SkillNode { nodeId = "b_instinct_de_tueur", branch = SkillBranch.SpecB,  pointCost = 1, unlockLevel = 16, prerequisiteNodeIds = new[] { "b_frenésie" } },
            new SkillNode { nodeId = "b_pouls_de_guerre",   branch = SkillBranch.SpecB,  pointCost = 1, unlockLevel = 18, prerequisiteNodeIds = new[] { "b_peau_epaisse" } },
            new SkillNode { nodeId = "b_carnage",           branch = SkillBranch.SpecB,  pointCost = 1, unlockLevel = 22, prerequisiteNodeIds = new[] { "b_instinct_de_tueur" } },
            new SkillNode { nodeId = "b_avatar_du_chaos",   branch = SkillBranch.SpecB,  pointCost = 1, unlockLevel = 27, prerequisiteNodeIds = new[] { "b_carnage", "b_pouls_de_guerre" } },
        };
        EditorUtility.SetDirty(tree);
        AssetDatabase.SaveAssets();
        Debug.Log("[RPGAssetCreator] GuerrierSkillTree created.");
    }

    // ────────────────────────────── COMPANIONS ──────────────────────────────

    private static void CreateStarterCompanions()
    {
        // ── Loup des Ombres (Offensif) ───────────────────────────────────
        var morsure = CreateCompanionSkill("LoupMorsure",
            skillName: "Morsure Sombre", description: "Le loup mord l'ennemi avec une force obscure.",
            effect: CompanionEffectType.DirectDamage, target: CompanionTargetType.EnemySingle,
            value: 35, cooldown: 3);
        var hurlement = CreateCompanionSkill("LoupHurlement",
            skillName: "Hurlement", description: "Un hurlement qui touche tous les ennemis.",
            effect: CompanionEffectType.DirectDamage, target: CompanionTargetType.AllEnemies,
            value: 20, cooldown: 4);
        var bond = CreateCompanionSkill("LoupBond",
            skillName: "Bond Prédateur", description: "Un bond puissant sur une cible unique.",
            effect: CompanionEffectType.DirectDamage, target: CompanionTargetType.EnemySingle,
            value: 50, cooldown: 6);

        var loup = CreateOrLoad<CompanionSO>("Assets/_Data/Companions/LoupDesOmbres.asset");
        loup.companionName = "Loup des Ombres";
        loup.description = "Un loup spectral aux crocs imprégnés d'ombre.";
        loup.type = CompanionType.Offensif;
        loup.skills = new[] { morsure, hurlement, bond };
        EditorUtility.SetDirty(loup);

        // ── Golem de Cristal (Défensif) ──────────────────────────────────
        var absorption = CreateCompanionSkill("GolemAbsorption",
            skillName: "Absorption", description: "Le golem restaure les HP d'un allié blessé.",
            effect: CompanionEffectType.Heal, target: CompanionTargetType.AllySingle,
            value: 40, cooldown: 4);
        var reparation = CreateCompanionSkill("GolemReparation",
            skillName: "Réparation", description: "Le golem répare légèrement toute l'équipe.",
            effect: CompanionEffectType.Heal, target: CompanionTargetType.AllAllies,
            value: 20, cooldown: 5);
        var purification = CreateCompanionSkill("GolemPurification",
            skillName: "Purification", description: "Supprime les statuts négatifs d'un allié.",
            effect: CompanionEffectType.RemoveStatuses, target: CompanionTargetType.AllySingle,
            value: 0, cooldown: 3);

        var golem = CreateOrLoad<CompanionSO>("Assets/_Data/Companions/GolemDeCristal.asset");
        golem.companionName = "Golem de Cristal";
        golem.description = "Un gardien de cristal qui protège ses alliés.";
        golem.type = CompanionType.Defensif;
        golem.skills = new[] { absorption, reparation, purification };
        EditorUtility.SetDirty(golem);

        // ── Fée Sylvestre (Support) ──────────────────────────────────────
        var soinFee = CreateCompanionSkill("FeeSoin",
            skillName: "Soin Sylvestre", description: "La fée soigne généreusement un allié.",
            effect: CompanionEffectType.Heal, target: CompanionTargetType.AllySingle,
            value: 35, cooldown: 3);
        var benediction = CreateCompanionSkill("FeeBenediction",
            skillName: "Bénédiction", description: "La fée soigne légèrement toute l'équipe.",
            effect: CompanionEffectType.Heal, target: CompanionTargetType.AllAllies,
            value: 15, cooldown: 3);
        var purificationTotale = CreateCompanionSkill("FeePurification",
            skillName: "Purification Totale", description: "Supprime tous les statuts négatifs de l'équipe.",
            effect: CompanionEffectType.RemoveStatuses, target: CompanionTargetType.AllAllies,
            value: 0, cooldown: 5);

        var fee = CreateOrLoad<CompanionSO>("Assets/_Data/Companions/FeeSylvestre.asset");
        fee.companionName = "Fée Sylvestre";
        fee.description = "Une fée des bois dotée de pouvoirs de guérison.";
        fee.type = CompanionType.Support;
        fee.skills = new[] { soinFee, benediction, purificationTotale };
        EditorUtility.SetDirty(fee);

        // ── Corbeau Analyste (Utilitaire) ────────────────────────────────
        var analyse = CreateCompanionSkill("CorbeauAnalyse",
            skillName: "Analyse", description: "Le corbeau révèle les faiblesses de l'ennemi.",
            effect: CompanionEffectType.RevealInfo, target: CompanionTargetType.EnemySingle,
            value: 0, cooldown: 3);
        var distraction = CreateCompanionSkill("CorbeauDistraction",
            skillName: "Distraction", description: "Le corbeau distrait et griffe un ennemi.",
            effect: CompanionEffectType.DirectDamage, target: CompanionTargetType.EnemySingle,
            value: 15, cooldown: 3);
        var acceleration = CreateCompanionSkill("CorbeauAcceleration",
            skillName: "Accélération", description: "Le corbeau augmente l'initiative d'un allié.",
            effect: CompanionEffectType.BoostAgi, target: CompanionTargetType.AllySingle,
            value: 0, cooldown: 5);

        var corbeau = CreateOrLoad<CompanionSO>("Assets/_Data/Companions/CorbeauAnalyste.asset");
        corbeau.companionName = "Corbeau Analyste";
        corbeau.description = "Un corbeau intelligent qui analyse et perturbe les ennemis.";
        corbeau.type = CompanionType.Utilitaire;
        corbeau.skills = new[] { analyse, distraction, acceleration };
        EditorUtility.SetDirty(corbeau);

        AssetDatabase.SaveAssets();
        Debug.Log("[RPGAssetCreator] 4 starter companions created.");
    }

    private static CompanionSkillSO CreateCompanionSkill(string assetName, string skillName,
        string description, CompanionEffectType effect, CompanionTargetType target, int value, int cooldown)
    {
        var skill = CreateOrLoad<CompanionSkillSO>($"Assets/_Data/Companions/Skills/{assetName}.asset");
        skill.skillName    = skillName;
        skill.description  = description;
        skill.effectType   = effect;
        skill.targetType   = target;
        skill.value        = value;
        skill.cooldownTurns = cooldown;
        EditorUtility.SetDirty(skill);
        return skill;
    }
}
#endif
