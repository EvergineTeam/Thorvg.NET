using System;
using Evergine.Bindings.ThorVG;
using LowLevelDemo.Infrastructure;

namespace LowLevelDemo.Examples
{
	/// <summary>
	/// README tile "Blending" — condensed port of thorvg.example/src/Blending.cpp: one labelled
	/// row per blend method, each with an opaque rect pair, a linear-gradient rect pair and a
	/// tiger pair whose top paint carries tvg_paint_set_blend_method.
	/// </summary>
	public unsafe class BlendingExample : ExampleBase
	{
		private static readonly (string Name, Tvg_Blend_Method Method)[] Methods =
		{
			("Normal", Tvg_Blend_Method.TVG_BLEND_METHOD_NORMAL),
			("Multiply", Tvg_Blend_Method.TVG_BLEND_METHOD_MULTIPLY),
			("Screen", Tvg_Blend_Method.TVG_BLEND_METHOD_SCREEN),
			("Overlay", Tvg_Blend_Method.TVG_BLEND_METHOD_OVERLAY),
			("Darken", Tvg_Blend_Method.TVG_BLEND_METHOD_DARKEN),
			("Lighten", Tvg_Blend_Method.TVG_BLEND_METHOD_LIGHTEN),
			("Difference", Tvg_Blend_Method.TVG_BLEND_METHOD_DIFFERENCE),
			("Add", Tvg_Blend_Method.TVG_BLEND_METHOD_ADD),
		};

		public override string Title => "Blending";

		public override void Build(IntPtr canvas, uint width, uint height)
		{
			AddBackground(canvas, width, height, 0, 0, 0);
			IntPtr root = AddDesignRoot(canvas, width, height, 900, 8 * 150);

			for (int i = 0; i < Methods.Length; i++)
			{
				BuildRow(root, Methods[i].Name, Methods[i].Method, 0, i * 150f);
			}
		}

		private static void BuildRow(IntPtr root, string name, Tvg_Blend_Method method, float x, float y)
		{
			AddLabel(root, name, x + 20, y, 15, 255, 255, 255);

			// Opaque rect pair.
			{
				IntPtr bottom = ThorVG.tvg_shape_new();
				Check(ThorVG.tvg_shape_append_rect(bottom, x + 20, y + 25, 100, 100, 10, 10, true), "append(bottom)");
				Check(ThorVG.tvg_shape_set_fill_color(bottom, 255, 255, 0, 255), "fill(bottom)");
				Check(ThorVG.tvg_scene_add(root, bottom), "add(bottom)");

				IntPtr top = ThorVG.tvg_shape_new();
				Check(ThorVG.tvg_shape_append_rect(top, x + 45, y + 50, 100, 100, 10, 10, true), "append(top)");
				Check(ThorVG.tvg_shape_set_fill_color(top, 0, 255, 255, 255), "fill(top)");
				Check(ThorVG.tvg_paint_set_blend_method(top, method), "blend(top)");
				Check(ThorVG.tvg_scene_add(root, top), "add(top)");
			}

			// Linear-gradient rect pair.
			{
				var stops = new Tvg_Color_Stop[]
				{
					new() { offset = 0, r = 255, g = 0, b = 255, a = 255 },
					new() { offset = 1, r = 0, g = 255, b = 0, a = 127 },
				};

				IntPtr bottom = ThorVG.tvg_shape_new();
				Check(ThorVG.tvg_shape_append_rect(bottom, x + 325, y + 25, 100, 100, 10, 10, true), "append(gbottom)");
				Check(ThorVG.tvg_shape_set_gradient(bottom, MakeLinear(x + 325, y + 25, x + 425, y + 125, stops)), "gradient(gbottom)");
				Check(ThorVG.tvg_scene_add(root, bottom), "add(gbottom)");

				IntPtr top = ThorVG.tvg_shape_new();
				Check(ThorVG.tvg_shape_append_rect(top, x + 350, y + 50, 100, 100, 10, 10, true), "append(gtop)");
				Check(ThorVG.tvg_shape_set_gradient(top, MakeLinear(x + 350, y + 50, x + 450, y + 150, stops)), "gradient(gtop)");
				Check(ThorVG.tvg_paint_set_blend_method(top, method), "blend(gtop)");
				Check(ThorVG.tvg_scene_add(root, top), "add(gtop)");
			}

			// Tiger pair.
			{
				IntPtr bottom = ThorVG.tvg_picture_new();
				Check(ThorVG.tvg_picture_load(bottom, Asset("tiger.svg")), "load(tiger bottom)");
				Check(ThorVG.tvg_paint_translate(bottom, x + 600, y + 25), "translate(tiger bottom)");
				Check(ThorVG.tvg_paint_scale(bottom, 0.11f), "scale(tiger bottom)");
				Check(ThorVG.tvg_scene_add(root, bottom), "add(tiger bottom)");

				IntPtr top = ThorVG.tvg_paint_duplicate(bottom);
				Check(ThorVG.tvg_paint_translate(top, x + 625, y + 50), "translate(tiger top)");
				Check(ThorVG.tvg_paint_set_blend_method(top, method), "blend(tiger top)");
				Check(ThorVG.tvg_scene_add(root, top), "add(tiger top)");
			}
		}

		private static IntPtr MakeLinear(float x1, float y1, float x2, float y2, Tvg_Color_Stop[] stops)
		{
			IntPtr gradient = ThorVG.tvg_linear_gradient_new();
			Check(ThorVG.tvg_linear_gradient_set(gradient, x1, y1, x2, y2), "linear_gradient_set");
			fixed (Tvg_Color_Stop* stopsPtr = stops)
			{
				Check(ThorVG.tvg_gradient_set_color_stops(gradient, stopsPtr, (uint)stops.Length), "set_color_stops");
			}

			return gradient;
		}
	}
}
