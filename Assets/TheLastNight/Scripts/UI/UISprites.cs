// UISprites: crisp, anti-aliased UI art generated at runtime from signed-distance shapes
// (icons, rings, gradients, vignette, paper). No image files needed, scales cleanly.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastNight
{
    public static class UISprites
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public static Sprite Circle { get { return Get("circle", 128, p => Len(p) - 0.96f); } }
        public static Sprite SoftCircle { get { return Soft("softcircle", 128, 0.0f); } }
        public static Sprite Ring { get { return Get("ring", 256, p => Mathf.Abs(Len(p) - 0.92f) - 0.035f); } }
        public static Sprite RingThick { get { return Get("ringthick", 256, p => Mathf.Abs(Len(p) - 0.9f) - 0.07f); } }
        public static Sprite Pill { get { return Sliced("pill", 64, 31); } }
        public static Sprite Rounded { get { return Sliced("rounded", 64, 12); } }
        public static Sprite Square { get { return Sliced("square", 8, 0); } }

        // ---------------- icons (drawn in a -1..1 box, y up) ----------------
        public static Sprite IconHand { get { return Get("hand", 128, Hand); } }
        public static Sprite IconRun { get { return Get("run", 128, Run); } }
        public static Sprite IconCrouch { get { return Get("crouch", 128, Crouch); } }
        public static Sprite IconTorch { get { return Get("torch", 128, Torch); } }
        public static Sprite IconBag { get { return Get("bag", 128, Bag); } }
        public static Sprite IconPause { get { return Get("pause", 128, p => Mathf.Min(Box(p - new Vector2(-0.28f, 0), new Vector2(0.13f, 0.55f), 0.05f), Box(p - new Vector2(0.28f, 0), new Vector2(0.13f, 0.55f), 0.05f))); } }
        public static Sprite IconBattery { get { return Get("battery", 128, Battery); } }
        public static Sprite IconHeart { get { return Get("heart", 128, Heart); } }
        public static Sprite IconFuse { get { return Get("fuse", 128, Fuse); } }

        static float Hand(Vector2 p)
        {
            float d = Box(p - new Vector2(0.02f, -0.3f), new Vector2(0.42f, 0.36f), 0.18f);                 // palm
            float[] fx = { -0.3f, -0.1f, 0.1f, 0.3f };
            float[] fh = { 0.38f, 0.5f, 0.48f, 0.36f };
            for (int i = 0; i < 4; i++) d = Mathf.Min(d, Seg(p, new Vector2(fx[i], -0.1f), new Vector2(fx[i], fh[i]), 0.085f));
            d = Mathf.Min(d, Seg(p, new Vector2(-0.38f, -0.32f), new Vector2(-0.68f, -0.02f), 0.09f));       // thumb
            return d;
        }

        static float Run(Vector2 p)
        {
            float d = Len(p - new Vector2(0.18f, 0.62f)) - 0.15f;                                          // head
            d = Mathf.Min(d, Seg(p, new Vector2(0.1f, 0.38f), new Vector2(-0.08f, -0.08f), 0.09f));         // torso
            d = Mathf.Min(d, Seg(p, new Vector2(0.06f, 0.3f), new Vector2(0.42f, 0.12f), 0.07f));           // arm fwd
            d = Mathf.Min(d, Seg(p, new Vector2(0.06f, 0.3f), new Vector2(-0.3f, 0.36f), 0.07f));           // arm back
            d = Mathf.Min(d, Seg(p, new Vector2(-0.08f, -0.08f), new Vector2(0.3f, -0.32f), 0.08f));        // leg fwd
            d = Mathf.Min(d, Seg(p, new Vector2(0.3f, -0.32f), new Vector2(0.22f, -0.72f), 0.08f));
            d = Mathf.Min(d, Seg(p, new Vector2(-0.08f, -0.08f), new Vector2(-0.38f, -0.4f), 0.08f));       // leg back
            d = Mathf.Min(d, Seg(p, new Vector2(-0.38f, -0.4f), new Vector2(-0.66f, -0.3f), 0.08f));
            return d;
        }

        static float Crouch(Vector2 p)
        {
            float d = Len(p - new Vector2(0.28f, 0.22f)) - 0.15f;
            d = Mathf.Min(d, Seg(p, new Vector2(0.16f, 0.06f), new Vector2(-0.3f, -0.12f), 0.09f));
            d = Mathf.Min(d, Seg(p, new Vector2(-0.3f, -0.12f), new Vector2(0.05f, -0.4f), 0.08f));
            d = Mathf.Min(d, Seg(p, new Vector2(0.05f, -0.4f), new Vector2(-0.2f, -0.62f), 0.08f));
            d = Mathf.Min(d, Seg(p, new Vector2(0.1f, 0.0f), new Vector2(0.3f, -0.25f), 0.065f));
            d = Mathf.Min(d, Box(p - new Vector2(0f, -0.82f), new Vector2(0.72f, 0.04f), 0.03f));            // ground line
            return d;
        }

        static float Torch(Vector2 p)
        {
            // rotate 45° so the torch points up-right
            Vector2 q = Rot(p - new Vector2(-0.12f, -0.12f), -45f);
            float d = Box(q - new Vector2(0f, -0.25f), new Vector2(0.13f, 0.38f), 0.05f);                   // body
            d = Mathf.Min(d, Trap(q - new Vector2(0f, 0.22f), 0.13f, 0.24f, 0.13f));                        // head
            Vector2 r = p - new Vector2(-0.12f, -0.12f);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 dir = Rot(new Vector2(0.62f, 0.62f).normalized, i * 22f);
                d = Mathf.Min(d, Seg(r, dir * 0.62f, dir * 0.92f, 0.05f));                                   // rays
            }
            return d;
        }

        static float Bag(Vector2 p)
        {
            float d = Box(p - new Vector2(0f, -0.2f), new Vector2(0.58f, 0.46f), 0.14f);
            float handle = Mathf.Abs(Len(p - new Vector2(0f, 0.28f)) - 0.27f) - 0.07f;
            handle = Mathf.Max(handle, -(p.y - 0.28f));
            d = Mathf.Min(d, handle);
            d = Mathf.Max(d, -Box(p - new Vector2(0f, -0.08f), new Vector2(0.58f, 0.035f), 0f));            // strap cut
            return d;
        }

        static float Battery(Vector2 p)
        {
            float outer = Box(p, new Vector2(0.72f, 0.38f), 0.08f);
            float inner = Box(p, new Vector2(0.62f, 0.28f), 0.04f);
            float d = Mathf.Max(outer, -inner);
            return Mathf.Min(d, Box(p - new Vector2(0.82f, 0f), new Vector2(0.08f, 0.16f), 0.03f));
        }

        static float Heart(Vector2 p)
        {
            p.y -= 0.12f;
            float d = Mathf.Min(Len(p - new Vector2(-0.3f, 0.12f)) - 0.34f, Len(p - new Vector2(0.3f, 0.12f)) - 0.34f);
            Vector2 a = new Vector2(-0.62f, 0.02f), b = new Vector2(0.62f, 0.02f), c = new Vector2(0f, -0.78f);
            return Mathf.Min(d, Tri(p, a, b, c));
        }

        static float Fuse(Vector2 p)
        {
            float d = Box(p, new Vector2(0.24f, 0.5f), 0.12f);
            d = Mathf.Min(d, Box(p - new Vector2(0, 0.62f), new Vector2(0.16f, 0.1f), 0.03f));
            d = Mathf.Min(d, Box(p - new Vector2(0, -0.62f), new Vector2(0.16f, 0.1f), 0.03f));
            return d;
        }

        // ---------------- large textures ----------------
        /// <summary>Red/black radial vignette for damage and fear.</summary>
        public static Sprite Vignette
        {
            get
            {
                return Make("vignette", 256, 256, (u, v) =>
                {
                    float d = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 1.41f;
                    float a = Mathf.SmoothStep(0.45f, 1.05f, d);
                    return new Color(1, 1, 1, a);
                });
            }
        }

        /// <summary>Horizontal dark gradient for menu backdrops (opaque at left).</summary>
        public static Sprite SideShade
        {
            get { return Make("sideshade", 256, 4, (u, v) => new Color(1, 1, 1, Mathf.Clamp01(1.15f - u * 1.3f))); }
        }

        public static Sprite VerticalShade
        {
            get { return Make("vshade", 4, 256, (u, v) => new Color(1, 1, 1, Mathf.SmoothStep(0f, 1f, 1f - v))); }
        }

        /// <summary>Aged paper with stains for notes.</summary>
        public static Sprite Paper
        {
            get
            {
                return Make("paper", 512, 512, (u, v) =>
                {
                    float n = Mathf.PerlinNoise(u * 6f, v * 6f) * 0.5f + Mathf.PerlinNoise(u * 40f, v * 40f) * 0.15f;
                    float edge = Mathf.Min(Mathf.Min(u, 1 - u), Mathf.Min(v, 1 - v));
                    float burn = Mathf.Clamp01(1f - edge * 9f) * 0.45f;
                    float stain = Mathf.Clamp01(Mathf.PerlinNoise(u * 3f + 7f, v * 3f) - 0.62f) * 1.3f;
                    float b = 0.86f - n * 0.18f - burn - stain * 0.25f;
                    return new Color(b, b * 0.94f, b * 0.8f, 1f);
                });
            }
        }

        // ---------------- SDF machinery ----------------
        static float Len(Vector2 p) { return p.magnitude; }
        static Vector2 Rot(Vector2 p, float deg) { float r = deg * Mathf.Deg2Rad, c = Mathf.Cos(r), s = Mathf.Sin(r); return new Vector2(p.x * c - p.y * s, p.x * s + p.y * c); }
        static float Box(Vector2 p, Vector2 b, float r)
        {
            Vector2 q = new Vector2(Mathf.Abs(p.x) - b.x + r, Mathf.Abs(p.y) - b.y + r);
            return new Vector2(Mathf.Max(q.x, 0), Mathf.Max(q.y, 0)).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0) - r;
        }
        static float Seg(Vector2 p, Vector2 a, Vector2 b, float r)
        {
            Vector2 pa = p - a, ba = b - a;
            float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * h).magnitude - r;
        }
        static float Trap(Vector2 p, float r1, float r2, float he)
        {
            // isosceles trapezoid: bottom half-width r1, top half-width r2, half-height he
            float t = Mathf.Clamp01((p.y + he) / (2f * he));
            float w = Mathf.Lerp(r1, r2, t);
            return Mathf.Max(Mathf.Abs(p.x) - w, Mathf.Abs(p.y) - he);
        }
        static float Tri(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            // sign-of-edges inside test with distance to nearest edge
            float d = Mathf.Min(Seg(p, a, b, 0f), Mathf.Min(Seg(p, b, c, 0f), Seg(p, c, a, 0f)));
            bool s1 = Cross(b - a, p - a) < 0, s2 = Cross(c - b, p - b) < 0, s3 = Cross(a - c, p - c) < 0;
            bool inside = (s1 == s2) && (s2 == s3);
            return inside ? -d : d;
        }
        static float Cross(Vector2 a, Vector2 b) { return a.x * b.y - a.y * b.x; }

        static Sprite Get(string key, int size, Func<Vector2, float> sdf)
        {
            Sprite s;
            if (cache.TryGetValue(key, out s) && s != null) return s;
            float px = 2f / size; // one pixel in shape units
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            var data = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f);
                    float a = Mathf.Clamp01(0.5f - sdf(p) / px);
                    data[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            tex.SetPixels32(data); tex.Apply();
            s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s.name = key;
            cache[key] = s;
            return s;
        }

        static Sprite Soft(string key, int size, float hard)
        {
            Sprite s;
            if (cache.TryGetValue(key, out s) && s != null) return s;
            s = Make(key, size, size, (u, v) =>
            {
                float d = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f;
                return new Color(1, 1, 1, Mathf.Clamp01((1f - d) / Mathf.Max(0.01f, 1f - hard)));
            });
            return s;
        }

        static Sprite Sliced(string key, int size, int radius)
        {
            Sprite s;
            if (cache.TryGetValue(key, out s) && s != null) return s;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            var data = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float a = 1f;
                    if (radius > 0)
                    {
                        float cx = Mathf.Clamp(x + 0.5f, radius, size - radius), cy = Mathf.Clamp(y + 0.5f, radius, size - radius);
                        a = Mathf.Clamp01(radius - Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy)) + 0.5f);
                    }
                    data[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            tex.SetPixels32(data); tex.Apply();
            float b = Mathf.Max(radius, 1);
            s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(b, b, b, b));
            cache[key] = s;
            return s;
        }

        static Sprite Make(string key, int w, int h, Func<float, float, Color> f)
        {
            Sprite s;
            if (cache.TryGetValue(key, out s) && s != null) return s;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            var data = new Color[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) data[y * w + x] = f((x + 0.5f) / w, (y + 0.5f) / h);
            tex.SetPixels(data); tex.Apply();
            s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            cache[key] = s;
            return s;
        }
    }
}
