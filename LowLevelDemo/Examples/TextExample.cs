using System;
using Evergine.Bindings.ThorVG;
using LowLevelDemo.Infrastructure;

namespace LowLevelDemo.Examples
{
	/// <summary>
	/// README tile "Text" — port of thorvg.example/src/TextTtf.cpp with the bundled Public Sans
	/// standing in for every font: sizes, italic shear, solid colors, linear and radial gradient
	/// fills, an outline, a rotated line, a line feed and letter/line spacing.
	/// </summary>
	public unsafe class TextExample : ExampleBase
	{
		public override string Title => "Text";

		public override void Build(IntPtr canvas, uint width, uint height)
		{
			AddBackground(canvas, width, height, 75, 75, 75);
			IntPtr root = AddDesignRoot(canvas, width, height, 1024, 800);

			string font = EnsureFont();

			AddText(root, font, 60, "THORVG TrueType Font", 0, 0, 255, 255, 255);

			IntPtr italic = MakeText(font, 30, "Font = \"PublicSans-Regular\", Size = 30, Style = Italic", 255, 255, 255);
			Check(ThorVG.tvg_text_set_italic(italic, 0.3f), "italic");
			Check(ThorVG.tvg_paint_translate(italic, 0, 120), "translate(italic)");
			Check(ThorVG.tvg_scene_add(root, italic), "add(italic)");

			AddText(root, font, 40, "Kerning Test: VA, AV, TJ, JT", 0, 195, 255, 255, 255);

			AddText(root, font, 25, "Purple Text", 0, 280, 255, 0, 255);
			AddText(root, font, 25, "Gray Text", 220, 280, 150, 150, 150);
			AddText(root, font, 25, "Yellow Text", 400, 280, 255, 255, 0);

			// Rotated line.
			IntPtr rotated = MakeText(font, 16, "Transformed Text - 30'", 0, 0, 0);
			Check(ThorVG.tvg_paint_translate(rotated, 600, 360), "translate(rotated)");
			Check(ThorVG.tvg_paint_rotate(rotated, 30), "rotate(rotated)");
			Check(ThorVG.tvg_scene_add(root, rotated), "add(rotated)");

			// Linear-gradient text.
			IntPtr linearText = MakeText(font, 50, "Linear Text", 255, 255, 255);
			IntPtr linear = ThorVG.tvg_linear_gradient_new();
			Check(ThorVG.tvg_linear_gradient_set(linear, 0, 25, 280, 25), "linear(text)");
			var stops = new Tvg_Color_Stop[]
			{
				new() { offset = 0.0f, r = 255, g = 0, b = 0, a = 255 },
				new() { offset = 0.5f, r = 255, g = 255, b = 0, a = 255 },
				new() { offset = 1.0f, r = 255, g = 255, b = 255, a = 255 },
			};
			fixed (Tvg_Color_Stop* stopsPtr = stops)
			{
				Check(ThorVG.tvg_gradient_set_color_stops(linear, stopsPtr, (uint)stops.Length), "stops(linear text)");
			}

			Check(ThorVG.tvg_text_set_gradient(linearText, linear), "gradient(linear text)");
			Check(ThorVG.tvg_paint_translate(linearText, 0, 320), "translate(linear text)");
			Check(ThorVG.tvg_scene_add(root, linearText), "add(linear text)");

			// Radial-gradient text.
			IntPtr radialText = MakeText(font, 40, "Radial Gradient Text", 255, 255, 255);
			IntPtr radial = ThorVG.tvg_radial_gradient_new();
			Check(ThorVG.tvg_radial_gradient_set(radial, 190, 25, 190, 190, 25, 0), "radial(text)");
			var stops2 = new Tvg_Color_Stop[]
			{
				new() { offset = 0.0f, r = 0, g = 255, b = 255, a = 255 },
				new() { offset = 0.5f, r = 255, g = 255, b = 0, a = 255 },
				new() { offset = 1.0f, r = 255, g = 255, b = 255, a = 255 },
			};
			fixed (Tvg_Color_Stop* stopsPtr = stops2)
			{
				Check(ThorVG.tvg_gradient_set_color_stops(radial, stopsPtr, (uint)stops2.Length), "stops(radial text)");
			}

			Check(ThorVG.tvg_text_set_gradient(radialText, radial), "gradient(radial text)");
			Check(ThorVG.tvg_paint_translate(radialText, 0, 420), "translate(radial text)");
			Check(ThorVG.tvg_scene_add(root, radialText), "add(radial text)");

			// Outlined text.
			IntPtr outlined = MakeText(font, 50, "Outlined Text", 255, 25, 25);
			Check(ThorVG.tvg_text_set_outline(outlined, 3, 255, 200, 200), "outline");
			Check(ThorVG.tvg_paint_translate(outlined, 0, 495), "translate(outlined)");
			Check(ThorVG.tvg_scene_add(root, outlined), "add(outlined)");

			// Line feed and spacing.
			AddText(root, font, 20, "LINE-FEED TEST. THIS IS THE FIRST LINE - \nTHIS IS THE SECOND LINE.", 0, 595, 255, 255, 255);

			IntPtr spaced = MakeText(font, 20, "1.5x SPACING TEST. THIS IS THE FIRST LINE - \nTHIS IS THE SECOND LINE.", 255, 255, 255);
			Check(ThorVG.tvg_text_spacing(spaced, 1.5f, 1.5f), "spacing");
			Check(ThorVG.tvg_paint_translate(spaced, 0, 670), "translate(spaced)");
			Check(ThorVG.tvg_scene_add(root, spaced), "add(spaced)");
		}

		private static IntPtr MakeText(string font, float size, string content, byte r, byte g, byte b)
		{
			IntPtr text = ThorVG.tvg_text_new();
			Check(ThorVG.tvg_text_set_font(text, font), "set_font");
			Check(ThorVG.tvg_text_set_size(text, size), "set_size");
			Check(ThorVG.tvg_text_set_text(text, content), "set_text");
			Check(ThorVG.tvg_text_set_color(text, r, g, b), "set_color");
			return text;
		}

		private static void AddText(IntPtr root, string font, float size, string content, float x, float y, byte r, byte g, byte b)
		{
			IntPtr text = MakeText(font, size, content, r, g, b);
			Check(ThorVG.tvg_paint_translate(text, x, y), "translate(text)");
			Check(ThorVG.tvg_scene_add(root, text), "add(text)");
		}
	}
}
