using System;
using System.Data;
using System.Configuration;
using System.Drawing;

namespace Asset.CommonComponent
{
    public class PictureSlightly
    {
        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="SourceImage">源图路径(含图片，物理路径)</param>
        /// <param name="ImageMapth">缩略图路径（含图片，物理路径)</param>
        /// <param name="width">缩略图宽</param>
        /// <param name="height">缩略图高</param>
        //public static void MakeImage(string sourceImage, string imageMapth, int width, int height, string mode)
        public void MakeImage(string sourceImage, string imageMapth, int width, int height, string mode)
        {
            System.Drawing.Image MyImage = System.Drawing.Image.FromFile(sourceImage);
            int towidth = width;
            int toheight = height;
            int x = 0, y = 0;
            int ow = MyImage.Width;
            int oh = MyImage.Height;
            switch (mode)
            {
                case "hw": break;       //指定高宽缩放（可能变形）
                case "w": toheight = MyImage.Height * width / MyImage.Width; break;     //指定宽，高按比例
                case "h": towidth = MyImage.Width * height / MyImage.Height; break;     //指定高，宽按比例
                case "cut":             //指定高宽裁减（不变形）
                    if ((double)MyImage.Width / (double)MyImage.Height > (double)towidth / (double)toheight)
                    {
                        oh = MyImage.Height;
                        ow = MyImage.Width * towidth / toheight;
                        y = 0;
                        x = (MyImage.Width - ow) / 2;
                    }
                    else
                    {
                        ow = MyImage.Width;
                        oh = MyImage.Height * height / toheight;
                        x = 0;
                        y = (MyImage.Height - oh) / 2;
                    }; break;
                default: break;
            }

            //新建一个bmp图片
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(towidth, toheight);

            //新建一个画板
            Graphics g = Graphics.FromImage(bitmap);

            //设置高质量插值法
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            g.Clear(Color.Transparent);

            //在指定位置并且按指定大小绘制原图片的指定部分
            g.DrawImage(MyImage, new Rectangle(0, 0, towidth, toheight), new Rectangle(x, y, ow, oh), GraphicsUnit.Pixel);

            try
            {
                //以jpg格式保存缩略图
                bitmap.Save(imageMapth, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch
            {
            }
            MyImage.Dispose();
            bitmap.Dispose();
            g.Dispose();
        }
    }
}