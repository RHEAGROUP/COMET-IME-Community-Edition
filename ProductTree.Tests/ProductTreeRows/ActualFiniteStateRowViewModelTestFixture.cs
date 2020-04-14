﻿// ------------------------------------------------------------------------------------------------
// <copyright file="ActualFiniteStateRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------


namespace ProductTree.Tests.ProductTreeRows
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using CDP4ProductTree.ViewModels;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class ActualFiniteStateRowViewModelTestFixture
    {
        private Mock<ISession> session;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private Parameter parameter1;
        private ParameterType parameterType1;
        private ActualFiniteStateList stateList;
        private ActualFiniteState state1;
        private ParameterValueSet valueset;
        private ElementDefinition elementdef1;
        private Iteration iteration;
        private IterationSetup iterationSetup;
        private EngineeringModel model;
        private EngineeringModelSetup modelSetup;
        private SiteDirectory siteDirectory;
        private Participant participant;
        private Person person;
        private Option option;
        private DomainOfExpertise domain;
        private ParameterRowViewModel parameterRowViewModel;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain", ShortName = "dom" };
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri) { GivenName = "test", Surname = "test" };
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = this.person };
            this.option = new Option(Guid.NewGuid(), this.cache, this.uri);
            this.elementdef1 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);

            this.siteDirectory.Model.Add(this.modelSetup);
            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            this.modelSetup.Participant.Add(this.participant);
            this.siteDirectory.Person.Add(this.person);

            this.model.Iteration.Add(this.iteration);
            this.model.EngineeringModelSetup = this.modelSetup;
            this.iteration.IterationSetup = this.iterationSetup;
            this.iteration.Option.Add(this.option);
            this.iteration.Element.Add(this.elementdef1);

            this.parameterType1 = new EnumerationParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "pt1" };
            this.parameter1 = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = this.parameterType1, Owner = this.domain };
            this.stateList = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain };
            this.state1 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.valueset = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri);
            this.stateList.ActualState.Add(this.state1);
            this.elementdef1.Parameter.Add(this.parameter1);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());

            this.cache.TryAdd(new CacheKey(this.parameter1.Iid, null), new Lazy<Thing>(() => this.parameter1));
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            this.parameterRowViewModel = new ParameterRowViewModel(this.parameter1, this.option, this.session.Object, null);
            var vm = new ActualFiniteStateRowViewModel(this.parameter1, this.state1 , this.session.Object, this.parameterRowViewModel);

            Assert.AreEqual(vm.ActualState, this.state1);
            Assert.AreEqual(vm.IsDefault, vm.ActualState.IsDefault);
            Assert.AreSame(this.state1.Name, vm.ActualState.Name);
        }

        [Test]
        public void VerifyThatSetScalarValueProperly()
        {
            var published = new ValueArray<string>(new List<string> { "manual" }, this.valueset);
            var actual = new ValueArray<string>(new List<string> { "manual" }, this.valueset);

            this.valueset.Published = published;
            this.valueset.Manual = actual;
            this.valueset.ValueSwitch = ParameterSwitchKind.MANUAL;
            this.valueset.ActualOption = this.option;

            this.parameter1.ValueSet.Add(this.valueset);
            this.parameter1.IsOptionDependent = true;

            var row = new ActualFiniteStateRowViewModel(this.parameter1, this.state1, this.session.Object, this.parameterRowViewModel);
            row.SetScalarValue(this.valueset);

            Assert.AreEqual(row.Switch, this.valueset.ValueSwitch);
            Assert.AreEqual(row.ModelCode, this.valueset.ModelCode());
        }
    }
}