using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

namespace BackSub
{
	public class GLShader : IRenderable, IDisposable
	{
		public readonly int VertexShaderId;
		public readonly int FragmentShaderId;
		public readonly int ProgramId;
		public GLShader(string vertexShaderSource, string fragmentShaderSource)
		{
			CreateShader(vertexShaderSource, fragmentShaderSource, out VertexShaderId, out FragmentShaderId, out ProgramId);
		}

		public void Render()
		{
			GL.UseProgram(ProgramId);
		}

		protected int GetUniformLocation(string name)
		{
			return GL.GetUniformLocation(ProgramId, name);
		}

		#region SetUniform
		public void SetUniform(string name, int value)
		{
			GL.Uniform1(GetUniformLocation(name), value);
		}
		public void SetUniform(string name, double value)
		{
			GL.Uniform1(GetUniformLocation(name), value);
		}
		/// <summary>
		/// Sets the uniform equal to a TextureUnit. The required subtraction of TextureUnit.Texture0 is done by this method.
		/// </summary>
		public void SetUniform(string name, TextureUnit textureUnit)
		{
			SetUniform(name, textureUnit - TextureUnit.Texture0);
		}
		#endregion


		/// <summary>
		/// Creates the shaders.
		/// </summary>
		public static void CreateShader(string vertexShaderSource, string fragmentShaderSource,
			out int vertexShaderId, out int fragmentShaderId,
			out int programId)
		{
			int status_code;
			string info;

			vertexShaderId = GL.CreateShader(ShaderType.VertexShader);
			fragmentShaderId = GL.CreateShader(ShaderType.FragmentShader);

			// Compile vertex shader
			GL.ShaderSource(vertexShaderId, vertexShaderSource);
			GL.CompileShader(vertexShaderId);
			GL.GetShaderInfoLog(vertexShaderId, out info);
			GL.GetShader(vertexShaderId, ShaderParameter.CompileStatus, out status_code);

			if (status_code != 1)
				throw new ApplicationException(info);

			// Compile vertex shader
			GL.ShaderSource(fragmentShaderId, fragmentShaderSource);
			GL.CompileShader(fragmentShaderId);
			GL.GetShaderInfoLog(fragmentShaderId, out info);
			GL.GetShader(fragmentShaderId, ShaderParameter.CompileStatus, out status_code);

			if (status_code != 1)
				throw new ApplicationException(info);

			programId = GL.CreateProgram();
			GL.AttachShader(programId, fragmentShaderId);
			GL.AttachShader(programId, vertexShaderId);

			GL.LinkProgram(programId);
			//GL.UseProgram(program);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (GraphicsContext.CurrentContext != null)
			{
				GL.DeleteProgram(ProgramId);
				GL.DeleteShader(VertexShaderId);
				GL.DeleteShader(FragmentShaderId);
			}
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		~GLShader()
		{
			Dispose(false);
		}
	}
}
