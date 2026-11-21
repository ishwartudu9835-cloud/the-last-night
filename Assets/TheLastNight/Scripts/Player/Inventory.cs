// Inventory: counts of simple stackable items. Phase 3 will add keys / puzzle items here.
using System;
using UnityEngine;

namespace TheLastNight
{
    public enum ItemType { Fuse, Battery, Medkit, Note }

    public class Inventory : MonoBehaviour
    {
        public int maxBatteries = 4;
        public int Fuses { get; private set; }
        public int Batteries { get; private set; }
        public int Medkits { get; private set; }
        public event Action Changed;

        PlayerController pc;
        void Awake() { pc = GetComponent<PlayerController>(); }

        /// <returns>false if the item cannot be carried (e.g. full)</returns>
        public bool Add(ItemType type)
        {
            switch (type)
            {
                case ItemType.Fuse: Fuses++; if (GameManager.Instance != null) GameManager.Instance.OnFusePicked(); break;
                case ItemType.Battery:
                    if (Batteries >= maxBatteries) { Say("Aur batteries nahi utha sakte"); return false; }
                    Batteries++; Say("+1 Battery (Inventory se lagao)"); break;
                case ItemType.Medkit: Medkits++; Say("+1 Medkit"); break;
                default: break;
            }
            Notify();
            return true;
        }

        public bool TakeFuse() { if (Fuses <= 0) return false; Fuses--; Notify(); return true; }

        public void UseBattery()
        {
            if (Batteries <= 0 || pc == null || pc.Flashlight == null) { Say("Koi spare battery nahi"); return; }
            Batteries--;
            pc.Flashlight.InsertBattery();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayUI(ProceduralAudio.Pickup, 0.5f);
            Say("Nayi battery lagi");
            Notify();
        }

        public void UseMedkit()
        {
            if (Medkits <= 0 || pc == null || pc.Health == null) return;
            Medkits--;
            pc.Health.Heal(50f);
            Say("Medkit use kiya");
            Notify();
        }

        void Say(string t) { if (GameManager.Instance != null) GameManager.Instance.Say(t, 2.2f); }
        void Notify() { if (Changed != null) Changed(); }
    }
}
