using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SSI2Showcase_Equipment : MonoBehaviour
{
    public SSI2_InventoryScript inventory;

    [System.Serializable]
    public class gunObj {
        public string gunName;
        public Transform viewmodelObj;
        
    }
    public List<gunObj> guns;

    public KeyCode fireKey = KeyCode.Mouse0;
    public KeyCode reloadKey = KeyCode.R;
    public SSI2Showcase_GunItem equippedGun = null;
    public Transform currentViewmodel;
    public Text ammoText;

    AudioSource aud;
    bool firingGun = false;
    bool reloading = false;

    public LayerMask firingMask;
    // Start is called before the first frame update
    void Start()
    {
        aud = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(equippedGun != null)
        {
            ammoText.text = $"{equippedGun.ammo}/{equippedGun.maxAmmo}";
            if (Input.GetKeyDown(fireKey) && !firingGun && equippedGun.ammo > 0 && !inventory.inventoryOpen && !reloading)
            {
                StartCoroutine(FireGun());
            }
            if(Input.GetKeyDown(reloadKey) && equippedGun.ammo != equippedGun.maxAmmo && !inventory.inventoryOpen && !reloading)
            {
                StartCoroutine(Reload());
            }
        }
    }

    void equipItem(SSI2_ItemScript item)
    {
        if(item.itemTag == "Gun")
        {
            equippedGun = item.GetComponent<SSI2Showcase_GunItem>();
            foreach (gunObj gun in guns)
            {
                gun.viewmodelObj.gameObject.SetActive(item.itemName == gun.gunName ? true : false);
                if(item.itemName == gun.gunName) currentViewmodel = gun.viewmodelObj;
            }
        }
    }

    void dequipItem(SSI2_ItemScript item)
    {
        if (item.itemTag == "Gun")
        {
            equippedGun = null;
            foreach (gunObj gun in guns)
            {
                gun.viewmodelObj.gameObject.SetActive(false);
                currentViewmodel = null;
            }
        }
    }

    public IEnumerator Reload()
    {
        reloading = true;
        int initialAmmoNeeded = equippedGun.maxAmmo - equippedGun.ammo;
        int ammoNeeded = equippedGun.maxAmmo - equippedGun.ammo;
        List<string> items = new List<string>();
        List<int> quantities = new List<int>();
        foreach (SSI2Showcase_GunItem.ammoClass i in equippedGun.acceptedAmmo) //First we're going to check if there is any ammo to take
        {
            if(i.type == SSI2Showcase_GunItem.ammoType.Name) //Find ammo by name first
            {
                List<SSI2_UIItemScript> itemsOfType;
                List<int> quantitiesOfType;
                inventory.FetchItemsOfName(i.ammoName, ammoNeeded, false, out itemsOfType, out quantitiesOfType);
                if (itemsOfType.Count > 0)
                {
                    foreach (int quantity in quantitiesOfType)
                    {
                        ammoNeeded = Mathf.Clamp(ammoNeeded - quantity, 0, equippedGun.maxAmmo - equippedGun.ammo); //In case we don't have enough ammo
                    }
                    items.Add(i.ammoName);
                    quantities.Add((equippedGun.maxAmmo - equippedGun.ammo) - ammoNeeded);
                }
            }
            else //Then by type if it doesn't work
            {
                List<SSI2_UIItemScript> itemsOfType;
                List<int> quantitiesOfType;
                inventory.FetchItemsOfType(i.ammoName, ammoNeeded, false, out itemsOfType, out quantitiesOfType);
                if (itemsOfType.Count > 0)
                {
                    foreach (int quantity in quantitiesOfType)
                    {
                        ammoNeeded = Mathf.Clamp(ammoNeeded - quantity, 0, equippedGun.maxAmmo - equippedGun.ammo); //In case we don't have enough ammo
                    }
                    items.Add(i.ammoName);
                    quantities.Add((equippedGun.maxAmmo - equippedGun.ammo) - ammoNeeded);
                }
            }
            if (ammoNeeded <= 0) break;
        }

        if(items.Count > 0) //Now we need to get the inventory to destroy the amount of ammo we've fetched
        {
            currentViewmodel.GetComponent<Animation>().Play(equippedGun.animationSet.reloadAnim.name);
            yield return new WaitForSeconds(currentViewmodel.GetComponent<Animation>()[equippedGun.animationSet.reloadAnim.name].length);
            for (int i = 0; i < items.Count; i++)
            {
                SSI2Showcase_GunItem.ammoType ammoType = equippedGun.acceptedAmmo.Find(x => x.ammoName == items[i]).type;
                if(ammoType == SSI2Showcase_GunItem.ammoType.Name) inventory.DropItemsOfName(items[i], quantities[i], false, true);
                else inventory.DropItemsOfType(items[i], quantities[i], false, true);

                foreach (int quantity in quantities)
                {
                    equippedGun.ammo += quantity;
                }
            }
        }
        reloading = false;
        yield return new WaitForSeconds(0);
    }

    public IEnumerator FireGun()
    {
        currentViewmodel.GetComponent<Animation>().Stop();
        currentViewmodel.GetComponent<Animation>().Play(equippedGun.animationSet.fireAnim.name);
        firingGun = true;
        equippedGun.ammo -= 1;
        aud.volume = equippedGun.fireSoundVolume;
        aud.PlayOneShot(equippedGun.fireSound);

        FireShot();

        yield return new WaitForSeconds(1 / equippedGun.rps);

        if (!Input.GetKey(fireKey) || !equippedGun.fullAuto || equippedGun.ammo == 0)
        {
            firingGun = false;
        } else if (Input.GetKey(fireKey) && equippedGun.fullAuto && equippedGun.ammo > 0)
        {
            StartCoroutine(FireGun());
        }
    }

    public void FireShot()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, equippedGun.range, firingMask))
        {
            hit.transform.SendMessage("Damage", equippedGun.damage, SendMessageOptions.DontRequireReceiver);
        }
    }
}
