﻿// -------------------------------------------------------------------------------------------------
// <copyright file="SavedConfigurationDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Tests.ViewModel
{
    using CDP4Composition.ViewModels;

    using CDP4RelationshipMatrix.Settings;
    using NUnit.Framework;
    using ViewModels;

    public class SavedConfigurationDialogViewModelTestFixture : ViewModelTestBase
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }
        
        [Test]
        public void AssertViewModelWorks()
        {
            var savedConfiguration = new SavedConfiguration();

            var vm = new SavedConfigurationDialogViewModel<RelationshipMatrixPluginSettings>(this.pluginService.Object, savedConfiguration);

            vm.Name = "adda";
            vm.Description = "dde";

            Assert.DoesNotThrow(() => vm.OkCommand.Execute());
        }
    }
}
