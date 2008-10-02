using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;

namespace Glue.Lib.Graphics
{
	/// <summary>
	/// Helper functions for uploading and thumbnailing stuff
	/// </summary>
	public class Imaging
	{
        /// <summary>
        /// If either the width or the height are larger than the max allowed width, the image 
        /// is rescaled. The aspect ration is used to determine whether the rescaling is done 
        /// using the height or the width of the image.
        /// </summary>
        public static Bitmap FitBitmap(Bitmap input, int maxWidth, int maxHeight)
        {
            if (input.Width > maxWidth || input.Height > maxHeight)
            {
                double f = GetScalingFactor(input.Width, input.Height, maxWidth, maxHeight);
                Bitmap output = new Bitmap((int)(f * input.Width), (int)(f * input.Height), PixelFormat.Format24bppRgb);
                System.Drawing.Graphics canvas = System.Drawing.Graphics.FromImage(output);
                
                // Set the interpolation mode to high quality bicubic
                // interpolation, to maximize the quality of the scaled image
                canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                canvas.ScaleTransform((float)f, (float)f);

                // Drive the input bitmap through the matrix
                Rectangle drawRect = new Rectangle(0, 0, input.Size.Width, input.Size.Height);
                canvas.DrawImage(input, drawRect, drawRect, GraphicsUnit.Pixel);
                
                // We now have a scaled in-memory bitmap. Convert 
                // to original format before returning
                return ConvertBitmap(output, input.RawFormat);
            }
            else
            {
                return ConvertBitmap(input, input.RawFormat);
            }
        }

        /// <summary>
        /// If either the width or the height are larger than the max allowed width, the image 
        /// is rescaled. The aspect ration is used to determine whether the rescaling is done 
        /// using the height or the width of the image.
        /// The image will be padded with a background of given color.
        /// </summary>
        public static Bitmap FitBitmap(Bitmap input, int fitToWidth, int fitToHeight, Color backgroundColor)
        {
            // Create a new bitmap object with given size
            Bitmap output = new Bitmap(
                fitToWidth,
                fitToHeight,
                PixelFormat.Format24bppRgb //Graphics.FromImage doesn't like Indexed pixel format
                );
            System.Drawing.Graphics canvas = System.Drawing.Graphics.FromImage(output);

            // Fill background
            canvas.Clear(backgroundColor);

            // Set the interpolation mode to high quality bicubic
            // interpolation, to maximize the quality of the scaled image
            canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Set the transformation matrix on the canvas
            if (input.Width > fitToWidth || input.Height > fitToHeight)
            {
                if ((double)input.Width / input.Height > (double)fitToWidth / fitToHeight)
                {
                    // The image is 'more' wide than high, we'll scale to fitToWidth, 
                    // this will ensure the bottom of the image is not cropped.
                    float s = (float)fitToWidth / input.Width; 
                    float d = ((float)fitToHeight - input.Height * s) / 2; 
                    // Set transformation matrix:
                    // | s 0 0 |
                    // | 0 s d |
                    canvas.Transform = new Matrix(s, 0, 0, s, 0, d);
                }
                else
                {
                    // The image is 'more' high than wide; scale to fitToHeight.
                    float s = (float)fitToHeight / input.Height;
                    float d = ((float)fitToWidth - input.Width * s) / 2;
                    // Set transformation matrix:
                    // | s 0 d |
                    // | 0 s 0 |
                    canvas.Transform = new Matrix(s, 0, 0, s, d, 0);
                }
            }
            else
            {
                // No scaling necessary, perform a translation to center the image.
                // | 1 0 dx |
                // | 0 1 dy |
                canvas.TranslateTransform(
                    (fitToWidth - input.Width) / 2, 
                    (fitToHeight - input.Height) / 2
                    );
            }

            // Drive the input bitmap through the matrix
            Rectangle drawRect = new Rectangle(0, 0, input.Size.Width, input.Size.Height);
            canvas.DrawImage(
                input, 
                drawRect, 
                drawRect,
                GraphicsUnit.Pixel
                );
            
            // We now have a (scaled and centered) in-memory bitmap. Convert 
            // to original format before returning
            return ConvertBitmap(output, input.RawFormat);
        }

        /// <summary>
        /// Returns the scaling factor to fit an image inside the
        /// given boundaries.
        /// </summary>
        public static double GetScalingFactor(int width, int height, int fitToWidth, int fitToHeight)
        {
            if (width > fitToWidth || height > fitToHeight)
                if ((double)width / height > (double)fitToWidth / fitToHeight)
                    return (double)fitToWidth / width;
                else
                    return (double)fitToHeight / height;
            return 1.0;
        }

        /// <summary>
        /// Scale a bitmap
        /// </summary>
        public static Bitmap ScaleBitmap(Bitmap input, double scaleFactor)
        {
            return ScaleBitmap(input, scaleFactor, scaleFactor);
        }

        /// <summary>
        /// Scale a bitmap
        /// </summary>
        public static Bitmap ScaleBitmap(Bitmap input, double xScaleFactor, double yScaleFactor)
        {
            return ScaleBitmap(input, (int)(input.Size.Width * xScaleFactor), (int)(input.Size.Height * yScaleFactor));
        }
            
        /// <summary>
        /// Scale a bitmap to given width and height.
        /// </summary>
        public static Bitmap ScaleBitmap(Bitmap input, int width, int height)
        {
            // Create a new bitmap object based on the input
            Bitmap output = new Bitmap(
                width,
                height,
                PixelFormat.Format24bppRgb //Graphics.FromImage doesn't like Indexed pixel format
                );
            
            //Create a graphics object attached to the new scaled bitmap
            System.Drawing.Graphics canvas = System.Drawing.Graphics.FromImage(output);

            // Set the interpolation mode to high quality bicubic
            // interpolation, to maximize the quality of the scaled image
            canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
            canvas.ScaleTransform((float)width / input.Size.Width, (float)height / input.Size.Height);

            // Draw the bitmap in the graphics object, which will apply
            // the scale transform
            // Note that pixel units must be specified to 
            // ensure the framework doesn't attempt
            // to compensate for varying horizontal resolutions 
            // in images by resizing; in this case,
            // that's the opposite of what we want.
            Rectangle drawRect = new Rectangle(0, 0, input.Size.Width, input.Size.Height);
            canvas.DrawImage(
                input, 
                drawRect, 
                drawRect,
                GraphicsUnit.Pixel
                );

            // Dispose the graphics object, which leaves us with a 
            // standalone scaled bitmap.
            canvas.Dispose();

            // newBmp will have a RawFormat of MemoryBmp because it was created
            // from scratch instead of being based on inputBmp. 
            // convert the scaled bitmap back to the format of the source bitmap
            return ConvertBitmap(output, input.RawFormat);
        }

        /// <summary>
        /// Convert a bitmap to naother format
        /// </summary>
        public static Bitmap ConvertBitmap(Bitmap input, ImageFormat destFormat)
        {
            // Create an in-memory stream which will be used to save the converted image
            System.IO.Stream stm = new System.IO.MemoryStream();
            // Save the bitmap out to the memory stream, using the format indicated 
            input.Save(stm, destFormat);
            // imgStream contains the binary form of the bitmap in the target format.
            // load it into a new bitmap object
            return new Bitmap(stm);
        }

        /// <summary>
        /// Determines the ImageFormat from the filename. 
        /// </summary>
        public static ImageFormat FormatFromPath(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            if (ext == ".jpg" || ext == ".jpeg")
                return ImageFormat.Jpeg;
            if (ext == ".png")
                return ImageFormat.Png;
            if (ext == ".gif")
                return ImageFormat.Gif;
            if (ext == ".bmp")
                return ImageFormat.Bmp;
            if (ext == ".tiff" || ext == ".tif")
                return ImageFormat.Tiff;
            return ImageFormat.Jpeg;
        }
    }
}
