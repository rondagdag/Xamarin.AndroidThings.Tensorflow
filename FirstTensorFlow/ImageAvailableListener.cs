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

