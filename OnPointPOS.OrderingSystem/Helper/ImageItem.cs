using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.ImageLibrary
{
    public class ImageItem
    {
        public ImageItem(string name, int width)
        {
            Name = name;
            Width = width;
            Height = width;
            Color = Color.Transparent;
        }

        public ImageItem(string name, int width, int height)
        {
            Name = name;
            Width = width;
            Height = height;
            Color = Color.Transparent;
        }

        public ImageItem(string name, int width, int height, Color color)
        {
            Name = name;
            Width = width;
            Height = height;
            Color = color;
        }

        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Color Color { get; set; }
    }
}
