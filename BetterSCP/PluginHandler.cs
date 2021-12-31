// -----------------------------------------------------------------------
// <copyright file="PluginHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using HarmonyLib;

namespace Mistaken.BetterSCP
{
    /// <inheritdoc/>
    public class PluginHandler : Plugin<Config, Translation>
    {
        /// <inheritdoc/>
        public override string Author => "Mistaken Devs";

        /// <inheritdoc/>
        public override string Name => "BetterSCP";

        /// <inheritdoc/>
        public override string Prefix => "MBSCP";

        /// <inheritdoc/>
        public override PluginPriority Priority => PluginPriority.Default;

        /// <inheritdoc/>
        public override Version RequiredExiledVersion => new Version(4, 1, 2);

#pragma warning disable SA1202 // Elements should be ordered by access
        private Version version;

        /// <inheritdoc/>
        public override Version Version
        {
            get
            {
                if (this.version == null)
                    this.version = this.Assembly.GetName().Version;
                return this.version;
            }
        }
#pragma warning restore SA1202 // Elements should be ordered by access

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            Instance = this;

            Exiled.Events.Events.DisabledPatchesHashSet.Add(typeof(PlayableScps.Scp173).GetMethod(nameof(PlayableScps.Scp173.UpdateObservers), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            Exiled.Events.Events.Instance.ReloadDisabledPatches();

            this.harmony = new Harmony("mistaken.betterscp");
            this.harmony.PatchAll();

            new GlobalHandler(this);
            new SCPGUIHandler(this);

            API.Diagnostics.Module.OnEnable(this);

            base.OnEnabled();
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            API.Diagnostics.Module.OnDisable(this);

            Exiled.Events.Events.DisabledPatchesHashSet.Remove(typeof(PlayableScps.Scp173).GetMethod(nameof(PlayableScps.Scp173.UpdateObservers), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            Exiled.Events.Events.Instance.ReloadDisabledPatches();

            this.harmony.UnpatchAll();
            this.harmony = null;

            base.OnDisabled();
        }

        internal static PluginHandler Instance { get; private set; }

        private Harmony harmony;
    }
}
