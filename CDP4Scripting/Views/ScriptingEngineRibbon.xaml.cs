﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptingEngineRibbon .cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
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

namespace CDP4Scripting.Views
{
    using System.ComponentModel.Composition;

    using CDP4Composition.Navigation;
    using CDP4Composition.Ribbon;
    using CDP4Composition.Mvvm;

    using CDP4Scripting.Interfaces;
    using CDP4Scripting.ViewModels;

    /// <summary>
    /// Interaction logic for ScriptingEngineRibbonPageGroup.xaml
    /// </summary>
    [Export(typeof(ExtendedRibbonPage))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class ScriptingEngineRibbon : ExtendedRibbonPage, IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptingEngineRibbonPageGroup"/> class
        /// </summary>       
        [ImportingConstructor]
        public ScriptingEngineRibbon(IPanelNavigationService panelNavigationService, IOpenSaveFileDialogService fileDialogService, IScriptingProxy scriptingProxy)
        {
            this.InitializeComponent();
            this.DataContext = new ScriptingEngineRibbonPageGroupViewModel(panelNavigationService, fileDialogService, scriptingProxy);
        }
    }
}
