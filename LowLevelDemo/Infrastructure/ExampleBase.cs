using System;
using System.Collections.Generic;
using System.IO;
using Evergine.Bindings.ThorVG;

namespace LowLevelDemo.Infrastructure
{
	/// <summary>
	/// One README tile. Implementations build a ThorVG scene once in <see cref="Build"/>; animated
	/// examples also override <see cref="Update"/> and return true when the canvas must be
	/// re-rasterised. The runner owns the window, the canvas and the CPU buffer — an example only
	/// ever touches the canvas handle it is given.
	/// </summary>
	public abstract unsafe class ExampleBase
	{
		/// <summary>Window title suffix; defaults to the type name.</summary>
		public virtual string Title => this.GetType().Name;

		/// <summary>
		/// Engine option for tvg_swcanvas_create. DEFAULT rasterises anti-aliased; flip a single
		/// example to TVG_ENGINE_OPTION_ALIASED to compare.
		/// </summary>
		public virtual Tvg_Engine_Option CanvasEngineOption => Tvg_Engine_Option.TVG_ENGINE_OPTION_DEFAULT;

		/// <summary>Builds the scene. Called once, after the canvas target is set.</summary>
		public abstract void Build(IntPtr canvas, uint width, uint height);

		/// <summary>Per-frame hook. Return true to re-rasterise; static scenes keep the default.</summary>
		public virtual bool Update(IntPtr canvas, float elapsedSeconds) => false;

		/// <summary>Releases handles the canvas does not own (animations, detached paints).</summary>
		public virtual void Dispose()
		{
		}

		protected static string Asset(string file) => Path.Combine(AppContext.BaseDirectory, "Assets", file);

		protected static void Check(Tvg_Result result, string operation)
		{
			if (result != Tvg_Result.TVG_RESULT_SUCCESS)
			{
				throw new InvalidOperationException($"{operation} returned {result}");
			}
		}

		/// <summary>Full-canvas solid background, added first.</summary>
		protected static void AddBackground(IntPtr canvas, uint width, uint height, byte r, byte g, byte b)
		{
			IntPtr bg = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_append_rect(bg, 0, 0, width, height, 0, 0, true), "tvg_shape_append_rect(bg)");
			Check(ThorVG.tvg_shape_set_fill_color(bg, r, g, b, 255), "tvg_shape_set_fill_color(bg)");
			Check(ThorVG.tvg_canvas_add(canvas, bg), "tvg_canvas_add(bg)");
		}

		/// <summary>
		/// Root scene scaled uniformly so the upstream example's design box fits the canvas,
		/// centred — the ported code can then use upstream coordinates verbatim.
		/// </summary>
		protected static IntPtr AddDesignRoot(IntPtr canvas, uint width, uint height, float designWidth, float designHeight)
		{
			IntPtr root = ThorVG.tvg_scene_new();
			float scale = MathF.Min(width / designWidth, height / designHeight);
			var m = new Tvg_Matrix
			{
				e11 = scale, e12 = 0,     e13 = (width - (designWidth * scale)) / 2f,
				e21 = 0,     e22 = scale, e23 = (height - (designHeight * scale)) / 2f,
				e31 = 0,     e32 = 0,     e33 = 1,
			};
			Check(ThorVG.tvg_paint_set_transform(root, &m), "tvg_paint_set_transform(root)");
			Check(ThorVG.tvg_canvas_add(canvas, root), "tvg_canvas_add(root)");
			return root;
		}

		/// <summary>
		/// Loads a bundled font once and returns the family name to pass to tvg_text_set_font —
		/// which is the file name without its extension, not the name recorded inside the font.
		/// </summary>
		protected static string EnsureFont(string file = "PublicSans-Regular.ttf")
		{
			if (loadedFonts.Add(file))
			{
				Check(ThorVG.tvg_font_load(Asset(file)), $"tvg_font_load({file})");
			}

			return Path.GetFileNameWithoutExtension(file);
		}

		private static readonly HashSet<string> loadedFonts = new();

		/// <summary>A one-line text label; the font is loaded on first use.</summary>
		protected static void AddLabel(IntPtr scene, string content, float x, float y, float size, byte r, byte g, byte b)
		{
			IntPtr text = ThorVG.tvg_text_new();
			Check(ThorVG.tvg_text_set_font(text, EnsureFont()), "tvg_text_set_font(label)");
			Check(ThorVG.tvg_text_set_size(text, size), "tvg_text_set_size(label)");
			Check(ThorVG.tvg_text_set_text(text, content), "tvg_text_set_text(label)");
			Check(ThorVG.tvg_text_set_color(text, r, g, b), "tvg_text_set_color(label)");
			Check(ThorVG.tvg_paint_translate(text, x, y), "tvg_paint_translate(label)");
			Check(ThorVG.tvg_scene_add(scene, text), "tvg_scene_add(label)");
		}

		/// <summary>Row-major rotation about (cx, cy); translation lives in e13/e23.</summary>
		protected static Tvg_Matrix Rotation(float cx, float cy, float degrees)
		{
			float radians = degrees * MathF.PI / 180f;
			float cos = MathF.Cos(radians);
			float sin = MathF.Sin(radians);
			return new Tvg_Matrix
			{
				e11 = cos, e12 = -sin, e13 = cx,
				e21 = sin, e22 = cos,  e23 = cy,
				e31 = 0,   e32 = 0,    e33 = 1,
			};
		}
	}
}
