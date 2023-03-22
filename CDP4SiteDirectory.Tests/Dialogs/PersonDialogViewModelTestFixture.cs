﻿// -------------------------------------------------------------------------------------------------
// <copyright file="PersonDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.Types;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4SiteDirectory.ViewModels;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class PersonDialogViewModelTestFixture
    {
        private Uri uri;
        private Person person;
        private SiteDirectory siteDir;
        private Mock<ISession> session;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private SiteDirectory clone;

        [SetUp]
        public void Setup()
        {
            this.uri = new Uri("http://www.rheagroup.com");
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.session = new Mock<ISession>();
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri) { ShortName = "personRole", Password = "123" };
            this.siteDir.Person.Add(this.person);

            this.cache.TryAdd(new CacheKey(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));
            this.cache.TryAdd(new CacheKey(this.person.Iid, null), new Lazy<Thing>(() => this.person));

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.clone = this.siteDir.Clone(false);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [TearDown]
        public void Teardown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatValidationWorksOnPassword()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, this.clone);

            var vm = new PersonDialogViewModel(this.person.Clone(false), transaction, this.session.Object, true,
                ThingDialogKind.Update, null, this.clone);

            Assert.AreEqual(0, vm.ValidationErrors.Count);

            // assert 2 errors messages are added when password update is activated
            vm.PwdEditIsChecked = true;
            
            Assert.That(vm["Password"], Is.Not.Null.Or.Empty);
            Assert.That(vm["PasswordConfirmation"], Is.Not.Null.Or.Empty);

            Assert.AreEqual(2, vm.ValidationErrors.Count);

            // assert that the errors are removed upon good password match
            vm.Password = "123";
            vm.PasswordConfirmation = "123";

            Assert.That(vm["Password"], Is.Null.Or.Empty);
            Assert.That(vm["PasswordConfirmation"], Is.Null.Or.Empty);
            
            Assert.AreEqual(0, vm.ValidationErrors.Count);

            Assert.IsTrue(((ICommand)vm.OkCommand).CanExecute(null));
        }

        [Test]
        public async Task VerifyThatUpdateTransactionDoesnotUpdatePassword()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, this.clone);

            var vm = new PersonDialogViewModel(this.person.Clone(false), transaction, this.session.Object, true,
                ThingDialogKind.Update, null, this.clone);

            await vm.OkCommand.Execute();
            var personclone = transaction.UpdatedThing.Select(x => x.Value).OfType<Person>().Single();

            Assert.AreEqual(this.person.Password, personclone.Password);
        }

        [Test]
        public async Task VerifyThatUpdateTransactionUpdatesPassword()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, this.clone);
            var vm = new PersonDialogViewModel(this.person.Clone(false), transaction, this.session.Object, true,
                ThingDialogKind.Update, null, this.clone);

            vm.PwdEditIsChecked = true;
            vm.Password = "456";

            await vm.OkCommand.Execute();
            var personclone = transaction.UpdatedThing.Select(x => x.Value).OfType<Person>().Single();

            Assert.AreEqual("456", personclone.Password);
        }

        [Test]
        public async Task VerifyThatSetDefaultTelephoneNumberWorks()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, this.clone);
            var pers = new Person(Guid.NewGuid(), this.cache, this.uri);
            var phone = new TelephoneNumber(Guid.NewGuid(), this.cache, this.uri);

            pers.TelephoneNumber.Add(phone);

            var vm = new PersonDialogViewModel(pers, transaction, this.session.Object, true,
                ThingDialogKind.Create, null, this.clone);

            vm.SelectedTelephoneNumber = vm.TelephoneNumber.Single();
            await vm.SetDefaultTelephoneNumberCommand.Execute();

            Assert.AreSame(vm.SelectedDefaultTelephoneNumber, phone);
        }

        [Test]
        public async Task VerifyThatSetDefaultEmailAddressWorks()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, this.clone);

            var pers = new Person(Guid.NewGuid(), this.cache, this.uri);
            var email = new EmailAddress(Guid.NewGuid(), this.cache, this.uri);

            pers.EmailAddress.Add(email);

            var vm = new PersonDialogViewModel(pers, transaction, this.session.Object, true,
                ThingDialogKind.Create, null, this.clone);

            vm.SelectedEmailAddress = vm.EmailAddress.Single();
            await vm.SetDefaultEmailAddressCommand.Execute();

            Assert.AreSame(vm.SelectedDefaultEmailAddress, email);
        }

        [Test]
        public void VerifyThatDefaultConstructorDoesNotThrowException()
        {
            var personDialogViewModel = new PersonDialogViewModel();
            Assert.IsNotNull(personDialogViewModel);
        }

        [Test]
        public void VerifyThatPasswordWarningIsDisplayedWhenCreatingUserWithoutPassword()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, this.clone);

            var vm = new PersonDialogViewModel(this.person.Clone(false), transaction, this.session.Object, true,
                ThingDialogKind.Create, null, this.clone);

            vm.PwdEditIsChecked = false;

            Assert.IsTrue(vm.ShoudDisplayPasswordNotSetWarning);
        }
        
        [Test]
        public void VerifyThatPasswordWarningIsNotDisplayedWhenCreatingUserWithPassword()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, this.clone);

            var vm = new PersonDialogViewModel(this.person.Clone(false), transaction, this.session.Object, true,
                ThingDialogKind.Create, null, this.clone);

            vm.PwdEditIsChecked = true;

            Assert.IsFalse(vm.ShoudDisplayPasswordNotSetWarning);
        }

        [Test]
        public void VerifyThatPasswordWarningIsNotDisplayedWhenEditingUserWithoutPassword()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, this.clone);

            var vm = new PersonDialogViewModel(this.person.Clone(false), transaction, this.session.Object, true,
                ThingDialogKind.Update, null, this.clone);

            vm.PwdEditIsChecked = false;

            Assert.IsFalse(vm.ShoudDisplayPasswordNotSetWarning);
        }
    }
}