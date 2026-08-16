using System;
using Evergine.Bindings.ThorVG;
using LowLevelDemo.Infrastructure;

namespace LowLevelDemo.Examples
{
	/// <summary>
	/// README tile "Text" — port of thorvg.example/src/TextTtf.cpp. The left column is the Latin
	/// specimen: sizes, italic shear, solid colours, linear and radial gradient fills, an outline,
	/// a rotated line, a line feed and letter/line spacing. The right column checks that non-Latin
	/// UTF-8 survives the trip: Chinese, Japanese (hiragana, katakana and kanji), Korean, and
	/// accented Latin with Greek and Cyrillic.
	/// </summary>
	public unsafe class TextExample : ExampleBase
	{
		// tvg_text_set_text is marshalled as LPUTF8Str, so these C# UTF-16 literals are converted
		// to UTF-8 at the boundary — the glyphs appearing at all is the proof it round-trips.
		private const string Chinese = "不到长城非好汉！";
		private const string Japanese = "日本語 — ひらがな・カタカナ・漢字";
		private const string Korean = "유니코드 텍스트 (UTF-8)";
		private const string Mixed = "Español ñáéíóü¿! · ΕΛΛΗΝΙΚΑ · Кириллица";

		public override string Title => "Text";

		public override void Build(IntPtr canvas, uint width, uint height)
		{
			AddBackground(canvas, width, height, 75, 75, 75);
			IntPtr root = AddDesignRoot(canvas, width, height, 1460, 790);

			BuildLatinColumn(root);
			BuildUnicodeColumn(root, x: 760);
		}

		private static void BuildLatinColumn(IntPtr root)
		{
			string font = EnsureFont();

			AddText(root, font, 50, "THORVG TrueType Font", 0, 0, 255, 255, 255);

			IntPtr italic = MakeText(font, 25, "Font = \"PublicSans-Regular\", Style = Italic", 255, 255, 255);
			Check(ThorVG.tvg_text_set_italic(italic, 0.3f), "italic");
			Check(ThorVG.tvg_paint_translate(italic, 0, 100), "translate(italic)");
			Check(ThorVG.tvg_scene_add(root, italic), "add(italic)");

			AddText(root, font, 36, "Kerning Test: VA, AV, TJ, JT", 0, 165, 255, 255, 255);

			AddText(root, font, 24, "Purple Text", 0, 240, 255, 0, 255);
			AddText(root, font, 24, "Gray Text", 190, 240, 150, 150, 150);
			AddText(root, font, 24, "Yellow Text", 350, 240, 255, 255, 0);

			// Rotated line, parked in the gap the left column leaves below the colour samples.
			IntPtr rotated = MakeText(font, 16, "Transformed Text - 30'", 0, 0, 0);
			Check(ThorVG.tvg_paint_translate(rotated, 520, 300), "translate(rotated)");
			Check(ThorVG.tvg_paint_rotate(rotated, 30), "rotate(rotated)");
			Check(ThorVG.tvg_scene_add(root, rotated), "add(rotated)");

			// Linear-gradient fill.
			IntPtr linearText = MakeText(font, 46, "Linear Text", 255, 255, 255);
			Check(ThorVG.tvg_text_set_gradient(linearText, Linear(0, 23, 260, 23,
				Stop(0.0f, 255, 0, 0), Stop(0.5f, 255, 255, 0), Stop(1.0f, 255, 255, 255))), "gradient(linear)");
			Check(ThorVG.tvg_paint_translate(linearText, 0, 300), "translate(linear)");
			Check(ThorVG.tvg_scene_add(root, linearText), "add(linear)");

			// Radial-gradient fill.
			IntPtr radialText = MakeText(font, 38, "Radial Gradient Text", 255, 255, 255);
			Check(ThorVG.tvg_text_set_gradient(radialText, Radial(190, 19, 190,
				Stop(0.0f, 0, 255, 255), Stop(0.5f, 255, 255, 0), Stop(1.0f, 255, 255, 255))), "gradient(radial)");
			Check(ThorVG.tvg_paint_translate(radialText, 0, 380), "translate(radial)");
			Check(ThorVG.tvg_scene_add(root, radialText), "add(radial)");

			// Outline.
			IntPtr outlined = MakeText(font, 46, "Outlined Text", 255, 25, 25);
			Check(ThorVG.tvg_text_set_outline(outlined, 3, 255, 200, 200), "outline");
			Check(ThorVG.tvg_paint_translate(outlined, 0, 455), "translate(outlined)");
			Check(ThorVG.tvg_scene_add(root, outlined), "add(outlined)");

			AddText(root, font, 19, "LINE-FEED TEST. THIS IS THE FIRST LINE - \nTHIS IS THE SECOND LINE.", 0, 545, 255, 255, 255);

			IntPtr spaced = MakeText(font, 19, "1.5x SPACING TEST. THIS IS THE FIRST LINE - \nTHIS IS THE SECOND LINE.", 255, 255, 255);
			Check(ThorVG.tvg_text_spacing(spaced, 1.5f, 1.5f), "spacing");
			Check(ThorVG.tvg_paint_translate(spaced, 0, 630), "translate(spaced)");
			Check(ThorVG.tvg_scene_add(root, spaced), "add(spaced)");
		}

		private static void BuildUnicodeColumn(IntPtr root, float x)
		{
			string latin = EnsureFont();
			string cjk = EnsureFont("NotoSansCJK.ttf");
			string korean = EnsureFont("NotoSansKR.ttf");

			// Column rule and heading.
			IntPtr rule = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_append_rect(rule, x - 55, 0, 2, 700, 0, 0, true), "append(rule)");
			Check(ThorVG.tvg_shape_set_fill_color(rule, 120, 120, 120, 255), "fill(rule)");
			Check(ThorVG.tvg_scene_add(root, rule), "add(rule)");

			AddText(root, latin, 22, "UTF-8 round-trip", x, 0, 150, 210, 255);

			// Chinese, with an outline so it reads as the headline of this column.
			IntPtr chinese = MakeText(cjk, 50, Chinese, 255, 235, 120);
			Check(ThorVG.tvg_text_set_outline(chinese, 2, 90, 70, 20), "outline(chinese)");
			Check(ThorVG.tvg_paint_translate(chinese, x, 60), "translate(chinese)");
			Check(ThorVG.tvg_scene_add(root, chinese), "add(chinese)");

			AddText(root, latin, 15, "Chinese  (Han)", x, 140, 160, 160, 160);

			// Japanese: hiragana, katakana and kanji in one string.
			AddText(root, cjk, 28, Japanese, x, 190, 255, 255, 255);
			AddText(root, latin, 15, "Japanese  (hiragana / katakana / kanji)", x, 240, 160, 160, 160);

			// Korean, from a second CJK font — proves more than one font can be live at once.
			AddText(root, korean, 28, Korean, x, 290, 255, 255, 255);
			AddText(root, latin, 15, "Korean  (hangul)", x, 340, 160, 160, 160);

			// Accented Latin, Greek and Cyrillic, with a gradient fill.
			IntPtr mixed = MakeText(cjk, 22, Mixed, 255, 255, 255);
			Check(ThorVG.tvg_text_set_gradient(mixed, Linear(0, 11, 470, 11,
				Stop(0.0f, 120, 255, 200), Stop(1.0f, 120, 190, 255))), "gradient(mixed)");
			Check(ThorVG.tvg_paint_translate(mixed, x, 390), "translate(mixed)");
			Check(ThorVG.tvg_scene_add(root, mixed), "add(mixed)");

			AddText(root, latin, 15, "Latin diacritics / Greek / Cyrillic", x, 435, 160, 160, 160);
		}

		private static Tvg_Color_Stop Stop(float offset, byte r, byte g, byte b)
			=> new() { offset = offset, r = r, g = g, b = b, a = 255 };

		private static IntPtr Linear(float x1, float y1, float x2, float y2, params Tvg_Color_Stop[] stops)
		{
			IntPtr gradient = ThorVG.tvg_linear_gradient_new();
			Check(ThorVG.tvg_linear_gradient_set(gradient, x1, y1, x2, y2), "linear_gradient_set");
			SetStops(gradient, stops);
			return gradient;
		}

		private static IntPtr Radial(float cx, float cy, float r, params Tvg_Color_Stop[] stops)
		{
			IntPtr gradient = ThorVG.tvg_radial_gradient_new();
			Check(ThorVG.tvg_radial_gradient_set(gradient, cx, cy, r, cx, cy, 0), "radial_gradient_set");
			SetStops(gradient, stops);
			return gradient;
		}

		private static void SetStops(IntPtr gradient, Tvg_Color_Stop[] stops)
		{
			fixed (Tvg_Color_Stop* stopsPtr = stops)
			{
				Check(ThorVG.tvg_gradient_set_color_stops(gradient, stopsPtr, (uint)stops.Length), "set_color_stops");
			}
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
