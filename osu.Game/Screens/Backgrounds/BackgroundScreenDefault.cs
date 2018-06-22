// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Threading;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundScreenDefault : BackgroundScreen
    {
        private int currentDisplay;
        private const int background_count = 5;

        private string backgroundName => $@"Menu/menu-background-{currentDisplay % background_count + 1}";

        private Background current;

        [BackgroundDependencyLoader]
        private void load()
        {
            display(new Background(backgroundName));
        }

        private void display(Background newBackground)
        {
            current?.FadeOut(800, Easing.InOutSine);
            current?.Expire();

            Add(current = newBackground);
            currentDisplay++;
        }

        private ScheduledDelegate nextTask;

        public void Next()
        {
            nextTask?.Cancel();
            nextTask = Scheduler.AddDelayed(() =>
            {
                LoadComponentAsync(new Background(backgroundName) { Depth = currentDisplay }, display);
            }, 100);
        }
    }
}
