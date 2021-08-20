﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BahaviorRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software{colon} you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation{colon} either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY{colon} without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.CommonView.HandCodedRows
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// A row view model for <see cref="Behavior" />
    /// </summary>
    public class BehaviorRowViewModel : RowViewModelBase<Behavior>
    {
        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="BehavioralModelKind"/>
        /// </summary>
        private BehavioralModelKind behavioralModelKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorRowViewModel" /> class.
        /// </summary>
        /// <param name="behavior">The <see cref="Behavior"/> that is represented by the current view-model</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public BehaviorRowViewModel(Behavior behavior, ISession session, IViewModelBase<Thing> containerViewModel)
                : base(behavior, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Name"/>
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="BehavioralModelKind"/>
        /// </summary>
        public BehavioralModelKind BehavioralModelKind
        {
            get { return this.behavioralModelKind; }
            set { this.RaiseAndSetIfChanged(ref this.behavioralModelKind, value); }
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.ModifiedOn = this.Thing.ModifiedOn;
            this.Name = this.Thing.Name;
            this.BehavioralModelKind = this.Thing.BehavioralModelKind;
        }
    }
}
