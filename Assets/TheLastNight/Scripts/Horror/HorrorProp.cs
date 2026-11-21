// HorrorProp: a physics object the HorrorDirector can knock over / move ("moving objects").
using UnityEngine;

namespace TheLastNight
{
    [RequireComponent(typeof(Rigidbody))]
    public class HorrorProp : MonoBehaviour
    {
        public static readonly System.Collections.Generic.List<HorrorProp> All = new System.Collections.Generic.List<HorrorProp>();
        public float pushForce = 3.5f;
        public bool IsRadio;               // radios play static instead of falling
        Rigidbody rb;

        void Awake() { rb = GetComponent<Rigidbody>(); }
        void OnEnable() { All.Add(this); }
        void OnDisable() { All.Remove(this); }

        public void Disturb()
        {
            var am = AudioManager.Instance;
            if (IsRadio)
            {
                if (am != null) am.PlayAt(ProceduralAudio.RadioStatic, transform.position, 0.9f, Random.Range(0.8f, 1.2f), 18f);
                return;
            }
            Vector3 dir = Random.insideUnitSphere; dir.y = Mathf.Abs(dir.y) * 0.3f + 0.2f;
            rb.AddForce(dir.normalized * pushForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * pushForce, ForceMode.Impulse);
            if (am != null) am.PlayAt(ProceduralAudio.ObjectFall, transform.position, 0.9f, Random.Range(0.8f, 1.2f), 22f);
            NoiseSystem.Emit(transform.position, NoiseSystem.DefaultRadius(NoiseKind.Object), NoiseKind.Object, null);
        }
    }
}
