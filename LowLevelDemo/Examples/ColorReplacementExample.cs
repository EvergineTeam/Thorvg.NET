using System;
using Evergine.Bindings.ThorVG;
using LowLevelDemo.Infrastructure;

namespace LowLevelDemo.Examples
{
	/// <summary>
	/// README tile "Color Replacement" — port of the color half of
	/// thorvg.example/src/SceneEffects.cpp: three tiger scenes recoloured with the scene-level
	/// effects tvg_scene_add_effect_fill (flat override), _tint (black/white remap) and _tritone
	/// (shadow/midtone/highlight). There is no per-paint recolour in the C API.
	/// </summary>
	public unsafe class ColorReplacementExample : ExampleBase
	{
		public override string Title => "Color Replacement";

		public override void Build(IntPtr canvas, uint width, uint height)
		{
			AddBackground(canvas, width, height, 255, 255, 255);
			IntPtr root = AddDesignRoot(canvas, width, height, 1200, 460);

			const float size = 400;

			// Fill: every pixel takes the flat colour, keeping only the alpha — the silhouette.
			IntPtr fill = MakeTigerScene(root, size, 0);
			Check(ThorVG.tvg_scene_add_effect_fill(fill, 0, 255, 0, 255), "effect_fill");
			AddLabel(root, "fill", 20, 410, 24, 40, 40, 40);

			// Tint: black maps to black, white maps to green.
			IntPtr tint = MakeTigerScene(root, size, 1);
			Check(ThorVG.tvg_scene_add_effect_tint(tint, 0, 0, 0, 0, 255, 0, 100.0), "effect_tint");
			AddLabel(root, "tint", size + 20, 410, 24, 40, 40, 40);

			// Tritone: green shadows, ochre midtones, white highlights.
			IntPtr tritone = MakeTigerScene(root, size, 2);
			Check(ThorVG.tvg_scene_add_effect_tritone(tritone, 0, 255, 0, 199, 110, 36, 255, 255, 255, 0), "effect_tritone");
			AddLabel(root, "tritone", (size * 2) + 20, 410, 24, 40, 40, 40);
		}

		private static IntPtr MakeTigerScene(IntPtr root, float size, int column)
		{
			IntPtr scene = ThorVG.tvg_scene_new();

			IntPtr picture = ThorVG.tvg_picture_new();
			Check(ThorVG.tvg_picture_load(picture, Asset("tiger.svg")), "load(tiger)");
			Check(ThorVG.tvg_picture_set_size(picture, size, size), "size(tiger)");
			Check(ThorVG.tvg_paint_translate(picture, size * column, 0), "translate(tiger)");
			Check(ThorVG.tvg_scene_add(scene, picture), "add(tiger)");

			Check(ThorVG.tvg_scene_add(root, scene), "add(scene)");
			return scene;
		}
	}
}
