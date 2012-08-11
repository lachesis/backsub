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
		//This Dictionary should never change. "texo" should always point to TextureUnit.Texture0
		private readonly Dictionary<string, TextureUnit> textureNames = new Dictionary<string, TextureUnit>();
		//The same TextureUnit will not always be assocated with the same texture
		private readonly Dictionary<string, KeyValuePair<GLTextureObject, FramebufferAttachment>> textures = new Dictionary<string, KeyValuePair<GLTextureObject, FramebufferAttachment>>();
		
		public TextureManager(Rectangle viewport, IEnumerable<string> textureNames)
		{
			Fbo = new GLFrameBufferObject(viewport);
			List<string> texNames = textureNames.ToList();
			texNames.Add("scratch");
			GLTextureObject curr;
			for (int i = 0; i < texNames.Count; i++)
			{
				this.textureNames.Add(texNames[i], TextureUnit.Texture0 + i);
				curr = new GLTextureObject(viewport.Size);
				curr.TextureUnit = TextureUnit.Texture0 + i;
				this.textures.Add(texNames[i], new KeyValuePair<GLTextureObject, FramebufferAttachment>(curr, FramebufferAttachment.ColorAttachment0 + i));
				Fbo.AttachTexture2D(FramebufferAttachment.ColorAttachment0 + i, curr.TextureId);
				Fbo.DrawBuffer = FramebufferAttachment.ColorAttachment0 + i;
			}
			Fbo.Validate(true);
		}

		public GLTextureObject GetTexture(string textureName)
		{
			return textures[textureName].Key;
		}

		public void Bind()
		{
			Fbo.Bind();
			foreach (var tex in textures)
				tex.Value.Key.Bind();
		}

		public void EndRender(string textureName)
		{
			//First have to swap the texture unit so that the shaders are bound to the right shader location
			var tempTexUnit = textures["scratch"].Key.TextureUnit;
			textures["scratch"].Key.TextureUnit = textures[textureName].Key.TextureUnit;
			textures[textureName].Key.TextureUnit = tempTexUnit;
			//second have to swap the actual texture so that lookup in textures dict is correct
			var tempTex = textures["scratch"];
			textures["scratch"] = textures[textureName];
			textures[textureName] = tempTex;
			//Bind it to make everthing active
			Fbo.DrawBuffer = textures["scratch"].Value;
			Bind();
		}
	}
}
