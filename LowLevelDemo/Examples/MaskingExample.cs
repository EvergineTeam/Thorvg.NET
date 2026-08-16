using System;
using System.IO;
using Evergine.Bindings.ThorVG;
using LowLevelDemo.Infrastructure;

namespace LowLevelDemo.Examples
{
	/// <summary>
	/// README tile "Masking" — port of thorvg.example/src/Masking.cpp: four blocks, one per mask
	/// method (alpha, inverse alpha, luma, inverse luma). Each block masks a solid rect (with a
	/// nested mask), the cartman SVG, a stroked star, and the raw ARGB image via
	/// tvg_paint_set_mask_method.
	/// </summary>
	public unsafe class MaskingExample : ExampleBase
	{
		private byte[] rawImage;    // kept alive until copy: true completes in Build

		public override string Title => "Masking";

		public override void Build(IntPtr canvas, uint width, uint height)
		{
			AddBackground(canvas, width, height, 255, 255, 255);
			IntPtr root = AddDesignRoot(canvas, width, height, 1200, 1050);

			// 200x300 ARGB8888 pixels, straight from thorvg.example/res/image.
			this.rawImage = File.ReadAllBytes(Asset("rawimage_200x300.raw"));

			BuildBlock(root, 0, 0, Tvg_Mask_Method.TVG_MASK_METHOD_ALPHA);
			BuildBlock(root, 600, 0, Tvg_Mask_Method.TVG_MASK_METHOD_INVERSE_ALPHA);
			BuildBlock(root, 0, 525, Tvg_Mask_Method.TVG_MASK_METHOD_LUMA);
			BuildBlock(root, 600, 525, Tvg_Mask_Method.TVG_MASK_METHOD_INVERSE_LUMA);

			this.rawImage = null;
		}

		private void BuildBlock(IntPtr root, float x, float y, Tvg_Mask_Method method)
		{
			// Solid rect masked by a circle that is itself masked by a shifted circle.
			IntPtr shape = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_append_rect(shape, x, y, 300, 300, 0, 0, true), "append(rect)");
			Check(ThorVG.tvg_shape_set_fill_color(shape, 0, 0, 255, 255), "fill(rect)");

			IntPtr mask = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_append_circle(mask, x + 150, y + 150, 93.75f, 93.75f, true), "append(mask)");
			Check(ThorVG.tvg_shape_set_fill_color(mask, 255, 255, 255, 255), "fill(mask)");

			IntPtr nMask = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_append_circle(nMask, x + 165, y + 165, 93.75f, 93.75f, true), "append(nMask)");
			Check(ThorVG.tvg_shape_set_fill_color(nMask, 255, 255, 255, 255), "fill(nMask)");

			Check(ThorVG.tvg_paint_set_mask_method(mask, nMask, method), "mask(nested)");
			Check(ThorVG.tvg_paint_set_mask_method(shape, mask, method), "mask(rect)");
			Check(ThorVG.tvg_scene_add(root, shape), "add(rect)");

			// The cartman SVG masked by a circle + rounded rect.
			IntPtr svg = ThorVG.tvg_picture_new();
			Check(ThorVG.tvg_picture_load(svg, Asset("cartman.svg")), "load(cartman)");
			Check(ThorVG.tvg_paint_set_opacity(svg, 100), "opacity(cartman)");
			Check(ThorVG.tvg_paint_scale(svg, 2.25f), "scale(cartman)");
			Check(ThorVG.tvg_paint_translate(svg, x + 37.5f, y + 300), "translate(cartman)");

			IntPtr mask2 = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_append_circle(mask2, x + 112.5f, y + 375, 56.25f, 56.25f, true), "append(mask2 circle)");
			Check(ThorVG.tvg_shape_append_rect(mask2, x + 112.5f, y + 375, 150, 150, 22.5f, 22.5f, true), "append(mask2 rect)");
			Check(ThorVG.tvg_shape_set_fill_color(mask2, 255, 255, 255, 255), "fill(mask2)");
			Check(ThorVG.tvg_paint_set_mask_method(svg, mask2, method), "mask(cartman)");
			Check(ThorVG.tvg_scene_add(root, svg), "add(cartman)");

			// A stroked star masked by a circle.
			IntPtr star = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_set_fill_color(star, 80, 80, 80, 255), "fill(star)");
			Check(ThorVG.tvg_shape_move_to(star, x + 449.25f, y + 25.5f), "star move_to");
			Check(ThorVG.tvg_shape_line_to(star, x + 489.75f, y + 107.25f), "star line_to");
			Check(ThorVG.tvg_shape_line_to(star, x + 580.5f, y + 120.0f), "star line_to");
			Check(ThorVG.tvg_shape_line_to(star, x + 515.25f, y + 183.0f), "star line_to");
			Check(ThorVG.tvg_shape_line_to(star, x + 530.25f, y + 273.75f), "star line_to");
			Check(ThorVG.tvg_shape_line_to(star, x + 449.25f, y + 231.75f), "star line_to");
			Check(ThorVG.tvg_shape_line_to(star, x + 372.75f, y + 273.75f), "star line_to");
			Check(ThorVG.tvg_shape_line_to(star, x + 384.0f, y + 183.75f), "star line_to");
			Check(ThorVG.tvg_shape_line_to(star, x + 319.5f, y + 120.75f), "star line_to");
			Check(ThorVG.tvg_shape_line_to(star, x + 409.5f, y + 107.25f), "star line_to");
			Check(ThorVG.tvg_shape_close(star), "star close");
			Check(ThorVG.tvg_shape_set_stroke_width(star, 7.5f), "stroke_width(star)");
			Check(ThorVG.tvg_shape_set_stroke_color(star, 255, 255, 255, 255), "stroke(star)");

			IntPtr mask3 = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_append_circle(mask3, x + 450, y + 150, 93.75f, 93.75f, true), "append(mask3)");
			Check(ThorVG.tvg_shape_set_fill_color(mask3, 255, 255, 255, 255), "fill(mask3)");
			Check(ThorVG.tvg_paint_set_mask_method(star, mask3, method), "mask(star)");
			Check(ThorVG.tvg_scene_add(root, star), "add(star)");

			// The raw ARGB image masked by a star path.
			IntPtr image = ThorVG.tvg_picture_new();
			fixed (byte* data = this.rawImage)
			{
				Check(ThorVG.tvg_picture_load_raw(image, (uint*)data, 200, 300,
					Tvg_Colorspace.TVG_COLORSPACE_ARGB8888, true), "load_raw");
			}

			Check(ThorVG.tvg_paint_translate(image, x + 375, y + 300), "translate(image)");
			Check(ThorVG.tvg_paint_scale(image, 0.75f), "scale(image)");

			IntPtr mask4 = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_move_to(mask4, x + 449.25f, y + 288.0f), "mask4 move_to");
			Check(ThorVG.tvg_shape_line_to(mask4, x + 489.75f, y + 369.75f), "mask4 line_to");
			Check(ThorVG.tvg_shape_line_to(mask4, x + 580.5f, y + 382.5f), "mask4 line_to");
			Check(ThorVG.tvg_shape_line_to(mask4, x + 515.25f, y + 445.5f), "mask4 line_to");
			Check(ThorVG.tvg_shape_line_to(mask4, x + 530.25f, y + 536.25f), "mask4 line_to");
			Check(ThorVG.tvg_shape_line_to(mask4, x + 449.25f, y + 494.25f), "mask4 line_to");
			Check(ThorVG.tvg_shape_line_to(mask4, x + 372.75f, y + 536.25f), "mask4 line_to");
			Check(ThorVG.tvg_shape_line_to(mask4, x + 384.0f, y + 446.25f), "mask4 line_to");
			Check(ThorVG.tvg_shape_line_to(mask4, x + 319.5f, y + 383.25f), "mask4 line_to");
			Check(ThorVG.tvg_shape_line_to(mask4, x + 409.5f, y + 369.75f), "mask4 line_to");
			Check(ThorVG.tvg_shape_close(mask4), "mask4 close");
			Check(ThorVG.tvg_shape_set_fill_color(mask4, 255, 255, 255, 255), "fill(mask4)");
			Check(ThorVG.tvg_paint_set_opacity(mask4, 70), "opacity(mask4)");
			Check(ThorVG.tvg_paint_set_mask_method(image, mask4, method), "mask(image)");
			Check(ThorVG.tvg_scene_add(root, image), "add(image)");
		}
	}
}
