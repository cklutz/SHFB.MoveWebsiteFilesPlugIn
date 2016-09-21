using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

namespace PerfDiag
{
    [HelpFileBuilderPlugInExport("PerfDiag", Version = AssemblyInfo.ProductVersion,
      Copyright = AssemblyInfo.Copyright, 
      Description = "A plugin that hooks onto every buildstep's before and after behaviours and " +
        "reports the total runtime of each step.")]
    public sealed class PerfDiagPlugIn : IPlugIn
    {
        private List<ExecutionPoint> _executionPoints;
        private BuildProcess _builder;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private BuildStep _lastBuildStep = BuildStep.None;

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
                    _executionPoints = new List<ExecutionPoint>();
                    foreach (BuildStep step in Enum.GetValues(typeof(BuildStep)))
                    {
                        // Not a valid build step for a plugin.
                        if (step == BuildStep.None)
                            continue;

                        // These are "complicated" (see ExecutionPoint.cs source).
                        if (step == BuildStep.Canceled ||
                            step == BuildStep.Failed ||
                            step == BuildStep.Initializing)
                            continue;

                        _executionPoints.Add(new ExecutionPoint(step, ExecutionBehaviors.BeforeAndAfter, Int32.MaxValue));
                    }
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
            MessageBox.Show("This plug-in has no configurable settings", "Build Process Plug-In",
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

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            _builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            if (context.Behavior == ExecutionBehaviors.Before)
            {
                if (_lastBuildStep != BuildStep.None && _stopwatch.ElapsedMilliseconds > 100)
                {
                    _stopwatch.Stop();
                    Message("Elapsed time between BuildStep '{0}' and '{1}' was {2}.", _lastBuildStep, context.BuildStep, _stopwatch.Elapsed);
                }
                _stopwatch.Restart();
            }
            else if (context.Behavior == ExecutionBehaviors.After)
            {
                _stopwatch.Stop();
                Message("BuildStep '{0}' completed in {1}.", context.BuildStep, _stopwatch.Elapsed);
                _stopwatch.Restart();
                _lastBuildStep = context.BuildStep;
            }

        }

        private void Message(string format, params object[] args)
        {
            Console.WriteLine("PERF: " + format, args);
            _builder.ReportProgress(format, args);
        }

        public void Dispose()
        {
        }
    }
}
