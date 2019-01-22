﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ScriptingEngineRibbonPageGroupTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Tests.ViewModels
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading;
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Dal;
    using CDP4Scripting.ViewModels;
    using Events;
    using ICSharpCode.AvalonEdit;
    using Interfaces;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ScriptingEngineRibbonPageGroup"/>
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ScriptingEngineRibbonPageGroupTestFixture
    {
        private ScriptingEngineRibbonPageGroupViewModel scriptingEngineRibbonPageGroupViewModel;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IOpenSaveFileDialogService> fileDialogService;
        private Mock<IScriptingProxy> scriptingProxy;
        private Mock<ScriptPanelViewModel> scriptPanelViewModel;
        private ReactiveList<ISession> openSessions;
        private string filePathOpenTest = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.lua");
        private readonly string filePathSaveTest = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.lua");
        private readonly string filePath2 = Path.Combine(TestContext.CurrentContext.TestDirectory, "testFile2.lua");

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.openSessions = new ReactiveList<ISession>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.scriptingProxy = new Mock<IScriptingProxy>();

            var avalonEditor = new TextEditor();
            avalonEditor.Text = "Content of the editor";
            this.scriptPanelViewModel = new Mock<ScriptPanelViewModel>("header", this.scriptingProxy.Object, "*.lua", openSessions);
            this.scriptPanelViewModel.SetupProperty(x => x.AvalonEditor, avalonEditor);

            this.fileDialogService = new Mock<IOpenSaveFileDialogService>();
            this.fileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4)).Returns((string[])null);

            this.scriptingEngineRibbonPageGroupViewModel = new ScriptingEngineRibbonPageGroupViewModel(this.panelNavigationService.Object, this.fileDialogService.Object, this.scriptingProxy.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatNullParametersThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new ScriptingEngineRibbonPageGroupViewModel(null, this.fileDialogService.Object, this.scriptingProxy.Object));
            Assert.Throws<ArgumentNullException>(() => new ScriptingEngineRibbonPageGroupViewModel(this.panelNavigationService.Object, null, this.scriptingProxy.Object));
            Assert.Throws<ArgumentNullException>(() => new ScriptingEngineRibbonPageGroupViewModel(this.panelNavigationService.Object, this.fileDialogService.Object, null));
        }

        [Test]
        public void VerifyThatNewScriptCommandsWork()
        {
            Assert.DoesNotThrow(() => this.scriptingEngineRibbonPageGroupViewModel.NewLuaScriptCommand.Execute(null));
            this.panelNavigationService.Verify(x => x.Open(It.IsAny<IPanelViewModel>(), true), Times.Once);
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 1);
            var scriptViewModel = this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.ElementAt(0);
            Assert.AreEqual(scriptViewModel.Caption, "lua0");
            Assert.AreEqual(scriptViewModel.FileExtension, "*.lua");

            Assert.DoesNotThrow(() => this.scriptingEngineRibbonPageGroupViewModel.NewPythonScriptCommand.Execute(null));
            this.panelNavigationService.Verify(x => x.Open(It.IsAny<IPanelViewModel>(), true), Times.Exactly(2));
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 2);
            scriptViewModel = this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.ElementAt(1);
            Assert.AreEqual(scriptViewModel.Caption, "python1");
            Assert.AreEqual(scriptViewModel.FileExtension, "*.py");

            Assert.DoesNotThrow(() => this.scriptingEngineRibbonPageGroupViewModel.NewTextScriptCommand.Execute(null));
            this.panelNavigationService.Verify(x => x.Open(It.IsAny<IPanelViewModel>(), true), Times.Exactly(3));
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 3);
            scriptViewModel = this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.ElementAt(2);
            Assert.AreEqual(scriptViewModel.Caption, "text2");
            Assert.AreEqual(scriptViewModel.FileExtension, "*.txt");

            this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.RemoveAt(1);
            Assert.DoesNotThrow(() => this.scriptingEngineRibbonPageGroupViewModel.NewTextScriptCommand.Execute(null));
            this.panelNavigationService.Verify(x => x.Open(It.IsAny<IPanelViewModel>(), true), Times.Exactly(4));
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 3);
            scriptViewModel = this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.ElementAt(2);
            Assert.AreEqual(scriptViewModel.Caption, "text3");
            Assert.AreEqual(scriptViewModel.FileExtension, "*.txt");
        }

        [Test]
        public void VerifyThatSaveScriptWorks()
        {
            // Initialization of a script panel view model
            var editor = new TextEditor();
            editor.Text = "some text";
            var panelVM = new Mock<IScriptPanelViewModel>();
            panelVM.SetupProperty(x => x.Caption, "header");
            panelVM.SetupProperty(x => x.AvalonEditor, editor);

            // The fileDialogService.GetSaveFileDialog should be called for the first time and return a valid path. 
            // A couple should be added in the dictionary to store the path of the file associated to the panel saved.
            this.fileDialogService.Setup(x => x.GetSaveFileDialog(It.IsAny<string>(), null, ScriptingEngineRibbonPageGroupViewModel.DialogFilters, It.IsAny<string>(), 1)).Returns(this.filePathSaveTest);
            var scriptSaved = new ScriptPanelEvent(panelVM.Object, ScriptPanelStatus.Saved);
            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(scriptSaved));
            this.fileDialogService.Verify(x => x.GetSaveFileDialog(It.IsAny<string>(), null, ScriptingEngineRibbonPageGroupViewModel.DialogFilters, It.IsAny<string>(), 1), Times.Once);
            Assert.IsTrue(File.Exists(this.filePathSaveTest));
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("test.lua"));
            string result;
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.TryGetValue("test.lua", out result);
            Assert.AreEqual(this.filePathSaveTest, result);

            // The path of the file associated to the script panel is already stored in the dictionnary, the file should be overwritten
            panelVM.SetupProperty(x => x.Caption, "test.lua*");
            editor.Text = "new content";
            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(scriptSaved));
            var content = File.ReadAllText(this.filePathSaveTest);
            Assert.AreEqual("new content", content);

            // The file has been deleted, the dictionary should be updated with the new value of the path.
            File.Delete(this.filePathSaveTest);
            this.fileDialogService.Setup(x => x.GetSaveFileDialog("test.lua", null, It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(this.filePath2);
            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(scriptSaved));
            this.fileDialogService.Verify(x => x.GetSaveFileDialog("test.lua", null, ScriptingEngineRibbonPageGroupViewModel.DialogFilters, It.IsAny<string>(), 1), Times.Once);
            Assert.IsTrue(File.Exists(this.filePath2));
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.TryGetValue("testFile2.lua", out result);
            Assert.AreEqual(this.filePath2, result);
            Assert.IsFalse(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsValue(this.filePathSaveTest));
            Assert.IsFalse(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("test.lua"));

            // The fileDialogService.GetSaveFileDialog should be called once and returns "" that leads to an exception.
            panelVM.SetupProperty(x => x.Caption, "new header");
            this.fileDialogService.Setup(x => x.GetSaveFileDialog(It.IsAny<string>(), null, It.IsAny<string>(), It.IsAny<string>(), 1)).Returns("");
            Assert.Throws<ArgumentNullException>(() => CDPMessageBus.Current.SendMessage(scriptSaved));
            this.fileDialogService.Verify(x => x.GetSaveFileDialog("new header", It.IsAny<string>(), ScriptingEngineRibbonPageGroupViewModel.DialogFilters, It.IsAny<string>(), 1), Times.Once);

            // The button "save all" has been pressed, the 2 scripts should be saved
            // The first one has been saved previously, the content of the file associated should be overwritten
            // The second one has never been saved, a new file should be created
            // A couple should be added to the dictionary to store the path of the second file.
            panelVM.SetupProperty(x => x.Caption, "testFile2.lua*");
            editor.Text = "content of the testFile2.lua file";
            var panelVM2 = new Mock<IScriptPanelViewModel>();
            panelVM2.SetupProperty(x => x.Caption, "python*");
            panelVM2.SetupProperty(x => x.AvalonEditor, editor);
            this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Clear();
            this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Add(panelVM.Object);
            this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Add(panelVM2.Object);
            var pythonFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.py");
            this.fileDialogService.Setup(x => x.GetSaveFileDialog("python", null, It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(pythonFilePath);
            Assert.DoesNotThrow(() => this.scriptingEngineRibbonPageGroupViewModel.SaveAllCommand.Execute(null));
            content = File.ReadAllText(this.filePath2);
            Assert.AreEqual("content of the testFile2.lua file", content);
            this.fileDialogService.Verify(x => x.GetSaveFileDialog("python", null, ScriptingEngineRibbonPageGroupViewModel.DialogFilters, It.IsAny<string>(), 1), Times.Once);
            Assert.IsTrue(File.Exists(pythonFilePath));
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.TryGetValue("test.py", out result);
            Assert.AreEqual(pythonFilePath, result);
        }

        [Test]
        public void VerifyFindFilterIndexWorks()
        {
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.FindFilterIndex("*.lua"), 1);
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.FindFilterIndex("*.py"), 2);
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.FindFilterIndex("*.txt"), 3);
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.FindFilterIndex("*.jar"), 1);
        }

        [Test]
        public void VerifyThatOpenScriptFileWorks()
        {
            // No path returned by the fileDialogService.GetOpenFileDialog, the method should return at the first conditionnal statement 
            Assert.DoesNotThrow(() => this.scriptingEngineRibbonPageGroupViewModel.OpenScriptCommand.Execute(null));
            this.fileDialogService.Verify(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4), Times.Once);
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 0);

            // The file is already opened in the scripting engine, the method should return without open a new panel
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.Add("tab1", "C\\Users\\test.lua");
            string[] paths = { "C\\Users\\test.lua" };
            this.fileDialogService.Setup(x => x.GetOpenFileDialog(false, false, true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4)).Returns(paths);
            this.scriptingEngineRibbonPageGroupViewModel.OpenScriptCommand.Execute(null);
            this.fileDialogService.Verify(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4), Times.Exactly(2));
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 0);

            // The path returns a directory. An exception should be trhown and no panel should be created. 
            paths[0] = "C\\Users";
            this.scriptingEngineRibbonPageGroupViewModel.OpenScriptCommand.Execute(null);
            this.fileDialogService.Verify(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4), Times.Exactly(3));
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 0);

            // The path returns a filename with an invalid extension. An exception should be trhown and no panel should be created. 
            paths[0] = "C\\Users\\test.jar";
            this.scriptingEngineRibbonPageGroupViewModel.OpenScriptCommand.Execute(null);
            this.fileDialogService.Verify(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4), Times.Exactly(4));
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 0);

            // The path returns a filename with a lua extension, the panel navigation service should open a panel.
            // The collection of script panels should add the new panel.
            // The dictionary should add the path of the file opened.
            File.WriteAllText(this.filePathOpenTest, "content of the lua file");
            paths[0] = this.filePathOpenTest;
            Assert.DoesNotThrow(() => this.scriptingEngineRibbonPageGroupViewModel.OpenScriptCommand.Execute(null));
            this.fileDialogService.Verify(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4), Times.Exactly(5));
            this.panelNavigationService.Verify(x => x.Open(It.IsAny<IPanelViewModel>(), true), Times.Once);
            var viewModel = this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.ElementAt(0);
            Assert.AreEqual("content of the lua file", viewModel.AvalonEditor.Text);
            Assert.AreEqual("test.lua", viewModel.Caption);
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("test.lua"));
            string result;
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.TryGetValue("test.lua", out result);
            Assert.AreEqual(this.filePathOpenTest, result);

            this.scriptPanelViewModel.SetupProperty(x => x.Caption, "test.lua");

            // The path returns a filename with a python extension.
            // The collection of script panels should add the new panels.
            // The dictionary should add the path of the file opened.
            this.filePathOpenTest = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.py");
            File.WriteAllText(this.filePathOpenTest, "content of the python file");
            paths[0] = this.filePathOpenTest;
            Assert.DoesNotThrow(() => this.scriptingEngineRibbonPageGroupViewModel.OpenScriptCommand.Execute(null));
            this.fileDialogService.Verify(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4), Times.Exactly(6));
            this.panelNavigationService.Verify(x => x.Open(It.IsAny<IPanelViewModel>(), true), Times.Exactly(2));
            viewModel = this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.ElementAt(1);
            Assert.AreEqual("content of the python file", viewModel.AvalonEditor.Text);
            Assert.AreEqual("test.py", viewModel.Caption);
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("test.py"));
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.TryGetValue("test.py", out result);
            Assert.AreEqual(this.filePathOpenTest, result);

            // The path returns a filename with a text extension, the tabItemFactory.CreateTabItem should be called once.
            // The collection of tab items should add the new tab item.
            // The dictionary should add the path of the file opened.
            this.filePathOpenTest = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.txt");
            File.WriteAllText(this.filePathOpenTest, "content of the text file");
            paths[0] = this.filePathOpenTest;
            Assert.DoesNotThrow(() => this.scriptingEngineRibbonPageGroupViewModel.OpenScriptCommand.Execute(null));
            this.fileDialogService.Verify(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4), Times.Exactly(7));
            this.panelNavigationService.Verify(x => x.Open(It.IsAny<IPanelViewModel>(), true), Times.Exactly(3));
            viewModel = this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.ElementAt(2);
            Assert.AreEqual("content of the text file", viewModel.AvalonEditor.Text);
            Assert.AreEqual("test.txt", viewModel.Caption);
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("test.txt"));
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.TryGetValue("test.txt", out result);
            Assert.AreEqual(this.filePathOpenTest, result);

            // The method GetOpenFileDialog returns 3 paths
            // 1 of the 3 panels is already open, therefore only 2 panels should be created
            var filePathOpenTest2 = Path.Combine(TestContext.CurrentContext.TestDirectory, "test2.txt");
            File.WriteAllText(filePathOpenTest2, "content of the text file 2");
            var filePathOpenTest3 = Path.Combine(TestContext.CurrentContext.TestDirectory, "test2.py");
            File.WriteAllText(filePathOpenTest3, "content of the python file 2");
            paths = new[] {this.filePathOpenTest, filePathOpenTest2, filePathOpenTest3};
            this.fileDialogService.Setup(x => x.GetOpenFileDialog(false, false, true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4)).Returns(paths);
            Assert.DoesNotThrow(() => this.scriptingEngineRibbonPageGroupViewModel.OpenScriptCommand.Execute(null));
            this.fileDialogService.Verify(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4), Times.Exactly(8));
            this.panelNavigationService.Verify(x => x.Open(It.IsAny<IPanelViewModel>(), true), Times.Exactly(5));
            viewModel = this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.ElementAt(3);
            Assert.AreEqual("test2.txt", viewModel.Caption);
            Assert.AreEqual("content of the text file 2", viewModel.AvalonEditor.Text);
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("test2.txt"));
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.TryGetValue("test2.txt", out result);
            Assert.AreEqual(filePathOpenTest2, result);
            viewModel = this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.ElementAt(4);
            Assert.AreEqual("test2.py", viewModel.Caption);
            Assert.AreEqual("content of the python file 2", viewModel.AvalonEditor.Text);
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("test2.py"));
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.TryGetValue("test2.py", out result);
            Assert.AreEqual(filePathOpenTest3, result);
        }

        [Test]
        public void VerifyThatHandleClosedPanelWorks()
        {
            var avalonEditor = new TextEditor();

            var panel1 = new Mock<ScriptPanelViewModel>("panel 1", this.scriptingProxy.Object, "*.lua", this.openSessions, true);
            panel1.As<IPanelViewModel>();
            panel1.As<IScriptPanelViewModel>().SetupProperty(x => x.Caption, "panel 1");
            panel1.SetupProperty(x => x.AvalonEditor, avalonEditor);

            var panel2 = new Mock<ScriptPanelViewModel>("panel 2", this.scriptingProxy.Object, "*.lua", this.openSessions, true);
            panel2.As<IPanelViewModel>();
            panel2.As<IScriptPanelViewModel>().SetupProperty(x => x.Caption, "panel 2");
            panel2.SetupProperty(x => x.AvalonEditor, avalonEditor);
            
            var panel3 = new Mock<ScriptPanelViewModel>("panel 3", this.scriptingProxy.Object, "*.lua", this.openSessions, true);
            panel3.As<IPanelViewModel>();
            panel3.As<IScriptPanelViewModel>().SetupProperty(x => x.Caption, "panel 3");
            panel3.SetupProperty(x => x.AvalonEditor, avalonEditor);

            this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Add(panel1.Object);
            this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Add(panel2.Object);
            this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Add(panel3.Object);

            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.Add("panel 1", "path panel 1");
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.Add("panel 3", "path panel 3");

            var panelView = new Mock<IPanelView>();

            var navigationPanelEvent = new NavigationPanelEvent(panel3.Object, panelView.Object, PanelStatus.Closed);
            CDPMessageBus.Current.SendMessage(navigationPanelEvent);
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 2);
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.Count, 1);
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Contains(panel1.Object));
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Contains(panel2.Object));
            Assert.IsFalse(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Contains(panel3.Object));
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("panel 1"));
            Assert.IsFalse(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("panel 3"));

            navigationPanelEvent = new NavigationPanelEvent(panel1.Object, panelView.Object, PanelStatus.Closed);
            CDPMessageBus.Current.SendMessage(navigationPanelEvent);
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 1);
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.Count, 0);
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Contains(panel2.Object));
            Assert.IsFalse(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Contains(panel1.Object));
            Assert.IsFalse(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("panel 1"));
        }
    }
}