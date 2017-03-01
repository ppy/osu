// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Diagnostics;
using System.Security;
using osu.Framework.Extensions;

namespace osu.Game.Online.API
{
    internal class SecurePassword
    {
        private readonly SecureString storage = new SecureString();
        private readonly Representation representation;

        //todo: move this to a central constants file.
        private const string password_entropy = @"cu24180ncjeiu0ci1nwui";

        public SecurePassword(string input, bool encrypted = false)
        {
            //if (encrypted)
            //{
            //    string rep;
            //    input = DPAPI.Decrypt(input, password_entropy, out rep);
            //    Enum.TryParse(rep, out representation);
            //}
            //else
            {
                representation = Representation.Raw;
            }

            foreach (char c in input)
                storage.AppendChar(c);
            storage.MakeReadOnly();
        }

        internal string Get(Representation request = Representation.Raw)
        {
            Debug.Assert(representation == request);

            switch (request)
            {
                default:
                    return storage.UnsecureRepresentation();
                //case Representation.Encrypted:
                //    return DPAPI.Encrypt(DPAPI.KeyType.UserKey, storage.UnsecureRepresentation(), password_entropy, representation.ToString());
            }
        }
    }

    enum Representation
    {
        Raw,
        Encrypted
    }
}
