using Evergine.Bindings.ThorVG;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Test
{
	/// <summary>
	/// Validates the generated bindings against the shipped native library: the managed and native
	/// versions must agree, and a real render has to put pixels in a buffer. Building the binding
	/// only proves it is well-formed; nothing here is checked by the compiler.
	/// </summary>
	unsafe class Program
	{
		private const int Width = 64;
		private const int Height = 64;

		private const string Svg =
			"<svg viewBox=\"0 0 64 64\" xmlns=\"http://www.w3.org/2000/svg\">" +
			"<circle cx=\"32\" cy=\"32\" r=\"28\" fill=\"#3498db\"/></svg>";

		static int Main()
		{
			// ProjectReference scenario: NuGet consumers get runtimes/<rid>/native wired into the
			// probing paths for free, a project reference does not.
			NativeLibrary.SetDllImportResolver(typeof(Tvg_Point).Assembly, (name, assembly, searchPath) =>
			{
				var folder = Path.Combine(Path.GetDirectoryName(assembly.Location), "runtimes",
					$"win-{RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()}", "native");
				if (Directory.Exists(folder))
				{
					foreach (var file in Directory.GetFiles(folder, $"*{name}*"))
					{
						if (NativeLibrary.TryLoad(file, out var handle))
						{
							return handle;
						}
					}
				}

				return NativeLibrary.Load(name, assembly, searchPath);
			});

			int failures = 0;

			// ---- Version parity ---------------------------------------------------------------
			uint major, minor, micro;
			byte* versionText;
			var result = ThorVG.tvg_engine_version(&major, &minor, &micro, &versionText);
			failures += Check("tvg_engine_version", result, Tvg_Result.TVG_RESULT_SUCCESS);

			Console.WriteLine($"header  : {ThorVG.TVG_VERSION_MAJOR}.{ThorVG.TVG_VERSION_MINOR}.{ThorVG.TVG_VERSION_MICRO}");
			Console.WriteLine($"library : {major}.{minor}.{micro} ({Marshal.PtrToStringUTF8((IntPtr)versionText)})");

			if (major != ThorVG.TVG_VERSION_MAJOR || minor != ThorVG.TVG_VERSION_MINOR || micro != ThorVG.TVG_VERSION_MICRO)
			{
				Console.Error.WriteLine("FAIL: the native library does not match the header the bindings came from.");
				failures++;
			}

			// ---- Engine + software canvas over a managed buffer -------------------------------
			failures += Check("tvg_engine_init", ThorVG.tvg_engine_init(0), Tvg_Result.TVG_RESULT_SUCCESS);

			IntPtr canvas = ThorVG.tvg_swcanvas_create(Tvg_Engine_Option.TVG_ENGINE_OPTION_DEFAULT);
			if (canvas == IntPtr.Zero)
			{
				Console.Error.WriteLine("FAIL: tvg_swcanvas_create returned null.");
				return 1;
			}

			var buffer = new uint[Width * Height];

			fixed (uint* pixels = buffer)
			{
				failures += Check("tvg_swcanvas_set_target",
					ThorVG.tvg_swcanvas_set_target(canvas, pixels, Width, Width, Height, Tvg_Colorspace.TVG_COLORSPACE_ARGB8888),
					Tvg_Result.TVG_RESULT_SUCCESS);

				// A filled rectangle covering the left half.
				IntPtr shape = ThorVG.tvg_shape_new();
				failures += Check("tvg_shape_append_rect",
					ThorVG.tvg_shape_append_rect(shape, 0, 0, Width / 2f, Height, 0, 0, true),
					Tvg_Result.TVG_RESULT_SUCCESS);
				failures += Check("tvg_shape_set_fill_color",
					ThorVG.tvg_shape_set_fill_color(shape, 255, 0, 0, 255),
					Tvg_Result.TVG_RESULT_SUCCESS);
				failures += Check("tvg_canvas_add", ThorVG.tvg_canvas_add(canvas, shape), Tvg_Result.TVG_RESULT_SUCCESS);

				// An SVG parsed from memory, which exercises UTF-8 string marshalling and the
				// copy flag on a caller-owned buffer.
				IntPtr picture = ThorVG.tvg_picture_new();
				var svgBytes = Encoding.UTF8.GetBytes(Svg);
				fixed (byte* svg = svgBytes)
				{
					failures += Check("tvg_picture_load_data",
						ThorVG.tvg_picture_load_data(picture, svg, (uint)svgBytes.Length, "image/svg+xml", null, true),
						Tvg_Result.TVG_RESULT_SUCCESS);
				}

				failures += Check("tvg_canvas_add(picture)", ThorVG.tvg_canvas_add(canvas, picture), Tvg_Result.TVG_RESULT_SUCCESS);
				failures += Check("tvg_canvas_draw", ThorVG.tvg_canvas_draw(canvas, true), Tvg_Result.TVG_RESULT_SUCCESS);
				failures += Check("tvg_canvas_sync", ThorVG.tvg_canvas_sync(canvas), Tvg_Result.TVG_RESULT_SUCCESS);
			}

			// ---- The render actually produced pixels ------------------------------------------
			int painted = 0;
			foreach (var pixel in buffer)
			{
				if ((pixel & 0x00FFFFFF) != 0)
				{
					painted++;
				}
			}

			Console.WriteLine($"painted : {painted} of {buffer.Length} pixels");
			Console.WriteLine($"corners : TL=0x{buffer[0]:X8}  TR=0x{buffer[Width - 1]:X8}  centre=0x{buffer[(Height / 2 * Width) + (Width / 2)]:X8}");

			if (painted < buffer.Length / 4)
			{
				Console.Error.WriteLine($"FAIL: only {painted} pixels were painted; the canvas is essentially empty.");
				failures++;
			}

			ThorVG.tvg_canvas_destroy(canvas);
			failures += Check("tvg_engine_term", ThorVG.tvg_engine_term(), Tvg_Result.TVG_RESULT_SUCCESS);

			Console.WriteLine();
			Console.WriteLine(failures == 0 ? "OK: all checks passed." : $"{failures} check(s) FAILED.");
			return failures == 0 ? 0 : 1;
		}

		private static int Check(string name, Tvg_Result actual, Tvg_Result expected)
		{
			if (actual != expected)
			{
				Console.Error.WriteLine($"FAIL: {name} returned {actual}, expected {expected}");
				return 1;
			}

			return 0;
		}
	}
}
