using System;
using System.IO;
using System.Runtime.InteropServices;
using Evergine.Bindings.ThorVG;

namespace LowLevelDemo.Infrastructure
{
	/// <summary>
	/// ProjectReference scenario: NuGet consumers get runtimes/&lt;rid&gt;/native wired into the
	/// probing paths for free, a project reference does not.
	/// </summary>
	internal static class NativeResolver
	{
		private static bool installed;

		public static void Install()
		{
			if (installed)
			{
				return;
			}

			installed = true;
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
		}
	}
}
