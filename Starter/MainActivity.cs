using Android.App;
using Android.Widget;
using Android.OS;
using Android.Things.Pio;
using System;
using System.IO;
using Android.Util;
using Android.Content;

namespace Starter
{
    
    [Activity(Label = "Starter", MainLauncher = true, Icon = "@mipmap/icon", HardwareAccelerated = true)]
    [IntentFilter(new[] { Intent.ActionMain }, Categories = new[] { Intent.CategoryLauncher })]
    [IntentFilter(new[] { Intent.ActionMain }, Categories = new[] { "android.intent.category.IOT_LAUNCHER", Intent.CategoryDefault })]
    public class MainActivity : Activity, SeekBar.IOnSeekBarChangeListener
    {
        private static string TAG = "StarterActivity";
        private PeripheralManagerService _service;
        private ToggleButton _ledToggleView;

        private Gpio _pin26;
        private Gpio _pin21;
        private Pwm _pwm0;

        private SeekBar _ledBrightnessView;

        int count = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _service = new PeripheralManagerService();

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.myButton);

            button.Click += delegate { button.Text = $"{count++} clicks!"; };

            SetupDemo1();
            SetupDemo2();
            SetupDemo3();
        }

        private void SetupDemo1()
        {
            _ledToggleView = FindViewById<ToggleButton>(Resource.Id.ledToggle);

            try
            {
                _pin26 = _service.OpenGpio("BCM26");
                _pin26.SetDirection(Gpio.DirectionOutInitiallyLow);
            }
            catch (IOException ex)
            {
                Log.Error(TAG, "Error during onCreate!", ex);
            }

            try
            {
                _ledToggleView.Checked = _pin26.Value;
            }
            catch (IOException ex)
            {
                Log.Error(TAG, "Error during setChecked!", ex);
            }


            _ledToggleView.CheckedChange += (sender, e) => {
                try
                {
                    _pin26.Value = e.IsChecked;
                }
                catch (IOException ex)
                {
                    Log.Error(TAG, "Error during onCheckedChanged!", ex);
                }
            }; 

        }

        private void SetupDemo2()
        {
            try
            {
                _pin21 = _service.OpenGpio("BCM21");
                _pin21.SetDirection(Gpio.DirectionIn);
                _pin21.SetActiveType(Gpio.ActiveHigh);
                _pin21.SetEdgeTriggerType(Gpio.EdgeFalling);

                _pin21.RegisterGpioCallback(new ButtonCallback() { LedToggleView = _ledToggleView });

            } catch (IOException ex) {
                Log.Error(TAG, "Error during onCreate!", ex);
            }
        }

        public class ButtonCallback : GpioCallback
        {
            public Gpio ButtonPin { get; set; }

            public ToggleButton LedToggleView { get; set; }

            public override bool OnGpioEdge(Gpio gpio)
            {
                LedToggleView.Checked = !LedToggleView.Checked;
                return true;
            }

            public override void OnGpioError(Gpio gpio, int error)
            {
                Log.Error(TAG, "Error during pin22 gpio callback: " + error);
            }
        }


        private void SetupDemo3()
        {
            _ledBrightnessView = FindViewById<SeekBar>(Resource.Id.ledBrightness);

            try
            {
                _pwm0 = _service.OpenPwm("PWM0");
                _pwm0.SetPwmFrequencyHz(120);
                _pwm0.SetEnabled(true);
            }
            catch (IOException ex)
            {
                Log.Error(TAG, "Error during onCreate!", ex);
            }

            _ledBrightnessView.SetOnSeekBarChangeListener(this);
        }

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            try
            {
                _pwm0.SetPwmDutyCycle(progress);
            }
            catch (IOException ex)
            {
                Log.Error(TAG, "Error during pwm0 setPwmDutyCycle!", ex);
            }
        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {
        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
        }

        protected override void OnDestroy()
        {
            try
            {
                _pin26.Close();
                _pin21.Close();
                _pwm0.Close();
                /*pin22.unregisterGpioCallback(pin22Callback);
                pin22.close();

                pwm0.close();

                uart0.close();*/
            }
            catch (IOException ex)
            {
                Log.Error(TAG, "Error during onDestroy!", ex);
            }
            base.OnDestroy();
        }
    }
}

