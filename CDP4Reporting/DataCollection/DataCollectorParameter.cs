﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCollectorParameterColumn.cs" company="RHEA System S.A.">
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
    using CDP4Common.Types;

    /// <summary>
    /// Abstract base class from which all parameter columns for a <see cref="DataCollectorRow"/> need to derive.
    /// </summary>
    public abstract class DataCollectorParameter<TRow, TValue> : DataCollectorColumn<TRow>, IDataCollectorParameter where TRow : DataCollectorRow, new()
    {
        /// <summary>
        /// The associated <see cref="CDP4Common.EngineeringModelData.ParameterBase"/>.
        /// </summary>
        public ParameterBase ParameterBase { get; set; }

        /// <summary>
        /// Gets or sets the associated <see cref="ParameterType"/> short name.
        /// </summary>
        protected string ShortName { get; set; }

        /// <summary>
        /// Gets or sets the associated <see cref="ParameterType"/> field name in the result Data Object.
        /// </summary>
        internal string FieldName { get; set; }

        /// <summary>
        /// Gets a flag that indicates that a parameter also collects parent values up a tree of <see cref="CategoryDecompositionHierarchy"/>s
        /// </summary>
        public bool CollectParentValues { get; private set; }

        /// <summary>
        /// Gets or sets the associated field name prefix in the result Data Object.
        /// </summary>
        public string ParentValuePrefix { get; set; } = string.Empty;

        /// <summary>
        /// The <see cref="ParameterValueContext"/> for which the value needs to be retrieved
        /// in case <see cref="DataCollectorParameter{TRow, TValue}.ParameterBase"/> is of type <see cref="Parameter"/>
        /// or of type <see cref="ParameterOverride"/>
        /// </summary>
        internal ParameterValueContext ParameterValueContext { get; set; } = ParameterValueContext.PublishedValue;

        /// <summary>
        /// The <see cref="ParameterValueContext"/> for which the value needs to be retrieved
        /// in case <see cref="DataCollectorParameter{TRow, TValue}.ParameterBase"/> is of type <see cref="ParameterSubscription"/>
        /// </summary>
        internal ParameterValueContext ParameterSubscriptionValueContext { get; set; } = ParameterValueContext.PublishedValue;

        /// <summary>
        /// The value of the <see cref="IValueSet"/> of the associated <see cref="ParameterBase"/>.
        /// </summary>
        public TValue Value
        {
            get
            {
                if (this.ValueSets?.Any() ?? false)
                { 
                    return this.GetValueSetValue(this.ValueSets.FirstOrDefault());
                }

                return default;
            }
        }

        /// <summary>
        /// The <see cref="ParameterSwitchKind"/> of the <see cref="IValueSet"/>s of the associated <see cref="ParameterBase"/>.
        /// </summary>
        public string ValueSwitch
        {
            get
            {
                if (this.ValueSets?.Any() ?? false)
                { 
                    return this.ValueSets.FirstOrDefault()?.ValueSwitch.ToString() ?? string.Empty;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Get the correct value of a <see cref="IValueSet"/>.
        /// </summary>
        /// <param name="valueSet">
        /// The <see cref="IValueSet"/>
        /// </param>
        /// <param name="position">
        /// Position of the expected value within the <see cref="IValueSet"/>
        /// </param>
        /// <returns>The correct value of the <see cref="IValueSet"/></returns>
        public TValue GetValueSetValue(IValueSet valueSet, int position = 0)
        {
            ValueArray<string> valueArray = null;

            if (valueSet is ParameterSubscriptionValueSet valueSetSubscription)
            {
                if (this.ParameterSubscriptionValueContext == ParameterValueContext.ActualValue)
                {
                    valueArray = valueSet.ActualValue;
                }

                if (this.ParameterSubscriptionValueContext == ParameterValueContext.PublishedValue)
                {
                    valueArray = valueSetSubscription.Computed;
                }
            }

            if (valueSet is ParameterValueSetBase valueSetBase)
            {
                if (this.ParameterValueContext == ParameterValueContext.ActualValue)
                {
                    valueArray = valueSetBase.ActualValue;
                }

                if (this.ParameterValueContext == ParameterValueContext.PublishedValue)
                {
                    valueArray = valueSetBase.Published;
                }
            }

            if (valueArray != null)
            {
                if (position == 0)
                {
                    return this.Parse(valueArray.FirstOrDefault()) ?? default;
                }

                return this.Parse(valueArray[position]) ?? default; 
            }

            return default;
        }

        /// <summary>
        /// The ValueSets of the associated object.
        /// The <see cref="IEnumerable{IValueSet}"/>s of the associated object/>.
        /// </summary>
        public IEnumerable<IValueSet> ValueSets { get; set; }

        /// <summary>
        /// Gets the owner <see cref="DomainOfExpertise"/> of the associated <see cref="ParameterBase"/>.
        /// </summary>
        public DomainOfExpertise Owner { get; set; }

        /// <summary>
        /// Gets the <see cref="CollectParentValuesAttribute"/> decorating the property described by <paramref name="propertyType"/>.
        /// </summary>
        /// <param name="propertyType">
        /// Describes the current property.
        /// </param>
        /// <returns>
        /// The <see cref="CollectParentValuesAttribute"/> decorating the current parameter class.
        /// </returns>
        protected static CollectParentValuesAttribute GetCollectParentValuesAttribute(PropertyInfo propertyType)
        {
            var attr = Attribute
                .GetCustomAttributes(propertyType)
                .SingleOrDefault(attribute => attribute is CollectParentValuesAttribute);

            return attr as CollectParentValuesAttribute;
        }

        /// <summary>
        /// Initializes a reported parameter column based on the corresponding
        /// <see cref="CDP4Common.EngineeringModelData.ParameterBase"/> within the associated
        /// <see cref="DataCollectorNode{T}"/>.
        /// </summary>
        /// <param name="node">
        /// The associated <see cref="DataCollectorNode{TRow}"/>.
        /// </param>
        /// <param name="propertyInfo">
        /// The <see cref="PropertyInfo"/> object for this <see cref="DataCollectorCategory{TRow}"/>'s usage in a class.
        /// </param>
        internal override void Initialize(DataCollectorNode<TRow> node, PropertyInfo propertyInfo)
        {
            var definedShortNameAttribute = GetParameterAttribute(propertyInfo);

            this.ShortName = definedShortNameAttribute?.ShortName;
            this.FieldName = definedShortNameAttribute?.FieldName;

            this.CollectParentValues = GetCollectParentValuesAttribute(propertyInfo) != null;

            var parameterValueContext = GetParameterValueContextAttribute(propertyInfo);

            if (parameterValueContext != null)
            {
                this.ParameterValueContext = parameterValueContext.ParameterValueContext;
                this.ParameterSubscriptionValueContext = parameterValueContext.ParameterSubscriptionValueContext;
            }

            this.Node = node;

            var firstNestedParameter = this.Node.NestedElement.NestedParameter.FirstOrDefault(
                x => x.AssociatedParameter.ParameterType.ShortName == this.ShortName);

            this.ParameterBase = firstNestedParameter?.AssociatedParameter;

            this.Owner = (firstNestedParameter?.ValueSet as IOwnedThing)?.Owner ?? firstNestedParameter?.Owner;

            var nestedParameterData =
                this.Node.NestedElement.NestedParameter
                .Where(x => x.AssociatedParameter.ParameterType.ShortName == this.ShortName)
                .Select(x => x.ValueSet)
                .ToList();

            if (nestedParameterData.Any())
            {
                this.ValueSets = nestedParameterData;
            }
        }

        /// <summary>
        /// Gets the <see cref="ParameterValueContextAttribute"/> decorating the property described by <paramref name="propertyType"/>.
        /// </summary>
        /// <param name="propertyType">
        /// Describes the current property.
        /// </param>
        /// <returns>
        /// The <see cref="ParameterValueContextAttribute"/> decorating the current parameter class.
        /// </returns>
        protected static ParameterValueContextAttribute GetParameterValueContextAttribute(PropertyInfo propertyType)
        {
            var attr = Attribute
                .GetCustomAttributes(propertyType)
                .SingleOrDefault(attribute => attribute is ParameterValueContextAttribute);

            return attr as ParameterValueContextAttribute;
        }

        /// <summary>
        /// Populates with data the <see cref="DataTable.Columns"/> associated with this object
        /// in the given <paramref name="row"/>.
        /// </summary>
        /// <param name="table">
        /// The <see cref="DataTable"/> to which the <paramref name="row"/> belongs to.
        /// </param>
        /// <param name="row">
        /// The <see cref="DataRow"/> to be populated.
        /// </param>
        public override void Populate(DataTable table, DataRow row)
        {
            var fieldName = $"{this.ParentValuePrefix}{this.FieldName}";

            if (this.HasValueSets)
            {
                foreach (var valueSet in this.ValueSets)
                {
                    var columnName = $"{fieldName}{valueSet.ActualState?.ShortName ?? ""}";

                    if (!table.Columns.Contains(columnName))
                    {
                        table.Columns.Add(columnName, typeof(TValue));
                    }

                    if (valueSet.ActualState == null)
                    {
                        row[columnName] = this.Value;
                    }
                    else
                    {
                        var stateDependentValueSet = this.ValueSets.FirstOrDefault(x => x.ActualState == valueSet.ActualState);

                        if (stateDependentValueSet != null)
                        {
                            row[columnName] = this.GetValueSetValue(stateDependentValueSet);
                        }
                    }
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(this.ParentValuePrefix))
                {
                    var columnName = fieldName;

                    if (!table.Columns.Contains(columnName))
                    {
                        table.Columns.Add(columnName, typeof(TValue));
                    }
                }
            }
        }

        /// <summary>
        /// Parses a parameter value as the type that will be used for the row representation.
        /// </summary>
        /// <param name="value">
        /// The parameter value to be parsed. This needs to be specified as a state dependent
        /// <see cref="DataCollectorParameter{TRow,TValue}"/> can have multiple values.
        /// </param>
        /// <returns>
        /// The parsed value.
        /// </returns>
        public abstract TValue Parse(string value);

        /// <summary>
        /// Gets a flag that indicates whether this instance has <see cref="IValueSet"/>s.
        /// </summary>
        public bool HasValueSets => this.ValueSets?.Any() ?? false;
    }
}
