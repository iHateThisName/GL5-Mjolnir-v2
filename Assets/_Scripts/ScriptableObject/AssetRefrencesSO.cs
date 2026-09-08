using System.Collections.Generic;
using System.Linq;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

/// <summary>
/// Global asset database used to provide access to shared project assets.
///
/// This ScriptableObject is loaded from the Resources folder and can be
/// accessed through <see cref="Instance"/> from anywhere in the codebase.
///
/// Typical use cases:
/// - Singleton managers that need access to their prefab.
/// - Shared prefabs used by multiple systems.
/// - Common ScriptableObjects, AudioClips, VFX, etc.
///
/// This asset must be stored at:
/// Assets/Resources/AssetReferencesSO.asset
///
/// Example:
/// AudioManager.Instance may instantiate its prefab using:
///
/// Instantiate(
///     AssetReferencesSO.Instance.audioManagerPrefab);
///
/// This ensures the instantiated object keeps all Inspector-assigned
/// references from the original prefab.
/// </summary>
[CreateAssetMenu(fileName = "AssetReferencesSO", menuName = "Game/Asset References")]
[AutoStaticsCleanup]
public partial class AssetReferencesSO : ScriptableObject {
    private static AssetReferencesSO instance;
    public static AssetReferencesSO Instance {
        get {
            if (instance == null) {
                instance = Resources.Load<AssetReferencesSO>(nameof(AssetReferencesSO));

                if (instance == null) {
                    Debug.LogError(
                        $"{nameof(AssetReferencesSO)} could not be loaded. " +
                        $"Make sure it exists in a Resources folder.");
                }
            }

            return instance;
        }
    }
    [SerializeField] private List<MonoBehaviour> scriptReferences = new List<MonoBehaviour>();
    public T GetReference<T>() where T : Component => scriptReferences.OfType<T>().FirstOrDefault();
}
