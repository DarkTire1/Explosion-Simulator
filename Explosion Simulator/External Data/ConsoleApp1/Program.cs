using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics; // Не забудьте добавить ссылку на System.Numerics.Vectors
using System.Text.Json;

class Program
{
    // Класс для представления точки
    public class PointVector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public PointVector2(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    static void Main()
    {
        // Загрузка изображения
        Mat originalImage = Cv2.ImRead("C:\\Users\\Артем\\Pictures\\test.png");
        if (originalImage.Empty())
        {
            Console.WriteLine("Не удалось загрузить изображение.");
            return;
        }

        // Преобразование изображения в оттенки серого
        Mat grayImage = new Mat();
        Cv2.CvtColor(originalImage, grayImage, ColorConversionCodes.BGR2GRAY);

        // Применение гауссового размытия для снижения шума
        Mat blurredImage = new Mat();
        Cv2.GaussianBlur(grayImage, blurredImage, new Size(1, 1), 0);

        // Применение алгоритма Canny для обнаружения границ
        Mat edges = new Mat();
        Cv2.Canny(blurredImage, edges, 100, 180);

        // Нахождение контуров
        Cv2.FindContours(edges, out Point[][] contours, out HierarchyIndex[] hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

        // Создание копии оригинала для правого изображения (будет "стираться" контуры)
        Mat erasedImage = originalImage.Clone();

        // Список для хранения точек контуров
        var allPoints = new List<PointVector2>();

        // Первый пробег — закрашиваем контуры на правом изображении (erasedImage)
        foreach (var contour in contours)
        {
            // Закрашиваем найденные контуры белым цветом на правом изображении
            Cv2.DrawContours(erasedImage, contours, Array.IndexOf(contours, contour), new Scalar(255, 255, 255), 5); // Закрашиваем контуры

            // Сохраняем точки контуров в список
            foreach (var point in contour)
            {
                allPoints.Add(new PointVector2(point.X, point.Y));
            }
        }

        // Второй пробег: находим оставшиеся контуры на закрашенном изображении
        Mat edgesAfterErasing = new Mat();
        Cv2.Canny(erasedImage, edgesAfterErasing, 80, 100);

        Cv2.FindContours(edgesAfterErasing, out Point[][] remainingContours, out HierarchyIndex[] remainingHierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

        // Отрисовываем оставшиеся контуры на правом изображении (erasedImage)
        foreach (var contour in remainingContours)
        {
            Cv2.DrawContours(erasedImage, remainingContours, Array.IndexOf(remainingContours, contour), new Scalar(0, 0, 255), 1); // Рисуем оставшиеся контуры красным
        }

        // Объединение изображений: слева оригинал, справа - с закрашенными и оставшимися контурами
        Mat combinedImage = new Mat();
        Cv2.HConcat(new[] { originalImage, erasedImage }, combinedImage); // Слева оригинал, справа измененное изображение

        // Сохранение результата
        Cv2.ImWrite("output.jpg", combinedImage);

        // Сериализация точек в JSON
        string json = JsonSerializer.Serialize(allPoints, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("contours.json", json); // Сохранение в файл

        // Отображение результата
        Cv2.ImShow("Original and Processed", combinedImage);
        Cv2.WaitKey(0); // Ожидание нажатия клавиши
        Cv2.DestroyAllWindows();
    }
} 

