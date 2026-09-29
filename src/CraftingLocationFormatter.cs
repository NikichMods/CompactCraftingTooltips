// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CompactCraftingTooltips
{
    internal sealed class CraftLocationEntry
    {
        internal CraftLocationEntry(string id, string name)
        {
            Id = id ?? string.Empty;
            Name = name ?? string.Empty;
        }

        internal string Id { get; private set; }
        internal string Name { get; private set; }
    }

    internal sealed class CompactDisplayEntry
    {
        internal CompactDisplayEntry(string text, int sourceCount)
        {
            Text = text;
            SourceCount = sourceCount;
        }

        internal string Text { get; private set; }
        internal int SourceCount { get; private set; }
    }

    internal sealed class CompactFormatResult
    {
        internal CompactFormatResult(
            List<CompactDisplayEntry> entries,
            bool changed)
        {
            Entries = entries;
            Changed = changed;
        }

        internal List<CompactDisplayEntry> Entries { get; private set; }
        internal bool Changed { get; private set; }

        internal bool AllEntriesGrouped
        {
            get
            {
                if (Entries.Count == 0)
                    return false;

                for (int i = 0; i < Entries.Count; i++)
                {
                    if (Entries[i].SourceCount < 2)
                        return false;
                }

                return true;
            }
        }
    }

    internal static class CraftingLocationFormatter
    {
        private const string HomeCookingTable1 = "cooking_table";
        private const string HomeCookingTable2 = "cooking_table_2";
        private const string RefugeeCookingTable1 = "refugee_camp_cooking_table";
        private const string RefugeeCookingTable2 = "refugee_camp_cooking_table_2";
        private const string DistillationCube1 = "mf_distcube_2_clay";
        private const string DistillationCube2 = "mf_distcube_2_cuprum";
        private const string AlchemyWorkbench1 = "mf_alchemy_craft_02";
        private const string AlchemyWorkbench2 = "mf_alchemy_craft_03";
        private const string ZombieMineFront = "zombie_mine_fence_front";
        private const string ZombieMineLeftFront = "zombie_mine_fence_left_front";

        private static readonly Regex TierSuffix = new Regex(
            @"^(?<base>.+?)\s+(?:\((?<paren>[IVX]+)\)|(?<plain>[IVX]+))$",
            RegexOptions.CultureInvariant);

        internal static CompactFormatResult Format(
            IList<CraftLocationEntry> source,
            string nativeSeparator,
            string refugeeCampLabel)
        {
            List<CompactDisplayEntry> output =
                new List<CompactDisplayEntry>();

            if (source == null || source.Count == 0)
                return new CompactFormatResult(output, false);

            string separator = NormalizeSeparator(nativeSeparator);
            bool changed = false;
            int index = 0;

            while (index < source.Count)
            {
                int specialEnd;
                string specialText;
                int specialSourceCount;

                if (TryBuildSpecial(
                    source,
                    index,
                    separator,
                    refugeeCampLabel,
                    out specialEnd,
                    out specialText,
                    out specialSourceCount))
                {
                    output.Add(new CompactDisplayEntry(
                        specialText,
                        specialSourceCount));
                    changed = true;
                    index = specialEnd + 1;
                    continue;
                }

                int groupEnd;
                string groupText;

                if (TryBuildGenericGroup(
                    source,
                    index,
                    separator,
                    out groupEnd,
                    out groupText))
                {
                    output.Add(new CompactDisplayEntry(
                        groupText,
                        groupEnd - index + 1));
                    changed = true;
                    index = groupEnd + 1;
                    continue;
                }

                output.Add(new CompactDisplayEntry(
                    source[index].Name,
                    1));
                index++;
            }

            return new CompactFormatResult(output, changed);
        }

        internal static string NormalizeSeparator(string nativeSeparator)
        {
            if (string.IsNullOrEmpty(nativeSeparator))
                return ", ";

            char last = nativeSeparator[nativeSeparator.Length - 1];
            if (char.IsWhiteSpace(last))
                return nativeSeparator;

            if (nativeSeparator == "," || nativeSeparator == ";")
                return nativeSeparator + " ";

            return nativeSeparator;
        }

        private static bool TryBuildSpecial(
            IList<CraftLocationEntry> source,
            int start,
            string separator,
            string refugeeCampLabel,
            out int end,
            out string text,
            out int sourceCount)
        {
            end = start;
            text = null;
            sourceCount = 0;

            if (TryCollapseZombieMineDuplicate(
                source,
                start,
                out end,
                out text))
            {
                sourceCount = end - start + 1;
                return true;
            }

            if (TryBuildExactTierPair(
                source,
                start,
                HomeCookingTable1,
                HomeCookingTable2,
                separator,
                out end,
                out text) ||
                TryBuildExactTierPair(
                    source,
                    start,
                    DistillationCube1,
                    DistillationCube2,
                    separator,
                    out end,
                    out text) ||
                TryBuildExactTierPair(
                    source,
                    start,
                    AlchemyWorkbench1,
                    AlchemyWorkbench2,
                    separator,
                    out end,
                    out text))
            {
                sourceCount = end - start + 1;
                return true;
            }

            if (TryBuildRefugeeCookingTable(
                source,
                start,
                separator,
                refugeeCampLabel,
                out end,
                out text,
                out sourceCount))
            {
                return true;
            }

            return false;
        }

        private static bool TryBuildExactTierPair(
            IList<CraftLocationEntry> source,
            int start,
            string firstId,
            string secondId,
            string separator,
            out int end,
            out string text)
        {
            end = start;
            text = null;

            if (start + 1 >= source.Count ||
                !string.Equals(
                    source[start].Id,
                    firstId,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    source[start + 1].Id,
                    secondId,
                    StringComparison.Ordinal))
            {
                return false;
            }

            string baseName;
            int? ignoredTier;
            ParseVisibleTier(
                source[start].Name,
                out baseName,
                out ignoredTier);

            if (string.IsNullOrEmpty(baseName))
                return false;

            end = start + 1;
            text = baseName + " I" + separator + "II";
            return true;
        }

        private static bool TryBuildRefugeeCookingTable(
            IList<CraftLocationEntry> source,
            int start,
            string separator,
            string refugeeCampLabel,
            out int end,
            out string text,
            out int sourceCount)
        {
            end = start;
            text = null;
            sourceCount = 0;

            if (string.IsNullOrWhiteSpace(refugeeCampLabel))
                return false;

            CraftLocationEntry current = source[start];

            if (string.Equals(
                current.Id,
                RefugeeCookingTable1,
                StringComparison.Ordinal))
            {
                string baseName;
                int? ignoredTier;
                ParseVisibleTier(
                    current.Name,
                    out baseName,
                    out ignoredTier);

                if (string.IsNullOrEmpty(baseName))
                    return false;

                if (start + 1 < source.Count &&
                    string.Equals(
                        source[start + 1].Id,
                        RefugeeCookingTable2,
                        StringComparison.Ordinal))
                {
                    end = start + 1;
                    sourceCount = 2;
                    text = refugeeCampLabel +
                           ": " +
                           baseName +
                           " I" +
                           separator +
                           "II";
                    return true;
                }

                sourceCount = 1;
                text = refugeeCampLabel +
                       ": " +
                       baseName +
                       " I";
                return true;
            }

            if (!string.Equals(
                current.Id,
                RefugeeCookingTable2,
                StringComparison.Ordinal))
            {
                return false;
            }

            string secondBase;
            int? secondTier;
            ParseVisibleTier(
                current.Name,
                out secondBase,
                out secondTier);

            if (string.IsNullOrEmpty(secondBase))
                return false;

            sourceCount = 1;
            text = refugeeCampLabel +
                   ": " +
                   secondBase +
                   " " +
                   (secondTier.HasValue
                       ? ToRoman(secondTier.Value)
                       : "II");
            return true;
        }

        private static bool TryCollapseZombieMineDuplicate(
            IList<CraftLocationEntry> source,
            int start,
            out int end,
            out string text)
        {
            end = start;
            text = null;

            if (start + 1 >= source.Count)
                return false;

            string firstId = source[start].Id;
            string secondId = source[start + 1].Id;

            bool exactPair =
                (string.Equals(
                    firstId,
                    ZombieMineFront,
                    StringComparison.Ordinal) &&
                 string.Equals(
                    secondId,
                    ZombieMineLeftFront,
                    StringComparison.Ordinal)) ||
                (string.Equals(
                    firstId,
                    ZombieMineLeftFront,
                    StringComparison.Ordinal) &&
                 string.Equals(
                    secondId,
                    ZombieMineFront,
                    StringComparison.Ordinal));

            if (!exactPair ||
                !string.Equals(
                    source[start].Name,
                    source[start + 1].Name,
                    StringComparison.Ordinal) ||
                string.IsNullOrEmpty(source[start].Name))
            {
                return false;
            }

            end = start + 1;
            text = source[start].Name;
            return true;
        }

        private static bool TryBuildGenericGroup(
            IList<CraftLocationEntry> source,
            int start,
            string separator,
            out int end,
            out string text)
        {
            end = start;
            text = null;

            if (start + 1 >= source.Count)
                return false;

            string family = GetFamilyKey(source[start].Id);
            if (!string.Equals(
                family,
                GetFamilyKey(source[start + 1].Id),
                StringComparison.Ordinal))
            {
                return false;
            }

            string baseName;
            int? firstTier;
            ParseVisibleTier(
                source[start].Name,
                out baseName,
                out firstTier);

            string nextBase;
            int? nextTier;
            ParseVisibleTier(
                source[start + 1].Name,
                out nextBase,
                out nextTier);

            if (!string.Equals(
                baseName,
                nextBase,
                StringComparison.Ordinal))
            {
                return false;
            }

            int expectedCurrentTier;
            if (!firstTier.HasValue)
            {
                if (nextTier != 2)
                    return false;

                expectedCurrentTier = 1;
            }
            else
            {
                if (!nextTier.HasValue ||
                    nextTier.Value != firstTier.Value + 1)
                {
                    return false;
                }

                expectedCurrentTier = firstTier.Value;
            }

            List<int> tiers = new List<int>
            {
                expectedCurrentTier,
                nextTier.Value
            };

            end = start + 1;
            int expectedNext = nextTier.Value + 1;

            while (end + 1 < source.Count)
            {
                CraftLocationEntry candidate = source[end + 1];

                if (!string.Equals(
                    family,
                    GetFamilyKey(candidate.Id),
                    StringComparison.Ordinal))
                {
                    break;
                }

                string candidateBase;
                int? candidateTier;
                ParseVisibleTier(
                    candidate.Name,
                    out candidateBase,
                    out candidateTier);

                if (!string.Equals(
                    baseName,
                    candidateBase,
                    StringComparison.Ordinal) ||
                    !candidateTier.HasValue ||
                    candidateTier.Value != expectedNext)
                {
                    break;
                }

                tiers.Add(candidateTier.Value);
                end++;
                expectedNext++;
            }

            List<string> roman = new List<string>();
            for (int i = 0; i < tiers.Count; i++)
            {
                string tier = ToRoman(tiers[i]);
                if (tier == null)
                {
                    end = start;
                    text = null;
                    return false;
                }

                roman.Add(tier);
            }

            text = baseName + " " + string.Join(
                separator,
                roman.ToArray());
            return true;
        }

        private static string GetFamilyKey(string id)
        {
            if (string.IsNullOrEmpty(id))
                return string.Empty;

            int underscore = id.LastIndexOf('_');
            if (underscore <= 0 || underscore == id.Length - 1)
                return id;

            for (int i = underscore + 1; i < id.Length; i++)
            {
                if (!char.IsDigit(id[i]))
                    return id;
            }

            return id.Substring(0, underscore);
        }

        private static void ParseVisibleTier(
            string value,
            out string baseName,
            out int? tier)
        {
            string trimmed = (value ?? string.Empty).Trim();
            Match match = TierSuffix.Match(trimmed);

            if (!match.Success)
            {
                baseName = trimmed;
                tier = null;
                return;
            }

            string roman = match.Groups["paren"].Success
                ? match.Groups["paren"].Value
                : match.Groups["plain"].Value;

            int parsed;
            if (!TryParseRoman(roman, out parsed))
            {
                baseName = trimmed;
                tier = null;
                return;
            }

            baseName = match.Groups["base"].Value.TrimEnd();
            tier = parsed;
        }

        private static bool TryParseRoman(
            string value,
            out int tier)
        {
            switch (value)
            {
                case "I":
                    tier = 1;
                    return true;
                case "II":
                    tier = 2;
                    return true;
                case "III":
                    tier = 3;
                    return true;
                case "IV":
                    tier = 4;
                    return true;
                case "V":
                    tier = 5;
                    return true;
                default:
                    tier = 0;
                    return false;
            }
        }

        private static string ToRoman(int value)
        {
            switch (value)
            {
                case 1:
                    return "I";
                case 2:
                    return "II";
                case 3:
                    return "III";
                case 4:
                    return "IV";
                case 5:
                    return "V";
                default:
                    return null;
            }
        }
    }
}
