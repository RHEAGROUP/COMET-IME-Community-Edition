﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceExportViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Alexander van Delft, Nathanael Smiechowski
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Threading;
    using System.Threading.Tasks;

    using CDP4Common.DTO;

    using CDP4Composition.Navigation;

    using CDP4Dal;
    using CDP4Dal.Composition;
    using CDP4Dal.DAL;

    using CDP4IME.ViewModels;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="DataSourceExportViewModel"/>
    /// </summary>
    [TestFixture]
    public class DataSourceExportViewModelTestFixture
    {
        /// <summary>
        /// The view model that is being tested
        /// </summary>
        private DataSourceExportViewModel viewModel;

        /// <summary>
        /// mocked data service
        /// </summary>
        private Mock<IDal> mockedDal;

        /// <summary>
        /// The <see cref="CancellationTokenSource"/>
        /// </summary>
        private CancellationTokenSource tokenSource;

        /// <summary>
        /// mocked metadata
        /// </summary>
        private Mock<IDalMetaData> mockedMetaData;

        /// <summary>
        /// The session.
        /// </summary>
        private Mock<ISession> session;

        private List<Thing> dalOutputs;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IOpenSaveFileDialogService> fileDialogService;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.tokenSource = new CancellationTokenSource();
            this.session = new Mock<ISession>();
            this.mockedDal = new Mock<IDal>();
            this.fileDialogService = new Mock<IOpenSaveFileDialogService>();
            var openTaskCompletionSource = new TaskCompletionSource<IEnumerable<Thing>>();
            openTaskCompletionSource.SetResult(this.dalOutputs);
            this.mockedDal.Setup(x => x.IsValidUri(It.IsAny<string>())).Returns(true);
            this.mockedDal.Setup(x => x.Open(It.IsAny<Credentials>(), this.tokenSource.Token)).Returns(openTaskCompletionSource.Task);

            this.mockedMetaData = new Mock<IDalMetaData>();
            this.mockedMetaData.Setup(x => x.Name).Returns("MockedDal");
            this.mockedMetaData.Setup(x => x.DalType).Returns(DalType.File);

            this.session.Setup(x => x.DalVersion).Returns(new Version("1.0.0"));

            var dataAccessLayerKinds = new List<Lazy<IDal, IDalMetaData>>();
            dataAccessLayerKinds.Add(new Lazy<IDal, IDalMetaData>(() => this.mockedDal.Object, this.mockedMetaData.Object));

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.serviceLocator.Setup(x => x.GetInstance<AvailableDals>())
                .Returns(new AvailableDals(dataAccessLayerKinds));

            this.viewModel = new DataSourceExportViewModel(new List<ISession> { this.session.Object }, this.fileDialogService.Object);
        }

        [Test]
        public void VerifyOkCommand()
        {
            this.viewModel.Path = @"C:\test\somerandom\no\existant\path\doubletest.zip";
            this.viewModel.Password = "pass";
            this.viewModel.PasswordRetype = "pass";

            Assert.AreEqual(this.viewModel.Password, this.viewModel.PasswordRetype);
            Assert.That(this.viewModel.ErrorMessage, Is.Null.Or.Empty);

            Assert.IsNotNull(this.viewModel.SelectedDal);
            Assert.IsNotNull(this.viewModel.SelectedSession);
            Assert.IsNotNull(this.viewModel.SelectedVersion.Key);

            Assert.IsTrue(this.viewModel.OkCommand.CanExecute(null));

            this.viewModel.OkCommand.Execute(null);

            Assert.AreEqual("The output directory does not exist.", this.viewModel.ErrorMessage);
        }

        [Test]
        public void VerifyCancelCommand()
        {
            Assert.IsTrue(this.viewModel.CancelCommand.CanExecute(null));
            this.viewModel.CancelCommand.Execute(null);

            Assert.IsFalse(this.viewModel.DialogResult.Result.Value);
        }

        [Test]
        public void VerifyVersionChecks()
        {
            this.viewModel = new DataSourceExportViewModel(new List<ISession> { this.session.Object }, this.fileDialogService.Object);
            Assert.AreEqual(1, this.viewModel.Versions.Count);

            this.session.Setup(x => x.DalVersion).Returns(new Version("1.1.0"));
            this.viewModel = new DataSourceExportViewModel(new List<ISession> { this.session.Object }, this.fileDialogService.Object);
            Assert.AreEqual(2, this.viewModel.Versions.Count);

            this.session.Setup(x => x.DalVersion).Returns(new Version("1.2.0"));
            this.viewModel = new DataSourceExportViewModel(new List<ISession> { this.session.Object }, this.fileDialogService.Object);
            Assert.AreEqual(3, this.viewModel.Versions.Count);
        }

        [Test]
        public void VerifyBrowseCommand()
        {
            Assert.IsTrue(this.viewModel.BrowseCommand.CanExecute(null));
        }
    }
}
