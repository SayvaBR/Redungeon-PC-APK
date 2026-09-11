using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Knighter.Graphics;

/// <summary>Small antialiased UI glyphs, independent of the pixel-art world sampler.</summary>
internal static class CleanPromptTexture
{
	public static Texture2D Create(GraphicsDevice device)
	{
		var pixels = new Color[512 * 64];
		return CreateTexture(device, pixels);
	}
	private static Texture2D CreateTexture(GraphicsDevice device, Color[] pixels)
	{
		Color[] colors = { new(126,184,0), new(246,82,87), new(255,183,0), new(61,151,231),
			new(128,99,235), new(255,91,101), new(40,220,160), new(236,74,231) };
		for (int cell=0; cell<8; cell++)
		for (int y=0; y<64; y++)
		for (int x=0; x<64; x++)
		{
			Vector4 sum = Vector4.Zero;
			for (int sy=0; sy<4; sy++) for (int sx=0; sx<4; sx++)
			{
				float px=x+(sx+.5f)/4-32, py=y+(sy+.5f)/4-32;
				float r=MathF.Sqrt(px*px+py*py);
				Color c = r>30 ? Color.Transparent : r>28.5f ? new Color(35,35,40) :
					r>24 ? Color.White : r>22.5f ? new Color(55,55,62) : colors[cell];
				if (r<22.5f && Symbol(cell,px,py)) c=Color.White;
				sum+=c.ToVector4();
			}
			pixels[y*512+cell*64+x]=new Color(sum/16);
		}
		var texture=new Texture2D(device,512,64);
		texture.SetData(pixels);
		return texture;
	}
	private static bool Line(float x,float y,float ax,float ay,float bx,float by)
	{
		var p=new Vector2(x-ax,y-ay); var v=new Vector2(bx-ax,by-ay);
		return (p-v*Math.Clamp(Vector2.Dot(p,v)/v.LengthSquared(),0,1)).Length()<=1.9f;
	}
	private static bool Symbol(int c,float x,float y)
	{
		bool L(float a,float b,float d,float e)=>Line(x,y,a,b,d,e);
		return c switch {
			0=>L(-9,11,0,-11)||L(0,-11,9,11)||L(-5,3,5,3),
			1=>L(-7,-11,-7,11)||L(-7,-11,3,-11)||L(3,-11,8,-6)||L(8,-6,3,0)||L(-7,0,3,0)||L(3,0,8,6)||L(8,6,3,11)||L(3,11,-7,11),
			2=>L(-9,-11,0,0)||L(9,-11,0,0)||L(0,0,0,11),
			3 or 4=>L(-8,-9,8,9)||L(-8,9,8,-9),
			5=>MathF.Abs(MathF.Sqrt(x*x+y*y)-10)<1.9f,
			6=>L(0,-12,-12,9)||L(-12,9,12,9)||L(12,9,0,-12),
			7=>L(-9,-9,9,-9)||L(9,-9,9,9)||L(9,9,-9,9)||L(-9,9,-9,-9),
			_=>false };
	}
}
