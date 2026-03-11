using UnityEngine;
using PokemonUnity.Inventory;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>Item pocket / bag section.</summary>
    public enum ItemPocket
    {
        Items, Medicine, Pokeballs, TmHm, Berries, Mail, BattleItems, KeyItems
    }

    /// <summary>
    /// ScriptableObject storing static data for a single item.
    /// Create via menu: PokemonUnity → Item Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewItemData", menuName = "PokemonUnity/Item Data", order = 3)]
    public class ItemDataSO : ScriptableObject
    {
        [Header("Identity")]
        public Items      id;
        public string     displayName;
        [TextArea(2, 4)]
        public string     description;
        public Sprite     icon;

        [Header("Economy")]
        public int        buyPrice;
        public int        sellPrice; // usually buyPrice / 2

        [Header("Pocket")]
        public ItemPocket pocket = ItemPocket.Items;

        [Header("Usage")]
        public bool usableInBattle;
        public bool usableInField;
        public bool holdable;
        public bool consumable;  // disappears after use

        [Header("Hold Effect")]
        [Range(0, 255)] public int holdEffectParam; // parameter for held-item effect
    }
}
