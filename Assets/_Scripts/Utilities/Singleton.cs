using UnityEngine;
/// <summary>
/// Generic singleton base class for Unity components.
///
/// Creation priority:
/// 1. Existing scene instance.
/// 2. Registered prefab from AssetReferencesSO.
/// 3. Auto-generated GameObject.
///
/// Example:
/// <code>
/// public class AudioManager : Singleton&lt;AudioManager&gt;
/// {
///     protected override bool IsPersistent => true;
/// }
/// </code>
///
/// To support automatic prefab creation, register the prefab in
/// AssetReferencesSO. If no reference is found, an empty GameObject
/// containing the component will be created automatically.
/// </summary>
/// <typeparam name="T">
/// The component type to be used as a singleton.
/// </typeparam>
[Unity.Scripting.LifecycleManagement.AutoStaticsCleanup]
public abstract partial class Singleton<T> : MonoBehaviour where T : Component {
        [HideInInspector] public bool AutoUnparentOnAwake = true;
        protected virtual bool IsPersistent => false;
        protected static T instance;
        public static bool HasInstance => instance != null;
        public static T TryGetInstance() => HasInstance ? instance : null;
        private static bool applicationIsQuitting;
        private string SingletonType => IsPersistent ? "PersistentSingleton" : "Singleton";
        public static T Instance {
            get {
                if (applicationIsQuitting) return null;
                if (instance == null) {
                    instance = FindAnyObjectByType<T>();
                    if (instance == null) {
                        var prefabReference = AssetReferencesSO.Instance.GetReference<T>();
                        if (prefabReference != null) {
                            instance = Instantiate(prefabReference);
                            instance.name = $"{typeof(T).Name} Reference-Generated";
                        }
                        if (instance == null) {
                            GameObject go = new GameObject(typeof(T).Name + " Auto-Generated");
                            instance = go.AddComponent<T>();

                            Debug.LogWarning($"No prefab reference found for {typeof(T).Name}. An auto-generated instance has been created.");
                        }
                    }
                }
                return instance;
            }
        }

        protected virtual void Awake() => InitializeSingleton();
        protected virtual void OnApplicationQuit() => applicationIsQuitting = true;

        protected virtual void InitializeSingleton() {
            if (!Application.isPlaying) return;
            if (this.AutoUnparentOnAwake) transform.SetParent(null);

            if (instance == null) {
                instance = this as T;
                if (this.IsPersistent) DontDestroyOnLoad(gameObject);
                instance.name += $" {this.SingletonType}";

            } else if (instance != this) {
                Destroy(gameObject);
            }
        }
    }
