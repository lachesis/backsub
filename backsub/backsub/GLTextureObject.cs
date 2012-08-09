using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

namespace BackSub
{
	public class GLTextureObject : IDisposable, IRenderable
	{
		public int TextureId { get { return _id; } }
		public TextureUnit TextureUnit { get; set; }
		private int _id;
		
		public GLTextureObject(Bitmap bitmap) : this(bitmap, 1) { }
		public GLTextureObject(Bitmap bitmap, int numMipMapLevels)
		{
			_id = GL.GenTexture();
			this.TextureUnit = TextureUnit.Texture0;
			GL.ActiveTexture(this.TextureUnit);
			GL.BindTexture(TextureTarget.Texture2D, _id);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, numMipMapLevels - 1);
			Size currentSize = new Size(bitmap.Width, bitmap.Height);
			Bitmap currentBitmap = flipAndRotate(bitmap); //bitmaps coordinate system does not match opengl's coordinate system
			for (int i = 0; i < numMipMapLevels; i++)
			{
				//Load currentBitmap
				BitmapData currentData = currentBitmap.LockBits(new System.Drawing.Rectangle(0, 0, currentBitmap.Width, currentBitmap.Height),
					ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				GL.TexImage2D(TextureTarget.Texture2D, i, PixelInternalFormat.Rgba, currentSize.Width, currentSize.Height, 0,
					OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, currentData.Scan0);
				currentBitmap.UnlockBits(currentData);
				//Prepare for next iteration
				currentSize = new Size(currentSize.Width / 2, currentSize.Height / 2);
				Bitmap tempBitmap = scaleBitmap(currentBitmap, currentSize);
				currentBitmap.Dispose();
				currentBitmap = tempBitmap;
			}
			currentBitmap.Dispose();

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
		}

		private static Bitmap flipAndRotate(Bitmap source)
		{
			Bitmap dest = new Bitmap(source);
			dest.RotateFlip(RotateFlipType.RotateNoneFlipY);
			return dest;
		}

		private static Bitmap scaleBitmap(Bitmap source, Size outSize)
		{
			Bitmap dest = new Bitmap(outSize.Width, outSize.Height);
			using (Graphics graphics = Graphics.FromImage(dest))
			{
				graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
				graphics.DrawImage(source, 0, 0, dest.Width, dest.Height);
				//graphics.FillRegion(new SolidBrush(Color.FromArgb(255 / 4, 255, 0, 0)), new Region(new Rectangle(0, 0, dest.Width, dest.Height)));
			}
			return dest;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (GraphicsContext.CurrentContext != null)
			{
				GL.DeleteTexture(_id);
			}
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		~GLTextureObject()
		{
			Dispose(false);
		}
		public void Render()
		{
			GL.ActiveTexture(this.TextureUnit);
            GL.BindTexture(TextureTarget.Texture2D, _id);
		}
	}
}
