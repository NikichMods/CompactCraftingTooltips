// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections;
using System.Collections.Generic;

namespace CompactCraftingTooltips
{
    internal static class CraftingTooltipCompactor
    {
        internal static void TryCompact(
            object itemDefinition,
            IList tooltipRows)
        {
            string itemId = GameApi.GetId(itemDefinition);
            if (string.IsNullOrEmpty(itemId))
                return;

            IList locations = GameApi.GetItemCraftsIn(itemId);
            if (locations == null || locations.Count < 2)
                return;

            List<CraftLocationEntry> source =
                new List<CraftLocationEntry>();

            for (int i = 0; i < locations.Count; i++)
            {
                object location = locations[i];
                string id = GameApi.GetId(location);
                if (string.IsNullOrEmpty(id))
                    return;

                source.Add(new CraftLocationEntry(
                    id,
                    GameApi.Localize(id)));
            }

            string nativeSeparator =
                GameApi.GetNativeListSeparator();

            CompactFormatResult formatted =
                CraftingLocationFormatter.Format(
                    source,
                    nativeSeparator,
                    LocationQualifierLocalization.RefugeeCamp(
                        GameApi.GetCurrentLanguage()));

            if (!formatted.Changed)
                return;

            string vanillaText = BuildVanillaText(
                source,
                nativeSeparator);

            string replacement = BuildCompactText(
                formatted,
                nativeSeparator);

            for (int i = 0; i < tooltipRows.Count; i++)
            {
                string text;
                if (!GameApi.TryGetBubbleText(
                    tooltipRows[i],
                    out text))
                {
                    continue;
                }

                if (!string.Equals(
                    text,
                    vanillaText,
                    StringComparison.Ordinal))
                {
                    continue;
                }

                GameApi.SetBubbleText(
                    tooltipRows[i],
                    replacement);
                return;
            }
        }

        private static string BuildVanillaText(
            IList<CraftLocationEntry> source,
            string nativeSeparator)
        {
            List<string> names = new List<string>();

            for (int i = 0; i < source.Count; i++)
                names.Add(source[i].Name);

            return GameApi.Localize("crafted_at") +
                   " " +
                   string.Join(
                       nativeSeparator,
                       names.ToArray());
        }

        private static string BuildCompactText(
            CompactFormatResult formatted,
            string nativeSeparator)
        {
            List<string> entries = new List<string>();

            for (int i = 0;
                i < formatted.Entries.Count;
                i++)
            {
                entries.Add(formatted.Entries[i].Text);
            }

            string heading = GameApi.Localize("crafted_at");

            if (formatted.AllEntriesGrouped &&
                entries.Count >= 2)
            {
                return heading +
                       "\n" +
                       string.Join("\n", entries.ToArray());
            }

            return heading +
                   " " +
                   string.Join(
                       CraftingLocationFormatter.NormalizeSeparator(
                           nativeSeparator),
                       entries.ToArray());
        }
    }
}
