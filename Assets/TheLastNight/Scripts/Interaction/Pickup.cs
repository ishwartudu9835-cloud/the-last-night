// Pickup: fuses, batteries, medkits and readable notes (story clues).
using UnityEngine;

namespace TheLastNight
{
    public class Pickup : MonoBehaviour, IInteractable
    {
        public ItemType type = ItemType.Battery;
        [Header("Notes only")]
        public string noteTitle;
        [TextArea(4, 12)] public string noteBody;

        [Header("Visual")]
        public float bobAmount = 0.04f;
        Vector3 basePos;
        float phase;

        void Awake() { basePos = transform.position; phase = Random.value * 6f; }

        void Update()
        {
            if (type == ItemType.Note || bobAmount <= 0f) return;
            transform.position = basePos + Vector3.up * Mathf.Sin(Time.time * 2f + phase) * bobAmount;
            transform.Rotate(0f, 40f * Time.deltaTime, 0f, Space.World);
        }

        public string Prompt(PlayerController p)
        {
            switch (type)
            {
                case ItemType.Fuse: return "Fuse uthao";
                case ItemType.Battery: return "Battery uthao";
                case ItemType.Medkit: return "Medkit uthao";
                default: return "Padho: " + noteTitle;
            }
        }

        public float HoldTime(PlayerController p) { return 0f; }

        public void Interact(PlayerController p)
        {
            var am = AudioManager.Instance;
            if (type == ItemType.Note)
            {
                if (am != null) am.PlayAt(ProceduralAudio.Paper, transform.position, 0.6f);
                if (GameManager.Instance != null) GameManager.Instance.OnNoteRead(noteTitle, noteBody);
                gameObject.SetActive(false);
                return;
            }
            if (p.Inventory == null || !p.Inventory.Add(type)) return;
            if (am != null) am.PlayAt(ProceduralAudio.Pickup, transform.position, 0.7f);
            NoiseSystem.Emit(transform.position, NoiseSystem.DefaultRadius(NoiseKind.Item), NoiseKind.Item, p.gameObject);
            gameObject.SetActive(false);
        }
    }
}
