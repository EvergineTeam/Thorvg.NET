using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Evergine.Bindings.ThorVG;
using Evergine.Common.Graphics;
using Evergine.DirectX11;
using Evergine.Forms;

namespace LowLevelDemo.Infrastructure
{
	/// <summary>
	/// Hosts one <see cref="ExampleBase"/>: ThorVG rasterises the example's scene into a CPU
	/// buffer, and the Evergine low-level graphics API uploads that buffer to a texture and
	/// presents it with a fullscreen triangle — the same CPU-to-GPU bridge as the engine's video
	/// player. Static examples rasterise once and stay idle; animated ones re-rasterise per frame.
	/// </summary>
	public static unsafe class Runner
	{
		private const int RenderWidth = 1280;
		private const int RenderHeight = 720;

		// ThorVG side.
		private static ExampleBase example;
		private static IntPtr canvas;
		private static uint[] pixels;
		private static GCHandle pixelsHandle;
		private static bool firstFrame;

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

		public static int Run<T>()
			where T : ExampleBase, new()
		{
			example = new T();

			NativeResolver.Install();

			// ---- ThorVG ------------------------------------------------------------------------
			Check(ThorVG.tvg_engine_init(4), "tvg_engine_init");

			canvas = ThorVG.tvg_swcanvas_create(example.CanvasEngineOption);
			if (canvas == IntPtr.Zero)
			{
				Console.Error.WriteLine("tvg_swcanvas_create returned null.");
				return 1;
			}

			CreateRenderTargetBuffer();
			example.Build(canvas, RenderWidth, RenderHeight);
			firstFrame = true;

			// ---- Evergine low-level: window + swapchain (DesktopUtils pattern) -----------------
			var windowSystem = new FormsWindowsSystem();
			window = windowSystem.CreateWindow($"Thorvg.NET - {example.Title}", RenderWidth, RenderHeight);
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
			example.Dispose();
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

			// ---- ThorVG: rasterise into the CPU buffer only when the scene changed ---------------
			bool dirty = example.Update(canvas, t) || firstFrame;
			firstFrame = false;

			if (dirty)
			{
				Check(ThorVG.tvg_canvas_update(canvas), "tvg_canvas_update");
				Check(ThorVG.tvg_canvas_draw(canvas, true), "tvg_canvas_draw");
				Check(ThorVG.tvg_canvas_sync(canvas), "tvg_canvas_sync");

				// The last parameter is the subresource index, despite the managed overloads
				// calling it an offset; Default-usage textures go through UpdateSubresource on
				// DX11, which is the per-frame-safe path (Dynamic maps with a flat copy that
				// ignores row pitch).
				graphics.UpdateTextureData(texture, pixelsHandle.AddrOfPinnedObject(), (uint)(RenderWidth * RenderHeight * 4), 0);
			}

			// ---- Evergine: present (unconditional; keeps resize/present logic simple) ------------
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
				window.Title = $"Thorvg.NET - {example.Title}  [{fpsFrames} fps]";
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
