using CppAst;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ThorvgGen
{
	public class CsCodeGenerator
	{
		private const string Namespace = "Evergine.Bindings.ThorVG";
		private const string NativeClass = "ThorVG";
		private const string LibraryName = "thorvg";

		public static readonly CsCodeGenerator Instance = new CsCodeGenerator();

		/// <summary>
		/// InlineArray wrapper types emitted for fixed-size arrays of non-primitive elements,
		/// keyed by generated type name.
		/// </summary>
		private readonly SortedDictionary<string, string> inlineArrayTypes = new SortedDictionary<string, string>();

		private CsCodeGenerator()
		{
		}

		public void Generate(CppCompilation compilation, string outputPath)
		{
			Helpers.TypedefList = compilation.Typedefs
				.Where(t => t.TypeKind == CppTypeKind.Typedef
					&& t.ElementType is CppPointerType pointer
					&& pointer.ElementType.TypeKind != CppTypeKind.Function)
				.Select(t => t.Name)
				.ToList();

			GenerateConstants(compilation, outputPath);
			GenerateEnums(compilation, outputPath);
			GenerateDelegates(compilation, outputPath);
			GenerateStructs(compilation, outputPath);
			GenerateFunctions(compilation, outputPath);
		}

		/// <summary>
		/// True when the element comes from the vendored ThorVG header. Without this filter the
		/// libc stubs would be emitted too, since ParseMacros pulls in the whole preprocessor
		/// state.
		/// </summary>
		private static bool IsThorVGElement(CppElement element)
		{
			var file = element.Span.Start.File;
			if (string.IsNullOrEmpty(file))
			{
				return false;
			}

			return file.Replace('\\', '/').EndsWith("/thorvg_capi.h", StringComparison.OrdinalIgnoreCase);
		}

		private static StreamWriter CreateFile(string outputPath, string fileName, params string[] usings)
		{
			var file = new StreamWriter(Path.Combine(outputPath, fileName));

			foreach (var @using in usings)
			{
				file.WriteLine($"using {@using};");
			}

			file.WriteLine();
			file.WriteLine($"namespace {Namespace}");
			file.WriteLine("{");

			return file;
		}

		private static void CloseFile(StreamWriter file)
		{
			file.WriteLine("}");
			file.Dispose();
		}

		// -------------------------------------------------------------------------------------
		// Constants
		// -------------------------------------------------------------------------------------

		private void GenerateConstants(CppCompilation compilation, string outputPath)
		{
			using var file = CreateFile(outputPath, "Constants.cs", "System");

			file.WriteLine($"\tpublic static partial class {NativeClass}");
			file.WriteLine("\t{");

			var emitted = new HashSet<string>();

			foreach (var macro in compilation.Macros)
			{
				if (!IsThorVGElement(macro) || macro.Parameters != null)
				{
					// Function-like macros (mjENABLED, mjMAX, mjMARKSTACK) have no C# equivalent.
					continue;
				}

				if (!Helpers.TryParseMacroValue(macro.Value, out var csValue, out var csType))
				{
					// Alias macros such as "#define mju_sqrt sqrt" land here and are dropped.
					continue;
				}

				var name = Helpers.EscapeReservedKeyword(Helpers.StripPrefix(macro.Name));
				if (!emitted.Add(name))
				{
					continue;
				}

				file.WriteLine($"\t\tpublic const {csType} {name} = {csValue};");
			}

			file.WriteLine("\t}");
			CloseFile(file);
		}

		// -------------------------------------------------------------------------------------
		// Enums
		// -------------------------------------------------------------------------------------

		private void GenerateEnums(CppCompilation compilation, string outputPath)
		{
			using var file = CreateFile(outputPath, "Enums.cs", "System");

			var enums = compilation.Enums
				.Where(e => IsThorVGElement(e) && !e.IsAnonymous && e.Items.Count > 0)
				.ToList();

			bool first = true;
			foreach (var @enum in enums)
			{
				if (!first)
				{
					file.WriteLine();
				}

				first = false;

				var enumName = Helpers.NormalizeTypeName(@enum.Name);

				Helpers.PrintComments(file, @enum.Comment, "\t");

				if (compilation.Typedefs.Any(t => t.Name == enumName + "Flags"))
				{
					file.WriteLine("\t[Flags]");
				}

				file.WriteLine($"\tpublic enum {enumName}");
				file.WriteLine("\t{");

				foreach (var item in @enum.Items)
				{
					var itemName = Helpers.EscapeReservedKeyword(Helpers.ScreamingToPascalCase(Helpers.StripPrefix(item.Name)));
					file.WriteLine($"\t\t{itemName} = {item.Value},");
				}

				file.WriteLine("\t}");
			}

			CloseFile(file);
		}

		// -------------------------------------------------------------------------------------
		// Delegates
		// -------------------------------------------------------------------------------------

		private void GenerateDelegates(CppCompilation compilation, string outputPath)
		{
			using var file = CreateFile(outputPath, "Delegates.cs", "System", "System.Runtime.InteropServices");

			var delegates = compilation.Typedefs
				.Where(t => IsThorVGElement(t) && Helpers.ResolvesToFunctionPointer(t, out _))
				.ToList();

			bool first = true;
			foreach (var typedef in delegates)
			{
				Helpers.ResolvesToFunctionPointer(typedef, out var functionType);

				if (!first)
				{
					file.WriteLine();
				}

				first = false;

				Helpers.PrintComments(file, typedef.Comment, "\t");

				var returnType = Helpers.ShowAsMarshalType(Helpers.ConvertToCSharpType(functionType.ReturnType), Helpers.Family.ret);
				var name = Helpers.NormalizeTypeName(typedef.Name);

				file.WriteLine("\t[UnmanagedFunctionPointer(CallingConvention.Cdecl)]");
				file.WriteLine($"\tpublic unsafe delegate {returnType} {name}({BuildParameterList(functionType.Parameters)});");
			}

			CloseFile(file);
		}

		private static string BuildParameterList(IEnumerable<CppParameter> parameters)
		{
			var list = parameters.ToList();
			var parts = new List<string>();

			for (int index = 0; index < list.Count; index++)
			{
				var parameter = list[index];
				var next = index + 1 < list.Count ? list[index + 1] : null;

				var type = GetParameterType(parameter, next);
				var name = string.IsNullOrEmpty(parameter.Name) ? $"arg{index}" : parameter.Name;
				parts.Add($"{type} {Helpers.EscapeReservedKeyword(name)}");
			}

			return string.Join(", ", parts);
		}

		/// <summary>
		/// Input strings are const char* and are marshalled as UTF-8, which is what ThorVG's path,
		/// name and mimetype arguments expect.
		///
		/// The exception is a payload buffer. tvg_picture_load_data and tvg_font_load_data both take
		/// `const char* data` followed by an explicit size, and that is bytes, not text: SVG happens
		/// to be textual, but PNG, JPEG, WebP, TTF and compressed Lottie are not, and marshalling
		/// them as UTF-8 would mangle every one. Anchoring the rule on the name AND the adjacent
		/// size parameter keeps it from swallowing the ordinary string arguments that sit right
		/// beside it in the same signatures.
		/// </summary>
		private static string GetParameterType(CppParameter parameter, CppParameter next)
		{
			if (IsConstCharPointer(parameter.Type))
			{
				if (IsPayloadBuffer(parameter, next))
				{
					return "byte*";
				}

				return "[MarshalAs(UnmanagedType.LPUTF8Str)] string";
			}

			return Helpers.ShowAsMarshalType(Helpers.ConvertToCSharpType(parameter.Type), Helpers.Family.param);
		}

		private static bool IsPayloadBuffer(CppParameter parameter, CppParameter next)
		{
			if (parameter.Name != "data" || next == null)
			{
				return false;
			}

			return next.Name is "size" or "len" or "length";
		}

		private static bool IsConstCharPointer(CppType type)
		{
			if (type is CppQualifiedType outer)
			{
				type = outer.ElementType;
			}

			if (type is not CppPointerType pointer)
			{
				return false;
			}

			if (pointer.ElementType is not CppQualifiedType qualified || qualified.Qualifier != CppTypeQualifier.Const)
			{
				return false;
			}

			return qualified.ElementType is CppPrimitiveType primitive
				&& (primitive.Kind == CppPrimitiveKind.Char || primitive.Kind == CppPrimitiveKind.UnsignedChar);
		}

		// -------------------------------------------------------------------------------------
		// Structs
		// -------------------------------------------------------------------------------------

		private void GenerateStructs(CppCompilation compilation, string outputPath)
		{
			this.inlineArrayTypes.Clear();

			var classes = compilation.Classes
				.Where(c => IsThorVGElement(c)
					&& c.IsDefinition
					&& !c.IsAnonymous
					&& !string.IsNullOrEmpty(c.Name)
					&& c.ClassKind != CppClassKind.Class)
				.ToList();

			// Anonymous aggregates (mjVisual's six sub-structs, mjuiItem's union) get a synthesized
			// name before anything is written, so that fields can refer to them.
			foreach (var @class in classes)
			{
				RegisterAnonymousMembers(@class);
			}

			var body = new StringWriter();
			foreach (var @class in classes)
			{
				WriteStruct(body, @class);
			}

			using var file = CreateFile(outputPath, "Structs.cs", "System", "System.Runtime.CompilerServices", "System.Runtime.InteropServices");

			foreach (var inlineArray in this.inlineArrayTypes.Values)
			{
				file.Write(inlineArray);
			}

			file.Write(body.ToString());
			CloseFile(file);
		}

		private static void RegisterAnonymousMembers(CppClass @class)
		{
			var parentName = Helpers.GetGeneratedClassName(@class);
			int anonymousIndex = 0;

			foreach (var field in @class.Fields)
			{
				if (GetAnonymousClass(field.Type) is not CppClass anonymous)
				{
					continue;
				}

				var suffix = string.IsNullOrEmpty(field.Name) ? $"anonymous{anonymousIndex++}" : field.Name;
				Helpers.RegisterAnonymousName(anonymous, $"{parentName}_{suffix}");
				RegisterAnonymousMembers(anonymous);
			}

			// An anonymous union declared without a member name (mjuiItem) is reported as a nested
			// class with no corresponding field; it still needs a type and a field of its own.
			foreach (var nested in @class.Classes.Where(c => c.IsAnonymous && !Helpers.HasAnonymousName(c)))
			{
				Helpers.RegisterAnonymousName(nested, $"{parentName}_anonymous{anonymousIndex++}");
				RegisterAnonymousMembers(nested);
			}
		}

		private static CppClass GetAnonymousClass(CppType type)
		{
			if (type is CppQualifiedType qualified)
			{
				type = qualified.ElementType;
			}

			return type is CppClass @class && @class.IsAnonymous ? @class : null;
		}

		private void WriteStruct(TextWriter file, CppClass @class)
		{
			// Nested anonymous aggregates are emitted as top-level types first.
			foreach (var field in @class.Fields)
			{
				if (GetAnonymousClass(field.Type) is CppClass anonymous)
				{
					WriteStruct(file, anonymous);
				}
			}

			foreach (var nested in @class.Classes.Where(c => c.IsAnonymous && !HasBackingField(@class, c)))
			{
				WriteStruct(file, nested);
			}

			var name = Helpers.GetGeneratedClassName(@class);
			bool isUnion = @class.ClassKind == CppClassKind.Union;

			file.WriteLine();
			Helpers.PrintComments(file, @class.Comment, "\t");
			file.WriteLine($"\t[StructLayout(LayoutKind.{(isUnion ? "Explicit" : "Sequential")})]");
			file.WriteLine($"\tpublic unsafe struct {name}");
			file.WriteLine("\t{");

			foreach (var field in @class.Fields)
			{
				WriteField(file, field, isUnion);
			}

			foreach (var nested in @class.Classes.Where(c => c.IsAnonymous && !HasBackingField(@class, c)))
			{
				var nestedName = Helpers.GetGeneratedClassName(nested);
				if (isUnion)
				{
					file.WriteLine("\t\t[FieldOffset(0)]");
				}

				file.WriteLine($"\t\tpublic {nestedName} {nestedName.Substring(nestedName.LastIndexOf('_') + 1)};");
			}

			file.WriteLine("\t}");
		}

		private static bool HasBackingField(CppClass parent, CppClass nested)
		{
			return parent.Fields.Any(f => ReferenceEquals(GetAnonymousClass(f.Type), nested));
		}

		private void WriteField(TextWriter file, CppField field, bool isUnion)
		{
			var rawName = field.Name;
			if (string.IsNullOrEmpty(rawName) && GetAnonymousClass(field.Type) is CppClass unnamed)
			{
				// An anonymous union declared without a member name (mjuiItem) is exposed under the
				// suffix of its synthesized type, so the C# name stays predictable.
				var typeName = Helpers.GetGeneratedClassName(unnamed);
				rawName = typeName.Substring(typeName.LastIndexOf('_') + 1);
			}

			var fieldName = Helpers.EscapeReservedKeyword(Helpers.PascalCaseField(rawName));

			Helpers.PrintComments(file, field.Comment, "\t\t");

			if (isUnion)
			{
				file.WriteLine("\t\t[FieldOffset(0)]");
			}

			var type = field.Type;
			if (type is CppQualifiedType qualified)
			{
				type = qualified.ElementType;
			}

			if (type is CppArrayType arrayType)
			{
				var length = Helpers.GetFlattenedArrayLength(arrayType, out var elementType);
				var elementCsType = Helpers.ShowAsMarshalType(Helpers.ConvertToCSharpType(elementType), Helpers.Family.field);

				if (Helpers.IsFixedBufferElement(elementCsType))
				{
					file.WriteLine($"\t\tpublic fixed {elementCsType} {fieldName}[{length}];");
				}
				else
				{
					// fixed buffers only accept primitives, so arrays of structs use an InlineArray.
					var wrapper = this.GetOrCreateInlineArrayType(elementCsType, length);
					file.WriteLine($"\t\tpublic {wrapper} {fieldName};");
				}

				return;
			}

			var csType = Helpers.ShowAsMarshalType(Helpers.ConvertToCSharpType(type), Helpers.Family.field);
			file.WriteLine($"\t\tpublic {csType} {fieldName};");
		}

		private string GetOrCreateInlineArrayType(string elementCsType, int length)
		{
			var name = $"InlineArray_{elementCsType.Replace("*", "Ptr")}_{length}";

			if (!this.inlineArrayTypes.ContainsKey(name))
			{
				var writer = new StringWriter();
				writer.WriteLine();
				writer.WriteLine($"\t[InlineArray({length})]");
				writer.WriteLine($"\tpublic unsafe struct {name}");
				writer.WriteLine("\t{");
				writer.WriteLine($"\t\tprivate {elementCsType} element0;");
				writer.WriteLine("\t}");

				this.inlineArrayTypes[name] = writer.ToString();
			}

			return name;
		}

		// -------------------------------------------------------------------------------------
		// Functions
		// -------------------------------------------------------------------------------------

		private void GenerateFunctions(CppCompilation compilation, string outputPath)
		{
			using var file = CreateFile(outputPath, "Functions.cs", "System", "System.Runtime.InteropServices");

			file.WriteLine($"\tpublic static unsafe partial class {NativeClass}");
			file.WriteLine("\t{");

			var functions = compilation.Functions
				.Where(f => IsThorVGElement(f)
					&& !f.Flags.HasFlag(CppFunctionFlags.Inline)
					&& !f.Flags.HasFlag(CppFunctionFlags.FunctionTemplate))
				.ToList();

			var emitted = new HashSet<string>();

			bool first = true;
			foreach (var function in functions)
			{
				var csName = Helpers.EscapeReservedKeyword(Helpers.StripPrefix(function.Name));
				if (!emitted.Add(function.Name))
				{
					continue;
				}

				if (!first)
				{
					file.WriteLine();
				}

				first = false;

				Helpers.PrintComments(file, function.Comment, "\t\t");

				var returnType = Helpers.ShowAsMarshalType(Helpers.ConvertToCSharpType(function.ReturnType), Helpers.Family.ret);

				file.WriteLine($"\t\t[DllImport(\"{LibraryName}\", EntryPoint = \"{function.Name}\", CallingConvention = CallingConvention.Cdecl)]");
				file.WriteLine($"\t\tpublic static extern {returnType} {csName}({BuildParameterList(function.Parameters)});");
			}

			file.WriteLine("\t}");
			CloseFile(file);
		}
	}
}
