using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUi : MonoBehaviour // 4 x 5
{
    [SerializeField] Transform inventoryContainer;
    [SerializeField] Transform inventorySlot;
    [SerializeField] RectTransform inventoryBackgroundRectTransform;

    [SerializeField] Player player;

    bool isItemInventoryInitalized;

    InventoryUiScrollHandler inventoryUiScrollHandler = new InventoryUiScrollHandler();
    void OnEnable()
    {
        if (!isItemInventoryInitalized) // enable is runs before iteminventory.instance is initalized
        {
            NextFrame.Create(() => UpdateItemInventoryUi());
            isItemInventoryInitalized = true;

            return; // so UpdateItemInventoryUi() does not run again on initalization
        }

        inventoryUiScrollHandler.UpdateInventoryUiScroll(inventoryBackgroundRectTransform);
        UpdateItemInventoryUi();
    }
    private void Start() => ItemInventory.instance.OnItemListChange += Instance_OnItemListChange;

    private void Instance_OnItemListChange(object sender, System.EventArgs e)
    {
        inventoryUiScrollHandler.UpdateInventoryUiScroll(inventoryBackgroundRectTransform);
        UpdateItemInventoryUi();
    }

    public void UpdateItemInventoryUi()
    {
        foreach (Transform childTransform in inventoryContainer)
        {
            if (childTransform == inventorySlot) continue;

            Destroy(childTransform.gameObject);
        }

        float slotSizeX = 85, slotOriginalPositionX = 0f;
        float slotSizeY = 85, slotOriginalPositionY = 0f;
        float spaceBetweenSlot = 22f;

        int x = 0;
        int y = 0;

        foreach (Item item in ItemInventory.instance.GetItemList())
        {
            RectTransform inventorySlotRectTransform = Instantiate(inventorySlot, inventoryContainer).GetComponent<RectTransform>();
            inventorySlotRectTransform.gameObject.SetActive(true);
            inventorySlotRectTransform.anchoredPosition = new Vector2(slotOriginalPositionX + (slotSizeX + spaceBetweenSlot) * x, -(slotOriginalPositionY + (slotSizeY + spaceBetweenSlot) * y)); // y is negative 

            Image iconImage = inventorySlotRectTransform.Find("ItemIcon(Image)").GetComponent<Image>();
            iconImage.sprite = ItemAsset.instance.GetItemSprite(item);

            if (item.GetIsContainExpirary()) // if its eatable item it needs to have expirary image effect
            {
                Image FillImage = inventorySlotRectTransform.Find("FillImage(Image)").GetComponent<Image>();
                FillImage.gameObject.SetActive(true);
                if (item is DropableEatableItems)
                {
                    DropableEatableItems dropEatableItems = (DropableEatableItems)item;
                    dropEatableItems.InstantiateExpiraryTimerGameObject(FillImage);
                }
                else if (item is CookEatableItems)
                {
                    CookEatableItems cookEatableItems = (CookEatableItems)item;
                    cookEatableItems.InstantiateExpiraryTimerGameObject(FillImage);
                }
                else
                    Debug.Log("Error");
            }

            Button clickButton = inventorySlotRectTransform.Find("ClickButton(Button)").GetComponent<Button>();

            // Item toolTip
            ItemToolTipHandler itemToolTipHandler = clickButton.gameObject.GetComponent<ItemToolTipHandler>();
            itemToolTipHandler.item = item;
            itemToolTipHandler.activeItemToolTipUIi = ItemToolTipUi.ActiveItemToolTipUI.InventoryUi;
            itemToolTipHandler.itemToolTipUiDisplayLocation = x > 1 ? ItemToolTipUi.ItemToolTipDisplayLocation.Right : ItemToolTipUi.ItemToolTipDisplayLocation.Left; // 1,2 column itemtooltipui will be displayed on right

            if (item is StructureItems) // if item is structure item make hovering item icon would display structure preview item
            {
                StructurePreviewMonobehaviour structurePreviewMonobehaviour = clickButton.gameObject.AddComponent<StructurePreviewMonobehaviour>(); // add to click button since click button is very buttom part of ui
                structurePreviewMonobehaviour.SetItem(item);
            }

            if (item.GetIsStackableItem()) // nothing happens if you press material Item
                clickButton.interactable = false;
            else
            {
                clickButton.onClick.RemoveAllListeners();
                clickButton.onClick.AddListener(() =>
                {
                    ItemToolTipHandler.DisableItemToolTip();
                    item.OnInventoryUse();
                });
            }

            Button removeButton = inventorySlotRectTransform.Find("RemoveButton(Button)").GetComponent<Button>();

            if (item is CraftableMaterialItems) // purpose of this loop is to remove remove item button from inventory because bags increases inventory slot so player cant not throw away
            {
                CraftableMaterialItems craftableMaterialItem = (CraftableMaterialItems)item;
                if (craftableMaterialItem.craftMaterialItemType == CraftableMaterialItems.CraftMaterialItemType.LeafBag) removeButton.gameObject.SetActive(false);
            }

            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(() =>
            {
                if (item.GetIsStackableItem()) // if its material item
                {
                    new ItemWorldSpawner(item, player.GetPlayerPosition());

                    int currentNumberOfStackableItems = item.GetNumberOfItem();
                    ItemInventory.instance.RemoveSpecificStackableItems(item, currentNumberOfStackableItems); // this will reduce number of item or remove item if it has one number of item only for mateiralItems
                    item.AddNumberOfItem(currentNumberOfStackableItems); // item is a reference, since its reducing number of item in revmoeitem method you need add item in spwaned item so when player picks up its equal to the number of item he threw a way
                }
                else
                {
                    new ItemWorldSpawner(item, player.GetPlayerPosition());
                    ItemInventory.instance.RemoveItem(item);
                }
            });

            if (item.GetIsStackableItem()) // material
            {
                TextMeshProUGUI numberOfItemTMP = inventorySlotRectTransform.Find("NumberOfItem (TMP)").GetComponent<TextMeshProUGUI>();
                numberOfItemTMP.gameObject.SetActive(true);
                numberOfItemTMP.text = item.GetNumberOfItem().ToString();
            }

            if (item.GetIsContainDurability()) // wearable
            {
                TextMeshProUGUI durabilityOfItemTMP = inventorySlotRectTransform.Find("Durability(TMP)").GetComponent<TextMeshProUGUI>();
                durabilityOfItemTMP.gameObject.SetActive(true);
                durabilityOfItemTMP.text = item.GetDurabilityPercentage().ToString() + "%";
            }

            if (x > 2) // x is increase after
            {
                y++;
                x = 0;
            }
            else
                x++;
        }
    }

    class InventoryUiScrollHandler
    {
        float spaceBetweenSlot => 22;
        float slotSize => 85;
        float displayedUiSizeY => 450f;
        int scrollPerInventoryCapacity => 4;
        public void UpdateInventoryUiScroll(RectTransform backGroundRectTransform)
        {
            int linesToUpdateUi = (ItemInventory.instance.maximumInventoryCapacity - ItemInventory.instance.defaultInventoryCapacity) / scrollPerInventoryCapacity;

            if(linesToUpdateUi == 0) return;

            backGroundRectTransform.sizeDelta = new Vector2(backGroundRectTransform.sizeDelta.x, displayedUiSizeY + (spaceBetweenSlot + slotSize) * linesToUpdateUi);
            backGroundRectTransform.anchoredPosition = new Vector2(0, -((spaceBetweenSlot + slotSize) * linesToUpdateUi) / 2);
        }
    }
}
