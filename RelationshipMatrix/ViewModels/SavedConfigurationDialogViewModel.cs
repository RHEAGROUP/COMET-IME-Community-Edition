﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SavedConfigurationDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2019 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru.
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.ViewModels
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using CDP4Composition.Navigation;
    using CDP4Composition.PluginSettingService;
    using DialogResult;
    using ReactiveUI;
    using Settings;

    /// <summary>
    /// ViewModel for the dialog to save matrix configuration
    /// </summary>
    public class SavedConfigurationDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Backing field for the <see cref="Name"/> property.
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for the <see cref="Description"/> property.
        /// </summary>
        private string description;

        /// <summary>
        /// The dialog navigation service.
        /// </summary>
        private IDialogNavigationService dialogNavigationService;

        /// <summary>
        /// The plugin settings service.
        /// </summary>
        private IPluginSettingsService pluginSettingService;

        /// <summary>
        /// The configuration to be saved.
        /// </summary>
        private SavedConfiguration savedConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="SavedConfigurationDialogViewModel"/> class.
        /// </summary>
        /// <param name="dialogNavigationService">An instance of <see cref="IDialogNavigationService"/>.</param>
        /// <param name="pluginSettingService">An instance of <see cref="IPluginSettingsService"/>.</param>
        /// <param name="configuration">The configuration to be saved.</param>
        public SavedConfigurationDialogViewModel(IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingService, SavedConfiguration configuration)
        {
            // reset the loading indicator
            this.IsBusy = false;

            this.dialogNavigationService = dialogNavigationService;
            this.pluginSettingService = pluginSettingService;
            this.savedConfiguration = configuration;

            var canOk = this.WhenAnyValue(
                vm => vm.Name,
                vm => vm.Description,
                (n, d) =>
                    !string.IsNullOrEmpty(n) && !string.IsNullOrEmpty(d));

            this.OkCommand = ReactiveCommand.CreateAsyncTask(canOk, x => this.ExecuteOk(), RxApp.MainThreadScheduler);
            this.OkCommand.ThrownExceptions.Select(ex => ex).Subscribe(x =>
            {
                this.ErrorMessage = x.Message;
            });

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());
        }

        /// <summary>
        /// Gets the Ok Command
        /// </summary>
        public ReactiveCommand<Unit> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel Command
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// Gets or sets the name of the saved configuration.
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref this.name, value);
            }
        }

        /// <summary>
        /// Gets or sets the description of the saved configuration.
        /// </summary>
        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref this.description, value);
            }
        }

        /// <summary>
        /// Executes the Ok Command and write the configuration to disk
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task ExecuteOk()
        {
            var settings = this.pluginSettingService.Read<RelationshipMatrixPluginSettings>();

            this.savedConfiguration.Name = this.Name;
            this.savedConfiguration.Description = this.Description;

            settings.SavedConfigurations.Add(this.savedConfiguration);

            this.IsBusy = true;

            try
            {
                this.LoadingMessage = "Saving Configuration...";
                await Task.Run(() => this.pluginSettingService.Write(settings));

                this.DialogResult = new SavedConfigurationResult(true);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Executes the Cancel Command and do not write the configuration to disk
        /// </summary>
        private void ExecuteCancel()
        {
            this.DialogResult = new SavedConfigurationResult(false);
        }
    }
}