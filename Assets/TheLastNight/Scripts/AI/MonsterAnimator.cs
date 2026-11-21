// Procedural animation for the Phase-1 placeholder creature (built from primitives).
// Phase 2: replace with a rigged model + Animator (Mixamo); MonsterAI does not depend on this.
using UnityEngine;
using UnityEngine.AI;

namespace TheLastNight
{
    public class MonsterAnimator : MonoBehaviour
    {
        public Transform body, head, armL, armR, legL, legR;
        public Renderer[] eyeRenderers;
        NavMeshAgent agent;
        MonsterAI ai;
        float phase;
        Material[] eyeMats;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            ai = GetComponent<MonsterAI>();
            if (eyeRenderers != null)
            {
                eyeMats = new Material[eyeRenderers.Length];
                for (int i = 0; i < eyeRenderers.Length; i++) eyeMats[i] = eyeRenderers[i].material;
            }
        }

        void Update()
        {
            float spd = agent != null ? agent.velocity.magnitude : 0f;
            phase += Time.deltaTime * (1.5f + spd * 1.6f);
            float sw = Mathf.Sin(phase) * Mathf.Clamp01(spd / 3f);
            bool chase = ai != null && (ai.State == MonsterState.Chase || ai.State == MonsterState.Attack);
            bool attack = ai != null && ai.State == MonsterState.Attack;

            if (legL != null) legL.localRotation = Quaternion.Euler(sw * 35f, 0f, 0f);
            if (legR != null) legR.localRotation = Quaternion.Euler(-sw * 35f, 0f, 0f);
            float reach = attack ? -150f : chase ? -70f : 0f;
            if (armL != null) armL.localRotation = Quaternion.Euler(reach - sw * 25f, 0f, 8f);
            if (armR != null) armR.localRotation = Quaternion.Euler(reach + sw * 25f, 0f, -8f);
            if (body != null) body.localRotation = Quaternion.Euler(chase ? 18f : 6f, 0f, Mathf.Sin(phase * 0.5f) * 3f);
            if (head != null)
            {
                // unnatural twitches
                float tw = (Mathf.PerlinNoise(Time.time * 3f, 0f) - 0.5f) * 40f;
                float snap = Mathf.PerlinNoise(Time.time * 9f, 5f) > 0.8f ? 25f : 0f;
                head.localRotation = Quaternion.Euler(tw * 0.3f, tw, snap + Mathf.Sin(Time.time * 13f) * 2f);
            }
            if (eyeMats != null)
            {
                Color c = new Color(1f, 0.05f, 0f) * (chase ? 6f : 2.5f);
                foreach (var m in eyeMats) m.SetColor("_EmissionColor", c);
            }
        }
    }
}
