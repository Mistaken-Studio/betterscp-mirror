// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using Mistaken.Updater.Config;

namespace Mistaken.BetterSCP
{
    /// <inheritdoc/>
    public class Config : IAutoUpdatableConfig
    {
        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether debug should be displayed.
        /// </summary>
        [Description("If true then debug will be displayed")]
        public bool VerbouseOutput { get; set; }

        /// <summary>
        /// Gets or sets a list of SCPs that can use human voice chat.
        /// </summary>
        [Description("List of SCPs that can use human voice chat (SCP-939 mimic)")]
        public List<RoleType> AllowedSCPVCRoles { get; set; } = new List<RoleType>()
        {
            RoleType.Scp93953,
            RoleType.Scp93989,
        };

        /// <inheritdoc/>
        [Description("Auto Update Settings")]
        public System.Collections.Generic.Dictionary<string, string> AutoUpdateConfig { get; set; } = new System.Collections.Generic.Dictionary<string, string>
        {
            { "Url", "https://git.mistaken.pl/api/v4/projects/38" },
            { "Token", string.Empty },
            { "Type", "GITLAB" },
            { "VerbouseOutput", "false" },
        };
    }
}
