using System.IO;
using Android.Graphics;
using Android.Media;
using Java.IO;
using Java.Lang;
using Java.Nio;
using static Android.Graphics.Bitmap;

public class ImagePreprocessor
{

    private static bool SAVE_PREVIEW_BITMAP = true;

    private Bitmap rgbFrameBitmap;

    private Bitmap croppedBitmap;

    public ImagePreprocessor()
    {
        this.croppedBitmap = Bitmap.CreateBitmap(Helper.IMAGE_SIZE, Helper.IMAGE_SIZE, Config.Argb8888);
        this.rgbFrameBitmap = Bitmap.CreateBitmap(CameraHandler.IMAGE_WIDTH, CameraHandler.IMAGE_HEIGHT, Config.Argb8888);
    }

    private static byte[] getByteArrayFromByteBuffer(ByteBuffer byteBuffer)
    {
        byte[] bytesArray = new byte[byteBuffer.Remaining()];
        byteBuffer.Get(bytesArray, 0, bytesArray.Length);
        return bytesArray;
    }

    public Bitmap PreprocessImage(Image image)
    {
        if ((image == null))
        {
            return null;
        }

        //Assert.assertEquals("Invalid size width", this.rgbFrameBitmap.Width, image.Width);
        //Assert.assertEquals("Invalid size height", this.rgbFrameBitmap.Height, image.Heigh)t;
        if (((this.croppedBitmap != null)
                    && (this.rgbFrameBitmap != null)))
        {
            ByteBuffer bb = image.GetPlanes()[0].Buffer;
            var byteArray = getByteArrayFromByteBuffer(bb);
            using (var stream = new MemoryStream(byteArray))
            {
                this.rgbFrameBitmap = BitmapFactory.DecodeStream(stream);
                Helper.cropAndRescaleBitmap(this.rgbFrameBitmap, this.croppedBitmap, 0);
            }
        }

        image.Close();
        //  For debugging
        if (SAVE_PREVIEW_BITMAP)
        {
            Helper.SaveBitmap(this.croppedBitmap);
        }

        return this.croppedBitmap;
    }

    private class ByteBufferBackedInputStream : InputStream
    {

        ByteBuffer buf;

        public ByteBufferBackedInputStream(ByteBuffer buf) => this.buf = buf;

        public int read()
        {
            if (!this.buf.HasRemaining)
            {
                return -1;
            }

            return (this.buf.Get() & 255);
        }

        public int read(byte[] bytes, int off, int len)
        {
            if (!this.buf.HasRemaining)
            {
                return -1;
            }

            len = Math.Min(len, this.buf.Remaining());
            this.buf.Get(bytes, off, len);
            return len;
        }

        public override int Read()
        {
            return read();
        }
    }
}