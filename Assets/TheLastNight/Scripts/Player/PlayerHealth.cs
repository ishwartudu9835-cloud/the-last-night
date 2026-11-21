// Health + fear/stress. Fear rises near the monster, in darkness and on scares, and drives
// heartbeat, breathing, screen distortion (FearPostFX) and the monster's perception.
using System;
using UnityEngine;

namespace TheLastNight
{
    public class PlayerHealth : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float regenDelay = 8f;
        public float regenPerSecond = 4f;

        public float Health { get; private set; }
        public float Health01 { get { return Health / maxHealth; } }
        public bool IsDead { get; private set; }
        public float Fear { get; private set; }            // 0..1
        public float SpeedMultiplier { get { return Health01 < 0.35f ? 0.85f : 1f; } }

        public event Action<float> Damaged;                 // amount
        public event Action Died;

        float lastHitTime = -99f, scareBoost;
        PlayerController pc;

        void Awake() { Health = maxHealth; pc = GetComponent<PlayerController>(); }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || !gm.IsPlaying || IsDead) return;

            if (Time.time - lastHitTime > regenDelay && Health < maxHealth)
                Health = Mathf.Min(maxHealth, Health + regenPerSecond * Time.deltaTime);

            // ---- fear target ----
            float target = 0.05f;
            var m = gm.Monster;
            if (m != null)
            {
                float d = Vector3.Distance(m.transform.position, transform.position);
                target += Mathf.Clamp01(1f - d / 22f) * 0.55f;
                if (m.State == MonsterState.Chase || m.State == MonsterState.Attack) target += 0.3f;
            }
            if (pc != null && pc.Flashlight != null && !pc.Flashlight.IsOn) target += 0.12f;
            if (pc != null && pc.IsHidden && m != null && Vector3.Distance(m.transform.position, transform.position) < 6f) target += 0.2f;
            target += scareBoost;
            target += (1f - Health01) * 0.2f;
            scareBoost = Mathf.Max(0f, scareBoost - Time.deltaTime * 0.08f);
            target = Mathf.Clamp01(target);
            Fear = Mathf.MoveTowards(Fear, target, Time.deltaTime * (target > Fear ? 0.6f : 0.12f));
        }

        public void Heal(float amount) { if (!IsDead) Health = Mathf.Min(maxHealth, Health + amount); }

        public void AddScare(float amount) { scareBoost = Mathf.Clamp01(scareBoost + amount); }

        public void TakeDamage(float amount)
        {
            if (IsDead) return;
            Health -= amount;
            lastHitTime = Time.time;
            AddScare(0.4f);
            if (Damaged != null) Damaged(amount);
            if (Health <= 0f)
            {
                Health = 0f;
                IsDead = true;
                if (Died != null) Died();
                if (GameManager.Instance != null) GameManager.Instance.PlayerDied();
            }
        }
    }
}
