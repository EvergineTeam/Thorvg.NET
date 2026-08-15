// Minimal stand-in for the C library's <stdint.h>, used only while parsing ThorVG's C API header.
//
// thorvg_capi.h includes <stdint.h> and <stdbool.h> and nothing else. libclang ships its own
// freestanding headers but finds no include paths at all on a bare Linux CI runner, so the include
// fails there and the parse dies, while it succeeds on Windows, where the Windows SDK happens to be
// on the search path.
//
// Rather than teach the generator where each platform keeps its libc, these stubs make parsing
// hermetic: the generated bindings no longer depend on the host's libc version, and Windows and
// Linux produce byte-identical output. Nothing from these headers ever reaches the bindings —
// CsCodeGenerator only emits declarations whose source file is thorvg_capi.h itself.
//
// The types come from clang's own predefined macros rather than fixed spellings, so they always
// match the target the parser is configured for.

#pragma once

typedef __INT8_TYPE__ int8_t;
typedef __INT16_TYPE__ int16_t;
typedef __INT32_TYPE__ int32_t;
typedef __INT64_TYPE__ int64_t;

typedef __UINT8_TYPE__ uint8_t;
typedef __UINT16_TYPE__ uint16_t;
typedef __UINT32_TYPE__ uint32_t;
typedef __UINT64_TYPE__ uint64_t;

typedef __INTPTR_TYPE__ intptr_t;
typedef __UINTPTR_TYPE__ uintptr_t;
