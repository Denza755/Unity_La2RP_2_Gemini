[System.Serializable]
public class Weapon : AbstractItem {
    public Weapongrp Weapongrp { get { return _itemgrp as Weapongrp; } }

    public Weapon(int id, ItemName itemName, ItemStatData itemStatData, Weapongrp weapongrp) 
        : base(id, itemName, itemStatData, weapongrp, weapongrp != null ? weapongrp.Icon : "") {
    }
}
