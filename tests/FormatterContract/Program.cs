// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections.Generic;
using CompactCraftingTooltips;

internal static class Program
{
    private static int _checks;

    private static void Main()
    {
        AssertFormat(
            "farm full",
            new[]
            {
                E("zombie_garden_desk_0", "Зомби-ферма"),
                E("zombie_garden_desk_1", "Зомби-ферма II"),
                E("zombie_garden_desk_2", "Зомби-ферма III"),
                E("refugee_camp_garden_bed_1", "Грядка"),
                E("refugee_camp_garden_bed_2", "Грядка II"),
                E("refugee_camp_garden_bed_3", "Грядка III")
            },
            true,
            "Зомби-ферма I, II, III",
            "Грядка I, II, III");

        AssertFormat(
            "farm partial",
            new[]
            {
                E("zombie_garden_desk_1", "Зомби-ферма II"),
                E("zombie_garden_desk_2", "Зомби-ферма III"),
                E("refugee_camp_garden_bed_2", "Грядка II"),
                E("refugee_camp_garden_bed_3", "Грядка III")
            },
            true,
            "Зомби-ферма II, III",
            "Грядка II, III");

        AssertFormat(
            "renamed anvil member stays distinct",
            new[]
            {
                E("mf_anvil_1", "Деревянная наковальня"),
                E("mf_anvil_2", "Наковальня"),
                E("mf_anvil_3", "Наковальня II")
            },
            true,
            "Деревянная наковальня",
            "Наковальня I, II");

        AssertFormat(
            "alchemy renamed member stays distinct",
            new[]
            {
                E("mf_alchemy_craft_01", "Алхимический верстак (I)"),
                E("mf_alchemy_craft_02", "Алхимический стол"),
                E("mf_alchemy_craft_03", "Алхимический стол II")
            },
            true,
            "Алхимический верстак (I)",
            "Алхимический стол I, II");

        AssertFormat(
            "reversed vanilla order is preserved",
            new[]
            {
                E("mf_workbench_2", "Столярный верстак II"),
                E("mf_workbench_1", "Столярный верстак")
            },
            false,
            "Столярный верстак II",
            "Столярный верстак");

        AssertFormat(
            "different canonical families do not merge",
            new[]
            {
                E("cooking_table", "Кухонный стол"),
                E("refugee_camp_cooking_table_2", "Кухонный стол II")
            },
            false,
            "Кухонный стол",
            "Кухонный стол II");

        AssertFormat(
            "renamed upgrade is untouched",
            new[]
            {
                E("cooking_table", "Кухонный стол"),
                E("cooking_table_2", "Обновленный кухонный стол")
            },
            false,
            "Кухонный стол",
            "Обновленный кухонный стол");

        AssertFormat(
            "explicit I II III",
            new[]
            {
                E("soul_extractor", "Извлекатель души I"),
                E("soul_extractor_2", "Извлекатель души II"),
                E("soul_extractor_3", "Извлекатель души III")
            },
            true,
            "Извлекатель души I, II, III");

        AssertFormat(
            "safe false negative without manual mapping",
            new[]
            {
                E("mf_distcube_2_clay", "Дистилляционный куб"),
                E("mf_distcube_2_cuprum", "Дистилляционный куб II")
            },
            false,
            "Дистилляционный куб",
            "Дистилляционный куб II");

        Assert(
            CraftingLocationFormatter.NormalizeSeparator(",") == ", ",
            "ASCII comma gets readable spacing");

        Assert(
            CraftingLocationFormatter.NormalizeSeparator("，") == "，",
            "non-ASCII localized separator is preserved");

        Console.WriteLine(
            "FORMATTER_CONTRACT_OK checks=" + _checks);
    }

    private static CraftLocationEntry E(
        string id,
        string name)
    {
        return new CraftLocationEntry(id, name);
    }

    private static void AssertFormat(
        string scenario,
        CraftLocationEntry[] source,
        bool expectedChanged,
        params string[] expected)
    {
        CompactFormatResult actual =
            CraftingLocationFormatter.Format(
                new List<CraftLocationEntry>(source),
                ",");

        Assert(
            actual.Changed == expectedChanged,
            scenario + " changed");

        Assert(
            actual.Entries.Count == expected.Length,
            scenario + " entry count");

        for (int i = 0; i < expected.Length; i++)
        {
            Assert(
                actual.Entries[i].Text == expected[i],
                scenario + " entry " + i);
        }
    }

    private static void Assert(
        bool condition,
        string name)
    {
        _checks++;

        if (!condition)
            throw new InvalidOperationException(
                "FORMATTER_CONTRACT_FAIL " + name);
    }
}
