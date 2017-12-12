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

using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using Android.OS;
using Android.Util;
using Android.Views;
using Java.Lang;
using Java.Nio;
using static Android.Media.ImageReader;

public class CameraHandler
{

    private static string TAG = typeof(CameraHandler).FullName;

    public static int IMAGE_WIDTH = 640;

    public static int IMAGE_HEIGHT = 480;

    private static int MAX_IMAGES = 1;

    public CameraDevice CameraDevice;

    public CameraCaptureSession CaptureSession;

    private ImageReader mImageReader;

    //  Lazy-loaded singleton, so only one instance of the camera is created.
    private CameraHandler()
    {

    }

    private static CameraHandler _camera;

    public static CameraHandler Instance
    {
        get
        {
            if (_camera == null){
                _camera = new CameraHandler();
            }
            return _camera;
        }
    }


    //public static CameraHandler getInstance() => InstanceHolder.camera;

    public void InitializeCamera(Context context, Handler backgroundHandler, IOnImageAvailableListener imageAvailableListener)
    {
        //  Discover the camera instance
        CameraManager manager = ((CameraManager)(context.GetSystemService(Context.CameraService)));
        string[] camIds = new string[0];
        try
        {
            camIds = manager.GetCameraIdList();
        }
        catch (CameraAccessException e)
        {
            Log.Debug(TAG, "Cam access exception getting IDs", e);
        }

        if ((camIds.Length < 1))
        {
            Log.Debug(TAG, "No cameras found");
            return;
        }

        string id = camIds[0];
        Log.Debug(TAG, ("Using camera id " + id));
        //  Initialize the image processor
        this.mImageReader = ImageReader.NewInstance(IMAGE_WIDTH, IMAGE_HEIGHT, Android.Graphics.ImageFormatType.Jpeg, MAX_IMAGES);
        this.mImageReader.SetOnImageAvailableListener(imageAvailableListener, backgroundHandler);
        //  Open the camera resource
        try
        {
            var mStateCallback = new CameraStateListener()
            {
                CameraHandler = this
            };
            manager.OpenCamera(id, mStateCallback, backgroundHandler);
        }
        catch (CameraAccessException cae)
        {
            Log.Debug(TAG, "Camera access exception", cae);
        }

    }

    private class CameraStateListener : CameraDevice.StateCallback
    {
        public CameraHandler CameraHandler;

        public override void OnOpened(CameraDevice camera)
        {
            Log.Debug(TAG, "Opened camera.");
            CameraHandler.CameraDevice = camera;
        }

        public override void OnDisconnected(CameraDevice camera)
        {
            Log.Debug(TAG, "Camera disconnected, closing.");
            CameraHandler.CloseCaptureSession();
            camera.Close();
        }

        public override void OnError(CameraDevice camera, CameraError error)
        {
            Log.Debug(TAG, "Camera device error " + error + ", closing.");
            CameraHandler.CloseCaptureSession();
            camera.Close();
        }

        public override void OnClosed(CameraDevice camera)
        {
            Log.Debug(TAG, "Closed camera, releasing");
            _camera = null;
        }
    }

    public void CloseCaptureSession()
    {
        if (CaptureSession != null)
        {
            try
            {
                CaptureSession.Close();
            }
            catch (Java.Lang.Exception ex)
            {
                Log.Error(TAG, "Could not close capture session", ex);
            }
            CaptureSession = null;
        }
    }

    public void TakePicture()
    {
        if ((this.CameraDevice == null))
        {
            Log.Warn(TAG, "Cannot capture image. Camera not initialized.");
            return;
        }

        //  Here, we create a CameraCaptureSession for capturing still images.
        try
        {
            var outputSurfaces = new List<Surface>();
            outputSurfaces.Add(this.mImageReader.Surface);
  

            this.CameraDevice.CreateCaptureSession(outputSurfaces, new CameraCaptureStateListener()
            {
                OnConfiguredAction = (CameraCaptureSession cameraCaptureSession) =>
                {
                    try
                    {
                        if (CameraDevice == null)
                        {
                            return;
                        }
                        // When the session is ready, we start capture.
                        CaptureSession = cameraCaptureSession;
                        TriggerImageCapture();
                    }
                    catch (CameraAccessException ex)
                    {
                        Log.WriteLine(LogPriority.Info, "Capture Session error: ", ex.ToString());
                    }
                } 
            }, null);
        }
        catch (CameraAccessException cae)
        {
            Log.Debug(TAG, "access exception while preparing pic", cae);
        }

    }


    private void TriggerImageCapture()
    {
        try
        {
            CaptureRequest.Builder captureBuilder = CameraDevice.CreateCaptureRequest(CameraTemplate.StillCapture);
            captureBuilder.AddTarget(this.mImageReader.Surface);
            captureBuilder.Set(CaptureRequest.ControlAeMode, new Integer((int)ControlAEMode.On));
            //captureBuilder.set(CaptureRequest.ControlAwbMode, CaptureRequest.CONTROL_AWB_MODE_AUTO);
            Log.Debug(TAG, "Capture request created.");
            this.CaptureSession.Capture(captureBuilder.Build(), 
                                        new CameraCaptureListener(){
                OnCaptureCompletedAction =  (CameraCaptureSession session) => {
                    session.Close();
                    CaptureSession = null;
                    Log.Debug(TAG, "CaptureSession closed");
                }
            }, null);
        }
        catch (CameraAccessException cae)
        {
            Log.Debug(TAG, "camera capture exception");
        }

    }


    public void ShutDown()
    {
        this.CloseCaptureSession();
        if ((this.CameraDevice != null))
        {
            this.CameraDevice.Close();
        }

    }

    public static void dumpFormatInfo(Context context)
    {
        CameraManager manager = ((CameraManager)(context.GetSystemService(Context.CameraService)));
        string[] camIds = new string[0];
        try
        {
            camIds = manager.GetCameraIdList();
        }
        catch (CameraAccessException e)
        {
            Log.Debug(TAG, "Cam access exception getting IDs");
        }

        if ((camIds.Length < 1))
        {
            Log.Debug(TAG, "No cameras found");
        }

        string id = camIds[0];
        Log.Debug(TAG, ("Using camera id " + id));
        try
        {
            CameraCharacteristics characteristics = manager.GetCameraCharacteristics(id);
            StreamConfigurationMap configs = (Android.Hardware.Camera2.Params.StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap); //CameraCharacteristics.ScalerStreamConfigurationMap);
            foreach (int format in configs.GetOutputFormats())
            {
                Log.Debug(TAG, ("Getting sizes for format: " + format));
                foreach (Size s in configs.GetOutputSizes(format))
                {
                    Log.Debug(TAG, ("\t" + s.ToString()));
                }

            }

            int[] effects = (int[])characteristics.Get(CameraCharacteristics.ControlAvailableEffects);
            foreach (int effect in effects)
            {
                Log.Debug(TAG, ("Effect available: " + effect));
            }

        }
        catch (CameraAccessException e)
        {
            Log.Debug(TAG, "Cam access exception getting characteristics.");
        }

    }
}

