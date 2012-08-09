using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenTK.Graphics;

namespace BackSub
{
	/*
	 Options to switch the texture being rendered to in order of increasing performance:

    1. Multiple FBOs
		create a separate FBO for each texture you want to render to
		switch using BindFramebuffer()
		can be 2x faster than wglMakeCurrent() in beta NVIDIA drivers
	2. Single FBO, multiple texture attachments
		textures should have same format and dimensions
		use FramebufferTexture2D() to switch between textures
	3. Single FBO, multiple texture attachments
		attach textures to different color attachments
		use glDrawBuffer() to switch rendering to different color attachments
	 */
	public class GLFrameBufferObject : IRenderable, IDisposable
	{
		public readonly int FramebufferId;
		private bool _validated;
		public Rectangle Viewport { get; protected set; }
		public GLFrameBufferObject(Rectangle viewport)
		{
			Viewport = viewport;
			_validated = false;
			DrawBuffer = FramebufferAttachment.ColorAttachment0Ext;
			FramebufferId = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.FramebufferExt, FramebufferId);
		}
		
		/// <summary>
		/// protected constructor that does nothing
		/// </summary>
		protected GLFrameBufferObject() { }

		public virtual void AttachTexture2D(FramebufferAttachment attachmentPoint, int textureId)
		{
			_validated = false;
			GL.BindFramebuffer(FramebufferTarget.FramebufferExt, FramebufferId);
			GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, attachmentPoint, TextureTarget.Texture2D, textureId, 0);
		}

		public virtual void AttachRenderbuffer(FramebufferAttachment attachmentPoint, int renderbufferId)
		{
			_validated = false;
			GL.BindFramebuffer(FramebufferTarget.FramebufferExt, FramebufferId);
			GL.Ext.FramebufferRenderbuffer(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt, RenderbufferTarget.RenderbufferExt, renderbufferId);
		}

		public void Validate(bool forceValidation)
		{
			if (forceValidation || !_validated)
			{
				GL.BindFramebuffer(FramebufferTarget.FramebufferExt, FramebufferId);
				FramebufferErrorCode errorCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
				if (errorCode != FramebufferErrorCode.FramebufferComplete)
				{
					throw new ApplicationException("FrameBufferObject validation error. Error code = " + errorCode.ToString());	
				}
				_validated = true;
			}
		}

		/// <summary>
		/// Indicates which attached buffer should be drawn to.
		/// </summary>
		public FramebufferAttachment DrawBuffer { get; set; }

		protected virtual void Dispose(bool disposing)
		{
			if (GraphicsContext.CurrentContext != null)
			{
				GL.DeleteFramebuffer(FramebufferId);
			}
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		~GLFrameBufferObject()
		{
			Dispose(false);
		}

		public virtual void Render()
		{
			GL.BindFramebuffer(FramebufferTarget.FramebufferExt, FramebufferId);
			Validate(false);
			GL.DrawBuffer((DrawBufferMode)DrawBuffer);
			GL.Viewport(Viewport.X, Viewport.Y, Viewport.Width, Viewport.Height);
		}
	}

	public class GLVisibleFrameBufferObject : GLFrameBufferObject
	{
		public GLVisibleFrameBufferObject(Rectangle viewport)
		{
			Viewport = viewport;
			GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
		}
		public override void Render()
		{
			GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0); // return to visible framebuffer
			GL.DrawBuffer(DrawBufferMode.Back);
			GL.Viewport(Viewport.X, Viewport.Y, Viewport.Width, Viewport.Height);
		}

		protected override void Dispose(bool disposing) { }

		public override void AttachRenderbuffer(FramebufferAttachment attachmentPoint, int renderbufferId)
		{
			throw new System.InvalidOperationException();
		}

		public override void AttachTexture2D(FramebufferAttachment attachmentPoint, int textureId)
		{
			throw new System.InvalidOperationException();
		}
	}
}
