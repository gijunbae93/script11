using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookUi : MonoBehaviour
{
    [SerializeField] CookHandler cookHandler;

    Transform cookLevelOneUi;
    Transform cookAboveLevelOneUi;

    bool isInitalized; // used in enable cookui should not update at start

    List<CookUiExpiraryTimerFillImage> cookUiexpiraryTimerFillImageList = new List<CookUiExpiraryTimerFillImage>();
    int currentExpirationTimerIndex;
    void Start()
    {
        ItemInventory.instance.OnItemListChange += Instance_OnItemListChange;

        cookLevelOneUi = transform.Find("CookLevelOneUi(Panel)");
        cookAboveLevelOneUi = transform.Find("CookAboveLevelOneUi(Panel)");

        isInitalized = true; // this will run after enable
    }

    private void Instance_OnItemListChange(object sender, System.EventArgs e)
    {
        if (!gameObject.activeSelf) return;

        UpdateCookUi(); //incase eatable item expired or thrown to ground by clicking x; this check cookhandler cookslotlist and refresh ui accordingly
    }

    private void OnEnable() { if(isInitalized) UpdateCookUi(); } // the ui will be set active true at the beinning in order to updatecookui to be only when player approches cook structure

    private void OnDisable()
    {
        cookHandler.EmptyCookSlotList();
        ResetCookUi();
    }

    private void Update() 
    {
        for (currentExpirationTimerIndex = 0; currentExpirationTimerIndex < cookUiexpiraryTimerFillImageList.Count; currentExpirationTimerIndex++) 
            cookUiexpiraryTimerFillImageList[currentExpirationTimerIndex].Update(); // cant use foreach clicking cook ui eatableitem will modify cookUiexpiraryTimerFillImageList
    }

    public void UpdateCookUi()
    {
        ResetCookUi();

        if (cookHandler.cookStructureLevel == 1) // differnet ui for structure cook level;
            RefreshCookUi(cookLevelOneUi);
        else 
            RefreshCookUi(cookAboveLevelOneUi);
    }

    void RefreshCookUi(Transform cookUi)
    {
        Transform cookUiTransform = Instantiate(cookUi, transform);
        cookUiTransform.gameObject.SetActive(true);

        if (cookHandler.cookStructureLevel == 1) // campfire only has one cook slot
        {
            if (cookHandler.GetCookSlotList().Count == 1) // if list is empty just do nothing, fill image and eatableimage are default setactive false;, // if the list is empty just modify button
            {
                Transform itemSlot = cookUiTransform.Find("ItemSlot(Panel)");
                Image eatableItemImage = itemSlot.Find("EatableItem(Image)").GetComponent<Image>();
                eatableItemImage.gameObject.SetActive(true);
                eatableItemImage.sprite = ItemAsset.instance.GetItemSprite(cookHandler.GetCookSlotList()[0]);

                Image fillImage = itemSlot.Find("FillImage(Image)").GetComponent<Image>();
                fillImage.gameObject.SetActive(true);
                float totalExpiraryTimer = 0, currentExpiraryTimer = 0;
                if (cookHandler.GetCookSlotList()[0] is EatableItems)
                {
                    EatableItems eatableItem = (EatableItems)cookHandler.GetCookSlotList()[0];
                    totalExpiraryTimer = eatableItem.totalExpiraryTimer;
                    currentExpiraryTimer = eatableItem.GetCurrentExpiryTimer();
                }
                else Debug.Log("Error");
                new CookUiExpiraryTimerFillImage(totalExpiraryTimer, currentExpiraryTimer, fillImage, this);

                Button removeItemButton = itemSlot.Find("RemoveItem(Button)").GetComponent<Button>();
                removeItemButton.gameObject.SetActive(true);
                removeItemButton.onClick.RemoveAllListeners();
                removeItemButton.onClick.AddListener(() =>
                {
                    cookHandler.GetCookSlotList().RemoveAt(0);
                    currentExpirationTimerIndex--;
                    UpdateCookUi();
                });
            }
            Transform cookButtonContainer = cookUiTransform.Find("CookButtonContainer(Panel)");
            Button unCookableButton = cookButtonContainer.Find("UnCookable(Button)").GetComponent<Button>();
            Button cookableButton = cookButtonContainer.Find("Cookable(Button)").GetComponent<Button>();
            if (cookHandler.GetCookSlotList().Count > 0 && !ItemInventory.instance.IsInventoryFull()) // default both interactable and uninteractable buttons are disabled;
            {
                cookableButton.gameObject.SetActive(true);
                cookableButton.onClick.RemoveAllListeners();
                cookableButton.onClick.AddListener(() => cookHandler.CookItem());
            }
            else
                unCookableButton.gameObject.SetActive(true);
            
        }
        else // other structures more then one cook slot
        {
            int i = 0;
            Transform itemSlot = null;
            foreach (Item item in cookHandler.GetCookSlotList())
            {
                itemSlot = i == 0 ? cookUiTransform.Find("ItemSlot1(Panel)") : cookUiTransform.Find("ItemSlot2(Panel)");

                Image eatableItemImage = itemSlot.Find("EatableItem(Image)").GetComponent<Image>();
                eatableItemImage.gameObject.SetActive(true);
                eatableItemImage.sprite = ItemAsset.instance.GetItemSprite(item);

                Image fillImage = itemSlot.Find("FillImage(Image)").GetComponent<Image>();
                fillImage.gameObject.SetActive(true);
                float totalExpiraryTimer = 0, currentExpiraryTimer = 0;

                EatableItems eatableItem = (EatableItems)item;
                totalExpiraryTimer = eatableItem.totalExpiraryTimer;
                currentExpiraryTimer = eatableItem.GetCurrentExpiryTimer();
                new CookUiExpiraryTimerFillImage(totalExpiraryTimer, currentExpiraryTimer, fillImage, this);

                Button removeItemButton = itemSlot.Find("RemoveItem(Button)").GetComponent<Button>();
                removeItemButton.gameObject.SetActive(true);
                removeItemButton.onClick.RemoveAllListeners();
                int j = i; // doing this because i changes every loop
                removeItemButton.onClick.AddListener(() =>
                {
                    cookHandler.GetCookSlotList().RemoveAt(j);
                    currentExpirationTimerIndex--;
                    UpdateCookUi();
                });

                i++;
            }

            Transform cookButtonContainer = cookUiTransform.Find("CookButtonContainer(Panel)");
            Button unCookableButton = cookButtonContainer.Find("UnCookable(Button)").GetComponent<Button>();
            Button cookableButton = cookButtonContainer.Find("Cookable(Button)").GetComponent<Button>();
            if (cookHandler.GetCookSlotList().Count > 0 && !ItemInventory.instance.IsInventoryFull()) // default both interactable and uninteractable buttons are disabled;
            {
                cookableButton.gameObject.SetActive(true);
                cookableButton.onClick.RemoveAllListeners();
                cookableButton.onClick.AddListener(() => cookHandler.CookItem());
            }
            else
                unCookableButton.gameObject.SetActive(true);

        }
    }

    public void ResetCookUi()
    {
        foreach (Transform childTransform in transform)
        {
            if (childTransform == cookLevelOneUi | childTransform == cookAboveLevelOneUi) continue;

            Destroy(childTransform.gameObject);
        }
    }

    public class CookUiExpiraryTimerFillImage
    {
        float totalExpiraryTimer, currentExpiraryTimer;
        float TotalExpiraryTimer
        {
            get => totalExpiraryTimer;
            set
            {
                if (value <= 0)
                {
                    Debug.Log("Error"); // cnat have something number/0
                    totalExpiraryTimer = 1f;
                }
                else
                    totalExpiraryTimer = value;
            }
        }
        Image image;
        CookUi cookUi;
        bool isDestoryed;
        public CookUiExpiraryTimerFillImage(float totalExpiraryTimer, float currentExpiraryTimer, Image image, CookUi cookUi)
        {
            TotalExpiraryTimer = totalExpiraryTimer;
            this.currentExpiraryTimer = currentExpiraryTimer;
            this.image = image;
            this.cookUi = cookUi;

            cookUi.cookUiexpiraryTimerFillImageList.Add(this);
        }

        public void Update()
        {
            if (!image) // when ui refreshes 
            {
                DestorySelf(); // this will trigger deconstrutor
                return;
            }

            if (image != null)
                if (image.gameObject.activeSelf)
                    if (currentExpiraryTimer < TotalExpiraryTimer)
                        image.fillAmount = Mathf.Lerp(0, 1, currentExpiraryTimer / TotalExpiraryTimer);

            currentExpiraryTimer += Time.deltaTime;
        }

        void DestorySelf()
        {
            if (isDestoryed) return;

            cookUi.cookUiexpiraryTimerFillImageList.Remove(this);  // this will trigger deconstrutor
            isDestoryed = true;
        }
    }
}
