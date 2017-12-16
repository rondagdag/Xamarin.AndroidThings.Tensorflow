
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

