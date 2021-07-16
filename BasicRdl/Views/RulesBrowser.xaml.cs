﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RulesBrowser.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;
    using DevExpress.Xpf.Grid;
    using NLog;

    /// <summary>
    /// Interaction logic for <see cref="RulesBrowser"/>
    /// </summary>
    [PanelViewExport(RegionNames.LeftPanel)]
    public partial class RulesBrowser : IPanelView
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="RulesBrowser"/> class.
        /// </summary>
        public RulesBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RulesBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public RulesBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
