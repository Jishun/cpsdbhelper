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
        public override bool Execute()
        {
            var m = new BuildMessageEventArgs("DbHelper code generating", "", "CpsDbHelperBuildTask", MessageImportance.Normal);
            BuildEngine.LogMessageEvent(m);

            var proj = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(ProjectPath).FirstOrDefault() ?? new Project(ProjectPath);
            var settingsFile = proj.GetItemsByEvaluatedInclude("CodeGeneratorSettings.xml").FirstOrDefault();
            if (settingsFile != null)
            {
                var xml = XDocument.Load(settingsFile.UnevaluatedInclude);
                var parser = xml.Root.FromXElement<DacpacExtractor>();
                if (!File.Exists(parser.DbProjectPath))
                {
                    var me = new BuildErrorEventArgs("DbHelper code generator", "", ProjectPath, 0, 0, 0, 0, "please config 'DbProjectPath' in 'CodeGeneratorSettings.xml'", "CodeGeneratorSettings.xml", "CpsDbHelperCodeGenerator", DateTime.Now, MessageImportance.High);
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
                parser.ParseDacpac(dacpac);
            }
            else
            {
                var me = new BuildErrorEventArgs("DbHelper code generator", "", ProjectPath, 0, 0, 0, 0, "Unable to find 'CodeGeneratorSettings.xml'", "CodeGeneratorSettings.xml", "CpsDbHelperCodeGenerator", DateTime.Now, MessageImportance.High);
                BuildEngine.LogErrorEvent(me);
                return false;
            }
            
            return true;
        }
    }
}
