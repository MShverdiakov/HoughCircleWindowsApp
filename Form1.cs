using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.Math.Geometry;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Punkt2
{
    public partial class Form1 : Form
    {
        Bitmap ImgInput;
        Bitmap grayImage;

        public Form1()
        {
            InitializeComponent();
        }

        // Применяем подавление немаксимумов
        private Bitmap ApplyNonMaximumSuppression(Bitmap gradientImage)
        {
            int width = gradientImage.Width;
            int height = gradientImage.Height;
            Bitmap result = new Bitmap(width, height);

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    Color pixel = gradientImage.GetPixel(x, y);

                    double gradientMagnitude = CalculateGradientMagnitude(gradientImage, x, y);

                    // Определяем направление градиента в градусах (0° - 180°)
                    double gradientDirection = CalculateGradientDirection(gradientImage, x, y);

                    // Проверяем направление градиента и подавляем немаксимумы
                    bool isLocalMaximum = IsLocalMaximum(gradientImage, x, y, gradientMagnitude, gradientDirection);

                    if (isLocalMaximum)
                    {
                        result.SetPixel(x, y, Color.FromArgb(255, 255, 255)); // Устанавливаем белый цвет для края
                    }
                    else
                    {
                        result.SetPixel(x, y, Color.FromArgb(0, 0, 0)); // Устанавливаем черный цвет для немаксимума
                    }
                }
            }

            return result;
        }

        // Метод для вычисления модуля градиента в пикселе
        private double CalculateGradientMagnitude(Bitmap gradientImage, int x, int y)
        {
            Color leftPixel = gradientImage.GetPixel(x - 1, y);
            Color rightPixel = gradientImage.GetPixel(x + 1, y);
            Color topPixel = gradientImage.GetPixel(x, y - 1);
            Color bottomPixel = gradientImage.GetPixel(x, y + 1);

            double gradientX = Math.Abs(leftPixel.R - rightPixel.R);
            double gradientY = Math.Abs(topPixel.R - bottomPixel.R);

            return Math.Sqrt(Math.Pow(gradientX, 2) + Math.Pow(gradientY, 2));
        }

        // Метод для вычисления направления градиента в пикселе в градусах (0° - 180°)
        private double CalculateGradientDirection(Bitmap gradientImage, int x, int y)
        {
            Color leftPixel = gradientImage.GetPixel(x - 1, y);
            Color rightPixel = gradientImage.GetPixel(x + 1, y);
            Color topPixel = gradientImage.GetPixel(x, y - 1);
            Color bottomPixel = gradientImage.GetPixel(x, y + 1);

            double gradientX = rightPixel.R - leftPixel.R;
            double gradientY = bottomPixel.R - topPixel.R;

            return Math.Atan2(gradientY, gradientX) * (180 / Math.PI);
        }

        // Метод для проверки, является ли пиксель локальным максимумом
        private bool IsLocalMaximum(Bitmap gradientImage, int x, int y, double gradientMagnitude, double gradientDirection)
        {
            double angle = gradientDirection % 180; // Приводим угол к диапазону [0, 180)

            if ((angle >= 0 && angle < 22.5) || (angle >= 157.5 && angle < 180))
            {
                // Горизонтальные направления
                return gradientMagnitude >= gradientImage.GetPixel(x - 1, y).R && gradientMagnitude >= gradientImage.GetPixel(x + 1, y).R;
            }
            else if (angle >= 22.5 && angle < 67.5)
            {
                // Диагональные направления (45°)
                return gradientMagnitude >= gradientImage.GetPixel(x - 1, y - 1).R && gradientMagnitude >= gradientImage.GetPixel(x + 1, y + 1).R;
            }
            else if (angle >= 67.5 && angle < 112.5)
            {
                // Вертикальные направления
                return gradientMagnitude >= gradientImage.GetPixel(x, y - 1).R && gradientMagnitude >= gradientImage.GetPixel(x, y + 1).R;
            }
            else
            {
                // Диагональные направления (135°)
                return gradientMagnitude >= gradientImage.GetPixel(x + 1, y - 1).R && gradientMagnitude >= gradientImage.GetPixel(x - 1, y + 1).R;
            }
        }

        private void BtnOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ImgInput = new Bitmap(dialog.FileName);
                pictureBox1.Image = ImgInput;
            }
        }

        private void BtnHoughCircle_Click(object sender, EventArgs e)
        {
            Grayscale grayscaleFilter = new Grayscale(0.2125, 0.7154, 0.0721);
            Bitmap grayImage = grayscaleFilter.Apply(ImgInput);

            Bitmap image = grayImage;

            HoughCircleTransformation houghTransform = new HoughCircleTransformation(35);
            houghTransform.ProcessImage(image);
            Bitmap houghImage = houghTransform.ToBitmap();

            HoughCircle[] circles = houghTransform.GetCirclesByRelativeIntensity(0.5); // Adjust the threshold as needed

            houghImage = houghTransform.ToBitmap();

            image.Save("./result.jpg");
            pictureBox1.Image = houghImage;
        }
    }
}
