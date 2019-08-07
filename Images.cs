using System;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

namespace SimpRef
{
    class Frame
    {
        public BitmapImage image;
        public double translation_x = 0.0;
        public double translation_y = 0.0;
        public double rotaion = 0.0;
        public double scale_x = 1.0;
        public double scale_y = 1.0;
        public double width_window = 200.0;
        public double height_window = 200.0;
        public double width = 200.0;
        public double height = 200.0;

        public Frame()
        {
            image = new BitmapImage();
        }

        public Frame(String uri)
        {
            image = new BitmapImage(new Uri(uri));
            reset();
        }

        public Frame(BitmapImage img)
        {
            image = img;
            reset();
        }

        public void set(MainWindow mw)
        {
            width_window = mw.Width;
            height_window = mw.Height;
            translation_x = mw.translation_img.X;
            translation_y = mw.translation_img.Y;
            rotaion = mw.rotation_img.Angle;
            scale_x = mw.scale_img.ScaleX;
            scale_y = mw.scale_img.ScaleY;
            width = mw.imgbox.Width;
            height = mw.imgbox.Height;
        }

        public void apply(MainWindow mw)
        {
            mw.Width = width_window;
            mw.Height = height_window;
            mw.translation_img.X = translation_x;
            mw.translation_img.Y = translation_y;
            mw.rotation_img.Angle = rotaion;
            mw.scale_img.ScaleX = scale_x;
            mw.scale_img.ScaleY = scale_y;
            mw.imgbox.Width = width;
            mw.imgbox.Height = height;
        }

        public void reset()
        {
            double ratio;

            //pick a better size
            if (image.Height > image.Width)
            {
                height_window = 400.0;
                ratio = (height_window - GloVar.BarHeight) / image.Height;
                width_window = image.Width * ratio;

                width = 1.0;
                height = height_window / width_window;

                scale_x = width_window;
                scale_y = width_window;
            }
            else
            {
                width_window = 400.0;
                ratio = width_window / image.Width;
                height_window = image.Height * ratio + GloVar.BarHeight;

                width = width_window / height_window;
                height = 1.0;

                scale_x = height_window;
                scale_y = height_window;
            }

            translation_x = 0.0;
            translation_y = 0.0;
            rotaion = 0.0;
        }

        public void flip()
        {
            rotaion = - rotaion;
            scale_x = -scale_x;
            translation_x = width_window - translation_x;
        }
    }

    class Images
    {
        int index;
        List<Frame> images;

        public Images()
        {
            index = -1;
            images = new List<Frame>();
        }

        public void Insert(string uri)
        {
            if (uri.Contains(".jpeg") || uri.Contains(".jpg") || uri.Contains(".bmp") || uri.Contains(".png") || uri.Contains(".tif"))
            {
                if (index == -1)
                    index = 0;

                images.Insert(index, new Frame(uri));
            }
        }

        public void Insert(BitmapImage img)
        {
            if (index == -1)
                index = 0;

            images.Insert(index, new Frame(img));
        }
        /*
        public void Dispose()
        {
            foreach(Frame f in images)
            {
                f.image.StreamSource = null;
            }
            images.Clear();
        }
        */
        public int Index()
        {
            return index;
        }

        public void Insert(string[] uri)
        {
            for (int i = 0; i < uri.Length; i++)
                Insert(uri[i]);
        }

        public int Length()
        {
            return images.Count;
        }

        public Frame Remove()
        {
            if (images.Count > 0)
            {
                images.RemoveAt(index);

                index = index >= images.Count ? images.Count - 1 : index;

                if (images.Count > 0)
                    return images[index];
            }
            return new Frame();
        }

        public void Clear()
        {
            images.Clear();
        }

        public int IndexOf(String uri)
        {
            for (int i = 0; i < images.Count; i++)
                if (images[i].image.UriSource.AbsoluteUri == uri)
                    return i;
            return -1;
        }

        public Frame Next()
        {
            if (images.Count > 0)
            {
                index = index >= images.Count - 1 ? 0 : index + 1;
               return images[index];
            }
            return new Frame();
        }

        public Frame Previous()
        {
            if (images.Count > 0)
            {
                index = (index <= 0 ? images.Count : index) - 1;
                return images[index];
            }
            return new Frame();
        }

        public Frame Current()
        {
            if (images.Count > 0)
            {
                return images[index];
            }
            return new Frame();
        }
    }
}
