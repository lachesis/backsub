// Released to the public domain. Use, modify and relicense at will.

using System;
using System.IO;
using System.Drawing;

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
			: base(800, 600, GraphicsMode.Default, "OpenTK Quick Start Sample")
		{
			VSync = VSyncMode.On;
		}
		
		int shaderProgram;
		GLTextureObject mainTexture;
		
		/// <summary>Load resources here.</summary>
		/// <param name="e">Not used.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			GL.ClearColor(0.1f, 0.2f, 0.5f, 0.0f);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Texture2D);
			
			int vertexObject, fragmentObject, program;
			CreateShaders(File.ReadAllText(GetAbsolutePath("shader.vert")), File.ReadAllText(GetAbsolutePath("shader.frag")),
			              out vertexObject, out fragmentObject, out program);
			
			this.shaderProgram = program;
			this.mainTexture = new GLTextureObject(new Bitmap(GetAbsolutePath("output0106.png")));
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
		string GetAbsolutePath(string relpath)
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

			GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

			Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);
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

		/// <summary>
		/// Called when it is time to render the next frame. Add your rendering code here.
		/// </summary>
		/// <param name="e">Contains timing information.</param>
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			Matrix4 modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref modelview);

			GL.Uniform1(GL.GetUniformLocation(this.shaderProgram, "tex"), 0);
			mainTexture.Render();
			
			GL.Begin(BeginMode.Quads);
			
			GL.Color3(0.8f, 0.2f, 1.0f); GL.Vertex3(1.0f, 1.0f, 4.0f); GL.TexCoord2(1,0);
			GL.Color3(0.2f, 0.9f, 1.0f); GL.Vertex3(-1.0f, 1.0f, 4.0f); GL.TexCoord2(1,1);
			GL.Color3(1.0f, 1.0f, 0.0f); GL.Vertex3(-1.0f, -1.0f, 4.0f); GL.TexCoord2(0,1);
			GL.Color3(1.0f, 0.0f, 0.0f); GL.Vertex3(1.0f, -1.0f, 4.0f); GL.TexCoord2(0,0);
			
			GL.End();

			SwapBuffers();
		}
		
		#region CreateShaders
		/// <summary>
		/// Creates the shaders.
		/// </summary>
        void CreateShaders(string vertex_shader_source, string fragment_shader_source,
            out int vertexObject, out int fragmentObject, 
            out int program)
        {
            int status_code;
            string info;

            vertexObject = GL.CreateShader(ShaderType.VertexShader);
            fragmentObject = GL.CreateShader(ShaderType.FragmentShader);

            // Compile vertex shader
            GL.ShaderSource(vertexObject, vertex_shader_source);
            GL.CompileShader(vertexObject);
            GL.GetShaderInfoLog(vertexObject, out info);
            GL.GetShader(vertexObject, ShaderParameter.CompileStatus, out status_code);

            if (status_code != 1)
                throw new ApplicationException(info);

            // Compile vertex shader
            GL.ShaderSource(fragmentObject, fragment_shader_source);
            GL.CompileShader(fragmentObject);
            GL.GetShaderInfoLog(fragmentObject, out info);
            GL.GetShader(fragmentObject, ShaderParameter.CompileStatus, out status_code);
            
            if (status_code != 1)
                throw new ApplicationException(info);

            program = GL.CreateProgram();
            GL.AttachShader(program, fragmentObject);
            GL.AttachShader(program, vertexObject);

            GL.LinkProgram(program);
            GL.UseProgram(program);
        }
        #endregion

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
}