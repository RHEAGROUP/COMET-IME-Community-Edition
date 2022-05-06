﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMessageBusEventHandler.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.MessageBus
{
    using System;

    /// <summary>
    /// Defines the interface of a MessageBusHandler
    /// </summary>
    /// <typeparam name="T">The Type of events that the <see cref="IMessageBusEventHandler{T}"/></typeparam> handles
    public interface IMessageBusEventHandler<in T> : IMessageBusEventHandlerBase
    {
        /// <summary>
        /// Adds an event handler to based on an <see cref="IObservable{T}"/> <see cref="CDPMessageBus"/> event using a specific <see cref="IMessageBusEventHandlerSubscription"/>
        /// </summary>
        /// <param name="messageBusEvent">The <see cref="IObservable{T}"/></param>
        /// <param name="messageBusHandlerData">The <see cref="IMessageBusEventHandlerSubscription"/></param>
        /// <returns>The event handler as an <see cref="IDisposable"/></returns>
        IDisposable RegisterEventHandler(IObservable<T> messageBusEvent, IMessageBusEventHandlerSubscription messageBusHandlerData);

        /// <summary>
        /// Removes an existing <see cref="IMessageBusEventHandlerSubscription"/> from the event handler cache
        /// </summary>
        /// <param name="messageBusHandlerData">The <see cref="IMessageBusEventHandlerSubscription"/></param>
        void UnregisterEventHandler(IMessageBusEventHandlerSubscription messageBusHandlerData);
    }
}