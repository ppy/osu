// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using Newtonsoft.Json;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Screens.Ladder.Components;

namespace osu.Game.Tournament.Tests
{
    public abstract class LadderTestCase : OsuTestCase
    {
        protected LadderInfo Ladder;

        protected LadderTestCase()
        {
            Ladder = File.Exists(@"bracket.json") ? JsonConvert.DeserializeObject<LadderInfo>(File.ReadAllText(@"bracket.json")) : new LadderInfo();

            Add(new OsuButton
            {
                Text = "Save Changes",
                Width = 140,
                Height = 50,
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Padding = new MarginPadding(10),
                Action = SaveChanges,
            });
        }

        protected virtual void SaveChanges()
        {
            File.WriteAllText(@"bracket.json", JsonConvert.SerializeObject(Ladder,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                }));
        }
    }
}
