using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BackSub
{
	public interface ICamera
	{
		bool UpdateTexture();
		void Start();
		GLTextureObject Texture { get; }
	}
	public class FileCamera : ICamera
	{
		private IList<string> image_files;
		public GLTextureObject Texture { get; private set; }
		private int current_image = 0;
		public FileCamera(IEnumerable<string> imageFiles, Size textureSize)
		{
			this.image_files = imageFiles.ToList();
			this.Texture = new GLTextureObject(textureSize);
			this.Texture.TextureUnit = TextureUnit.Texture8;
		}
		
		public void Start() { } //NOP
		
		public bool UpdateTexture() 
		{
			current_image++;
			if(current_image == image_files.Count)
				current_image = 0;	
			
			Bitmap currentBitmap = new Bitmap(image_files[current_image]);
			currentBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
			
			BitmapData currentData = currentBitmap.LockBits(new System.Drawing.Rectangle(0, 0, currentBitmap.Width, currentBitmap.Height),
				ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			
			Texture.Bind();
			GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, currentBitmap.Width, currentBitmap.Height,
			                 OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, currentData.Scan0);
			
			currentBitmap.UnlockBits(currentData);
			currentBitmap.Dispose();
			
			return true;
		}
	}
	public class OpenCVCamera : ICamera
	{
		private Capture _capture;
		private object _lockObject = new object();
		private Image<Bgr, Byte> _lastFrame;
		public GLTextureObject Texture { get; private set; }
		public OpenCVCamera(int cameraIndex, Size textureSize)
		{
			Texture = new GLTextureObject(textureSize);
			this.Texture.TextureUnit = TextureUnit.Texture8;
			_capture = new Capture(cameraIndex);
			_capture.FlipType = FLIP.VERTICAL;
			_capture.ImageGrabbed += new Capture.GrabEventHandler(_capture_ImageGrabbed);
		}

		void _capture_ImageGrabbed(object sender, EventArgs e)
		{
			lock (_lockObject)
			{
				_lastFrame = _capture.RetrieveBgrFrame();
			}
		}

		public void Start()
		{
			_capture.Start();
		}

		public bool UpdateTexture()
		{
			lock (_lockObject)
			{
				if (_lastFrame == null) return false;
				IntPtr ptr = _lastFrame.Ptr;
				Texture.Bind();
				GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, _lastFrame.Width, _lastFrame.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, _lastFrame.Bytes); // ref ptr);
				return true;
			}
		}
	}
}
