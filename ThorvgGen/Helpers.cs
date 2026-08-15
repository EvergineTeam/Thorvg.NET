using CppAst;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ThorvgGen
{
	public static class Helpers
	{
		/// <summary>
		/// Typedefs that resolve to an opaque (non-function) pointer, mapped to IntPtr. ThorVG hands
		/// out six of them — Tvg_Canvas, Tvg_Paint, Tvg_Gradient, Tvg_Saver, Tvg_Animation and
		/// Tvg_Accessor — spelled `typedef struct _Tvg_Canvas* Tvg_Canvas;`, so the pointer is
		/// already part of the typedef and must not be given another level of indirection.
		/// </summary>
		public static List<string> TypedefList = new List<string>();

		/// <summary>
		/// Typedef name -> C# type, for typedefs that must not be emitted as their own type.
		/// </summary>
		private static readonly Dictionary<string, string> csNameMappings = new Dictionary<string, string>()
		{
			{ "bool", "byte" },
			{ "uint8_t", "byte" },
			{ "uint16_t", "ushort" },
			{ "uint32_t", "uint" },
			{ "uint64_t", "ulong" },
			{ "int8_t", "sbyte" },
			{ "int16_t", "short" },
			{ "int32_t", "int" },
			{ "int64_t", "long" },
			{ "char", "byte" },
			{ "size_t", "nuint" },
			{ "intptr_t", "nint" },
			{ "uintptr_t", "nuint" },
		};

		/// <summary>
		/// Identity. ThorVG's public type names are already the ones the header declares; the
		/// leading underscore appears only on the incomplete struct behind an opaque handle
		/// (<c>struct _Tvg_Canvas*</c>), which never surfaces because the typedef resolves to
		/// IntPtr. Names are kept verbatim — tvg_engine_init, Tvg_Paint, TVG_RESULT_SUCCESS — for
		/// parity with the upstream documentation and examples, matching MuJoCo.NET's choice.
		/// </summary>
		public static string NormalizeTypeName(string name) => name;

		public static string ConvertToCSharpType(CppType type, bool isPointer = false)
		{
			return GetCsTypeName(type, isPointer);
		}

		private static string GetCsTypeName(CppPointerType pointerType)
		{
			if (pointerType.ElementType is CppQualifiedType qualifiedType)
			{
				if (qualifiedType.ElementType is CppPointerType subPointerType)
				{
					return GetCsTypeName(subPointerType) + "*";
				}

				return GetCsTypeName(qualifiedType.ElementType, true);
			}

			if (pointerType.ElementType is CppFunctionType functionType)
			{
				return GetCsFunctionPointerType(functionType);
			}

			if (pointerType.ElementType is CppPointerType innerPointer)
			{
				return GetCsTypeName(innerPointer) + "*";
			}

			return GetCsTypeName(pointerType.ElementType, true);
		}

		private static string GetCsTypeName(CppType type, bool isPointer = false)
		{
			if (type is CppPrimitiveType primitiveType)
			{
				return GetCsTypeName(primitiveType, isPointer);
			}

			if (type is CppQualifiedType qualifiedType)
			{
				return GetCsTypeName(qualifiedType.ElementType, isPointer);
			}

			if (type is CppEnum enumType)
			{
				var enumCsName = GetCsCleanName(enumType.Name);
				return isPointer ? enumCsName + "*" : enumCsName;
			}

			if (type is CppTypedef typedef)
			{
				if (TypedefList.Contains(typedef.Name) || csNameMappings.ContainsKey(typedef.Name))
				{
					var mapped = GetCsCleanName(typedef.Name);
					return isPointer ? mapped + "*" : mapped;
				}

				// A typedef to a function pointer is emitted as a function pointer at every use
				// site: a managed delegate type would not be blittable inside a struct.
				if (ResolvesToFunctionPointer(typedef, out var functionType))
				{
					var fnPtr = GetCsFunctionPointerType(functionType);
					return isPointer ? fnPtr + "*" : fnPtr;
				}

				// Only typedefs that name an aggregate become a C# type of their own. Everything
				// else — including MuJoCo's opaque `typedef void mjString;` family — resolves to
				// whatever it actually aliases.
				var underlying = UnwrapTypedef(typedef.ElementType);
				if (underlying is CppClass || underlying is CppEnum)
				{
					var typeDefCsName = GetCsCleanName(typedef.Name);
					return isPointer ? typeDefCsName + "*" : typeDefCsName;
				}

				return GetCsTypeName(typedef.ElementType, isPointer);
			}

			if (type is CppClass @class)
			{
				var className = GetGeneratedClassName(@class);
				return isPointer ? className + "*" : className;
			}

			if (type is CppPointerType pointerType)
			{
				return GetCsTypeName(pointerType);
			}

			if (type is CppFunctionType functionType2)
			{
				return GetCsFunctionPointerType(functionType2);
			}

			if (type is CppArrayType arrayType)
			{
				return GetCsTypeName(arrayType.ElementType, isPointer);
			}

			return string.Empty;
		}

		private static string GetCsTypeName(CppPrimitiveType primitiveType, bool isPointer)
		{
			string result;

			switch (primitiveType.Kind)
			{
				case CppPrimitiveKind.Void: result = "void"; break;
				case CppPrimitiveKind.Bool: result = "bool"; break;
				case CppPrimitiveKind.Char: result = "byte"; break;
				case CppPrimitiveKind.WChar: result = "char"; break;
				case CppPrimitiveKind.Short: result = "short"; break;
				case CppPrimitiveKind.Int: result = "int"; break;
				case CppPrimitiveKind.Long: result = "int"; break;
				case CppPrimitiveKind.LongLong: result = "long"; break;
				case CppPrimitiveKind.UnsignedChar: result = "byte"; break;
				case CppPrimitiveKind.UnsignedShort: result = "ushort"; break;
				case CppPrimitiveKind.UnsignedInt: result = "uint"; break;
				case CppPrimitiveKind.UnsignedLong: result = "uint"; break;
				case CppPrimitiveKind.UnsignedLongLong: result = "ulong"; break;
				case CppPrimitiveKind.Float: result = "float"; break;
				case CppPrimitiveKind.Double: result = "double"; break;
				case CppPrimitiveKind.LongDouble: result = "double"; break;
				default: result = string.Empty; break;
			}

			if (isPointer)
			{
				result += "*";
			}

			return result;
		}

		private static string GetCsFunctionPointerType(CppFunctionType functionType)
		{
			var sb = new StringBuilder("delegate* unmanaged[Cdecl]<");

			foreach (var param in functionType.Parameters)
			{
				sb.Append(ConvertToCSharpType(param.Type));
				sb.Append(", ");
			}

			sb.Append(ConvertToCSharpType(functionType.ReturnType));
			sb.Append('>');

			return sb.ToString();
		}

		/// <summary>
		/// Resolves typedef and cv-qualifier layers down to the underlying type.
		/// </summary>
		public static CppType UnwrapTypedef(CppType type)
		{
			while (true)
			{
				switch (type)
				{
					case CppTypedef typedef:
						type = typedef.ElementType;
						continue;
					case CppQualifiedType qualified:
						type = qualified.ElementType;
						continue;
					default:
						return type;
				}
			}
		}

		/// <summary>
		/// True when the typedef ultimately aliases a pointer to a function.
		/// </summary>
		public static bool ResolvesToFunctionPointer(CppType type, out CppFunctionType functionType)
		{
			functionType = null;

			while (true)
			{
				switch (type)
				{
					case CppTypedef typedef:
						type = typedef.ElementType;
						continue;
					case CppQualifiedType qualified:
						type = qualified.ElementType;
						continue;
					case CppPointerType pointer when pointer.ElementType is CppFunctionType fn:
						functionType = fn;
						return true;
					default:
						return false;
				}
			}
		}

		public static string GetCsCleanName(string name)
		{
			if (TypedefList.Contains(name))
			{
				return "IntPtr";
			}

			if (csNameMappings.TryGetValue(name, out string mappedName))
			{
				return mappedName;
			}

			return NormalizeTypeName(name);
		}

		/// <summary>
		/// Name under which a parsed class is emitted. Anonymous aggregates get a synthesized name
		/// assigned by the generator (see <see cref="RegisterAnonymousName"/>).
		/// </summary>
		public static string GetGeneratedClassName(CppClass @class)
		{
			if (anonymousNames.TryGetValue(@class, out var synthesized))
			{
				return synthesized;
			}

			return NormalizeTypeName(@class.Name);
		}

		private static readonly Dictionary<CppClass, string> anonymousNames = new Dictionary<CppClass, string>();

		public static void RegisterAnonymousName(CppClass @class, string name)
		{
			anonymousNames[@class] = name;
		}

		public static bool HasAnonymousName(CppClass @class) => anonymousNames.ContainsKey(@class);

		public enum Family
		{
			param,
			field,
			ret,
		}

		public static string ShowAsMarshalType(string type, Family family)
		{
			switch (type)
			{
				case "bool":
					switch (family)
					{
						case Family.param:
							return "[MarshalAs(UnmanagedType.I1)] bool";
						case Family.ret:
							return "bool";
						case Family.field:
						default:
							return "byte";
					}
				case "bool*":
					return "byte*";
				default:
					return type;
			}
		}

		/// <summary>
		/// Pass-through. ThorVG keeps its C identifiers verbatim — see <see cref="NormalizeTypeName"/>
		/// for the rationale. Kept with the signature the C2CSharpBinding pattern expects so that
		/// prefix stripping can be turned on later from a single place.
		/// </summary>
		public static string StripPrefix(string name) => name;

		/// <summary>
		/// Pass-through, for the same reason as <see cref="StripPrefix"/>: <c>Tvg_Matrix.e11</c> must
		/// keep reading exactly like the upstream documentation.
		/// </summary>
		public static string PascalCaseField(string name) => name;

		/// <summary>
		/// Pass-through: enum values keep their SCREAMING_CASE C spelling (<c>TVG_RESULT_SUCCESS</c>).
		/// </summary>
		public static string ScreamingToPascalCase(string screaming) => screaming;

		/// <summary>
		/// Longest common prefix at underscore boundaries. Unused while the pass-through naming is
		/// in effect, but required by the C2CSharpBinding contract.
		/// </summary>
		public static string FindCommonPrefix(IEnumerable<string> names)
		{
			var list = names.ToList();
			if (list.Count < 2)
			{
				return string.Empty;
			}

			string first = list[0];
			int prefixLen = first.Length;

			for (int i = 1; i < list.Count; i++)
			{
				prefixLen = Math.Min(prefixLen, list[i].Length);
				for (int j = 0; j < prefixLen; j++)
				{
					if (first[j] != list[i][j])
					{
						prefixLen = j;
						break;
					}
				}
			}

			string commonPrefix = first.Substring(0, prefixLen);

			int lastUnderscore = commonPrefix.LastIndexOf('_');
			return lastUnderscore >= 0 ? commonPrefix.Substring(0, lastUnderscore + 1) : string.Empty;
		}

		public static string EscapeReservedKeyword(string name)
		{
			switch (name)
			{
				case "abstract": case "as": case "base": case "bool": case "break":
				case "byte": case "case": case "catch": case "char": case "checked":
				case "class": case "const": case "continue": case "decimal": case "default":
				case "delegate": case "do": case "double": case "else": case "enum":
				case "event": case "explicit": case "extern": case "false": case "finally":
				case "fixed": case "float": case "for": case "foreach": case "goto":
				case "if": case "implicit": case "in": case "int": case "interface":
				case "internal": case "is": case "lock": case "long": case "namespace":
				case "new": case "null": case "object": case "operator": case "out":
				case "override": case "params": case "private": case "protected": case "public":
				case "readonly": case "ref": case "return": case "sbyte": case "sealed":
				case "short": case "sizeof": case "stackalloc": case "static": case "string":
				case "struct": case "switch": case "this": case "throw": case "true":
				case "try": case "typeof": case "uint": case "ulong": case "unchecked":
				case "unsafe": case "ushort": case "using": case "virtual": case "void":
				case "volatile": case "while":
					return "@" + name;
				default:
					return name;
			}
		}

		/// <summary>
		/// True for C# primitive types, which are the only ones a <c>fixed</c> buffer accepts.
		/// </summary>
		public static bool IsFixedBufferElement(string csType)
		{
			switch (csType)
			{
				case "bool": case "byte": case "sbyte": case "short": case "ushort":
				case "int": case "uint": case "long": case "ulong": case "char":
				case "float": case "double":
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Total element count of a (possibly multi-dimensional) C array, and its innermost type.
		/// </summary>
		public static int GetFlattenedArrayLength(CppArrayType arrayType, out CppType elementType)
		{
			int total = arrayType.Size;
			CppType current = arrayType.ElementType;

			while (current is CppArrayType nested)
			{
				total *= nested.Size;
				current = nested.ElementType;
			}

			elementType = current;
			return total;
		}

		/// <summary>
		/// Parses a macro value into a C# literal plus its type. Returns false for anything that is
		/// not a plain numeric constant — thorvg_capi.h defines TVG_API and TVG_DEPRECATED as
		/// (mjENABLED, mjMAX) and alias macros (mju_sqrt -&gt; sqrt) which have no C# equivalent.
		/// </summary>
		public static bool TryParseMacroValue(string value, out string csValue, out string csType)
		{
			csValue = null;
			csType = null;

			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}

			value = value.Trim();

			// Drop a single layer of wrapping parentheses: "(-1)" -> "-1".
			while (value.Length > 2 && value[0] == '(' && value[value.Length - 1] == ')')
			{
				var inner = value.Substring(1, value.Length - 2);
				if (inner.IndexOf('(') >= 0 || inner.IndexOf(')') >= 0)
				{
					break;
				}

				value = inner.Trim();
			}

			if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				if (uint.TryParse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
				{
					csValue = value;
					csType = "uint";
					return true;
				}

				return false;
			}

			if (int.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _))
			{
				csValue = value;
				csType = "int";
				return true;
			}

			var floatCandidate = value.TrimEnd('f', 'F');
			if (double.TryParse(floatCandidate, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
			{
				csValue = floatCandidate;
				csType = "double";
				return true;
			}

			return false;
		}

		public static void PrintComments(TextWriter file, CppComment comment, string tabs = "", bool newLine = false)
		{
			if (comment == null)
			{
				return;
			}

			var lines = new List<string>();
			CollectText(comment, lines);

			if (lines.Count == 0)
			{
				return;
			}

			if (newLine)
			{
				file.WriteLine();
			}

			file.WriteLine($"{tabs}/// <summary>");
			foreach (var line in lines)
			{
				file.WriteLine($"{tabs}/// {EscapeXml(line)}");
			}

			file.WriteLine($"{tabs}/// </summary>");
		}

		private static void CollectText(CppComment comment, List<string> lines)
		{
			switch (comment.Kind)
			{
				case CppCommentKind.Text:
					var text = ((CppCommentTextBase)comment).Text;
					if (!string.IsNullOrWhiteSpace(text))
					{
						lines.Add(text.Trim());
					}

					break;
				case CppCommentKind.Paragraph:
				case CppCommentKind.Full:
					if (comment.Children != null)
					{
						foreach (var child in comment.Children)
						{
							CollectText(child, lines);
						}
					}

					break;
			}
		}

		/// <summary>
		/// ThorVG's header comments are Doxygen prose containing &lt;, &gt; and &amp; (e.g. "min&gt;=max: ignore"),
		/// which would produce malformed XML documentation.
		/// </summary>
		public static string EscapeXml(string text)
		{
			return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
		}
	}
}
