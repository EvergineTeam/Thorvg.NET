using CppAst;
using System;
using System.IO;
using System.Linq;

namespace ThorvgGen
{
	class Program
	{
		static int Main(string[] args)
		{
			var headerFile = Path.Combine(AppContext.BaseDirectory, "Headers", "thorvg_capi.h");

			if (!File.Exists(headerFile))
			{
				Console.Error.WriteLine($"Header not found: {headerFile}");
				return 1;
			}

			var options = new CppParserOptions
			{
				ParseMacros = true,
			};

			// Stand-ins for <stdint.h> and <stdbool.h>, the only two headers thorvg_capi.h
			// includes. libclang ships no libc and finds no include paths at all on a bare Linux
			// runner, so without these the parse dies there while succeeding on Windows, where the
			// Windows SDK happens to be on the search path. See Headers/libc-stubs/stdint.h.
			options.IncludeFolders.Add(Path.Combine(AppContext.BaseDirectory, "Headers", "libc-stubs"));

			// TVG_API expands to __declspec(dllimport) unless the static build is selected, which
			// would hang an import attribute on every declaration in the AST.
			options.Defines.Add("TVG_STATIC");

			var compilation = CppParser.ParseFile(headerFile, options);

			if (compilation.HasErrors)
			{
				foreach (var message in compilation.Diagnostics.Messages)
				{
					if (message.Type == CppLogMessageType.Error)
					{
						Console.Error.WriteLine(message);
					}
				}

				return 1;
			}

			var outputPath = ResolveOutputPath();
			if (outputPath == null)
			{
				Console.Error.WriteLine("Could not locate the Evergine.Bindings.ThorVG project folder.");
				return 1;
			}

			Directory.CreateDirectory(outputPath);

			CsCodeGenerator.Instance.Generate(compilation, outputPath);

			Console.WriteLine($"Bindings written to {outputPath}");
			return 0;
		}

		/// <summary>
		/// Walks up from the build output until the sibling binding project is found, instead of
		/// hard-coding a fixed number of parent hops, which breaks whenever the RuntimeIdentifier
		/// or the publish layout changes the output depth.
		/// </summary>
		private static string ResolveOutputPath()
		{
			var current = new DirectoryInfo(AppContext.BaseDirectory);

			while (current != null)
			{
				var candidate = Path.Combine(current.FullName, "Evergine.Bindings.ThorVG");
				if (Directory.Exists(candidate))
				{
					return Path.Combine(candidate, "Generated");
				}

				current = current.Parent;
			}

			return null;
		}
	}
}
