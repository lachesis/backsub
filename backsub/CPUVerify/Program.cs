using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using AForge.Imaging;

namespace CPUVerify
{
	public class BitmapInfo
	{
		public Bitmap Bitmap;
		public UnmanagedImage Image;
		public BitmapData Data;
	}

	public struct Color
	{
		public Color(System.Drawing.Color color)
		{
			R = color.R / 255f;
			G = color.G / 255f;
			B = color.B / 255f;
		}
		public float R;
		public float G;
		public float B;
		public System.Drawing.Color ToDrawingColor()
		{
			return System.Drawing.Color.FromArgb(255, (int)(clip(R) * 255f), (int)(clip(G) * 255f), (int)(clip(B) * 255f));
		}
		private float clip(float value)
		{
			if (value > 1) return 1;
			if (value < 0) return 0;
			return value;
		}
	}

	public static class ExtMethods
	{
		public static Bitmap GetBitmap(this Color[,] colors)
		{
			Bitmap bitmap = new Bitmap(colors.GetLength(0), colors.GetLength(1));
			BitmapInfo info = CreateBitmapInfo(bitmap);
			UnmanagedImage image = info.Image;
			for (int x = 0; x < bitmap.Width; x++)
			{
				for (int y = 0; y < bitmap.Width; y++)
				{
					image.SetPixel(x, y, colors[x, y].ToDrawingColor());
				}
			}
			info.Bitmap.UnlockBits(info.Data);
			return info.Bitmap;
		}
		public static IEnumerable<Color> GetPixel(this IEnumerable<BitmapInfo> infos, int x, int y)
		{
			foreach (BitmapInfo info in infos)
			{
				yield return new Color(info.Image.GetPixel(x, y));
			}
		}
		public static IEnumerable<float> GetRChannel(this IEnumerable<Color> colors)
		{
			foreach (Color color in colors)
			{
				yield return color.R;
			}
		}
		public static IEnumerable<float> GetGChannel(this IEnumerable<Color> colors)
		{
			foreach (Color color in colors)
			{
				yield return color.G;
			}
		}
		public static IEnumerable<float> GetBChannel(this IEnumerable<Color> colors)
		{
			foreach (Color color in colors)
			{
				yield return color.B;
			}
		}
		public static BitmapInfo CreateBitmapInfo(this Bitmap bitmap)
		{
			BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
			var image = new UnmanagedImage(data);
			return new BitmapInfo()
			{
				Bitmap = bitmap,
				Data = data,
				Image = image
			};
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			//Create Images
			var images = Directory.GetFiles(GetAbsolutePath(""), "*.png")
				.Select(i => Bitmap.FromFile(i) as Bitmap)
				.Select(ExtMethods.CreateBitmapInfo)
				.ToList();

			//Create output image
			Color[,] output = new Color[images.First().Bitmap.Width, images.First().Bitmap.Height];
			string mode = args.Length >= 2 ? args[1].Trim().ToLower() : "stddev";
			for (int x = 0; x < output.GetLength(0); x++)
			{
				if (x % 10 == 0) Console.WriteLine(string.Format("Processing Column {0}...", x));
				for (int y = 0; y < output.GetLength(0); y++)
				{
					output[x, y] = ProcessPixel(mode, images.GetPixel(x, y).ToArray());
				}
			}

			//Save output
			using (Bitmap outputBitmap = output.GetBitmap())
			{
				outputBitmap.Save(GetAbsolutePath("") + "output.bmp", ImageFormat.Bmp);
			}

			//Clean Up
			images.Select(i =>
				{
					i.Bitmap.UnlockBits(i.Data);
					i.Bitmap.Dispose();
					return i;
				})
				.ToList();
		}

		static public Color ProcessPixel(string mode, IEnumerable<Color> values)
		{
			Color ret;
			switch (mode)
			{
				case "mean":
					ret = new Color();
					ret.R = values.GetRChannel().Average();
					ret.G = values.GetGChannel().Average();
					ret.B = values.GetBChannel().Average();
					return ret;
				case "stddev":
					Color mean = ProcessPixel("mean", values);
					ret = new Color();
					ret.R = values.GetRChannel().Select(i => (float)Math.Pow(i - mean.R, 2)).Average();
					ret.R = (float)Math.Sqrt(ret.R) * 50;
					ret.G = values.GetGChannel().Select(i => (float)Math.Pow(i - mean.G, 2)).Average();
					ret.G = (float)Math.Sqrt(ret.G) * 50;
					ret.B = values.GetBChannel().Select(i => (float)Math.Pow(i - mean.B, 2)).Average();
					ret.B = (float)Math.Sqrt(ret.B) * 50;
					return ret;
				default:
					throw new System.Exception("The passed mode was not valid!!");
			}
		}

		/// <summary>
		/// Gets the absolute path of a file relative to executing location.
		/// </summary>
		/// <returns>
		/// The absolute path.
		/// </returns>
		/// <param name='relpath'>
		/// Relative path 
		/// </param>
		static string GetAbsolutePath(string relpath)
		{
			return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + relpath;
		}
	}
}
