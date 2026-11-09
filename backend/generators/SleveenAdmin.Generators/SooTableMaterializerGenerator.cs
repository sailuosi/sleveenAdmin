using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SleveenAdmin.Generators
{
    /// <summary>
    /// 扫描 [SooTable] 实体，生成 mooSQL 读侧物化器与写侧 Accessor（不引用 mooSQL.Pure.Generators）。
    /// Type=Select 的查询行类型跳过。
    /// </summary>
    [Generator]
    public sealed class SooTableMaterializerGenerator : IIncrementalGenerator
    {
        private const string SooTableAttributeName = "mooSQL.data.SooTableAttribute";
        private const string SooColumnAttributeName = "mooSQL.data.SooColumnAttribute";
        /// <summary>mooSQL.data.DBTableType.Select</summary>
        private const int DbTableTypeSelect = 4;

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var types = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    SooTableAttributeName,
                    predicate: static (node, _) => node is TypeDeclarationSyntax,
                    transform: static (ctx, _) => GetTypeInfo(ctx))
                .Where(static info => info is not null);

            context.RegisterSourceOutput(types.Collect(), static (spc, source) => Execute(source, spc));
        }

        private static TypeGenInfo? GetTypeInfo(GeneratorAttributeSyntaxContext context)
        {
            if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
                return null;
            if (typeSymbol.TypeKind != TypeKind.Class && typeSymbol.TypeKind != TypeKind.Struct)
                return null;

            if (IsSelectTable(context.Attributes))
                return null;

            var members = new List<MemberGenInfo>();
            foreach (var member in typeSymbol.GetMembers())
            {
                if (member is IPropertySymbol prop && prop.SetMethod != null && !prop.IsStatic)
                {
                    var columnName = GetColumnName(prop);
                    members.Add(new MemberGenInfo(columnName, prop.Name, prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
                }
                else if (member is IFieldSymbol field && !field.IsStatic && !field.IsReadOnly && !field.IsImplicitlyDeclared)
                {
                    var columnName = GetColumnName(field);
                    members.Add(new MemberGenInfo(columnName, field.Name, field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
                }
            }

            if (members.Count == 0)
                return null;

            return new TypeGenInfo(
                typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                typeSymbol.Name,
                typeSymbol.ContainingNamespace.IsGlobalNamespace
                    ? string.Empty
                    : typeSymbol.ContainingNamespace.ToDisplayString(),
                members);
        }

        private static bool IsSelectTable(ImmutableArray<AttributeData> attributes)
        {
            foreach (var attr in attributes)
            {
                if (attr.AttributeClass?.ToDisplayString() != SooTableAttributeName)
                    continue;

                foreach (var named in attr.NamedArguments)
                {
                    if (named.Key != "Type")
                        continue;
                    if (named.Value.Value is int i && i == DbTableTypeSelect)
                        return true;
                    if (named.Value.Value is null)
                        continue;
                    // enum boxed as underlying int or as enum
                    try
                    {
                        var num = System.Convert.ToInt32(named.Value.Value);
                        if (num == DbTableTypeSelect)
                            return true;
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
            return false;
        }

        private static string GetColumnName(ISymbol member)
        {
            foreach (var attr in member.GetAttributes())
            {
                if (attr.AttributeClass?.ToDisplayString() != SooColumnAttributeName)
                    continue;
                if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string name && !string.IsNullOrWhiteSpace(name))
                    return name;
                foreach (var named in attr.NamedArguments)
                {
                    if (named.Key == "Name" && named.Value.Value is string n && !string.IsNullOrWhiteSpace(n))
                        return n;
                }
            }
            return member.Name;
        }

        private static void Execute(ImmutableArray<TypeGenInfo?> types, SourceProductionContext context)
        {
            var valid = types.Where(t => t is not null).Cast<TypeGenInfo>().ToList();
            if (valid.Count == 0) return;

            var registration = new StringBuilder();
            registration.AppendLine("// <auto-generated />");
            registration.AppendLine("using System;");
            registration.AppendLine("using System.Data.Common;");
            registration.AppendLine("using System.Runtime.CompilerServices;");
            registration.AppendLine("using mooSQL.data;");
            registration.AppendLine();
            registration.AppendLine("namespace SleveenAdmin.Generated");
            registration.AppendLine("{");
            registration.AppendLine("    internal static class SooTableMaterializerRegistration");
            registration.AppendLine("    {");
            registration.AppendLine("        [ModuleInitializer]");
            registration.AppendLine("        internal static void Init()");
            registration.AppendLine("        {");

            foreach (var type in valid)
            {
                context.AddSource($"{SanitizeFileName(type.TypeName)}_Materializer.g.cs", SourceText.From(GenerateMaterializer(type), Encoding.UTF8));
                context.AddSource($"{SanitizeFileName(type.TypeName)}_Accessor.g.cs", SourceText.From(GenerateAccessor(type), Encoding.UTF8));

                var typeFull = type.TypeFullName;
                var nsPrefix = string.IsNullOrEmpty(type.Namespace) ? string.Empty : $"global::{type.Namespace}.";
                var materializerType = $"{nsPrefix}{type.TypeName}_Materializer";
                var accessorType = $"{nsPrefix}{type.TypeName}_Accessor";
                // 仅登记 Client 安装器（非 MaterializerRegistry 静态查找表）；由 registerGeneratedMaterializers → Apply
                registration.AppendLine($"            GeneratedClientBootstrap.Add(c =>");
                registration.AppendLine("            {");
                registration.AppendLine($"                c.RegisterMaterializer(typeof({typeFull}), (r, d) => {materializerType}.Deserialize(r, d));");
                registration.AppendLine($"                c.RegisterAccessor(typeof({typeFull}), new {accessorType}());");
                registration.AppendLine("            });");
            }

            registration.AppendLine("        }");
            registration.AppendLine("    }");
            registration.AppendLine("}");

            context.AddSource("SooTableMaterializerRegistration.g.cs", SourceText.From(registration.ToString(), Encoding.UTF8));
        }

        private static string SanitizeFileName(string name)
        {
            var sb = new StringBuilder(name.Length);
            foreach (var c in name)
                sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');
            return sb.ToString();
        }

        private static string GenerateAccessor(TypeGenInfo type)
        {
            var ns = string.IsNullOrEmpty(type.Namespace) ? "SleveenAdmin.Generated" : type.Namespace;
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine("using System;");
            sb.AppendLine("using mooSQL.data;");
            sb.AppendLine();
            sb.AppendLine($"namespace {ns}");
            sb.AppendLine("{");
            sb.AppendLine($"    internal sealed class {type.TypeName}_Accessor : IEntityTypeAccessor");
            sb.AppendLine("    {");
            sb.AppendLine("        public object GetValue(object entity, string propertyName)");
            sb.AppendLine("        {");
            sb.AppendLine($"            var e = ({type.TypeFullName})entity;");
            sb.AppendLine("            switch (propertyName)");
            sb.AppendLine("            {");
            foreach (var member in type.Members)
            {
                sb.AppendLine($"                case \"{member.MemberName}\":");
                sb.AppendLine($"                    return e.{member.MemberName};");
            }
            sb.AppendLine("                default:");
            sb.AppendLine("                    return null;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public void SetValue(object entity, string propertyName, object value)");
            sb.AppendLine("        {");
            sb.AppendLine($"            var e = ({type.TypeFullName})entity;");
            sb.AppendLine("            switch (propertyName)");
            sb.AppendLine("            {");
            foreach (var member in type.Members)
            {
                sb.AppendLine($"                case \"{member.MemberName}\":");
                sb.AppendLine($"                    e.{member.MemberName} = ({member.TypeName})value;");
                sb.AppendLine("                    break;");
            }
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public bool IsNull(object entity, string propertyName)");
            sb.AppendLine("            => GetValue(entity, propertyName) == null;");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string GenerateMaterializer(TypeGenInfo type)
        {
            var ns = string.IsNullOrEmpty(type.Namespace) ? "SleveenAdmin.Generated" : type.Namespace;
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data.Common;");
            sb.AppendLine("using mooSQL.data;");
            sb.AppendLine();
            sb.AppendLine($"namespace {ns}");
            sb.AppendLine("{");
            sb.AppendLine($"    internal static class {type.TypeName}_Materializer");
            sb.AppendLine("    {");
            sb.AppendLine($"        internal static {type.TypeFullName} Deserialize(DbDataReader reader, DBInstance db)");
            sb.AppendLine("        {");
            sb.AppendLine($"            var result = new {type.TypeFullName}();");
            sb.AppendLine("            for (int i = 0; i < reader.FieldCount; i++)");
            sb.AppendLine("            {");
            sb.AppendLine("                switch (reader.GetName(i))");
            sb.AppendLine("                {");

            foreach (var member in type.Members)
            {
                sb.AppendLine($"                    case \"{member.ColumnName}\":");
                sb.AppendLine($"                        result.{member.MemberName} = ({member.TypeName})ColumnValueReader.ReadValue(reader, i, typeof({member.TypeName}), null, db);");
                sb.AppendLine("                        break;");
            }

            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("            return result;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private sealed class TypeGenInfo
        {
            public TypeGenInfo(string typeFullName, string typeName, string ns, List<MemberGenInfo> members)
            {
                TypeFullName = typeFullName;
                TypeName = typeName;
                Namespace = ns;
                Members = members;
            }

            public string TypeFullName { get; }
            public string TypeName { get; }
            public string Namespace { get; }
            public List<MemberGenInfo> Members { get; }
        }

        private sealed class MemberGenInfo
        {
            public MemberGenInfo(string columnName, string memberName, string typeName)
            {
                ColumnName = columnName;
                MemberName = memberName;
                TypeName = typeName;
            }

            public string ColumnName { get; }
            public string MemberName { get; }
            public string TypeName { get; }
        }
    }
}
