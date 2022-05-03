﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelSelectionSessionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2022 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.ViewModels
{
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4CommonView;

    using CDP4Composition.Mvvm.Types;

    using CDP4Dal;

    /// <summary>
    /// The purpose of the <see cref="SessionRowViewModel"/> is to represent a <see cref="Session"/> and its <see cref="SiteDirectory"/>
    /// </summary>
    public class SwitchDomainSessionRowViewModel : SiteDirectoryRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelSelectionSessionRowViewModel"/> class. 
        /// </summary>
        /// <param name="siteDirectory">
        /// The <see cref="SiteDirectory"/> that is represented by the row-view-model
        /// </param>
        /// <param name="session">
        /// The <see cref="CDP4Dal.Session"/> that is represented by the row-view-model
        /// </param>
        public SwitchDomainSessionRowViewModel(SiteDirectory siteDirectory, ISession session)
            : base(siteDirectory, session, null)
        {
            this.EngineeringModelSetupRowViewModels = new DisposableReactiveList<SwitchDomainEngineeringModelSetupRowViewModel>();
            
            var models = this.Session.OpenIterations.Select(i => i.Key).Select(o => o.Container).OfType<EngineeringModel>()
                            .Select(em => em.EngineeringModelSetup).Distinct();

            foreach (var model in models.OrderBy(m => m.Name))
            {
                var isParticipant = model.Participant.Any(x => x.Person == this.Session.ActivePerson);
                if (isParticipant)
                {
                    this.AddModelRowViewModel(model);
                }
            }

            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the <see cref="EngineeringModelSetupRowViewModel"/> that are contained by the row-view-model
        /// </summary>
        public DisposableReactiveList<SwitchDomainEngineeringModelSetupRowViewModel> EngineeringModelSetupRowViewModels { get; private set; }
        
        /// <summary>
        /// Add a <see cref="EngineeringModelSetupRowViewModel"/> to the list of <see cref="EngineeringModelSetup"/>s
        /// </summary>
        /// <param name="model">The <see cref="EngineeringModelSetup"/> that is to be added</param>
        private void AddModelRowViewModel(EngineeringModelSetup model)
        {
            var row = new SwitchDomainEngineeringModelSetupRowViewModel(model, this.Session);
            this.EngineeringModelSetupRowViewModels.Add(row);
        }

        /// <summary>
        /// Update the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.Name += string.Format(" ({0})", this.Session.Name);
        }
    }
}
