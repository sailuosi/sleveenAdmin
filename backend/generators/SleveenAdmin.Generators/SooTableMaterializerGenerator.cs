using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SleveenAdmin.Generators
{
    /// <summary>
    /// 扫描 [SooTable] 实体，生成 mooSQL 读侧物化器、写侧 Accessor 与 EntityInfo 工厂（不引用 mooSQL.Pure.Generators）。
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

            var table = ParseTableAttr(context.Attributes);
            var members = new List<MemberGenInfo>();
            var columns = new List<ColumnGenInfo>();

            foreach (var member in typeSymbol.GetMembers())
            {
                if (member is IPropertySymbol prop && prop.SetMethod != null && !prop.IsStatic)
                {
                    var typeName = prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    var columnName = GetColumnName(prop);
                    members.Add(new MemberGenInfo(columnName, prop.Name, typeName));
                    var col = TryParseColumn(prop, typeName);
                    if (col != null)
                        columns.Add(col);
                }
                else if (member is IFieldSymbol field && !field.IsStatic && !field.IsReadOnly && !field.IsImplicitlyDeclared)
                {
                    var typeName = field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    var columnName = GetColumnName(field);
                    members.Add(new MemberGenInfo(columnName, field.Name, typeName));
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
                members,
                columns,
                table);
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
                    if (TryGetInt(named.Value, out var num) && num == DbTableTypeSelect)
                        return true;
                }
            }
            return false;
        }

        private static TableGenInfo ParseTableAttr(ImmutableArray<AttributeData> attributes)
        {
            var table = new TableGenInfo();
            foreach (var attr in attributes)
            {
                if (attr.AttributeClass?.ToDisplayString() != SooTableAttributeName)
                    continue;

                if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string ctorName)
                    table.Name = ctorName;

                foreach (var named in attr.NamedArguments)
                {
                    switch (named.Key)
                    {
                        case "Name" when named.Value.Value is string n:
                            table.Name = n;
                            break;
                        case "Schema" when named.Value.Value is string s:
                            table.Schema = s;
                            break;
                        case "Database" when named.Value.Value is string d:
                            table.Database = d;
                            break;
                        case "Server" when named.Value.Value is string sv:
                            table.Server = sv;
                            break;
                        case "Caption" when named.Value.Value is string c:
                            table.Caption = c;
                            break;
                        case "Alias" when named.Value.Value is string a:
                            table.Alias = a;
                            break;
                        case "ViewSQL" when named.Value.Value is string v:
                            table.ViewSQL = v;
                            break;
                        case "Type" when TryGetInt(named.Value, out var t):
                            table.Type = t;
                            break;
                        case "LiveName" when named.Value.Value is bool ln:
                            table.LiveName = ln;
                            table.HasLiveName = true;
                            break;
                        case "ShardMode" when TryGetInt(named.Value, out var sm):
                            table.ShardMode = sm;
                            break;
                        case "NameTemplate" when named.Value.Value is string nt:
                            table.NameTemplate = nt;
                            break;
                        case "ShardAnchor" when named.Value.Value is string sa:
                            table.ShardAnchor = sa;
                            break;
                        case "ShardIntervalValue" when TryGetInt(named.Value, out var siv):
                            table.ShardIntervalValue = siv;
                            break;
                        case "ShardIntervalUnit" when named.Value.Value is string siu:
                            table.ShardIntervalUnit = siu;
                            break;
                        case "ShardFirstSpan" when TryGetInt(named.Value, out var sfs):
                            table.ShardFirstSpan = sfs;
                            table.HasShardFirstSpan = true;
                            break;
                        case "DefaultRecentTables" when TryGetInt(named.Value, out var drt):
                            table.DefaultRecentTables = drt;
                            break;
                        case "MaxTablesPerQuery" when TryGetInt(named.Value, out var mtq):
                            table.MaxTablesPerQuery = mtq;
                            table.HasMaxTablesPerQuery = true;
                            break;
                        case "AutoCreateOnInsert" when named.Value.Value is bool ac:
                            table.AutoCreateOnInsert = ac;
                            break;
                        case "CustomShardStrategyType":
                            table.CustomShardStrategyType = GetTypeOfFullName(named.Value);
                            break;
                    }
                }
            }
            return table;
        }

        private static ColumnGenInfo? TryParseColumn(IPropertySymbol prop, string typeName)
        {
            AttributeData? colAttr = null;
            foreach (var attr in prop.GetAttributes())
            {
                if (attr.AttributeClass?.ToDisplayString() == SooColumnAttributeName)
                {
                    colAttr = attr;
                    break;
                }
            }
            if (colAttr == null)
                return null;

            var col = new ColumnGenInfo
            {
                MemberName = prop.Name,
                TypeName = typeName,
                IsColumn = true,
            };

            if (colAttr.ConstructorArguments.Length > 0 && colAttr.ConstructorArguments[0].Value is string ctorName
                && !string.IsNullOrWhiteSpace(ctorName))
            {
                col.ColumnName = ctorName;
                col.HasExplicitName = true;
            }

            foreach (var named in colAttr.NamedArguments)
            {
                switch (named.Key)
                {
                    case "Name" when named.Value.Value is string n && !string.IsNullOrWhiteSpace(n):
                        col.ColumnName = n;
                        col.HasExplicitName = true;
                        break;
                    case "IsPrimaryKey" when named.Value.Value is bool pk:
                        col.IsPrimaryKey = pk;
                        col.HasIsPrimaryKey = true;
                        break;
                    case "CanBeNull" when named.Value.Value is bool nb:
                        col.CanBeNull = nb;
                        col.HasCanBeNull = true;
                        break;
                    case "IsIdentity" when named.Value.Value is bool id:
                        col.IsIdentity = id;
                        col.HasIsIdentity = true;
                        break;
                    case "SkipOnInsert" when named.Value.Value is bool si:
                        col.SkipOnInsert = si;
                        col.HasSkipOnInsert = true;
                        break;
                    case "SkipOnUpdate" when named.Value.Value is bool su:
                        col.SkipOnUpdate = su;
                        col.HasSkipOnUpdate = true;
                        break;
                    case "Length" when TryGetInt(named.Value, out var len):
                        col.Length = len;
                        col.HasLength = true;
                        break;
                    case "Precision" when TryGetInt(named.Value, out var prec):
                        col.Precision = prec;
                        col.HasPrecision = true;
                        break;
                    case "Scale" when TryGetInt(named.Value, out var scale):
                        col.Scale = scale;
                        col.HasScale = true;
                        break;
                    case "Caption" when named.Value.Value is string cap:
                        col.Caption = cap;
                        break;
                    case "DataType" when TryGetInt(named.Value, out var dt):
                        col.DataType = dt;
                        col.HasDataType = true;
                        break;
                    case "Kind" when TryGetInt(named.Value, out var kind):
                        col.Kind = kind;
                        col.HasKind = true;
                        break;
                    case "AutoKey" when named.Value.Value is string ak:
                        col.AutoKey = ak;
                        break;
                    case "KeyGeneratorType":
                        col.KeyGeneratorType = GetTypeOfFullName(named.Value);
                        break;
                    case "IsColumn" when named.Value.Value is bool ic:
                        col.IsColumn = ic;
                        break;
                    case "Shard" when named.Value.Value is bool sh:
                        col.Shard = sh;
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(col.ColumnName))
                col.ColumnName = prop.Name;

            return col;
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

        private static bool TryGetInt(TypedConstant value, out int result)
        {
            result = 0;
            if (value.Value is null)
                return false;
            try
            {
                result = Convert.ToInt32(value.Value, CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string? GetTypeOfFullName(TypedConstant tc)
        {
            if (tc.Kind == TypedConstantKind.Type && tc.Value is ITypeSymbol ts)
                return ts.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return null;
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
                context.AddSource($"{SanitizeFileName(type.TypeName)}_EntityMeta.g.cs", SourceText.From(GenerateEntityMeta(type), Encoding.UTF8));

                var typeFull = type.TypeFullName;
                var nsPrefix = string.IsNullOrEmpty(type.Namespace) ? string.Empty : $"global::{type.Namespace}.";
                var materializerType = $"{nsPrefix}{type.TypeName}_Materializer";
                var accessorType = $"{nsPrefix}{type.TypeName}_Accessor";
                var metaType = $"{nsPrefix}{type.TypeName}_EntityMeta";
                // 仅登记 Client 安装器（非 MaterializerRegistry 静态查找表）；由 registerGeneratedMaterializers → Apply
                registration.AppendLine($"            GeneratedClientBootstrap.Add(c =>");
                registration.AppendLine("            {");
                registration.AppendLine($"                c.RegisterEntityFactory(typeof({typeFull}), {metaType}.Create);");
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

        private static string Escape(string? value)
        {
            if (value == null) return string.Empty;
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
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
            sb.AppendLine("        public object ReadValue(object entity, string propertyName)");
            sb.AppendLine("        {");
            sb.AppendLine($"            var e = ({type.TypeFullName})entity;");
            sb.AppendLine("            switch (propertyName)");
            sb.AppendLine("            {");
            foreach (var member in type.Members)
            {
                sb.AppendLine($"                case \"{Escape(member.MemberName)}\":");
                sb.AppendLine($"                    return e.{member.MemberName};");
            }
            sb.AppendLine("                default:");
            sb.AppendLine("                    return null;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public void WriteValue(object entity, string propertyName, object value)");
            sb.AppendLine("        {");
            sb.AppendLine($"            var e = ({type.TypeFullName})entity;");
            sb.AppendLine("            switch (propertyName)");
            sb.AppendLine("            {");
            foreach (var member in type.Members)
            {
                sb.AppendLine($"                case \"{Escape(member.MemberName)}\":");
                sb.AppendLine($"                    e.{member.MemberName} = ({member.TypeName})value;");
                sb.AppendLine("                    break;");
            }
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public bool IsNull(object entity, string propertyName)");
            sb.AppendLine("            => ReadValue(entity, propertyName) == null;");
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
                sb.AppendLine($"                    case \"{Escape(member.ColumnName)}\":");
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

        private static string GenerateEntityMeta(TypeGenInfo type)
        {
            var ns = string.IsNullOrEmpty(type.Namespace) ? "SleveenAdmin.Generated" : type.Namespace;
            var t = type.Table;
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Reflection;");
            sb.AppendLine("using mooSQL.data;");
            sb.AppendLine("using mooSQL.data.Mapping;");
            sb.AppendLine("using mooSQL.data.model;");
            sb.AppendLine();
            sb.AppendLine($"namespace {ns}");
            sb.AppendLine("{");
            sb.AppendLine($"    internal static class {type.TypeName}_EntityMeta");
            sb.AppendLine("    {");
            sb.AppendLine("        internal static EntityInfo Create()");
            sb.AppendLine("        {");
            sb.AppendLine("            var info = new EntityInfo();");
            sb.AppendLine($"            info.Type = typeof({type.TypeFullName});");
            sb.AppendLine($"            info.EntityName = \"{Escape(type.TypeName)}\";");

            if (!string.IsNullOrWhiteSpace(t.Name))
                sb.AppendLine($"            info.DbTableName = \"{Escape(t.Name)}\";");
            if (!string.IsNullOrWhiteSpace(t.Schema))
                sb.AppendLine($"            info.SchemaName = \"{Escape(t.Schema)}\";");
            if (!string.IsNullOrWhiteSpace(t.Database))
                sb.AppendLine($"            info.DatabaseName = \"{Escape(t.Database)}\";");
            if (!string.IsNullOrWhiteSpace(t.Server))
                sb.AppendLine($"            info.ServerName = \"{Escape(t.Server)}\";");
            if (t.HasLiveName)
                sb.AppendLine($"            info.LiveName = {(t.LiveName ? "true" : "false")};");
            if (!string.IsNullOrWhiteSpace(t.Caption))
                sb.AppendLine($"            info.TableDescription = \"{Escape(t.Caption)}\";");
            if (!string.IsNullOrWhiteSpace(t.Alias))
                sb.AppendLine($"            info.Alias = \"{Escape(t.Alias)}\";");
            if (!string.IsNullOrWhiteSpace(t.ViewSQL))
                sb.AppendLine($"            info.ViewSQL = \"{Escape(t.ViewSQL)}\";");

            var dtype = t.Type;
            if (dtype == 0 && !string.IsNullOrWhiteSpace(t.Name))
                dtype = 1; // DBTableType.Table
            sb.AppendLine($"            info.DType = (DBTableType){dtype};");
            if (dtype == 2 || !string.IsNullOrWhiteSpace(t.ViewSQL))
                sb.AppendLine("            info.IsView = true;");

            if (t.ShardMode != 0 || !string.IsNullOrEmpty(t.CustomShardStrategyType))
            {
                sb.AppendLine("            ShardRegistration.ApplyTableAttribute(info, new SooTableAttribute");
                sb.AppendLine("            {");
                if (!string.IsNullOrWhiteSpace(t.Name))
                    sb.AppendLine($"                Name = \"{Escape(t.Name)}\",");
                sb.AppendLine($"                ShardMode = (TableShardMode){t.ShardMode},");
                if (!string.IsNullOrWhiteSpace(t.NameTemplate))
                    sb.AppendLine($"                NameTemplate = \"{Escape(t.NameTemplate)}\",");
                if (!string.IsNullOrWhiteSpace(t.ShardAnchor))
                    sb.AppendLine($"                ShardAnchor = \"{Escape(t.ShardAnchor)}\",");
                if (t.ShardIntervalValue != 0)
                    sb.AppendLine($"                ShardIntervalValue = {t.ShardIntervalValue},");
                if (!string.IsNullOrWhiteSpace(t.ShardIntervalUnit))
                    sb.AppendLine($"                ShardIntervalUnit = \"{Escape(t.ShardIntervalUnit)}\",");
                if (t.HasShardFirstSpan)
                    sb.AppendLine($"                ShardFirstSpan = {t.ShardFirstSpan},");
                if (t.DefaultRecentTables != 0)
                    sb.AppendLine($"                DefaultRecentTables = {t.DefaultRecentTables},");
                if (t.HasMaxTablesPerQuery)
                    sb.AppendLine($"                MaxTablesPerQuery = {t.MaxTablesPerQuery},");
                if (t.AutoCreateOnInsert)
                    sb.AppendLine("                AutoCreateOnInsert = true,");
                if (t.HasLiveName && t.LiveName)
                    sb.AppendLine("                LiveName = true,");
                if (!string.IsNullOrEmpty(t.CustomShardStrategyType))
                    sb.AppendLine($"                CustomShardStrategyType = typeof({t.CustomShardStrategyType}),");
                sb.AppendLine("            });");
            }

            foreach (var col in type.Columns)
            {
                sb.AppendLine("            {");
                sb.AppendLine("                var col = new EntityColumn(info);");
                sb.AppendLine($"                col.PropertyName = \"{Escape(col.MemberName)}\";");
                sb.AppendLine($"                col.DbColumnName = \"{Escape(col.ColumnName)}\";");
                sb.AppendLine($"                col.PropertyInfo = typeof({type.TypeFullName}).GetProperty(\"{Escape(col.MemberName)}\");");
                sb.AppendLine($"                col.UnderType = Nullable.GetUnderlyingType(typeof({col.TypeName})) ?? typeof({col.TypeName});");

                if (string.IsNullOrWhiteSpace(col.ColumnName))
                    sb.AppendLine("                col.IsIgnore = true;");

                if (col.HasLength)
                    sb.AppendLine($"                col.Length = {col.Length};");
                if (col.HasPrecision)
                    sb.AppendLine($"                col.Precision = {col.Precision};");
                if (col.HasScale)
                    sb.AppendLine($"                col.Scale = {col.Scale};");
                if (col.HasIsPrimaryKey)
                    sb.AppendLine($"                col.IsPrimarykey = {(col.IsPrimaryKey ? "true" : "false")};");
                if (col.HasCanBeNull)
                    sb.AppendLine($"                col.IsNullable = {(col.CanBeNull ? "true" : "false")};");
                if (col.HasDataType && col.DataType != 0)
                    sb.AppendLine($"                col.DataType = (DataFam){col.DataType};");
                if (!string.IsNullOrWhiteSpace(col.Caption))
                    sb.AppendLine($"                col.ColumnDescription = \"{Escape(col.Caption)}\";");
                if (col.HasKind && col.Kind != 0)
                    sb.AppendLine($"                col.Kind = (FieldKind){col.Kind};");
                else
                    sb.AppendLine("                col.Kind = FieldKind.Base;");
                if (col.HasIsIdentity)
                    sb.AppendLine($"                col.IsIdentity = {(col.IsIdentity ? "true" : "false")};");

                if (col.HasSkipOnInsert)
                    sb.AppendLine($"                col.IsOnlyIgnoreInsert = {(col.SkipOnInsert ? "true" : "false")};");
                else if (col.HasIsIdentity && col.IsIdentity)
                    sb.AppendLine("                col.IsOnlyIgnoreInsert = true;");

                if (col.HasSkipOnUpdate)
                    sb.AppendLine($"                col.IsOnlyIgnoreUpdate = {(col.SkipOnUpdate ? "true" : "false")};");
                else if (col.HasIsIdentity && col.IsIdentity)
                    sb.AppendLine("                col.IsOnlyIgnoreUpdate = true;");

                if (!string.IsNullOrWhiteSpace(col.AutoKey))
                    sb.AppendLine($"                col.AutoKey = \"{Escape(col.AutoKey!.Trim())}\";");
                if (!string.IsNullOrEmpty(col.KeyGeneratorType))
                    sb.AppendLine($"                col.KeyGeneratorType = typeof({col.KeyGeneratorType});");

                if ((col.HasIsIdentity && col.IsIdentity)
                    && (!string.IsNullOrWhiteSpace(col.AutoKey) || !string.IsNullOrEmpty(col.KeyGeneratorType)))
                {
                    sb.AppendLine($"                throw new InvalidOperationException(\"Column '{Escape(col.MemberName)}' cannot be both IsIdentity and AutoKey/KeyGeneratorType.\");");
                }

                if (!col.IsColumn)
                    sb.AppendLine("                col.IsIgnore = true;");

                if (!col.HasExplicitName)
                {
                    sb.AppendLine($"                if (MooEntityAnalyser.IsNonScalarCollection(typeof({col.TypeName})))");
                    sb.AppendLine("                    col.IsIgnore = true;");
                }

                if (col.Shard)
                    sb.AppendLine("                ShardRegistration.MarkShardField(info, col);");

                sb.AppendLine("                info.AddColumnInfo(col);");
                sb.AppendLine("            }");
            }

            sb.AppendLine("            ShardRegistration.FinalizeEntityShard(info);");
            sb.AppendLine("            return info;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private sealed class TypeGenInfo
        {
            public TypeGenInfo(
                string typeFullName,
                string typeName,
                string ns,
                List<MemberGenInfo> members,
                List<ColumnGenInfo> columns,
                TableGenInfo table)
            {
                TypeFullName = typeFullName;
                TypeName = typeName;
                Namespace = ns;
                Members = members;
                Columns = columns;
                Table = table;
            }

            public string TypeFullName { get; }
            public string TypeName { get; }
            public string Namespace { get; }
            public List<MemberGenInfo> Members { get; }
            public List<ColumnGenInfo> Columns { get; }
            public TableGenInfo Table { get; }
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

        private sealed class TableGenInfo
        {
            public string? Name { get; set; }
            public string? Schema { get; set; }
            public string? Database { get; set; }
            public string? Server { get; set; }
            public string? Caption { get; set; }
            public string? Alias { get; set; }
            public string? ViewSQL { get; set; }
            public int Type { get; set; }
            public bool LiveName { get; set; }
            public bool HasLiveName { get; set; }
            public int ShardMode { get; set; }
            public string? NameTemplate { get; set; }
            public string? ShardAnchor { get; set; }
            public int ShardIntervalValue { get; set; }
            public string? ShardIntervalUnit { get; set; }
            public int ShardFirstSpan { get; set; }
            public bool HasShardFirstSpan { get; set; }
            public int DefaultRecentTables { get; set; }
            public int MaxTablesPerQuery { get; set; }
            public bool HasMaxTablesPerQuery { get; set; }
            public bool AutoCreateOnInsert { get; set; }
            public string? CustomShardStrategyType { get; set; }
        }

        private sealed class ColumnGenInfo
        {
            public string MemberName { get; set; } = "";
            public string ColumnName { get; set; } = "";
            public string TypeName { get; set; } = "";
            public bool HasExplicitName { get; set; }
            public bool HasIsPrimaryKey { get; set; }
            public bool IsPrimaryKey { get; set; }
            public bool HasCanBeNull { get; set; }
            public bool CanBeNull { get; set; }
            public bool HasIsIdentity { get; set; }
            public bool IsIdentity { get; set; }
            public bool HasSkipOnInsert { get; set; }
            public bool SkipOnInsert { get; set; }
            public bool HasSkipOnUpdate { get; set; }
            public bool SkipOnUpdate { get; set; }
            public bool HasLength { get; set; }
            public int Length { get; set; }
            public bool HasPrecision { get; set; }
            public int Precision { get; set; }
            public bool HasScale { get; set; }
            public int Scale { get; set; }
            public string? Caption { get; set; }
            public bool HasDataType { get; set; }
            public int DataType { get; set; }
            public bool HasKind { get; set; }
            public int Kind { get; set; }
            public string? AutoKey { get; set; }
            public string? KeyGeneratorType { get; set; }
            public bool IsColumn { get; set; } = true;
            public bool Shard { get; set; }
        }
    }
}
