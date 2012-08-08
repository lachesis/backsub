using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackSub
{
	public interface IRenderable
	{
		void Render();
	}
	public interface IRenderableBatch
	{
		void BeginRender();
		void EndRender();
	}
}
