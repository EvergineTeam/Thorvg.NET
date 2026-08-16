using System;
using Evergine.Bindings.ThorVG;
using LowLevelDemo.Infrastructure;

namespace LowLevelDemo.Examples
{
	/// <summary>
	/// README tile "Stroking" — port of thorvg.example/src/Stroke.cpp: stroke joins (bevel,
	/// round, miter), a width ramp, cap styles (round, square, butt), plus dashed strokes with
	/// different patterns and offsets (the tile shows dashed outlines prominently).
	/// </summary>
	public unsafe class StrokingExample : ExampleBase
	{
		public override string Title => "Stroking";

		public override void Build(IntPtr canvas, uint width, uint height)
		{
			AddBackground(canvas, width, height, 30, 30, 30);
			IntPtr root = AddDesignRoot(canvas, width, height, 800, 800);

			// ---- Stroke joins over filled rects ---------------------------------------------------
			AddJoinRect(root, 50, 50, Tvg_Stroke_Join.TVG_STROKE_JOIN_BEVEL);
			AddJoinRect(root, 300, 50, Tvg_Stroke_Join.TVG_STROKE_JOIN_ROUND);
			AddJoinRect(root, 550, 50, Tvg_Stroke_Join.TVG_STROKE_JOIN_MITER);

			// ---- Thin outlines over circles -------------------------------------------------------
			AddOutlinedCircle(root, 150, 400, 1);
			AddOutlinedCircle(root, 400, 400, 2);
			AddOutlinedCircle(root, 650, 400, 4);

			// ---- Width ramp with round caps ---------------------------------------------------------
			for (int i = 0; i < 10; i++)
			{
				AddLine(root, 50, 550 + (25 * i), 300, 550 + (25 * i), i + 1, Tvg_Stroke_Cap.TVG_STROKE_CAP_ROUND, null, 0);
			}

			// ---- Cap styles ---------------------------------------------------------------------
			AddLine(root, 360, 580, 450, 580, 15, Tvg_Stroke_Cap.TVG_STROKE_CAP_ROUND, null, 0);
			AddLine(root, 360, 630, 450, 630, 15, Tvg_Stroke_Cap.TVG_STROKE_CAP_SQUARE, null, 0);
			AddLine(root, 360, 680, 450, 680, 15, Tvg_Stroke_Cap.TVG_STROKE_CAP_BUTT, null, 0);

			// ---- Dashed strokes (the tile's signature look) ---------------------------------------
			AddLine(root, 500, 570, 750, 570, 4, Tvg_Stroke_Cap.TVG_STROKE_CAP_BUTT, new float[] { 10, 10 }, 0);
			AddLine(root, 500, 620, 750, 620, 4, Tvg_Stroke_Cap.TVG_STROKE_CAP_ROUND, new float[] { 1, 12 }, 0);
			AddLine(root, 500, 670, 750, 670, 6, Tvg_Stroke_Cap.TVG_STROKE_CAP_BUTT, new float[] { 20, 8, 4, 8 }, 6);

			IntPtr dashedRect = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_append_rect(dashedRect, 500, 710, 250, 70, 12, 12, true), "append_rect(dashed)");
			Check(ThorVG.tvg_shape_set_stroke_width(dashedRect, 3), "stroke_width(dashed rect)");
			Check(ThorVG.tvg_shape_set_stroke_color(dashedRect, 80, 220, 120, 255), "stroke_color(dashed rect)");
			fixed (float* dash = new float[] { 12f, 6f })
			{
				Check(ThorVG.tvg_shape_set_stroke_dash(dashedRect, dash, 2, 0), "stroke_dash(rect)");
			}

			Check(ThorVG.tvg_scene_add(root, dashedRect), "add(dashed rect)");
		}

		private static void AddJoinRect(IntPtr root, float x, float y, Tvg_Stroke_Join join)
		{
			IntPtr shape = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_append_rect(shape, x, y, 200, 200, 0, 0, true), "append_rect(join)");
			Check(ThorVG.tvg_shape_set_fill_color(shape, 50, 50, 50, 255), "fill(join)");
			Check(ThorVG.tvg_shape_set_stroke_color(shape, 255, 255, 255, 255), "stroke_color(join)");
			Check(ThorVG.tvg_shape_set_stroke_join(shape, join), "stroke_join");
			Check(ThorVG.tvg_shape_set_stroke_width(shape, 10), "stroke_width(join)");
			Check(ThorVG.tvg_scene_add(root, shape), "add(join)");
		}

		private static void AddOutlinedCircle(IntPtr root, float cx, float cy, float strokeWidth)
		{
			IntPtr shape = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_append_circle(shape, cx, cy, 100, 100, true), "append_circle(outline)");
			Check(ThorVG.tvg_shape_set_fill_color(shape, 50, 50, 50, 255), "fill(outline)");
			Check(ThorVG.tvg_shape_set_stroke_color(shape, 255, 255, 255, 255), "stroke_color(outline)");
			Check(ThorVG.tvg_shape_set_stroke_width(shape, strokeWidth), "stroke_width(outline)");
			Check(ThorVG.tvg_scene_add(root, shape), "add(outline)");
		}

		private static void AddLine(IntPtr root, float x1, float y1, float x2, float y2, float strokeWidth,
			Tvg_Stroke_Cap cap, float[] dash, float dashOffset)
		{
			IntPtr line = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_move_to(line, x1, y1), "move_to(line)");
			Check(ThorVG.tvg_shape_line_to(line, x2, y2), "line_to(line)");
			Check(ThorVG.tvg_shape_set_stroke_color(line, 255, 255, 255, 255), "stroke_color(line)");
			Check(ThorVG.tvg_shape_set_stroke_width(line, strokeWidth), "stroke_width(line)");
			Check(ThorVG.tvg_shape_set_stroke_cap(line, cap), "stroke_cap(line)");
			if (dash != null)
			{
				fixed (float* dashPtr = dash)
				{
					Check(ThorVG.tvg_shape_set_stroke_dash(line, dashPtr, (uint)dash.Length, dashOffset), "stroke_dash(line)");
				}
			}

			Check(ThorVG.tvg_scene_add(root, line), "add(line)");
		}
	}
}
