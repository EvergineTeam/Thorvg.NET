using System;
using LowLevelDemo.Examples;
using LowLevelDemo.Infrastructure;

namespace LowLevelDemo
{
	/// <summary>
	/// Uncomment exactly one line to pick the example to run (VisualTests runner pattern).
	/// Each example reproduces one tile of ThorVG's README example_primitives.png; Showcase is
	/// the original animated composite demo.
	/// </summary>
	class Program
	{
		[STAThread]
		static int Main()
		{
			//// ### README TILES ###
			////return Runner.Run<AntialiasedShapesExample>();
			////return Runner.Run<GradientFillingExample>();
			////return Runner.Run<StrokingExample>();
			////return Runner.Run<PathClippingExample>();
			////return Runner.Run<SvgExample>();
			////return Runner.Run<MaskingExample>();
			////return Runner.Run<BlendingExample>();
			////return Runner.Run<TexturingExample>();
			return Runner.Run<TextExample>();
			////return Runner.Run<GaussianBlurExample>();
			////return Runner.Run<DropShadowExample>();
			////return Runner.Run<ColorReplacementExample>();

			//// ### EXTRAS ###
			////return Runner.Run<ShowcaseExample>();
		}
	}
}
