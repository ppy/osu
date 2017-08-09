// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Rulesets;
using OpenTK.Input;
using SQLite.Net;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace osu.Game.Input
{
    public class ActionMappingInputManager<T> : PassThroughInputManager
        where T : struct
    {
        private readonly RulesetInfo ruleset;

        private readonly int? variant;

        protected ActionMappingInputManager(RulesetInfo ruleset = null, int? variant = null)
        {
            this.ruleset = ruleset;
            this.variant = variant;
        }

        protected IDictionary<Key, T> Mappings { get; set; }

        [BackgroundDependencyLoader]
        private void load(BindingStore bindings)
        {
            var rulesetId = ruleset?.ID;
            foreach (var b in bindings.Query<Binding>(b => b.RulesetID == rulesetId && b.Variant == variant))
                Mappings[b.Key] = (T)(object)b.Action;
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            mapKey(state, args.Key);
            return base.OnKeyDown(state, args);
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            mapKey(state, args.Key);
            return base.OnKeyUp(state, args);
        }

        private void mapKey(InputState state, Key key)
        {
            T mappedData;
            if (Mappings.TryGetValue(key, out mappedData))
                state.Data = mappedData;
        }

        private T parseStringRepresentation(string str)
        {
            T res;

            if (Enum.TryParse(str, out res))
                return res;

            return default(T);
        }
    }

    public class Binding
    {
        [ForeignKey(typeof(RulesetInfo))]
        public int? RulesetID { get; set; }

        [Indexed]
        public int? Variant { get; set; }

        public Key Key { get; set; }

        public int Action { get; set; }
    }

    public class BindingStore : DatabaseBackedStore
    {
        public BindingStore(SQLiteConnection connection, Storage storage = null)
            : base(connection, storage)
        {
        }

        protected override void Prepare(bool reset = false)
        {
            Connection.CreateTable<Binding>();
        }

        protected override Type[] ValidTypes => new[]
        {
            typeof(Binding)
        };

    }
}
