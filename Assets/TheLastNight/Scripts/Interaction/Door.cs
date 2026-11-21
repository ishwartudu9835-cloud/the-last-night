// Hinged door. Player opens/closes it (creak + noise), the monster bursts through it,
// and the HorrorDirector can slam it shut.
using System.Collections;
using UnityEngine;

namespace TheLastNight
{
    public class Door : MonoBehaviour, IInteractable
    {
        public Transform hinge;          // rotates around Y
        public float openAngle = 100f;
        public float openSeconds = 0.9f;
        public bool locked;
        public string lockedMessage = "Band hai. Andar se kisi ne lagaya hai...";

        public bool IsOpen { get; private set; }
        Coroutine anim;
        float closedYaw;

        void Awake() { closedYaw = hinge.localEulerAngles.y; }

        public string Prompt(PlayerController p) { return locked ? "Darwaza (band)" : IsOpen ? "Darwaza band karo" : "Darwaza kholo"; }
        public float HoldTime(PlayerController p) { return 0f; }

        public void Interact(PlayerController p)
        {
            if (locked)
            {
                if (GameManager.Instance != null) GameManager.Instance.Say(lockedMessage, 2.5f);
                Play(ProceduralAudio.DoorRattle, 0.6f);
                NoiseSystem.Emit(transform.position, 6f, NoiseKind.Door, p.gameObject);
                return;
            }
            // opening slowly while crouched is quieter
            float speed = p.IsCrouching ? 1.8f : 1f;
            SetOpen(!IsOpen, openSeconds * speed, p.transform.position);
            Play(ProceduralAudio.DoorCreak, p.IsCrouching ? 0.35f : 0.8f);
            NoiseSystem.Emit(transform.position, NoiseSystem.DefaultRadius(NoiseKind.Door) * (p.IsCrouching ? 0.45f : 1f), NoiseKind.Door, p.gameObject);
        }

        /// <summary>Monster pushes through: fast, loud.</summary>
        public void ForceOpen(Vector3 fromPos)
        {
            if (IsOpen) return;
            locked = false;
            SetOpen(true, 0.25f, fromPos);
            Play(ProceduralAudio.DoorSlam, 1f);
        }

        /// <summary>Horror event: slam shut.</summary>
        public void Slam()
        {
            if (!IsOpen) return;
            SetOpen(false, 0.18f, transform.position);
            Play(ProceduralAudio.DoorSlam, 1f);
            NoiseSystem.Emit(transform.position, NoiseSystem.DefaultRadius(NoiseKind.Door), NoiseKind.Door, null);
        }

        void SetOpen(bool open, float seconds, Vector3 fromPos)
        {
            IsOpen = open;
            // swing away from whoever opens it
            float sign = Vector3.Dot(fromPos - hinge.position, transform.forward) > 0f ? -1f : 1f;
            float target = open ? closedYaw + openAngle * sign : closedYaw;
            if (anim != null) StopCoroutine(anim);
            anim = StartCoroutine(Swing(target, seconds));
        }

        IEnumerator Swing(float targetYaw, float seconds)
        {
            Quaternion from = hinge.localRotation;
            Quaternion to = Quaternion.Euler(hinge.localEulerAngles.x, targetYaw, hinge.localEulerAngles.z);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, seconds);
                hinge.localRotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            anim = null;
        }

        void Play(AudioClip clip, float vol)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayAt(clip, hinge.position + Vector3.up, vol, Random.Range(0.9f, 1.1f));
        }
    }
}
