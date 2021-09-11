// -----------------------------------------------------------------------
// <copyright file="Translation.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Interfaces;

namespace Mistaken.BetterSCP
{
    /// <inheritdoc/>
    public class Translation : ITranslation
    {
#pragma warning disable CS1591 // Brak komentarza XML dla widocznego publicznie typu lub składowej
        public string Info_SCP_Swap { get; set; } = "You can type \".swapscp\" to <b>change</b> <color=red>SCP</color> for next <color=yellow>{0}</color> seconds";

        public string Info_SCP_List { get; set; } = "You are <color=red>SCP</color> with:";

        public string Info_SCP_List_Element { get; set; } = "<color=yellow>{0}</color> as <color=red>{1}</color>";
#pragma warning restore CS1591 // Brak komentarza XML dla widocznego publicznie typu lub składowej
    }
}
