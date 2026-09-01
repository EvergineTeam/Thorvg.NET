using System;
using Evergine.Bindings.ThorVG;
using LowLevelDemo.Infrastructure;

namespace LowLevelDemo.Examples
{
	/// <summary>
	/// README tile "Anti-aliased Shapes" — port of thorvg.example/src/Shapes.cpp: a composite
	/// shape (rounded rect + two circles), a rounded rectangle, a circle and an ellipse, all
	/// rendered with the default anti-aliased engine. Override CanvasEngineOption with
	/// TVG_ENGINE_OPTION_ALIASED to see the difference.
	/// </summary>
	public unsafe class AntialiasedShapesExample : ExampleBase
	{
		public override string Title => "Anti-aliased Shapes";

		public override void Build(IntPtr canvas, uint width, uint height)
		{
			AddBackground(canvas, width, height, 255, 255, 255);
			IntPtr root = AddDesignRoot(canvas, width, height, 800, 800);

			// Composite shape: rounded rect + circle + ellipse in a single path.
			IntPtr shape4 = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_append_rect(shape4, 0, 0, 300, 300, 50, 50, true), "append_rect(shape4)");
			Check(ThorVG.tvg_shape_append_circle(shape4, 400, 150, 150, 150, true), "append_circle(shape4)");
			Check(ThorVG.tvg_shape_append_circle(shape4, 600, 150, 150, 100, true), "append_circle(shape4b)");
			Check(ThorVG.tvg_shape_set_fill_color(shape4, 255, 255, 0, 255), "fill(shape4)");
			Check(ThorVG.tvg_scene_add(root, shape4), "add(shape4)");

			// Round rectangle.
			IntPtr shape1 = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_append_rect(shape1, 0, 450, 300, 300, 50, 50, true), "append_rect(shape1)");
			Check(ThorVG.tvg_shape_set_fill_color(shape1, 0, 255, 0, 255), "fill(shape1)");
			Check(ThorVG.tvg_scene_add(root, shape1), "add(shape1)");

			// Circle.
			IntPtr shape2 = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_append_circle(shape2, 400, 600, 150, 150, true), "append_circle(shape2)");
			Check(ThorVG.tvg_shape_set_fill_color(shape2, 255, 255, 0, 255), "fill(shape2)");
			Check(ThorVG.tvg_scene_add(root, shape2), "add(shape2)");

			// Ellipse.
			IntPtr shape3 = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_append_circle(shape3, 600, 600, 150, 100, true), "append_circle(shape3)");
			Check(ThorVG.tvg_shape_set_fill_color(shape3, 0, 255, 255, 255), "fill(shape3)");
			Check(ThorVG.tvg_scene_add(root, shape3), "add(shape3)");
		}
	}
}
