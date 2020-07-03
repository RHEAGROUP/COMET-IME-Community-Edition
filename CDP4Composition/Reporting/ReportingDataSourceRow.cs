﻿using CDP4Common.EngineeringModelData;
using CDP4Common.SiteDirectoryData;

using System.Collections.Generic;
using System.Linq;

namespace CDP4Composition.Reporting
{
    public class ReportingDataSourceRow
    {
        #region Hierarchy

        private readonly ReportingDataSourceRow parent;

        public List<ReportingDataSourceRow> Children { get; } = new List<ReportingDataSourceRow>();

        #endregion

        #region Associated Element

        private readonly ElementBase elementBase;

        internal ElementDefinition ElementDefinition
            => (this.elementBase as ElementDefinition) ?? (this.elementBase as ElementUsage)?.ElementDefinition;

        internal ElementUsage ElementUsage
            => this.elementBase as ElementUsage;

        #endregion

        #region Parameters

        private readonly List<ReportingDataSourceParameter> reportedParameters = new List<ReportingDataSourceParameter>();

        #endregion

        public ReportingDataSourceRow(ElementBase elementBase, ReportingDataSourceRow parent = null)
        {
            this.parent = parent;

            this.elementBase = elementBase;

            var parameterStore = ParameterStore.GetInstance(elementBase.TopContainer);

            foreach (var type in parameterStore.DeclaredParameters.Values)
            {
                var parameter = type
                    .GetConstructor(new[] { typeof(ReportingDataSourceRow) })
                    .Invoke(new object[] { this }) as ReportingDataSourceParameter;

                this.reportedParameters.Add(parameter);

                if (parameterStore.ParameterTypes.TryGetValue(parameter.ShortName, out var parameterType))
                {
                    this.InitializeParameter(parameter, parameterType);
                }
            }

            foreach (var childUsage in this.ElementDefinition.ContainedElement)
            {
                this.Children.Add(new ReportingDataSourceRow(childUsage, this));
            }
        }

        private void InitializeParameter(ReportingDataSourceParameter reportedParameter, ParameterType parameterType)
        {
            var parameter = this.ElementDefinition.Parameter
                .SingleOrDefault(x => x.ParameterType == parameterType);

            if (parameter != null)
            {
                reportedParameter.Initialize(parameter.ValueSet);
            }

            var parameterOverride = this.ElementUsage?.ParameterOverride
                .SingleOrDefault(x => x.Parameter.ParameterType == parameterType);

            if (parameterOverride != null)
            {
                reportedParameter.Initialize(parameterOverride.ValueSet);
            }
        }

        public T GetParameter<T>() where T : ReportingDataSourceParameter
        {
            return this.reportedParameters.First(parameter => parameter is T) as T;
        }

        public List<ReportingDataSourceRow> GetTabularRepresentation()
        {
            var tabularRepresentation = new List<ReportingDataSourceRow>();

            tabularRepresentation.Add(this);

            foreach (var row in this.Children)
            {
                tabularRepresentation.AddRange(row.GetTabularRepresentation());
            }

            return tabularRepresentation;
        }
    }
}
