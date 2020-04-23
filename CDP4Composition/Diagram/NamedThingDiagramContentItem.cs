﻿// -------------------------------------------------------------------------------------------------
// <copyright file="NamedThingDiagramContentItem.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Diagram
{
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Represents a <see cref="ThingDiagramContentItem"/> with a name.
    /// </summary>
    public class NamedThingDiagramContentItem : ThingDiagramContentItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedThingDiagramContentItem"/> class.
        /// </summary>
        /// <param name="thing">
        /// The thing.
        /// </param>
        public NamedThingDiagramContentItem(Thing thing) : base(thing)
        {
            this.SetProperty();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedThingDiagramContentItem"/> class.
        /// </summary>
        /// <param name="diagramthing"></param>
        /// <param name="container"></param>
        public NamedThingDiagramContentItem(DiagramObject diagramthing, IDiagramEditorViewModel container) 
            : base(diagramthing, container)
        {
            this.SetProperty();
        }

        /// <summary>
        /// Sets <see cref="ThingDiagramContentItem.Thing"/> related property used to display
        /// </summary>
        private void SetProperty()
        {
            if (this.Thing is INamedThing namedThing)
            {
                this.FullName = namedThing.Name;
            }

            if (this.Thing is IShortNamedThing shortNamedThing)
            {
                this.ShortName = shortNamedThing.ShortName;
            }

            this.ClassKind = $"<<{this.Thing.ClassKind}>>";

            // special cases
            if (this.Thing is ParameterBase parameterBaseThing)
            {
                this.FullName = parameterBaseThing.UserFriendlyName;
                this.ShortName = parameterBaseThing.UserFriendlyShortName;
            }
        }

        /// <summary>
        /// Gets or sets the class kind of the <see cref="NamedThingDiagramContentItem"/>
        /// </summary>
        public string ClassKind { get; set; }

        /// <summary>
        /// Gets or sets the name of the <see cref="NamedThingDiagramContentItem"/>
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shortname of the <see cref="NamedThingDiagramContentItem"/>
        /// </summary>
        public string ShortName { get; set; } = string.Empty;
    }
}
