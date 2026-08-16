using Evergine.Bindings.ThorVG;
using Evergine.Common.Graphics;
using Evergine.DirectX11;
using Evergine.Forms;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace LowLevelDemo
{
	/// <summary>
	/// Real-time demo: ThorVG rasterises an animated vector scene — procedural shapes plus a
	/// looping Lottie — into a CPU buffer every frame, and the Evergine low-level graphics API
	/// uploads that buffer to a texture and presents it with a fullscreen triangle.
	///
	/// The roles are the inverse of the MuJoCo.NET demo: there Evergine drew the geometry and the
	/// native library supplied transforms; here the native library supplies finished pixels and
	/// Evergine's job is the CPU-to-GPU bridge, the same pattern as the engine's video player.
	/// </summary>
	unsafe class Program
	{
		private const int RenderWidth = 1280;
		private const int RenderHeight = 720;

		// ThorVG side.
		private static IntPtr canvas;
		private static IntPtr background;       // radial gradient rect, rebuilt per frame
		private static IntPtr star;             // path-command star
		private static IntPtr arc;              // dashed stroked arc
		private static IntPtr[] petals;         // ring of rounded rects
		private static IntPtr animation;        // the Lottie
		private static float lottieTotalFrames;
		private static float lottieDuration;
		private static uint[] pixels;
		private static GCHandle pixelsHandle;

		// Evergine side.
		private static Window window;
		private static SwapChain swapChain;
		private static GraphicsContext graphics;
		private static FrameBuffer frameBuffer;
		private static CommandQueue commandQueue;
		private static GraphicsPipelineState pipeline;
		private static ResourceSet resourceSet;
		private static ResourceLayout resourceLayout;
		private static SamplerState sampler;
		private static Texture texture;
		private static Viewport[] viewports;
		private static Evergine.Mathematics.Rectangle[] scissors;
		private static bool surfaceResized;

		private static Stopwatch clock;
		private static Stopwatch fpsTimer;
		private static int fpsFrames;

		[STAThread]
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

			// ---- ThorVG ------------------------------------------------------------------------
			Check(ThorVG.tvg_engine_init(4), "tvg_engine_init");

			canvas = ThorVG.tvg_swcanvas_create(Tvg_Engine_Option.TVG_ENGINE_OPTION_DEFAULT);
			if (canvas == IntPtr.Zero)
			{
				Console.Error.WriteLine("tvg_swcanvas_create returned null.");
				return 1;
			}

			CreateRenderTargetBuffer();
			BuildScene();

			// ---- Evergine low-level: window + swapchain (DesktopUtils pattern) -----------------
			var windowSystem = new FormsWindowsSystem();
			window = windowSystem.CreateWindow("Thorvg.NET - Evergine low-level", RenderWidth, RenderHeight);
			window.OnScreenSizeChanged += (s, e) => surfaceResized = true;

			var swapChainDescription = new SwapChainDescription()
			{
				Width = window.Width,
				Height = window.Height,
				SurfaceInfo = window.SurfaceInfo,
				ColorTargetFormat = PixelFormat.R8G8B8A8_UNorm,
				ColorTargetFlags = TextureFlags.RenderTarget | TextureFlags.ShaderResource,
				DepthStencilTargetFormat = PixelFormat.D24_UNorm_S8_UInt,
				DepthStencilTargetFlags = TextureFlags.DepthStencil,
				SampleCount = TextureSampleCount.None,
				IsWindowed = true,
				RefreshRate = 60,
			};

			graphics = new DX11GraphicsContext();
			graphics.CreateDevice(new ValidationLayer(ValidationLayer.NotifyMethod.Trace));
			swapChain = graphics.CreateSwapChain(swapChainDescription);
			swapChain.VerticalSync = true;

			windowSystem.Run(Load, Draw);

			// ---- Teardown ----------------------------------------------------------------------
			ThorVG.tvg_canvas_destroy(canvas);
			ThorVG.tvg_animation_del(animation);
			ThorVG.tvg_engine_term();
			pixelsHandle.Free();
			graphics.Dispose();
			return 0;
		}

		private static void Load()
		{
			frameBuffer = swapChain.FrameBuffer;

			// Fullscreen triangle: no vertex buffer, no input layout (ComputeTextureTest pattern).
			var vsBytes = graphics.ShaderCompile(Shaders.Hlsl, "VS", ShaderStages.Vertex).ByteCode;
			var psBytes = graphics.ShaderCompile(Shaders.Hlsl, "PS", ShaderStages.Pixel).ByteCode;
			var vsDescription = new ShaderDescription(ShaderStages.Vertex, "VS", vsBytes);
			var psDescription = new ShaderDescription(ShaderStages.Pixel, "PS", psBytes);
			var vertexShader = graphics.Factory.CreateShader(ref vsDescription);
			var pixelShader = graphics.Factory.CreateShader(ref psDescription);

			var samplerDescription = SamplerStates.LinearClamp;
			sampler = graphics.Factory.CreateSamplerState(ref samplerDescription);

			var layoutDescription = new ResourceLayoutDescription(
				new LayoutElementDescription(0, ResourceType.TextureView, ShaderStages.Pixel),
				new LayoutElementDescription(0, ResourceType.Sampler, ShaderStages.Pixel));
			resourceLayout = graphics.Factory.CreateResourceLayout(ref layoutDescription);

			CreateTextureAndResourceSet();

			var pipelineDescription = new GraphicsPipelineDescription()
			{
				PrimitiveTopology = PrimitiveTopology.TriangleList,
				InputLayouts = null,
				ResourceLayouts = new[] { resourceLayout },
				Shaders = new GraphicsShaderStateDescription()
				{
					VertexShader = vertexShader,
					PixelShader = pixelShader,
				},
				RenderStates = new RenderStateDescription()
				{
					RasterizerState = RasterizerStates.CullBack,
					BlendState = BlendStates.Opaque,
					DepthStencilState = DepthStencilStates.None,
				},
				Outputs = frameBuffer.OutputDescription,
			};
			pipeline = graphics.Factory.CreateGraphicsPipeline(ref pipelineDescription);
			commandQueue = graphics.Factory.CreateCommandQueue();

			UpdateSurfaceSize();
			clock = Stopwatch.StartNew();
			fpsTimer = Stopwatch.StartNew();
		}

		private static void Draw()
		{
			if (surfaceResized)
			{
				surfaceResized = false;
				swapChain.ResizeSwapChain(window.Width, window.Height);
				frameBuffer = swapChain.FrameBuffer;
				UpdateSurfaceSize();
			}

			swapChain.InitFrame();

			float t = (float)clock.Elapsed.TotalSeconds;

			// ---- ThorVG: animate and rasterise into the CPU buffer -----------------------------
			AnimateScene(t);

			Check(ThorVG.tvg_canvas_update(canvas), "tvg_canvas_update");
			Check(ThorVG.tvg_canvas_draw(canvas, true), "tvg_canvas_draw");
			Check(ThorVG.tvg_canvas_sync(canvas), "tvg_canvas_sync");

			// ---- Evergine: upload and present ---------------------------------------------------
			// The last parameter is the subresource index, despite the managed overloads calling
			// it an offset; Default-usage textures go through UpdateSubresource on DX11, which is
			// the per-frame-safe path (Dynamic maps with a flat copy that ignores row pitch).
			graphics.UpdateTextureData(texture, pixelsHandle.AddrOfPinnedObject(), (uint)(RenderWidth * RenderHeight * 4), 0);

			var commandBuffer = commandQueue.CommandBuffer();
			commandBuffer.Begin();

			var renderPassDescription = new RenderPassDescription(frameBuffer, ClearValue.Default);
			commandBuffer.BeginRenderPass(ref renderPassDescription);
			commandBuffer.SetGraphicsPipelineState(pipeline);
			commandBuffer.SetResourceSet(resourceSet);
			commandBuffer.SetViewports(viewports);
			commandBuffer.SetScissorRectangles(scissors);
			commandBuffer.Draw(3);
			commandBuffer.EndRenderPass();
			commandBuffer.End();
			commandBuffer.Commit();

			commandQueue.Submit();
			commandQueue.WaitIdle();

			swapChain.Present();
			CountFps();
		}

		// ------------------------------------------------------------------------------------------
		// Scene
		// ------------------------------------------------------------------------------------------

		private const int PetalCount = 10;
		private const float Cx = RenderWidth / 2f;
		private const float Cy = RenderHeight / 2f;

		private static void BuildScene()
		{
			// Radial-gradient background. The gradient handle is owned by the shape after
			// tvg_shape_set_gradient, so no separate teardown.
			background = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_canvas_add(canvas, background), "tvg_canvas_add(background)");

			// Ring of petals: rounded rects rotated around the centre via Tvg_Matrix.
			petals = new IntPtr[PetalCount];
			for (int i = 0; i < PetalCount; i++)
			{
				petals[i] = ThorVG.tvg_shape_new();
				Check(ThorVG.tvg_shape_append_rect(petals[i], -18, -170, 36, 110, 18, 18, true), "tvg_shape_append_rect(petal)");
				Check(ThorVG.tvg_canvas_add(canvas, petals[i]), "tvg_canvas_add(petal)");
			}

			// A five-pointed star from raw path commands: the two parallel arrays are the most
			// delicate marshalling case in the API.
			star = ThorVG.tvg_shape_new();
			AppendStarPath(star, outer: 90, inner: 36);
			Check(ThorVG.tvg_shape_set_fill_color(star, 255, 214, 64, 255), "tvg_shape_set_fill_color(star)");
			Check(ThorVG.tvg_canvas_add(canvas, star), "tvg_canvas_add(star)");

			// A dashed, round-capped stroked arc, counter-rotating.
			arc = ThorVG.tvg_shape_new();
			Check(ThorVG.tvg_shape_append_circle(arc, 0, 0, 240, 240, true), "tvg_shape_append_circle(arc)");
			Check(ThorVG.tvg_shape_set_stroke_width(arc, 14), "tvg_shape_set_stroke_width");
			Check(ThorVG.tvg_shape_set_stroke_color(arc, 90, 200, 250, 255), "tvg_shape_set_stroke_color");
			Check(ThorVG.tvg_shape_set_stroke_cap(arc, Tvg_Stroke_Cap.TVG_STROKE_CAP_ROUND), "tvg_shape_set_stroke_cap");
			fixed (float* dash = new float[] { 38f, 26f })
			{
				Check(ThorVG.tvg_shape_set_stroke_dash(arc, dash, 2, 0), "tvg_shape_set_stroke_dash");
			}

			Check(ThorVG.tvg_canvas_add(canvas, arc), "tvg_canvas_add(arc)");

			// The Lottie, loaded from memory: the byte* payload path. The picture belongs to the
			// animation; it is sized and centred once and driven by frame number afterwards.
			animation = ThorVG.tvg_animation_new();
			IntPtr picture = ThorVG.tvg_animation_get_picture(animation);

			var lottie = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Assets", "animation.lot"));
			fixed (byte* data = lottie)
			{
				Check(ThorVG.tvg_picture_load_data(picture, data, (uint)lottie.Length, "lottie", null, true), "tvg_picture_load_data");
			}

			float total = 0, duration = 0;
			Check(ThorVG.tvg_animation_get_total_frame(animation, &total), "tvg_animation_get_total_frame");
			Check(ThorVG.tvg_animation_get_duration(animation, &duration), "tvg_animation_get_duration");
			lottieTotalFrames = total;
			lottieDuration = duration;
			Console.WriteLine($"lottie: {total} frames, {duration:F2}s");

			const float lottieSize = 340f;
			Check(ThorVG.tvg_picture_set_size(picture, lottieSize, lottieSize), "tvg_picture_set_size");
			Check(ThorVG.tvg_paint_translate(picture, Cx - (lottieSize / 2f), Cy - (lottieSize / 2f)), "tvg_paint_translate(picture)");
			Check(ThorVG.tvg_canvas_add(canvas, picture), "tvg_canvas_add(picture)");
		}

		private static void AnimateScene(float t)
		{
			// Background: rebuild the gradient with a slowly breathing radius. Gradients are
			// consumed by the shape, so a fresh one per frame is the intended usage.
			Check(ThorVG.tvg_shape_reset(background), "tvg_shape_reset(background)");
			Check(ThorVG.tvg_shape_append_rect(background, 0, 0, RenderWidth, RenderHeight, 0, 0, true), "tvg_shape_append_rect(background)");

			IntPtr gradient = ThorVG.tvg_radial_gradient_new();
			float radius = 520f + (90f * MathF.Sin(t * 0.7f));
			Check(ThorVG.tvg_radial_gradient_set(gradient, Cx, Cy, radius, Cx, Cy, 0), "tvg_radial_gradient_set");

			var stops = new Tvg_Color_Stop[]
			{
				new() { offset = 0.0f, r = 38,  g = 44,  b = 78,  a = 255 },
				new() { offset = 0.6f, r = 24,  g = 26,  b = 48,  a = 255 },
				new() { offset = 1.0f, r = 10,  g = 10,  b = 22,  a = 255 },
			};
			fixed (Tvg_Color_Stop* stopsPtr = stops)
			{
				Check(ThorVG.tvg_gradient_set_color_stops(gradient, stopsPtr, (uint)stops.Length), "tvg_gradient_set_color_stops");
			}

			Check(ThorVG.tvg_shape_set_gradient(background, gradient), "tvg_shape_set_gradient");

			// Petals: each with its own rotation matrix around the centre.
			for (int i = 0; i < PetalCount; i++)
			{
				float angle = (360f / PetalCount * i) + (t * 24f);
				float radians = angle * MathF.PI / 180f;
				float cos = MathF.Cos(radians);
				float sin = MathF.Sin(radians);

				// Rotate about the origin, then translate to the centre (row-major, applied to
				// column vectors: translation lives in e13/e23).
				var m = new Tvg_Matrix
				{
					e11 = cos, e12 = -sin, e13 = Cx,
					e21 = sin, e22 = cos,  e23 = Cy,
					e31 = 0,   e32 = 0,    e33 = 1,
				};
				Check(ThorVG.tvg_paint_set_transform(petals[i], &m), "tvg_paint_set_transform(petal)");

				byte hue = (byte)(120 + (110 * i / PetalCount));
				Check(ThorVG.tvg_shape_set_fill_color(petals[i], (byte)(60 + (12 * i)), hue, 220, 200), "tvg_shape_set_fill_color(petal)");
			}

			// Star: gentle spin one way...
			var starMatrix = Rotation(Cx, Cy - 0f, -t * 40f);
			Check(ThorVG.tvg_paint_set_transform(star, &starMatrix), "tvg_paint_set_transform(star)");

			// ...arc: dashed ring spinning the other way.
			var arcMatrix = Rotation(Cx, Cy, t * 18f);
			Check(ThorVG.tvg_paint_set_transform(arc, &arcMatrix), "tvg_paint_set_transform(arc)");

			// The Lottie loops at its own frame rate.
			if (lottieTotalFrames > 0 && lottieDuration > 0)
			{
				float frame = (t % lottieDuration) / lottieDuration * lottieTotalFrames;
				Check(ThorVG.tvg_animation_set_frame(animation, frame), "tvg_animation_set_frame");
			}
		}

		private static Tvg_Matrix Rotation(float cx, float cy, float degrees)
		{
			float radians = degrees * MathF.PI / 180f;
			float cos = MathF.Cos(radians);
			float sin = MathF.Sin(radians);
			return new Tvg_Matrix
			{
				e11 = cos, e12 = -sin, e13 = cx,
				e21 = sin, e22 = cos,  e23 = cy,
				e31 = 0,   e32 = 0,    e33 = 1,
			};
		}

		private static void AppendStarPath(IntPtr shape, float outer, float inner)
		{
			const int points = 5;
			var cmds = new byte[points * 2 + 2];
			var pts = new Tvg_Point[points * 2 + 1];

			int c = 0, p = 0;
			cmds[c++] = ThorVG.TVG_PATH_COMMAND_MOVE_TO;
			for (int i = 0; i < points * 2; i++)
			{
				float radius = (i % 2 == 0) ? outer : inner;
				float angle = (MathF.PI * i / points) - (MathF.PI / 2);
				var point = new Tvg_Point { x = radius * MathF.Cos(angle), y = radius * MathF.Sin(angle) };

				if (i == 0)
				{
					pts[p++] = point;
				}
				else
				{
					cmds[c++] = ThorVG.TVG_PATH_COMMAND_LINE_TO;
					pts[p++] = point;
				}
			}

			cmds[c++] = ThorVG.TVG_PATH_COMMAND_CLOSE;

			fixed (byte* cmdsPtr = cmds)
			fixed (Tvg_Point* ptsPtr = pts)
			{
				Check(ThorVG.tvg_shape_append_path(shape, cmdsPtr, (uint)c, ptsPtr, (uint)p), "tvg_shape_append_path");
			}
		}

		// ------------------------------------------------------------------------------------------
		// Plumbing
		// ------------------------------------------------------------------------------------------

		private static void CreateRenderTargetBuffer()
		{
			if (pixelsHandle.IsAllocated)
			{
				pixelsHandle.Free();
			}

			pixels = new uint[RenderWidth * RenderHeight];
			pixelsHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);

			// ABGR8888 packs to R,G,B,A bytes in memory on little-endian, which is exactly
			// Evergine's R8G8B8A8_UNorm — no swizzle, no flip (ThorVG writes row 0 at the top).
			Check(ThorVG.tvg_swcanvas_set_target(canvas,
				(uint*)pixelsHandle.AddrOfPinnedObject(), RenderWidth, RenderWidth, RenderHeight,
				Tvg_Colorspace.TVG_COLORSPACE_ABGR8888), "tvg_swcanvas_set_target");
		}

		private static void CreateTextureAndResourceSet()
		{
			texture?.Dispose();

			var textureDescription = new TextureDescription()
			{
				Type = TextureType.Texture2D,
				Width = RenderWidth,
				Height = RenderHeight,
				Depth = 1,
				ArraySize = 1,
				// Default + None on purpose: on DX11 this goes through UpdateSubresource, the
				// same per-frame path the engine's video player uses. Dynamic maps with a flat
				// copy that ignores the driver's row pitch.
				Usage = ResourceUsage.Default,
				CpuAccess = ResourceCpuAccess.None,
				Flags = TextureFlags.ShaderResource,
				Format = PixelFormat.R8G8B8A8_UNorm,
				MipLevels = 1,
				SampleCount = TextureSampleCount.None,
			};
			texture = graphics.Factory.CreateTexture(ref textureDescription);

			var resourceSetDescription = new ResourceSetDescription(resourceLayout, texture, sampler);
			resourceSet = graphics.Factory.CreateResourceSet(ref resourceSetDescription);
		}

		private static void UpdateSurfaceSize()
		{
			// The ThorVG render size stays fixed; the fullscreen triangle stretches it. Only the
			// viewport tracks the window.
			viewports = new Viewport[] { new Viewport(0, 0, window.Width, window.Height) };
			scissors = new Evergine.Mathematics.Rectangle[] { new Evergine.Mathematics.Rectangle(0, 0, (int)window.Width, (int)window.Height) };
		}

		private static void CountFps()
		{
			fpsFrames++;
			if (fpsTimer.ElapsedMilliseconds >= 1000)
			{
				window.Title = $"Thorvg.NET - Evergine low-level  [{fpsFrames} fps]";
				fpsFrames = 0;
				fpsTimer.Restart();
			}
		}

		private static void Check(Tvg_Result result, string operation)
		{
			if (result != Tvg_Result.TVG_RESULT_SUCCESS)
			{
				throw new InvalidOperationException($"{operation} returned {result}");
			}
		}
	}
}
