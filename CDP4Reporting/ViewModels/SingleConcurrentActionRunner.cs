﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SingleTaskRunner.cs" company="RHEA System S.A.">
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.ViewModels
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The <see cref="SingleConcurrentActionRunner"/> runs a single task at a time.
    /// If a <see cref="Action"/> is already scheduled, the current execution
    /// will be cancelled and the new Action will be scheduled for execution.
    /// </summary>
    public class SingleConcurrentActionRunner
    {
        /// <summary>
        /// The <see cref="Task"/> that executes the <see cref="Action"/>
        /// </summary>
        private Task currentTask;

        /// <summary>
        /// The <see cref="CancellationTokenSource"/> used to stop execution
        /// of the <see cref="Task"/> that executes the <see cref="Action"/>
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// The <see cref="CancellationToken"/> generated by the cancellationTokenSource
        /// in order to stop the <see cref="Task"/> that executes the <see cref="Action"/>
        /// </summary>
        protected CancellationToken cancellationToken;

        /// <summary>
        /// Cancels the <see cref="Task"/> that executes the <see cref="Action"/>
        /// </summary>
        public void CancelCurrentTask()
        {
            if (this.currentTask != null && !this.currentTask.Status.Equals(TaskStatus.Canceled))
            {
                this.cancellationTokenSource.Cancel();
            }
        }

        /// <summary>
        /// Creates a task that delayed executes the <see cref="Action"/> 
        /// </summary>
        /// <param name="action">The <see cref="Action"/></param>
        /// <param name="milliseconds">Milliseconds to delay the execution of the <see cref="Action"/></param>
        public void DelayRunAction(Action action, int milliseconds)
        {
            this.CancelCurrentTask();

            this.cancellationTokenSource = new CancellationTokenSource();
            this.cancellationToken = this.cancellationTokenSource.Token;

            this.currentTask = Task.Delay(milliseconds, this.cancellationToken)
                .ContinueWith(_ =>
                {
                    this.cancellationToken.ThrowIfCancellationRequested();
                    action.Invoke();
                }, this.cancellationToken);
        }

        /// <summary>
        /// Executes the <see cref="Action"/> immediately
        /// </summary>
        /// <param name="action">The <see cref="Action"/></param>
        public void RunAction(Action action)
        {
            this.DelayRunAction(action, 0);
        }
    }
}