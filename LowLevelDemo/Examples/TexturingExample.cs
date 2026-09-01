using System;
using Evergine.Bindings.ThorVG;
using LowLevelDemo.Infrastructure;

namespace LowLevelDemo.Examples
{
	/// <summary>
	/// README tile "Texturing" — adaptation of thorvg.example/src/PictureTransform.cpp. The C API
	/// has no pattern-fill, so the tile's photo-on-a-rotated-quad look is reproduced by loading a
	/// JPEG picture, rotating it about its centre with tvg_picture_set_origin, and clipping it to
	/// a rounded quad; a second, fainter copy underneath plays the reflection.
	/// </summary>
	public unsafe class TexturingExample : ExampleBase
	{
		public override string Title => "Texturing";

		public override void Build(IntPtr canvas, uint width, uint height)
		{
			AddBackground(canvas, width, height, 255, 255, 255);

			float cx = width / 2f;
			float cy = height / 2f;

			// The reflection: same photo, flatter angle, translucent.
			IntPtr reflection = ThorVG.tvg_picture_new();
			Check(ThorVG.tvg_picture_load(reflection, Asset("test.jpg")), "load(reflection)");
			Check(ThorVG.tvg_picture_set_origin(reflection, 0.5f, 0.5f), "origin(reflection)");
			Check(ThorVG.tvg_picture_set_size(reflection, 460, 345), "size(reflection)");
			Check(ThorVG.tvg_paint_translate(reflection, cx + 120, cy + 60), "translate(reflection)");
			Check(ThorVG.tvg_paint_rotate(reflection, 12), "rotate(reflection)");
			Check(ThorVG.tvg_paint_set_opacity(reflection, 90), "opacity(reflection)");
			Check(ThorVG.tvg_canvas_add(canvas, reflection), "add(reflection)");

			// The main photo, rotated the other way and clipped to a rounded quad — the "page".
			IntPtr picture = ThorVG.tvg_picture_new();
			Check(ThorVG.tvg_picture_load(picture, Asset("test.jpg")), "load(picture)");
			Check(ThorVG.tvg_picture_set_origin(picture, 0.5f, 0.5f), "origin(picture)");
			Check(ThorVG.tvg_picture_set_size(picture, 560, 420), "size(picture)");
			Check(ThorVG.tvg_paint_translate(picture, cx - 80, cy - 40), "translate(picture)");
			Check(ThorVG.tvg_paint_rotate(picture, -14), "rotate(picture)");

			IntPtr page = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_append_rect(page, cx - 320, cy - 230, 480, 380, 24, 24, true), "append(page)");
			Check(ThorVG.tvg_paint_set_clip(picture, page), "clip(picture)");

			Check(ThorVG.tvg_canvas_add(canvas, picture), "add(picture)");
		}
	}
}
