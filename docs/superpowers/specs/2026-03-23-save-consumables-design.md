# Save/Load Consumables — Design Spec
Date: 2026-03-23

## Contexte

La persistence du compagnon est déjà entièrement implémentée dans `GameSession.Save()` / `Load()`.
Il manque uniquement la persistence des consommables de l'inventaire.

## Ce qui existe déjà

- `Inventory.Consumables` — `List<ConsumableSO>` avec `AddConsumable` / `RemoveConsumable`
- `ConsumableSO` — ScriptableObject avec champ `itemName` (clé d'identification)
- `GameDataRegistry` — registre central chargé via `Resources.Load`, contient déjà `companions[]` et `GetCompanion(key)`
- `SaveData.companionKey` + `GameSession.Save/Load` compagnon — entièrement fonctionnel

## Changements

### SaveData.cs

Ajouter un champ sérialisable :

```csharp
public List<string> consumableKeys = new List<string>();
```

### GameDataRegistry.cs

Ajouter tableau + méthode de lookup :

```csharp
[Header("Consumables")]
public ConsumableSO[] consumables;

public ConsumableSO GetConsumable(string key)
{
    if (consumables == null || key == null) return null;
    foreach (var c in consumables)
        if (c != null && c.itemName == key) return c;
    return null;
}
```

### GameSession.cs — Save()

Après la boucle `equippedItemKeys`, ajouter :

```csharp
foreach (var c in ActiveCharacter.Inventory.Consumables)
    if (c != null) data.consumableKeys.Add(c.itemName);
```

### GameSession.cs — Load()

Après la boucle `equippedItemKeys`, ajouter :

```csharp
foreach (var key in data.consumableKeys)
{
    var consumable = registry.GetConsumable(key);
    if (consumable != null) character.Inventory.AddConsumable(consumable);
}
```

## Comportement

- Les consommables sont identifiés par `itemName` (comme les équipements par `itemName`)
- Les doublons sont supportés : 2 potions = 2 entrées dans `consumableKeys`
- Si un `itemName` est introuvable dans le registre au chargement, il est silencieusement ignoré (cohérent avec le comportement existant des équipements)
- Les consommables assets (`PotionDeSoin`, `Antidote`, `Ether`) doivent être ajoutés manuellement dans `GameDataRegistry.consumables[]` dans l'éditeur Unity après implémentation

## Ce qui n'est PAS dans ce scope

- Achat de consommables (sub-projet 3 : boutique)
- Sélection du compagnon (sub-projet 2 : Character Select)
- Interface d'inventaire hors combat
