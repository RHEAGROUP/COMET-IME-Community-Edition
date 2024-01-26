﻿// ------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrixRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------


namespace CDP4RelationshipMatrix.Tests.Ribbon
{
    using System;
    using System.Linq;
    using System.Reactive.Concurrency;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CommonServiceLocator;
    using Moq;
    using NUnit.Framework;

    using ReactiveUI;

    using ViewModels;

    [TestFixture]
    internal class RelationshipMatrixRibbonViewModelTestFixture
    {
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> navigationService;

        private Mock<ISession> session;
        private Iteration iteration;
        private Person person;
        private Participant participant;
        private Uri uri;
        private Mock<IPermissionService> permissionService;
        private Assembler assembler;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.uri = new Uri("http://test.com");
            this.assembler = new Assembler(this.uri);

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigationService = new Mock<IPanelNavigationService>();

            var siteDirectory = new SiteDirectory(Guid.NewGuid(), null, this.uri);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>())
                .Returns(this.navigationService.Object);

            this.BuildIterationTestData();
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.IsVersionSupported(It.IsAny<Version>())).Returns(true);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatIterationArePopulated()
        {
            var viewmodel = new RelationshipMatrixRibbonViewModel();
            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Added);

            Assert.AreEqual(1, viewmodel.OpenModels.Count);

            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Removed);
            Assert.AreEqual(0, viewmodel.OpenModels.Count);
        }

        private void BuildIterationTestData()
        {
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.participant = new Participant(Guid.NewGuid(), null, this.uri) { Person = this.person };

            var iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationIid = this.iteration.Iid, IterationNumber = 1 };
            var modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "ModelSetup",
                EngineeringModelIid = model.Iid
            };
            modelSetup.Participant.Add(this.participant);

            modelSetup.Container = this.session.Object.RetrieveSiteDirectory();

            iterationSetup.Container = modelSetup;
            this.iteration.Container = model;

            this.iteration.IterationSetup = iterationSetup;
            model.EngineeringModelSetup = modelSetup;

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
        }
    }
}