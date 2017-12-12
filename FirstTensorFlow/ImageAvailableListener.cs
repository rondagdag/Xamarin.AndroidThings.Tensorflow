/*using android.content.Context;
using android.graphics.ImageFormat;
using android.hardware.camera2.CameraAccessException;
using android.hardware.camera2.CameraCaptureSession;
using android.hardware.camera2.CameraCharacteristics;
using android.hardware.camera2.CameraDevice;
using android.hardware.camera2.CameraManager;
using android.hardware.camera2.CaptureRequest;
using android.hardware.camera2.CaptureResult;
using android.hardware.camera2.TotalCaptureResult;
using android.hardware.camera2.params.StreamConfigurationMap;
using android.media.ImageReader;
using android.os.Handler;
using android.support.annotation.NonNull;
using android.util.Log;
using android.util.Size;
using java.util.Collections;
using 
android.content.Context.CAMERA_SERVICE;*/

using System;
using Android.Media;
using static Android.Media.ImageReader;
class ImageAvailableListener : Java.Lang.Object, IOnImageAvailableListener
{

    public Action<ImageReader> OnImageAvailableAction;

    public void OnImageAvailable(ImageReader reader)
    {
        if (OnImageAvailableAction != null)
        {
            OnImageAvailableAction(reader);
        }
    }
}

