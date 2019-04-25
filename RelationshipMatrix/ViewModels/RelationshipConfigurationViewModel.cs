﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipConfigurationViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// A view-model for dynamic column definition
    /// </summary>
    public class RelationshipConfigurationViewModel : MatrixConfigurationViewModelBase
    {
        /// <summary>
        /// Backing field for <see cref="SelectedRule"/>
        /// </summary>
        private BinaryRelationshipRule selectedRule;

        /// <summary>
        /// The <see cref="IThingDialogNavigationService"/> used to navigate to details dialog of a <see cref="Thing"/>
        /// </summary>
        private readonly IThingDialogNavigationService thingDialogNavigationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipConfigurationViewModel"/>
        /// </summary>
        /// <param name="session">The current session</param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> used to navigate to details dialog of a <see cref="Thing"/>
        /// </param>
        /// <param name="iteration">The current iteration</param>
        /// <param name="action">The action to perform on update</param>
        /// <param name="settings">The module settings</param>
        public RelationshipConfigurationViewModel(ISession session, IThingDialogNavigationService thingDialogNavigationService, Iteration iteration, Action action, RelationshipMatrixPluginSettings settings) : base(session, iteration, action, settings)
        {
            this.thingDialogNavigationService = thingDialogNavigationService;

            this.PossibleRules = new ReactiveList<BinaryRelationshipRule>();
            this.WhenAnyValue(x => x.SelectedRule).Skip(1).Subscribe(_ => this.OnUpdateAction());

            this.InitializeCommands();
        }

        /// <summary>
        /// Gets or sets the <see cref="BinaryRelationshipRule"/> to use
        /// </summary>
        public BinaryRelationshipRule SelectedRule
        {
            get { return this.selectedRule; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRule, value); }
        }

        /// <summary>
        /// Gets the possible <see cref="BinaryRelationshipRule"/>
        /// </summary>
        public ReactiveList<BinaryRelationshipRule> PossibleRules { get; }

        /// <summary>
        /// Populates the possible <see cref="BinaryRelationshipRule"/> based on <paramref name="sourceY"/> and <paramref name="sourceX"/>
        /// </summary>
        /// <param name="sourceY">The first type of the source/target of the <see cref="BinaryRelationship"/></param>
        /// <param name="sourceX">The second type of the source/target of the <see cref="BinaryRelationship"/></param>
        public void PopulatePossibleRules(ClassKind? sourceY, ClassKind? sourceX)
        {
            this.PossibleRules.Clear();
            if (!sourceY.HasValue || !sourceX.HasValue)
            {
                return;
            }

            var rules = this.ReferenceDataLibraries.SelectMany(x => x.Rule).OfType<BinaryRelationshipRule>().Where(
                x =>
                    (x.SourceCategory.PermissibleClass.Contains(sourceY.Value) || x.SourceCategory.PermissibleClass.Contains(sourceX.Value))
                    && (x.TargetCategory.PermissibleClass.Contains(sourceY.Value) || x.TargetCategory.PermissibleClass.Contains(sourceX.Value))).ToList();

            this.PossibleRules.AddRange(rules.OrderBy(x => x.Name));
            this.SelectedRule = this.PossibleRules.FirstOrDefault(x => x == this.SelectedRule);
        }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the selected <see cref="BinaryRelationshipRule"/>
        /// </summary>
        public ReactiveCommand<object> InspectRuleCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        private void InitializeCommands()
        {
            var canExecuteInpsectRuleCommand = this.WhenAny(vm => vm.SelectedRule, v => v.Value != null);
            this.InspectRuleCommand = ReactiveCommand.Create(canExecuteInpsectRuleCommand);
            this.InspectRuleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedRule));
        }

        /// <summary>
        /// Execute the Inspect command for a <see cref="Thing"/>
        /// </summary>
        /// <param name="thing">the <see cref="Thing"/> to inspect</param>
        protected void ExecuteInspectCommand(Thing thing)
        {
            this.thingDialogNavigationService.Navigate(thing, null, this.Session, false, ThingDialogKind.Inspect, this.thingDialogNavigationService, thing.Container);
        }
    }
}
