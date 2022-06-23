﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementScalesBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="MeasurementScalesBrowserViewModel"/>
    /// </summary>
    [TestFixture]
    public class MeasurementScalesBrowserViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Uri uri;
        private SiteDirectory siteDirectory;
        private MeasurementScalesBrowserViewModel measurementScalesBrowserViewModel;
        private Person person;
        private Assembler assembler;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://test.com");
            this.assembler = new Assembler(this.uri);

            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "site directory" };
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri) { GivenName = "John", Surname = "Doe" };
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.measurementScalesBrowserViewModel = new MeasurementScalesBrowserViewModel(this.session.Object, this.siteDirectory, null, null, null, null);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        /// <summary>
        /// The verify panel properties.
        /// </summary>
        [Test]
        public void VerifyPanelProperties()
        {
            Assert.IsTrue(this.measurementScalesBrowserViewModel.Caption.Contains(this.siteDirectory.Name));
            Assert.IsTrue(this.measurementScalesBrowserViewModel.ToolTip.Contains(this.siteDirectory.IDalUri.ToString()));
        }

        [Test]
        public void VerifyThatMeasurementScaleIsAddedAndRemoveWhenItIsSentAsObjectChangeMessage()
        {
            Assert.IsEmpty(this.measurementScalesBrowserViewModel.MeasurementScales);

            var simpleUnit = new SimpleUnit(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "simple unit name",
                ShortName = "simpleunitshortname"
            };

            var ratioScale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.uri)
                                 {
                                     Name = "ratio scale",
                                     ShortName = "ratioscale",
                                     Unit = simpleUnit
                                 };

            CDPMessageBus.Current.SendObjectChangeEvent(ratioScale, EventKind.Added);
            Assert.AreEqual(1, this.measurementScalesBrowserViewModel.MeasurementScales.Count);

            CDPMessageBus.Current.SendObjectChangeEvent(ratioScale, EventKind.Removed);
            Assert.IsFalse(this.measurementScalesBrowserViewModel.MeasurementScales.Any(x => x.Thing == ratioScale));
        }

        [Test]
        public void VerifyThatScalesFromExistingRdlsAreLoaded()
        {
            var siterefenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null);
            var ratioscale1 = new RatioScale(Guid.NewGuid(), null, null);
            var ratioscale2 = new RatioScale(Guid.NewGuid(), null, null);
            siterefenceDataLibrary.Scale.Add(ratioscale1);
            siterefenceDataLibrary.Scale.Add(ratioscale2);
            this.siteDirectory.SiteReferenceDataLibrary.Add(siterefenceDataLibrary);

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, null);
            var modelReferenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), null, null);
            var ratioscale3 = new RatioScale(Guid.NewGuid(), null, null);
            var ratioscale4 = new RatioScale(Guid.NewGuid(), null, null);
            modelReferenceDataLibrary.Scale.Add(ratioscale3);
            modelReferenceDataLibrary.Scale.Add(ratioscale4);
            engineeringModelSetup.RequiredRdl.Add(modelReferenceDataLibrary);
            this.siteDirectory.Model.Add(engineeringModelSetup);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDirectory.SiteReferenceDataLibrary) { modelReferenceDataLibrary }); 

            var browser = new MeasurementScalesBrowserViewModel(this.session.Object, this.siteDirectory, null, null, null, null);
            Assert.AreEqual(4, browser.MeasurementScales.Count);

            browser.Dispose();
            Assert.IsNull(browser.MeasurementScales.First().Thing);
        }

        [Test]
        public void VerifyThatRdlShortnameIsUpdated()
        {
            var vm = new MeasurementScalesBrowserViewModel(this.session.Object, this.siteDirectory, null, null, null, null);

            var sRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            sRdl.Container = this.siteDirectory;

            var cat = new CyclicRatioScale(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat1", ShortName = "1", Container = sRdl };
            var cat2 = new CyclicRatioScale(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat2", ShortName = "2", Container = sRdl };

            CDPMessageBus.Current.SendObjectChangeEvent(cat, EventKind.Added);
            CDPMessageBus.Current.SendObjectChangeEvent(cat2, EventKind.Added);

            var rev = typeof(Thing).GetProperty("RevisionNumber");
            rev.SetValue(sRdl, 3);
            sRdl.ShortName = "test";

            CDPMessageBus.Current.SendObjectChangeEvent(sRdl, EventKind.Updated);
            Assert.IsTrue(vm.MeasurementScales.Count(x => x.ContainerRdl == "test") == 2);
        }
    }
}
