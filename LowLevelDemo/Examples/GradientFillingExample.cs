using System;
using Evergine.Bindings.ThorVG;
using LowLevelDemo.Infrastructure;

namespace LowLevelDemo.Examples
{
	/// <summary>
	/// README tile "Gradient Filling" — port of thorvg.example/src/LinearGradient.cpp (left half)
	/// and RadialGradient.cpp (right half): rects, circles and ellipses filled with multi-stop
	/// linear and radial gradients.
	/// </summary>
	public unsafe class GradientFillingExample : ExampleBase
	{
		public override string Title => "Gradient Filling";

		public override void Build(IntPtr canvas, uint width, uint height)
		{
			AddBackground(canvas, width, height, 255, 255, 255);
			IntPtr root = AddDesignRoot(canvas, width, height, 1600, 800);

			// ---- Linear gradients (LinearGradient.cpp) -------------------------------------------
			AddRect(root, 0, 0, 400, 400, Linear(0, 0, 400, 400,
				Stop(0.00f, 0, 0, 0, 255), Stop(1.00f, 255, 255, 255, 255)));

			AddCircle(root, 400, 400, 200, 200, Linear(400, 200, 400, 600,
				Stop(0.00f, 255, 0, 0, 255), Stop(0.50f, 255, 255, 0, 255), Stop(1.00f, 255, 255, 255, 255)));

			AddCircle(root, 600, 600, 150, 100, Linear(450, 600, 750, 600,
				Stop(0.00f, 0, 127, 0, 127), Stop(0.25f, 0, 170, 170, 170), Stop(0.50f, 200, 0, 200, 200), Stop(1.00f, 255, 255, 255, 255)));

			// ---- Radial gradients (RadialGradient.cpp), shifted +800 on x ------------------------
			AddRect(root, 800, 0, 400, 400, Radial(1000, 200, 200, 1000, 200, 0,
				Stop(0.00f, 255, 255, 255, 255), Stop(1.00f, 0, 0, 0, 255)));

			AddCircle(root, 1200, 400, 200, 200, Radial(1200, 400, 200, 1200, 400, 0,
				Stop(0.00f, 255, 0, 0, 255), Stop(0.50f, 255, 255, 0, 255), Stop(1.00f, 255, 255, 255, 255)));

			AddCircle(root, 1400, 600, 150, 100, Radial(1400, 600, 150, 1500, 600, 20,
				Stop(0.00f, 0, 127, 0, 127), Stop(0.25f, 0, 170, 170, 170), Stop(0.50f, 200, 0, 200, 200), Stop(1.00f, 255, 255, 255, 255)));
		}

		private static Tvg_Color_Stop Stop(float offset, byte r, byte g, byte b, byte a)
			=> new() { offset = offset, r = r, g = g, b = b, a = a };

		private static IntPtr Linear(float x1, float y1, float x2, float y2, params Tvg_Color_Stop[] stops)
		{
			IntPtr gradient = ThorVG.tvg_linear_gradient_new();
			Check(ThorVG.tvg_linear_gradient_set(gradient, x1, y1, x2, y2), "tvg_linear_gradient_set");
			SetStops(gradient, stops);
			return gradient;
		}

		private static IntPtr Radial(float cx, float cy, float r, float fx, float fy, float fr, params Tvg_Color_Stop[] stops)
		{
			IntPtr gradient = ThorVG.tvg_radial_gradient_new();
			Check(ThorVG.tvg_radial_gradient_set(gradient, cx, cy, r, fx, fy, fr), "tvg_radial_gradient_set");
			SetStops(gradient, stops);
			return gradient;
		}

		private static void SetStops(IntPtr gradient, Tvg_Color_Stop[] stops)
		{
			fixed (Tvg_Color_Stop* stopsPtr = stops)
			{
				Check(ThorVG.tvg_gradient_set_color_stops(gradient, stopsPtr, (uint)stops.Length), "tvg_gradient_set_color_stops");
			}
		}

		private static void AddRect(IntPtr root, float x, float y, float w, float h, IntPtr gradient)
		{
			IntPtr shape = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_append_rect(shape, x, y, w, h, 0, 0, true), "append_rect(gradient)");
			Check(ThorVG.tvg_shape_set_gradient(shape, gradient), "set_gradient(rect)");
			Check(ThorVG.tvg_scene_add(root, shape), "add(rect)");
		}

		private static void AddCircle(IntPtr root, float cx, float cy, float rx, float ry, IntPtr gradient)
		{
			IntPtr shape = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_append_circle(shape, cx, cy, rx, ry, true), "append_circle(gradient)");
			Check(ThorVG.tvg_shape_set_gradient(shape, gradient), "set_gradient(circle)");
			Check(ThorVG.tvg_scene_add(root, shape), "add(circle)");
		}
	}
}
