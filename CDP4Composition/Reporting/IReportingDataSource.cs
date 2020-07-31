﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICDP4ReportingDataSource.cs" company="RHEA System S.A.">
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Reporting
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The interface used for creating a reporting data source.
    /// </summary>
    /// <typeparam name="T">
    /// The <see cref="ReportingDataSourceRow"/> representing the data source rows.
    /// </typeparam>
    public interface IReportingDataSource<T> where T : ReportingDataSourceRow, new()
    {
        /// <summary>
        /// Creates a new data source instance.
        /// </summary>
        /// <param name="option">
        /// The <see cref="Option"/> for which the data source is built.
        /// </param>
        /// <param name="domainOfExpertise">
        /// The <see cref="DomainOfExpertise"/> for which the data source is built.
        /// </param>
        /// <returns>
        /// A new <see cref="ReportingDataSourceClass{T}"/> instance.
        /// </returns>
        ReportingDataSourceClass<T> CreateDataSource(Option option, DomainOfExpertise domainOfExpertise);
    }
}