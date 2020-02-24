﻿// -------------------------------------------------------------------------------------------------
// <copyright file="AppSettings.cs" company="RHEA System S.A.">
//   Copyright (c) 2018-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services.AppSettingService
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    /// <summary>
    /// Base class from which all <see cref="AppSettings"/> shall derive
    /// </summary>
    public abstract class AppSettings
    {
        /// <summary>
        /// Initializes a the list <see cref="Init"/> class
        /// </summary>
        public void Init(bool initializeDefaults)
        {
            if (initializeDefaults)
            {
                this.DisabledPlugins = new List<string>();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DisabledPlugins"/>
        /// </summary>
        [JsonProperty]
        public List<string> DisabledPlugins { get; set; }
    }
}