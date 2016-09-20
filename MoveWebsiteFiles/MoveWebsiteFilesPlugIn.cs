using System;
using System.Collections.Generic;
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
        Description = "MoveWebsiteFiles plug-in")]
    public sealed class MoveWebsiteFilesPlugIn : IPlugIn
    {
        private readonly HelpFileBuilderPlugInExportAttribute _metadata;
        private List<ExecutionPoint> _executionPoints;
        private BuildProcess _builder;

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
            MessageBox.Show("This plug-in has no configurable settings", _metadata.Id,
                MessageBoxButtons.OK, MessageBoxIcon.Information);

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
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            if (context.BuildStep == BuildStep.CopyingWebsiteFiles)
            {
                _builder.ReportProgress("Moving website files...");

                string workingFolder = _builder.WorkingFolder;
                string outputFolder = _builder.OutputFolder;
                string webWorkingFolder = String.Format(CultureInfo.InvariantCulture, "{0}Output\\{1}", workingFolder, HelpFileFormats.Website);

                int fileCount = 0;
                RecursiveMove(webWorkingFolder + "\\*.*", outputFolder, ref fileCount);
                _builder.ReportProgress("Moved {0} files for the webiste content", fileCount);
            }
        }

        private void RecursiveMove(string sourcePath, string destPath, ref int fileCount)
        {
            if (sourcePath == null)
                throw new ArgumentNullException("sourcePath");

            if (destPath == null)
                throw new ArgumentNullException("destPath");

            int idx = sourcePath.LastIndexOf('\\');

            string dirName = sourcePath.Substring(0, idx), fileSpec = sourcePath.Substring(idx + 1);

            foreach (string name in Directory.EnumerateFiles(dirName, fileSpec))
            {
                var filename = destPath + Path.GetFileName(name);

                if (!Directory.Exists(destPath))
                    Directory.CreateDirectory(destPath);

                // All attributes are turned off so that we can delete it later
                File.Move(name, filename);
                File.SetAttributes(filename, FileAttributes.Normal);

                fileCount++;

                if ((fileCount % 500) == 0)
                    _builder.ReportProgress("Moved {0} files", fileCount);
            }

            // For "*.*", copy subfolders too
            if (fileSpec == "*.*")
            {
                // Ignore hidden folders as they may be under source control and are not wanted
                foreach (string folder in Directory.EnumerateDirectories(dirName))
                {
                    if ((File.GetAttributes(folder) & FileAttributes.Hidden) != FileAttributes.Hidden)
                    {
                        RecursiveMove(folder + @"\*.*", destPath + folder.Substring(dirName.Length + 1) + @"\", ref fileCount);
                    }
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
