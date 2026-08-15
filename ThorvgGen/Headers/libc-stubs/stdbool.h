// Minimal stand-in for <stdbool.h>. See stdint.h in this folder for why these stubs exist.
//
// Deliberately empty. CppAst always drives libclang in C++ mode, where bool, true and false are
// keywords and a real <stdbool.h> is a no-op as well. Defining bool as _Bool here — the C
// spelling — breaks the parse instead, because _Bool is not a C++ type.
//
// thorvg_capi.h uses plain `bool` in around thirty signatures. Those reach the bindings as
// [MarshalAs(UnmanagedType.I1)] bool for parameters and byte for struct fields, which is the
// one-byte C ABI the library was compiled against.

#pragma once
