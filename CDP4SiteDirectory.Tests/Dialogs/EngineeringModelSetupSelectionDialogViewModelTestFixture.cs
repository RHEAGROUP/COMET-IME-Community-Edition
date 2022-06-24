﻿// -------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelSetupSelectionDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Dal;
    using CDP4SiteDirectory.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="EngineeringModelSetupSelectionDialogViewModel"/> class
    /// </summary>
    [TestFixture]
    public class EngineeringModelSetupSelectionDialogViewModelTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Uri uri;
        private Mock<ISession> session;
            
        [SetUp]
        public async Task SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.uri = new Uri("http://www.rheagroup.com");
            var assembler = new Assembler(this.uri);

            var siteDirectoryDto = new CDP4Common.DTO.SiteDirectory(Guid.NewGuid(), 0);
            var dtos = new List<CDP4Common.DTO.Thing>();
            dtos.Add(siteDirectoryDto);

            await assembler.Synchronize(dtos);

            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Assembler).Returns(assembler);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatIfOnlyOneSessionExistsItIsSelected()
        {
            var availableSessions = new List<ISession>();
            availableSessions.Add(this.session.Object);

            var vm = new EngineeringModelSetupSelectionDialogViewModel(availableSessions);
            Assert.AreEqual(vm.SelectedSession, this.session.Object);
        }

        [Test]
        public void VerifyThatIfMultipleSessionsExistsNoneAreSelected()
        {
            var availableSessions = new List<ISession>();
            availableSessions.Add(this.session.Object);

            var session = new Mock<ISession>();
            availableSessions.Add(session.Object);

            var vm = new EngineeringModelSetupSelectionDialogViewModel(availableSessions);
            Assert.IsNull(vm.SelectedSession);
        }

        [Test]
        public void VerifyThatEngineeringModelSetupsAreLoaded()
        {
            var availableSessions = new List<ISession>();
            availableSessions.Add(this.session.Object);

            var siteDirectory = this.session.Object.Assembler.RetrieveSiteDirectory();

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            engineeringModelSetup.Name = "Test Model";
            engineeringModelSetup.ShortName = "testmodel";

            siteDirectory.Model.Add(engineeringModelSetup);

            var vm = new EngineeringModelSetupSelectionDialogViewModel(availableSessions);

            Assert.Contains(engineeringModelSetup, vm.PossibleEngineeringModelSetups);            
        }

        [Test]
        public void VerifyThatWhenEngineeringModelSelectedCanOk()
        {
            var availableSessions = new List<ISession>();
            availableSessions.Add(this.session.Object);

            var siteDirectory = this.session.Object.Assembler.RetrieveSiteDirectory();

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            engineeringModelSetup.Name = "Test Model";
            engineeringModelSetup.ShortName = "testmodel";

            siteDirectory.Model.Add(engineeringModelSetup);

            var vm = new EngineeringModelSetupSelectionDialogViewModel(availableSessions);

            Assert.IsFalse(((ICommand)vm.OkCommand).CanExecute(null));

            vm.SelectedEngineeringModelSetup = engineeringModelSetup;

            Assert.IsTrue(((ICommand)vm.OkCommand).CanExecute(null));
        }

        [Test]
        public async Task VerifyThatWhenCancelledIsExecutedEmptyResultIsReturned()
        {
            var availableSessions = new List<ISession>();
            availableSessions.Add(this.session.Object);

            var vm = new EngineeringModelSetupSelectionDialogViewModel(availableSessions);

            await vm.CancelCommand.Execute();

            Assert.IsFalse(vm.DialogResult.Result.Value);
        }

        [Test]
        public async Task VerifyThatPopulatedDialogResultIsReturnedWhenOkExecuted()
        {
            var availableSessions = new List<ISession>();
            availableSessions.Add(this.session.Object);

            var siteDirectory = this.session.Object.Assembler.RetrieveSiteDirectory();

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            engineeringModelSetup.Name = "Test Model";
            engineeringModelSetup.ShortName = "testmodel";

            siteDirectory.Model.Add(engineeringModelSetup);

            var vm = new EngineeringModelSetupSelectionDialogViewModel(availableSessions);
            vm.SelectedSession = this.session.Object;
            vm.SelectedEngineeringModelSetup = engineeringModelSetup;

            await vm.OkCommand.Execute();

            var dialogResult = (EngineeringModelSetupSelectionResult)vm.DialogResult;
            Assert.IsTrue(dialogResult.Result.Value);
            Assert.AreEqual(this.session.Object, dialogResult.SelectedSession);
            Assert.AreEqual(engineeringModelSetup, dialogResult.SelectedEngineeringModelSetup);
        }
    }
}
