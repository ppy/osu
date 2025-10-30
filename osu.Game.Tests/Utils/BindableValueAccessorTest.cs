// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Game.Utils;

namespace osu.Game.Tests.Utils
{
    [TestFixture]
    public class BindableValueAccessorTest
    {
        [Test]
        public void GetValue()
        {
            const int value = 1337;

            BindableInt bindable = new BindableInt(value);
            Assert.That(BindableValueAccessor.GetValue(bindable), Is.EqualTo(value));
        }

        [Test]
        public void SetValue()
        {
            const int value = 1337;

            BindableInt bindable = new BindableInt();
            BindableValueAccessor.SetValue(bindable, value);

            Assert.That(bindable.Value, Is.EqualTo(value));
        }

        [Test]
        public void GetInvalidBindable()
        {
            BindableList<object> list = new BindableList<object>();
            Assert.That(BindableValueAccessor.GetValue(list), Is.EqualTo(list));
        }

        [Test]
        public void SetInvalidBindable()
        {
            const int value = 1337;

            BindableList<int> list = new BindableList<int> { value };
            BindableValueAccessor.SetValue(list, 2);

            Assert.That(list, Has.Exactly(1).Items);
            Assert.That(list[0], Is.EqualTo(value));
        }
    }
}
