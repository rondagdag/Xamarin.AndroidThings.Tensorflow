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
using Android.Hardware.Camera2;
//android.content.Context.CAMERA_SERVICE;


public class CameraCaptureStateListener : CameraCaptureSession.StateCallback
{
    /// <summary>
    /// The on configure failed action.
    /// </summary>
    public Action<CameraCaptureSession> OnConfigureFailedAction;

    /// <summary>
    /// The on configured action.
    /// </summary>
    public Action<CameraCaptureSession> OnConfiguredAction;

    /// <summary>
    /// Ons the configure failed.
    /// </summary>
    /// <param name="session">Session.</param>
    public override void OnConfigureFailed(CameraCaptureSession session)
    {
        if (OnConfigureFailedAction != null)
        {
            OnConfigureFailedAction(session);
        }
    }

    /// <summary>
    /// Ons the configured.
    /// </summary>
    /// <param name="session">Session.</param>
    public override void OnConfigured(CameraCaptureSession session)
    {
        if (OnConfiguredAction != null)
        {
            OnConfiguredAction(session);
        }
    }
}

