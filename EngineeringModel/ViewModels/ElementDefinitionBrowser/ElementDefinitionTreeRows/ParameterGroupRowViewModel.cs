﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterGroupRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4EngineeringModel.Comparers;
    using CDP4EngineeringModel.Utilities;

    /// <summary>
    /// The row representing a <see cref="ParameterGroup"/>
    /// </summary>
    public class ParameterGroupRowViewModel : CDP4CommonView.ParameterGroupRowViewModel, IDropTarget
    {
        #region Fields
        /// <summary>
        /// The <see cref="IComparer{T}"/>
        /// </summary>
        public static readonly IComparer<IRowViewModelBase<Thing>> ChildRowComparer = new ParameterGroupChildRowComparer();

        /// <summary>
        /// The active <see cref="DomainOfExpertise"/>
        /// </summary>
        private DomainOfExpertise currentDomain;

        /// <summary>
        /// The current <see cref="ParameterGroup"/>
        /// </summary>
        private ParameterGroup currentGroup;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterGroupRowViewModel"/> class
        /// </summary>
        /// <param name="parameterGroup">The associated <see cref="ParameterGroup"/></param>
        /// <param name="currentDomain">The active <see cref="DomainOfExpertise"/></param>
        /// <param name="session">The associated <see cref="ISession"/></param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{T}"/> row that contains this row</param>
        public ParameterGroupRowViewModel(ParameterGroup parameterGroup, DomainOfExpertise currentDomain, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(parameterGroup, session, containerViewModel)
        {
            this.currentDomain = currentDomain;
            this.currentGroup = this.Thing.ContainingGroup;
            this.UpdateProperties();
        }
        #endregion

        #region public Properties
        /// <summary>
        /// Gets or sets the <see cref="IThingCreator"/> that is used to create different <see cref="Things"/>.
        /// </summary>
        public IThingCreator ThingCreator { get; set; }

        /// <summary>
        /// Gets a value indicating whether the value set editors are active
        /// </summary>
        public bool IsValueSetEditorActive
        {
            get { return false; }
        }
        #endregion
        
        #region IDropTarget
        /// <summary>
        /// Queries whether a drag can be started
        /// </summary>
        /// <param name="dragInfo">
        /// Information about the drag.
        /// </param>
        /// <remarks>
        /// To allow a drag to be started, the <see cref="IDragInfo.Effects"/> property on <paramref name="dragInfo"/> 
        /// should be set to a value other than <see cref="DragDropEffects.None"/>. 
        /// </remarks>
        public override void StartDrag(IDragInfo dragInfo)
        {
            if (!this.PermissionService.CanWrite(this.Thing))
            {
                dragInfo.Effects = DragDropEffects.None;
                return;
            }

            dragInfo.Payload = this.Thing;
            dragInfo.Effects = DragDropEffects.All;
        }

        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">
        ///  Information about the drag operation.
        /// </param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects"/> property on 
        /// <paramref name="dropInfo"/> should be set to a value other than <see cref="DragDropEffects.None"/>
        /// and <see cref="DropInfo.Payload"/> should be set to a non-null value.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            var parameterTypeAndScale = dropInfo.Payload as Tuple<ParameterType, MeasurementScale>;
            if (parameterTypeAndScale != null)
            {
                this.DragOver(dropInfo, parameterTypeAndScale);
                return;
            }

            // moving the paramenter into a group of the same element definition
            var parameter = dropInfo.Payload as Parameter;
            if (parameter != null)
            {
                this.DragOver(dropInfo, parameter);
                return;
            }

            // moving the group into a group of the same element definition
            var group = dropInfo.Payload as ParameterGroup;
            if (group != null)
            {
                this.DragOver(dropInfo, group);
                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            var parameterTypeAndScale = dropInfo.Payload as Tuple<ParameterType, MeasurementScale>;
            if (parameterTypeAndScale != null)
            {
                await this.Drop(dropInfo, parameterTypeAndScale);
            }

            // moving 
            var parameter = dropInfo.Payload as Parameter;
            if (parameter != null)
            {
                await this.Drop(dropInfo, parameter);
            }

            // moving the group into this group
            var group = dropInfo.Payload as ParameterGroup;
            if (group != null)
            {
                await this.Drop(dropInfo, group);
            }

            dropInfo.Effects = DragDropEffects.None;
        }
        #endregion

        /// <summary>
        /// Update the properties of this row on update
        /// </summary>
        private void UpdateProperties()
        {
            this.UpdateThingStatus();
            // update the group-row under which this row shall be displayed
            if (this.currentGroup != this.Thing.ContainingGroup)
            {
                this.currentGroup = this.Thing.ContainingGroup;
                var elementBaseRow = this.ContainerViewModel as IElementBaseRowViewModel;
                if (elementBaseRow != null)
                {
                    elementBaseRow.UpdateParameterGroupPosition(this.Thing);
                }
            }
        }

        #region Row-Base
        /// <summary>
        /// Update the <see cref="ThingStatus"/> property
        /// </summary>
        protected override void UpdateThingStatus()
        {
            this.ThingStatus = new ThingStatus(this.Thing);
        }

        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        #endregion

        #region Drag-over handler
        /// <summary>
        /// Update the drag state when the payload is a <see cref="Tuple{ParameterType, MeasurementScale}"/>
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/> to update</param>
        /// <param name="parameterTypeAndScale">The <see cref="Tuple{ParameterType, MeasurementScale}"/> payload</param>
        private void DragOver(IDropInfo dropInfo, Tuple<ParameterType, MeasurementScale> parameterTypeAndScale)
        {
            var usage = this.ContainerViewModel as ElementUsageRowViewModel;
            if (usage != null)
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            // check if parameter type is in the chain of rdls
            var model = (EngineeringModel)this.Thing.TopContainer;
            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();
            var rdlChains = new List<ReferenceDataLibrary> { mrdl };
            rdlChains.AddRange(mrdl.RequiredRdls);

            if (!rdlChains.Contains(parameterTypeAndScale.Item1.Container))
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            var parameterType = parameterTypeAndScale.Item1;
            var elementDefinition = this.Thing.Container as ElementDefinition;
            if (elementDefinition != null)
            {
                if (elementDefinition.Parameter.Any(x => x.ParameterType == parameterType))
                {
                    dropInfo.Effects = DragDropEffects.None;
                    return;
                }

                if (!this.PermissionService.CanWrite(ClassKind.Parameter, elementDefinition))
                {
                    dropInfo.Effects = DragDropEffects.None;
                    return;
                }

                dropInfo.Effects = DragDropEffects.Copy;
                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Update the drag state when the payload is a <see cref="Parameter"/>
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/> to update</param>
        /// <param name="parameter">The <see cref="Parameter"/> payload</param>
        private void DragOver(IDropInfo dropInfo, Parameter parameter)
        {
            if (parameter.Container == this.Thing.Container)
            {
                if (!this.PermissionService.CanWrite(parameter))
                {
                    dropInfo.Effects = DragDropEffects.None;
                    return;
                }

                dropInfo.Effects = DragDropEffects.Move;
                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Update the drag state when the payload is a <see cref="ParameterGroup"/>
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/> to update</param>
        /// <param name="group">The <see cref="ParameterGroup"/> payload</param>
        private void DragOver(IDropInfo dropInfo, ParameterGroup group)
        {
            if (group.Container == this.Thing.Container && !group.ContainedGroup(true).Contains(this.Thing) && group != this.Thing)
            {
                if (!this.PermissionService.CanWrite(group))
                {
                    dropInfo.Effects = DragDropEffects.None;
                    return;
                }

                dropInfo.Effects = DragDropEffects.Move;
                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }
        #endregion

        #region Drop Methods
        /// <summary>
        /// Performs the drop operation when the payload is a <see cref="Tuple{ParameterType, MeasurementScale}"/>
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        /// <param name="parameterTypeAndScale">
        /// The <see cref="Tuple{ParameterType, MeasurementScale}"/> payload
        /// </param>
        private async Task Drop(IDropInfo dropInfo, Tuple<ParameterType, MeasurementScale> parameterTypeAndScale)
        {
            var parameterType = parameterTypeAndScale.Item1;
            var measeurementScale = parameterTypeAndScale.Item2;
            var elementDefinition = this.Thing.Container as ElementDefinition;
            if (elementDefinition != null)
            {
                if (elementDefinition.Parameter.Any(x => x.ParameterType == parameterType))
                {
                    dropInfo.Effects = DragDropEffects.None;
                    return;
                }

                if (this.ThingCreator == null)
                {
                    this.ThingCreator = new ThingCreator();
                }

                try
                {
                    await this.ThingCreator.CreateParameter(elementDefinition, this.Thing, parameterType, measeurementScale, this.currentDomain, this.Session);
                }
                catch (Exception ex)
                {
                    this.ErrorMsg = ex.Message;
                }                
            }
        }

        /// <summary>
        /// Performs the drop operation when the payload is a <see cref="Parameter"/>
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        /// <param name="parameter">
        /// The <see cref="Parameter"/> payload
        /// </param>
        private async Task Drop(IDropInfo dropInfo, Parameter parameter)
        {
            if (dropInfo.Effects == DragDropEffects.Move)
            {
                var clone = parameter.Clone(false);
                clone.Group = this.Thing;
                await this.DalWrite(clone);
            }
        }

        /// <summary>
        /// Performs the drop operation when the payload is a <see cref="ParameterGroup"/>
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        /// <param name="group">
        /// The <see cref="ParameterGroup"/> payload
        /// </param>
        private async Task Drop(IDropInfo dropInfo, ParameterGroup group)
        {
            if (dropInfo.Effects == DragDropEffects.Move)
            {
                var clone = group.Clone(false);
                clone.ContainingGroup = this.Thing;
                await this.DalWrite(clone);
            }
        }
        #endregion
    }
}