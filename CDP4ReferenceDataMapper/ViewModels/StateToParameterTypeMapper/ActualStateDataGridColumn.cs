﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActualStateDataGridColumn.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace CDP4ReferenceDataMapper.StateToParameterTypeMapper
{
    using System;

    using CDP4Common.EngineeringModelData;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="ActualStateDataGridColumn"/> class represents a column in a data-grid
    /// </summary>
    public class ActualStateDataGridColumn : DataGridColumn
    {
        /// <summary>
        /// Backing field for the <see cref="ActualFiniteState"/> property
        /// </summary>
        private ActualFiniteState actualFiniteState;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActualStateDataGridColumn"/> class
        /// </summary>
        /// <param name="actualFiniteState">
        /// the value for the <see cref="ActualFiniteState"/> property
        /// </param>
        public ActualStateDataGridColumn(ActualFiniteState actualFiniteState)
        {
            this.ActualFiniteState = actualFiniteState;
            this.FieldName = actualFiniteState.ShortName;
        }

        public ActualFiniteState ActualFiniteState
        {
            get { return this.actualFiniteState; }
            set { this.RaiseAndSetIfChanged(ref this.actualFiniteState, value); }
        }
    }
}
