﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrixViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4RelationshipMatrix.Tests.ViewModel
{
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Services;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4RelationshipMatrix.Settings;
    using Moq;
    using NUnit.Framework;
    using ViewModels;

    /// <summary>
    /// Suite of tests for the <see cref="RelationshipMatrixViewModel"/> class.
    /// </summary>
    [TestFixture]
    public class RelationshipMatrixViewModelTestFixture : ViewModelTestBase
    {
        private Mock<IDeprecatableToggleViewModel> toggle;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            this.toggle = new Mock<IDeprecatableToggleViewModel>();
            this.toggle.Setup(x => x.ShowDeprecatedThings).Returns(true);

            FilterStringService.FilterString.RegisterDeprecatableToggleViewModel(this.toggle.Object);
        }

        [Test]
        public void AssertViewModelWorks()
        {
            var vm = new RelationshipMatrixViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginService.Object);

            Assert.IsFalse(vm.CanEditSourceY);
            Assert.IsFalse(vm.CanEditSourceX);
            Assert.IsFalse(vm.CanInspectSourceY);
            Assert.IsFalse(vm.CanInspectSourceX);

            Assert.AreEqual(this.participant, vm.ActiveParticipant);
            Assert.AreEqual($"{this.domain.Name} [{this.domain.ShortName}]", vm.DomainOfExpertise);

            Assert.AreEqual(this.settings.PossibleClassKinds.Count, vm.SourceYConfiguration.PossibleClassKinds.Count);
            Assert.AreEqual(0, vm.RelationshipConfiguration.PossibleRules.Count);

            vm.SourceYConfiguration.SelectedClassKind =
                vm.SourceYConfiguration.PossibleClassKinds.First(x => x == ClassKind.ElementDefinition);
            vm.SourceXConfiguration.SelectedClassKind =
                vm.SourceXConfiguration.PossibleClassKinds.First(x => x == ClassKind.ElementDefinition);

            Assert.AreEqual(
                this.srdl.DefinedCategory.Count(x => x.PermissibleClass.Contains(ClassKind.ElementDefinition)),
                vm.SourceYConfiguration.PossibleCategories.Count);
            Assert.AreEqual(
                this.srdl.DefinedCategory.Count(x => x.PermissibleClass.Contains(ClassKind.ElementDefinition)),
                vm.SourceXConfiguration.PossibleCategories.Count);

            Assert.AreEqual(this.srdl.Rule.Count, vm.RelationshipConfiguration.PossibleRules.Count);
            Assert.IsEmpty(vm.Matrix.Records);
            Assert.IsEmpty(vm.Matrix.Columns);

            vm.SourceYConfiguration.SelectedCategories = new List<Category>(
                vm.SourceYConfiguration.PossibleCategories.Where(x =>
                    x.Iid == this.catEd1.Iid || x.Iid == this.catEd2.Iid));
            vm.SourceXConfiguration.SelectedCategories =
                new List<Category>(vm.SourceXConfiguration.PossibleCategories.Where(x => x.Iid == this.catEd2.Iid));
            vm.SourceYConfiguration.SelectedBooleanOperatorKind = CategoryBooleanOperatorKind.OR;
            vm.RelationshipConfiguration.SelectedRule = vm.RelationshipConfiguration.PossibleRules.Single();

            vm.SourceXConfiguration.SelectedOwners.Add(this.domain);
            vm.SourceYConfiguration.SelectedOwners.Add(this.domain);

            vm.SourceYConfiguration.IncludeSubcategories = false;

            Assert.AreEqual(3, vm.Matrix.Records.Count);
            Assert.AreEqual(3, vm.Matrix.Columns.Count);

            vm.SourceYConfiguration.IncludeSubcategories = true;

            Assert.AreEqual(5, vm.Matrix.Records.Count);
            Assert.AreEqual(3, vm.Matrix.Columns.Count);

            vm.SourceYConfiguration.SelectedCategories =
                new List<Category>(vm.SourceYConfiguration.PossibleCategories.Where(x => x.Iid == this.catEd3.Iid));

            Assert.AreEqual(2, vm.Matrix.Records.Count);
            Assert.AreEqual(3, vm.Matrix.Columns.Count);

            vm.SourceYConfiguration.SelectedCategories =
                new List<Category>(vm.SourceYConfiguration.PossibleCategories.Where(x => x.Iid == this.catEd4.Iid));

            Assert.AreEqual(1, vm.Matrix.Records.Count);
            Assert.AreEqual(3, vm.Matrix.Columns.Count);

            vm.SourceYConfiguration.SelectedCategories = new List<Category>(
                vm.SourceYConfiguration.PossibleCategories.Where(x =>
                    x.Iid == this.catEd1.Iid || x.Iid == this.catEd2.Iid));
            vm.SourceYConfiguration.SelectedBooleanOperatorKind = CategoryBooleanOperatorKind.AND;

            Assert.AreEqual(1, vm.Matrix.Records.Count);
            Assert.AreEqual(3, vm.Matrix.Columns.Count);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            vm.SwitchAxisCommand.Execute(null);

            Assert.AreEqual(2, vm.Matrix.Records.Count);
            Assert.AreEqual(2, vm.Matrix.Columns.Count);

            vm.Dispose();
        }

        [Test]
        public void AssertSavingConfigurationsWork()
        {
            var vm = new RelationshipMatrixViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginService.Object);

            vm.SourceYConfiguration.SelectedClassKind =
                vm.SourceYConfiguration.PossibleClassKinds.First(x => x == ClassKind.ElementDefinition);
            vm.SourceXConfiguration.SelectedClassKind =
                vm.SourceXConfiguration.PossibleClassKinds.First(x => x == ClassKind.ElementDefinition);

            vm.SourceYConfiguration.SelectedCategories = new List<Category>(
                vm.SourceYConfiguration.PossibleCategories.Where(x =>
                    x.Iid == this.catEd1.Iid || x.Iid == this.catEd2.Iid));
            vm.SourceXConfiguration.SelectedCategories =
                new List<Category>(vm.SourceXConfiguration.PossibleCategories.Where(x => x.Iid == this.catEd2.Iid));
            vm.SourceYConfiguration.SelectedBooleanOperatorKind = CategoryBooleanOperatorKind.OR;
            vm.RelationshipConfiguration.SelectedRule = vm.RelationshipConfiguration.PossibleRules.Single();

            vm.SourceYConfiguration.IncludeSubcategories = false;

            Assert.DoesNotThrow(() => vm.SaveCurrentConfiguration.Execute(null));
            Assert.DoesNotThrow(() => vm.ManageSavedConfigurations.Execute(null));

            vm.Dispose();
        }
    }
}