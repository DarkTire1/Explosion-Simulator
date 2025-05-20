using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenCvSharp;
using UnityEngine;

namespace OpenCV2
{
    internal class ImageProcessor
    {
        public static Mat MergeImagesInZigzag(List<Mat> images, int rows, int columns)
        {
            List<Mat> rowsList = new List<Mat>();

            for (int i = 0; i < rows; i++)
            {
                List<Mat> rowImages = images.GetRange(i * columns, columns);

                if (i % 2 == 1)
                {
                    rowImages.Reverse();
                }

                Mat row = new Mat();
                Cv2.HConcat(rowImages.ToArray(), row);

                rowsList.Add(row);
            }

            Mat finalImage = new Mat();
            Cv2.VConcat(rowsList.ToArray(), finalImage);

            return finalImage;
        }

        public static Mat Texture2DToMat(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture), "Текстура не може бути null.");

            byte[] rawData = texture.GetRawTextureData();

            Mat mat = new Mat(texture.height, texture.width, MatType.CV_8UC3);

            Marshal.Copy(rawData, 0, mat.Data, rawData.Length);

            Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2RGB);

            Cv2.Flip(mat, mat, 0);

            return mat;
        }

        public static void ApplyCanny(Mat inputImageToBorder, out List<Vector2> outputCoordinates)
        {
            Mat grayImage = new Mat();
            Mat blurredImage = new Mat();
            Mat edges = new Mat();
            outputCoordinates = new List<Vector2>();

            Cv2.CvtColor(inputImageToBorder, grayImage, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(grayImage, blurredImage, new Size(7, 7), 1.5);
            Cv2.Canny(blurredImage, edges, 5, 15);

            Cv2.Dilate(edges, edges, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(7, 7)));
            Cv2.Erode(edges, edges, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(7, 7)));

            for (int y = 0; y < edges.Rows; y++)
            {
                for (int x = 0; x < edges.Cols; x++)
                {
                    if (edges.At<byte>(y, x) > 0)
                    {
                        float flippedY = edges.Rows - y - 1;
                        outputCoordinates.Add(new Vector2(x, flippedY));
                    }
                }
            }
        }

        public static Texture2D MatToTextureBGR(Mat mat)
        {
            if (mat.Type() != MatType.CV_8UC3)
                throw new ArgumentException("Вхідна Mat має бути типу CV_8UC3 (8-біт, 3 канали).");

            Mat matBGR = new Mat();
            Cv2.CvtColor(mat, matBGR, ColorConversionCodes.RGB2BGR);

            Cv2.Flip(matBGR, matBGR, 0);

            Texture2D texture = new Texture2D(matBGR.Cols, matBGR.Rows, TextureFormat.RGB24, false);

            byte[] rawData = new byte[matBGR.Rows * matBGR.Cols * matBGR.Channels()];
            Marshal.Copy(matBGR.Data, rawData, 0, rawData.Length);

            texture.LoadRawTextureData(rawData);
            texture.Apply();

            return texture;
        }
    }
}
