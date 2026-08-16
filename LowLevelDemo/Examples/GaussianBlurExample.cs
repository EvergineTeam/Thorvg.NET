using System;
using Evergine.Bindings.ThorVG;
using LowLevelDemo.Infrastructure;

namespace LowLevelDemo.Examples
{
	/// <summary>
	/// README tile "Gaussian-Blur" — port of the blur half of thorvg.example/src/SceneEffects.cpp:
	/// three tiger scenes with tvg_scene_add_effect_gaussian_blur in both, horizontal-only and
	/// vertical-only directions. Effects are scene-level, so each tiger sits in its own scene.
	/// </summary>
	public unsafe class GaussianBlurExample : ExampleBase
	{
		private static readonly string[] Labels = { "direction: both", "horizontal", "vertical" };

		public override string Title => "Gaussian-Blur";

		public override void Build(IntPtr canvas, uint width, uint height)
		{
			AddBackground(canvas, width, height, 255, 255, 255);
			IntPtr root = AddDesignRoot(canvas, width, height, 1200, 460);

			const float size = 400;
			for (int direction = 0; direction < 3; direction++)
			{
				IntPtr scene = ThorVG.tvg_scene_new();

				IntPtr picture = ThorVG.tvg_picture_new();
				Check(ThorVG.tvg_picture_load(picture, Asset("tiger.svg")), "load(tiger)");
				Check(ThorVG.tvg_picture_set_size(picture, size, size), "size(tiger)");
				Check(ThorVG.tvg_paint_translate(picture, size * direction, 0), "translate(tiger)");
				Check(ThorVG.tvg_scene_add(scene, picture), "add(tiger)");

				// (sigma, direction: 0 both / 1 horizontal / 2 vertical, border: 0 duplicate, quality 0-100)
				Check(ThorVG.tvg_scene_add_effect_gaussian_blur(scene, 6.0, direction, 0, 100), "gaussian_blur");

				Check(ThorVG.tvg_scene_add(root, scene), "add(scene)");
				AddLabel(root, Labels[direction], (size * direction) + 20, 410, 24, 40, 40, 40);
			}
		}
	}
}
