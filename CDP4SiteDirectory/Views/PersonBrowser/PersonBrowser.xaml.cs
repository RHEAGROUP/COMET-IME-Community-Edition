﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonBrowser.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;
    using DevExpress.Xpf.Grid;
    using NLog;

    /// <summary>
    /// Interaction logic for PersonBrowser
    /// </summary>
    [PanelViewExport(RegionNames.RightPanel)]
    public partial class PersonBrowser : IPanelView, IPanelFilterableDataGridView
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonBrowser"/> class.
        /// </summary>
        public PersonBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public PersonBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
                this.FilterableControl = this.PersonsTreeListControl;
            }
        }

        /// <summary>
        /// Gets the <see cref="DataControlBase"/> that is to be set up for filtering service.
        /// </summary>
        public DataControlBase FilterableControl { get; private set; }
    }
}
