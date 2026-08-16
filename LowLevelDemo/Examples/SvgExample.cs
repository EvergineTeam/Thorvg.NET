using System;
using Evergine.Bindings.ThorVG;
using LowLevelDemo.Infrastructure;

namespace LowLevelDemo.Examples
{
	/// <summary>
	/// README tile "SVG" — port of thorvg.example/src/Svg.cpp: a grid of SVG files loaded with
	/// tvg_picture_load, each scaled preserving its aspect ratio via tvg_picture_get_size and
	/// centred in its cell with tvg_picture_set_origin.
	/// </summary>
	public unsafe class SvgExample : ExampleBase
	{
		private static readonly string[] Files =
		{
			"tiger.svg", "car.svg", "cartman.svg",
			"thorvg-logo-clear.svg", "circles1.svg",
		};

		public override string Title => "SVG";

		public override void Build(IntPtr canvas, uint width, uint height)
		{
			AddBackground(canvas, width, height, 150, 150, 150);

			const int perRow = 3;
			const int rows = 2;
			float cellW = (float)width / perRow;
			float cellH = (float)height / rows;
			float size = MathF.Min(cellW, cellH);

			for (int i = 0; i < Files.Length; i++)
			{
				IntPtr picture = ThorVG.tvg_picture_new();
				Check(ThorVG.tvg_picture_set_origin(picture, 0.5f, 0.5f), $"set_origin({Files[i]})");
				Check(ThorVG.tvg_picture_load(picture, Asset(Files[i])), $"load({Files[i]})");

				// Scale preserving the aspect ratio, exactly as upstream.
				float w = 0, h = 0;
				Check(ThorVG.tvg_picture_get_size(picture, &w, &h), $"get_size({Files[i]})");
				Check(ThorVG.tvg_paint_scale(picture, (w > h) ? size / w : size / h), $"scale({Files[i]})");

				float cx = ((i % perRow) * cellW) + (cellW / 2f);
				float cy = ((i / perRow) * cellH) + (cellH / 2f);
				Check(ThorVG.tvg_paint_translate(picture, cx, cy), $"translate({Files[i]})");

				Check(ThorVG.tvg_canvas_add(canvas, picture), $"add({Files[i]})");
			}
		}
	}
}
