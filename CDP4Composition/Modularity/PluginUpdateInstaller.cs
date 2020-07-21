﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginUpdateInstaller.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4Composition.Modularity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;

    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Utilities;
    using CDP4Composition.ViewModels;
    using CDP4Composition.Views;

    using Microsoft.Practices.ServiceLocation;

    using Newtonsoft.Json;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="PluginUpdateInstaller"/> is responsible to check all the CDP4 download folders and to install/update the availables user-selected plugins 
    /// </summary>
    public class PluginUpdateInstaller
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private Logger logger = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Holds an <see cref="IEnumerable{T}"/> of type <code>(string pluginDownloadFullPath, Manifest theNewManifest, bool isImeCompatible)</code>
        /// of the updatable plugins
        /// </summary>
        public IEnumerable<(FileInfo pluginDownloadFullPath, Manifest theNewManifest)> UpdatablePlugins { get; }

        /// <summary>
        /// Instantiate a new <see cref="PluginUpdateInstaller"/>
        /// </summary>
        public PluginUpdateInstaller()
        {
            this.UpdatablePlugins = PluginUtilities.GetDownloadedInstallablePluginUpdate().ToList();

            if (this.UpdatablePlugins.Any())
            {
                new PluginInstaller() { DataContext = new PluginInstallerViewModel(this.UpdatablePlugins) }.ShowDialog();
            }
        }
    }
}
