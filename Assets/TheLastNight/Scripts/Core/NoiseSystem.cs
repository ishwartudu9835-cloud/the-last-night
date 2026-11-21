// NoiseSystem: anything loud calls NoiseSystem.Emit(); listeners (the monster) get notified.
// Kept static & allocation-free so footsteps can call it often.
using System;
using UnityEngine;

namespace TheLastNight
{
    public enum NoiseKind { Footstep, Sprint, Door, Object, Generator, Voice, Item }

    public struct NoiseEvent
    {
        public Vector3 Position;
        public float Radius;       // how far it can be heard (metres)
        public NoiseKind Kind;
        public GameObject Source;  // player who made it (may be null)
    }

    public static class NoiseSystem
    {
        public static event Action<NoiseEvent> OnNoise;

        public static void Emit(Vector3 position, float radius, NoiseKind kind, GameObject source = null)
        {
            if (OnNoise == null || radius <= 0f) return;
            NoiseEvent e;
            e.Position = position; e.Radius = radius; e.Kind = kind; e.Source = source;
            OnNoise(e);
        }

        public static float DefaultRadius(NoiseKind k)
        {
            switch (k)
            {
                case NoiseKind.Footstep: return 3f;
                case NoiseKind.Sprint: return 11f;
                case NoiseKind.Door: return 9f;
                case NoiseKind.Object: return 12f;
                case NoiseKind.Generator: return 30f;
                case NoiseKind.Voice: return 8f;
                default: return 5f;
            }
        }
    }
}
