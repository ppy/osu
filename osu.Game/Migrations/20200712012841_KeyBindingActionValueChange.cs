// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets;
using Microsoft.EntityFrameworkCore.Migrations;

namespace osu.Game.Migrations
{
    public partial class KeyBindingActionValueChange : Migration
    {
        private readonly RulesetStore rulesetStore;

        // OR

        private readonly KeybindActionLookup actionLookup;

        public KeyBindingActionValueChange(DatabaseContextFactory databaseContextFactory, Storage storage)
        {
            rulesetStore = new RulesetStore(databaseContextFactory, storage);
            // OR
            actionLookup = new KeybindActionLookup();
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (RulesetInfo rulesetInfo in rulesetStore.AvailableRulesets)
            {
                int? rulesetId = rulesetInfo.ID;

                Ruleset ruleset = rulesetInfo.CreateInstance();

                foreach (int variant in ruleset.AvailableVariants)
                {
                    foreach (var defaultKeyBinding in ruleset.GetDefaultKeyBindings(variant))
                    {
                        string stringValue = defaultKeyBinding.Action.ToString();
                        int intValue = (int)defaultKeyBinding.Action;
                        migrationBuilder.Sql($"UPDATE KeyBinding SET Action={stringValue} WHERE Action={intValue} AND RulesetID={rulesetId}");
                    }
                }
            }

            // OR

            foreach (var actionEnum in actionLookup.Lookup())
            {
                foreach (var enumValue in actionEnum.Value.GetEnumValues())
                {
                    migrationBuilder.Sql($"UPDATE KeyBinding SET Action={enumValue} WHERE Action={(int)enumValue} AND RulesetID={actionEnum.Key}");
                }

                // Probably is missing mania dual stage migrations
            }
        }
    }

    internal class KeybindActionLookup
    {
        private readonly IDictionary<int?, Type> lookup = new Dictionary<int?, Type>
        {
            { null!, typeof(GlobalAction) },
            { 0, typeof(OsuAction) },
            { 1, typeof(TaikoAction) },
            { 2, typeof(CatchAction) },
            { 3, typeof(ManiaAction) }
        };

        public string Lookup(int? rulesetId, int action) => lookup[rulesetId].GetEnumValues().GetValue(action).ToString();

        public IDictionary<int?, Type> Lookup() => lookup;
    }

    public enum OsuAction
    {
        LeftButton,
        RightButton
    }

    public enum CatchAction
    {
        MoveLeft,
        MoveRight,
        Dash,
    }

    public enum TaikoAction
    {
        LeftRim,
        LeftCentre,
        RightCentre,
        RightRim
    }

    public enum ManiaAction
    {
        Special1 = 1,
        Special2,
        Key1 = 10,
        Key2,
        Key3,
        Key4,
        Key5,
        Key6,
        Key7,
        Key8,
        Key9,
        Key10,
        Key11,
        Key12,
        Key13,
        Key14,
        Key15,
        Key16,
        Key17,
        Key18,
        Key19,
        Key20
    }
}
