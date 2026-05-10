using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Génère les visuels de la scène Village : sol, chemins, bâtiments, arbres, décorations.
/// Menu : RPG > Build Village Visuals
/// </summary>
public static class VillageBuilder
{
    // ── Couleurs palette ─────────────────────────────────────────────────
    static readonly Color C_Herbe      = new Color(0.29f, 0.62f, 0.24f);
    static readonly Color C_HerbeClr   = new Color(0.35f, 0.70f, 0.28f);
    static readonly Color C_Chemin     = new Color(0.72f, 0.62f, 0.44f);
    static readonly Color C_CheminBord = new Color(0.60f, 0.50f, 0.34f);
    static readonly Color C_Auberge    = new Color(0.72f, 0.44f, 0.22f);
    static readonly Color C_AubergeToit= new Color(0.55f, 0.22f, 0.12f);
    static readonly Color C_Forgeron   = new Color(0.38f, 0.38f, 0.42f);
    static readonly Color C_ForgeronT  = new Color(0.22f, 0.22f, 0.26f);
    static readonly Color C_Enchanteur = new Color(0.35f, 0.18f, 0.55f);
    static readonly Color C_EnchanteurT= new Color(0.20f, 0.08f, 0.40f);
    static readonly Color C_Maison     = new Color(0.82f, 0.72f, 0.56f);
    static readonly Color C_MaisonToit = new Color(0.60f, 0.28f, 0.20f);
    static readonly Color C_Boutique   = new Color(0.56f, 0.72f, 0.50f);
    static readonly Color C_BoutiqueT  = new Color(0.30f, 0.52f, 0.25f);
    static readonly Color C_Arbre      = new Color(0.15f, 0.48f, 0.12f);
    static readonly Color C_ArbreFonce = new Color(0.10f, 0.32f, 0.08f);
    static readonly Color C_Tronc      = new Color(0.42f, 0.28f, 0.12f);
    static readonly Color C_Puits      = new Color(0.50f, 0.46f, 0.40f);
    static readonly Color C_Porte      = new Color(0.30f, 0.18f, 0.08f);
    static readonly Color C_Fenetre    = new Color(0.70f, 0.85f, 1.00f);
    static readonly Color C_Sortie     = new Color(0.90f, 0.85f, 0.30f);
    static readonly Color C_Eau        = new Color(0.25f, 0.55f, 0.90f);
    static readonly Color C_Cloture    = new Color(0.65f, 0.52f, 0.35f);

    public static void Build()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Village")
        {
            Debug.LogError("[VillageBuilder] Ouvrir la scène Village avant de lancer.");
            return;
        }

        // Supprimer l'ancien groupe visuel s'il existe
        var old = GameObject.Find("_VillageVisuals");
        if (old != null) { Undo.DestroyObjectImmediate(old); }

        var root = new GameObject("_VillageVisuals");
        Undo.RegisterCreatedObjectUndo(root, "Build Village");

        // ── Sprites de base ───────────────────────────────────────────────
        var sWhite = GetWhiteSprite();

        // ── Sol ──────────────────────────────────────────────────────────
        MakeSprite(root, "Sol",          sWhite, C_Herbe,      new Vector3(0, 0, 1),   new Vector3(32, 24));
        MakeSprite(root, "Sol_Accent",   sWhite, C_HerbeClr,   new Vector3(-4, 3, 0.9f), new Vector3(8, 6));
        MakeSprite(root, "Sol_Accent2",  sWhite, C_HerbeClr,   new Vector3(5, -4, 0.9f), new Vector3(6, 5));

        // ── Chemins ───────────────────────────────────────────────────────
        MakeSprite(root, "Chemin_H",     sWhite, C_CheminBord, new Vector3(0, 0, 0.8f), new Vector3(32, 2.4f));
        MakeSprite(root, "Chemin_H_Ctr", sWhite, C_Chemin,     new Vector3(0, 0, 0.7f), new Vector3(32, 2.0f));
        MakeSprite(root, "Chemin_V",     sWhite, C_CheminBord, new Vector3(0, 0, 0.8f), new Vector3(2.4f, 24));
        MakeSprite(root, "Chemin_V_Ctr", sWhite, C_Chemin,     new Vector3(0, 0, 0.7f), new Vector3(2.0f, 24));

        // ── Bâtiments ─────────────────────────────────────────────────────
        // Auberge (haut-gauche)
        MakeBatiment(root, sWhite, "Auberge",    new Vector3(-7f, 5.5f),  new Vector3(5f, 4f),
                     C_Auberge, C_AubergeToit, C_Porte, C_Fenetre, "Auberge");

        // Forgeron (haut-droite)
        MakeBatiment(root, sWhite, "Forgeron",   new Vector3(7f, 5.5f),   new Vector3(4.5f, 3.5f),
                     C_Forgeron, C_ForgeronT, C_Porte, C_Fenetre, "Forgeron");

        // Enchanteur (bas-gauche)
        MakeBatiment(root, sWhite, "Enchanteur", new Vector3(-7f, -5.5f), new Vector3(4.5f, 3.5f),
                     C_Enchanteur, C_EnchanteurT, C_Porte, C_Fenetre, "Enchanteur");

        // Maison Villageois (bas-droite)
        MakeBatiment(root, sWhite, "MaisonVillageois", new Vector3(7f, -5.5f), new Vector3(4f, 3f),
                     C_Maison, C_MaisonToit, C_Porte, C_Fenetre, "Villageois");

        // Boutique Apothicaire (haut-centre)
        MakeBatiment(root, sWhite, "Apothicaire", new Vector3(0f, 7.5f),  new Vector3(3.5f, 3f),
                     C_Boutique, C_BoutiqueT, C_Porte, C_Fenetre, "Apothicaire");

        // ── Puits central ─────────────────────────────────────────────────
        MakeSprite(root, "Puits_Base",  sWhite, C_Puits,      new Vector3(0, 1.5f, 0.5f),  new Vector3(1.4f, 1.4f));
        MakeSprite(root, "Puits_Eau",   sWhite, C_Eau,        new Vector3(0, 1.5f, 0.4f),  new Vector3(0.8f, 0.8f));
        MakeSprite(root, "Puits_Bord",  sWhite, C_Cloture,    new Vector3(0, 2.1f, 0.35f), new Vector3(1.6f, 0.25f));

        // ── Arbres ────────────────────────────────────────────────────────
        var treePositions = new Vector2[]
        {
            new(-11f,  8f), new(-10f, 6f), new(-12f,  4f), new(-11f, -2f),
            new(-10f, -7f), new(-12f,-9f), new(-11f,-10f),
            new( 11f,  8f), new( 10f, 5f), new( 12f,  2f), new( 11f, -3f),
            new( 10f, -8f), new( 12f,-9f),
            new(-3f,  10f), new( 0f, 10f), new( 3f, 10f), new(-5f, 10f), new( 5f, 9.5f),
            new(-3f, -10f), new( 0f,-10f), new( 3f,-10f), new(-5f,-10f), new( 5f,-10f),
        };
        for (int i = 0; i < treePositions.Length; i++)
            MakeArbre(root, sWhite, i, treePositions[i]);

        // ── Clôtures latérales ────────────────────────────────────────────
        for (int i = -7; i <= 7; i += 2)
        {
            if (Mathf.Abs(i) < 2) continue; // laisser passage au centre
            MakeSprite(root, $"Clot_H_T_{i}", sWhite, C_Cloture, new Vector3(i, 11.5f, 0.6f), new Vector3(1.4f, 0.2f));
            MakeSprite(root, $"Clot_H_B_{i}", sWhite, C_Cloture, new Vector3(i,-11.5f, 0.6f), new Vector3(1.4f, 0.2f));
        }

        // ── Sorties ───────────────────────────────────────────────────────
        MakeSortie(root, sWhite, "Sortie_WorldMap",  new Vector3(0, -11.8f, 0.3f), new Vector3(3f, 0.6f), "WorldMap");
        MakeSortie(root, sWhite, "Sortie_Donjon",    new Vector3(0,  11.8f, 0.3f), new Vector3(3f, 0.6f), "Donjon");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[VillageBuilder] Village construit et sauvegardé !");
    }

    // ── Helpers ─────────────────────────────────────────────────────────

    static void MakeBatiment(GameObject parent, Sprite spr, string id,
        Vector3 pos, Vector3 size,
        Color murs, Color toit, Color porte, Color fenetre, string label)
    {
        float z = 0.5f;
        // Corps
        MakeSprite(parent, id + "_Murs", spr, murs,
            new Vector3(pos.x, pos.y - size.y * 0.1f, z), size);
        // Toit (bande supérieure)
        MakeSprite(parent, id + "_Toit", spr, toit,
            new Vector3(pos.x, pos.y + size.y * 0.42f, z - 0.05f),
            new Vector3(size.x, size.y * 0.22f));
        // Porte
        MakeSprite(parent, id + "_Porte", spr, porte,
            new Vector3(pos.x, pos.y - size.y * 0.38f, z - 0.05f),
            new Vector3(size.x * 0.22f, size.y * 0.30f));
        // Fenêtres
        MakeSprite(parent, id + "_Fen_L", spr, fenetre,
            new Vector3(pos.x - size.x * 0.28f, pos.y + size.y * 0.05f, z - 0.05f),
            new Vector3(size.x * 0.16f, size.y * 0.16f));
        MakeSprite(parent, id + "_Fen_R", spr, fenetre,
            new Vector3(pos.x + size.x * 0.28f, pos.y + size.y * 0.05f, z - 0.05f),
            new Vector3(size.x * 0.16f, size.y * 0.16f));
    }

    static void MakeArbre(GameObject parent, Sprite spr, int idx, Vector2 pos)
    {
        float z  = 0.6f;
        float sz = Random.Range(1.2f, 1.8f);
        MakeSprite(parent, $"Arbre_Tronc_{idx}", spr,
            new Color(0.42f, 0.28f, 0.12f),
            new Vector3(pos.x, pos.y - sz * 0.35f, z),
            new Vector3(sz * 0.25f, sz * 0.4f));
        MakeSprite(parent, $"Arbre_Ombre_{idx}", spr,
            new Color(0.10f, 0.32f, 0.08f),
            new Vector3(pos.x + sz * 0.1f, pos.y + sz * 0.05f, z - 0.02f),
            new Vector3(sz * 1.05f, sz * 1.05f));
        MakeSprite(parent, $"Arbre_Feuilles_{idx}", spr,
            new Color(0.15f, 0.48f, 0.12f),
            new Vector3(pos.x, pos.y + sz * 0.1f, z - 0.04f),
            new Vector3(sz, sz));
    }

    static void MakeSortie(GameObject parent, Sprite spr, string id,
        Vector3 pos, Vector3 size, string label)
    {
        MakeSprite(parent, id, spr, C_Sortie, pos, size);
    }

    static GameObject MakeSprite(GameObject parent, string name, Sprite spr,
        Color color, Vector3 pos, Vector3 scale)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = spr;
        sr.color  = color;
        sr.sortingOrder = Mathf.RoundToInt(-pos.z * 10f);
        return go;
    }

    static Sprite GetWhiteSprite()
    {
        // Utilise le sprite UI blanc intégré d'Unity (Background)
        var spr = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        if (spr != null) return spr;

        // Fallback : crée un sprite depuis une texture 4x4 blanche
        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        var pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }
}
