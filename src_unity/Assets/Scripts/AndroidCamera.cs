using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if PLATFORM_ANDROID

using UnityEngine.Android;

/// <summary> 
/// Usable for Android API Level 21+
/// <c> This Code isn't working while Unity uses Camera Features </c>
/// <example> [e.g. AR Scene or WebCamTexture] </example>
/// </summary>
public class AndroidCameraLIB {

    AndroidJavaClass unityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

    AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
    AndroidJavaClass cameraClass = new AndroidJavaClass("android.hardware.Camera");

    AndroidJavaObject toast;
    string backCamId = "none";

    public void makeToast(string message) {
        /* Preparing parameters for toast.makeText */
        object[] parameters = new object[3] {
            unityActivity.GetStatic<AndroidJavaObject>("currentActivity"),
            message,
            toastClass.GetStatic<int>("LENGTH_SHORT")
        };

        /* Creating toast and showing it on current activity */
        toast = toastClass.CallStatic<AndroidJavaObject>("makeText", parameters);
        toast.Call("show");
    }

    public void activateTorch() {
        var context = unityActivity.GetStatic<AndroidJavaObject>("currentActivity");
        var cameraManager = context.Call<AndroidJavaObject>("getSystemService", "camera");

        try {
            var camIDs = cameraManager.Call<string[]>("getCameraIdList");
            backCamId = camIDs[0];
        } catch {
            this.makeToast("Getting Back Camera ID failed");
        }


        if (backCamId != "none") {
            cameraManager.Call("setTorchMode", new object[] { backCamId, true });

            this.makeToast("Flashlight was started");
        } else {
            this.makeToast("Backfacing Camera wasn't detected!");
        }
    }

    public void deactivateTorch() {
        var context = unityActivity.GetStatic<AndroidJavaObject>("currentActivity");
        var cameraManager = context.Call<AndroidJavaObject>("getSystemService", "camera");

        cameraManager.Call("setTorchMode", new object[] { backCamId, false });
        this.makeToast("Flashlight was stopped");
    }
}

#endif