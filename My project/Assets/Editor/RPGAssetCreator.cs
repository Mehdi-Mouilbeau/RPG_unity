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
        CreateCampaignEnemies();
        CreateCampaignZones();
        CreateGameDataRegistry();
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
        EnsureFolder("Assets/_Data/Enemies");
        EnsureFolder("Assets/_Data/Zones");
        EnsureFolder("Assets/_Data/LootTables");
        EnsureFolder("Assets/Resources");
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

        // ─── Guerrier (nouveaux) ───
        CreateSkill("Tourbillon",
            skillName: "Tourbillon",
            description: "Frappe tous les ennemis en tournoyant.",
            damageType: SkillDamageType.Physical,
            targetType: SkillTargetType.AllEnemies,
            element: ElementType.None,
            mpCost: 8,
            cooldown: 2,
            power: 0.7f,
            statusEffect: StatusEffectType.None,
            statusChance: 0f);

        CreateSkill("CoupParalysant",
            skillName: "Coup Paralysant",
            description: "Un coup précis qui peut paralyser l'ennemi.",
            damageType: SkillDamageType.Physical,
            targetType: SkillTargetType.SingleEnemy,
            element: ElementType.None,
            mpCost: 6,
            cooldown: 2,
            power: 1.2f,
            statusEffect: StatusEffectType.Paralysis,
            statusChance: 0.4f);

        // ─── Mage (nouveaux) ───
        CreateSkill("Foudre",
            skillName: "Foudre",
            description: "Appelle la foudre sur tous les ennemis, peut paralyser.",
            damageType: SkillDamageType.Magical,
            targetType: SkillTargetType.AllEnemies,
            element: ElementType.Lightning,
            mpCost: 12,
            cooldown: 2,
            power: 1.0f,
            statusEffect: StatusEffectType.Paralysis,
            statusChance: 0.25f);

        // ─── Soigneur (nouveaux) ───
        CreateSkill("LumiereSacree",
            skillName: "Lumière Sacrée",
            description: "Blast de lumière sacrée qui peut brûler les ennemis obscurs.",
            damageType: SkillDamageType.Magical,
            targetType: SkillTargetType.SingleEnemy,
            element: ElementType.Light,
            mpCost: 8,
            cooldown: 1,
            power: 1.4f,
            statusEffect: StatusEffectType.Burn,
            statusChance: 0.2f);

        // ─── Démoniste ───
        CreateSkill("DrainVie",
            skillName: "Drain de Vie",
            description: "Aspire l'énergie vitale de l'ennemi avec les ténèbres.",
            damageType: SkillDamageType.Magical,
            targetType: SkillTargetType.SingleEnemy,
            element: ElementType.Dark,
            mpCost: 10,
            cooldown: 1,
            power: 1.0f,
            statusEffect: StatusEffectType.None,
            statusChance: 0f);

        CreateSkill("MaledictionSombre",
            skillName: "Malédiction Sombre",
            description: "Maudit un ennemi, l'empoisonnant avec une énergie obscure.",
            damageType: SkillDamageType.Status,
            targetType: SkillTargetType.SingleEnemy,
            element: ElementType.Dark,
            mpCost: 8,
            cooldown: 2,
            power: 0f,
            statusEffect: StatusEffectType.Poison,
            statusChance: 0.8f);

        CreateSkill("VagueObscure",
            skillName: "Vague Obscure",
            description: "Déferle une vague de ténèbres sur tous les ennemis, pouvant les confondre.",
            damageType: SkillDamageType.Magical,
            targetType: SkillTargetType.AllEnemies,
            element: ElementType.Dark,
            mpCost: 15,
            cooldown: 3,
            power: 0.8f,
            statusEffect: StatusEffectType.Confusion,
            statusChance: 0.25f);

        // ─── Voleur ───
        CreateSkill("FrappeEmpoisonnee",
            skillName: "Frappe Empoisonnée",
            description: "Un coup rapide enrobé de poison.",
            damageType: SkillDamageType.Physical,
            targetType: SkillTargetType.SingleEnemy,
            element: ElementType.Nature,
            mpCost: 5,
            cooldown: 1,
            power: 1.0f,
            statusEffect: StatusEffectType.Poison,
            statusChance: 0.6f);

        CreateSkill("CoupCritique",
            skillName: "Coup Critique",
            description: "Un strike concentré sur un point vital, garantissant un maximum de dégâts.",
            damageType: SkillDamageType.Physical,
            targetType: SkillTargetType.SingleEnemy,
            element: ElementType.None,
            mpCost: 8,
            cooldown: 3,
            power: 2.2f,
            statusEffect: StatusEffectType.None,
            statusChance: 0f);

        CreateSkill("GesteSournois",
            skillName: "Geste Sournois",
            description: "Une attaque traîtresse qui désoriente la cible.",
            damageType: SkillDamageType.Physical,
            targetType: SkillTargetType.SingleEnemy,
            element: ElementType.Dark,
            mpCost: 6,
            cooldown: 2,
            power: 1.2f,
            statusEffect: StatusEffectType.Confusion,
            statusChance: 0.35f);

        // ─── Archer ───
        CreateSkill("TirPrecis",
            skillName: "Tir Précis",
            description: "Une flèche tirée avec une précision mortelle sur une cible unique.",
            damageType: SkillDamageType.Physical,
            targetType: SkillTargetType.SingleEnemy,
            element: ElementType.None,
            mpCost: 5,
            cooldown: 1,
            power: 1.6f,
            statusEffect: StatusEffectType.None,
            statusChance: 0f);

        CreateSkill("FlecheEmpoisonnee",
            skillName: "Flèche Empoisonnée",
            description: "Une flèche trempée dans un venin de plantes.",
            damageType: SkillDamageType.Physical,
            targetType: SkillTargetType.SingleEnemy,
            element: ElementType.Nature,
            mpCost: 7,
            cooldown: 2,
            power: 1.1f,
            statusEffect: StatusEffectType.Poison,
            statusChance: 0.5f);

        CreateSkill("PluieDeFleches",
            skillName: "Pluie de Flèches",
            description: "Tire une volée de flèches sur tous les ennemis.",
            damageType: SkillDamageType.Physical,
            targetType: SkillTargetType.AllEnemies,
            element: ElementType.None,
            mpCost: 12,
            cooldown: 3,
            power: 0.65f,
            statusEffect: StatusEffectType.None,
            statusChance: 0f);

        // ─── Invocateur ───
        CreateSkill("AppelDesBetes",
            skillName: "Appel des Bêtes",
            description: "Invoque un groupe de bêtes sauvages qui attaquent tous les ennemis.",
            damageType: SkillDamageType.Magical,
            targetType: SkillTargetType.AllEnemies,
            element: ElementType.Nature,
            mpCost: 12,
            cooldown: 2,
            power: 0.9f,
            statusEffect: StatusEffectType.None,
            statusChance: 0f);

        CreateSkill("SoinBestial",
            skillName: "Soin Bestial",
            description: "La force de la nature restaure les HP d'un allié.",
            damageType: SkillDamageType.Healing,
            targetType: SkillTargetType.SingleAlly,
            element: ElementType.Nature,
            mpCost: 10,
            cooldown: 2,
            power: 1.2f,
            statusEffect: StatusEffectType.None,
            statusChance: 0f);

        CreateSkill("MordureEmpoisonnee",
            skillName: "Morsure Empoisonnée",
            description: "Une bête alliée mord l'ennemi et l'empoisonne.",
            damageType: SkillDamageType.Physical,
            targetType: SkillTargetType.SingleEnemy,
            element: ElementType.Nature,
            mpCost: 8,
            cooldown: 2,
            power: 1.1f,
            statusEffect: StatusEffectType.Poison,
            statusChance: 0.4f);

        // ─── Tank ───
        CreateSkill("BouclierDAcier",
            skillName: "Bouclier d'Acier",
            description: "Le tank génère un bouclier absorbant les dégâts.",
            damageType: SkillDamageType.Status,
            targetType: SkillTargetType.Self,
            element: ElementType.None,
            mpCost: 0,
            cooldown: 3,
            power: 0f,
            statusEffect: StatusEffectType.Shield,
            statusChance: 1.0f);

        CreateSkill("FrappeProvocatrice",
            skillName: "Frappe Provocatrice",
            description: "Un coup puissant qui force l'ennemi à se concentrer sur le Tank.",
            damageType: SkillDamageType.Physical,
            targetType: SkillTargetType.SingleEnemy,
            element: ElementType.None,
            mpCost: 6,
            cooldown: 2,
            power: 1.2f,
            statusEffect: StatusEffectType.None,
            statusChance: 0f);

        CreateSkill("ChargeDeFer",
            skillName: "Charge de Fer",
            description: "Charge violemment toute la ligne ennemie, pouvant confondre certains.",
            damageType: SkillDamageType.Physical,
            targetType: SkillTargetType.AllEnemies,
            element: ElementType.None,
            mpCost: 10,
            cooldown: 3,
            power: 0.7f,
            statusEffect: StatusEffectType.Confusion,
            statusChance: 0.2f);

        // ─── Ingénieur ───
        CreateSkill("GrenadeExplosive",
            skillName: "Grenade Explosive",
            description: "Lance une grenade incendiaire sur tous les ennemis.",
            damageType: SkillDamageType.Magical,
            targetType: SkillTargetType.AllEnemies,
            element: ElementType.Fire,
            mpCost: 10,
            cooldown: 2,
            power: 0.9f,
            statusEffect: StatusEffectType.Burn,
            statusChance: 0.3f);

        CreateSkill("ChocElectrique",
            skillName: "Choc Électrique",
            description: "Décharge électrique sur une cible, pouvant la paralyser.",
            damageType: SkillDamageType.Magical,
            targetType: SkillTargetType.SingleEnemy,
            element: ElementType.Lightning,
            mpCost: 8,
            cooldown: 1,
            power: 1.3f,
            statusEffect: StatusEffectType.Paralysis,
            statusChance: 0.3f);

        CreateSkill("BombeAcide",
            skillName: "Bombe Acide",
            description: "Projette un acide corrosif qui ronge et empoisonne l'ennemi.",
            damageType: SkillDamageType.Physical,
            targetType: SkillTargetType.SingleEnemy,
            element: ElementType.Nature,
            mpCost: 8,
            cooldown: 2,
            power: 1.0f,
            statusEffect: StatusEffectType.Poison,
            statusChance: 0.5f);

        // ─── Éclaireur ───
        CreateSkill("AnalyseFaiblesse",
            skillName: "Analyse Faiblesse",
            description: "Analyse et exploite mentalement l'ennemi, le perturbant profondément.",
            damageType: SkillDamageType.Status,
            targetType: SkillTargetType.SingleEnemy,
            element: ElementType.None,
            mpCost: 5,
            cooldown: 2,
            power: 0f,
            statusEffect: StatusEffectType.Confusion,
            statusChance: 0.8f);

        CreateSkill("CoupDemoralisant",
            skillName: "Coup Démoralisant",
            description: "Un coup calculé qui entame le moral et la concentration de l'ennemi.",
            damageType: SkillDamageType.Physical,
            targetType: SkillTargetType.SingleEnemy,
            element: ElementType.None,
            mpCost: 7,
            cooldown: 2,
            power: 1.1f,
            statusEffect: StatusEffectType.Confusion,
            statusChance: 0.4f);

        CreateSkill("RalliementAllies",
            skillName: "Ralliement des Alliés",
            description: "Galvanise toute l'équipe, restaurant légèrement leurs HP.",
            damageType: SkillDamageType.Healing,
            targetType: SkillTargetType.AllAllies,
            element: ElementType.Light,
            mpCost: 15,
            cooldown: 3,
            power: 0.5f,
            statusEffect: StatusEffectType.None,
            statusChance: 0f);
    }

    [MenuItem("RPG/Update HP Values")]
    public static void UpdateHPValues()
    {
        // ── Classes ──────────────────────────────────────────────────────────
        UpdateClassHP("Guerrier",   baseHP: 210, hpGrowth: 28);
        UpdateClassHP("Mage",       baseHP: 120, hpGrowth: 16);
        UpdateClassHP("Soigneur",   baseHP: 160, hpGrowth: 20);
        UpdateClassHP("Demoniste",  baseHP: 140, hpGrowth: 16);
        UpdateClassHP("Voleur",     baseHP: 150, hpGrowth: 18);
        UpdateClassHP("Archer",     baseHP: 160, hpGrowth: 20);
        UpdateClassHP("Invocateur", baseHP: 150, hpGrowth: 18);
        UpdateClassHP("Tank",       baseHP: 260, hpGrowth: 36);
        UpdateClassHP("Ingenieur",  baseHP: 165, hpGrowth: 21);
        UpdateClassHP("Eclaireur",  baseHP: 155, hpGrowth: 20);

        // ── Ennemis ──────────────────────────────────────────────────────────
        UpdateEnemyHP("Squelette",      hp: 120);
        UpdateEnemyHP("ArcherSquelette", hp: 90);
        UpdateEnemyHP("GolemOs",         hp: 180);
        UpdateEnemyHP("RoiSquelette",    hp: 600);

        AssetDatabase.SaveAssets();
        Debug.Log("RPG: HP values updated for all classes and enemies!");
    }

    private static void UpdateClassHP(string assetName, int baseHP, int hpGrowth)
    {
        var so = Load<ClassSO>($"Assets/_Data/Classes/{assetName}.asset");
        if (so == null) return;
        so.baseHP    = baseHP;
        so.hpGrowth  = hpGrowth;
        EditorUtility.SetDirty(so);
    }

    private static void UpdateEnemyHP(string assetName, int hp)
    {
        var so = Load<EnemySO>($"Assets/_Data/Enemies/{assetName}.asset");
        if (so == null) return;
        so.hp = hp;
        EditorUtility.SetDirty(so);
    }

    [MenuItem("RPG/Update Class Skills")]
    public static void UpdateClassSkills()
    {
        var attaqueBasique     = Load<SkillSO>("Assets/_Data/Skills/AttaqueBasique.asset");
        var frappeVigoureuse   = Load<SkillSO>("Assets/_Data/Skills/FrappeVigoureuse.asset");
        var tourbillon         = Load<SkillSO>("Assets/_Data/Skills/Tourbillon.asset");
        var coupParalysant     = Load<SkillSO>("Assets/_Data/Skills/CoupParalysant.asset");
        var bouleDeFeu         = Load<SkillSO>("Assets/_Data/Skills/BouleDeFeu.asset");
        var glaceCristal       = Load<SkillSO>("Assets/_Data/Skills/GlaceCristal.asset");
        var foudre             = Load<SkillSO>("Assets/_Data/Skills/Foudre.asset");
        var soinBasique        = Load<SkillSO>("Assets/_Data/Skills/SoinBasique.asset");
        var soinZone           = Load<SkillSO>("Assets/_Data/Skills/SoinZone.asset");
        var lumiereSacree      = Load<SkillSO>("Assets/_Data/Skills/LumiereSacree.asset");
        var drainVie           = Load<SkillSO>("Assets/_Data/Skills/DrainVie.asset");
        var maledictionSombre  = Load<SkillSO>("Assets/_Data/Skills/MaledictionSombre.asset");
        var vagueObscure       = Load<SkillSO>("Assets/_Data/Skills/VagueObscure.asset");
        var frappeEmpoisonnee  = Load<SkillSO>("Assets/_Data/Skills/FrappeEmpoisonnee.asset");
        var coupCritique       = Load<SkillSO>("Assets/_Data/Skills/CoupCritique.asset");
        var gesteSournois      = Load<SkillSO>("Assets/_Data/Skills/GesteSournois.asset");
        var tirPrecis          = Load<SkillSO>("Assets/_Data/Skills/TirPrecis.asset");
        var flecheEmpoisonnee  = Load<SkillSO>("Assets/_Data/Skills/FlecheEmpoisonnee.asset");
        var pluieDeFleches     = Load<SkillSO>("Assets/_Data/Skills/PluieDeFleches.asset");
        var appelDesBetes      = Load<SkillSO>("Assets/_Data/Skills/AppelDesBetes.asset");
        var soinBestial        = Load<SkillSO>("Assets/_Data/Skills/SoinBestial.asset");
        var mordureEmpoisonnee = Load<SkillSO>("Assets/_Data/Skills/MordureEmpoisonnee.asset");
        var bouclierDAcier     = Load<SkillSO>("Assets/_Data/Skills/BouclierDAcier.asset");
        var frappeProvocatrice = Load<SkillSO>("Assets/_Data/Skills/FrappeProvocatrice.asset");
        var chargeDeFer        = Load<SkillSO>("Assets/_Data/Skills/ChargeDeFer.asset");
        var grenadeExplosive   = Load<SkillSO>("Assets/_Data/Skills/GrenadeExplosive.asset");
        var chocElectrique     = Load<SkillSO>("Assets/_Data/Skills/ChocElectrique.asset");
        var bombeAcide         = Load<SkillSO>("Assets/_Data/Skills/BombeAcide.asset");
        var analyseFaiblesse   = Load<SkillSO>("Assets/_Data/Skills/AnalyseFaiblesse.asset");
        var coupDemoralisant   = Load<SkillSO>("Assets/_Data/Skills/CoupDemoralisant.asset");
        var ralliementAllies   = Load<SkillSO>("Assets/_Data/Skills/RalliementAllies.asset");

        var guerrier    = Load<ClassSO>("Assets/_Data/Classes/Guerrier.asset");
        var mage        = Load<ClassSO>("Assets/_Data/Classes/Mage.asset");
        var soigneur    = Load<ClassSO>("Assets/_Data/Classes/Soigneur.asset");
        var demoniste   = Load<ClassSO>("Assets/_Data/Classes/Demoniste.asset");
        var voleur      = Load<ClassSO>("Assets/_Data/Classes/Voleur.asset");
        var archer      = Load<ClassSO>("Assets/_Data/Classes/Archer.asset");
        var invocateur  = Load<ClassSO>("Assets/_Data/Classes/Invocateur.asset");
        var tank        = Load<ClassSO>("Assets/_Data/Classes/Tank.asset");
        var ingenieur   = Load<ClassSO>("Assets/_Data/Classes/Ingenieur.asset");
        var eclaireur   = Load<ClassSO>("Assets/_Data/Classes/Eclaireur.asset");

        if (guerrier != null)   { guerrier.startingSkills   = new[] { attaqueBasique, frappeVigoureuse, tourbillon, coupParalysant };             EditorUtility.SetDirty(guerrier); }
        if (mage != null)       { mage.startingSkills       = new[] { attaqueBasique, bouleDeFeu, glaceCristal, foudre };                        EditorUtility.SetDirty(mage); }
        if (soigneur != null)   { soigneur.startingSkills   = new[] { attaqueBasique, soinBasique, soinZone, lumiereSacree };                     EditorUtility.SetDirty(soigneur); }
        if (demoniste != null)  { demoniste.startingSkills  = new[] { attaqueBasique, drainVie, maledictionSombre, vagueObscure };                EditorUtility.SetDirty(demoniste); }
        if (voleur != null)     { voleur.startingSkills     = new[] { attaqueBasique, frappeEmpoisonnee, coupCritique, gesteSournois };           EditorUtility.SetDirty(voleur); }
        if (archer != null)     { archer.startingSkills     = new[] { attaqueBasique, tirPrecis, flecheEmpoisonnee, pluieDeFleches };             EditorUtility.SetDirty(archer); }
        if (invocateur != null) { invocateur.startingSkills = new[] { attaqueBasique, appelDesBetes, soinBestial, mordureEmpoisonnee };           EditorUtility.SetDirty(invocateur); }
        if (tank != null)       { tank.startingSkills       = new[] { attaqueBasique, bouclierDAcier, frappeProvocatrice, chargeDeFer };          EditorUtility.SetDirty(tank); }
        if (ingenieur != null)  { ingenieur.startingSkills  = new[] { attaqueBasique, grenadeExplosive, chocElectrique, bombeAcide };             EditorUtility.SetDirty(ingenieur); }
        if (eclaireur != null)  { eclaireur.startingSkills  = new[] { attaqueBasique, analyseFaiblesse, coupDemoralisant, ralliementAllies };     EditorUtility.SetDirty(eclaireur); }

        AssetDatabase.SaveAssets();
        Debug.Log("RPG: Class skills updated successfully!");
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

        CreateRace("Elfe",
            raceName: "Elfe",
            description: "Agiles et magiques. Excellents mages et archers.",
            hp: 0f, mp: 0f, atk: 0f, def: 0f, mag: 0.10f, res: 0f, agi: 0.10f, lck: 0f,
            affinity: ElementType.Light,
            immunities: new StatusEffectType[0],
            passive: "+10% AGI, +10% MAG");

        CreateRace("Gnome",
            raceName: "Gnome",
            description: "Inventifs et studieux. Réduisent le coût de crafting.",
            hp: 0f, mp: 0f, atk: 0f, def: 0f, mag: 0.05f, res: 0f, agi: 0f, lck: 0f,
            affinity: ElementType.Lightning,
            immunities: new StatusEffectType[0],
            passive: "+5% MAG, coût crafting -20%");

        CreateRace("Androide",
            raceName: "Androïde",
            description: "Corps mécanique robuste, immunisé au poison.",
            hp: 0f, mp: 0f, atk: 0f, def: 0.15f, mag: 0f, res: 0f, agi: 0f, lck: 0f,
            affinity: ElementType.Lightning,
            immunities: new[] { StatusEffectType.Poison },
            passive: "+15% DEF, immunité Poison");

        CreateRace("PeuplesDesPlantes",
            raceName: "Peuples des Plantes",
            description: "Régénèrent lentement leurs HP à chaque tour.",
            hp: 0.05f, mp: 0f, atk: 0f, def: 0f, mag: 0f, res: 0f, agi: 0f, lck: 0f,
            affinity: ElementType.Nature,
            immunities: new StatusEffectType[0],
            passive: "+5% HP max, régénération HP +3% / tour");

        CreateRace("EspritsElementaires",
            raceName: "Esprits / Élémentaires",
            description: "Êtres de pure magie. Affinité élémentaire au choix à la création.",
            hp: 0f, mp: 0f, atk: 0f, def: 0f, mag: 0.15f, res: 0f, agi: 0f, lck: 0f,
            affinity: ElementType.None,
            immunities: new StatusEffectType[0],
            passive: "+15% MAG, affinité élémentaire variable");

        CreateRace("Dragons",
            raceName: "Dragons",
            description: "Puissants guerriers dotés d'un souffle élémentaire.",
            hp: 0f, mp: 0f, atk: 0.10f, def: 0f, mag: 0f, res: 0.10f, agi: 0f, lck: 0f,
            affinity: ElementType.Fire,
            immunities: new StatusEffectType[0],
            passive: "+10% ATK, +10% RES, souffle élémentaire 1×/combat");

        CreateRace("EtresEnergie",
            raceName: "Êtres d'énergie",
            description: "Composés de pure énergie. Coût des sorts réduit.",
            hp: 0f, mp: 0.20f, atk: 0f, def: 0f, mag: 0f, res: 0f, agi: 0f, lck: 0f,
            affinity: ElementType.Light,
            immunities: new StatusEffectType[0],
            passive: "+20% MP max, coût sorts -20%");

        CreateRace("MortVivants",
            raceName: "Mort-vivants",
            description: "Immunisés au poison et au gel. Drainent la vie en attaquant.",
            hp: 0f, mp: 0f, atk: 0f, def: 0f, mag: 0f, res: 0f, agi: 0f, lck: 0f,
            affinity: ElementType.Dark,
            immunities: new[] { StatusEffectType.Poison, StatusEffectType.Freeze },
            passive: "Immunité Poison+Gel, drain HP +5% sur attaque");

        CreateRace("Lycanthropes",
            raceName: "Lycanthropes",
            description: "Peuvent se transformer temporairement, décuplant leur puissance.",
            hp: 0f, mp: 0f, atk: 0f, def: 0f, mag: 0f, res: 0f, agi: 0f, lck: 0f,
            affinity: ElementType.Nature,
            immunities: new StatusEffectType[0],
            passive: "Transformation 3 tours (ATK+25%, AGI+25%) 1×/combat");

        CreateRace("ColossesDepPierre",
            raceName: "Colosses de Pierre",
            description: "Incroyablement résistants mais lents. Provoquent naturellement les ennemis.",
            hp: 0.10f, mp: 0f, atk: 0f, def: 0.25f, mag: 0f, res: 0f, agi: -0.10f, lck: 0f,
            affinity: ElementType.None,
            immunities: new StatusEffectType[0],
            passive: "+25% DEF, -10% AGI, provocation passive");
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

        var drainVie           = Load<SkillSO>("Assets/_Data/Skills/DrainVie.asset");
        var maledictionSombre  = Load<SkillSO>("Assets/_Data/Skills/MaledictionSombre.asset");
        var vagueObscure       = Load<SkillSO>("Assets/_Data/Skills/VagueObscure.asset");
        var frappeEmpoisonnee  = Load<SkillSO>("Assets/_Data/Skills/FrappeEmpoisonnee.asset");
        var coupCritique       = Load<SkillSO>("Assets/_Data/Skills/CoupCritique.asset");
        var gesteSournois      = Load<SkillSO>("Assets/_Data/Skills/GesteSournois.asset");
        var tirPrecis          = Load<SkillSO>("Assets/_Data/Skills/TirPrecis.asset");
        var flecheEmpoisonnee  = Load<SkillSO>("Assets/_Data/Skills/FlecheEmpoisonnee.asset");
        var pluieDeFleches     = Load<SkillSO>("Assets/_Data/Skills/PluieDeFleches.asset");
        var appelDesBetes      = Load<SkillSO>("Assets/_Data/Skills/AppelDesBetes.asset");
        var soinBestial        = Load<SkillSO>("Assets/_Data/Skills/SoinBestial.asset");
        var mordureEmpoisonnee = Load<SkillSO>("Assets/_Data/Skills/MordureEmpoisonnee.asset");
        var bouclierDAcier     = Load<SkillSO>("Assets/_Data/Skills/BouclierDAcier.asset");
        var frappeProvocatrice = Load<SkillSO>("Assets/_Data/Skills/FrappeProvocatrice.asset");
        var chargeDeFer        = Load<SkillSO>("Assets/_Data/Skills/ChargeDeFer.asset");
        var grenadeExplosive   = Load<SkillSO>("Assets/_Data/Skills/GrenadeExplosive.asset");
        var chocElectrique     = Load<SkillSO>("Assets/_Data/Skills/ChocElectrique.asset");
        var bombeAcide         = Load<SkillSO>("Assets/_Data/Skills/BombeAcide.asset");
        var analyseFaiblesse   = Load<SkillSO>("Assets/_Data/Skills/AnalyseFaiblesse.asset");
        var coupDemoralisant   = Load<SkillSO>("Assets/_Data/Skills/CoupDemoralisant.asset");
        var ralliementAllies   = Load<SkillSO>("Assets/_Data/Skills/RalliementAllies.asset");

        // ─── Démoniste ───
        CreateClass("Demoniste",
            className: "Démoniste",
            role: ClassRole.DPS,
            description: "Maître des ténèbres, drain et malédictions. Spé: Pacte Démoniaque / Nécromancie.",
            baseHP: 80,  baseMP: 90,  baseATK: 7,  baseDEF: 7,
            baseMAG: 15, baseRES: 10, baseAGI: 10, baseLCK: 9,
            hpGrowth: 10, mpGrowth: 14, atkGrowth: 1, defGrowth: 1,
            magGrowth: 4, resGrowth: 2, agiGrowth: 1, lckGrowth: 2,
            affinity: ElementType.Dark,
            skills: new SkillSO[] { attaqueBasique, drainVie, maledictionSombre, vagueObscure });

        // ─── Voleur ───
        CreateClass("Voleur",
            className: "Voleur",
            role: ClassRole.DPS,
            description: "Maître du critique, du poison et de l'esquive. Spé: Assassin / Filou.",
            baseHP: 85,  baseMP: 50,  baseATK: 12, baseDEF: 7,
            baseMAG: 5,  baseRES: 6,  baseAGI: 16, baseLCK: 12,
            hpGrowth: 11, mpGrowth: 6, atkGrowth: 2, defGrowth: 1,
            magGrowth: 1, resGrowth: 1, agiGrowth: 3, lckGrowth: 2,
            affinity: ElementType.None,
            skills: new SkillSO[] { attaqueBasique, frappeEmpoisonnee, coupCritique, gesteSournois });

        // ─── Archer ───
        CreateClass("Archer",
            className: "Archer",
            role: ClassRole.DPS,
            description: "Snipe à distance et debuffs. Spé: Chasseur / Rôdeur.",
            baseHP: 90,  baseMP: 60,  baseATK: 13, baseDEF: 8,
            baseMAG: 6,  baseRES: 7,  baseAGI: 14, baseLCK: 9,
            hpGrowth: 12, mpGrowth: 8, atkGrowth: 3, defGrowth: 1,
            magGrowth: 1, resGrowth: 1, agiGrowth: 2, lckGrowth: 1,
            affinity: ElementType.Nature,
            skills: new SkillSO[] { attaqueBasique, tirPrecis, flecheEmpoisonnee, pluieDeFleches });

        // ─── Invocateur ───
        CreateClass("Invocateur",
            className: "Invocateur",
            role: ClassRole.Summoner,
            description: "Convoque des créatures alliées au combat. Spé: Maître des Bêtes / Nécro-invocateur.",
            baseHP: 85,  baseMP: 85,  baseATK: 6,  baseDEF: 7,
            baseMAG: 13, baseRES: 11, baseAGI: 8,  baseLCK: 8,
            hpGrowth: 11, mpGrowth: 13, atkGrowth: 1, defGrowth: 1,
            magGrowth: 3, resGrowth: 2, agiGrowth: 1, lckGrowth: 1,
            affinity: ElementType.Nature,
            skills: new SkillSO[] { attaqueBasique, appelDesBetes, soinBestial, mordureEmpoisonnee });

        // ─── Tank ───
        CreateClass("Tank",
            className: "Tank",
            role: ClassRole.Tank,
            description: "Provocation et absorption de dégâts. Spé: Gardien / Paladin Noir.",
            baseHP: 150, baseMP: 40,  baseATK: 10, baseDEF: 18,
            baseMAG: 5,  baseRES: 15, baseAGI: 5,  baseLCK: 5,
            hpGrowth: 22, mpGrowth: 5, atkGrowth: 2, defGrowth: 4,
            magGrowth: 1, resGrowth: 3, agiGrowth: 0, lckGrowth: 1,
            affinity: ElementType.None,
            skills: new SkillSO[] { attaqueBasique, bouclierDAcier, frappeProvocatrice, chargeDeFer });

        // ─── Ingénieur ───
        CreateClass("Ingenieur",
            className: "Ingénieur",
            role: ClassRole.DPS,
            description: "Déploie tourelles, bombes et drones technologiques. Spé: Mécanicien / Alchimiste.",
            baseHP: 95,  baseMP: 70,  baseATK: 9,  baseDEF: 10,
            baseMAG: 11, baseRES: 8,  baseAGI: 10, baseLCK: 8,
            hpGrowth: 13, mpGrowth: 10, atkGrowth: 2, defGrowth: 2,
            magGrowth: 2, resGrowth: 1, agiGrowth: 1, lckGrowth: 1,
            affinity: ElementType.Lightning,
            skills: new SkillSO[] { attaqueBasique, grenadeExplosive, chocElectrique, bombeAcide });

        // ─── Éclaireur ───
        CreateClass("Eclaireur",
            className: "Éclaireur",
            role: ClassRole.Support,
            description: "Debuffs ennemis, buffs alliés et analyse tactique. Spé: Buffer/Debuffer / Analyste.",
            baseHP: 88,  baseMP: 75,  baseATK: 9,  baseDEF: 9,
            baseMAG: 10, baseRES: 10, baseAGI: 12, baseLCK: 11,
            hpGrowth: 12, mpGrowth: 11, atkGrowth: 1, defGrowth: 2,
            magGrowth: 2, resGrowth: 2, agiGrowth: 2, lckGrowth: 1,
            affinity: ElementType.None,
            skills: new SkillSO[] { attaqueBasique, analyseFaiblesse, coupDemoralisant, ralliementAllies });
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
        // ── Génériques (aucune restriction) ──────────────────────────────────
        CreateEquipment("EpeeEnFer",
            itemName: "Épée en Fer", description: "Une lame solide, taillée pour le combat.",
            slot: EquipmentSlot.MainWeapon, rarity: EquipmentRarity.Common,
            atk: 6, def: 0, hp: 0, mp: 0, mag: 0, res: 0, agi: 0, lck: 0,
            materials: new[] { "FerBrut", "Charbon" }, goldCost: 80,
            effectPool: new[] { "atk_3", "crit_5", "damage_on_hit" });

        CreateEquipment("BouclierEnBois",
            itemName: "Bouclier en Bois", description: "Protection basique en bois épais.",
            slot: EquipmentSlot.Offhand, rarity: EquipmentRarity.Common,
            atk: 0, def: 4, hp: 10, mp: 0, mag: 0, res: 0, agi: 0, lck: 0,
            materials: new[] { "BoisDur" }, goldCost: 50,
            effectPool: new[] { "def_2", "hp_regen", "elemental_resist" });

        CreateEquipment("CasqueEnCuir",
            itemName: "Casque en Cuir", description: "Un casque léger en cuir tanné.",
            slot: EquipmentSlot.Helmet, rarity: EquipmentRarity.Common,
            atk: 0, def: 2, hp: 15, mp: 0, mag: 0, res: 1, agi: 0, lck: 0,
            materials: new[] { "CuirBrut" }, goldCost: 40,
            effectPool: new[] { "def_2", "res_2", "hp_regen" });

        CreateEquipment("ArmureEnCuir",
            itemName: "Armure en Cuir", description: "Armure légère offrant une bonne mobilité.",
            slot: EquipmentSlot.Armor, rarity: EquipmentRarity.Common,
            atk: 0, def: 5, hp: 25, mp: 0, mag: 0, res: 2, agi: 0, lck: 0,
            materials: new[] { "CuirBrut", "FerBrut" }, goldCost: 100,
            effectPool: new[] { "def_3", "hp_regen", "elemental_resist" });

        CreateEquipment("BottesEnCuir",
            itemName: "Bottes en Cuir", description: "Des bottes confortables pour les longs voyages.",
            slot: EquipmentSlot.Boots, rarity: EquipmentRarity.Common,
            atk: 0, def: 1, hp: 0, mp: 0, mag: 0, res: 0, agi: 3, lck: 0,
            materials: new[] { "CuirBrut" }, goldCost: 30,
            effectPool: new[] { "agi_2", "lck_2", "crit_3" });

        CreateEquipment("AnneauBasique1",
            itemName: "Anneau Basique", description: "Un anneau simple qui porte chance.",
            slot: EquipmentSlot.Ring1, rarity: EquipmentRarity.Common,
            atk: 0, def: 0, hp: 0, mp: 5, mag: 0, res: 0, agi: 0, lck: 2,
            materials: new[] { "PierreGemme" }, goldCost: 60,
            effectPool: new[] { "lck_3", "crit_5", "mp_regen" });

        CreateEquipment("AnneauBasique2",
            itemName: "Anneau de Vigueur", description: "Un anneau qui renforce la résistance.",
            slot: EquipmentSlot.Ring2, rarity: EquipmentRarity.Common,
            atk: 0, def: 0, hp: 5, mp: 0, mag: 0, res: 2, agi: 0, lck: 0,
            materials: new[] { "PierreGemme" }, goldCost: 60,
            effectPool: new[] { "res_2", "hp_regen", "elemental_resist" });

        CreateEquipment("BatonArcanique",
            itemName: "Bâton Arcanique", description: "Un bâton chargé de puissance magique.",
            slot: EquipmentSlot.MainWeapon, rarity: EquipmentRarity.Common,
            atk: 0, def: 0, hp: 0, mp: 10, mag: 7, res: 0, agi: 0, lck: 0,
            materials: new[] { "CristalArcanique", "BoisDur" }, goldCost: 90,
            effectPool: new[] { "mag_4", "mp_regen", "crit_5" });

        CreateEquipment("EpeeRunique",
            itemName: "Épée Runique", description: "Lame gravée de runes anciennement oubliées.",
            slot: EquipmentSlot.MainWeapon, rarity: EquipmentRarity.Rare,
            atk: 10, def: 0, hp: 0, mp: 0, mag: 0, res: 0, agi: 2, lck: 0,
            materials: new[] { "FerRunique", "Charbon", "CristalArcanique" }, goldCost: 250,
            effectPool: new[] { "atk_5", "crit_8", "damage_on_hit", "elemental_fire" });

        // ── Équipements de Classe ────────────────────────────────────────────

        // Guerrier
        CreateEquipment("ArmeGuerrierC", "Épée du Guerrier",
            "Lame massive forgée pour les grands combattants.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Common,
            atk: 18, def: 5, hp: 10, mp: 0, mag: 0, res: 0, agi: 0, lck: 0,
            new[] { "FerBrut", "Charbon" }, 120, new[] { "atk_5", "crit_5", "damage_on_hit" },
            classes: new[] { "Guerrier" });

        CreateEquipment("ArmeGuerrierU", "Hache de Guerre",
            "Hache lourde qui fend les armures. Dégâts critiques dévastateurs.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Uncommon,
            atk: 26, def: -3, hp: 5, mp: 0, mag: 0, res: 0, agi: -2, lck: 0,
            new[] { "FerBrut", "Charbon", "PierreDure" }, 200, new[] { "atk_8", "crit_10", "damage_on_hit" },
            classes: new[] { "Guerrier" });

        CreateEquipment("ArmureGuerrierC", "Armure de Plates",
            "Armure rigide offrant une protection maximale.",
            EquipmentSlot.Armor, EquipmentRarity.Common,
            atk: 0, def: 22, hp: 35, mp: 0, mag: 0, res: 0, agi: -3, lck: 0,
            new[] { "FerBrut", "CuirBrut" }, 150, new[] { "def_5", "hp_regen", "elemental_resist" },
            classes: new[] { "Guerrier", "Tank" });

        CreateEquipment("ArmureGuerrierU", "Plastron du Héros",
            "Cuirasse portée par les héros légendaires. Force et gloire.",
            EquipmentSlot.Armor, EquipmentRarity.Uncommon,
            atk: 5, def: 28, hp: 45, mp: 0, mag: 0, res: 0, agi: -2, lck: 0,
            new[] { "FerRunique", "FerBrut" }, 260, new[] { "def_8", "hp_regen", "atk_5" },
            classes: new[] { "Guerrier" });

        // Mage
        CreateEquipment("ArmeMageC", "Grimoire Arcanique",
            "Ouvrage ancien amplifiant les sorts à l'extrême.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Common,
            atk: 0, def: 0, hp: 0, mp: 20, mag: 18, res: 5, agi: 0, lck: 0,
            new[] { "CristalArcanique", "ParcheminsAnciennes" }, 140, new[] { "mag_5", "mp_regen", "crit_5" },
            classes: new[] { "Mage" });

        CreateEquipment("ArmeMageU", "Baguette de Puissance",
            "Baguette qui concentre le mana en un faisceau dévastateur.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Uncommon,
            atk: 0, def: 0, hp: 0, mp: 30, mag: 24, res: 8, agi: 5, lck: 0,
            new[] { "CristalArcanique", "BoisDur", "EssenceMagique" }, 220, new[] { "mag_8", "mp_regen", "crit_8" },
            classes: new[] { "Mage" });

        CreateEquipment("ArmureMageC", "Robe Runique",
            "Robe légère tissée de runes protectrices.",
            EquipmentSlot.Armor, EquipmentRarity.Common,
            atk: 0, def: 0, hp: 0, mp: 30, mag: 12, res: 10, agi: 0, lck: 0,
            new[] { "SoieArcanique", "CristalArcanique" }, 130, new[] { "mag_5", "mp_regen", "res_4" },
            classes: new[] { "Mage", "Invocateur" });

        // Soigneur
        CreateEquipment("ArmeSoigneurC", "Sceptre Sacré",
            "Sceptre béni qui amplifie les soins et la résistance.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Common,
            atk: 0, def: 0, hp: 15, mp: 25, mag: 12, res: 8, agi: 0, lck: 0,
            new[] { "PierreGemme", "BoisDur" }, 130, new[] { "mag_5", "mp_regen", "res_4" },
            classes: new[] { "Soigneur" });

        CreateEquipment("ArmeSoigneurU", "Sceptre de Lumière",
            "Rayonne d'une lumière sacrée. Soins renforcés.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Uncommon,
            atk: 0, def: 0, hp: 20, mp: 30, mag: 18, res: 14, agi: 0, lck: 5,
            new[] { "PierreGemme", "EssenceSacree" }, 230, new[] { "mag_8", "mp_regen", "res_6" },
            classes: new[] { "Soigneur" });

        CreateEquipment("ArmureSoigneurC", "Toge Sacrée",
            "Vêtement sacré qui renforce la résistance et le mana.",
            EquipmentSlot.Armor, EquipmentRarity.Common,
            atk: 0, def: 0, hp: 15, mp: 25, mag: 0, res: 18, agi: 0, lck: 0,
            new[] { "SoieArcanique" }, 120, new[] { "res_5", "mp_regen", "hp_regen" },
            classes: new[] { "Soigneur" });

        // Démoniste
        CreateEquipment("ArmeDemonisteC", "Sceptre des Ombres",
            "Sceptre qui puise dans les forces obscures.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Common,
            atk: 0, def: 0, hp: 0, mp: 15, mag: 20, res: 0, agi: 0, lck: 5,
            new[] { "CendresDemon", "CristalArcanique" }, 150, new[] { "mag_6", "damage_on_hit", "crit_5" },
            classes: new[] { "Demoniste" });

        CreateEquipment("ArmeDemonisteU", "Tome Maudit",
            "Grimoire interdit. Puissance immense, âme en danger.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Uncommon,
            atk: 0, def: 0, hp: -10, mp: 20, mag: 28, res: 0, agi: 0, lck: 8,
            new[] { "CendresDemon", "SangObscur" }, 280, new[] { "mag_10", "damage_on_hit", "crit_8" },
            classes: new[] { "Demoniste" });

        CreateEquipment("ArmureDemonisteC", "Robe des Abysses",
            "Tissu imprégné d'ombres. Renforce la magie sombre.",
            EquipmentSlot.Armor, EquipmentRarity.Common,
            atk: 0, def: 0, hp: 0, mp: 20, mag: 14, res: 8, agi: 0, lck: 0,
            new[] { "CendresDemon", "SoieArcanique" }, 140, new[] { "mag_5", "damage_on_hit", "res_3" },
            classes: new[] { "Demoniste" });

        // Voleur
        CreateEquipment("ArmeVoleurC", "Dague Rapide",
            "Lame légère, parfaite pour frapper vite et disparaître.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Common,
            atk: 14, def: 0, hp: 0, mp: 0, mag: 0, res: 0, agi: 12, lck: 8,
            new[] { "FerBrut", "CuirBrut" }, 110, new[] { "atk_4", "agi_5", "crit_8" },
            classes: new[] { "Voleur" });

        CreateEquipment("ArmeVoleurU", "Lame Empoisonnée",
            "Dague enduite de venin. Chaque frappe peut empoisonner.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Uncommon,
            atk: 18, def: 0, hp: 0, mp: 0, mag: 0, res: 0, agi: 14, lck: 10,
            new[] { "FerBrut", "VeninsRares" }, 210, new[] { "atk_5", "agi_6", "crit_10", "damage_on_hit" },
            classes: new[] { "Voleur" });

        CreateEquipment("ArmureVoleurC", "Cuir de Nuit",
            "Armure de cuir sombre, silencieuse et souple.",
            EquipmentSlot.Armor, EquipmentRarity.Common,
            atk: 0, def: 10, hp: 0, mp: 0, mag: 0, res: 0, agi: 15, lck: 8,
            new[] { "CuirBrut" }, 130, new[] { "agi_6", "lck_5", "crit_5" },
            classes: new[] { "Voleur", "Eclaireur" });

        // Archer
        CreateEquipment("ArmeArcherC", "Arc de Précision",
            "Arc calibré pour des tirs longs et précis.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Common,
            atk: 16, def: 0, hp: 0, mp: 0, mag: 0, res: 0, agi: 8, lck: 6,
            new[] { "BoisDur", "NerfAnimal" }, 120, new[] { "atk_5", "agi_4", "crit_8" },
            classes: new[] { "Archer" });

        CreateEquipment("ArmeArcherU", "Arc Long Elfique",
            "Arc taillé dans le bois d'Ife. Portée et puissance sans égal.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Uncommon,
            atk: 22, def: 0, hp: 0, mp: 0, mag: 0, res: 0, agi: 12, lck: 10,
            new[] { "BoisSacre", "NerfAnimal" }, 230, new[] { "atk_8", "agi_6", "crit_12" },
            classes: new[] { "Archer" });

        CreateEquipment("ArmureArcherC", "Armure de Rôdeur",
            "Légère et résistante. Idéale pour les combats à distance.",
            EquipmentSlot.Armor, EquipmentRarity.Common,
            atk: 0, def: 10, hp: 0, mp: 0, mag: 0, res: 0, agi: 12, lck: 5,
            new[] { "CuirBrut" }, 120, new[] { "agi_5", "lck_4", "crit_5" },
            classes: new[] { "Archer", "Eclaireur" });

        // Invocateur
        CreateEquipment("ArmeInvocateurC", "Orbe d'Invocation",
            "Orbe cristallin qui ouvre un pont vers le monde des esprits.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Common,
            atk: 0, def: 0, hp: 0, mp: 30, mag: 18, res: 5, agi: 0, lck: 0,
            new[] { "CristalArcanique", "EssenceMagique" }, 150, new[] { "mag_6", "mp_regen", "res_4" },
            classes: new[] { "Invocateur" });

        CreateEquipment("ArmureInvocateurC", "Robe Spectrale",
            "Vêtement translucide tissé de fils d'éther.",
            EquipmentSlot.Armor, EquipmentRarity.Common,
            atk: 0, def: 0, hp: 0, mp: 35, mag: 10, res: 12, agi: 0, lck: 0,
            new[] { "SoieArcanique", "EssenceMagique" }, 140, new[] { "mag_4", "mp_regen", "res_5" },
            classes: new[] { "Invocateur", "Mage" });

        // Tank
        CreateEquipment("ArmeTankC", "Masse de Guerre",
            "Arme lourde qui fracasse armures et boucliers.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Common,
            atk: 14, def: 12, hp: 20, mp: 0, mag: 0, res: 0, agi: -2, lck: 0,
            new[] { "FerBrut", "PierreDure" }, 140, new[] { "atk_5", "def_5", "damage_on_hit" },
            classes: new[] { "Tank" });

        CreateEquipment("ArmureTankC", "Armure Forteresse",
            "Armure monumentale. Presque indestructible.",
            EquipmentSlot.Armor, EquipmentRarity.Common,
            atk: 0, def: 28, hp: 55, mp: 0, mag: 0, res: 0, agi: -6, lck: 0,
            new[] { "FerBrut", "PierreDure", "CuirBrut" }, 180, new[] { "def_8", "hp_regen", "elemental_resist" },
            classes: new[] { "Tank" });

        CreateEquipment("ArmureTankU", "Cuirasse Imprenable",
            "Chef-d'œuvre des forgerons. Résiste à tout.",
            EquipmentSlot.Armor, EquipmentRarity.Uncommon,
            atk: 0, def: 36, hp: 70, mp: 0, mag: 0, res: 5, agi: -8, lck: 0,
            new[] { "FerRunique", "PierreDure" }, 320, new[] { "def_10", "hp_regen", "elemental_resist" },
            classes: new[] { "Tank" });

        // Ingénieur
        CreateEquipment("ArmeIngenieurC", "Pistolet à Vapeur",
            "Arme à feu mécanique propulsant des projectiles à grande vitesse.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Common,
            atk: 14, def: 0, hp: 0, mp: 0, mag: 8, res: 0, agi: 6, lck: 0,
            new[] { "FerBrut", "PiecesMecaniques" }, 130, new[] { "atk_5", "mag_4", "agi_4" },
            classes: new[] { "Ingenieur" });

        CreateEquipment("ArmeIngenieurU", "Lance-Grenades",
            "Lance plusieurs grenades explosives par salve.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Uncommon,
            atk: 20, def: 0, hp: 0, mp: 0, mag: 12, res: 0, agi: 5, lck: 0,
            new[] { "FerBrut", "PiecesMecaniques", "Poudre" }, 250, new[] { "atk_8", "mag_5", "damage_on_hit" },
            classes: new[] { "Ingenieur" });

        CreateEquipment("ArmureIngenieurC", "Combinaison Blindée",
            "Combinaison mécanique renforcée avec capteurs intégrés.",
            EquipmentSlot.Armor, EquipmentRarity.Common,
            atk: 0, def: 14, hp: 20, mp: 0, mag: 5, res: 0, agi: 4, lck: 0,
            new[] { "FerBrut", "PiecesMecaniques", "CuirBrut" }, 155, new[] { "def_5", "mag_3", "agi_3" },
            classes: new[] { "Ingenieur", "Androide" });

        // Éclaireur
        CreateEquipment("ArmeEclaireurC", "Lame Légère",
            "Épée fine et rapide, parfaite pour les combattants mobiles.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Common,
            atk: 13, def: 0, hp: 0, mp: 0, mag: 0, res: 0, agi: 12, lck: 5,
            new[] { "FerBrut" }, 110, new[] { "atk_4", "agi_5", "crit_6" },
            classes: new[] { "Eclaireur" });

        CreateEquipment("ArmureEclaireurU", "Tenue d'Infiltration",
            "Combinaison légère permettant de se fondre dans l'ombre.",
            EquipmentSlot.Armor, EquipmentRarity.Uncommon,
            atk: 0, def: 10, hp: 0, mp: 0, mag: 0, res: 8, agi: 20, lck: 5,
            new[] { "CuirBrut", "FibresSpeciales" }, 200, new[] { "agi_8", "lck_5", "crit_6" },
            classes: new[] { "Eclaireur" });

        // ── Équipements Raciaux ──────────────────────────────────────────────

        // Humain
        CreateEquipment("ArmeHumainU", "Épée Royale",
            "Symbole de la royauté humaine. Polyvalente et noble.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Uncommon,
            atk: 14, def: 5, hp: 0, mp: 0, mag: 0, res: 0, agi: 0, lck: 8,
            new[] { "FerRunique", "PierreGemme" }, 200, new[] { "atk_5", "lck_5", "crit_6" },
            races: new[] { "Humain" });

        CreateEquipment("ArmureHumainU", "Armure Royale",
            "Armure aux couleurs du royaume. Inspire confiance et autorité.",
            EquipmentSlot.Armor, EquipmentRarity.Uncommon,
            atk: 5, def: 14, hp: 20, mp: 0, mag: 0, res: 0, agi: 0, lck: 0,
            new[] { "FerBrut", "CuirBrut", "PierreGemme" }, 220, new[] { "def_5", "atk_4", "hp_regen" },
            races: new[] { "Humain" });

        // Elfe
        CreateEquipment("ArmeElfeU", "Arc Sylvestre",
            "Arc taillé dans le cœur d'un chêne millénaire elfique.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Uncommon,
            atk: 12, def: 0, hp: 0, mp: 0, mag: 10, res: 0, agi: 10, lck: 5,
            new[] { "BoisSacre" }, 210, new[] { "atk_5", "mag_4", "agi_6", "crit_8" },
            races: new[] { "Elfe" });

        CreateEquipment("ArmureElfeC", "Toge Sylvestre",
            "Tissu elfique tissé avec des fils de lune. Léger et résistant.",
            EquipmentSlot.Armor, EquipmentRarity.Common,
            atk: 0, def: 0, hp: 0, mp: 0, mag: 12, res: 10, agi: 8, lck: 0,
            new[] { "SoieArcanique", "BoisSacre" }, 150, new[] { "mag_5", "agi_5", "res_4" },
            races: new[] { "Elfe" });

        // Lycanthropes
        CreateEquipment("ArmeLycanthrU", "Griffes Bestiales",
            "Extensions naturelles de la bête. Rapides et déchirantes.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Uncommon,
            atk: 20, def: 0, hp: 15, mp: 0, mag: 0, res: 0, agi: 10, lck: 0,
            new[] { "OsRares", "CuirBrut" }, 200, new[] { "atk_8", "agi_6", "damage_on_hit" },
            races: new[] { "Lycanthropes" });

        CreateEquipment("ArmureLycanthrC", "Peau de Bête",
            "Armure naturelle renforcée par la transformation bestiale.",
            EquipmentSlot.Armor, EquipmentRarity.Common,
            atk: 0, def: 10, hp: 40, mp: 0, mag: 0, res: 0, agi: 6, lck: 0,
            new[] { "CuirBrut", "OsRares" }, 140, new[] { "hp_regen", "def_4", "agi_4" },
            races: new[] { "Lycanthropes" });

        // Gnome
        CreateEquipment("ArmeGnomeU", "Pistolet Mécanique",
            "Chef-d'œuvre de miniaturisation. Précis et puissant.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Uncommon,
            atk: 10, def: 0, hp: 0, mp: 0, mag: 12, res: 0, agi: 0, lck: 12,
            new[] { "PiecesMecaniques", "FerBrut" }, 210, new[] { "lck_6", "mag_5", "crit_10" },
            races: new[] { "Gnome" });

        CreateEquipment("ArmureGnomeC", "Veste de Bricoleur",
            "Veste bardée d'outils et de gadgets utiles.",
            EquipmentSlot.Armor, EquipmentRarity.Common,
            atk: 0, def: 8, hp: 0, mp: 0, mag: 8, res: 0, agi: 0, lck: 10,
            new[] { "PiecesMecaniques", "CuirBrut" }, 130, new[] { "mag_4", "lck_5", "crit_5" },
            races: new[] { "Gnome" });

        // Androïde
        CreateEquipment("ArmeAndroideU", "Bras Laser",
            "Bras cybernétique intégrant un canon laser haute énergie.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Uncommon,
            atk: 16, def: 5, hp: 0, mp: 0, mag: 10, res: 0, agi: 0, lck: 0,
            new[] { "PiecesMecaniques", "EssenceMagique" }, 250, new[] { "atk_6", "mag_5", "damage_on_hit" },
            races: new[] { "Androide" });

        CreateEquipment("ArmureAndroideU", "Exosquelette de Combat",
            "Structure mécanique externe. Augmente force et résistance.",
            EquipmentSlot.Armor, EquipmentRarity.Uncommon,
            atk: 0, def: 18, hp: 25, mp: 0, mag: 0, res: 0, agi: -2, lck: 0,
            new[] { "PiecesMecaniques", "FerBrut" }, 280, new[] { "def_8", "hp_regen", "atk_3" },
            races: new[] { "Androide" });

        // Peuples des Plantes
        CreateEquipment("ArmePlanteU", "Bâton de Racine",
            "Bâton vivant qui puise la force de la terre.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Uncommon,
            atk: 0, def: 0, hp: 20, mp: 0, mag: 14, res: 10, agi: 0, lck: 0,
            new[] { "SevePlante", "BoisDur" }, 200, new[] { "mag_6", "hp_regen", "res_5" },
            races: new[] { "PeuplesDesPlantes" });

        CreateEquipment("ArmurePlanteU", "Écorce Vivante",
            "Armure d'écorce régénérative. Guérit après chaque victoire.",
            EquipmentSlot.Armor, EquipmentRarity.Uncommon,
            atk: 0, def: 8, hp: 20, mp: 0, mag: 0, res: 15, agi: 0, lck: 0,
            new[] { "SevePlante", "BoisDur" }, 220, new[] { "res_6", "hp_regen", "heal_on_kill" },
            races: new[] { "PeuplesDesPlantes" });

        // Esprits Élémentaires
        CreateEquipment("ArmeEspritsU", "Lame d'Éther",
            "Lame cristallisée d'énergie pure. Résiste à tous les éléments.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Uncommon,
            atk: 0, def: 0, hp: 0, mp: 15, mag: 20, res: 10, agi: 0, lck: 0,
            new[] { "CristalEther", "EssenceMagique" }, 250, new[] { "mag_8", "res_6", "elemental_resist" },
            races: new[] { "EspritsElementaires" });

        CreateEquipment("ArmureEspritsU", "Voile d'Essence",
            "Armure immatérielle tissée d'énergie élémentaire.",
            EquipmentSlot.Armor, EquipmentRarity.Uncommon,
            atk: 0, def: 0, hp: 0, mp: 20, mag: 14, res: 18, agi: 0, lck: 0,
            new[] { "CristalEther" }, 240, new[] { "mag_6", "res_8", "elemental_resist" },
            races: new[] { "EspritsElementaires" });

        // Dragons
        CreateEquipment("ArmeDragonR", "Griffe du Dragon",
            "Griffe naturelle d'un dragon. Déchire tout sur son passage.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Rare,
            atk: 24, def: 0, hp: 10, mp: 0, mag: 14, res: 0, agi: 0, lck: 0,
            new[] { "EcailleDragon", "OsRares" }, 350, new[] { "atk_10", "mag_6", "damage_on_hit", "crit_8" },
            races: new[] { "Dragons" });

        CreateEquipment("ArmureDragonR", "Écaille de Dragon",
            "Armure d'écailles draconiques. Résiste au feu et aux éléments.",
            EquipmentSlot.Armor, EquipmentRarity.Rare,
            atk: 0, def: 20, hp: 40, mp: 0, mag: 0, res: 15, agi: 0, lck: 0,
            new[] { "EcailleDragon" }, 380, new[] { "def_10", "res_8", "elemental_resist" },
            races: new[] { "Dragons" });

        // Êtres d'énergie
        CreateEquipment("ArmeEnergieU", "Sceptre d'Énergie",
            "Sceptre qui canalise l'énergie pure de l'univers.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Uncommon,
            atk: 0, def: 0, hp: 0, mp: 25, mag: 20, res: 10, agi: 0, lck: 0,
            new[] { "EssenceMagique", "CristalArcanique" }, 260, new[] { "mag_8", "mp_regen", "res_5" },
            races: new[] { "EtresEnergie" });

        CreateEquipment("ArmureEnergieC", "Aura Cristalline",
            "Armure d'énergie cristallisée qui absorbe les dégâts magiques.",
            EquipmentSlot.Armor, EquipmentRarity.Common,
            atk: 0, def: 0, hp: 0, mp: 22, mag: 14, res: 14, agi: 0, lck: 0,
            new[] { "EssenceMagique" }, 170, new[] { "mag_5", "res_6", "mp_regen" },
            races: new[] { "EtresEnergie" });

        // Mort-vivants
        CreateEquipment("ArmeMortVivantsU", "Faux des Ombres",
            "Lame forgée dans l'obscurité. Draine la vie des ennemis.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Uncommon,
            atk: 18, def: 0, hp: 0, mp: 0, mag: 12, res: 0, agi: 0, lck: -5,
            new[] { "OsRares", "CendresDemon" }, 230, new[] { "atk_6", "mag_5", "damage_on_hit" },
            races: new[] { "MortVivants" });

        CreateEquipment("ArmureMortVivantsC", "Linceul Maudit",
            "Tissu de mort qui renforce les pouvoirs obscurs.",
            EquipmentSlot.Armor, EquipmentRarity.Common,
            atk: 0, def: 6, hp: 25, mp: 0, mag: 12, res: 0, agi: 0, lck: -5,
            new[] { "OsRares" }, 140, new[] { "mag_5", "hp_regen", "damage_on_hit" },
            races: new[] { "MortVivants" });

        // Colosses de Pierre
        CreateEquipment("ArmeCOlossesU", "Poing de Roc",
            "Gant de pierre massif. Chaque coup fait trembler le sol.",
            EquipmentSlot.MainWeapon, EquipmentRarity.Uncommon,
            atk: 22, def: 12, hp: 25, mp: 0, mag: 0, res: 0, agi: -5, lck: 0,
            new[] { "PierreDure", "FerBrut" }, 260, new[] { "atk_8", "def_6", "damage_on_hit" },
            races: new[] { "ColossesDepPierre" });

        CreateEquipment("ArmureColossesC", "Carapace de Pierre",
            "Armure naturelle de roc solide. Presque impossible à percer.",
            EquipmentSlot.Armor, EquipmentRarity.Common,
            atk: 0, def: 26, hp: 55, mp: 0, mag: 0, res: 0, agi: -10, lck: 0,
            new[] { "PierreDure" }, 180, new[] { "def_10", "hp_regen", "elemental_resist" },
            races: new[] { "ColossesDepPierre" });
    }

    private static void CreateEquipment(string assetName, string itemName, string description,
        EquipmentSlot slot, EquipmentRarity rarity,
        int atk, int def, int hp, int mp, int mag, int res, int agi, int lck,
        string[] materials, int goldCost, string[] effectPool,
        string[] classes = null, string[] races = null)
    {
        string path = $"Assets/_Data/Equipment/{assetName}.asset";
        if (AssetDatabase.LoadAssetAtPath<EquipmentSO>(path) != null) return;
        var so = ScriptableObject.CreateInstance<EquipmentSO>();
        so.itemName          = itemName;
        so.description       = description;
        so.slot              = slot;
        so.rarity            = rarity;
        so.atkBonus          = atk;
        so.defBonus          = def;
        so.hpBonus           = hp;
        so.mpBonus           = mp;
        so.magBonus          = mag;
        so.resBonus          = res;
        so.agiBonus          = agi;
        so.lckBonus          = lck;
        so.craftingMaterials = materials;
        so.craftingGoldCost  = goldCost;
        so.effectPool        = effectPool;

        if (classes != null)
        {
            var list = new System.Collections.Generic.List<ClassSO>();
            foreach (var c in classes)
            {
                var cls = AssetDatabase.LoadAssetAtPath<ClassSO>($"Assets/_Data/Classes/{c}.asset");
                if (cls != null) list.Add(cls);
            }
            so.allowedClasses = list.ToArray();
        }

        if (races != null)
        {
            var list = new System.Collections.Generic.List<RaceSO>();
            foreach (var r in races)
            {
                var race = AssetDatabase.LoadAssetAtPath<RaceSO>($"Assets/_Data/Races/{r}.asset");
                if (race != null) list.Add(race);
            }
            so.allowedRaces = list.ToArray();
        }

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

    // ─────────────────────────── CAMPAIGN ENEMIES ───────────────────────────

    private static void CreateCampaignEnemies()
    {
        var botNormal = CreateOrLoad<BotBrain>("Assets/_Data/AI/BotBrain_Normal.asset");

        CreateEnemy("Squelette",
            enemyName: "Squelette",
            affinity: ElementType.Dark,
            hp: 60, mp: 0, atk: 18, def: 8, mag: 0, res: 5, agi: 10, lck: 5,
            xpReward: 30, botBrain: botNormal);

        CreateEnemy("ArcherSquelette",
            enemyName: "Archer Squelette",
            affinity: ElementType.Dark,
            hp: 45, mp: 0, atk: 22, def: 5, mag: 0, res: 5, agi: 15, lck: 8,
            xpReward: 35, botBrain: botNormal);

        CreateEnemy("GolemOs",
            enemyName: "Golem d'Os",
            affinity: ElementType.Dark,
            hp: 90, mp: 0, atk: 15, def: 18, mag: 0, res: 10, agi: 6, lck: 3,
            xpReward: 50, botBrain: botNormal);

        CreateEnemy("RoiSquelette",
            enemyName: "Roi Squelette",
            affinity: ElementType.Dark,
            hp: 300, mp: 80, atk: 30, def: 15, mag: 25, res: 15, agi: 12, lck: 10,
            xpReward: 500, botBrain: botNormal);
    }

    private static EnemySO CreateEnemy(string assetName, string enemyName,
        ElementType affinity, int hp, int mp, int atk, int def,
        int mag, int res, int agi, int lck, int xpReward, BotBrain botBrain)
    {
        var enemy = CreateOrLoad<EnemySO>($"Assets/_Data/Enemies/{assetName}.asset");
        enemy.enemyName         = enemyName;
        enemy.elementalAffinity = affinity;
        enemy.hp  = hp;  enemy.mp  = mp;
        enemy.atk = atk; enemy.def = def;
        enemy.mag = mag; enemy.res = res;
        enemy.agi = agi; enemy.lck = lck;
        enemy.xpReward = xpReward;
        enemy.botBrain = botBrain;
        EditorUtility.SetDirty(enemy);
        return enemy;
    }

    // ─────────────────────────── CAMPAIGN ZONES ─────────────────────────────

    private static void CreateCampaignZones()
    {
        var botNormal = CreateOrLoad<BotBrain>("Assets/_Data/AI/BotBrain_Normal.asset");

        var squelette = CreateOrLoad<EnemySO>("Assets/_Data/Enemies/Squelette.asset");
        var archer    = CreateOrLoad<EnemySO>("Assets/_Data/Enemies/ArcherSquelette.asset");
        var golem     = CreateOrLoad<EnemySO>("Assets/_Data/Enemies/GolemOs.asset");
        var roi       = CreateOrLoad<EnemySO>("Assets/_Data/Enemies/RoiSquelette.asset");

        var village = CreateOrLoad<CampaignZoneSO>("Assets/_Data/Zones/Zone_Village.asset");
        village.zoneName        = "Village de Départ";
        village.sceneKey        = "Village";
        village.flagKey         = "village_visited";
        village.enemyPool       = new EnemySO[0];
        village.boss            = null;
        village.defaultBotBrain = botNormal;
        EditorUtility.SetDirty(village);

        var donjon = CreateOrLoad<CampaignZoneSO>("Assets/_Data/Zones/Zone_Donjon.asset");
        donjon.zoneName            = "Donjon du Roi Squelette";
        donjon.sceneKey            = "Donjon";
        donjon.flagKey             = "donjon_visited";
        donjon.bossDefeatedFlagKey = "boss_defeated";
        donjon.enemyPool           = new[] { squelette, archer, golem };
        donjon.boss                = roi;
        donjon.defaultBotBrain     = botNormal;
        EditorUtility.SetDirty(donjon);
    }

    // ─────────────────────────── GAME DATA REGISTRY ─────────────────────────

    private static void CreateGameDataRegistry()
    {
        EnsureFolder("Assets/Resources");
        var registry = CreateOrLoad<GameDataRegistry>("Assets/Resources/GameDataRegistry.asset");

        var guerrier   = CreateOrLoad<ClassSO>("Assets/_Data/Classes/Guerrier.asset");
        var mage       = CreateOrLoad<ClassSO>("Assets/_Data/Classes/Mage.asset");
        var soigneur   = CreateOrLoad<ClassSO>("Assets/_Data/Classes/Soigneur.asset");
        var demoniste  = CreateOrLoad<ClassSO>("Assets/_Data/Classes/Demoniste.asset");
        var voleur     = CreateOrLoad<ClassSO>("Assets/_Data/Classes/Voleur.asset");
        var archer     = CreateOrLoad<ClassSO>("Assets/_Data/Classes/Archer.asset");
        var invocateur = CreateOrLoad<ClassSO>("Assets/_Data/Classes/Invocateur.asset");
        var tank       = CreateOrLoad<ClassSO>("Assets/_Data/Classes/Tank.asset");
        var ingenieur  = CreateOrLoad<ClassSO>("Assets/_Data/Classes/Ingenieur.asset");
        var eclaireur  = CreateOrLoad<ClassSO>("Assets/_Data/Classes/Eclaireur.asset");
        registry.classes = new[] { guerrier, mage, soigneur, demoniste, voleur, archer, invocateur, tank, ingenieur, eclaireur };

        var humain            = CreateOrLoad<RaceSO>("Assets/_Data/Races/Humain.asset");
        var elfe              = CreateOrLoad<RaceSO>("Assets/_Data/Races/Elfe.asset");
        var gnome             = CreateOrLoad<RaceSO>("Assets/_Data/Races/Gnome.asset");
        var androide          = CreateOrLoad<RaceSO>("Assets/_Data/Races/Androide.asset");
        var peuplesDesPlantes = CreateOrLoad<RaceSO>("Assets/_Data/Races/PeuplesDesPlantes.asset");
        var esprits           = CreateOrLoad<RaceSO>("Assets/_Data/Races/EspritsElementaires.asset");
        var dragons           = CreateOrLoad<RaceSO>("Assets/_Data/Races/Dragons.asset");
        var etresEnergie      = CreateOrLoad<RaceSO>("Assets/_Data/Races/EtresEnergie.asset");
        var mortVivants       = CreateOrLoad<RaceSO>("Assets/_Data/Races/MortVivants.asset");
        var lycanthrope       = CreateOrLoad<RaceSO>("Assets/_Data/Races/Lycanthropes.asset");
        var colossespPierre   = CreateOrLoad<RaceSO>("Assets/_Data/Races/ColossesDepPierre.asset");
        registry.races = new[] { humain, elfe, gnome, androide, peuplesDesPlantes, esprits, dragons, etresEnergie, mortVivants, lycanthrope, colossespPierre };

        var loup    = CreateOrLoad<CompanionSO>("Assets/_Data/Companions/LoupDesOmbres.asset");
        var corbeau = CreateOrLoad<CompanionSO>("Assets/_Data/Companions/CorbeauAnalyste.asset");
        var fee     = CreateOrLoad<CompanionSO>("Assets/_Data/Companions/FeeSylvestre.asset");
        registry.companions = new[] { loup, corbeau, fee };

        EditorUtility.SetDirty(registry);
    }

    // ════════════════════════════════════════════════════════════════════════
    // SPRITES — BATCH FIX
    // ════════════════════════════════════════════════════════════════════════

    [MenuItem("RPG/Fix Village Sprites (Single Mode)")]
    public static void FixVillageSprites()
    {
        string folder = "Assets/Sprites/Village";
        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
        int count = 0;
        foreach (var guid in guids)
        {
            var path     = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16f;
            importer.filterMode          = FilterMode.Point;
            importer.textureCompression  = TextureImporterCompression.Uncompressed;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            count++;
        }
        Debug.Log($"[RPG] {count} sprites → Single mode, PPU=16, Point filter.");
    }

    // ════════════════════════════════════════════════════════════════════════
    // VILLAGE VISUAL BUILDER  —  Style Zelda top-down
    // Carte : 64×50 unités  |  Caméra orthographic size=5
    // Sorting : basé sur pos.y inversée (objets du nord rendus en premier)
    // ════════════════════════════════════════════════════════════════════════

    [MenuItem("RPG/Build Village Visuals")]
    public static void BuildVillageVisuals()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        if (scene.name != "Village")
        {
            Debug.LogError("[VillageBuilder] Ouvrir la scène Village avant de lancer.");
            return;
        }

        // Supprimer l'ancien visuel
        var old = GameObject.Find("_VillageVisuals");
        if (old != null) Undo.DestroyObjectImmediate(old);

        var root = new GameObject("_VillageVisuals");
        Undo.RegisterCreatedObjectUndo(root, "Build Village Zelda");

        var W = GetWhiteSprite();

        // ────────────────────────────────────────────────────────────────
        // 1. TERRAIN DE BASE  (plan le plus bas, z=10)
        // ────────────────────────────────────────────────────────────────
        // Herbe principale
        Lay(root,"Herbe",          W, Hex("3A7D2E"), V(0,0),      V(66,52), 100);
        // Zones d'herbe plus claire (variation)
        Lay(root,"Herbe_Clair_A",  W, Hex("42882F"), V(-8, 6),    V(14,10), 99);
        Lay(root,"Herbe_Clair_B",  W, Hex("42882F"), V(10,-8),    V(10, 8), 99);
        Lay(root,"Herbe_Clair_C",  W, Hex("42882F"), V(-5,-12),   V(8, 6),  99);
        // Lisière sombre (bord de carte)
        Lay(root,"Lisiere_N",      W, Hex("1E4A15"), V(0, 26),    V(66,2),  98);
        Lay(root,"Lisiere_S",      W, Hex("1E4A15"), V(0,-26),    V(66,2),  98);
        Lay(root,"Lisiere_E",      W, Hex("1E4A15"), V(33, 0),    V(2,52),  98);
        Lay(root,"Lisiere_O",      W, Hex("1E4A15"), V(-33,0),    V(2,52),  98);

        // ────────────────────────────────────────────────────────────────
        // 2. CHEMINS  (z=9)
        // ────────────────────────────────────────────────────────────────
        // Route principale N-S
        Lay(root,"Route_V_Bord",   W, Hex("7A6644"), V(0,0),      V(3.6f,52), 90);
        Lay(root,"Route_V",        W, Hex("C4AA72"), V(0,0),      V(3.0f,52), 89);
        // Route principale E-O (centre)
        Lay(root,"Route_H_Bord",   W, Hex("7A6644"), V(0,-2),     V(66,3.6f), 90);
        Lay(root,"Route_H",        W, Hex("C4AA72"), V(0,-2),     V(66,3.0f), 89);
        // Embranchements vers les bâtiments
        Lay(root,"Chemin_Auberge", W, Hex("B09A62"), V(-11,10),   V(8,2.5f),  88);
        Lay(root,"Chemin_Temple",  W, Hex("B09A62"), V(0, 15),    V(2.5f,8),  88);
        Lay(root,"Chemin_Forge",   W, Hex("B09A62"), V(12, 10),   V(8,2.5f),  88);
        Lay(root,"Chemin_Enchant", W, Hex("B09A62"), V(-12,-12),  V(8,2.5f),  88);
        Lay(root,"Chemin_Apotek",  W, Hex("B09A62"), V(12,-12),   V(8,2.5f),  88);
        Lay(root,"Chemin_Ferme",   W, Hex("B09A62"), V(-16,-6),   V(2.5f,10), 88);
        Lay(root,"Chemin_Mare",    W, Hex("B09A62"), V(16,-4),    V(2.5f,8),  88);

        // Plaza centrale (pavée)
        Lay(root,"Plaza_Bord",     W, Hex("8A8070"), V(0, 3),     V(12,10),   87);
        Lay(root,"Plaza",          W, Hex("A8967C"), V(0, 3),     V(11,9),    86);
        // Détails pavés (lignes)
        for (int px = -4; px <= 4; px += 2)
            Lay(root,$"Pave_H_{px}",W,Hex("9A8A6A"),V(px,3),V(0.08f,8.5f),85);
        for (int py = -1; py <= 7; py += 2)
            Lay(root,$"Pave_V_{py}",W,Hex("9A8A6A"),V(0,py),V(10.5f,0.08f),85);

        // ────────────────────────────────────────────────────────────────
        // 3. EAU / MARE  (z=9)
        // ────────────────────────────────────────────────────────────────
        Lay(root,"Mare_Fond",      W, Hex("2255AA"), V(18,-10),   V(9,7),     90);
        Lay(root,"Mare_Eau",       W, Hex("3370CC"), V(18,-10),   V(8,6),     89);
        Lay(root,"Mare_Reflet",    W, Hex("4488EE"), V(17,-10),   V(2.5f,1.5f),88);
        Lay(root,"Mare_Nenuph",    W, Hex("2A8840"), V(18.5f,-9.5f),V(1,0.6f),87);
        // Berges
        Lay(root,"Berge_N",        W, Hex("4A7A30"), V(18,-7.2f), V(9,0.5f),  86);
        Lay(root,"Berge_S",        W, Hex("4A7A30"), V(18,-12.8f),V(9,0.5f),  86);

        // ────────────────────────────────────────────────────────────────
        // 4. ZONE FERME  (z=9)
        // ────────────────────────────────────────────────────────────────
        Lay(root,"Ferme_Sol",      W, Hex("8B5E2A"), V(-17,-8),   V(10,8),    90);
        // Rangées de cultures
        for (int row = 0; row < 4; row++)
        {
            float ry = -6f + row * 1.8f;
            Lay(root,$"Culture_{row}A",W,Hex("2E7A20"),V(-18f,ry),V(3.5f,1.0f),88);
            Lay(root,$"Culture_{row}B",W,Hex("3A8A2A"),V(-15f,ry),V(3.5f,1.0f),88);
        }
        // Clôture ferme
        VCloture(root, W, -17f, -8f, 10f, 8f);

        // ────────────────────────────────────────────────────────────────
        // 5. BÂTIMENTS  —  style Zelda top-down
        //    Chaque bâtiment : Ombre → Sol → Mur_S → Toit → Détails
        // ────────────────────────────────────────────────────────────────

        // Temple / Église (nord, grand)
        ZBat(root, W, "Temple",
             cx:0,   cy:20,  w:10, h:8,
             toit:   Hex("7A3030"),
             mur:    Hex("E8DCC8"),
             murS:   Hex("D4C4A8"),
             porte:  Hex("4A2800"),
             nFen:2, couleurToit2: Hex("5A2020"));

        // Auberge (NO)
        ZBat(root, W, "Auberge",
             cx:-16, cy:12,  w:9,  h:7,
             toit:   Hex("8B4513"),
             mur:    Hex("DEB887"),
             murS:   Hex("C8A070"),
             porte:  Hex("3A1800"),
             nFen:2, couleurToit2: Hex("6B3010"));

        // Forgeron (NE)
        ZBat(root, W, "Forgeron",
             cx:16,  cy:12,  w:8,  h:6,
             toit:   Hex("444455"),
             mur:    Hex("888899"),
             murS:   Hex("6A6A7A"),
             porte:  Hex("222230"),
             nFen:1, couleurToit2: Hex("333344"));

        // Enchanteur (SO)
        ZBat(root, W, "Enchanteur",
             cx:-16, cy:-14, w:8,  h:6,
             toit:   Hex("4A1A7A"),
             mur:    Hex("9A70C8"),
             murS:   Hex("7A50A8"),
             porte:  Hex("200840"),
             nFen:2, couleurToit2: Hex("350F5A"));

        // Apothicaire (SE)
        ZBat(root, W, "Apothicaire",
             cx:16,  cy:-14, w:7,  h:5,
             toit:   Hex("2A6A30"),
             mur:    Hex("70C880"),
             murS:   Hex("50A860"),
             porte:  Hex("0F3015"),
             nFen:1, couleurToit2: Hex("1A4A20"));

        // Maison 1 (ONO)
        ZBat(root, W, "Maison1",
             cx:-20, cy:2,   w:6,  h:5,
             toit:   Hex("8B3030"),
             mur:    Hex("E8C890"),
             murS:   Hex("D0AA70"),
             porte:  Hex("3A1800"),
             nFen:1, couleurToit2: Hex("6B2020"));

        // Maison 2 (ENE)
        ZBat(root, W, "Maison2",
             cx:20,  cy:2,   w:6,  h:5,
             toit:   Hex("4A7030"),
             mur:    Hex("D8C890"),
             murS:   Hex("C0AA70"),
             porte:  Hex("2A1800"),
             nFen:1, couleurToit2: Hex("356020"));

        // Maison PNJ Sauvegarde (proche du centre)
        ZBat(root, W, "MaisonSauvegarde",
             cx:-6,  cy:12,  w:5,  h:4,
             toit:   Hex("305080"),
             mur:    Hex("B0C8E0"),
             murS:   Hex("90A8C0"),
             porte:  Hex("102040"),
             nFen:1, couleurToit2: Hex("204060"));

        // ────────────────────────────────────────────────────────────────
        // 6. PUITS CENTRAL
        // ────────────────────────────────────────────────────────────────
        Lay(root,"Puits_Ombre",  W, Hex("00000055"), V(0.3f,3.2f), V(2.2f,2.2f), 60);
        Lay(root,"Puits_Base",   W, Hex("7A7060"),   V(0,3.5f),    V(2.0f,2.0f), 59);
        Lay(root,"Puits_Eau",    W, Hex("2255AA"),   V(0,3.5f),    V(1.4f,1.4f), 58);
        Lay(root,"Puits_Reflet", W, Hex("4488EE"),   V(-0.2f,3.6f),V(0.4f,0.3f), 57);
        Lay(root,"Puits_MurN",   W, Hex("9A8A70"),   V(0,4.2f),    V(2.2f,0.5f), 56);
        Lay(root,"Puits_MurE",   W, Hex("8A7A60"),   V(0.9f,3.5f), V(0.4f,2.0f), 56);
        Lay(root,"Puits_MurO",   W, Hex("8A7A60"),   V(-0.9f,3.5f),V(0.4f,2.0f), 56);
        Lay(root,"Puits_Toit",   W, Hex("6B4513"),   V(0,4.5f),    V(2.6f,0.6f), 55);

        // ────────────────────────────────────────────────────────────────
        // 7. ARBRES  (groupes denses style Zelda)
        // ────────────────────────────────────────────────────────────────
        // Forêt Nord
        VForet(root, W, new Vector2[]{
            V(-28,22),V(-25,23),V(-22,21),V(-18,22),V(-14,23),V(-10,22),V(-6,23),
            V(6,23),V(10,22),V(14,23),V(18,22),V(22,21),V(26,23),V(28,22),
            V(-28,18),V(-24,19),V(-20,20),V(-16,19),V(16,19),V(20,20),V(24,19),V(28,18),
        });
        // Forêt Sud
        VForet(root, W, new Vector2[]{
            V(-28,-22),V(-24,-23),V(-20,-21),V(-16,-22),V(-8,-23),
            V(8,-23),V(16,-22),V(20,-21),V(24,-23),V(28,-22),
        });
        // Bois latéraux O
        VForet(root, W, new Vector2[]{
            V(-28,14),V(-29,10),V(-28,6),V(-29,2),V(-28,-2),V(-29,-6),V(-28,-10),V(-29,-14),V(-28,-18),
        });
        // Bois latéraux E
        VForet(root, W, new Vector2[]{
            V(28,14),V(29,10),V(28,6),V(29,2),V(28,-2),V(29,-6),V(28,-10),V(29,-14),V(28,-18),
        });
        // Arbres isolés décoratifs
        VForet(root, W, new Vector2[]{
            V(-10,16),V(10,16),V(-8,-5),V(8,-5),V(-22,8),V(22,8),V(-5,18),V(5,18),
        });

        // ────────────────────────────────────────────────────────────────
        // 8. DÉCORATIONS
        // ────────────────────────────────────────────────────────────────
        // Fleurs
        VFleurs(root, W, new Vector2[]{
            V(-4,7),V(4,7),V(-3,0),V(3,0),V(-9,14),V(9,14),
            V(-14,5),V(14,5),V(-14,-4),V(14,-4),V(0,14),
        });
        // Rochers
        VRocher(root, W, 0, V(-7,14)); VRocher(root, W, 1, V(7,14));
        VRocher(root, W, 2, V(-22,14));VRocher(root, W, 3, V(22,14));
        VRocher(root, W, 4, V(-10,-18));VRocher(root, W, 5, V(10,-18));
        // Barils (décorations portes)
        VBaril(root, W, V(-18.5f,9));   // Auberge
        VBaril(root, W, V(-17.5f,9));
        VBaril(root, W, V(13.5f,8.5f)); // Forgeron
        VBaril(root, W, V(14.5f,8.5f));
        // Lampadaires
        VLamp(root, W, V(-3.5f,7));  VLamp(root, W, V(3.5f,7));
        VLamp(root, W, V(-3.5f,-1)); VLamp(root, W, V(3.5f,-1));

        // ────────────────────────────────────────────────────────────────
        // 9. SORTIES (portails lumineux)
        // ────────────────────────────────────────────────────────────────
        // Sud → WorldMap
        Lay(root,"Sortie_WM_Glow", W, Hex("FFEE8860"), V(0,-24),   V(4,1.5f),  75);
        Lay(root,"Sortie_WM",      W, Hex("FFD700"),   V(0,-24),   V(3,0.6f),  74);
        Lay(root,"Sortie_WM_L",    W, Hex("FFA500"),   V(-1.3f,-23.8f),V(0.2f,1.5f),73);
        Lay(root,"Sortie_WM_R",    W, Hex("FFA500"),   V(1.3f,-23.8f), V(0.2f,1.5f),73);
        // Nord → Donjon
        Lay(root,"Sortie_DJ_Glow", W, Hex("FF448860"), V(0, 24),   V(4,1.5f),  75);
        Lay(root,"Sortie_DJ",      W, Hex("CC3333"),   V(0, 24),   V(3,0.6f),  74);
        Lay(root,"Sortie_DJ_L",    W, Hex("992222"),   V(-1.3f,23.8f),V(0.2f,1.5f),73);
        Lay(root,"Sortie_DJ_R",    W, Hex("992222"),   V(1.3f,23.8f), V(0.2f,1.5f),73);

        // ────────────────────────────────────────────────────────────────
        // 10. CAMÉRA — réglages de base
        // ────────────────────────────────────────────────────────────────
        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographicSize = 5f;
            cam.backgroundColor  = new Color(0.13f, 0.28f, 0.10f);
        }

        // ────────────────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("[VillageBuilder] Village Zelda construit (64×50). Camera follow ajoutée.");
    }

    // ── Bâtiment style Zelda top-down ────────────────────────────────────
    static void ZBat(GameObject root, Sprite W,
                     string id, float cx, float cy, float w, float h,
                     Color toit, Color mur, Color murS, Color porte,
                     int nFen, Color couleurToit2)
    {
        int so = -(int)(cy * 10f); // sorting par position Y (sud devant nord)

        // Ombre portée
        SRS(root, id+"_Ombre",  W, new Color(0,0,0,0.35f),
            V(cx+0.3f, cy-0.35f), V(w,h), so+5);
        // Sol / empreinte bâtiment
        SRS(root, id+"_Sol",    W, murS,
            V(cx, cy), V(w,h), so+4);
        // Toit (corps principal vu du dessus)
        SRS(root, id+"_Toit",   W, toit,
            V(cx, cy+h*0.12f), V(w, h*0.72f), so+3);
        // Bande de toit secondaire (faîtage)
        SRS(root, id+"_Toit2",  W, couleurToit2,
            V(cx, cy+h*0.32f), V(w*0.85f, h*0.18f), so+2);
        // Mur sud (visible de dessous)
        SRS(root, id+"_MurS",   W, mur,
            V(cx, cy-h*0.32f), V(w, h*0.28f), so+1);
        // Porte
        SRS(root, id+"_Porte",  W, porte,
            V(cx, cy-h*0.44f), V(w*0.18f, h*0.28f), so);
        // Fenêtres (dans le mur sud)
        if (nFen >= 1)
        {
            SRS(root, id+"_FenL", W, new Color(0.7f,0.85f,1f),
                V(cx - w*0.28f, cy-h*0.32f), V(w*0.12f, h*0.14f), so);
            SRS(root, id+"_FenR", W, new Color(0.7f,0.85f,1f),
                V(cx + w*0.28f, cy-h*0.32f), V(w*0.12f, h*0.14f), so);
        }
        if (nFen >= 2)
        {
            SRS(root, id+"_FenL2",W, new Color(0.6f,0.8f,1f),
                V(cx - w*0.10f, cy-h*0.32f), V(w*0.10f, h*0.12f), so);
            SRS(root, id+"_FenR2",W, new Color(0.6f,0.8f,1f),
                V(cx + w*0.10f, cy-h*0.32f), V(w*0.10f, h*0.12f), so);
        }
        // Cheminée
        SRS(root, id+"_Cheminee",W, new Color(0.4f,0.35f,0.3f),
            V(cx+w*0.3f, cy+h*0.38f), V(w*0.08f, h*0.22f), so+1);
    }

    // ── Forêt dense (groupe d'arbres) ────────────────────────────────────
    static void VForet(GameObject root, Sprite W, Vector2[] positions)
    {
        int idx = System.Array.IndexOf(
            System.Array.FindAll(root.GetComponentsInChildren<SpriteRenderer>(), _ => true),
            null); // juste pour avoir un idx unique
        for (int i = 0; i < positions.Length; i++)
        {
            var p = positions[i];
            float sz = 1.8f + (i % 4) * 0.3f;
            int so = -(int)(p.y * 10f);
            SRS(root,$"Arbre_Fond_{i}_{(int)p.x}", W, Hex("103010"),
                V(p.x+sz*0.08f,p.y-sz*0.05f), V(sz*1.1f,sz*1.1f), so+3);
            SRS(root,$"Arbre_Feu_{i}_{(int)p.x}",  W, Hex("1E5C15"),
                V(p.x,p.y+sz*0.06f),           V(sz,sz),            so+2);
            SRS(root,$"Arbre_Ctr_{i}_{(int)p.x}",  W, Hex("28781C"),
                V(p.x-sz*0.05f,p.y+sz*0.1f),  V(sz*0.65f,sz*0.65f),so+1);
            SRS(root,$"Arbre_Trc_{i}_{(int)p.x}",  W, Hex("6B4012"),
                V(p.x,p.y-sz*0.4f),            V(sz*0.2f,sz*0.45f), so);
        }
    }

    // ── Fleurs décoratives ────────────────────────────────────────────────
    static void VFleurs(GameObject root, Sprite W, Vector2[] positions)
    {
        Color[] fc = { Hex("FF6688"), Hex("FFEE44"), Hex("FF8833"), Hex("CC44FF"), Hex("44CCFF") };
        for (int i = 0; i < positions.Length; i++)
        {
            var p = positions[i];
            int so = -(int)(p.y*10f);
            SRS(root,$"Fleur_Tige_{i}", W, Hex("2A6010"), V(p.x,p.y), V(0.15f,0.35f), so);
            SRS(root,$"Fleur_{i}",      W, fc[i % fc.Length], V(p.x,p.y+0.2f), V(0.35f,0.35f), so-1);
        }
    }

    // ── Rocher ────────────────────────────────────────────────────────────
    static void VRocher(GameObject root, Sprite W, int i, Vector2 p)
    {
        int so = -(int)(p.y*10f);
        SRS(root,$"Roc_Ombre_{i}", W, new Color(0,0,0,0.3f), V(p.x+0.1f,p.y-0.1f), V(1.1f,0.7f), so+2);
        SRS(root,$"Roc_Base_{i}",  W, Hex("7A7570"),          V(p.x,p.y),            V(1.0f,0.65f),so+1);
        SRS(root,$"Roc_Haut_{i}",  W, Hex("9A9590"),          V(p.x-0.05f,p.y+0.1f),V(0.75f,0.5f),so);
    }

    // ── Baril ─────────────────────────────────────────────────────────────
    static void VBaril(GameObject root, Sprite W, Vector2 p)
    {
        int i = (int)(p.x * 10 + p.y);
        int so = -(int)(p.y*10f);
        SRS(root,$"Baril_Corps_{i}", W, Hex("8B5E2A"), V(p.x,p.y),      V(0.55f,0.7f), so+1);
        SRS(root,$"Baril_Bande_{i}",W, Hex("444444"),  V(p.x,p.y+0.1f), V(0.6f,0.1f),  so);
        SRS(root,$"Baril_Bande2{i}",W, Hex("444444"),  V(p.x,p.y-0.1f), V(0.6f,0.1f),  so);
    }

    // ── Lampadaire ────────────────────────────────────────────────────────
    static void VLamp(GameObject root, Sprite W, Vector2 p)
    {
        int i = (int)(p.x * 100 + p.y);
        int so = -(int)(p.y*10f);
        SRS(root,$"Lamp_Poteau_{i}", W, Hex("555540"), V(p.x,p.y-0.3f),  V(0.1f,1.0f),  so+1);
        SRS(root,$"Lamp_Lueur_{i}",  W, new Color(1f,0.95f,0.5f,0.4f), V(p.x,p.y+0.6f), V(0.9f,0.9f), so-1);
        SRS(root,$"Lamp_Globe_{i}",  W, Hex("FFEE88"), V(p.x,p.y+0.6f),  V(0.35f,0.35f),so);
    }

    // ── Clôture (rectangle) ───────────────────────────────────────────────
    static void VCloture(GameObject root, Sprite W, float cx, float cy, float w, float h)
    {
        Color c = Hex("9A7A40");
        Lay(root,"Clot_N", W, c, V(cx,     cy+h/2f), V(w,0.2f), 75);
        Lay(root,"Clot_S", W, c, V(cx,     cy-h/2f), V(w,0.2f), 75);
        Lay(root,"Clot_E", W, c, V(cx+w/2f,cy),      V(0.2f,h), 75);
        Lay(root,"Clot_O", W, c, V(cx-w/2f,cy),      V(0.2f,h), 75);
        // Piquets
        for (float px = cx-w/2f+1f; px < cx+w/2f; px += 2f)
        {
            Lay(root,$"Piquet_{(int)(px*10)}", W, Hex("7A5A25"),
                V(px, cy+h/2f), V(0.15f,0.4f), 76);
            Lay(root,$"PiquetS_{(int)(px*10)}", W, Hex("7A5A25"),
                V(px, cy-h/2f), V(0.15f,0.4f), 76);
        }
    }

    // ── Helpers de création ───────────────────────────────────────────────

    // Sprite positionné dans le monde (sorting explicite)
    static void SRS(GameObject parent, string name, Sprite spr, Color color,
                    Vector2 pos, Vector2 size, int sortOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.position   = new Vector3(pos.x, pos.y, 0f);
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = spr;
        sr.color        = color;
        sr.sortingOrder = sortOrder;
    }

    // Raccourci (Lay = Layer / plan)
    static void Lay(GameObject parent, string name, Sprite spr, Color color,
                    Vector2 pos, Vector2 size, int sortOrder)
        => SRS(parent, name, spr, color, pos, size, sortOrder);

    // Vector2 raccourci
    static Vector2 V(float x, float y) => new Vector2(x, y);

    // Hex color parser (#RRGGBB ou #RRGGBBAA)
    static Color Hex(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length == 6)
            hex += "FF";
        if (hex.Length != 8) return Color.magenta;
        float r = System.Convert.ToInt32(hex.Substring(0,2),16)/255f;
        float g = System.Convert.ToInt32(hex.Substring(2,2),16)/255f;
        float b = System.Convert.ToInt32(hex.Substring(4,2),16)/255f;
        float a = System.Convert.ToInt32(hex.Substring(6,2),16)/255f;
        return new Color(r,g,b,a);
    }

    static Sprite GetWhiteSprite()
    {
        var spr = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        if (spr != null) return spr;
        var tex = new Texture2D(4,4,TextureFormat.RGBA32,false);
        var pix = new Color[16];
        for (int i = 0; i < 16; i++) pix[i] = Color.white;
        tex.SetPixels(pix); tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0,0,4,4), Vector2.one*0.5f, 4f);
    }

    // ── Fill Ground Tilemap ───────────────────────────────────────────────
    [MenuItem("RPG/Fill Ground With Grass")]
    public static void FillGroundWithGrass()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        if (scene.name != "Village")
        {
            Debug.LogError("[RPG] Ouvrir la scène Village avant de lancer.");
            return;
        }

        // Trouver la tilemap Ground
        UnityEngine.Tilemaps.Tilemap ground = null;
        foreach (var go in scene.GetRootGameObjects())
        {
            ground = FindTilemap(go, "Ground");
            if (ground != null) break;
        }
        if (ground == null) { Debug.LogError("[RPG] Tilemap 'Ground' introuvable."); return; }

        // Charger tile_0001
        var tile = AssetDatabase.LoadAssetAtPath<UnityEngine.Tilemaps.TileBase>(
            "Assets/Sprites/Village/tile_0001.asset");
        if (tile == null) { Debug.LogError("[RPG] tile_0001.asset introuvable dans Assets/Sprites/Village/"); return; }

        // Remplir la zone -40,-30 à 40,30 (80x60 tiles, centré sur l'origine)
        int xMin = -40, xMax = 40, yMin = -30, yMax = 30;
        var positions = new System.Collections.Generic.List<Vector3Int>();
        var tiles     = new System.Collections.Generic.List<UnityEngine.Tilemaps.TileBase>();
        for (int x = xMin; x <= xMax; x++)
            for (int y = yMin; y <= yMax; y++)
            {
                positions.Add(new Vector3Int(x, y, 0));
                tiles.Add(tile);
            }

        Undo.RecordObject(ground, "Fill Ground");
        ground.SetTiles(positions.ToArray(), tiles.ToArray());
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log($"[RPG] Ground rempli avec tile_0001 sur {positions.Count} cases !");
    }

    static UnityEngine.Tilemaps.Tilemap FindTilemap(GameObject go, string name)
    {
        if (go.name == name)
        {
            var tm = go.GetComponent<UnityEngine.Tilemaps.Tilemap>();
            if (tm != null) return tm;
        }
        foreach (Transform child in go.transform)
        {
            var result = FindTilemap(child.gameObject, name);
            if (result != null) return result;
        }
        return null;
    }
}
#endif
