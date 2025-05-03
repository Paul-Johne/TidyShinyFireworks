using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARLogic : MonoBehaviour {

    /* Debugging Textview */
    [SerializeField]
    private Text textdebug;

    /* Button References to enable/disable them */
    [SerializeField]
    private GameObject buttonPlaceFirework;
    [SerializeField]
    private GameObject buttonIgniteFirework;

    /* TrackedImage Variables */
    [SerializeField]
    private ARSessionOrigin sessionOrigin;
    [SerializeField]
    private XRReferenceImageLibrary xrRefImgLib;
    public GameObject trackedImagePrefab;
    private ARTrackedImageManager trackedImageManager;

    private string currentFilePath;

    private AndroidCameraLIB camFunction;

    private void Start() {
        currentFilePath = Downloader.filePath;
        textdebug.text = $"current filepath: {currentFilePath}";
        camFunction = new AndroidCameraLIB();
    }

    public void OnPlaceObject() {
        LoadPrefabFromAsset();

        trackedImageManager = sessionOrigin.gameObject.AddComponent<ARTrackedImageManager>();
        trackedImageManager.referenceLibrary = trackedImageManager.CreateRuntimeLibrary(xrRefImgLib);
        trackedImageManager.requestedMaxNumberOfMovingImages = 1;
        trackedImageManager.trackedImagePrefab = trackedImagePrefab;
        trackedImageManager.enabled = true;

        buttonPlaceFirework.SetActive(false);
        buttonIgniteFirework.SetActive(true);
    }

    private void LoadPrefabFromAsset() {
        AssetBundle loadedAssetBundle = AssetBundle.LoadFromFile(currentFilePath);

        if (loadedAssetBundle == null) {
            textdebug.text = "Failed to load AssetBundle!";
            return;
        }

        textdebug.text = "AssetBundle was loaded";

        trackedImagePrefab = loadedAssetBundle.LoadAsset<GameObject>(loadedAssetBundle.GetAllAssetNames().GetValue(0) as string);
        loadedAssetBundle.Unload(false);

        textdebug.text = $"Firework placed on QR Code: {trackedImagePrefab.transform.hierarchyCount}";
    }

    public void OnIgniteObject() {
        foreach (var i in trackedImageManager.trackables) {
            i.gameObject.transform.GetChild(1).GetComponent<ParticleSystem>().Play();
        }
        textdebug.text = "BOOM!";

        buttonIgniteFirework.SetActive(false);
    }
}