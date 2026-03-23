# Shop System — Design Spec
Date: 2026-03-23

## Contexte

Aucun système de boutique n'existe. Le joueur a un champ `Gold` dans `GameSession` (sauvegardé), et les `EquipmentSO` / `ConsumableSO` existent déjà. `NpcInteractor` est un stub vide. `Inventory` a déjà `Bag` (équipement non-équipé) et `Consumables`.

## Objectif

Un système de boutique générique : les NPCs marchands du Village ouvrent une boutique qui vend des consommables et/ou des équipements (armes comprises). Le type de boutique est implicite dans son contenu — un marchand d'armes vend des `EquipmentSO` de slots `MainWeapon`/`Offhand`, un apothicaire vend des `ConsumableSO`, etc.

## Architecture

### ShopSO — ScriptableObject

Définit le contenu d'une boutique (assigné dans l'Inspector du NPC marchand) :

```csharp
[CreateAssetMenu(fileName = "NewShop", menuName = "RPG/Shop")]
public class ShopSO : ScriptableObject
{
    public string shopName;

    [System.Serializable]
    public struct EquipmentEntry  { public EquipmentSO  item; public int price; }
    [System.Serializable]
    public struct ConsumableEntry { public ConsumableSO item; public int price; }

    public EquipmentEntry[]  equipmentItems;
    public ConsumableEntry[] consumableItems;
}
```

### NpcInteractor — mise à jour

Ajouter un champ `[SerializeField] private ShopSO shopData;`. Si `shopData != null` lors d'une interaction (touche E dans le trigger), publier `ShopOpenedEvent { Shop = shopData }` via EventBus. Sinon, comportement actuel (log debug).

### ShopOpenedEvent — nouveau dans GameEvents.cs

```csharp
public struct ShopOpenedEvent { public ShopSO Shop; }
```

### ShopUI — MonoBehaviour dans la scène Village

- S'abonne à `ShopOpenedEvent` dans `OnEnable`, se désabonne dans `OnDisable`
- Panel caché par défaut
- À l'ouverture : affiche `shopData.shopName`, génère un bouton par item, affiche l'or courant
- Bouton par item : `"{itemName} — {price}G"` ; grisé si `Gold < price`
- Clic → achat : déduit l'or, ajoute l'item à l'inventaire, rafraîchit l'UI, sauvegarde
- Bouton **Fermer** : cache le panel
- Les boutons sont générés dynamiquement (pattern SkillMenuUI) avec layout fixe

### Achat — logique

**Équipement acheté** → `character.Inventory.Bag.Add(item)` (pas d'auto-équipement — l'équipement en boutique va dans le sac)

**Consommable acheté** → `character.Inventory.AddConsumable(item)`

Après chaque achat — avec null guards obligatoires :
```csharp
var session = GameSession.Instance;
if (session == null) return;
var character = session.ActiveCharacter;
if (character == null) return;
session.Gold -= price;
// ajouter l'item à l'inventaire ici
session.Save();
```

### Gold display

Un `TMP_Text goldText` dans ShopUI — mis à jour à l'ouverture et après chaque achat : `$"Or : {GameSession.Instance.Gold}G"`

## Fichiers

| Fichier | Action |
|---------|--------|
| `Assets/Scripts/Data/ShopSO.cs` | Nouveau ScriptableObject |
| `Assets/Scripts/Core/GameEvents.cs` | Ajouter `ShopOpenedEvent` |
| `Assets/Scripts/Exploration/NpcInteractor.cs` | Ajouter `shopData` + publish event |
| `Assets/Scripts/UI/ShopUI.cs` | Nouveau MonoBehaviour |

## Ce qui n'est PAS dans ce scope

- Vente d'items par le joueur (sell)
- Inventaire équipement consultable hors combat
- Stock limité (items en quantité finie)
- Prix dynamiques
- Dialogue avant d'ouvrir la boutique (Yarn Spinner)
