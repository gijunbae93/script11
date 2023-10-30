using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEditor;

public class CraftUi : MonoBehaviour
{
    CategoryButtonUi categoryButtonUi;
    CraftItemListUi craftItemListUi;
    CraftItemDescriptionUi craftItemDescriptionUi;
    CraftUiScrollHandler craftUiScrollHandler = new CraftUiScrollHandler();

    [SerializeField] Transform categoryButtonContainer;

    [SerializeField] Transform craftItemListContainer;

    [SerializeField] Transform craftItemDescriptionContainer;

    [SerializeField] PlayerStatHandler playerStatHandler;

    event Action OnUiUpdate = null;

    [SerializeField] InventoryUi inventoryUi;

    [SerializeField] PlayerEquipmentHandler playerEquipmentHandler;


    private void Start()
    {
        craftItemDescriptionUi = new CraftItemDescriptionUi(craftItemDescriptionContainer, inventoryUi); // craftItemDescriptionUi, craftItemListUi, categoryButtonUi initalization order matters
        craftItemListUi = new CraftItemListUi(playerStatHandler, craftItemListContainer, craftItemDescriptionUi, craftUiScrollHandler);
        categoryButtonUi = new CategoryButtonUi(categoryButtonContainer, craftItemListUi);
 
        craftItemListUi.SetCategoryButtonUi(categoryButtonUi);

        OnUiUpdate = () =>
        {
            categoryButtonUi.UpdateCategoryButton(); // left side of ui which category will be refreshed first
            craftItemListUi.UpdateCraftItemListUi(categoryButtonUi.categoryButtonTypesCraftItemListDictionary[categoryButtonUi.selectedCategoryButtonType]);
            craftItemDescriptionUi.UpdateDescriptionUi(categoryButtonUi.categoryButtonTypesCraftItemListDictionary[categoryButtonUi.selectedCategoryButtonType][craftItemListUi.uiIndex]); // list[index]
        };

        ItemInventory.instance.OnItemListChange += Instance_OnItemListChange;

        NextFrame.Create(() => playerEquipmentHandler.InitializePlayerInventoryWithBasicEquipment()); // called in this script because this need to run after it initalizes in OnUiUpdate
    }

    // material for craft item list changes as well. try to not invoke OnUiUpdate at start by add item at start, need to update ui because craftneed item can changes on itemlistchange invoke
    private void Instance_OnItemListChange(object sender, EventArgs e) 
    {
        if (OnUiUpdate == null || !IsGameObjectActive()) return; // if OnUiUpdate is null ui is not ready to update, no need to update item list if game object is not active onenable will update for this

        craftItemListUi.UpdateCraftItemListUi(categoryButtonUi.categoryButtonTypesCraftItemListDictionary[categoryButtonUi.selectedCategoryButtonType], true);
        craftItemDescriptionUi.UpdateDescriptionUi(categoryButtonUi.categoryButtonTypesCraftItemListDictionary[categoryButtonUi.selectedCategoryButtonType][craftItemListUi.uiIndex]); // list[index], need to update craft item list number of items
    }

    private void OnEnable() => OnUiUpdate?.Invoke(); // update ui on enable
    private void OnDisable() // reset ui on disable
    {
        categoryButtonUi.selectedCategoryButtonType = CategoryButtonUi.CategoryButtonTypes.Tools; // if ui is disabled selectedCategoryButtonType becomes tools so when player turns ui back on it will start at beginning
        craftItemListUi.uiIndex = 0;
    }

    public void OnItemCraftLevelChange() // this is used when craft level changes. eg woodenworkshop
    {
        craftItemListUi.InitalizeCraftList();

        if (!IsGameObjectActive()) return; // only refresh craft ui if its active other wise just initalize the list

        craftItemListUi.UpdateCraftItemListUi(categoryButtonUi.categoryButtonTypesCraftItemListDictionary[categoryButtonUi.selectedCategoryButtonType], true);
        craftUiScrollHandler.UpdateCraftItemListScrollUiSize(categoryButtonUi.categoryButtonTypesCraftItemListDictionary[categoryButtonUi.selectedCategoryButtonType].Count, craftItemListContainer.GetComponent<RectTransform>()); ;
    }

    bool IsGameObjectActive() => gameObject.activeSelf;

    public class CategoryButtonUi
    {
        CraftItemListUi craftItemListUi;

        public enum CategoryButtonTypes { Tools, Weapons, Armors, Materials, Structures, Defences }
        Transform buttonVisualContainer;
        Transform categoryButtonContainer;

        public CategoryButtonTypes selectedCategoryButtonType { get; set; }
        public Dictionary<CategoryButtonTypes, List<Item>> categoryButtonTypesCraftItemListDictionary = new Dictionary<CategoryButtonTypes, List<Item>>();

        public CategoryButtonUi(Transform categoryButtonContainer, CraftItemListUi craftItemListUi)
        {
            this.categoryButtonContainer = categoryButtonContainer;
            this.craftItemListUi = craftItemListUi;

            buttonVisualContainer = categoryButtonContainer.Find("ButtonVisualContainer");

            selectedCategoryButtonType = CategoryButtonTypes.Tools;

            UpdateCategoryButton();
        }

        public void UpdateDictionary()
        {
            categoryButtonTypesCraftItemListDictionary[CategoryButtonTypes.Tools] = craftItemListUi.toolCraftTypeList;
            categoryButtonTypesCraftItemListDictionary[CategoryButtonTypes.Weapons] = craftItemListUi.weaponCraftTypeList;
            categoryButtonTypesCraftItemListDictionary[CategoryButtonTypes.Armors] = craftItemListUi.armorCraftTypeList;
            categoryButtonTypesCraftItemListDictionary[CategoryButtonTypes.Materials] = craftItemListUi.materialCraftTypeList;
            categoryButtonTypesCraftItemListDictionary[CategoryButtonTypes.Structures] = craftItemListUi.structureCraftTypeList;
            categoryButtonTypesCraftItemListDictionary[CategoryButtonTypes.Defences] = craftItemListUi.defenceCraftTypeList;
        }

        public void UpdateCategoryButton()
        {
            foreach (Transform childTransform in categoryButtonContainer)
            {
                if (childTransform == buttonVisualContainer) continue;

                Destroy(childTransform.gameObject);
            }

            int y = 0;
            float distanceBetweenSlot = 60;
            float originalYPosition = -70; float originalXPosition = 0;
            for (int i = 0; i < Enum.GetValues(typeof(CategoryButtonTypes)).Length; i++)
            {
                RectTransform buttonVisualRectTransform = Instantiate(buttonVisualContainer, categoryButtonContainer).GetComponent<RectTransform>();
                buttonVisualRectTransform.gameObject.SetActive(true);
                buttonVisualRectTransform.anchoredPosition = new Vector2(originalXPosition, originalYPosition + -(distanceBetweenSlot * y));

                TextMeshProUGUI pressedButtonText = buttonVisualRectTransform.transform.Find("PressedButton").Find("BubbleGumSants (TMP)").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI unpressedButtonText = buttonVisualRectTransform.transform.Find("UnPressedButton").Find("BubbleGumSants (TMP)").GetComponent<TextMeshProUGUI>();
                pressedButtonText.text = ((CategoryButtonTypes)i).ToString();
                unpressedButtonText.text = ((CategoryButtonTypes)i).ToString();

                if (selectedCategoryButtonType == (CategoryButtonTypes)i)
                {
                    GameObject unPressedButton = buttonVisualRectTransform.transform.Find("UnPressedButton").gameObject;
                    unPressedButton.SetActive(false);
                    GameObject pressedButton = buttonVisualRectTransform.transform.Find("PressedButton").gameObject;
                    pressedButton.SetActive(true);
                }
                else
                {
                    Button unPressedButton = buttonVisualRectTransform.transform.Find("UnPressedButton").GetComponent<Button>();
                    CategoryButtonTypes categoryButtonType = (CategoryButtonTypes)i;
                    unPressedButton.onClick.RemoveAllListeners();
                    unPressedButton.onClick.AddListener(() =>
                    {
                        craftItemListUi.uiIndex = 0; // when you click category button it will show description of very top craftuilist ui
                        craftItemListUi.UpdateCraftItemListUi(categoryButtonTypesCraftItemListDictionary[categoryButtonType]);

                        selectedCategoryButtonType = categoryButtonType; // if you press a button this will replace selectedCategoryButtonType and on next refresh pressedbutton will be active matching selectedCategoryButtonType
                        UpdateCategoryButton();

                        
                    });
                }

                y++;
            }
        }
    } 
    public class CraftItemListUi
    {
        public List<Item> armorCraftTypeList { get; private set; }
        public List<Item> weaponCraftTypeList { get; private set; }
        public List<Item> toolCraftTypeList { get; private set; }
        public List<Item> materialCraftTypeList { get; private set; }
        public List<Item> structureCraftTypeList { get; private set; }
        public List<Item> defenceCraftTypeList { get; private set; }

        PlayerStatHandler playerStatHandler;
        CraftItemDescriptionUi craftItemDescriptionUi;
        CraftUiScrollHandler craftUiScrollHandler;
        Transform craftItemListContainer;

        Transform buttonVisualContainer;

        CategoryButtonUi categoryButtonUi;

        public int uiIndex { get; set; } // this represetns what craft list is pressed (showing description of craft)
        public CraftItemListUi(PlayerStatHandler playerStatHandler, Transform craftItemListContainer, CraftItemDescriptionUi craftItemDescriptionUi, CraftUiScrollHandler craftUiScrollHandler)
        {
            this.playerStatHandler = playerStatHandler;
            this.craftItemDescriptionUi = craftItemDescriptionUi;
            this.craftUiScrollHandler = craftUiScrollHandler;

            this.craftItemListContainer = craftItemListContainer;
            buttonVisualContainer = craftItemListContainer.Find("ButtonVisualContainer(Panel)");

            uiIndex = 0;
            // playerStatHandler.itemBuildLevel goes level one in awake function of playerstatHandler;
            NextFrame.Create(() =>
            {
                InitalizeCraftList();
                UpdateCraftItemListUi(toolCraftTypeList);
            });
        }

        #region CraftLists
        public void InitalizeCraftList()
        {
            switch (playerStatHandler.itemCraftLevel)
            {
                case 1:
                    InitalizeLevelOneToolCraftItemList();
                    InitalizeLevelOneWeaponCraftItemList();
                    InitalizeLevelOneArmorCraftItemList();
                    InitalizeLevelOneMaterialCraftItemList();
                    InitalizeLevelOneStructureCraftItemList();
                    InitalizeLevelOneDefenceCraftItemList();

                    categoryButtonUi.UpdateDictionary(); // dictionary needs to be initalized again when reference change
                    break;
                case 2:
                    InitalizeLevelTwoToolCraftItemList();
                    InitalizeLevelOneWeaponCraftItemList();
                    InitalizeLevelTwoArmorCraftItem();
                    InitalizeLevelOneMaterialCraftItemList();
                    InitalizeLevelTwoStructureCraftItemList();
                    InitalizeLevelTwoDefenceCraftItemList();

                    categoryButtonUi.UpdateDictionary(); // dictionary needs to be initalized again when reference change
                    break;
            }
        }

        void InitalizeLevelOneToolCraftItemList() //1
        {
            toolCraftTypeList = new List<Item>
            {
                new Weapon(Weapon.WeaponType.WoodenAxe),
                new Weapon(Weapon.WeaponType.WoodenPickAxe),
            };
        }

        void InitalizeLevelTwoToolCraftItemList() // 2
        {
            toolCraftTypeList = new List<Item>
            {
                new Weapon(Weapon.WeaponType.WoodenAxe),
                new Weapon(Weapon.WeaponType.WoodenPickAxe),
                new Weapon(Weapon.WeaponType.SaphireAxe),
                new Weapon(Weapon.WeaponType.SaphirePickAxe),
                new CraftableMaterialItems(CraftableMaterialItems.CraftMaterialItemType.LeafBag),
            };
        }

        void InitalizeLevelThreeCraftItemList() // 3
        {
            toolCraftTypeList = new List<Item>
            {
                new Weapon(Weapon.WeaponType.WoodenAxe),
                new Weapon(Weapon.WeaponType.WoodenPickAxe),
                new Weapon(Weapon.WeaponType.SaphireAxe),
                new Weapon(Weapon.WeaponType.SaphirePickAxe),
                new CraftableMaterialItems(CraftableMaterialItems.CraftMaterialItemType.LeafBag),
                new CraftableMaterialItems(CraftableMaterialItems.CraftMaterialItemType.LeatherBag),
            };
        }

        void InitalizeLevelOneWeaponCraftItemList() // 1
        {
            weaponCraftTypeList = new List<Item>
            {
                new Weapon(Weapon.WeaponType.WoodenSpear),
                new Weapon(Weapon.WeaponType.WoodenSword),
                new Weapon(Weapon.WeaponType.WoodenHammer),
            };
        }


        void InitalizeLevelOneArmorCraftItemList() // 1
        {
            armorCraftTypeList = new List<Item>
            {
                new Helmet(Helmet.HelmetType.LevelOneHelmet),
                new Armor(Armor.ArmorType.LevelOneArmor),
                new Glove(Glove.GloveType.LevelOneGlove),
                new Shoe(Shoe.ShoesType.LevelOneShoes),
            };
        }

        void InitalizeLevelTwoArmorCraftItem() // 2
        {
            armorCraftTypeList = new List<Item>
            {
                new Helmet(Helmet.HelmetType.LevelOneHelmet),
                new Armor(Armor.ArmorType.LevelOneArmor),
                new Glove(Glove.GloveType.LevelOneGlove),
                new Shoe(Shoe.ShoesType.LevelOneShoes),

                new Helmet (Helmet.HelmetType.LevelTwoHelmet),
                new Armor (Armor.ArmorType.LevelTwoArmor),
                new Glove(Glove.GloveType.LevelTwoGlove),
                new Shoe(Shoe.ShoesType.LevelTwoShoes),

                new Helmet(Helmet.HelmetType.LevelThreeHelmet),
                new Armor(Armor.ArmorType.LevelThreeArmor),
                new Glove(Glove.GloveType.LevelThreeGlove),
                new Shoe(Shoe.ShoesType.LevelThreeShoes),
            };
        }
        void InitalizeLevelOneMaterialCraftItemList() // 1
        {
            materialCraftTypeList = new List<Item>
            {
                new CraftableMaterialItems(CraftableMaterialItems.CraftMaterialItemType.ProcessedWood),
            };
        }

        void InitalizeLevelOneStructureCraftItemList() // 1
        {
            structureCraftTypeList = new List<Item>
            {
                new StructureItems(StructureItems.StructureType.WoodenWorkShop),
            };
        }

        void InitalizeLevelTwoStructureCraftItemList() // 2
        {
            structureCraftTypeList = new List<Item>
            {
                new StructureItems(StructureItems.StructureType.WoodenWorkShop),
                new StructureItems(StructureItems.StructureType.CampFire),
                new StructureItems(StructureItems.StructureType.RainCollector),
                new StructureItems(StructureItems.StructureType.LeatherTent),
                new StructureItems(StructureItems.StructureType.IronForge),
            };
        }

        void InitalizeLevelOneDefenceCraftItemList() // 1
        {
            defenceCraftTypeList = new List<Item>
            {
                new StructureItems(StructureItems.StructureType.LevelOneTower),
                new StructureItems(StructureItems.StructureType.DefenceWallOne),
            };
        }

        void InitalizeLevelTwoDefenceCraftItemList() // 2
        {
            defenceCraftTypeList = new List<Item>
            {
                new StructureItems(StructureItems.StructureType.LevelOneTower),
                new StructureItems(StructureItems.StructureType.LevelTwoTower),
                new StructureItems(StructureItems.StructureType.LevelThreeTower),
                new StructureItems(StructureItems.StructureType.DefenceWallOne),
                new StructureItems(StructureItems.StructureType.DefenceWallTwo),
                new StructureItems(StructureItems.StructureType.DefenceWallThree),
            };
        }
        #endregion

        public void UpdateCraftItemListUi(List<Item> itemList, bool ignoreScrollUpdate = false)
        {
            foreach (Transform childTransform in craftItemListContainer)
            {
                if (childTransform == buttonVisualContainer) continue;

                Destroy(childTransform.gameObject);
            }

            int y = 0; // only changing Y;
            float distanceBetweenSlot = 55;
            float originalYPosition = -35; float originalXPosition = 0;
            foreach (Item item in itemList)
            {
                RectTransform buttonVisualContainerRectTransform = Instantiate(buttonVisualContainer, craftItemListContainer).GetComponent<RectTransform>();
                buttonVisualContainerRectTransform.gameObject.SetActive(true);
                buttonVisualContainerRectTransform.anchoredPosition = new Vector2(originalXPosition, originalYPosition + -(distanceBetweenSlot * y));

                if (y == uiIndex)
                {
                    GameObject pressedButton = buttonVisualContainerRectTransform.transform.Find("PressedButton").gameObject;
                    pressedButton.SetActive(true);
                    GameObject unPressedButton = buttonVisualContainerRectTransform.transform.Find("UnPressedButton").gameObject;
                    unPressedButton.SetActive(false);

                    TextMeshProUGUI pressedButtonTextMeshProUGUI = pressedButton.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
                    pressedButtonTextMeshProUGUI.text = item.GetItemName();

                    Image pressedButtonImage = pressedButton.transform.Find("Image").GetComponent<Image>();
                    pressedButtonImage.sprite = ItemAsset.instance.GetItemSprite(item);

                    craftItemDescriptionUi.UpdateDescriptionUi(item);
                }
                else
                {
                    Button unPressedButton = buttonVisualContainerRectTransform.transform.Find("UnPressedButton").GetComponent<Button>();
                    int i = y;
                    unPressedButton.onClick.RemoveAllListeners();
                    unPressedButton.onClick.AddListener(() =>
                    {
                        uiIndex = i;
                        UpdateCraftItemListUi(itemList, true); // when clicking button in same category should ignore scroll position update. it should maintain position
                    });

                    TextMeshProUGUI unPressedButtonTextMeshProUGUI = unPressedButton.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
                    unPressedButtonTextMeshProUGUI.text = item.GetItemName();

                    Image unPressedButtonImage = unPressedButton.transform.Find("Image").GetComponent<Image>();
                    unPressedButtonImage.sprite = ItemAsset.instance.GetItemSprite(item);
                }

                y++;
            }

            if(!ignoreScrollUpdate) craftUiScrollHandler.UpdateCraftItemListScrollUi(itemList.Count, craftItemListContainer.GetComponent<RectTransform>());
        }

        public void SetCategoryButtonUi(CategoryButtonUi categoryButtonUi) => this.categoryButtonUi = categoryButtonUi;
    }
    public class CraftItemDescriptionUi
    {
        Transform craftItemDescriptionContainer;
        InventoryUi inventoryUi;

        Transform requiredCraftItemContainer;
        Transform requiredCraftItem;

        int maxNumberOfItemRequiredCraft => 5;

        public CraftItemDescriptionUi(Transform craftItemDescriptionContainer, InventoryUi inventoryUi)
        {
            this.craftItemDescriptionContainer = craftItemDescriptionContainer;
            this.inventoryUi = inventoryUi;

            requiredCraftItemContainer = craftItemDescriptionContainer.Find("RequiredCraftItemContainer(panel)");
            requiredCraftItem = requiredCraftItemContainer.Find("RequiredCraftItem(Image)");
        }

        public void UpdateDescriptionUi(Item item)
        {
            foreach (Transform childTransform in requiredCraftItemContainer)
            {
                if (childTransform == requiredCraftItem) continue;

                Destroy(childTransform.gameObject);
            }

            TextMeshProUGUI itemNameTextMeshProUGUI = craftItemDescriptionContainer.Find("ItemName (TMP)").GetComponent<TextMeshProUGUI>();
            itemNameTextMeshProUGUI.text = item.GetItemName();

            Image itemImage = craftItemDescriptionContainer.Find("ItemImage(Image)").GetComponent<Image>();
            itemImage.sprite = ItemAsset.instance.GetItemSprite(item);

            Transform descriptionPanel = craftItemDescriptionContainer.Find("Description(Panel)");
            TextMeshProUGUI descriptionTextMeshProUGUI = descriptionPanel.Find("ItemDescription (TMP)").GetComponent<TextMeshProUGUI>();
            descriptionTextMeshProUGUI.text = ItemAsset.instance.GetItemDescription(item);

            /////////////////////////////////////////////////////////////////////////////////////////////////////// requiredCraft
            bool canCraft = false;
            Action craftItem = null;

            float originalXPosition = 0; float originalYPosition = 0;
            float spaceBetweenCellXAxis = 55;
            RectTransform[] requiredCraftItemRectTransformArray = new RectTransform[maxNumberOfItemRequiredCraft];
            for (int i = 0; i < maxNumberOfItemRequiredCraft; i++)
            {
                RectTransform requiredCraftItemRectTransform = Instantiate(requiredCraftItem, requiredCraftItemContainer).GetComponent<RectTransform>();
                requiredCraftItemRectTransform.gameObject.SetActive(true);
                requiredCraftItemRectTransform.anchoredPosition = new Vector2(originalXPosition + spaceBetweenCellXAxis * i, originalYPosition); // only changing X;

                if (i < ItemInventory.instance.itemCraftDictionary[item.GetItemName()].Length)
                {
                    GameObject requiredCraftItemImage = requiredCraftItemRectTransform.Find("ItemImage(Image)").gameObject;
                    requiredCraftItemImage.SetActive(true);
                    GameObject itemNeededText = requiredCraftItemRectTransform.Find("ItemNeededText (TMP)").gameObject;
                    itemNeededText.SetActive(true);
                }

                requiredCraftItemRectTransformArray[i] = requiredCraftItemRectTransform;
            }

            if (ItemInventory.instance.itemCraftDictionary[item.GetItemName()].Length > maxNumberOfItemRequiredCraft)
            {
                Debug.Log("Error"); // if required mateiral is more than 5
                return;
            }
            for (int i = 0; i < ItemInventory.instance.itemCraftDictionary[item.GetItemName()].Length; i++)
            {
                Image itemSprite = requiredCraftItemRectTransformArray[i].Find("ItemImage(Image)").GetComponent<Image>();     // set sprite
                itemSprite.sprite = ItemAsset.instance.GetItemSprite(ItemInventory.instance.itemCraftDictionary[item.GetItemName()][i].item);

                ItemToolTipHandler itemToolTipHandler = requiredCraftItemRectTransformArray[i].Find("ItemImage(Image)").GetComponent<ItemToolTipHandler>(); // set itemToolTip
                itemToolTipHandler.item = ItemInventory.instance.itemCraftDictionary[item.GetItemName()][i].item;
                itemToolTipHandler.activeItemToolTipUIi = ItemToolTipUi.ActiveItemToolTipUI.CraftRequiredItemUi;
                itemToolTipHandler.itemToolTipUiDisplayLocation = ItemToolTipUi.ItemToolTipDisplayLocation.Left;
    
                int numberOfItemPlayerCurrentlyOwn = 0;
                foreach (Item playerItem in ItemInventory.instance.GetItemList())
                    if (playerItem.GetItemName() == ItemInventory.instance.itemCraftDictionary[item.GetItemName()][i].item.GetItemName())
                        numberOfItemPlayerCurrentlyOwn += playerItem.GetNumberOfItem(); // since there could be more than one stackable item in a slot i need to keep searching for item

                TextMeshProUGUI requiredNumberOfItemText = requiredCraftItemRectTransformArray[i].transform.Find("ItemNeededText (TMP)").GetComponent<TextMeshProUGUI>();
                requiredNumberOfItemText.text = $"{numberOfItemPlayerCurrentlyOwn}/{ItemInventory.instance.itemCraftDictionary[item.GetItemName()][i].numberOfItemRequired}";
            }

            canCraft = ItemInventory.instance.CanCraftItem(item); // this also checks if inventory is full or not if inventory is full craft button is deactivated
            craftItem = () => ItemInventory.instance.CraftItem(item);
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            Transform buttonContainer = craftItemDescriptionContainer.Find("ButtonContainer(Panel)");
            Button interactableButton = buttonContainer.Find("InteractableButton").GetComponent<Button>();
            Button unInteractableButton = buttonContainer.Find("UninteractableButton").GetComponent<Button>();
            if (canCraft)
            {
                interactableButton.gameObject.SetActive(true);
                unInteractableButton.gameObject.SetActive(false);

                interactableButton.onClick.RemoveAllListeners();
                interactableButton.onClick.AddListener(() =>
                {
                    craftItem();
                    UpdateDescriptionUi(item); // need to refresh ui so player know player can craft again or not
                    inventoryUi.UpdateItemInventoryUi();
                });
            }
            else
            {
                interactableButton.gameObject.SetActive(false);
                unInteractableButton.gameObject.SetActive(true);
            }

        }
    }

    public class CraftUiScrollHandler
    {
        #region craftItemListUi 
        float craftItemListUiSize => 50f;
        float uiMaskSizeY => 380f;
        float spaceBetweenCraftItemListUi => 5f;

        int defaultCraftItemListContainerXPosition => 0;

        int numberOfCraftUiList;

        // less than 5 its okay to use default image size of defulat uimask size after 6 ui 365 7 420, 8 475 increases by 55
        // 7 - 400
        public void UpdateCraftItemListScrollUi(int numberOfUi, RectTransform craftItemlistContainer)
        {
            if(numberOfUi <= 6) //less than 6 its okay to use default image size of defulat uimask size
            {
                craftItemlistContainer.sizeDelta = new Vector2(craftItemlistContainer.sizeDelta.x, uiMaskSizeY); // preserve existing value of width (craftItemlistContainer.sizeDelta.x) only modify size
                craftItemlistContainer.anchoredPosition = new Vector2(defaultCraftItemListContainerXPosition, 0);  // -(new height - default height)/ 2 i have no idea why its divided by 2
            }
            else
            {
                int integer = numberOfUi - 7;  // if craftlist item is 7 the ui size should be 400
                float rectTransformYSize = 400 + (integer * (craftItemListUiSize + spaceBetweenCraftItemListUi)); // if its greater than 7 just multiply by the craftItemListUiSize

                craftItemlistContainer.sizeDelta = new Vector2(craftItemlistContainer.sizeDelta.x, rectTransformYSize);
                craftItemlistContainer.anchoredPosition = new Vector2(defaultCraftItemListContainerXPosition, -((rectTransformYSize - uiMaskSizeY)/2)); // -(new height - default height)/ 2 i have no idea why its divided by 2
            }

            numberOfCraftUiList = numberOfUi;
        }

        public void UpdateCraftItemListScrollUiSize(int numberOfUi, RectTransform craftItemlistContainer) // update Size only , when item build level goes up we just need to add more craftable items and update size instead of changing position to default
        {
            if (numberOfUi <= 6) //less than 6 its okay to use default image size of defulat uimask size
            {
                craftItemlistContainer.sizeDelta = new Vector2(craftItemlistContainer.sizeDelta.x, uiMaskSizeY); // preserve existing value of width (craftItemlistContainer.sizeDelta.x) only modify size
                craftItemlistContainer.anchoredPosition = new Vector2(defaultCraftItemListContainerXPosition, 0);  // -(new height - default height)/ 2 i have no idea why its divided by 2
            }
            else
            {
                float currentRectTransformYPosition = craftItemlistContainer.anchoredPosition.y;
                int integer1 = numberOfUi - numberOfCraftUiList;

                int integer = numberOfUi - 7;  // if craftlist item is 7 the ui size should be 400

                float rectTransformYSize = 400 + (integer * (craftItemListUiSize + spaceBetweenCraftItemListUi)); // if its greater than 7 just multiply by the craftItemListUiSize
                craftItemlistContainer.sizeDelta = new Vector2(craftItemlistContainer.sizeDelta.x, rectTransformYSize);
                craftItemlistContainer.anchoredPosition = new Vector2(defaultCraftItemListContainerXPosition, currentRectTransformYPosition + -(integer1 *(craftItemListUiSize + spaceBetweenCraftItemListUi))/2);
            }

            numberOfCraftUiList = numberOfUi;
        }
        #endregion
    }
}