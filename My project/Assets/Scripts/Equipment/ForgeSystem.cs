/// <summary>
/// Forge system: upgrade an item's rarity by one tier.
/// Requires the player to have enough gold and all listed crafting materials.
/// </summary>
public static class ForgeSystem
{
    public class ForgeResult
    {
        public bool Success;
        public string Message;
        public EquipmentRarity NewRarity;
    }

    /// <summary>
    /// Attempts to upgrade the item's rarity by one tier.
    /// On success, mutates item.rarity in place (use runtime copies, not asset files).
    /// </summary>
    /// <param name="item">Item to upgrade.</param>
    /// <param name="gold">Gold the player currently has.</param>
    /// <param name="playerMaterials">Material names in the player's inventory.</param>
    public static ForgeResult Craft(EquipmentSO item, int gold, string[] playerMaterials)
    {
        if (item == null)
            return Fail("Aucun objet sélectionné.");

        if (!RaritySystem.CanUpgrade(item.rarity))
            return Fail($"Cet objet est déjà Légendaire — upgrade impossible.");

        if (gold < item.craftingGoldCost)
            return Fail($"Or insuffisant. Requis : {item.craftingGoldCost} or.");

        if (item.craftingMaterials != null)
        {
            foreach (var mat in item.craftingMaterials)
            {
                bool found = playerMaterials != null &&
                             System.Array.Exists(playerMaterials, m => m == mat);
                if (!found) return Fail($"Matériau manquant : {mat}.");
            }
        }

        item.rarity = RaritySystem.Upgrade(item.rarity);
        return new ForgeResult { Success = true, Message = "Amélioration réussie !", NewRarity = item.rarity };
    }

    private static ForgeResult Fail(string message) =>
        new ForgeResult { Success = false, Message = message };
}
