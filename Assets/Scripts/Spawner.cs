using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [SerializeField] private string[] addressableKeys = null;
    [SerializeField] private bool verboseLogging;
    
    private AsyncOperationHandle<IResourceLocator> _opHandle;
    private GameObject _spawnedGameObj = null;

    /// <summary> Called once when component is created/initialized </summary>
    private void Awake()
    {
        _opHandle = Addressables.InitializeAsync();
        StartCoroutine(WaitAndLoadPrefab());
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator WaitAndLoadPrefab()
    {
        yield return _opHandle;

        // Log all assets found in the addressables resources
        if (verboseLogging)
        {
            foreach (var locator in Addressables.ResourceLocators)
            {
                Debug.Log($"Listing all keys in ResourceLocator '{locator}'...");
                foreach (var key in locator.Keys)
                {
                    Debug.Log(key);
                }
            }
        }
        
        Debug.Assert(addressableKeys?.Length > 0, "Addressable Keys not populated in Inspector");

        // Filter down to prefabs
        var prefabKeys = addressableKeys.Where(key => key.EndsWith(".prefab")).ToList();

        // Choose asset to load
        Debug.Assert(prefabKeys.Count > 0, "Addressable Keys contain no prefab, nothing to load");
        var prefabToLoad = prefabKeys[Random.Range(0, addressableKeys.Length)]; 
        Debug.Log($"My favourite prefab is '{prefabToLoad}'");
        var loadHandler = Addressables.LoadAssetAsync<GameObject>(prefabToLoad);
        yield return loadHandler;

        // Load asset
        var loadHandle = Addressables.LoadAssetAsync<GameObject>(prefabToLoad);
        yield return loadHandle;
        
        // Spawn object
        _spawnedGameObj = Instantiate(loadHandle.Result);
    }

    private void OnDestroy()
    {
        if (_spawnedGameObj != null) // does Unity lifetime check
        {
            Destroy(_spawnedGameObj);
        }
        
        // TODO: Uncomment this if "autoReleaseHandle:false" used in Addressables.Initialize()
        //Addressables.Release(_opHandle);
    }
}
