﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCollectorNode.cs" company="RHEA System S.A.">
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.DataCollection
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Class representing a node in the hierarhical tree upon which the data object is based.
    /// Each node corresponds to a row in the data object's tabular representation.
    /// </summary>
    /// <typeparam name="T">
    /// The <see cref="DataCollectorRow"/> representing the data collector rows.
    /// </typeparam>
    internal class DataCollectorNode<T> where T : DataCollectorRow, new()
    {
        /// <summary>
        /// A <see cref="Dictionary{TKey,TValue}"/> of all the <see cref="DataCollectorColumn{T}"/>s
        /// declared as <see cref="DataCollectorRow"/> fields.
        /// </summary>
        private IEnumerable<KeyValuePair<PropertyInfo, Type>> allColumns => this.normalColumns.AsEnumerable().Union(this.stateDependentColumns);

        /// <summary>
        /// Gets or sets a <see cref="Dictionary{TKey,TValue}"/> of all the <see cref="DataCollectorColumn{T}"/>s
        /// declared as <see cref="DataCollectorRow"/> fields.
        /// </summary>
        private Dictionary<PropertyInfo, Type> normalColumns { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="Dictionary{TKey,TValue}"/> of all the <see cref="DataCollectorStateDependentPerRowParameter{TRow,TValue}"/>s
        /// declared as <see cref="DataCollectorRow"/> fields.
        /// </summary>
        private Dictionary<PropertyInfo, Type> stateDependentColumns { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="IEnumerable{T}"/> of all the public getters on the <see cref="DataCollectorRow"/>
        /// representation.
        /// </summary>
        private IEnumerable<PropertyInfo> publicGetterProperties { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataCollectorRow"/> representation of the current node.
        /// </summary>
        private T rowRepresentation { get; set; }

        /// <summary>
        /// Gets the parent node in the hierarhical tree upon which the data object is based.
        /// </summary>
        private DataCollectorNode<T> parent { get; }

        /// <summary>
        /// Gets or sets The filtering <see cref="CategoryDecompositionHierarchy"/> that must be matched on the current <see cref="ElementBase"/>.
        /// </summary>
        private CategoryDecompositionHierarchy categoryDecompositionHierarchy { get; set; }

        /// <summary>
        /// Gets or sets the name of the field/column when the data is transfered to a <see cref="DataRow"/>.
        /// </summary>
        private string FieldName { get; set; }

        /// <summary>
        /// The children nodes in the hierarhical tree upon which the data object is based.
        /// </summary>
        internal List<DataCollectorNode<T>> Children { get; } = new List<DataCollectorNode<T>>();

        /// <summary>
        /// Gets the <see cref="CDP4Common.EngineeringModelData.NestedElement"/> associated with this node.
        /// </summary>
        internal NestedElement NestedElement { get; }

        /// <summary>
        /// The <see cref="ICategorizableThing"/> associated with this node.
        /// </summary>
        internal ICategorizableThing CategorizableThing => this.ElementBase;

        /// <summary>
        /// The <see cref="CDP4Common.EngineeringModelData.ElementBase"/> associated with this node.
        /// </summary>
        internal ElementBase ElementBase => this.NestedElement.GetElementBase();

        /// <summary>
        /// The <see cref="CDP4Common.EngineeringModelData.ElementDefinition"/> representing this node.
        /// </summary>
        internal ElementDefinition ElementDefinition => this.NestedElement.GetElementDefinition();

        /// <summary>
        /// The <see cref="ElementUsage"/> representing this node, if it exists.
        /// </summary>
        internal ElementUsage ElementUsage =>  this.NestedElement.GetElementUsage();

        /// <summary>
        /// GEts or sets an <see cref="IReadOnlyList{Category}"/> that contains all <see cref="Category"/>s in scope of this node.
        /// </summary>
        public IReadOnlyList<Category> CategoriesInRequiredRdl { get; set; }

        /// <summary>
        /// Boolean flag indicating whether the current <see cref="ElementBase"/> matches the <see cref="categoryDecompositionHierarchy"/>.
        /// </summary>
        private bool IsVisible => this.NestedElement.IsMemberOfCategory(this.categoryDecompositionHierarchy.Category);

        /// <summary>
        /// Initializes a new instance of the <see cref="DataCollectorNode{T}"/> class.
        /// </summary>
        /// <param name="categoryDecompositionHierarchy">
        /// The <see cref="CategoryDecompositionHierarchy"/> associated with this node's subtree.
        /// </param>
        /// <param name="topElement">
        /// The <see cref="CDP4Common.EngineeringModelData.NestedElement"/> associated with this node.
        /// </param>
        /// <param name="parent">
        /// The parent node in the hierarhical tree upon which the data collector is based.
        /// </param>
        public DataCollectorNode(
            CategoryDecompositionHierarchy categoryDecompositionHierarchy,
            NestedElement topElement,
            DataCollectorNode<T> parent = null)
        {
            this.Initialize();
            this.NestedElement = topElement;
            this.parent = parent;
            this.InitializeCategoryDecompositionHierarchy(categoryDecompositionHierarchy);
        }

        /// <summary>
        /// Sets all properties according to data in the related <see cref="CategoryDecompositionHierarchy"/>
        /// </summary>
        /// <param name="categoryDecompositionHierarchy">
        /// The related <see cref="CategoryDecompositionHierarchy"/>
        /// </param>
        private void InitializeCategoryDecompositionHierarchy(CategoryDecompositionHierarchy categoryDecompositionHierarchy)
        {
            this.categoryDecompositionHierarchy = categoryDecompositionHierarchy;
            this.CategoriesInRequiredRdl = categoryDecompositionHierarchy.CategoriesInRequiredRdl;

            if (categoryDecompositionHierarchy.IsRecursive)
            {
                var level = this.CountCategoryRecursionLevel(this.categoryDecompositionHierarchy.Category);
                this.FieldName = $"{categoryDecompositionHierarchy.FieldName}_{Math.Min(level, categoryDecompositionHierarchy.MaximumRecursiveLevels)}";
            }
            else
            {
                this.FieldName = categoryDecompositionHierarchy.FieldName;
            }
        }

        /// <summary>
        /// Initializes this instance of <see cref="DataCollectorNode{T}"/>
        /// </summary>
        private void Initialize()
        {
            this.stateDependentColumns = typeof(T).GetProperties()
                .Where(f => this.isSubclassOfRawGeneric(f.PropertyType, typeof(DataCollectorStateDependentPerRowParameter<,>)))
                .ToDictionary(f => f, f => f.PropertyType);

            if (this.stateDependentColumns.Count > 1)
            {
                throw new NotSupportedException($"Currently only one property of {typeof(DataCollectorStateDependentPerRowParameter<,>).Name} can be used per {nameof(DataCollectorRow)}.");
            }

            this.normalColumns = typeof(T).GetProperties()
                .Where(f => f.PropertyType.IsSubclassOf(typeof(DataCollectorColumn<T>)))
                .Except(this.stateDependentColumns.Keys)
                .ToDictionary(f => f, f => f.PropertyType);

            this.publicGetterProperties = typeof(T).GetProperties()
                .Where(p => p.GetMethod?.IsPublic == true)
                .Except(this.allColumns.Select(x =>x.Key));
        }

        /// <summary>
        /// Checks if a property <see cref="Type"/> is a subclass of a specific generic type.
        /// </summary>
        /// <param name="propertyType">The property <see cref="Type"/></param>
        /// <param name="genericType">The generic <see cref="Type"/></param>
        /// <returns>True if the <paramref name="propertyType"/> is a subclass of <see cref="genericType"/>, otherwise false.</returns>
        private bool isSubclassOfRawGeneric(Type propertyType, Type genericType)
        {
            while (propertyType != null && propertyType != typeof(object))
            {
                var cur = propertyType.IsGenericType ? propertyType.GetGenericTypeDefinition() : propertyType;

                if (genericType == cur)
                {
                    return true;
                }

                propertyType = propertyType.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Counts the times a category was (recusrively) found in this instance or up its parent tree.
        /// </summary>
        /// <param name="category">
        /// The <see cref="Category"/>.
        /// </param>
        /// <returns>
        /// True if found, otherwise false.
        /// </returns>
        internal int CountCategoryRecursionLevel(Category category)
        {
            var count = this.parent?.CountCategoryRecursionLevel(category) ?? 0;

            if (this.NestedElement.IsMemberOfCategory(category))
            {
                count += 1;
            }

            return count;
        }

        /// <summary>
        /// Creates a <see cref="DataTable"/> representation based on the <see cref="DataCollectorRow"/>
        /// representation.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/> representation.
        /// </returns>
        public DataTable GetTable()
        {
            var table = new DataTable();

            for (var hierarchy = this.categoryDecompositionHierarchy; hierarchy != null; hierarchy = hierarchy.Child)
            {
                if (hierarchy.IsRecursive)
                {
                    for (var recursiveCounter = 1; recursiveCounter <= hierarchy.MaximumRecursiveLevels; recursiveCounter++)
                    {
                        table.Columns.Add($"{hierarchy.FieldName}_{recursiveCounter}", typeof(string));
                    }
                }
                else
                {
                    table.Columns.Add(hierarchy.FieldName, typeof(string));
                }
            }

            foreach (var publicGetter in this.publicGetterProperties)
            {
                table.Columns.Add(publicGetter.Name, publicGetter.GetMethod.ReturnType);
            }

            this.AddDataRows(table);

            return table;
        }

        /// <summary>
        /// Gets the row representation of this node.
        /// </summary>
        /// <returns>
        /// A <see cref="DataCollectorRow"/>.
        /// </returns>
        private T GetRowRepresentation()
        {
            if (!this.IsVisible)
            {
                return null;
            }

            if (this.rowRepresentation != null)
            {
                return this.rowRepresentation;
            }

            var row = new T
            {
                ElementBase = this.ElementBase,
                IsVisible = this.IsVisible
            };

            foreach (var rowField in this.allColumns)
            {
                var newObject = Activator.CreateInstance(rowField.Value);
                var column = newObject as DataCollectorColumn<T>;

                if (newObject is DataCollectorCategory<T> categoryColumn)
                {
                    categoryColumn.CategoriesInRequiredRdl = this.CategoriesInRequiredRdl;
                }

                column?.Initialize(this, rowField.Key);

                rowField.Key.SetValue(row, column);
            }

            return this.rowRepresentation = row;
        }

        /// <summary>
        /// Gets the columns of type <see cref="TP"/> associated with this node.
        /// </summary>
        /// <typeparam name="TP">
        /// The desired column type.
        /// </typeparam>
        /// <returns>
        /// The <see cref="IEnumerable{TP}"/> of <see cref="DataCollectorColumn{T}"/>s of type <see cref="TP"/>.
        /// </returns>
        public IEnumerable<TP> GetColumns<TP>() where TP : DataCollectorColumn<T>
        {
            return this.allColumns.Where(x => x.Value == typeof(TP) || x.Value.IsSubclassOf(typeof(TP)))
                .Select(x => x.Key.GetValue(this.GetRowRepresentation()) as TP);
        }

        /// <summary>
        /// Adds to the <paramref name="table"/> the <see cref="DataRow"/> representations
        /// of this node's subtree.
        /// </summary>
        /// <param name="table">
        /// The associated <see cref="DataTable"/>.
        /// </param>
        internal void AddDataRows(DataTable table)
        {
            if (this.IsVisible && this.categoryDecompositionHierarchy.Child == null)
            {
                table.Rows.Add(this.GetDataRow(table));
            }

            foreach (var child in this.Children)
            {
                child.AddDataRows(table);
            }
        }

        /// <summary>
        /// Gets the <see cref="DataRow"/> representation of this node.
        /// </summary>
        /// <param name="table">
        /// The associated <see cref="DataTable"/>.
        /// </param>
        /// <returns>
        /// A <see cref="DataRow"/>.
        /// </returns>
        private DataRow GetDataRow(DataTable table)
        {
            var row = table.NewRow();

            this.InitializeCategoryColumns(row);

            foreach (var rowField in this.normalColumns)
            {
                var column = rowField.Key.GetValue(this.GetRowRepresentation()) as DataCollectorColumn<T>;

                column?.Populate(table, row);
            }

            foreach (var publicGetter in this.publicGetterProperties)
            {
                row[publicGetter.Name] = publicGetter.GetMethod.Invoke(
                    this.GetRowRepresentation(),
                    new object[] { });
            }

            foreach (var rowField in this.stateDependentColumns)
            {
                var column = rowField.Key.GetValue(this.GetRowRepresentation()) as DataCollectorColumn<T>;

                column?.Populate(table, row);
            }

            return row;
        }

        /// <summary>
        /// Initializes the category columns for the given <paramref name="row"/>
        /// with values from the current node.
        /// </summary>
        /// <param name="row">
        /// The <see cref="DataRow"/> to initialize.
        /// </param>
        private void InitializeCategoryColumns(DataRow row)
        {
            this.parent?.InitializeCategoryColumns(row);

            row[this.FieldName] = this.ElementBase.Name;
        }
    }
}