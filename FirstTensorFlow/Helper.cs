//package com.example.androidthings.imageclassifier;


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Android.Content.Res;
using Android.Graphics;
using Android.Media;
using Android.Util;
using Java.IO;
using Java.Util;
using Android.OS;
using System.IO;
using Android.Content;
using Java.Lang;

public class Helper
{

    //public static int IMAGE_SIZE = 224;
    public static int IMAGE_SIZE = 299;
    //private static int IMAGE_MEAN = 117;
    private static int IMAGE_MEAN = 128;
    //private static float IMAGE_STD = 1;
    private static float IMAGE_STD = 128;

    //private static string LABELS_FILE = "imagenet_comp_graph_label_strings.txt";
    //private static string LABELS_FILE = "retrained_labels.txt";
    private static string LABELS_FILE = "labels.txt";
    //public static string MODEL_FILE = "tensorflow_inception_graph.pb";
    //public static string MODEL_FILE = "retrained_graph.pb";
    //public static string MODEL_FILE = "hot_dog_graph.pb";
    public static string MODEL_FILE = "optimized_graph.pb";
    //public static string INPUT_NAME = "input:0";
    public static string INPUT_NAME = "Mul";

    //public static string OUTPUT_OPERATION = "output";
    public static string OUTPUT_OPERATION = "final_result";
    public static string OUTPUT_NAME = (OUTPUT_OPERATION + ":0");

    public static string[] OUTPUT_NAMES = { OUTPUT_NAME };

    public static long[] NETWORK_STRUCTURE = { 1, IMAGE_SIZE, IMAGE_SIZE, 3 };

    //public static long[] IMAGE_SIZE;

    //public static long[] IMAGE_SIZE;

    //public static long[] 3;
    
    //public static int NUM_CLASSES = 1008;
    public static int NUM_CLASSES = 2;

    private static int MAX_BEST_RESULTS = 3;

    private static float RESULT_CONFIDENCE_THRESHOLD = 0.1f;

    public static string PreviewPath = System.IO.Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath, "tensorflow_preview.png");

    public static string[] ReadLabels(Context context)
    {
        AssetManager assetManager = context.Assets;
        var result = new List<string>();
        try
        {
            var inputstream = assetManager.Open(LABELS_FILE);
            using (BufferedReader br = new BufferedReader(new InputStreamReader(inputstream)))
            {
                string line;
                while ((line = br.ReadLine()) != null)
                {
                    result.Add(line);
                }
            }
            return result.ToArray();
        }
        catch (Java.IO.IOException ex)
        {
            throw new IllegalStateException(("Cannot read labels from " + LABELS_FILE));
        }

    }

    public static string[] GetBestResults(float[] confidenceLevels, string[] labels)
    {
        //  Find the best classifications.

        var pq = new SortedList<string, float>();
        for (int i = 0; (i < confidenceLevels.Length); i++)
        {
            if ((confidenceLevels[i] > RESULT_CONFIDENCE_THRESHOLD))
            {
                pq.Add(labels[i], confidenceLevels[i]);
            }
        }
        int recognitionsSize = Java.Lang.Math.Min(pq.Count(), MAX_BEST_RESULTS);
        string[] recognitions = new string[recognitionsSize];
        for (int i = 0; (i < recognitionsSize); i++)
        {
            var item = pq.Keys.FirstOrDefault();
            pq.Remove(item);
            recognitions[i] = item;
        }

        return recognitions;

        /*var comparator = new Java.Util.Comparator<string, string>();
        var pq = new PriorityQueue(3,new Comparator() new Java.Util.Comparator<String, float>());//Pair<String, Float>>());
        for (int i = 0; (i < confidenceLevels.Length); i++)
        {
            if ((confidenceLevels[i] > RESULT_CONFIDENCE_THRESHOLD))
            {
                pq.Add(Pair.Create(labels[i], confidenceLevels[i]));
            }

        }

        int recognitionsSize = Math.Min(pq.Size(), MAX_BEST_RESULTS);
        String[] recognitions = new String[recognitionsSize];
        for (int i = 0; (i < recognitionsSize); i++)
        {
            recognitions[i] = pq.Poll().First();
        }

        return recognitions;*/
    }

    public static string formatResults(string[] results)
    {
        string resultStr;
        if (((results == null)
                    || (results.Length == 0)))
        {
            resultStr = "I don\'t know what I see.";
        }
        else if ((results.Length == 1))
        {
            //resultStr = ("I see a " + results[0]);
            resultStr = results[0];
        }
        else
        {
            resultStr = ("I see a "
                        + (results[0] + (" or maybe a " + results[1])));
        }

        return resultStr;
    }

    public static float[] getPixels(Bitmap bitmap)
    {
        if (((bitmap.Width != IMAGE_SIZE)
                    || (bitmap.Height != IMAGE_SIZE)))
        {
            //  rescale the bitmap if needed
            bitmap = ThumbnailUtils.ExtractThumbnail(bitmap, IMAGE_SIZE, IMAGE_SIZE);
        }

        int[] intValues = new int[(IMAGE_SIZE * IMAGE_SIZE)];
        bitmap.GetPixels(intValues, 0, bitmap.Width, 0, 0, bitmap.Width, bitmap.Height);
        float[] floatValues = new float[(IMAGE_SIZE
                    * (IMAGE_SIZE * 3))];
        //  Preprocess the image data from 0-255 int to normalized float based
        //  on the provided parameters.
        for (int i = 0; (i < intValues.Length); i++)
        {
            int val = intValues[i];
            floatValues[(i * 3)] = ((((val + 16)
                        & 255)
                        - IMAGE_MEAN)
                        / IMAGE_STD);
            floatValues[((i * 3)
                        + 1)] = ((((val + 8)
                        & 255)
                        - IMAGE_MEAN)
                        / IMAGE_STD);
            floatValues[((i * 3)
                        + 2)] = (((val & 255)
                        - IMAGE_MEAN)
                        / IMAGE_STD);
        }

        return floatValues;
    }

    public static void SaveBitmap(Bitmap bitmap)
    {
        
        var fs = new FileStream(PreviewPath, FileMode.OpenOrCreate);
        if (fs != null)
        {
            bitmap.Compress(Bitmap.CompressFormat.Png, 90, fs);
            fs.Close();
        }
    }


    public static void cropAndRescaleBitmap(Bitmap src, Bitmap dst, int sensorOrientation)
    {
        //Assert.Equals(dst.Width, dst.Height;
        float minDim = Java.Lang.Math.Min(src.Width, src.Height);
        Matrix matrix = new Matrix();
        //  We only want the center square out of the original rectangle.
        float translateX = (Java.Lang.Math.Max(0, ((src.Width - minDim)
                        / 2)) * -1);
        float translateY = (Java.Lang.Math.Max(0, ((src.Height - minDim)
                        / 2)) * -1);
        matrix.PreTranslate(translateX, translateY);
        float scaleFactor = (dst.Height / minDim);
        matrix.PostScale(scaleFactor, scaleFactor);
        //  Rotate around the center if necessary.
        if ((sensorOrientation != 0))
        {
            matrix.PostTranslate(((dst.Width / 2)
                            * -1), ((dst.Height / 2)
                            * -1));
            matrix.PostRotate(sensorOrientation);
            matrix.PostTranslate((dst.Width / 2), (dst.Height / 2));
        }

        Canvas canvas = new Canvas(dst);
        canvas.DrawBitmap(src, matrix, null);
    }
}
