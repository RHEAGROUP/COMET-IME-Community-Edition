﻿//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ModuleExportNameAttribute.cs" company="RHEA System S.A.">
//     Copyright (c) 2015-2020 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
//             Nathanael Smiechowski, Kamil Wojnowski
// 
//     This file is part of CDP4-IME Community Edition.
//     The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//     compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-IME Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or any later version.
// 
//     The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//     GNU Affero General Public License for more details.
// 
//     You should have received a copy of the GNU Affero General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Attributes
{
    using System;
    using System.ComponentModel.Composition;

    /// <summary>
    /// The purpose of the <see cref="ModuleExportAttribute"/> is to decorate <see cref="IModule"/> implementations
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [MetadataAttribute]
    public class ModuleExportNameAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleExportNameAttribute"/> class.
        /// </summary>
        /// <param name="type">
        /// The <see cref="Type"/> of the module to be exported.
        /// </param>
        /// <param name="name">
        /// The human readable name of the <see cref="IModule"/> implementation that is being decorated
        /// </param>
        /// <param name="isMandatory">
        ///Specifies if the plugin should be mandatory for the application
        /// </param>
        public ModuleExportNameAttribute(Type type, string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the human readable name of the exported <see cref="IModule"/>
        /// </summary>
        public string Name { get; private set; }
    }
}
