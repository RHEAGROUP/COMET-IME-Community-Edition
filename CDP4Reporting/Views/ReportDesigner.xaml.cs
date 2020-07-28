﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportDesigner.xaml.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.Views
{
    using System;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Events;
    using CDP4Composition.Reporting;
    using CDP4Dal;
    using CDP4Reporting.ViewModels;

    using DevExpress.DataAccess.ObjectBinding;
    using DevExpress.Xpf.Bars;
    using DevExpress.XtraReports.Parameters;
    using DevExpress.XtraReports.Security;
    using DevExpress.XtraReports.UI;
    using ReactiveUI;

    /// <summary>
    /// Interaction logic for ReportDesigner.xaml
    /// </summary>
    [PanelViewExport(RegionNames.EditorPanel)]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class ReportDesigner : IPanelView
    {
        /// <summary>
        /// The <see cref="Task"/> that executes the compile command
        /// </summary>
        private Task compileTask;

        /// <summary>
        /// The <see cref="CancellationTokenSource"/> used to stop a compile task
        /// </summary>
        private readonly CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// The <see cref="CancellationToken"/> generated by the cancellationTokenSource in order to stop compile task
        /// </summary>
        protected CancellationToken cancellationToken;

        /// <summary>
        /// The subscription to display the taskbar notification
        /// </summary>
        private IDisposable subscription;

        /// <summary>
        /// Struct that maps streams on current archive zip file path
        /// </summary>
        private struct ReportZipArchive
        {
            public Stream Repx;
            public Stream DataSource;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportDesigner"/> class.
        /// </summary>
        public ReportDesigner()
        {
            this.InitializeComponent();

            ScriptPermissionManager.GlobalInstance = new ScriptPermissionManager(ExecutionMode.Deny);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportDesigner"/> class.
        /// </summary>
        /// <param name="initializeComponent">A value indicating whether the contained Components shall be loaded</param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ReportDesigner(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();

                this.cancellationTokenSource = new CancellationTokenSource();
                this.cancellationToken = this.cancellationTokenSource.Token;

                this.reportDesigner.ActiveDocumentChanged += this.ReportDesigner_ActiveDocumentChanged;
                this.textEditor.TextChanged += this.TextEditor_TextChanged;

                this.subscription = CDPMessageBus.Current.Listen<ReportDesignerEvent>()
                                    .ObserveOn(RxApp.MainThreadScheduler)
                                    .Subscribe(this.ReportNotifications);
            }
        }

        /// <summary>
        /// Trigger text changed editor event
        /// </summary>
        /// <param name="sender">The caller</param>
        /// <param name="e">The <see cref="EventArgs"/></param>
        private void TextEditor_TextChanged(object sender, EventArgs e)
        {
            var viewModel = this.DataContext as ReportDesignerViewModel;

            if (viewModel != null && !viewModel.IsAutoBuildEnabled)
            {
                return;
            }

            if (this.compileTask != null && this.compileTask.Status.Equals(TaskStatus.Running))
            {
                this.cancellationTokenSource.Cancel();
            }

            this.compileTask = Task.Run(async delegate
            {
                this.cancellationToken.ThrowIfCancellationRequested();

                Thread.Sleep(5 * 1000);

                if (viewModel != null)
                {
                    await viewModel.AutomaticBuildScript();
                }
            }, this.cancellationToken);
        }

        /// <summary>
        /// Trigger active document changed event, when a new report was loaded
        /// </summary>
        /// <param name="sender">The caller</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/></param>
        private void ReportDesigner_ActiveDocumentChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                return;
            }

            var viewModel = this.DataContext as ReportDesignerViewModel;

            if (viewModel != null && (viewModel.Thing == null || viewModel.BuildResult == null))
            {
                return;
            }

            if (viewModel.BuildResult.Errors.Count != 0)
            {
                return;
            }

            this.Dispatcher.InvokeAsync(this.SetDataSource, DispatcherPriority.ApplicationIdle);
        }



        /// <summary>
        /// Trigger context menu action
        /// </summary>
        /// <param name="sender">The caller</param>
        /// <param name="e">The <see cref="ItemClickEventArgs"/></param>
        private void BarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            switch (e.Item.Content.ToString())
            {
                case "Copy":
                    this.OutputTextBox.Copy();
                    break;
                case "Clear":
                    (this.DataContext as ReportDesignerViewModel).Output = string.Empty;
                    break;
                case "Rebuild Schema":
                    this.Dispatcher.InvokeAsync(this.SetDataSource, DispatcherPriority.ApplicationIdle);
                    break;
            }
        }

        /// <summary>
        /// Trigger textboxes text changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (e.Source == this.OutputTextBox)
            {
                this.lgTabs.SelectTab(this.lgOutput);
            }
            else if (e.Source == this.ErrorTextBox && !string.IsNullOrEmpty(this.ErrorTextBox.Text))
            {
                this.lgTabs.SelectTab(this.lgErrors);
            }
        }

        /// <summary>
        /// Set report datasource
        /// </summary>
        private void SetDataSource()
        {
            if (this.reportDesigner.ActiveDocument == null)
            {
                (this.DataContext as ReportDesignerViewModel).Output += $"{DateTime.Now:HH:mm:ss} Report not found.{Environment.NewLine}";
                return;
            }

            var dataSourceName = "ReportBudgetDataSource";
            var localReport = this.reportDesigner.ActiveDocument.Report;
            var dataSource = localReport.ComponentStorage.OfType<ObjectDataSource>().ToList().FirstOrDefault(x => x.Name.Equals(dataSourceName));

            if (dataSource == null)
            {
                // Create new datasource
                dataSource = new ObjectDataSource
                {
                    DataSource = this.GetDataSource(),
                    Name = dataSourceName
                };
                localReport.ComponentStorage.Add(dataSource);
                localReport.DataSource = dataSource;
            }
            else
            {
                // Use existing datasource
                dataSource.DataSource = this.GetDataSource();
            }

            // Rebuild datasource schema always
            dataSource.RebuildResultSchema();
        }

        /// <summary>
        /// Get tabular data representation for the report
        /// </summary>
        /// <returns></returns>
        private object GetDataSource()
        {
            var viewModel = this.DataContext as ReportDesignerViewModel;

            if (viewModel.BuildResult == null)
            {
                viewModel.Output += $"{DateTime.Now:HH:mm:ss} Build data source code first.{Environment.NewLine}";
                return null;
            }

            var editorFullClassName = viewModel.BuildResult.CompiledAssembly.GetTypes().FirstOrDefault(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReportingDataSource<>))).FullName;
            var instObj = viewModel.BuildResult.CompiledAssembly.CreateInstance(editorFullClassName);

            if (instObj == null)
            {
                viewModel.Output += $"{DateTime.Now:HH:mm:ss} Data source class not found.{Environment.NewLine}";
                return null;
            }

            var dsObj = instObj.GetType().GetMethod("CreateDataSource").Invoke(instObj, new object[] { viewModel.Thing });

            return dsObj?.GetType().GetMethod("GetTable").Invoke(dsObj, new object[] { });
        }

        /// <summary>
        /// Process report notification
        /// </summary>
        /// <param name="notificationEvent">The <see cref="ReportDesignerEvent"/> containing the data to show</param>
        private void ReportNotifications(ReportDesignerEvent notificationEvent)
        {
            switch (notificationEvent.NotificationKind)
            {
                case ReportNotificationKind.REPORT_OPEN:
                    OpenReportStream(notificationEvent.Rep4File);
                    break;
                case ReportNotificationKind.REPORT_SAVE:
                    SaveReportStream(notificationEvent.Rep4File);
                    break;
            }

            this.Dispatcher.InvokeAsync(this.SetDataSource, DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// Extract zip archive
        /// </summary>
        /// <param name="reportArchive"></param>
        private void OpenReportStream(string reportArchiveFile)
        {
            var report = new XtraReport();
            var reportStream = this.GetReportStream(reportArchiveFile);

            report.LoadLayoutFromXml(reportStream.Repx);

            this.reportDesigner.OpenDocument(report);

            if (reportStream.DataSource != null)
            {
                this.textEditor.Text = new StreamReader(reportStream.DataSource).ReadToEnd();
            }
        }

        /// <summary>
        /// Build zip archive
        private void SaveReportStream(string reportArchiveFile)
        {
            if (File.Exists(reportArchiveFile))
            {
                File.Delete(reportArchiveFile);
            }

            var reportStream = new MemoryStream();
            this.reportDesigner.ActiveDocument.Report.SaveLayoutToXml(reportStream);
            var dataSourceStream = new MemoryStream(Encoding.ASCII.GetBytes(this.textEditor.Text));

            using (var zipFile = ZipFile.Open(reportArchiveFile, ZipArchiveMode.Create))
            {
                using (var reportEntry = zipFile.CreateEntry("Report.repx").Open())
                {
                    reportStream.Position = 0;
                    reportStream.CopyTo(reportEntry);
                }

                using (var reportEntry = zipFile.CreateEntry("Datasource.cs").Open())
                {
                    dataSourceStream.Position = 0;
                    dataSourceStream.CopyTo(reportEntry);
                }
            }
        }

        /// <summary>
        /// Get report zip archive components
        /// </summary>
        /// <param name="rep4File">archive zip file</param>
        /// <returns></returns>
        private ReportZipArchive GetReportStream(string rep4File)
        {
            var zipFile = ZipFile.OpenRead(rep4File);

            return new ReportZipArchive()
            {
                Repx = zipFile.Entries.FirstOrDefault(x => x.Name.EndsWith(".repx"))?.Open(),
                DataSource = zipFile.Entries.FirstOrDefault(x => x.Name.EndsWith(".cs"))?.Open()
            };
        }
    }
}
