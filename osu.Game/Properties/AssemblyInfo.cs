// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

extern alias IOS;

using System.Runtime.CompilerServices;
using IOS::Foundation;

[assembly: Preserve]
[assembly: InternalsVisibleTo("osu.Game.Tests")]
[assembly: InternalsVisibleTo("osu.Game.Tests.Dynamic")]
