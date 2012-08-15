using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Runtime.InteropServices;

namespace BackSub
{
	public interface ICamera
	{
		bool UpdateTexture();
		void Start();
	}
	public class Camera : ICamera
	{
		private Capture _capture;
		private object _lockObject = new object();
		private Image<Bgr, Byte> _lastFrame;
		public readonly GLTextureObject Texture;
		public Camera(int cameraIndex, Size textureSize)
		{
			Texture = new GLTextureObject(textureSize);
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
				GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, _lastFrame.Width, _lastFrame.Height, PixelFormat.Bgr, PixelType.UnsignedByte, _lastFrame.Bytes); // ref ptr);
				return true;
			}
		}
	}
}
