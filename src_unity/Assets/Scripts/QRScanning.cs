using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using ZXing;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class QRScanning : MonoBehaviour {

    [SerializeField]
    private RawImage camImage;
    [SerializeField]
    private AspectRatioFitter aspectRatioFitter;
    [SerializeField]
    private RectTransform scanArea;
    [SerializeField]
    private Text textDebug;
    
    [SerializeField]
    private GameObject buttonStartScan;
    [SerializeField]
    private GameObject buttonStopScan;
    [SerializeField]
    private GameObject buttonStartDownload;
    [SerializeField]
    private GameObject buttonToAR;

    private bool _isCamAvailable;
    private WebCamTexture _camTexture;

    private AndroidCameraLIB camFunctions;
    private bool torchActive;

    Thread threadOfScan;
    IBarcodeReader qrReader;

    private void Start() {
        /* Code for Android Version */
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            Permission.RequestUserPermission(Permission.Camera);

        camFunctions = new AndroidCameraLIB();
#endif
        qrReader = new BarcodeReader();
    }

    private void OnDestroy() {
        if (torchActive == true) {
            camFunctions.deactivateTorch();
            torchActive = false;
        }
        if (threadOfScan.IsAlive)
            threadOfScan.Abort();
    }

    private void Update() {
        UpdateCameraRender();
    }

    private void UpdateCameraRender() {
        if (!_isCamAvailable) {
            if (Permission.HasUserAuthorizedPermission(Permission.Camera))
                SetCamera();
            return;
        }

        /* Updates video if flipped */
        // Set aspectRatio
        float ratio = (float)_camTexture.width / (float)_camTexture.height;
        aspectRatioFitter.aspectRatio = ratio;
        // Rotation of raw image
        int orientation = -_camTexture.videoRotationAngle;
        // Z-Roll
        camImage.rectTransform.localEulerAngles = new Vector3(0, 0, orientation);
    }

    private void SetCamera() {
        // Trying to access cameras
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0) {
            _isCamAvailable = false;
            return;
        }

        for (int i = 0; i < devices.Length; i++) {
            if (!devices[i].isFrontFacing) {
                _camTexture = new WebCamTexture(devices[i].name,
                                                (int)scanArea.rect.width, 
                                                (int)scanArea.rect.height);
                break;
            }
        }

        // Starting back camera
        if (_camTexture != null) {
            _isCamAvailable = true;
            _camTexture.Play();
            camImage.texture = _camTexture;
            camImage.color = new Color(255, 255, 255, 255);
        }
    }

    public void OnClickScan() {
        threadOfScan = new Thread(Scan);
        threadOfScan.Start();
    }

    public void OnClickStopScan() {
        threadOfScan.Abort();
        textDebug.text = "Stopped scanning";
        buttonStopScan.SetActive(false);
        buttonStartScan.SetActive(true);
    }

    private void Scan() {
        Result qrContent;
        textDebug.text = "Scanning..";
        buttonStartScan.SetActive(false);
        buttonStopScan.SetActive(true);
        buttonStartDownload.SetActive(false);

        do {
            qrContent = qrReader.Decode(_camTexture.GetPixels32(), _camTexture.width, _camTexture.height);
            if (qrContent != null)
                textDebug.text = qrContent.Text;
        } while (qrContent == null);

        buttonStopScan.SetActive(false);
        buttonStartScan.SetActive(true);
        buttonStartDownload.SetActive(true);

        if (File.Exists(Path.Combine(Application.persistentDataPath, textDebug.text))) {
            Downloader.filePath = Path.Combine(Downloader.storagePath, textDebug.text);
            buttonToAR.SetActive(true);
        }
    }

    /* Turns on the Torch of the mobile device if available */
    public void OnClickTorch() {
#if PLATFORM_ANDROID
        if (!torchActive) {
            camFunctions.activateTorch();
            torchActive = true;
        } else {
            camFunctions.deactivateTorch();
            torchActive = false;
        }
#endif
    }
}