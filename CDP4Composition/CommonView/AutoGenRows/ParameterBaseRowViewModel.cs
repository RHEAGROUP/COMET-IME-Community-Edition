// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterBaseRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition.
//    This is an auto-generated class. Any manual changes to this file will be overwritten!
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using ReactiveUI;

    /// <summary>
    /// Row class representing a <see cref="ParameterBase"/>
    /// </summary>
    public abstract partial class ParameterBaseRowViewModel<T> : RowViewModelBase<T> where T : ParameterBase
    {
        /// <summary>
        /// Backing field for <see cref="Group"/> property
        /// </summary>
        private ParameterGroup group;

        /// <summary>
        /// Backing field for <see cref="GroupName"/> property
        /// </summary>
        private string groupName;

        /// <summary>
        /// Backing field for <see cref="IsOptionDependent"/> property
        /// </summary>
        private bool isOptionDependent;

        /// <summary>
        /// Backing field for <see cref="Owner"/> property
        /// </summary>
        private DomainOfExpertise owner;

        /// <summary>
        /// Backing field for <see cref="OwnerName"/> property
        /// </summary>
        private string ownerName;

        /// <summary>
        /// Backing field for <see cref="OwnerShortName"/> property
        /// </summary>
        private string ownerShortName;

        /// <summary>
        /// Backing field for <see cref="ParameterType"/> property
        /// </summary>
        private ParameterType parameterType;

        /// <summary>
        /// Backing field for <see cref="ParameterTypeName"/> property
        /// </summary>
        private string parameterTypeName;

        /// <summary>
        /// Backing field for <see cref="ParameterTypeShortName"/> property
        /// </summary>
        private string parameterTypeShortName;

        /// <summary>
        /// Backing field for <see cref="Scale"/> property
        /// </summary>
        private MeasurementScale scale;

        /// <summary>
        /// Backing field for <see cref="ScaleName"/> property
        /// </summary>
        private string scaleName;

        /// <summary>
        /// Backing field for <see cref="ScaleShortName"/> property
        /// </summary>
        private string scaleShortName;

        /// <summary>
        /// Backing field for <see cref="StateDependence"/> property
        /// </summary>
        private ActualFiniteStateList stateDependence;

        /// <summary>
        /// Backing field for <see cref="StateDependenceName"/> property
        /// </summary>
        private string stateDependenceName;

        /// <summary>
        /// Backing field for <see cref="StateDependenceShortName"/> property
        /// </summary>
        private string stateDependenceShortName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterBaseRowViewModel{T}"/> class
        /// </summary>
        /// <param name="parameterBase">The <see cref="ParameterBase"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        protected ParameterBaseRowViewModel(T parameterBase, ISession session, IViewModelBase<Thing> containerViewModel) : base(parameterBase, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the Group
        /// </summary>
        public ParameterGroup Group
        {
            get { return this.group; }
            set { this.RaiseAndSetIfChanged(ref this.group, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Group"/>
        /// </summary>
        public string GroupName
        {
            get { return this.groupName; }
            set { this.RaiseAndSetIfChanged(ref this.groupName, value); }
        }

        /// <summary>
        /// Gets or sets the IsOptionDependent
        /// </summary>
        public bool IsOptionDependent
        {
            get { return this.isOptionDependent; }
            set { this.RaiseAndSetIfChanged(ref this.isOptionDependent, value); }
        }

        /// <summary>
        /// Gets or sets the Owner
        /// </summary>
        public DomainOfExpertise Owner
        {
            get { return this.owner; }
            set { this.RaiseAndSetIfChanged(ref this.owner, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Owner"/>
        /// </summary>
        public string OwnerName
        {
            get { return this.ownerName; }
            set { this.RaiseAndSetIfChanged(ref this.ownerName, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="Owner"/>
        /// </summary>
        public string OwnerShortName
        {
            get { return this.ownerShortName; }
            set { this.RaiseAndSetIfChanged(ref this.ownerShortName, value); }
        }

        /// <summary>
        /// Gets or sets the ParameterType
        /// </summary>
        public ParameterType ParameterType
        {
            get { return this.parameterType; }
            set { this.RaiseAndSetIfChanged(ref this.parameterType, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="ParameterType"/>
        /// </summary>
        public string ParameterTypeName
        {
            get { return this.parameterTypeName; }
            set { this.RaiseAndSetIfChanged(ref this.parameterTypeName, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="ParameterType"/>
        /// </summary>
        public string ParameterTypeShortName
        {
            get { return this.parameterTypeShortName; }
            set { this.RaiseAndSetIfChanged(ref this.parameterTypeShortName, value); }
        }

        /// <summary>
        /// Gets or sets the Scale
        /// </summary>
        public MeasurementScale Scale
        {
            get { return this.scale; }
            set { this.RaiseAndSetIfChanged(ref this.scale, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Scale"/>
        /// </summary>
        public string ScaleName
        {
            get { return this.scaleName; }
            set { this.RaiseAndSetIfChanged(ref this.scaleName, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="Scale"/>
        /// </summary>
        public string ScaleShortName
        {
            get { return this.scaleShortName; }
            set { this.RaiseAndSetIfChanged(ref this.scaleShortName, value); }
        }

        /// <summary>
        /// Gets or sets the StateDependence
        /// </summary>
        public ActualFiniteStateList StateDependence
        {
            get { return this.stateDependence; }
            set { this.RaiseAndSetIfChanged(ref this.stateDependence, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="StateDependence"/>
        /// </summary>
        public string StateDependenceName
        {
            get { return this.stateDependenceName; }
            set { this.RaiseAndSetIfChanged(ref this.stateDependenceName, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="StateDependence"/>
        /// </summary>
        public string StateDependenceShortName
        {
            get { return this.stateDependenceShortName; }
            set { this.RaiseAndSetIfChanged(ref this.stateDependenceShortName, value); }
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
            this.Group = this.Thing.Group;
            if (this.Thing.Group != null)
            {
                this.GroupName = this.Thing.Group.Name;
            }
            else
            {
                this.GroupName = string.Empty;
            }
            this.IsOptionDependent = this.Thing.IsOptionDependent;
            this.Owner = this.Thing.Owner;
            if (this.Thing.Owner != null)
            {
                this.OwnerName = this.Thing.Owner.Name;
                this.OwnerShortName = this.Thing.Owner.ShortName;
            }
            else
            {
                this.OwnerName = string.Empty;
                this.OwnerShortName = string.Empty;
            }
            this.ParameterType = this.Thing.ParameterType;
            if (this.Thing.ParameterType != null)
            {
                this.ParameterTypeName = this.Thing.ParameterType.Name;
                this.ParameterTypeShortName = this.Thing.ParameterType.ShortName;
            }
            else
            {
                this.ParameterTypeName = string.Empty;
                this.ParameterTypeShortName = string.Empty;
            }
            this.Scale = this.Thing.Scale;
            if (this.Thing.Scale != null)
            {
                this.ScaleName = this.Thing.Scale.Name;
                this.ScaleShortName = this.Thing.Scale.ShortName;
            }
            else
            {
                this.ScaleName = string.Empty;
                this.ScaleShortName = string.Empty;
            }
            this.StateDependence = this.Thing.StateDependence;
            if (this.Thing.StateDependence != null)
            {
                this.StateDependenceName = this.Thing.StateDependence.Name;
                this.StateDependenceShortName = this.Thing.StateDependence.ShortName;
            }
            else
            {
                this.StateDependenceName = string.Empty;
                this.StateDependenceShortName = string.Empty;
            }
        }
    }
}
