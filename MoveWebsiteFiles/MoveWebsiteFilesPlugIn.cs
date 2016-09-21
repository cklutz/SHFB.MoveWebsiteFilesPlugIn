using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml.XPath;
using Sandcastle.Core;
using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

namespace MoveWebsiteFiles
{
    [HelpFileBuilderPlugInExport(
        "MoveWebsiteFiles",
        Version = AssemblyInfo.ProductVersion,
        Copyright = AssemblyInfo.Copyright,
        Description = "A plugin for the Sandcastle Help File Builder that moves the website files, instead of copying them.",
        IsConfigurable = true)]
    public sealed class MoveWebsiteFilesPlugIn : IPlugIn
    {
        private readonly HelpFileBuilderPlugInExportAttribute _metadata;
        private List<ExecutionPoint> _executionPoints;
        private BuildProcess _builder;
        private ConfigurationData _configurationData;

        public MoveWebsiteFilesPlugIn()
        {
            _metadata = (HelpFileBuilderPlugInExportAttribute)GetType().GetCustomAttributes(typeof(HelpFileBuilderPlugInExportAttribute), false)[0];
        }

        /// <summary>
        /// This read-only property returns a collection of execution points that define when the plug-in should
        /// be invoked during the build process.
        /// </summary>
        public IEnumerable<ExecutionPoint> ExecutionPoints
        {
            get
            {
                if (_executionPoints == null)
                {
                    _executionPoints = new List<ExecutionPoint>
                    {
                        new ExecutionPoint(BuildStep.CopyingWebsiteFiles, ExecutionBehaviors.Before)
                    };
                }
                return _executionPoints;
            }
        }

        /// <summary>
        /// This method is used by the Sandcastle Help File Builder to let the plug-in perform its own
        /// configuration.
        /// </summary>
        /// <param name="project">A reference to the active project</param>
        /// <param name="currentConfig">The current configuration XML fragment</param>
        /// <returns>A string containing the new configuration XML fragment</returns>
        /// <remarks>The configuration data will be stored in the help file builder project</remarks>
        public string ConfigurePlugIn(SandcastleProject project, string currentConfig)
        {
            var configuration = ConfigurationData.FromXml(project, currentConfig);
            using (var form = new ConfigurationForm(configuration))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    return ConfigurationData.ToXml(form.NewConfiguration);
                }
            }

            return currentConfig;
        }

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the build process
        /// </summary>
        /// <param name="buildProcess">A reference to the current build process</param>
        /// <param name="configuration">The configuration data that the plug-in should use to initialize itself</param>
        public void Initialize(BuildProcess buildProcess, XPathNavigator configuration)
        {
            _builder = buildProcess;
            _builder.ReportProgress("{0} Version {1}\r\n{2}", _metadata.Id, _metadata.Version, _metadata.Copyright);
            _configurationData = ConfigurationData.FromXml(buildProcess.CurrentProject, configuration);
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            if (context.BuildStep == BuildStep.CopyingWebsiteFiles)
            {
                string outputFolder = _builder.OutputFolder;
                string workingFolder = _builder.WorkingFolder;
                string webWorkingFolder = string.Format(CultureInfo.InvariantCulture, "{0}Output\\{1}", workingFolder, HelpFileFormats.Website);

                _builder.ReportProgress("Moving website files from '{0}' to '{1}'...", webWorkingFolder, outputFolder);

                var sw = Stopwatch.StartNew();
                if (_configurationData.UseDirectMove)
                {
                    DirectMove(webWorkingFolder, outputFolder);
                    sw.Stop();
                    _builder.ReportProgress("Moved files for the website content in {0}.", sw.Elapsed);
                }
                else
                {
                    int fileCount = 0;
                    ManualMove(webWorkingFolder, outputFolder, ref fileCount);
                    sw.Stop();
                    _builder.ReportProgress("Moved {0} files for the website content in {1}.", fileCount, sw.Elapsed);
                }
            }
        }

        private static void DirectMove(string sourcePath, string destPath)
        {
            if (!Directory.Exists(destPath))
                Directory.CreateDirectory(destPath);

            foreach (var entry in Directory.EnumerateDirectories(sourcePath))
            {
                Directory.Move(entry, Path.Combine(destPath, Path.GetFileName(entry)));
            }
            foreach (var entry in Directory.EnumerateFiles(sourcePath))
            {
                File.Move(entry, Path.Combine(destPath, Path.GetFileName(entry)));
            }
       }

        private void ManualMove(string sourcePath, string destPath, ref int fileCount)
        {
            foreach (string name in Directory.EnumerateFiles(sourcePath))
            {
                if (!Directory.Exists(destPath))
                    Directory.CreateDirectory(destPath);

                File.Move(name, Path.Combine(destPath, Path.GetFileName(name)));
                fileCount++;

                if ((fileCount % 500) == 0)
                    _builder.ReportProgress("Moved {0} files", fileCount);
            }

            foreach (string folder in Directory.EnumerateDirectories(sourcePath))
            {
                ManualMove(folder, Path.Combine(destPath, Path.GetFileName(folder)), ref fileCount);
            }
        }

        public void Dispose()
        {
        }
    }
}
