using System;
using Evergine.Bindings.ThorVG;
using LowLevelDemo.Infrastructure;

namespace LowLevelDemo.Examples
{
	/// <summary>
	/// README tile "Drop-Shadow" — port of thorvg.example/src/EffectDropShadow.cpp: three SVG
	/// scenes, each with tvg_scene_add_effect_drop_shadow (color, angle, distance, blur sigma,
	/// quality). Effects are scene-level, so each picture sits in its own scene.
	/// </summary>
	public unsafe class DropShadowExample : ExampleBase
	{
		public override string Title => "Drop-Shadow";

		public override void Build(IntPtr canvas, uint width, uint height)
		{
			AddBackground(canvas, width, height, 255, 255, 255);
			IntPtr root = AddDesignRoot(canvas, width, height, 800, 800);

			// Soft black shadow under the ThorVG logo.
			IntPtr scene1 = MakeSvgScene(root, "thorvg-logo-clear.svg", 0.6f, 400, 50);
			Check(ThorVG.tvg_scene_add_effect_drop_shadow(scene1, 0, 0, 0, 125, 120.0, 20.0, 7.0, 100), "drop_shadow(1)");

			// Hard blue shadow, no blur.
			IntPtr scene2 = MakeSvgScene(root, "cartman.svg", 3.0f, 330, 250);
			Check(ThorVG.tvg_scene_add_effect_drop_shadow(scene2, 65, 143, 222, 255, 135.0, 10.0, 3.0, 100), "drop_shadow(2)");

			// Sharp offset shadow on overlapping circles.
			IntPtr scene3 = MakeSvgScene(root, "circles1.svg", 0.7f, 400, 550);
			Check(ThorVG.tvg_scene_add_effect_drop_shadow(scene3, 0, 0, 0, 125, 45.0, 20.0, 0.0, 100), "drop_shadow(3)");
		}

		private static IntPtr MakeSvgScene(IntPtr root, string file, float scale, float cx, float y)
		{
			IntPtr scene = ThorVG.tvg_scene_new();

			IntPtr picture = ThorVG.tvg_picture_new();
			Check(ThorVG.tvg_picture_load(picture, Asset(file)), $"load({file})");
			Check(ThorVG.tvg_paint_scale(picture, scale), $"scale({file})");
			Check(ThorVG.tvg_picture_set_origin(picture, 0.5f, 0.0f), $"origin({file})");
			Check(ThorVG.tvg_paint_translate(picture, cx, y), $"translate({file})");

			Check(ThorVG.tvg_scene_add(scene, picture), $"add({file})");
			Check(ThorVG.tvg_scene_add(root, scene), "add(scene)");
			return scene;
		}
	}
}
