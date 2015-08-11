using System.IO;
using CpsDbHelper.CodeGenerator;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace CpsDbHelper.CodeGerator.BuildTask
{
    public class CpsDbHelperBuildTask : Task
    {
        [Required]
        public string OutputModelPath { get; set; }
        [Required]
        public string ModelNamespace { get; set; }

        public string DalNamespace { get; set; }

        public string FileExtPrefix { get; set; }

        public override bool Execute()
        {
            var m = new BuildMessageEventArgs("DbHelper code generating", "", "CpsDbHelperBuildTask", MessageImportance.Normal);
            BuildEngine.LogMessageEvent(m);
            var proj = new Project(BuildEngine.ProjectFileOfTaskNode);
            var fileName = proj.GetPropertyValue("SqlTargetFile");
            var dir = proj.GetPropertyValue("TargetDir");
            var dacpac = Path.Combine(dir, fileName);
            DacpacExtractor.ParseDacpac(dacpac, ModelNamespace, OutputModelPath, FileExtPrefix);
            
            return true;
        }
    }
}
