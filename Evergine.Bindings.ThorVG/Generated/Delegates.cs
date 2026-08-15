using System;
using System.Runtime.InteropServices;

namespace Evergine.Bindings.ThorVG
{
	/// <summary>
	/// This callback is invoked when a Picture requires an external asset
	/// (such as an image or font resource). Implementations should load the asset
	/// into the given
	/// object.
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public unsafe delegate bool Tvg_Picture_Asset_Resolver(IntPtr paint, [MarshalAs(UnmanagedType.LPUTF8Str)] string src, void* data);

	/// <summary>
	/// Applications can use this callback to synchronize external audio
	/// playback with the animation timeline.
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public unsafe delegate void Tvg_Audio_Resolver(Tvg_Audio_Info* info, void* data);
}
