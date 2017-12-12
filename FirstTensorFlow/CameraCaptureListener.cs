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
public class CameraCaptureListener : CameraCaptureSession.CaptureCallback
{
    /// <summary>
    /// Occurs when photo complete.
    /// </summary>
    public Action<CameraCaptureSession> OnCaptureCompletedAction;

    /// <summary>
    /// Ons the capture completed.
    /// </summary>
    /// <param name="session">Session.</param>
    /// <param name="request">Request.</param>
    /// <param name="result">Result.</param>
    public override void OnCaptureCompleted(CameraCaptureSession session, CaptureRequest request,
                                            TotalCaptureResult result)
    {
        if (OnCaptureCompletedAction != null)
        {
            OnCaptureCompletedAction(session);
        }
    }
}

