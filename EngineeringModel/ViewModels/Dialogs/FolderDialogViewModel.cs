﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FolderDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="FolderDialogViewModel"/> is to allow a <see cref="Folder"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="Folder"/> will result in an <see cref="Folder"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.Folder)]
    public class FolderDialogViewModel : CDP4CommonView.FolderDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FolderDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public FolderDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderDialogViewModel"/> class.
        /// </summary>
        /// <param name="folder">
        /// The <see cref="Folder"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name=""></param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="FolderDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="FolderDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/>
        /// </param>
        /// <param name="container">
        /// The Container <see cref="Thing"/> of the created <see cref="Thing"/>
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public FolderDialogViewModel(Folder folder, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, 
            IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(folder, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Populate the possible owners
        /// </summary>
        protected override void PopulatePossibleOwner()
        {
            base.PopulatePossibleOwner();

            var engineeringModel = this.Container.GetContainerOfType<EngineeringModel>();
            var domains = engineeringModel.EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name);
            this.PossibleOwner.AddRange(domains);
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();

            if (this.Container is Folder folder && this.Thing.ContainingFolder == null)
            {
                this.Thing.ContainingFolder = folder;
            }

            this.CreatedOn = this.Thing.CreatedOn.Equals(DateTime.MinValue) ? DateTime.UtcNow : this.Thing.CreatedOn;

            if (this.SelectedCreator == null)
            {
                var iteration = this.Container.GetContainerOfType<Iteration>();

                if (iteration != null)
                {
                    this.Session.OpenIterations.TryGetValue(iteration, out var tuple);
                    this.SelectedCreator = tuple?.Item2;
                }
            }
        }

        /// <summary>
        /// Initialize the <see cref="ICommand"/>s and listeners
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.WhenAnyValue(x => x.SelectedOwner).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(x => x.Name).Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Returns whether it is possible to close the current dialog by clicking the OK button
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.SelectedOwner != null && !string.IsNullOrWhiteSpace(this.Name);
        }
    }
}
