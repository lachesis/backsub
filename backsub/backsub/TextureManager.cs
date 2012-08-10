using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace BackSub
{
	public class TextureManager : IBindable
	{
		public GLFrameBufferObject Fbo;
		private List<GLTextureObject> textures = new List<GLTextureObject>();
		private Dictionary<string, int> textureNames = new Dictionary<string, int>();
		
		public TextureManager(Rectangle viewport, IEnumerable<string> textureNames)
		{
			Fbo = new GLFrameBufferObject(viewport);
			Fbo.DrawBuffer = FramebufferAttachment.ColorAttachment0 + textureNames.ToList().Count - 1;
			{
				GLTextureObject curr;
				List<string> texNames = textureNames.ToList();
				texNames.Add("scratch");
				for (int i = 0; i < texNames.Count; i++)
				{
					curr = new GLTextureObject(viewport.Size);
					curr.TextureUnit = TextureUnit.Texture0 + i;
					textures.Add(curr);
					this.textureNames.Add(texNames[i], i);
					Fbo.AttachTexture2D(FramebufferAttachment.ColorAttachment0 + i, curr.TextureId);
				}
			}
			Fbo.Validate(true);
		}

		public GLTextureObject GetTexture(string textureName)
		{
			return textures[textureNames[textureName]];
		}

		public void Bind()
		{
			Fbo.Bind();
			foreach (var tex in textures)
				tex.Bind();
		}

		public void EndRender(string textureName)
		{
			/*var temptu = textures[textureNames["scratch"]].TextureUnit;
			textures[textureNames["scratch"]].TextureUnit = textures[textureNames[textureName]].TextureUnit;
			textures[textureNames[textureName]].TextureUnit = temptu;*/

			var temp = textureNames["scratch"];
			textureNames["scratch"] = textureNames[textureName];
			textureNames[textureName] = temp;
			
			Fbo.DrawBuffer = FramebufferAttachment.ColorAttachment0 + textureNames["scratch"];
			this.Bind();
		}
	}
}
