﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductTreeViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ProductTree.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services.NestedElementTreeService;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4ProductTree.ViewModels;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ProductTreeViewModel"/> class.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal class ProductTreeViewModelTestFixture
    {
        private Mock<IServiceLocator> serviceLocator;
        private Mock<INestedElementTreeService> nestedElementTreeService;
        private Mock<IPermissionService> permissionService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<ISession> session;
        private readonly Uri uri = new Uri("http://test.com");
        private SiteDirectory siteDir;
        private EngineeringModel model;
        private Iteration iteration;
        private EngineeringModelSetup modelSetup;
        private IterationSetup iterationSetup;
        private Person person;
        private Participant participant;
        private Option option;
        private ElementDefinition elementDef;
        private DomainOfExpertise domain;
        private Assembler assembler;
        private readonly string nestedParameterPath = "PATH";

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.nestedElementTreeService = new Mock<INestedElementTreeService>();
            this.nestedElementTreeService.Setup(x => x.GetNestedParameterPath(It.IsAny<ParameterBase>(), It.IsAny<Option>())).Returns(this.nestedParameterPath);

            this.serviceLocator.Setup(x => x.GetInstance<INestedElementTreeService>()).Returns(this.nestedElementTreeService.Object);

            this.permissionService = new Mock<IPermissionService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri) { GivenName = "John", Surname = "Doe" };
            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelSetup.Name = "model name";
            this.modelSetup.ShortName = "modelshortname";

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.option = new Option(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "option name",
                ShortName = "optionshortname"
            };

            this.elementDef = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "domain",
                ShortName = "domainshortname"
            };

            this.siteDir.Person.Add(this.person);
            this.siteDir.Model.Add(this.modelSetup);
            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            this.modelSetup.Participant.Add(this.participant);
            this.participant.Person = this.person;

            this.model.Iteration.Add(this.iteration);
            this.model.EngineeringModelSetup = this.modelSetup;

            this.iteration.IterationSetup = this.iterationSetup;
            this.iteration.Option.Add(this.option);
            this.iteration.Element.Add(this.elementDef);
            this.iteration.TopElement = this.elementDef;

            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<IDialogViewModel>())).Returns(new BaseDialogResult(true));

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(this.domain);

            this.session.Setup(x => x.OpenIterations).Returns(
                new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
                {
                    { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null) }
                });
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            Assert.That(vm.Caption, Is.Not.Null.Or.Empty);
            Assert.That(vm.ToolTip, Is.Not.Null.Or.Empty);

            Assert.AreEqual("model name", vm.CurrentModel);
            Assert.AreEqual("option name", vm.CurrentOption);
            Assert.AreEqual("domain [domainshortname]", vm.DomainOfExpertise);
            Assert.AreEqual("John Doe", vm.Person);
            Assert.AreEqual(0, vm.CurrentIteration);

            Assert.IsNotNull(vm.ActiveParticipant);
            Assert.AreEqual(1, vm.TopElement.Count);
        }

        [Test]
        public void VerifyThatUpdateOptionEventWorks()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            Assert.AreEqual("option name", vm.CurrentOption);

            var revisionNumber = typeof(Option).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.option, 50);
            this.option.Name = "blablabla";

            CDPMessageBus.Current.SendObjectChangeEvent(this.option, EventKind.Updated);
            Assert.AreEqual("blablabla", vm.CurrentOption);
        }

        [Test]
        public void VerifyThatUpdateIterationSetupEventIsHandled()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            Assert.AreEqual(0, vm.CurrentIteration);

            var revisionNumber = typeof(IterationSetup).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.iterationSetup, 50);

            var iterationNumber = typeof(IterationSetup).GetProperty("IterationNumber");
            iterationNumber.SetValue(this.iterationSetup, 1);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iterationSetup, EventKind.Updated);
            Assert.AreEqual(1, vm.CurrentIteration);
        }

        [Test]
        public void VerifyThatUpdateDomainEventIsHandled()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            var revisionNumber = typeof(DomainOfExpertise).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.domain, 50);

            this.domain.Name = "System";
            this.domain.ShortName = "SYS";

            CDPMessageBus.Current.SendObjectChangeEvent(this.domain, EventKind.Updated);
            Assert.AreEqual("System [SYS]", vm.DomainOfExpertise);
        }

        [Test]
        public void VerifyThatUpdatePersonEventIsHandled()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            var revisionNumber = typeof(Person).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.person, 50);

            this.person.GivenName = "Jane";

            CDPMessageBus.Current.SendObjectChangeEvent(this.person, EventKind.Updated);
            Assert.AreEqual("Jane Doe", vm.Person);
        }

        [Test]
        public void VerifyThatTopElementIsRemovedUponIterationUpdateWithNoTop()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            var revisionNumber = typeof(Iteration).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.iteration, 50);
            this.iteration.TopElement = null;

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreEqual(0, vm.TopElement.Count);
        }

        [Test]
        public void VerifyThatTopElementIsModifiedUponNewTopElement()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            var revisionNumber = typeof(Iteration).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.iteration, 50);

            var elementdef = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iteration.TopElement = elementdef;

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreSame(elementdef, vm.TopElement.Single().Thing);
        }

        [Test]
        public void VerifyExecuteCreateSubscriptionCommand()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            var revisionNumber = typeof(Iteration).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.iteration, 50);

            var elementdef = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Container = this.iteration };
            var anotherDomain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "Not owned" };
            var boolParamType = new BooleanParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri) { Owner = anotherDomain, Container = elementdef, ParameterType = boolParamType };
            var published = new ValueArray<string>(new List<string> { "published" });

            var paramValueSet = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Published = published,
                Manual = published,
                Computed = published,
                ValueSwitch = ParameterSwitchKind.COMPUTED
            };

            parameter.ValueSet.Add(paramValueSet);
            elementdef.Parameter.Add(parameter);
            this.iteration.TopElement = elementdef;

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreSame(elementdef, vm.TopElement.Single().Thing);
            Assert.AreEqual(1, vm.TopElement.Single().ContainedRows.Count);
            var paramRow = vm.TopElement.Single().ContainedRows.First() as ParameterOrOverrideBaseRowViewModel;
            Assert.NotNull(paramRow);
            vm.SelectedThing = paramRow;

            Assert.IsTrue(vm.CreateSubscriptionCommand.CanExecute(null));
            Assert.AreEqual(0, paramRow.Thing.ParameterSubscription.Count);

            vm.SelectedThing = null;
            vm.CreateSubscriptionCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never);

            vm.SelectedThing = vm.TopElement.Single();
            vm.CreateSubscriptionCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never);

            vm.SelectedThing = paramRow;
            vm.CreateSubscriptionCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(1));
        }

        [Test]
        public void VerifyExecuteDeleteSubscriptionCommand()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null);

            var revisionNumber = typeof(Iteration).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.iteration, 50);

            var elementdef = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Container = this.iteration };
            var anotherDomain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "Not owned" };
            var boolParamType = new BooleanParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri) { Owner = anotherDomain, Container = elementdef, ParameterType = boolParamType };
            parameter.ParameterSubscription.Add(new ParameterSubscription(Guid.NewGuid(), this.assembler.Cache, this.uri) { Owner = this.domain });
            var published = new ValueArray<string>(new List<string> { "published" });

            var paramValueSet = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Published = published,
                Manual = published,
                Computed = published,
                ValueSwitch = ParameterSwitchKind.COMPUTED
            };

            parameter.ValueSet.Add(paramValueSet);
            elementdef.Parameter.Add(parameter);
            this.iteration.TopElement = elementdef;

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreSame(elementdef, vm.TopElement.Single().Thing);
            Assert.AreEqual(1, vm.TopElement.Single().ContainedRows.Count);
            var paramRow = vm.TopElement.Single().ContainedRows.First() as ParameterOrOverrideBaseRowViewModel;
            Assert.NotNull(paramRow);
            vm.SelectedThing = paramRow;

            vm.PopulateContextMenu();
            Assert.AreEqual(8, vm.ContextMenu.Count);

            Assert.IsTrue(vm.DeleteSubscriptionCommand.CanExecute(null));
            Assert.AreEqual(1, paramRow.Thing.ParameterSubscription.Count);
            vm.DeleteSubscriptionCommand.Execute(null);
        }

        [Test]
        public void VerifyCopyPathToClipboardCommand()
        {
            var elementdef = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Container = this.iteration, ShortName = "ELEMENT" };
            var anotherDomain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "Not owned" };
            var boolParamType = new BooleanParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "PARAM" };
            var parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri) { Owner = anotherDomain, Container = elementdef, ParameterType = boolParamType };
            var published = new ValueArray<string>(new List<string> { "published" });

            var paramValueSet = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Published = published,
                Manual = published,
                Computed = published,
                ValueSwitch = ParameterSwitchKind.COMPUTED
            };

            parameter.ValueSet.Add(paramValueSet);
            elementdef.Parameter.Add(parameter);
            this.iteration.TopElement = elementdef;

            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null);

            var paramRow = vm.TopElement.Single().ContainedRows.First() as ParameterOrOverrideBaseRowViewModel;
            Assert.NotNull(paramRow);
            vm.SelectedThing = paramRow;

            vm.PopulateContextMenu();
            Assert.AreEqual(7, vm.ContextMenu.Count);

            Clipboard.SetText("Reset");

            vm.CopyPathToClipboardCommand.Execute(null);

            Assert.IsTrue(Clipboard.GetDataObject().GetData(typeof(string)).ToString().Contains($"{this.nestedParameterPath}"));
        }

        [Test]
        public void VerifyThatActiveDomainIsDisplayed()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, null, null, null, null);
            Assert.AreEqual("domain [domainshortname]", vm.DomainOfExpertise);

            this.domain = null;
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(this.domain);
            
            vm = new ProductTreeViewModel(this.option, this.session.Object, null, null, null, null);
            Assert.AreEqual("None", vm.DomainOfExpertise);
        }

        [Test]
        public void VerifyCreateParameterOverride()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null);
            var revisionNumber = typeof(Iteration).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.iteration, 50);
            var elementdef = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Container = this.iteration };
            var boolParamType = new BooleanParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri);

            var elementUsage = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Container = elementdef,
                ElementDefinition = elementdef
            };

            var parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri) { Owner = this.domain, Container = elementUsage, ParameterType = boolParamType };
            elementdef.Parameter.Add(parameter);
            var published = new ValueArray<string>(new List<string> { "published" });

            var paramValueSet = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Published = published,
                Manual = published,
                Computed = published,
                ValueSwitch = ParameterSwitchKind.COMPUTED
            };

            parameter.ValueSet.Add(paramValueSet);

            var usageRow = new ElementUsageRowViewModel(elementUsage, this.option, this.session.Object, null);
            var parameterRow = new ParameterRowViewModel(parameter, this.option, this.session.Object, usageRow);

            this.iteration.TopElement = elementdef;
            vm.SelectedThing = parameterRow;

            Assert.IsTrue(vm.CreateOverrideCommand.CanExecute(null));

            vm.SelectedThing = null;
            vm.CreateOverrideCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never);

            vm.SelectedThing = vm.TopElement.Single();
            vm.CreateOverrideCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never);

            vm.SelectedThing = parameterRow;
            vm.CreateOverrideCommand.Execute(parameter);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));

            vm.PopulateContextMenu();
            Assert.AreEqual(7, vm.ContextMenu.Count);
        }

        [Test]
        public void VerifyCreateParameterOverrideIsDisabledForTopElement()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null);
            var revisionNumber = typeof(Iteration).GetProperty("RevisionNumber");
            revisionNumber.SetValue(this.iteration, 50);
            var elementdef = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Container = this.iteration };
            this.iteration.TopElement = elementdef;
            var boolParamType = new BooleanParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri) { Owner = this.domain, Container = elementdef, ParameterType = boolParamType };
            elementdef.Parameter.Add(parameter);

            Assert.IsFalse(vm.CreateOverrideCommand.CanExecute(parameter));
        }

        [Test]
        public void VerifyToggleNamesAndShortNames()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null);
            Assert.IsFalse(vm.IsDisplayShortNamesOn);
            Assert.IsTrue(vm.ToggleUsageNamesCommand.CanExecute(null));

            vm.ToggleUsageNamesCommand.Execute(null);
            Assert.IsTrue(vm.IsDisplayShortNamesOn);
            vm.ToggleUsageNamesCommand.Execute(null);
            Assert.IsFalse(vm.IsDisplayShortNamesOn);
            Assert.DoesNotThrow(vm.Dispose);
        }

        [Test]
        public void VerifyContextMenuPopulation()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null);
            Assert.AreEqual(0, vm.ContextMenu.Count);
            Assert.IsNull(vm.SelectedThing);
            vm.PopulateContextMenu();
            Assert.AreEqual(0, vm.ContextMenu.Count);

            var elemDef = vm.TopElement.Single();
            vm.SelectedThing = elemDef;
            vm.PopulateContextMenu();
            Assert.AreEqual(6, vm.ContextMenu.Count);
        }

        [Test]
        public void VerifyThatDragWorks()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, null, this.panelNavigationService.Object, null, null);
            var draginfo = new Mock<IDragInfo>();
            var dragSource = new Mock<IDragSource>();

            draginfo.Setup(x => x.Payload).Returns(dragSource.Object);

            vm.StartDrag(draginfo.Object);
            dragSource.Verify(x => x.StartDrag(draginfo.Object));
        }

        [Test]
        public void VerifyThatDropsWorkDomain()
        {
            var vm = new ProductTreeViewModel(this.option, this.session.Object, null, null, null, null);
            var dropinfo = new Mock<IDropInfo>();
            var droptarget = new Mock<IDropTarget>();

            dropinfo.Setup(x => x.TargetItem).Returns(droptarget.Object);
            droptarget.Setup(x => x.Drop(It.IsAny<IDropInfo>())).Throws(new Exception("ex"));

            vm.Drop(dropinfo.Object);
            droptarget.Verify(x => x.Drop(dropinfo.Object));

            Assert.AreEqual("ex", vm.Feedback);
        }
    }
}
