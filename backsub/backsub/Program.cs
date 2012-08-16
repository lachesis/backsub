// Released to the public domain. Use, modify and relicense at will.

using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Input;

namespace BackSub
{
	class Game : GameWindow
	{
		/// <summary>Creates a 800x600 window with the specified title.</summary>
		public Game()
			: base(512, 512, GraphicsMode.Default, "OpenTK Quick Start Sample")
		{
			VSync = VSyncMode.On;
		}

		GLShader shader;
		GLTextureObject inputTex;
		TextureManager texManager;
		GLVisibleFrameBufferObject visibleFbo;
		
		const int WIDTH = 1024;
		const int HEIGHT = 1024;
		const int INPUT_WIDTH = 640;
		const int INPUT_HEIGHT = 480;
		
		private ICamera camera;
		
		/// <summary>Load resources here.</summary>
		/// <param name="e">Not used.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			GL.ClearColor(0.1f, 0.2f, 0.5f, 0.0f);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Texture2D);
			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Back);
			
			//new GLTextureObject(new Bitmap(GetAbsolutePath("calib_img/big100.png"))).GetBitmapOfTexture().Save("/tmp/big100.bmp");

			this.shader = new GLShader(File.ReadAllText(GetAbsolutePath("shader.vert")), File.ReadAllText(GetAbsolutePath("calibrate.frag")));

			texManager = new TextureManager(new Rectangle(0, 0, WIDTH, HEIGHT), new string[] { "Sum", "SumSq", "StdDev" });

			visibleFbo = new GLVisibleFrameBufferObject(new Rectangle(0, 0, WIDTH, HEIGHT));
			visibleFbo.Bind();
			
			const int frameCount = 20;
			
			// Create camera
			camera = new FileCamera(
				Directory.GetFiles(GetAbsolutePath("calib_img"),"big*.png").Take(frameCount),
				new Size(WIDTH,HEIGHT)
			);
			this.inputTex = camera.Texture;
			
			// Set up render loop actions
			RenderActions = new List<Action>();
			for (int j = 0; j < frameCount; j++) {
				// Load the background frame
				Action<int> temp = (i) => {
					Console.WriteLine("Loading BkgndFrame {0}",i);
					//this.inputTex.Bind();
					camera.UpdateTexture();		
					//camera.Texture.GetBitmapOfTexture().Save("/tmp/out.bmp");
				};
				this.RenderActions.Add(temp.Curry(j));
				
				// Process it for sum (average)
				temp = (i) => {
					this.texManager.Bind();
					Console.WriteLine("Sum");
					
					this.shader.SetUniform("FrameTx", this.inputTex.TextureUnit);
					this.shader.SetUniform("SumTx", texManager.GetTexture("Sum").TextureUnit);
					this.shader.SetUniform("Mode", 1);
					this.shader.SetUniform("NumFrames", (float)frameCount);
		
					RenderToFramebuffer();
		
					texManager.EndRender("Sum");
				};
				this.RenderActions.Add(temp.Curry(j));
			}

			// Second pass
			for (int j = 0; j < frameCount; j++) {
				// Process it for SumSq
				Action<int> temp = (i) => {
					this.texManager.Bind();
					Console.WriteLine("SumSq");
					
					this.shader.SetUniform("FrameTx", this.inputTex.TextureUnit);
					this.shader.SetUniform("SumTx", texManager.GetTexture("Sum").TextureUnit);
					this.shader.SetUniform("SumSqTx", texManager.GetTexture("SumSq").TextureUnit);
					this.shader.SetUniform("Mode", 2);
					this.shader.SetUniform("NumFrames", (float)frameCount);
		
					RenderToFramebuffer();
		
					texManager.EndRender("SumSq");
				};
				this.RenderActions.Add(temp.Curry(j));
			}

			// Final step (stddev)
			{
				// Take the StdDev now that we have the sum and sumsq
				Action temp = () => {
					this.texManager.Bind();
					Console.WriteLine("StdDev");
					
					//this.shader.SetUniform("FrameTx", this.inputTex.TextureUnit);
					this.shader.SetUniform("SumTx", texManager.GetTexture("Sum").TextureUnit);
					this.shader.SetUniform("SumSqTx", texManager.GetTexture("SumSq").TextureUnit);
					this.shader.SetUniform("Mode", 3);
					this.shader.SetUniform("NumFrames", (float)frameCount);
		
					RenderToFramebuffer();
		
					texManager.EndRender("StdDev");
				};
				this.RenderActions.Add(temp);
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

		/// <summary>
		/// Called when your window is resized. Set your viewport here. It is also
		/// a good place to set up your projection matrix (which probably changes
		/// along when the aspect ratio of your window).
		/// </summary>
		/// <param name="e">Not used.</param>
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			//GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

			//Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);
			Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, 1, 0, 1, 1, 64);
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadMatrix(ref projection);
		}

		/// <summary>
		/// Called when it is time to setup the next frame. Add you game logic here.
		/// </summary>
		/// <param name="e">Contains timing information for framerate independent logic.</param>
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);

			if (Keyboard[Key.Escape])

				Exit();
		}
		
		private void RenderToFramebuffer()
		{
			GL.Begin(BeginMode.Quads);
	
			float sw = (float)INPUT_WIDTH / WIDTH;
			float sh = (float)INPUT_HEIGHT / HEIGHT;
			
			GL.TexCoord2(0f,0f);  GL.Color3(0.0f, 0.0f, 0.0f); GL.Vertex3(0f, 0f, -4.0f);
			GL.TexCoord2(sw,0f);  GL.Color3(0.0f, 0.0f, 0.0f); GL.Vertex3(sw, 0f, -4.0f);
			GL.TexCoord2(sw,sh);  GL.Color3(0.0f, 0.0f, 0.0f); GL.Vertex3(sw, sh, -4.0f);
			GL.TexCoord2(0f,sh);  GL.Color3(0.0f, 0.0f, 0.0f); GL.Vertex3(0f, sh, -4.0f);
			
			GL.End();	
		}
		
		private IList<Action> RenderActions;
		
		/// <summary>
		/// Called when it is time to render the next frame. Add your rendering code here.
		/// </summary>
		/// <param name="e">Contains timing information.</param>
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);

			// Set up modelview matrix
			Matrix4 modelview = Matrix4.Identity;
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref modelview);
			
			// Pop a function from the list and execute it
			if(RenderActions.Count > 0)
			{
				RenderActions[0]();
				RenderActions.RemoveAt(0);
			}
			else
			{
				// default function
				RenderTexToScreen(this.texManager.GetTexture("StdDev"));
				if(!dumped)
					this.texManager.GetTexture("StdDev").GetBitmapOfTexture().Save("/tmp/stdev-gl.png");
				dumped = true;
			}
		}
		private bool dumped;
		
		private void RenderTexToScreen(GLTextureObject tex)
		{
			this.visibleFbo.Bind();
			//Render texture to screen
			GL.ClearColor(0.0f, 1.0f, 0.0f, 0.0f);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			tex.Bind();
			this.shader.SetUniform("FrameTx", tex.TextureUnit);
			this.shader.SetUniform("Mode", 0);
			RenderToFramebuffer();
			SwapBuffers();
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// The 'using' idiom guarantees proper resource cleanup.
			// We request 30 UpdateFrame events per second, and unlimited
			// RenderFrame events (as fast as the computer can handle).
			using (Game game = new Game())
			{
				game.Run(30.0);
			}
		}
	}
	
	enum RenderSteps
	{
			
	}
}

public static class Extensions
{
	public static Action Curry<T>(this Action<T> act, T val)
	{
		return () => act(val);
	}
}