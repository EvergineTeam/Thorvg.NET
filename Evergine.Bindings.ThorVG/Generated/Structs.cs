using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Evergine.Bindings.ThorVG
{

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct Tvg_Point
	{
		public float x;
		public float y;
	}

	/// <summary>
	/// The elements e11, e12, e21 and e22 represent the rotation matrix, including the scaling factor.
	/// The elements e13 and e23 determine the translation of the object along the x and y-axis, respectively.
	/// The elements e31 and e32 are set to 0, e33 is set to 1.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct Tvg_Matrix
	{
		public float e11;
		public float e12;
		public float e13;
		public float e21;
		public float e22;
		public float e23;
		public float e31;
		public float e32;
		public float e33;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct Tvg_Color_Stop
	{
		/// <summary>
		/// The relative position of the color.
		/// </summary>
		public float offset;
		/// <summary>
		/// The red color channel value in the range [0 ~ 255].
		/// </summary>
		public byte r;
		/// <summary>
		/// The green color channel value in the range [0 ~ 255].
		/// </summary>
		public byte g;
		/// <summary>
		/// The blue color channel value in the range [0 ~ 255].
		/// </summary>
		public byte b;
		/// <summary>
		/// The alpha channel value in the range [0 ~ 255], where 0 is completely transparent and 255 is opaque.
		/// </summary>
		public byte a;
	}

	/// <summary>
	/// Provides the basic vertical layout metrics used for text rendering,
	/// such as ascent, descent, and line spacing (linegap).
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct Tvg_Text_Metrics
	{
		/// <summary>
		/// Distance from the baseline to the top of the highest glyph (usually positive).
		/// </summary>
		public float ascent;
		/// <summary>
		/// Distance from the baseline to the bottom of the lowest glyph (usually negative, as in TTF).
		/// </summary>
		public float descent;
		/// <summary>
		/// Additional spacing recommended between lines (leading).
		/// </summary>
		public float linegap;
		/// <summary>
		/// The total vertical advance between lines of text: ascent - descent + linegap (i.e., ascent + |descent| + linegap when descent is negative).
		/// </summary>
		public float advance;
	}

	/// <summary>
	/// Provides the basic layout metrics used for positioning an individual glyph,
	/// including its advance along the baseline direction, bearing relative to the
	/// inline axis origin, and its bounding box in local glyph space.
	/// The advance value represents the distance the pen position moves along the
	/// baseline (inline direction), regardless of whether the text is laid out
	/// horizontally or vertically.
	/// The bounding box is defined in the glyph’s local coordinate space and is
	/// independent of any layout direction or transformation.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct Tvg_Glyph_Metrics
	{
		/// <summary>
		/// The advance distance along the baseline (inline) direction.
		/// </summary>
		public float advance;
		/// <summary>
		/// The bearing from the origin to the glyph’s visible bound along the inline-start direction.
		/// </summary>
		public float bearing;
		/// <summary>
		/// The minimum point of the glyph bounding box in local space.
		/// </summary>
		public Tvg_Point min;
		/// <summary>
		/// The maximum point of the glyph bounding box in local space.
		/// </summary>
		public Tvg_Point max;
	}

	/// <summary>
	/// This structure contains the WebGPU objects used to initialize the rendering backend.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct Tvg_WgContext
	{
		/// <summary>
		/// WGPUInstance, context for all other wgpu objects.
		/// </summary>
		public void* instance;
		/// <summary>
		/// WGPUAdapter, the adapter associated with the rendering device.
		/// </summary>
		public void* adapter;
		/// <summary>
		/// WGPUDevice, a desired handle for the wgpu device.
		/// </summary>
		public void* device;
	}

	/// <summary>
	/// This structure is provided to the audio resolver callback and contains
	/// the information required to synchronize audio playback with the animation
	/// timeline. Applications are responsible for managing audio playback using
	/// their own audio engine.
	/// Example:
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct Tvg_Audio_Info
	{
		/// <summary>
		/// Audio source: a file path/URL or embedded raw bytes.
		/// </summary>
		public byte* src;
		/// <summary>
		/// MIME type string; valid when
		/// may be
		/// </summary>
		public byte* mimeType;
		/// <summary>
		/// Embedded data size in bytes; valid when
		/// </summary>
		public uint size;
		/// <summary>
		/// Position within the audio file in seconds; valid when
		/// </summary>
		public float offset;
		/// <summary>
		/// Volume [0, 100]; valid when
		/// </summary>
		public float volume;
		/// <summary>
		/// while the layer is within its playback range.
		/// </summary>
		public byte active;
		/// <summary>
		/// if
		/// points to embedded audio data;
		/// if it is a file path or URL.
		/// </summary>
		public byte embedded;
	}
}
