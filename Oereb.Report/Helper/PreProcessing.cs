using Oereb.Report.Helper.Exceptions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Oereb.Report.Helper
{
    public static class PreProcessing
    {
        public static XElement AssignCDataToGmlNamespace(XElement source)
        {

            var gmlElements = source.Descendants().Where(x => x.Name.Namespace == "http://www.opengis.net/gml/3.2");

            if (gmlElements.Any())
            {
                var gmlElement = gmlElements.First();
                gmlElement.Parent.ReplaceNodes(new XCData(gmlElement.ToString()));
                AssignCDataToGmlNamespace(source);
            }

            return source;
        }

        public static Bitmap MergeTwoImages(Image firstImage, Image secondImage)
        {
            if (firstImage == null && secondImage == null)
            {
                return null;
            }

            if (firstImage == null)
            {
                return secondImage as Bitmap;
            }

            if (secondImage == null)
            {
                return firstImage as Bitmap;
            }

            if (firstImage.Width != secondImage.Width || firstImage.Height != secondImage.Height)
            {
                secondImage = ResizeImage(secondImage, new Size(firstImage.Width, firstImage.Height));
            }

            var outputImage = firstImage as Bitmap;

            if (outputImage == null)
            {
                return null;
            }

            using (Graphics graphics = Graphics.FromImage(outputImage))
            {
                graphics.DrawImage(secondImage, new Rectangle(new Point(), secondImage.Size), new Rectangle(new Point(), secondImage.Size), GraphicsUnit.Pixel);
            }

            return outputImage;
        }

        public static Image ResizeImage(Image image, Size size)
        {
            return new Bitmap(image, size);
        }

        public static Image GetImageFromByteArray(byte[] byteArray)
        {
            using (var ms = new MemoryStream(byteArray))
            {
                try
                {
                    return Image.FromStream(ms);
                }
                catch (Exception ex)
                {
                    throw new ImageConversionException("Cannot convert to Image", "Trying to convert to bitmap failed. Only pixel images (PNG, TIFF, JPG, BMP, etc.) are allowed.");
                }
            }
        }

        /// <summary>set the transparency from a image</summary>  
        /// <param name="image">image to set opacity on</param>  
        /// <param name="opacity">percentage of opacity</param>  
        /// <returns></returns>  

        public static Image SetImageOpacity(Image image, float opacity)
        {
            try
            {
                Bitmap bitmap = new Bitmap(image.Width, image.Height);

                using (Graphics graphics = Graphics.FromImage(bitmap))
                {

                    ColorMatrix matrix = new ColorMatrix();

                    matrix.Matrix33 = opacity;

                    ImageAttributes attributes = new ImageAttributes();

                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    graphics.DrawImage(image, new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                }
                return bitmap;
            }
            catch (Exception ex)
            {
                throw new ImageConversionException("Setting new Opacity on Image failed.", "Trying to convert to bitmap failed. Only pixel images (PNG, TIFF, JPG, BMP, etc.) are allowed.");
            }
        }
    }
}
