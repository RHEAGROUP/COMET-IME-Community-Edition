﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewSheetGenerator.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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

namespace CDP4CrossViewEditor.Generator
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CrossViewEditor.Assemblers;

    using CDP4Dal;

    using NetOffice.ExcelApi;
    using NetOffice.ExcelApi.Enums;

    using NLog;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// The purpose of the <see cref="CrossviewSheetGenerator"/> is to generate in Excel
    /// the crossview sheet that contains the selected <see cref="ElementDefinition"/>s, <see cref="ElementUsage"/>s,
    /// and for each <see cref="ParameterType"/> display the value of the <see cref="Parameter"/> and/or <see cref="ParameterOverride"/>
    /// for the active <see cref="Participant"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CrossviewSheetGenerator
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The current excel application
        /// </summary>
        private Application excelApplication;

        /// <summary>
        /// The <see cref="Iteration"/> for which the crossview sheet needs to be generated.
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// The <see cref="Participant"/> for which the crossview sheet needs to be generated.
        /// </summary>
        private readonly Participant participant;

        /// <summary>
        /// The crossview <see cref="Worksheet"/>.
        /// </summary>
        private Worksheet crossviewSheet;

        /// <summary>
        /// The <see cref="CrossviewArrayAssembler"/>
        /// </summary>
        private CrossviewArrayAssembler crossviewArrayAssember;

        /// <summary>
        /// The <see cref="CrossviewHeaderArrayAssembler"/>
        /// </summary>
        private CrossviewHeaderArrayAssembler headerArrayAssembler;

        /// <summary>
        /// Gets the <see cref="ISession"/> that is active
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossviewSheetGenerator"/> class.
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> for which the crossview sheet is generated.
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the crossview sheet is generated.
        /// </param>
        /// <param name="participant">
        /// The <see cref="Participant"/> for which the crossview sheet is generated.
        /// </param>
        public CrossviewSheetGenerator(ISession session, Iteration iteration, Participant participant)
        {
            this.session = session;
            this.iteration = iteration;
            this.participant = participant;
        }

        /// <summary>
        /// Rebuild the crossview sheet
        /// </summary>
        /// <param name="application">
        /// The excel application object that contains the <see cref="Workbook"/>
        /// </param>
        /// <param name="workbook">
        /// The current <see cref="Workbook"/> when crossview sheet will be rebuild.
        /// </param>
        /// <param name="elementDefinitions">
        /// Selected element definition list
        /// </param>
        /// <param name="parameterTypes">
        /// Selected parameter types list
        /// </param>
        public void Rebuild(Application application, Workbook workbook, IEnumerable<ElementDefinition> elementDefinitions,
            IEnumerable<ParameterType> parameterTypes)
        {
            var sw = new Stopwatch();
            sw.Start();

            this.excelApplication = application;

            this.excelApplication.StatusBar = "Rebuilding Crossview Sheet";

            var enabledEvents = application.EnableEvents;
            var displayAlerts = application.DisplayAlerts;
            var screenupdating = application.ScreenUpdating;
            var calculation = application.Calculation;

            application.EnableEvents = false;
            application.DisplayAlerts = false;
            application.Calculation = XlCalculation.xlCalculationManual;
            application.ScreenUpdating = false;

            try
            {
                application.Cursor = XlMousePointer.xlWait;

                this.crossviewSheet = CrossviewSheetUtilities.RetrieveSheet(workbook, true);

                CrossviewSheetUtilities.ApplyLocking(this.crossviewSheet, false);

                this.PopulateSheetArrays(elementDefinitions, parameterTypes);
                this.WriteSheet();
                this.ApplySheetSettings();

                CrossviewSheetUtilities.ApplyLocking(this.crossviewSheet, true);

                this.excelApplication.StatusBar = $"CDP4: Crossview Sheet rebuilt in {sw.ElapsedMilliseconds} [ms]";
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                this.excelApplication.StatusBar = $"CDP4: The following error occured while rebuilding the sheet: {ex.Message}";
            }
            finally
            {
                application.EnableEvents = enabledEvents;
                application.DisplayAlerts = displayAlerts;
                application.Calculation = calculation;
                application.ScreenUpdating = screenupdating;

                application.Cursor = XlMousePointer.xlDefault;
            }
        }

        /// <summary>
        /// Apply formatting settings to the crossview sheet
        /// </summary>
        private void ApplySheetSettings()
        {
            var outline = this.crossviewSheet.Outline;
            outline.AutomaticStyles = false;
            outline.SummaryRow = XlSummaryRow.xlSummaryAbove;
            outline.SummaryColumn = XlSummaryColumn.xlSummaryOnRight;

            this.excelApplication.ActiveWindow.DisplayGridlines = false;
        }

        /// <summary>
        /// collect the information that is to be written to the crossview sheet
        /// </summary>
        /// <param name="elementDefinitions">
        /// Selected element definition list
        /// </param>
        /// <param name="parameterTypes">
        /// Selected parameter types list
        /// </param>
        private void PopulateSheetArrays(IEnumerable<ElementDefinition> elementDefinitions, IEnumerable<ParameterType> parameterTypes)
        {
            // Instantiate the different rows
            var assembler = new CrossviewSheetRowAssembler();
            assembler.Assemble(elementDefinitions);
            var excelRows = assembler.ExcelRows;

            // Use the instantiated rows to populate the excel array
            this.crossviewArrayAssember = new CrossviewArrayAssembler(excelRows, parameterTypes);

            // Instantiate header
            this.headerArrayAssembler = new CrossviewHeaderArrayAssembler(
                this.session,
                this.iteration,
                this.participant,
                this.crossviewArrayAssember.ContentArray.GetLength(1));
        }

        /// <summary>
        /// Write the data to the crossview sheet
        /// </summary>
        private void WriteSheet()
        {
            this.WriteHeader();
            this.WriteRows();
        }

        /// <summary>
        /// Write the header info to the crossview sheet
        /// </summary>
        private void WriteHeader()
        {
            var numberOfRows = this.headerArrayAssembler.HeaderArray.GetLength(0);
            var numberOfColumns = this.headerArrayAssembler.HeaderArray.GetLength(1);

            var range = this.crossviewSheet.Range(this.crossviewSheet.Cells[1, 1], this.crossviewSheet.Cells[numberOfRows, numberOfColumns]);
            range.HorizontalAlignment = XlHAlign.xlHAlignLeft;
            range.NumberFormat = this.headerArrayAssembler.FormatArray;
            range.Locked = this.headerArrayAssembler.LockArray;
            range.Name = CrossviewSheetConstants.HeaderName;
            range.Value = this.headerArrayAssembler.HeaderArray;
            range.Interior.ColorIndex = 8;
            range.EntireColumn.AutoFit();
        }

        /// <summary>
        /// Write the content of the <see cref="bodyContent"/> to the crossview sheet
        /// </summary>
        private void WriteRows()
        {
            var numberOfHeaderRows = this.headerArrayAssembler.HeaderArray.GetLength(0);

            var numberOfBodyRows = this.crossviewArrayAssember.ContentArray.GetLength(0);
            var numberOfColumns = this.crossviewArrayAssember.ContentArray.GetLength(1);

            var dataStartRow = numberOfHeaderRows + this.crossviewArrayAssember.ActualHeaderDepth;
            var dataEndRow = numberOfHeaderRows + numberOfBodyRows;

            var formattedRange = this.crossviewSheet.Range(
                this.crossviewSheet.Cells[numberOfHeaderRows + 1, 1],
                this.crossviewSheet.Cells[dataStartRow, numberOfColumns]);
            formattedRange.Interior.ColorIndex = 34;
            formattedRange.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            formattedRange.VerticalAlignment = XlVAlign.xlVAlignCenter;
            formattedRange.Font.Bold = true;
            formattedRange.Font.Underline = true;
            formattedRange.Font.Size = 11;
            formattedRange.EntireColumn.AutoFit();

            // format fixed columns
            for (var i = 1; i <= CrossviewSheetConstants.FixedColumns; ++i)
            {
                this.crossviewSheet.Range(
                        this.crossviewSheet.Cells[numberOfHeaderRows + 1, i],
                        this.crossviewSheet.Cells[dataStartRow, i])
                    .Merge();
            }

            // group horizontal parameter columns
            var bodyHeaderDictionary = this.crossviewArrayAssember.headerDictionary;
            foreach (var parameterTypeShortName in bodyHeaderDictionary.Keys)
            {
                var min = bodyHeaderDictionary[parameterTypeShortName].Values.Min();
                var max = bodyHeaderDictionary[parameterTypeShortName].Values.Max();

                if (min == max)
                {
                    continue;
                }

                this.crossviewSheet.Range(
                        this.crossviewSheet.Cells[numberOfHeaderRows + 1, min + 1],
                        this.crossviewSheet.Cells[numberOfHeaderRows + 1, max + 1])
                    .Merge();
            }

            var parameterRange = this.crossviewSheet.Range(
                this.crossviewSheet.Cells[numberOfHeaderRows + 1, 1],
                this.crossviewSheet.Cells[dataEndRow, numberOfColumns]);
            parameterRange.Name = CrossviewSheetConstants.RangeName;
            parameterRange.NumberFormat = this.crossviewArrayAssember.FormatArray;
            parameterRange.Value = this.crossviewArrayAssember.ContentArray;
            parameterRange.Locked = this.crossviewArrayAssember.LockArray;
            parameterRange.EntireColumn.AutoFit();

            this.ApplyCellNames(dataStartRow, CrossviewSheetConstants.FixedColumns + 1, dataEndRow, numberOfColumns);

            this.crossviewSheet.Cells[dataStartRow + 1, 1].Select();
            this.excelApplication.ActiveWindow.FreezePanes = true;
        }

        /// <summary>
        /// Apply cell names to the actual value column
        /// </summary>
        /// <param name="beginRow">
        /// The row at which the range begins
        /// </param>
        /// <param name="beginColumn">
        /// The column at which the range begins
        /// </param>
        /// <param name="endRow">
        /// The row at which the range ends
        /// </param>
        /// <param name="endColumn">
        /// The column at which the range ends
        /// </param>
        private void ApplyCellNames(int beginRow, int beginColumn, int endRow, int endColumn)
        {
            var range =
                this.crossviewSheet.Range(
                    this.crossviewSheet.Cells[beginRow, beginColumn],
                    this.crossviewSheet.Cells[endRow, endColumn]);

            range.CreateNames(top: false, left: false, bottom: false, right: true);
        }
    }
}
