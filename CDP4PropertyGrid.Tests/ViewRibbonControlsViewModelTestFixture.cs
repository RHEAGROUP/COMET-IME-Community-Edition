﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ViewRibbonControlsViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4PropertyGrid.Tests
{
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Dal;
    using CDP4PropertyGrid.ViewModels;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class ViewRibbonControlsViewModelTestFixture
    {
        private Mock<IPanelNavigationService> navigationService;
        private Mock<IServiceLocator> servicelocator;
        private Mock<IPanelView> panelView;

        [SetUp]
        public void Setup()
        {
            this.navigationService = new Mock<IPanelNavigationService>();
            this.servicelocator = new Mock<IServiceLocator>();
            this.panelView = new Mock<IPanelView>();

            ServiceLocator.SetLocatorProvider(() => this.servicelocator.Object);
            this.servicelocator.Setup(x => x.GetInstance<IPanelNavigationService>())
                .Returns(this.navigationService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatCommandWorks()
        {
            var vm = new ViewRibbonControlViewModel();

            vm.IsChecked = true;
            vm.OpenPanelCommand.Execute(null);

            this.navigationService.Verify(x => x.Open(It.IsAny<IPanelViewModel>(), true));

            vm.IsChecked = false;
            vm.OpenPanelCommand.Execute(null);
            this.navigationService.Verify(x => x.Close(It.IsAny<Type>()));

            // Verify PanelEVentClosed
            vm.IsChecked = true;
            CDPMessageBus.Current.SendMessage(new NavigationPanelEvent(new PropertyGridViewModel(), this.panelView.Object, PanelStatus.Closed));
            Assert.IsFalse(vm.IsChecked);
        }
    }
}