# Thorvg.NET

This repository contains low-level bindings for [ThorVG](https://github.com/thorvg/thorvg) used in [Evergine](https://evergine.com/).
This binding is generated from the ThorVG release:
[https://github.com/thorvg/thorvg/releases/tag/v1.1.0](https://github.com/thorvg/thorvg/releases/tag/v1.1.0)

[![CI](https://github.com/EvergineTeam/Thorvg.NET/actions/workflows/CI.yml/badge.svg)](https://github.com/EvergineTeam/Thorvg.NET/actions/workflows/CI.yml)
[![CD](https://github.com/EvergineTeam/Thorvg.NET/actions/workflows/CD.yml/badge.svg)](https://github.com/EvergineTeam/Thorvg.NET/actions/workflows/CD.yml)
[![Nuget](https://img.shields.io/nuget/v/Evergine.Bindings.ThorVG?logo=nuget)](https://www.nuget.org/packages/Evergine.Bindings.ThorVG)

## Purpose

ThorVG is a lightweight vector graphics engine: it rasterises SVG, Lottie animations, text and
shapes into a plain memory buffer, with no windowing system and no GPU required.

These bindings expose ThorVG's C API to .NET so that Evergine — and any other .NET application —
can rasterise vector content directly, with no marshalling layer in between. See the
[upstream repository](https://github.com/thorvg/thorvg) and the
[official documentation](https://www.thorvg.org/) for what the engine itself can do.

## Features

- **Complete C API** — 165 entry points covering the engine, canvases, shapes, gradients,
  pictures, text, animations and savers
- **Names match the C API** — `tvg_engine_init`, `Tvg_Paint`, `TVG_RESULT_SUCCESS` read exactly as
  they do in the ThorVG documentation, so upstream examples translate line by line
- **Zero-overhead interop** — opaque handles are plain `IntPtr`, value types are blittable structs;
  no wrapper allocations
- **Renders anywhere** — the software rasteriser writes straight into a buffer you own, so there is
  no window, context or GPU to set up
- **Generated, not hand-written** — regenerating for a new ThorVG release is a single command

## Supported Platforms

- [x] Windows x64, ARM64
- [x] Linux x64, ARM64
- [x] MacOS ARM64
- [x] Android ARM64, x64
- [x] iOS ARM64 and the ARM64 simulator — static archives, linked by `buildTransitive/`
- [ ] MacOS x64, browser-wasm — not built yet; each is one more leg in the native build

Android needs nothing special: the `.so` files sit under `runtimes/android-*/native/` and .NET
probes them by RID. iOS has no dynamic loader for third-party code, so it ships a static archive
and `buildTransitive/Evergine.Bindings.ThorVG.targets` tells the consuming build to link it.
**The iOS link has not been verified against a real application** — ThorVG bundles its own PNG,
JPEG and WebP decoders, so `-force_load` could surface duplicate symbols in an app that also
links those libraries.

## Usage

```csharp
using Evergine.Bindings.ThorVG;

unsafe
{
    ThorVG.tvg_engine_init(0);

    IntPtr canvas = ThorVG.tvg_swcanvas_create(Tvg_Engine_Option.TVG_ENGINE_OPTION_DEFAULT);
    var buffer = new uint[512 * 512];

    fixed (uint* pixels = buffer)
    {
        ThorVG.tvg_swcanvas_set_target(canvas, pixels, 512, 512, 512, Tvg_Colorspace.TVG_COLORSPACE_ARGB8888);

        IntPtr picture = ThorVG.tvg_picture_new();
        ThorVG.tvg_picture_load(picture, "logo.svg");
        ThorVG.tvg_canvas_add(canvas, picture);

        ThorVG.tvg_canvas_draw(canvas, true);
        ThorVG.tvg_canvas_sync(canvas);
    }

    // `buffer` now holds the rasterised image.
    ThorVG.tvg_canvas_destroy(canvas);
    ThorVG.tvg_engine_term();
}
```

`Test/Program.cs` is a runnable version of the above that also checks the native library against
the header the bindings were generated from.

### Demo

`LowLevelDemo/` drives the binding the way an application would: ThorVG rasterises an animated
vector scene — a breathing radial gradient, a ring of rotating petals, a star built from raw path
commands, a dashed stroked arc and a looping Lottie loaded from memory — into a CPU buffer every
frame, and the [Evergine](https://evergine.com/) low-level graphics API uploads that buffer to a
texture and presents it with a fullscreen triangle. The rasteriser is CPU-only, so no GPU features
beyond a textured triangle are involved.

```bash
dotnet run --project LowLevelDemo/LowLevelDemo.csproj
```

### Strings and buffers

`const char*` arguments — paths, mimetypes, names — are marshalled as UTF-8. The two payload
arguments, on `tvg_picture_load_data` and `tvg_font_load_data`, take a `byte*` instead: they carry
an explicit size and are bytes, not text, so marshalling them as a string would mangle PNG, WebP,
TTF and compressed Lottie. Version strings come back as `byte*` pointing at library-owned memory —
read them with `Marshal.PtrToStringUTF8` and never free them.

### Engines

The shipped libraries are built with the CPU rasteriser only. `thorvg_capi.h` declares the GL and
WebGPU canvas functions unconditionally, so they are part of the binding surface and return
`TVG_RESULT_NOT_SUPPORTED` at run time. Enabling those engines is a build-flag change and does not
alter the managed API.

## Building

```bash
dotnet build ThorvgGen.sln
```

To regenerate the bindings after changing the header:

```bash
dotnet run --project ThorvgGen/ThorvgGen.csproj
```

The generator parses `ThorvgGen/Headers/thorvg_capi.h` with
[CppAst](https://github.com/xoofx/CppAst.NET) and writes `Evergine.Bindings.ThorVG/Generated/`.
`ThorvgGen/Headers/libc-stubs/` stands in for the two system headers ThorVG includes, so parsing
does not depend on the host's libc and Windows and Linux produce identical output.

## Native libraries

ThorVG publishes no binaries — every release carries a single source tarball — so this repository
compiles its own. `.github/workflows/thorvg-meson.yml` builds all four RIDs with meson from a given
tag and stages each library at its final path; CD runs it automatically whenever the tracked release
moves, so the header and the libraries always come from one revision.

The build uses `-Dbindings=capi`, which is **off** by default and without which the library builds
perfectly and exports not one `tvg_*` symbol. The workflow checks the output for `tvg_engine_init`
rather than trusting a green build.

## Related Evergine Bindings

- [WebGPU.NET](https://github.com/EvergineTeam/WebGPU.NET) — Bindings for WebGPU
- [Meshoptimizer.NET](https://github.com/EvergineTeam/Meshoptimizer.NET) — Bindings for meshoptimizer
- [RenderDoc.NET](https://github.com/EvergineTeam/RenderDoc.NET) — Bindings for RenderDoc
- [XAtlas.NET](https://github.com/EvergineTeam/XAtlas.NET) — Bindings for xatlas
- [MuJoCo.NET](https://github.com/EvergineTeam/MuJoCo.NET) — Bindings for MuJoCo
