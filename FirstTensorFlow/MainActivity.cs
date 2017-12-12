using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using Com.Google.Android.Things.Contrib.Driver.Button;
using NotHotdog.Helpers;
using Android.Views;
using System;
using Android.Util;
using Org.Tensorflow.Contrib.Android;
using Android.Graphics;
using Android.Content.Res;
using Android.Media;
using ImageViews.Photo;

namespace NotHotdog
{
    [Activity(Label = "NotHotDog",  MainLauncher = true)]
    [IntentFilter(new[] { Intent.ActionMain }, Categories = new[] { Intent.CategoryLauncher })]
    [IntentFilter(new[] { Intent.ActionMain }, Categories = new[] { "android.intent.category.IOT_LAUNCHER", Intent.CategoryDefault })]
    public class MainActivity : Activity
    {
        private static String TAG = "ImageClassifierActivity";
        private ButtonInputDriver _buttonInputDriver;
        private bool _processing;

        private TensorFlowInferenceInterfaceContrib _inferenceInterface;
        private String[] _labels;

        //public static string Tag = typeof(MainActivity).FullName;
        private CameraHandler _cameraHandler;
        private ImagePreprocessor _imagePreprocessor;

        private PhotoView photoView;

        private Matrix currentMatrix = null;
        private TextView currentMatrixTextView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            InitClassifier();
            InitCamera();
            InitButton();
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            photoView = FindViewById<PhotoView>(Resource.Id.iv_photo);
            currentMatrixTextView = FindViewById<TextView>(Resource.Id.tv_current_matrix);


            Log.Debug(TAG, "READY");


        }

        private void InitButton()
        {
            try
            {
                _buttonInputDriver = new ButtonInputDriver(BoardDefaults.GetButtonGpioPin(),
                    ButtonContrib.LogicState.PressedWhenLow,
                    (int)KeyEvent.KeyCodeFromString("KEYCODE_A"));
                _buttonInputDriver.Register();
                Log.Debug(TAG, "Initialized GPIO Button that generates a keypress with KEYCODE_A");
            }
            catch (Exception e)
            {
                throw new Exception("Error initializing GPIO button", e);
            }
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.A)
            {
                /*_displayMode = AlphaNumericDisplayMode.Pressure;

                UpdateDisplay(_lastUpdatedPressure);

                try
                {
                    _led.Value = true;
                }
                catch (Exception exception)
                {
                    System.Console.WriteLine(exception);
                }*/
                return true;
            }
            return base.OnKeyUp(keyCode, e);
        }

        public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.A)
            {
                if (_processing)
                {
                    Log.Error(TAG, "Still processing, please wait");
                    return true;
                }
                Log.Debug(TAG, "Running photo recognition");
                _processing = true;
                LoadPhoto();

                /*_displayMode = AlphaNumericDisplayMode.Temperature;
                UpdateDisplay(_lastUpdatedTemperature);
                try
                {
                    _led.Value = false;
                }
                catch (Exception exception)
                {
                    System.Console.WriteLine(exception);
                }*/
                return true;
            }

            return base.OnKeyUp(keyCode, e);
        }

        private void InitClassifier()
        {
            // ADD ARTIFICIAL INTELLIGENCE
            AssetManager assets = this.Assets;
            //assets.List()
            _inferenceInterface = new TensorFlowInferenceInterfaceContrib(
                assets, Helper.MODEL_FILE);
            _labels = Helper.ReadLabels(this);
        }

        private void DestroyClassifier()
        {
            // ADD ARTIFICIAL INTELLIGENCE
            _inferenceInterface.Close();
        }

        private void InitCamera()
        {
            //  ADD CAMERA SUPPORT
            _imagePreprocessor = new ImagePreprocessor();
            _cameraHandler = CameraHandler.Instance;
            Handler threadLooper = new Handler(MainLooper);


            _cameraHandler.InitializeCamera(this, threadLooper, new ImageAvailableListener()
            {
                OnImageAvailableAction = (imageReader) =>
                {
                    Bitmap bitmap = _imagePreprocessor.PreprocessImage(imageReader.AcquireNextImage());
                    OnPhotoReady(bitmap);
                }
            });
        }

        private void CloseCamera()
        {
            //  ADD CAMERA SUPPORT
            _cameraHandler.ShutDown();
        }

        private Bitmap GetStaticBitmap()
        {
            Log.Debug(TAG, "Using sample photo in res/drawable/sampledog_224x224.png");
            return BitmapFactory.DecodeResource(Resources, Resource.Drawable.sampledog_224x224);
        }

        private void LoadPhoto()
        {
            // ADD CAMERA SUPPORT
            //Bitmap bitmap = GetStaticBitmap();
            //OnPhotoReady(bitmap);
            _cameraHandler.TakePicture();
        }

        private void OnPhotoReady(Bitmap bitmap)
        {
            DoRecognize(bitmap);
            photoView.SetImageBitmap(bitmap);
        }

        private void DoRecognize(Bitmap image)
        {
            // ADD ARTIFICIAL INTELLIGENCE
            float[] pixels = Helper.getPixels(image);

            // Feed the pixels of the image into the
            // TensorFlow Neural Network
            _inferenceInterface.Feed(Helper.INPUT_NAME, pixels,
                    Helper.NETWORK_STRUCTURE);

            // Run the TensorFlow Neural Network with the provided input
            _inferenceInterface.Run(Helper.OUTPUT_NAMES);

            // Extract the output from the neural network back
            // into an array of confidence per category
            float[] outputs = new float[Helper.NUM_CLASSES];
            _inferenceInterface.Fetch(Helper.OUTPUT_NAME, outputs);

            // Send to the callback the results with the highest
            // confidence and their labels
            OnPhotoRecognitionReady(Helper.GetBestResults(outputs, _labels));
        }

        private void OnPhotoRecognitionReady(String[] results)
        {
            var resultText = Helper.formatResults(results);
            Log.Debug(TAG, "RESULTS: " + resultText);
            currentMatrixTextView.Text = resultText;
            _processing = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            try
            {
                DestroyClassifier();
            }
            catch (Exception ex)
            {
                // close quietly
                Log.Debug(TAG, ex.ToString());
            }

            try
            {
                CloseCamera();
            }
            catch (Exception ex)
            {
                // close quietly
                Log.Debug(TAG, ex.ToString());
            }

            try
            {
                if (_buttonInputDriver != null) _buttonInputDriver.Close();
            }
            catch (Exception ex)
            {
                // close quietly
                Log.Debug(TAG,ex.ToString());
            }
        }
    }
}

