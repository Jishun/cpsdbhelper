using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CpsDbHelper.CodeGenerator;
using DotNetUtils;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace CpsDbHelper.CodeGenerator.BuildTask
{
    public class CpsDbHelperBuildTask : Task
    {
        public string ProjectPath { get; set; }
        public string Configuration { get; set; }
        public override bool Execute()
        {
            var m = new BuildMessageEventArgs("DbHelper code generating", "", "CpsDbHelperBuildTask", MessageImportance.Normal);
            BuildEngine.LogMessageEvent(m);

            var proj = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(ProjectPath).FirstOrDefault() ?? new Project(ProjectPath);
            var settingsFile = proj.GetItemsByEvaluatedInclude(DacpacExtractor.ConfigFileName).FirstOrDefault();
            if (settingsFile != null)
            {
                var xml = XDocument.Load(settingsFile.UnevaluatedInclude);
                string error;
                var parser = DacpacExtractor.LoadFromXml(xml.Root, out error);
                if (parser == null)
                {
                    var me = new BuildErrorEventArgs("DbHelper code generator", "", ProjectPath, 0, 0, 0, 0, error, DacpacExtractor.ConfigFileName, "CpsDbHelperCodeGenerator", DateTime.Now, MessageImportance.High);
                    BuildEngine.LogErrorEvent(me);
                    return false;
                }
                if (Configuration != null && parser.EnabledInConfigurations != null && !parser.EnabledInConfigurations.Contains(Configuration))
                {
                    return true;
                }
                if (!File.Exists(parser.DbProjectPath))
                {
                    var me = new BuildErrorEventArgs("DbHelper code generator", "", ProjectPath, 0, 0, 0, 0, "please config 'DbProjectPath' in '{0}'".FormatInvariantCulture(DacpacExtractor.ConfigFileName), DacpacExtractor.ConfigFileName, "CpsDbHelperCodeGenerator", DateTime.Now, MessageImportance.High);
                    BuildEngine.LogErrorEvent(me);
                    return false;
                }
                var dbProj = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(Path.GetFullPath(parser.DbProjectPath)).FirstOrDefault() ?? new Project(Path.GetFullPath(parser.DbProjectPath));
                if (dbProj.IsDirty)
                {
                    dbProj.Build();
                }
                var fileName = dbProj.GetPropertyValue("SqlTargetFile");
                var dir = dbProj.GetPropertyValue("TargetDir");
                var dacpac = Path.Combine(dir, fileName);
                if (File.Exists(dacpac))
                {
                    var errorMessage = parser.ParseDacpac(dacpac);
                    if (errorMessage != null)
                    {
                        var me = new BuildErrorEventArgs("DbHelper code generator", "", ProjectPath, 0, 0, 0, 0, errorMessage, DacpacExtractor.ConfigFileName, "CpsDbHelperCodeGenerator", DateTime.Now, MessageImportance.High);
                        BuildEngine.LogErrorEvent(me);
                        return false;
                    }
                }
                else if(parser.ErrorIfDacpacNotFound)
                {
                    var me = new BuildErrorEventArgs("DbHelper code generator", "", ProjectPath, 0, 0, 0, 0, "Cannot find dacpac file", DacpacExtractor.ConfigFileName, "CpsDbHelperCodeGenerator", DateTime.Now, MessageImportance.High);
                    BuildEngine.LogErrorEvent(me);
                    return false;
                }
                else
                {
                    var me = new BuildMessageEventArgs("Cannot find dacpac file, skip code generating", "", "CpsDbHelperBuildTask", MessageImportance.Normal);
                    BuildEngine.LogMessageEvent(me);
                    return true;
                }
            }
            else
            {
                var me = new BuildErrorEventArgs("DbHelper code generator", "", ProjectPath, 0, 0, 0, 0, "Unable to find '{0}'".FormatInvariantCulture(), "CodeGeneratorSettings.xml", DacpacExtractor.ConfigFileName, DateTime.Now, MessageImportance.High);
                BuildEngine.LogErrorEvent(me);
                return false;
            }
            
            return true;
        }
    }
}
