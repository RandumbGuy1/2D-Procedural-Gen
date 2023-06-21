using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerItemController : MonoBehaviour
{
    [Header("Equip Settings")]
    [SerializeField] private bool smartCursor;
    [SerializeField] private float throwForce;
    [SerializeField] private Image[] inventorySlots = new Image[9];
    [SerializeField] private List<IItem> items = new List<IItem>();
    [SerializeField] private List<GameObject> queuedItems = new List<GameObject>();
    [SerializeField] private UIAnimationController itemUI;
    [SerializeField] private int selectedItem;
    [SerializeField] private int maxItems;

    public bool SmartCursor => smartCursor;

    [SerializeField] private Dictionary<int, List<IItem>> itemDictionary = new Dictionary<int, List<IItem>>();

    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI itemEquipText;
    [SerializeField] private Image itemEquipArt;

    [SerializeField] private List<TextMeshProUGUI> itemAbilitiesText = new List<TextMeshProUGUI>();
    [SerializeField] private List<Image> itemAbilitiesArt = new List<Image>();
    private Vector3[] spriteStartingPositions;

    [Header("Refrences")]
    [SerializeField] private PlayerRef player;
    [SerializeField] private Transform weaponPos;

    public IItem EquippedItem { get; private set; }
    public GameObject ItemGameObject { get; private set; }

    void Awake() {
        //Save all positions of the starting UI images
        spriteStartingPositions = new Vector3[itemAbilitiesArt.Count];
        for (int i = 0; i < spriteStartingPositions.Length; i++)
            spriteStartingPositions[i] = itemAbilitiesArt[i].rectTransform.localPosition;

        player.PlayerInput.OnItemDropInput += CheckForDrop;
        player.PlayerInput.OnItemSelectInput += CheckForSwitchItem;
        player.PlayerInput.OnMouseButtonDownInput += (int mouseButton) => {
            if (mouseButton == 2) smartCursor = !smartCursor;
        };

        //If no items reset everything
        if (queuedItems.Count <= 0) ResetData();
    }

    private void CheckForSwitchItem(int newSelect) {
        //Add the queued items into inventory
        if (queuedItems.Count > 0) {
            foreach (GameObject item in queuedItems) AddItem(item);
            queuedItems.Clear();
        }

        if (EquippedItem == null) return;

        EquippedItem.ItemUpdate(player);

        //If not inputting a keybind or using an invalid keybind return
        if (newSelect == -1 || newSelect + 1 > items.Count) return;

        int previousItem = selectedItem;
        selectedItem = newSelect;

        if (previousItem != selectedItem) SelectItem();
    }

    //Check for key press of dropping if there are items in our inventory
    private void CheckForDrop(bool dropping) {
        if (!dropping || items.Count <= 0) return;

        DropItem();
    }

    //Public method of adding items to the inventory
    public void AddItem(GameObject newObject) {
        IItem newItem = newObject.GetComponent<IItem>();
        if (newItem == null || items.Contains(newItem)) return;

        if (!itemDictionary.ContainsKey(newItem.ItemID)) itemDictionary.Add(newItem.ItemID, new List<IItem>() { newItem });
        else {
            itemDictionary[newItem.ItemID].Add(newItem);
            newObject.SetActive(false);
            RefreshSlots();
            return;
        }

        if (items.Count >= maxItems) {
            DropItem(true);
            items.Insert(selectedItem, newItem);
        }
        else {
            items.Add(newItem);
            selectedItem = items.Count - 1;
        }

        newItem.OnPickup(player);
        newObject.transform.SetParent(weaponPos);
        newObject.transform.localPosition = Vector3.zero;
        newObject.transform.localRotation = Quaternion.identity;

        SelectItem(false);
    }

    //Setting active only our selected items
    private void SelectItem(bool switching = true) {
        if (items.Count <= 0) return;

        for (int i = 0; i < items.Count; i++) items[i].GameObject.SetActive(i == selectedItem);

        ItemGameObject = items[selectedItem].GameObject;
        EquippedItem = ItemGameObject.GetComponent<IItem>();

        itemUI.HideUISnap();
        itemUI.HideUI(false);
        UpdateItemUI();
        RefreshSlots();

        AudioManager.Instance.PlayOnce(EquippedItem.HoldSettings.ItemEquipSound, transform.position);
    }

    //Dropping current item
    public void DropItem(bool pickupDrop = false, bool all = false) {
        if (items.Count == 0) return;

        ItemGameObject.transform.SetParent(null);

        EquippedItem.OnDrop(player, (rigidbody) => {
            rigidbody.velocity = player.PlayerMovement.Rb.velocity;

            Vector2 mousePos = player.PlayerCam.ScreenToWorldPoint(Input.mousePosition);
            rigidbody.AddForce(((Vector3) mousePos - transform.position).normalized * throwForce, ForceMode2D.Impulse);

            float rand = Random.Range(-1f, 1f);

            rigidbody.AddTorque(3f * throwForce * rand, ForceMode2D.Impulse);
        });

        itemDictionary[EquippedItem.ItemID].Remove(EquippedItem);
        items[selectedItem] = itemDictionary[EquippedItem.ItemID].Count > 0 ? itemDictionary[EquippedItem.ItemID][0] : null;

        if (items[selectedItem] == null) {
            itemDictionary.Remove(EquippedItem.ItemID);
            items.RemoveAt(selectedItem);
        }
        else {
            items[selectedItem].OnPickup(player);
            items[selectedItem].GameObject.transform.SetParent(weaponPos);
            items[selectedItem].GameObject.transform.localPosition = Vector3.zero;
            items[selectedItem].GameObject.transform.localRotation = Quaternion.identity;

            SelectItem(false);
            return;
        }

        if (items.Count > 0 && !pickupDrop) {
            selectedItem = selectedItem + 1 < items.Count ? selectedItem : items.Count - 1;
            SelectItem();

            if (all) DropItem(false, true);
            return;
        }

        if (items.Count != 0) return;

        ResetData();
    }

    //Updating item UI info
    private void UpdateItemUI() {
        if (EquippedItem == null) return;

        itemEquipArt.color = Color.white;
        itemEquipText.text = EquippedItem.SpriteSettings.ItemText;
        itemEquipArt.sprite = EquippedItem.SpriteSettings.ItemSprite;

        for (int i = 0; i < EquippedItem.AbilitySpriteSettings.Count; i++) {
            ItemArtSettings currentSettings = EquippedItem.AbilitySpriteSettings[i];

            if (currentSettings.NotVisible) {
                itemAbilitiesText[i].text = "";
                itemAbilitiesArt[i].color = Color.clear;
                continue;
            }

            itemAbilitiesArt[i].color = Color.white;
            itemAbilitiesText[i].text = currentSettings.ItemText;
            itemAbilitiesArt[i].sprite = currentSettings.ItemSprite;

            itemAbilitiesArt[i].rectTransform.localScale = currentSettings.Scale;
            itemAbilitiesArt[i].rectTransform.localRotation = Quaternion.Euler(currentSettings.Rotation);
            itemAbilitiesArt[i].rectTransform.localPosition = spriteStartingPositions[i] + currentSettings.Position;
        }
    }

    void ResetData() {
        itemEquipText.text = "";
        itemEquipArt.color = Color.clear;

        foreach (TextMeshProUGUI text in itemAbilitiesText) text.text = "";
        foreach (Image sprite in itemAbilitiesArt) sprite.color = Color.white;

        EquippedItem = null;
        ItemGameObject = null;

        itemUI.HideUI();
    }

    private Dictionary<Image, TextMeshProUGUI> slotCountTexts = new Dictionary<Image, TextMeshProUGUI>();
    void RefreshSlots() {
        foreach (Image slot in inventorySlots) {
            slot.sprite = null;
            slot.color = Color.white;

            if (!slotCountTexts.ContainsKey(slot)) {
                TextMeshProUGUI txt = slot.GetComponentInChildren<TextMeshProUGUI>();
                if (txt == null) continue;

                slotCountTexts.Add(slot, txt);
            }

            slotCountTexts[slot].text = "";
        }
        
        for (int i = 0; i < items.Count; i++) {
            inventorySlots[i].sprite = items[i].SpriteSettings.ItemSprite;
            inventorySlots[i].color = i == selectedItem ? Color.green : Color.white;

            if (!slotCountTexts.ContainsKey(inventorySlots[i])) continue;

            int count = itemDictionary[items[i].ItemID].Count;
            slotCountTexts[inventorySlots[i]].text = count <= 1 ? "" : count.ToString();
        }
    }
}
