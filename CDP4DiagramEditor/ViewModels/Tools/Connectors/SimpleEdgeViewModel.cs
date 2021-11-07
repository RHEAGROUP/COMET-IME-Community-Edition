﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleEdgeViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed, Simon Wood
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.ViewModels
{
    using System.Linq;
    using System.Text;

    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Diagram;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    /// <summary>
    /// View model for a Simple diagram edge
    /// </summary>
    public class SimpleEdgeViewModel : DrawnDiagramEdgeViewModel, IPersistedConnector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleEdgeViewModel" /> class
        /// </summary>
        /// <param name="diagramEdge">The associated <see cref="DiagramEdge" /></param>
        /// <param name="session">The <see cref="ISession" /></param>
        /// <param name="container">The container <see cref="IDiagramEditorViewModel" /></param>
        public SimpleEdgeViewModel(DiagramEdge diagramEdge, ISession session, IDiagramEditorViewModel container) : base(diagramEdge, session, container)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent" /> event-handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent" /></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Update the transaction with the data contained in this view-model
        /// </summary>
        /// <param name="transaction">The transaction to update</param>
        /// <param name="container">The container</param>
        public override void UpdateTransaction(IThingTransaction transaction, DiagramElementContainer container)
        {
            var clone = this.DiagramThing.Clone(true);

            clone.Name = this.DisplayedText;

            container.DiagramElement.Add(clone);
            transaction.CreateOrUpdate(clone);
        }

        /// <summary>
        /// Updates the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.DisplayedText = this.DiagramThing.Name;
        }
    }
}
