using System;
using System.IO;
using Evergine.Bindings.ThorVG;
using LowLevelDemo.Infrastructure;

namespace LowLevelDemo.Examples
{
	/// <summary>
	/// The original composite demo: a breathing radial-gradient background, a ring of rotating
	/// petals, a path-command star, a dashed stroked arc, and a looping Lottie loaded from memory.
	/// The only animated example — it re-rasterises every frame and exercises the tvg_animation_*
	/// API and the byte* payload path of tvg_picture_load_data.
	/// </summary>
	public unsafe class ShowcaseExample : ExampleBase
	{
		private const int PetalCount = 10;

		private IntPtr background;      // radial gradient rect, rebuilt per frame
		private IntPtr star;            // path-command star
		private IntPtr arc;             // dashed stroked arc
		private IntPtr[] petals;        // ring of rounded rects
		private IntPtr animation;       // the Lottie
		private float lottieTotalFrames;
		private float lottieDuration;
		private float cx;
		private float cy;
		private uint width;
		private uint height;

		public override string Title => "Showcase";

		public override void Build(IntPtr canvas, uint width, uint height)
		{
			this.width = width;
			this.height = height;
			this.cx = width / 2f;
			this.cy = height / 2f;

			// Radial-gradient background. The gradient handle is owned by the shape after
			// tvg_shape_set_gradient, so no separate teardown.
			this.background = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_canvas_add(canvas, this.background), "tvg_canvas_add(background)");

			// Ring of petals: rounded rects rotated around the centre via Tvg_Matrix.
			this.petals = new IntPtr[PetalCount];
			for (int i = 0; i < PetalCount; i++)
			{
				this.petals[i] = ThorVG.tvg_shape_new();
				Check(ThorVG.tvg_shape_append_rect(this.petals[i], -18, -170, 36, 110, 18, 18, true), "tvg_shape_append_rect(petal)");
				Check(ThorVG.tvg_canvas_add(canvas, this.petals[i]), "tvg_canvas_add(petal)");
			}

			// A five-pointed star from raw path commands: the two parallel arrays are the most
			// delicate marshalling case in the API.
			this.star = ThorVG.tvg_shape_new();
			AppendStarPath(this.star, outer: 90, inner: 36);
			Check(ThorVG.tvg_shape_set_fill_color(this.star, 255, 214, 64, 255), "tvg_shape_set_fill_color(star)");
			Check(ThorVG.tvg_canvas_add(canvas, this.star), "tvg_canvas_add(star)");

			// A dashed, round-capped stroked arc, counter-rotating.
			this.arc = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_append_circle(this.arc, 0, 0, 240, 240, true), "tvg_shape_append_circle(arc)");
			Check(ThorVG.tvg_shape_set_stroke_width(this.arc, 14), "tvg_shape_set_stroke_width");
			Check(ThorVG.tvg_shape_set_stroke_color(this.arc, 90, 200, 250, 255), "tvg_shape_set_stroke_color");
			Check(ThorVG.tvg_shape_set_stroke_cap(this.arc, Tvg_Stroke_Cap.TVG_STROKE_CAP_ROUND), "tvg_shape_set_stroke_cap");
			fixed (float* dash = new float[] { 38f, 26f })
			{
				Check(ThorVG.tvg_shape_set_stroke_dash(this.arc, dash, 2, 0), "tvg_shape_set_stroke_dash");
			}

			Check(ThorVG.tvg_canvas_add(canvas, this.arc), "tvg_canvas_add(arc)");

			// The Lottie, loaded from memory: the byte* payload path. The picture belongs to the
			// animation; it is sized and centred once and driven by frame number afterwards.
			this.animation = ThorVG.tvg_animation_new();
			IntPtr picture = ThorVG.tvg_animation_get_picture(this.animation);

			var lottie = File.ReadAllBytes(Asset("animation.lot"));
			fixed (byte* data = lottie)
			{
				Check(ThorVG.tvg_picture_load_data(picture, data, (uint)lottie.Length, "lottie", null, true), "tvg_picture_load_data");
			}

			float total = 0, duration = 0;
			Check(ThorVG.tvg_animation_get_total_frame(this.animation, &total), "tvg_animation_get_total_frame");
			Check(ThorVG.tvg_animation_get_duration(this.animation, &duration), "tvg_animation_get_duration");
			this.lottieTotalFrames = total;
			this.lottieDuration = duration;

			const float lottieSize = 340f;
			Check(ThorVG.tvg_picture_set_size(picture, lottieSize, lottieSize), "tvg_picture_set_size");
			Check(ThorVG.tvg_paint_translate(picture, this.cx - (lottieSize / 2f), this.cy - (lottieSize / 2f)), "tvg_paint_translate(picture)");
			Check(ThorVG.tvg_canvas_add(canvas, picture), "tvg_canvas_add(picture)");
		}

		public override bool Update(IntPtr canvas, float t)
		{
			// Background: rebuild the gradient with a slowly breathing radius. Gradients are
			// consumed by the shape, so a fresh one per frame is the intended usage.
			Check(ThorVG.tvg_shape_reset(this.background), "tvg_shape_reset(background)");
			Check(ThorVG.tvg_shape_append_rect(this.background, 0, 0, this.width, this.height, 0, 0, true), "tvg_shape_append_rect(background)");

			IntPtr gradient = ThorVG.tvg_radial_gradient_new();
			float radius = 520f + (90f * MathF.Sin(t * 0.7f));
			Check(ThorVG.tvg_radial_gradient_set(gradient, this.cx, this.cy, radius, this.cx, this.cy, 0), "tvg_radial_gradient_set");

			var stops = new Tvg_Color_Stop[]
			{
				new() { offset = 0.0f, r = 38,  g = 44,  b = 78,  a = 255 },
				new() { offset = 0.6f, r = 24,  g = 26,  b = 48,  a = 255 },
				new() { offset = 1.0f, r = 10,  g = 10,  b = 22,  a = 255 },
			};
			fixed (Tvg_Color_Stop* stopsPtr = stops)
			{
				Check(ThorVG.tvg_gradient_set_color_stops(gradient, stopsPtr, (uint)stops.Length), "tvg_gradient_set_color_stops");
			}

			Check(ThorVG.tvg_shape_set_gradient(this.background, gradient), "tvg_shape_set_gradient");

			// Petals: each with its own rotation matrix around the centre.
			for (int i = 0; i < PetalCount; i++)
			{
				float angle = (360f / PetalCount * i) + (t * 24f);
				var m = Rotation(this.cx, this.cy, angle);
				Check(ThorVG.tvg_paint_set_transform(this.petals[i], &m), "tvg_paint_set_transform(petal)");

				byte hue = (byte)(120 + (110 * i / PetalCount));
				Check(ThorVG.tvg_shape_set_fill_color(this.petals[i], (byte)(60 + (12 * i)), hue, 220, 200), "tvg_shape_set_fill_color(petal)");
			}

			// Star: gentle spin one way...
			var starMatrix = Rotation(this.cx, this.cy, -t * 40f);
			Check(ThorVG.tvg_paint_set_transform(this.star, &starMatrix), "tvg_paint_set_transform(star)");

			// ...arc: dashed ring spinning the other way.
			var arcMatrix = Rotation(this.cx, this.cy, t * 18f);
			Check(ThorVG.tvg_paint_set_transform(this.arc, &arcMatrix), "tvg_paint_set_transform(arc)");

			// The Lottie loops at its own frame rate.
			if (this.lottieTotalFrames > 0 && this.lottieDuration > 0)
			{
				float frame = (t % this.lottieDuration) / this.lottieDuration * this.lottieTotalFrames;
				Check(ThorVG.tvg_animation_set_frame(this.animation, frame), "tvg_animation_set_frame");
			}

			return true;
		}

		public override void Dispose()
		{
			ThorVG.tvg_animation_del(this.animation);
		}

		private static void AppendStarPath(IntPtr shape, float outer, float inner)
		{
			const int points = 5;
			var cmds = new byte[points * 2 + 2];
			var pts = new Tvg_Point[points * 2 + 1];

			int c = 0, p = 0;
			cmds[c++] = ThorVG.TVG_PATH_COMMAND_MOVE_TO;
			for (int i = 0; i < points * 2; i++)
			{
				float radius = (i % 2 == 0) ? outer : inner;
				float angle = (MathF.PI * i / points) - (MathF.PI / 2);
				var point = new Tvg_Point { x = radius * MathF.Cos(angle), y = radius * MathF.Sin(angle) };

				if (i == 0)
				{
					pts[p++] = point;
				}
				else
				{
					cmds[c++] = ThorVG.TVG_PATH_COMMAND_LINE_TO;
					pts[p++] = point;
				}
			}

			cmds[c++] = ThorVG.TVG_PATH_COMMAND_CLOSE;

			fixed (byte* cmdsPtr = cmds)
			fixed (Tvg_Point* ptsPtr = pts)
			{
				Check(ThorVG.tvg_shape_append_path(shape, cmdsPtr, (uint)c, ptsPtr, (uint)p), "tvg_shape_append_path");
			}
		}
	}
}
