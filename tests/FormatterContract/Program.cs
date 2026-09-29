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
            "Лагерь беженцев",
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
            "Лагерь беженцев",
            "Зомби-ферма II, III",
            "Грядка II, III");

        AssertFormat(
            "wooden anvil stays distinct",
            new[]
            {
                E("mf_anvil_1", "Деревянная наковальня"),
                E("mf_anvil_2", "Наковальня"),
                E("mf_anvil_3", "Наковальня II")
            },
            true,
            "Лагерь беженцев",
            "Деревянная наковальня",
            "Наковальня I, II");

        AssertFormat(
            "modern alchemy workbenches compact",
            new[]
            {
                E("mf_alchemy_craft_01", "Алхимический верстак (I)"),
                E("mf_alchemy_craft_02", "Алхимический стол"),
                E("mf_alchemy_craft_03", "Алхимический стол II")
            },
            true,
            "Лагерь беженцев",
            "Алхимический верстак (I)",
            "Алхимический стол I, II");

        AssertFormat(
            "home cooking table pair",
            new[]
            {
                E("cooking_table", "Кухонный стол"),
                E("cooking_table_2", "Обновленный кухонный стол")
            },
            true,
            "Лагерь беженцев",
            "Кухонный стол I, II");

        AssertFormat(
            "home refugee and professional kitchen stay distinct",
            new[]
            {
                E("cooking_table", "Cooking table"),
                E("cooking_table_2", "Updated cooking table"),
                E("refugee_camp_cooking_table", "Cooking table"),
                E("refugee_camp_cooking_table_2", "Cooking table II"),
                E("tavern_kitchen", "Professional kitchen")
            },
            true,
            "Refugee camp",
            "Cooking table I, II",
            "Refugee camp: Cooking table I, II",
            "Professional kitchen");

        AssertFormat(
            "refugee pair gets location qualifier",
            new[]
            {
                E("refugee_camp_cooking_table", "Кухонный стол"),
                E("refugee_camp_cooking_table_2", "Кухонный стол II")
            },
            true,
            "Лагерь беженцев",
            "Лагерь беженцев: Кухонный стол I, II");

        AssertFormat(
            "refugee tier two alone gets location qualifier",
            new[]
            {
                E("refugee_camp_cooking_table_2", "Cooking table II")
            },
            true,
            "Refugee camp",
            "Refugee camp: Cooking table II");

        AssertFormat(
            "distillation cube exact pair",
            new[]
            {
                E("mf_distcube_2_clay", "Дистилляционный куб"),
                E("mf_distcube_2_cuprum", "Дистилляционный куб II")
            },
            true,
            "Лагерь беженцев",
            "Дистилляционный куб I, II");

        AssertFormat(
            "zombie mine duplicate collapses without fake tiers",
            new[]
            {
                E("zombie_mine_fence_front", "Зомби-шахта"),
                E("zombie_mine_fence_left_front", "Зомби-шахта"),
                E("steep_stone", "Залежи камня")
            },
            true,
            "Лагерь беженцев",
            "Зомби-шахта",
            "Залежи камня");

        AssertFormat(
            "unrelated same-name stations are not globally deduped",
            new[]
            {
                E("unrelated_a", "Same name"),
                E("unrelated_b", "Same name")
            },
            false,
            "Refugee camp",
            "Same name",
            "Same name");

        AssertFormat(
            "reversed generic order is preserved",
            new[]
            {
                E("mf_workbench_2", "Столярный верстак II"),
                E("mf_workbench_1", "Столярный верстак")
            },
            false,
            "Лагерь беженцев",
            "Столярный верстак II",
            "Столярный верстак");

        AssertFormat(
            "professional oven is not a home oven tier",
            new[]
            {
                E("oven", "Oven"),
                E("tavern_oven", "Professional oven")
            },
            false,
            "Refugee camp",
            "Oven",
            "Professional oven");

        AssertFormat(
            "explicit I II III",
            new[]
            {
                E("soul_extractor", "Извлекатель души I"),
                E("soul_extractor_2", "Извлекатель души II"),
                E("soul_extractor_3", "Извлекатель души III")
            },
            true,
            "Лагерь беженцев",
            "Извлекатель души I, II, III");

        Assert(
            CraftingLocationFormatter.NormalizeSeparator(",") == ", ",
            "ASCII comma gets readable spacing");

        Assert(
            CraftingLocationFormatter.NormalizeSeparator("，") == "，",
            "non-ASCII localized separator is preserved");

        Assert(
            LocationQualifierLocalization.RefugeeCamp("en") ==
                "Refugee camp",
            "English refugee camp localization");

        Assert(
            LocationQualifierLocalization.RefugeeCamp("ru") ==
                "Лагерь беженцев",
            "Russian refugee camp localization");

        Assert(
            LocationQualifierLocalization.RefugeeCamp("de") ==
                "Flüchtlingslager",
            "German refugee camp localization");

        Assert(
            LocationQualifierLocalization.RefugeeCamp("es") ==
                "Campamento de refugiados",
            "Spanish refugee camp localization");

        Assert(
            LocationQualifierLocalization.RefugeeCamp("fr") ==
                "Camp de réfugiés",
            "French refugee camp localization");

        Assert(
            LocationQualifierLocalization.RefugeeCamp("pt-BR") ==
                "Acampamento de refugiados",
            "Brazilian Portuguese refugee camp localization");

        Assert(
            LocationQualifierLocalization.RefugeeCamp("pl") ==
                "Obóz uchodźców",
            "Polish refugee camp localization");

        Assert(
            LocationQualifierLocalization.RefugeeCamp("zh-CN") ==
                "难民营",
            "Simplified Chinese refugee camp localization");

        Assert(
            LocationQualifierLocalization.RefugeeCamp("ko") ==
                "난민캠프",
            "Korean refugee camp localization");

        Assert(
            LocationQualifierLocalization.RefugeeCamp("ja") ==
                "難民の野営地",
            "Japanese refugee camp localization");

        Assert(
            LocationQualifierLocalization.RefugeeCamp("it") ==
                "Campo profughi",
            "Italian refugee camp localization");

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
        string refugeeCampLabel,
        params string[] expected)
    {
        CompactFormatResult actual =
            CraftingLocationFormatter.Format(
                new List<CraftLocationEntry>(source),
                ",",
                refugeeCampLabel);

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
