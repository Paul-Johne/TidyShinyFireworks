using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

/// <summary>
/// Downloads the AssetBundle after acquiring the GoogleDrive ID from a QR code.
/// </summary>
public class Downloader : MonoBehaviour {

    [SerializeField]
    private GameObject buttonStartDownload;
    [SerializeField]
    private GameObject buttonToAR;
    [SerializeField]
    private Text textDebug;

    private string rawDownloadLink = "https://drive.google.com/uc?export=download&id=";

    /* Accessible global variables */
    public static string storagePath;
    public static string filePath;

    private void Start() {
        storagePath = Path.Combine(Application.persistentDataPath, "3DModels");

        if (!Directory.Exists(storagePath)) {
            Directory.CreateDirectory(storagePath);
            textDebug.text = "Directory for fireworks was created";
        } else {
            textDebug.text = "Directory for fireworks exists on your device";
        }
    }

    public async void OnClickDownload() {
        string scannedID = textDebug.text;

        if (!File.Exists(Path.Combine(storagePath, scannedID))) {
            buttonStartDownload.SetActive(false);
            await DownloadAssetBundle(scannedID);
        } else {
            filePath = Path.Combine(storagePath, scannedID);
            textDebug.text = "The scanned Firework already exists on your device";
            buttonStartDownload.SetActive(false);
        }

        buttonToAR.SetActive(true);
    }

    private async Task DownloadAssetBundle(string currentID) {
        var downloadLink = $"{rawDownloadLink}{currentID}";
        var taskInspector = new Task<UnityWebRequest>[1];

        UnityWebRequest request = UnityWebRequest.Get(downloadLink);

        taskInspector[0] = StartDownload(request, currentID);
        await Task.WhenAll(taskInspector);

        if (taskInspector[0].Result.result != UnityWebRequest.Result.Success)
            textDebug.text = $"Download failed: {request.error}";
        else
            textDebug.text = "Finished download";
    }

    private async Task<UnityWebRequest> StartDownload(UnityWebRequest request, string fileName) {
        filePath = Path.Combine(storagePath, fileName);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        var operation = request.SendWebRequest();
        while (!operation.isDone)
            await Task.Yield();

        if (request.result == UnityWebRequest.Result.Success) {
            File.WriteAllBytes(filePath, request.downloadHandler.data);
        }
        return request;
    }
}