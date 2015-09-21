<DacpacExtractor xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"
     SaveAsync="false" GetAsync="false" DeleteAsync="false" Enabled="true" ErrorIfDacpacNotFound="true"
    EnablePrimaryKey="true" EnableUniqueIndex="true" EnableNonUniqueIndex="true" EnableForeignKey="true">
    <!--For format/options example of the latest version, please take a look at https://github.com/djsxp/cpsdbhelper/blob/master/project/CpsDbHelper.CodeGerator.BuildTask/CodeGeneratorSettings.xml.pp-->
    <ModelNamespace>$rootnamespace$</ModelNamespace>
    <DbProjectPath><!--example:-->../CpsDbHelper.TestDatabase\CpsDbHelper.TestDatabase.sqlproj</DbProjectPath>
    <DalNamespace>$rootnamespace$.DataAccess</DalNamespace>
    <ModelOutPath>./Models</ModelOutPath>
    <DalOutPath>./Models</DalOutPath>
    <DataAccessClassName>CpsDbHelperDataAccess</DataAccessClassName>
    <FileNameExtensionPrefix>Generated</FileNameExtensionPrefix>
    <EnabledInConfigurations>Debug</EnabledInConfigurations>
    <ClassAccess>public</ClassAccess>
    <!--Uncomment below entries to enable more options-->
    <!--<Usings>System.Runtime.Serialization</Usings>-->
    <!--<EnabledInConfigurations>Release</EnabledInConfigurations>-->   <!--Allow Multiple-->
    <!--<PluralMappings EntityName="Tax"  PluralForm="Taxes"/>-->    <!--to map non 's' plural forms to get method name mroe pretty-->    <!--Allow Multiple-->
    <!--<EnumMappings ColumnFullName="[dbo].[Table1].[TableStatus]" EnumTypeName="TableStatusEnum"/>--><!--Allow Multiple-->
    <!--<AsyncMappings IndexName="[dbo].[PK_Table2]" SaveAsync="false" GetAsync="false" DeleteAsync="false"/>--><!--Allow Multiple-->
    <!--<AsyncMappings IndexName="[dbo].[Table1].[IX_Table2_Name1]" SaveAsync="false" GetAsync="false" DeleteAsync="false"/>--><!--Allow Multiple-->
    <!--<ColumnOverrides Name="[dbo].[Table1].[Name]" Type="int">--><!--Allow Multiple--><!--
        <Nullable>false</Nullable>
        <Annotations>DataMember</Annotations>--><!--Allow Multiple--><!--
    </ColumnOverrides>-->
    <!--<EntityOverrides TableName="[dbo].[Table1]" Name="TableAnother" ClassAccess="public" IncludeForeignKey="true">-->
    <!--Allow Multiple--><!--
        <Annotations>DataContract</Annotations>--><!--Allow Multiple--><!--
    </EntityOverrides>-->
</DacpacExtractor>
