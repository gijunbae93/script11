using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemToolTipUi : MonoBehaviour
{
    public enum ItemToolTipDisplayLocation { Right, Left };
    public enum ActiveItemToolTipUI { InventoryUi, CraftRequiredItemUi }

    TextMeshProUGUI titleText;
    Image itemImage;
    TextMeshProUGUI descriptionText;

    public Item item { get; private set; }

    public float inventoryYPositionOffSet => -230f; // off set from cursor position
    public float craftYPositionOffSet => 230f; // off set from cursor position
    float xPositionOffSet;
    private void Awake()
    {
        titleText = transform.Find("Title (TMP)").GetComponent<TextMeshProUGUI>();
        itemImage = transform.Find("ItemImage(Image)").GetComponent<Image>();
        descriptionText = transform.Find("Description (TMP)").GetComponent<TextMeshProUGUI>();
    }

    public void UpdateItemToolTipUi(Item item)
    {
        this.item = item;

        titleText.text = item.GetItemName();
        itemImage.sprite = ItemAsset.instance.GetItemSprite(item);
        descriptionText.text = ItemAsset.instance.GetItemDescription(item);
    }

    public float GetXPositionOffSet(ItemToolTipDisplayLocation itemToolTipDisplayLocation)
    {
        xPositionOffSet = itemToolTipDisplayLocation == ItemToolTipDisplayLocation.Right ? -210f : 210f;
        return xPositionOffSet;
    }
}
