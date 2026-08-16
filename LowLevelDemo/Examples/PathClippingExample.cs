using System;
using Evergine.Bindings.ThorVG;
using LowLevelDemo.Infrastructure;

namespace LowLevelDemo.Examples
{
	/// <summary>
	/// README tile "Path-Clipping" — port of thorvg.example/src/Clipping.cpp: stroked stars
	/// clipped by circles (including a whole scene clipped at once), a gradient star clipped by a
	/// rect, the cartman SVG clipped by a two-circle path, and a translucent rounded rect clipped
	/// by a circle. Clip shapes ignore color/alpha entirely.
	/// </summary>
	public unsafe class PathClippingExample : ExampleBase
	{
		public override string Title => "Path-Clipping";

		public override void Build(IntPtr canvas, uint width, uint height)
		{
			AddBackground(canvas, width, height, 255, 255, 255);
			IntPtr root = AddDesignRoot(canvas, width, height, 800, 800);

			// ---- Two stars in a scene, clipped per-paint and per-scene ----------------------------
			{
				IntPtr scene = ThorVG.tvg_scene_new();

				IntPtr star1 = ThorVG.tvg_shape_new();
				AppendStar(star1);
				Check(ThorVG.tvg_shape_set_fill_color(star1, 255, 255, 0, 255), "fill(star1)");
				Check(ThorVG.tvg_shape_set_stroke_color(star1, 255, 0, 0, 255), "stroke(star1)");
				Check(ThorVG.tvg_shape_set_stroke_width(star1, 10), "stroke_width(star1)");
				Check(ThorVG.tvg_paint_translate(star1, -10, -10), "translate(star1)");

				IntPtr clipStar = ThorVG.tvg_shape_new();
				Check(ThorVG.tvg_shape_append_circle(clipStar, 200, 230, 110, 110, true), "append(clipStar)");
				Check(ThorVG.tvg_paint_translate(clipStar, 10, 10), "translate(clipStar)");
				Check(ThorVG.tvg_paint_set_clip(star1, clipStar), "clip(star1)");

				IntPtr star2 = ThorVG.tvg_shape_new();
				AppendStar(star2);
				Check(ThorVG.tvg_shape_set_fill_color(star2, 0, 255, 255, 255), "fill(star2)");
				Check(ThorVG.tvg_shape_set_stroke_color(star2, 0, 255, 0, 255), "stroke(star2)");
				Check(ThorVG.tvg_shape_set_stroke_width(star2, 10), "stroke_width(star2)");
				Check(ThorVG.tvg_paint_set_opacity(star2, 100), "opacity(star2)");
				Check(ThorVG.tvg_paint_translate(star2, 10, 40), "translate(star2)");

				IntPtr clipScene = ThorVG.tvg_shape_new();
				Check(ThorVG.tvg_shape_append_circle(clipScene, 200, 230, 130, 130, true), "append(clipScene)");
				Check(ThorVG.tvg_paint_translate(clipScene, 10, 10), "translate(clipScene)");

				Check(ThorVG.tvg_scene_add(scene, star1), "add(star1)");
				Check(ThorVG.tvg_scene_add(scene, star2), "add(star2)");
				Check(ThorVG.tvg_paint_set_clip(scene, clipScene), "clip(scene)");
				Check(ThorVG.tvg_scene_add(root, scene), "add(scene)");
			}

			// ---- Gradient star clipped by a rect --------------------------------------------------
			{
				IntPtr star3 = ThorVG.tvg_shape_new();
				AppendStar(star3);

				IntPtr fill = ThorVG.tvg_linear_gradient_new();
				Check(ThorVG.tvg_linear_gradient_set(fill, 100, 100, 300, 300), "linear(star3)");
				var stops = new Tvg_Color_Stop[]
				{
					new() { offset = 0, r = 0, g = 0, b = 0, a = 255 },
					new() { offset = 1, r = 255, g = 255, b = 255, a = 255 },
				};
				fixed (Tvg_Color_Stop* stopsPtr = stops)
				{
					Check(ThorVG.tvg_gradient_set_color_stops(fill, stopsPtr, (uint)stops.Length), "stops(star3)");
				}

				Check(ThorVG.tvg_shape_set_gradient(star3, fill), "gradient(star3)");
				Check(ThorVG.tvg_shape_set_stroke_color(star3, 255, 0, 0, 255), "stroke(star3)");
				Check(ThorVG.tvg_shape_set_stroke_width(star3, 10), "stroke_width(star3)");
				Check(ThorVG.tvg_paint_translate(star3, 400, 0), "translate(star3)");

				IntPtr clipRect = ThorVG.tvg_shape_new();
				Check(ThorVG.tvg_shape_append_rect(clipRect, 500, 120, 200, 200, 0, 0, true), "append(clipRect)");
				Check(ThorVG.tvg_paint_translate(clipRect, 20, 20), "translate(clipRect)");
				Check(ThorVG.tvg_paint_set_clip(star3, clipRect), "clip(star3)");

				Check(ThorVG.tvg_scene_add(root, star3), "add(star3)");
			}

			// ---- SVG picture clipped by a two-circle path ------------------------------------------
			{
				IntPtr picture = ThorVG.tvg_picture_new();
				Check(ThorVG.tvg_picture_load(picture, Asset("cartman.svg")), "load(cartman)");
				Check(ThorVG.tvg_paint_scale(picture, 3), "scale(cartman)");
				Check(ThorVG.tvg_paint_translate(picture, 50, 400), "translate(cartman)");

				IntPtr clipPath = ThorVG.tvg_shape_new();
				Check(ThorVG.tvg_shape_append_circle(clipPath, 200, 510, 50, 50, true), "append(clipPath1)");
				Check(ThorVG.tvg_shape_append_circle(clipPath, 200, 650, 50, 50, true), "append(clipPath2)");
				Check(ThorVG.tvg_paint_translate(clipPath, 20, 20), "translate(clipPath)");
				Check(ThorVG.tvg_paint_set_clip(picture, clipPath), "clip(cartman)");

				Check(ThorVG.tvg_scene_add(root, picture), "add(cartman)");
			}

			// ---- Translucent rounded rect clipped by a circle ---------------------------------------
			{
				IntPtr shape1 = ThorVG.tvg_shape_new();
				Check(ThorVG.tvg_shape_append_rect(shape1, 500, 420, 250, 250, 20, 20, true), "append(shape1)");
				Check(ThorVG.tvg_shape_set_fill_color(shape1, 255, 0, 255, 160), "fill(shape1)");

				IntPtr clipShape = ThorVG.tvg_shape_new();
				Check(ThorVG.tvg_shape_append_circle(clipShape, 600, 550, 150, 150, true), "append(clipShape)");
				Check(ThorVG.tvg_paint_set_clip(shape1, clipShape), "clip(shape1)");

				Check(ThorVG.tvg_scene_add(root, shape1), "add(shape1)");
			}
		}

		private static void AppendStar(IntPtr star)
		{
			Check(ThorVG.tvg_shape_move_to(star, 199, 34), "star move_to");
			Check(ThorVG.tvg_shape_line_to(star, 253, 143), "star line_to");
			Check(ThorVG.tvg_shape_line_to(star, 374, 160), "star line_to");
			Check(ThorVG.tvg_shape_line_to(star, 287, 244), "star line_to");
			Check(ThorVG.tvg_shape_line_to(star, 307, 365), "star line_to");
			Check(ThorVG.tvg_shape_line_to(star, 199, 309), "star line_to");
			Check(ThorVG.tvg_shape_line_to(star, 97, 365), "star line_to");
			Check(ThorVG.tvg_shape_line_to(star, 112, 245), "star line_to");
			Check(ThorVG.tvg_shape_line_to(star, 26, 161), "star line_to");
			Check(ThorVG.tvg_shape_line_to(star, 146, 143), "star line_to");
			Check(ThorVG.tvg_shape_close(star), "star close");
		}
	}
}
