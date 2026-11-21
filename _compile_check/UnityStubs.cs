// Compile-check stubs: signatures only (copied to match the real Unity 6 API surface we use).
// NOT shipped. Used to compile The Last Night scripts with mcs outside the Unity editor.
#pragma warning disable 0067, 0649, 0414, 0169
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    public class Object
    {
        public string name { get; set; }
        public HideFlags hideFlags { get; set; }
        public static void Destroy(Object o) { }
        public static void Destroy(Object o, float t) { }
        public static void DestroyImmediate(Object o) { }
        public static T FindFirstObjectByType<T>() where T : Object { return null; }
        public static T[] FindObjectsByType<T>(FindObjectsSortMode m) where T : Object { return null; }
        public static T Instantiate<T>(T o) where T : Object { return o; }
        public static implicit operator bool(Object o) { return o != null; }
    }
    public enum FindObjectsSortMode { None, InstanceID }
    public enum HideFlags { None }
    public enum SendMessageOptions { RequireReceiver, DontRequireReceiver }
    public class Component : Object
    {
        public Transform transform { get { return null; } }
        public GameObject gameObject { get { return null; } }
        public string tag { get; set; }
        public T GetComponent<T>() { return default(T); }
        public T GetComponentInParent<T>() { return default(T); }
        public T GetComponentInChildren<T>() { return default(T); }
        public T[] GetComponentsInChildren<T>() { return null; }
        public void SendMessage(string m, object v, SendMessageOptions o) { }
    }
    public class Behaviour : Component { public bool enabled { get; set; } public bool isActiveAndEnabled { get { return true; } } }
    public class MonoBehaviour : Behaviour
    {
        public Coroutine StartCoroutine(IEnumerator r) { return null; }
        public void StopCoroutine(Coroutine c) { }
        public void Invoke(string m, float t) { }
    }
    public class ScriptableObject : Object { public static T CreateInstance<T>() where T : ScriptableObject { return null; } }
    public sealed class Coroutine { }
    public class YieldInstruction { }
    public class CustomYieldInstruction : IEnumerator { public object Current { get { return null; } } public bool MoveNext() { return false; } public void Reset() { } }
    public sealed class WaitForSeconds : YieldInstruction { public WaitForSeconds(float s) { } }
    public class WaitForSecondsRealtime : CustomYieldInstruction { public WaitForSecondsRealtime(float s) { } }

    public class GameObject : Object
    {
        public GameObject() { } public GameObject(string n) { } public GameObject(string n, params Type[] c) { }
        public Transform transform { get { return null; } }
        public string tag { get; set; }
        public bool activeSelf { get { return true; } }
        public bool isStatic { get; set; }
        public void SetActive(bool v) { }
        public T AddComponent<T>() where T : Component { return null; }
        public T GetComponent<T>() { return default(T); }
        public T[] GetComponentsInChildren<T>() { return null; }
        public static GameObject CreatePrimitive(PrimitiveType t) { return null; }
    }
    public enum PrimitiveType { Sphere, Capsule, Cylinder, Cube, Plane, Quad }
    public enum Space { World, Self }
    public class Transform : Component, IEnumerable
    {
        public Vector3 position { get; set; } public Vector3 localPosition { get; set; }
        public Quaternion rotation { get; set; } public Quaternion localRotation { get; set; }
        public Vector3 eulerAngles { get; set; } public Vector3 localEulerAngles { get; set; }
        public Vector3 localScale { get; set; } public Vector3 lossyScale { get { return default(Vector3); } }
        public Vector3 forward { get; set; } public Vector3 right { get; set; } public Vector3 up { get; set; }
        public Transform parent { get; set; }
        public void SetParent(Transform p) { } public void SetParent(Transform p, bool worldPositionStays) { }
        public void SetPositionAndRotation(Vector3 p, Quaternion r) { }
        public void Rotate(float x, float y, float z, Space s) { } public void Rotate(float x, float y, float z) { }
        public Vector3 TransformPoint(Vector3 p) { return p; }
        public IEnumerator GetEnumerator() { return null; }
    }
    public sealed class RectTransform : Transform
    {
        public Vector2 anchorMin { get; set; } public Vector2 anchorMax { get; set; } public Vector2 pivot { get; set; }
        public Vector2 sizeDelta { get; set; } public Vector2 anchoredPosition { get; set; } public Vector2 offsetMin { get; set; } public Vector2 offsetMax { get; set; }
    }
    public static class RectTransformUtility { public static bool ScreenPointToLocalPointInRectangle(RectTransform r, Vector2 sp, Camera c, out Vector2 lp) { lp = default(Vector2); return true; } }

    public struct Vector2
    {
        public float x, y; public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero { get { return default(Vector2); } } public static Vector2 one { get { return default(Vector2); } }
        public float magnitude { get { return 0; } } public float sqrMagnitude { get { return 0; } } public Vector2 normalized { get { return this; } }
        public static Vector2 ClampMagnitude(Vector2 v, float m) { return v; }
        public static float Distance(Vector2 a, Vector2 b) { return 0; }
        public static Vector2 operator +(Vector2 a, Vector2 b) { return a; } public static Vector2 operator -(Vector2 a, Vector2 b) { return a; }
        public static Vector2 operator -(Vector2 a) { return a; }
        public static Vector2 operator *(Vector2 a, float b) { return a; } public static Vector2 operator /(Vector2 a, float b) { return a; }
        public static bool operator ==(Vector2 a, Vector2 b) { return true; } public static bool operator !=(Vector2 a, Vector2 b) { return false; }
        public override bool Equals(object o) { return false; } public override int GetHashCode() { return 0; }
        public static implicit operator Vector2(Vector3 v) { return default(Vector2); } public static implicit operator Vector3(Vector2 v) { return default(Vector3); }
    }
    public struct Vector2Int { public int x, y; public Vector2Int(int x, int y) { this.x = x; this.y = y; } }
    public struct Vector3
    {
        public float x, y, z; public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero { get { return default(Vector3); } } public static Vector3 one { get { return default(Vector3); } }
        public static Vector3 up { get { return default(Vector3); } } public static Vector3 down { get { return default(Vector3); } }
        public static Vector3 left { get { return default(Vector3); } } public static Vector3 right { get { return default(Vector3); } }
        public static Vector3 forward { get { return default(Vector3); } }
        public float magnitude { get { return 0; } } public float sqrMagnitude { get { return 0; } } public Vector3 normalized { get { return this; } }
        public void Normalize() { }
        public static float Distance(Vector3 a, Vector3 b) { return 0; } public static float Dot(Vector3 a, Vector3 b) { return 0; }
        public static float Angle(Vector3 a, Vector3 b) { return 0; }
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) { return a; }
        public static Vector3 operator +(Vector3 a, Vector3 b) { return a; } public static Vector3 operator -(Vector3 a, Vector3 b) { return a; }
        public static Vector3 operator -(Vector3 a) { return a; }
        public static Vector3 operator *(Vector3 a, float b) { return a; } public static Vector3 operator *(float b, Vector3 a) { return a; } public static Vector3 operator /(Vector3 a, float b) { return a; }
        public static bool operator ==(Vector3 a, Vector3 b) { return true; } public static bool operator !=(Vector3 a, Vector3 b) { return false; }
        public override bool Equals(object o) { return false; } public override int GetHashCode() { return 0; }
    }
    public struct Vector4 { public Vector4(float x, float y, float z, float w) { } }
    public struct Quaternion
    {
        public static Quaternion identity { get { return default(Quaternion); } }
        public static Quaternion Euler(float x, float y, float z) { return default(Quaternion); }
        public static Quaternion Euler(Vector3 e) { return default(Quaternion); }
        public static Quaternion LookRotation(Vector3 f) { return default(Quaternion); }
        public static Quaternion Slerp(Quaternion a, Quaternion b, float t) { return a; }
        public static Quaternion operator *(Quaternion a, Quaternion b) { return a; }
        public static Vector3 operator *(Quaternion a, Vector3 b) { return b; }
    }
    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b) { this.r = r; this.g = g; this.b = b; a = 1; } public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static Color white { get { return default(Color); } } public static Color black { get { return default(Color); } } public static Color red { get { return default(Color); } } public static Color green { get { return default(Color); } }
        public static Color yellow { get { return default(Color); } }
        public static Color Lerp(Color a, Color b, float t) { return a; }
        public static Color operator *(Color a, float b) { return a; }
    }
    public struct Color32 { public Color32(byte r, byte g, byte b, byte a) { } }
    public struct Rect { public Rect(float x, float y, float w, float h) { } }
    public struct LayerMask { public static implicit operator int(LayerMask m) { return 0; } public static implicit operator LayerMask(int i) { return default(LayerMask); } }
    public static class Mathf
    {
        public const float PI = 3.14159f;
        public static float Sin(float f) { return 0; } public static float Cos(float f) { return 0; } public static float Abs(float f) { return 0; }
        public static float Clamp(float v, float a, float b) { return v; } public static int Clamp(int v, int a, int b) { return v; } public static float Clamp01(float v) { return v; }
        public static float Lerp(float a, float b, float t) { return a; } public static float MoveTowards(float a, float b, float d) { return a; }
        public static float SmoothStep(float a, float b, float t) { return a; } public static float Repeat(float t, float l) { return t; }
        public static float Max(float a, float b) { return a; } public static int Max(int a, int b) { return a; } public static float Min(float a, float b) { return a; } public static int Min(int a, int b) { return a; }
        public static float Pow(float a, float b) { return a; } public static float PerlinNoise(float x, float y) { return 0; }
        public static int CeilToInt(float f) { return 0; } public static int RoundToInt(float f) { return 0; }
    }
    public static class Random
    {
        public static float value { get { return 0; } }
        public static float Range(float a, float b) { return a; } public static int Range(int a, int b) { return a; }
        public static Vector3 insideUnitSphere { get { return default(Vector3); } } public static Vector2 insideUnitCircle { get { return default(Vector2); } }
    }
    public static class Time
    {
        public static float deltaTime { get { return 0; } } public static float time { get { return 0; } } public static float unscaledTime { get { return 0; } } public static float unscaledDeltaTime { get { return 0; } }
        public static float timeScale { get; set; } public static int frameCount { get { return 0; } }
    }
    public static class Debug { public static void Log(object o) { } public static void LogWarning(object o) { } public static void LogError(object o) { } }
    public static class Application { public static int targetFrameRate { get; set; } public static bool isMobilePlatform { get { return false; } } public static void Quit() { } }
    public static class Screen { public static int height { get { return 0; } } public static int width { get { return 0; } } }
    public static class SystemInfo { public static int systemMemorySize { get { return 0; } } }
    public static class PlayerPrefs
    {
        public static int GetInt(string k, int d) { return d; } public static void SetInt(string k, int v) { }
        public static float GetFloat(string k, float d) { return d; } public static void SetFloat(string k, float v) { } public static void Save() { }
    }
    public static class QualitySettings { public static int vSyncCount { get; set; } }
    public enum CursorLockMode { None, Locked, Confined }
    public static class Cursor { public static CursorLockMode lockState { get; set; } public static bool visible { get; set; } }
    public enum KeyCode { E, C, F, R, Tab, Escape, LeftShift }
    public static class Input
    {
        public static float GetAxisRaw(string a) { return 0; } public static float GetAxis(string a) { return 0; }
        public static bool GetKey(KeyCode k) { return false; } public static bool GetKeyDown(KeyCode k) { return false; }
    }
    public static class Resources { public static T GetBuiltinResource<T>(string p) where T : Object { return null; } }

    // ---- physics
    public enum QueryTriggerInteraction { UseGlobal, Ignore, Collide }
    public struct RaycastHit { public Collider collider { get { return null; } } public Vector3 point { get { return default(Vector3); } } }
    public static class Physics
    {
        public static bool Raycast(Vector3 o, Vector3 d, out RaycastHit h, float m, int mask, QueryTriggerInteraction q) { h = default(RaycastHit); return false; }
        public static bool Raycast(Vector3 o, Vector3 d, float m, int mask, QueryTriggerInteraction q) { return false; }
        public static bool SphereCast(Vector3 o, float r, Vector3 d, out RaycastHit h, float m, int mask, QueryTriggerInteraction q) { h = default(RaycastHit); return false; }
        public static int OverlapSphereNonAlloc(Vector3 p, float r, Collider[] res, int mask, QueryTriggerInteraction q) { return 0; }
    }
    public class Collider : Component { public bool enabled { get; set; } public bool isTrigger { get; set; } }
    public class BoxCollider : Collider { public Vector3 center { get; set; } public Vector3 size { get; set; } }
    public class SphereCollider : Collider { public float radius { get; set; } }
    public class CapsuleCollider : Collider { public float radius { get; set; } public float height { get; set; } public Vector3 center { get; set; } }
    public class CharacterController : Collider
    {
        public bool isGrounded { get { return false; } } public Vector3 velocity { get { return default(Vector3); } }
        public float height { get; set; } public float radius { get; set; } public Vector3 center { get; set; } public float stepOffset { get; set; } public float slopeLimit { get; set; }
        public CollisionFlags Move(Vector3 m) { return 0; }
    }
    public enum CollisionFlags { None }
    public enum ForceMode { Force, Impulse }
    public class Rigidbody : Component
    {
        public float mass { get; set; } public bool isKinematic { get; set; }
        public void AddForce(Vector3 f, ForceMode m) { } public void AddTorque(Vector3 f, ForceMode m) { }
    }

    // ---- rendering
    public class Camera : Behaviour
    {
        public float nearClipPlane { get; set; } public float farClipPlane { get; set; } public float fieldOfView { get; set; }
        public CameraClearFlags clearFlags { get; set; } public Color backgroundColor { get; set; }
    }
    public enum CameraClearFlags { Skybox, SolidColor }
    public enum LightType { Spot, Directional, Point }
    public enum LightShadows { None, Hard, Soft }
    public class Light : Behaviour
    {
        public LightType type { get; set; } public Color color { get; set; } public float intensity { get; set; } public float range { get; set; }
        public float spotAngle { get; set; } public float innerSpotAngle { get; set; } public LightShadows shadows { get; set; }
    }
    public class Renderer : Component { public Material material { get; set; } public Material sharedMaterial { get; set; } }
    public class MeshRenderer : Renderer { }
    public class MeshFilter : Component { }
    public class Shader : Object { public static Shader Find(string n) { return null; } }
    [Flags] public enum MaterialGlobalIlluminationFlags { None = 0 }
    public class Material : Object
    {
        public Material(Shader s) { }
        public void SetColor(string n, Color c) { } public void SetFloat(string n, float v) { } public void SetTexture(string n, Texture t) { }
        public void SetTextureScale(string n, Vector2 s) { } public void EnableKeyword(string k) { } public void SetOverrideTag(string t, string v) { }
        public int renderQueue { get; set; } public bool enableInstancing { get; set; } public MaterialGlobalIlluminationFlags globalIlluminationFlags { get; set; }
    }
    public class Texture : Object { public TextureWrapMode wrapMode { get; set; } }
    public enum TextureWrapMode { Repeat, Clamp }
    public enum TextureFormat { RGBA32 }
    public class Texture2D : Texture
    {
        public Texture2D(int w, int h) { } public Texture2D(int w, int h, TextureFormat f, bool mip) { }
        public void SetPixel(int x, int y, Color c) { } public Color GetPixel(int x, int y) { return default(Color); }
        public void SetPixels32(Color32[] c) { } public void Apply() { }
    }
    public static class ImageConversion { public static byte[] EncodeToPNG(this Texture2D t) { return null; } }
    public enum SpriteMeshType { FullRect, Tight }
    public class Sprite : Object
    {
        public static Sprite Create(Texture2D t, Rect r, Vector2 pivot, float ppu) { return null; }
        public static Sprite Create(Texture2D t, Rect r, Vector2 pivot, float ppu, uint extrude, SpriteMeshType mt, Vector4 border) { return null; }
    }
    public class Font : Object { }
    public enum TextAnchor { UpperLeft, UpperCenter, MiddleLeft, MiddleCenter, LowerLeft, LowerCenter }
    public enum FontStyle { Normal, Bold }
    public enum HorizontalWrapMode { Wrap, Overflow }
    public enum VerticalWrapMode { Truncate, Overflow }
    public enum RenderMode { ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace }
    public class Canvas : Behaviour { public RenderMode renderMode { get; set; } public int sortingOrder { get; set; } }
    public class CanvasGroup : Behaviour { public float alpha { get; set; } }
    public enum FogMode { Linear = 1, Exponential, ExponentialSquared }
    public static class RenderSettings
    {
        public static bool fog { get; set; } public static FogMode fogMode { get; set; } public static Color fogColor { get; set; } public static float fogDensity { get; set; }
        public static Rendering.AmbientMode ambientMode { get; set; } public static Color ambientLight { get; set; } public static Material skybox { get; set; }
    }
    public enum ColorSpace { Gamma, Linear }

    // ---- audio
    public enum AudioRolloffMode { Logarithmic, Linear, Custom }
    public sealed class AudioClip : Object
    {
        public float length { get { return 0; } } public int samples { get { return 0; } }
        public static AudioClip Create(string n, int len, int ch, int freq, bool stream) { return null; }
        public bool SetData(float[] d, int off) { return true; } public bool GetData(float[] d, int off) { return true; }
    }
    public class AudioSource : Behaviour
    {
        public AudioClip clip { get; set; } public float volume { get; set; } public float pitch { get; set; } public bool loop { get; set; }
        public float spatialBlend { get; set; } public AudioRolloffMode rolloffMode { get; set; } public float minDistance { get; set; } public float maxDistance { get; set; }
        public float dopplerLevel { get; set; } public bool playOnAwake { get; set; } public float time { get; set; }
        public void Play() { } public void Stop() { } public void PlayOneShot(AudioClip c, float v) { }
    }
    public class AudioListener : Behaviour { public static float volume { get; set; } }

    // ---- particles
    public class ParticleSystem : Component
    {
        public MainModule main { get { return default(MainModule); } }
        public EmissionModule emission { get { return default(EmissionModule); } }
        public ShapeModule shape { get { return default(ShapeModule); } }
        public struct MinMaxCurve { public static implicit operator MinMaxCurve(float f) { return default(MinMaxCurve); } }
        public struct MinMaxGradient { public static implicit operator MinMaxGradient(Color c) { return default(MinMaxGradient); } }
        public struct MainModule
        {
            public MinMaxCurve startLifetime { get; set; } public MinMaxCurve startSpeed { get; set; } public MinMaxCurve startSize { get; set; }
            public int maxParticles { get; set; } public ParticleSystemSimulationSpace simulationSpace { get; set; } public MinMaxGradient startColor { get; set; }
        }
        public struct EmissionModule { public MinMaxCurve rateOverTime { get; set; } }
        public struct ShapeModule { public ParticleSystemShapeType shapeType { get; set; } public Vector3 scale { get; set; } public Vector3 rotation { get; set; } }
    }
    public enum ParticleSystemSimulationSpace { Local, World }
    public enum ParticleSystemShapeType { Box }
    public enum ParticleSystemRenderMode { Billboard, Stretch }
    public class ParticleSystemRenderer : Renderer { public ParticleSystemRenderMode renderMode { get; set; } public float lengthScale { get; set; } public float velocityScale { get; set; } }

    public class HeaderAttribute : Attribute { public HeaderAttribute(string h) { } }
    public class TooltipAttribute : Attribute { public TooltipAttribute(string h) { } }
    public class RangeAttribute : Attribute { public RangeAttribute(float a, float b) { } }
    public class TextAreaAttribute : Attribute { public TextAreaAttribute(int a, int b) { } }
    public class RequireComponent : Attribute { public RequireComponent(Type t) { } }
    public static class Gizmos { public static Color color { get; set; } public static void DrawWireSphere(Vector3 c, float r) { } }
}

namespace UnityEngine.SceneManagement
{
    public struct Scene { public int buildIndex { get { return 0; } } }
    public static class SceneManager { public static Scene GetActiveScene() { return default(Scene); } public static void LoadScene(int i) { } }
}

namespace UnityEngine.AI
{
    public class NavMeshAgent : Behaviour
    {
        public float speed { get; set; } public float radius { get; set; } public float height { get; set; } public float angularSpeed { get; set; } public float acceleration { get; set; }
        public float stoppingDistance { get; set; } public bool autoBraking { get; set; } public bool isStopped { get; set; } public bool isOnNavMesh { get { return true; } }
        public bool pathPending { get { return false; } } public float remainingDistance { get { return 0; } } public Vector3 velocity { get { return default(Vector3); } }
        public ObstacleAvoidanceType obstacleAvoidanceType { get; set; }
        public bool SetDestination(Vector3 t) { return true; }
    }
    public enum ObstacleAvoidanceType { NoObstacleAvoidance, LowQualityObstacleAvoidance }
    public struct NavMeshHit { public Vector3 position { get { return default(Vector3); } } }
    public static class NavMesh { public const int AllAreas = -1; public static bool SamplePosition(Vector3 p, out NavMeshHit h, float d, int mask) { h = default(NavMeshHit); return true; } }
}

namespace UnityEngine.EventSystems
{
    public class UIBehaviour : MonoBehaviour { }
    public class EventSystem : UIBehaviour { }
    public abstract class BaseInputModule : UIBehaviour { }
    public class StandaloneInputModule : BaseInputModule { }
    public class PointerEventData
    {
        public int pointerId { get; set; } public Vector2 position { get; set; } public Vector2 delta { get; set; }
        public Camera pressEventCamera { get { return null; } } public GameObject pointerPress { get; set; }
    }
    public interface IEventSystemHandler { }
    public interface IPointerDownHandler : IEventSystemHandler { void OnPointerDown(PointerEventData e); }
    public interface IPointerUpHandler : IEventSystemHandler { void OnPointerUp(PointerEventData e); }
    public interface IPointerExitHandler : IEventSystemHandler { void OnPointerExit(PointerEventData e); }
    public interface IDragHandler : IEventSystemHandler { void OnDrag(PointerEventData e); }
}

namespace UnityEngine.UI
{
    using UnityEngine.EventSystems;
    using UnityEngine.Events;
    public abstract class Graphic : UIBehaviour { public Color color { get; set; } public bool raycastTarget { get; set; } public RectTransform rectTransform { get { return null; } } }
    public abstract class MaskableGraphic : Graphic { }
    public class Image : MaskableGraphic
    {
        public enum Type { Simple, Sliced, Tiled, Filled }
        public enum FillMethod { Horizontal, Vertical, Radial90, Radial180, Radial360 }
        public Sprite sprite { get; set; } public Type type { get; set; } public FillMethod fillMethod { get; set; } public float fillAmount { get; set; }
    }
    public class Text : MaskableGraphic
    {
        public Font font { get; set; } public string text { get; set; } public int fontSize { get; set; } public TextAnchor alignment { get; set; }
        public FontStyle fontStyle { get; set; } public HorizontalWrapMode horizontalOverflow { get; set; } public VerticalWrapMode verticalOverflow { get; set; }
    }
    public struct ColorBlock { public Color highlightedColor { get; set; } public Color pressedColor { get; set; } }
    public class Selectable : UIBehaviour { public ColorBlock colors { get; set; } public Graphic targetGraphic { get; set; } }
    public class Button : Selectable { public class ButtonClickedEvent : UnityEvent { } public ButtonClickedEvent onClick { get; set; } }
    public class Slider : Selectable
    {
        public class SliderEvent : UnityEvent<float> { }
        public RectTransform fillRect { get; set; } public RectTransform handleRect { get; set; }
        public float minValue { get; set; } public float maxValue { get; set; } public float value { get; set; } public SliderEvent onValueChanged { get; set; }
    }
    public class CanvasScaler : UIBehaviour
    {
        public enum ScaleMode { ConstantPixelSize, ScaleWithScreenSize }
        public ScaleMode uiScaleMode { get; set; } public Vector2 referenceResolution { get; set; } public float matchWidthOrHeight { get; set; }
    }
    public class GraphicRaycaster : UIBehaviour { }
    public abstract class HorizontalOrVerticalLayoutGroup : UIBehaviour
    {
        public float spacing { get; set; } public bool childControlWidth { get; set; } public bool childControlHeight { get; set; }
        public bool childForceExpandWidth { get; set; } public bool childForceExpandHeight { get; set; } public TextAnchor childAlignment { get; set; }
    }
    public class VerticalLayoutGroup : HorizontalOrVerticalLayoutGroup { }
    public class ContentSizeFitter : UIBehaviour { public enum FitMode { Unconstrained, MinSize, PreferredSize } public FitMode verticalFit { get; set; } }
}

namespace UnityEngine.Events
{
    public delegate void UnityAction(); public delegate void UnityAction<T0>(T0 a);
    public class UnityEvent { public void AddListener(UnityAction a) { } }
    public class UnityEvent<T0> { public void AddListener(UnityAction<T0> a) { } }
}

namespace UnityEngine.Rendering
{
    public enum AmbientMode { Skybox, Trilight, Flat }
    public enum BlendMode { Zero, One, SrcAlpha = 5, OneMinusSrcAlpha = 10 }
    public enum RenderQueue { Transparent = 3000 }
    public class RenderPipelineAsset : ScriptableObject { }
    public static class GraphicsSettings { public static RenderPipelineAsset currentRenderPipeline { get { return null; } } public static RenderPipelineAsset defaultRenderPipeline { get; set; } }
    public class VolumeParameter<T> { public T value { get; set; } public void Override(T x) { } }
    public class ClampedFloatParameter : VolumeParameter<float> { }
    public class FloatParameter : VolumeParameter<float> { }
    public class MinFloatParameter : VolumeParameter<float> { }
    public class ColorParameter : VolumeParameter<Color> { }
    public class VolumeComponent : ScriptableObject { }
    public class VolumeProfile : ScriptableObject
    {
        public List<VolumeComponent> components;
        public T Add<T>(bool overrides = false) where T : VolumeComponent { return null; }
        public bool TryGet<T>(out T c) where T : VolumeComponent { c = null; return false; }
    }
    public class Volume : MonoBehaviour { public bool isGlobal { get; set; } public float priority { get; set; } public VolumeProfile sharedProfile; public VolumeProfile profile { get; set; } }
}

namespace UnityEngine.Rendering.Universal
{
    public class UniversalRenderPipelineAsset : RenderPipelineAsset { public float renderScale { get; set; } public int msaaSampleCount { get; set; } public float shadowDistance { get; set; } }
    public class UniversalAdditionalCameraData : MonoBehaviour { public bool renderPostProcessing { get; set; } }
    public static class CameraExtensions { public static UniversalAdditionalCameraData GetUniversalAdditionalCameraData(this Camera c) { return null; } }
    public sealed class Vignette : VolumeComponent { public ColorParameter color; public ClampedFloatParameter intensity; public ClampedFloatParameter smoothness; }
    public sealed class ChromaticAberration : VolumeComponent { public ClampedFloatParameter intensity; }
    public sealed class FilmGrain : VolumeComponent { public ClampedFloatParameter intensity; }
    public sealed class LensDistortion : VolumeComponent { public ClampedFloatParameter intensity; }
    public sealed class ColorAdjustments : VolumeComponent { public FloatParameter postExposure; public ClampedFloatParameter contrast; public ColorParameter colorFilter; public ClampedFloatParameter saturation; }
    public enum TonemappingMode { None, Neutral, ACES }
    public sealed class TonemappingModeParameter : VolumeParameter<TonemappingMode> { }
    public sealed class Tonemapping : VolumeComponent { public TonemappingModeParameter mode; }
    public sealed class Bloom : VolumeComponent { public MinFloatParameter threshold; public MinFloatParameter intensity; }
}

namespace UnityEngine.InputSystem
{
    public class ButtonControl { public bool isPressed { get { return false; } } public bool wasPressedThisFrame { get { return false; } } }
    public class KeyControl : ButtonControl { }
    public class Vector2Control { public Vector2 ReadValue() { return default(Vector2); } }
    public class Keyboard
    {
        public static Keyboard current { get { return null; } }
        public KeyControl wKey, aKey, sKey, dKey, eKey, cKey, fKey, rKey, tabKey, escapeKey, leftShiftKey;
    }
    public class Mouse { public static Mouse current { get { return null; } } public Vector2Control delta { get { return null; } } }
}
namespace UnityEngine.InputSystem.UI { public class InputSystemUIInputModule : UnityEngine.EventSystems.BaseInputModule { } }

namespace Unity.AI.Navigation
{
    public enum CollectObjects { All, Volume, Children }
    public class NavMeshSurface : UnityEngine.MonoBehaviour
    {
        public CollectObjects collectObjects { get; set; } public UnityEngine.AI.NavMeshCollectGeometry useGeometry { get; set; }
        public void BuildNavMesh() { } public UnityEngine.AI.NavMeshData navMeshData { get; set; }
    }
}
namespace UnityEngine.AI { public class NavMeshData : UnityEngine.Object { } public enum NavMeshCollectGeometry { RenderMeshes, PhysicsColliders } }

namespace UnityEditor
{
    using UnityEngine;
    public class MenuItem : Attribute { public MenuItem(string p) { } public int priority; }
    public static class EditorUtility
    {
        public static bool DisplayDialog(string t, string m, string ok) { return true; }
        public static void DisplayProgressBar(string t, string i, float p) { } public static void ClearProgressBar() { }
        public static void SetDirty(Object o) { }
    }
    public static class AssetDatabase
    {
        public static bool IsValidFolder(string p) { return true; } public static string CreateFolder(string a, string b) { return ""; }
        public static bool DeleteAsset(string p) { return true; } public static void CreateAsset(Object o, string p) { } public static void AddObjectToAsset(Object o, Object a) { }
        public static void ImportAsset(string p) { } public static T LoadAssetAtPath<T>(string p) where T : Object { return null; } public static void SaveAssets() { }
    }
    public class AssetImporter : Object { public static AssetImporter GetAtPath(string p) { return null; } public void SaveAndReimport() { } }
    public class TextureImporter : AssetImporter { public int maxTextureSize { get; set; } public bool mipmapEnabled { get; set; } }
    [Flags] public enum StaticEditorFlags { BatchingStatic = 4, NavigationStatic = 8 }
    public static class GameObjectUtility { public static void SetStaticEditorFlags(GameObject g, StaticEditorFlags f) { } }
    public class EditorBuildSettingsScene { public EditorBuildSettingsScene(string p, bool e) { } public string path { get { return ""; } } }
    public static class EditorBuildSettings { public static EditorBuildSettingsScene[] scenes { get; set; } }
    public enum ScriptingImplementation { Mono2x, IL2CPP }
    public enum UIOrientation { Portrait, AutoRotation, LandscapeLeft }
    [Flags] public enum AndroidArchitecture { ARMv7 = 1, ARM64 = 2 }
    public enum AndroidSdkVersions { AndroidApiLevel26 = 26 }
    public static class PlayerSettings
    {
        public static string companyName { get; set; } public static string productName { get; set; }
        public static void SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget t, string id) { }
        public static void SetScriptingBackend(UnityEditor.Build.NamedBuildTarget t, ScriptingImplementation s) { }
        public static UIOrientation defaultInterfaceOrientation { get; set; }
        public static bool allowedAutorotateToPortrait { get; set; } public static bool allowedAutorotateToPortraitUpsideDown { get; set; }
        public static bool allowedAutorotateToLandscapeLeft { get; set; } public static bool allowedAutorotateToLandscapeRight { get; set; }
        public static ColorSpace colorSpace { get; set; }
        public static class Android
        {
            public static AndroidArchitecture targetArchitectures { get; set; } public static AndroidSdkVersions minSdkVersion { get; set; }
            public static bool startInFullscreen { get; set; } public static bool renderOutsideSafeArea { get; set; }
        }
    }
}
namespace UnityEditor.Build { public struct NamedBuildTarget { public static readonly NamedBuildTarget Android; } }
namespace UnityEditor.SceneManagement
{
    public enum NewSceneSetup { EmptyScene, DefaultGameObjects }
    public enum NewSceneMode { Single, Additive }
    public static class EditorSceneManager
    {
        public static UnityEngine.SceneManagement.Scene NewScene(NewSceneSetup s, NewSceneMode m) { return default(UnityEngine.SceneManagement.Scene); }
        public static bool SaveScene(UnityEngine.SceneManagement.Scene s, string p) { return true; }
        public static bool SaveCurrentModifiedScenesIfUserWantsTo() { return true; }
    }
}
