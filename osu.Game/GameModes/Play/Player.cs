//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Objects.Osu;
using osu.Game.GameModes.Play.Catch;
using osu.Game.GameModes.Play.Mania;
using osu.Game.GameModes.Play.Osu;
using osu.Game.GameModes.Play.Taiko;
using OpenTK;

namespace osu.Game.GameModes.Play
{
    class Player : GameModeWhiteBox
    {
        protected override BackgroundMode CreateBackground() => new BackgroundModeCustom(@"Backgrounds/bg4");

        protected override IEnumerable<Type> PossibleChildren => new[] {
                typeof(Results)
        };

        public override void Load()
        {
            base.Load();

            List<HitObject> objects = new List<HitObject>();

            double time = Time + 1000;
            for (int i = 0; i < 100; i++)
            {
                objects.Add(new Circle()
                {
                    StartTime = time,
                    Position = new Vector2(RNG.Next(0, 512), RNG.Next(0, 384))
                });

                time += RNG.Next(50, 500);
            }

            Beatmap beatmap = new Beatmap
            {
                HitObjects = objects
            };

            OsuGame osu = Game as OsuGame;

            switch (osu.PlayMode.Value)
            {
                case PlayMode.Osu:
                    Add(new OsuHitRenderer
                    {
                        Objects = beatmap.HitObjects,
                        Scale = new Vector2(0.8f),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    });
                    break;
                case PlayMode.Taiko:
                    Add(new TaikoHitRenderer
                    {
                        Objects = beatmap.HitObjects,
                        Scale = new Vector2(0.8f),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    });
                    break;
                case PlayMode.Catch:
                    Add(new CatchHitRenderer
                    {
                        Objects = beatmap.HitObjects,
                        Scale = new Vector2(0.8f),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    });
                    break;
                case PlayMode.Mania:
                    Add(new ManiaHitRenderer
                    {
                        Objects = beatmap.HitObjects,
                        Scale = new Vector2(0.8f),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    });
                    break;
            }
        }
    }
}