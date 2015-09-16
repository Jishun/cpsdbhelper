<DacpacExtractor xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    <!--For format/options example of the latest version, please take a look at https://github.com/djsxp/cpsdbhelper/blob/master/project/CpsDbHelper.CodeGerator.BuildTask/CodeGeneratorSettings.xml.pp -->
    <ModelNamespace>$rootnamespace$</ModelNamespace>
    <DbProjectPath>DbProjectPath</DbProjectPath>
    <DalNamespace>$rootnamespace$.DataAccess</DalNamespace>
    <ModelOutPath>./Models</ModelOutPath>
    <DalOutPath>./Models</DalOutPath>
    <DataAccessClassName>DataAccess</DataAccessClassName>
    <FileNameExtensionPrefix>Generated</FileNameExtensionPrefix>
    <Enabled>true</Enabled>
    <UseAsync>true</UseAsync>
    <ErrorIfDacpacNotFound>true</ErrorIfDacpacNotFound>
    <EnabledInConfigurations>Debug</EnabledInConfigurations>
    <EnabledInConfigurations>Release</EnabledInConfigurations>
    <ColumnOverrides>
        <Name>[dbo].[Table2].[Id]</Name>
        <Type>bigint</Type>
        <Nullable>true</Nullable>
    </ColumnOverrides>
    <PluralMappings>
        <EntityName>Example</EntityName>
        <PluralForm>Examples</PluralForm>
        <!--can be removed by default generator appends 's'-->
    </PluralMappings>
    <PluralMappings>
        <EntityName>Tax</EntityName>
        <PluralForm>Taxes</PluralForm>
        <!--to map non 's' plural forms to get method name mroe pretty-->
    </PluralMappings>
    <EnumMappings>
        <ColumnFullName>[dbo].[Table1].[TableStatus]</ColumnFullName>
        <EnumTypeName>TableStatusEnum</EnumTypeName>
        <!--to map non 's' plural forms to get method name mroe pretty-->
    </EnumMappings>
    <ObjectsToIgnore>[dbo].[Table1]</ObjectsToIgnore>
    <ObjectsToIgnore>[dbo].[Table2].[Id]</ObjectsToIgnore>
</DacpacExtractor>