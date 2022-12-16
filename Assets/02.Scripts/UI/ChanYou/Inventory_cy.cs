using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_cy : MonoBehaviour
{
    public static Inventory_cy instance;

    private DatabaseManager_cy theData;

    public List<RectTransform> Nodes;
    private List<Item_cy> InventoryItem;
    private List<Item_cy> Item_Consume;
    private List<Item_cy> Item_Equip;
    private List<Item_cy> Item_ETC;

    //test
    public GameObject[] test2;
    public GameObject test;

    [SerializeField]
    private InventorySlot_cy slots; //인벤토리 슬롯들
    public GameObject ItemSlot; //인벤토리 하위에 들어갈 아이템 정보가 담긴 오브젝트

    public GameObject prefab_floating_text;
    public Transform messageTr;


    void Start()
    {
        instance = this;
        theData = FindObjectOfType<DatabaseManager_cy>();
        InventoryItem = new List<Item_cy>();
        Item_Consume = new List<Item_cy>();
        Item_Equip = new List<Item_cy>();
        Item_ETC = new List<Item_cy>();

        slots = Resources.Load<GameObject>("Item/item").GetComponent<InventorySlot_cy>();
        ItemSlot = Resources.Load<GameObject>("Item/item");


    }


    void Update()
    {
        
    }

    public void GetAnItem(int itemID, int _count)
    {
        for (int i = 0; i < theData.itemList.Count; i++) //데이터베이스에서 아이템 검색
        {
            if (itemID == theData.itemList[i].itemID)
            {
                AddItem();
                GameObject clone = Instantiate(prefab_floating_text, messageTr.position, Quaternion.Euler(Vector3.zero));
                clone.GetComponent<FloatingText>().text.text = theData.itemList[i].itemName + " " + _count + "개 획득 +";
                clone.transform.SetParent(this.transform);

                for (int j = 0; j < InventoryItem.Count; j++) //소지품에 같은 아이템이 있는지 검색
                {
                    if (InventoryItem[j].itemID == itemID)
                    {
                        if (InventoryItem[j].itemType == Item_cy.ItemType.Equip)
                        {
                            InventoryItem.Add(theData.itemList[i]);
                        }
                        else  //소지품에 같은 아이템이 있다 -> 갯수만 증감시켜줌
                        {
                            InventoryItem[j].itemCount += _count;
                        }
                        return;
                    }
                }
                InventoryItem.Add(theData.itemList[i]); //소지품에 해당 아이템 추가
                InventoryItem[InventoryItem.Count - 1].itemCount = _count;
                return;
            }
        }
    }

    public void AddItem()
    {
        GameObject clone = Instantiate(ItemSlot, Vector3.zero, Quaternion.identity);
        clone.transform.SetParent(ItemGetIN().transform);
    }

    public void ShowItem()
    {
        for (int i = 0; i < InventoryItem.Count; i++)
        {
            if (Item_cy.ItemType.Consume == InventoryItem[i].itemType)
                Item_Consume.Add(InventoryItem[i]);
            else if (Item_cy.ItemType.Equip == InventoryItem[i].itemType)
                Item_Equip.Add(InventoryItem[i]);
            else
                Item_ETC.Add(InventoryItem[i]);
        }
    }

    public GameObject ItemGetIN()
    {
        GameObject emptyInven = null;
        for (int i = 0; i < test2.Length; i++)
        {
            if (test2[i].transform.GetChild(0).GetComponent<GameObject>() == null)
                emptyInven = test2[i];
            break;
        }
        return emptyInven;
    }

    //private void OnEnable() 하위 모든 트랜스폼에 대해서 적용이 되어버림...
    //{
    //    RectTransform[] transforms = GetComponentsInChildren<RectTransform>();
    //    Nodes = new List<RectTransform>();

    //    foreach (RectTransform tr in transforms)
    //    {
    //        if (tr != this.transform)
    //            Nodes.Add(tr);
    //    }
    //}
}
