using System.Runtime.CompilerServices;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Extensions
{
    public partial class OpenSimplexNoise
    {
        private const double stretch_2d = -0.211324865405187;    //(1/Math.sqrt(2+1)-1)/2;
        private const double squish_2d = 0.366025403784439;      //(Math.sqrt(2+1)-1)/2;
        private const double norm_2d = 1.0 / 47.0;

        private byte[] perm;
        private byte[] perm2D;
        private byte[] perm3D;
        private byte[] perm4D;

        private static double[] gradients2D = new double[]
        {
             5, 2, 2, 5,
            -5, 2, -2, 5,
             5, -2, 2, -5,
            -5, -2, -2, -5,
        };

        private readonly Contribution[] lookup2D = new Contribution[64];

        public OpenSimplexNoise(long seed = 0)
        {
            var base2D = new int[][]
            {
                new int[] { 1, 1, 0, 1, 0, 1, 0, 0, 0 },
                new int[] { 1, 1, 0, 1, 0, 1, 2, 1, 1 }
            };
            var p2D = new int[] { 0, 0, 1, -1, 0, 0, -1, 1, 0, 2, 1, 1, 1, 2, 2, 0, 1, 2, 0, 2, 1, 0, 0, 0 };
            var lookupPairs2D = new int[] { 0, 1, 1, 0, 4, 1, 17, 0, 20, 2, 21, 2, 22, 5, 23, 5, 26, 4, 39, 3, 42, 4, 43, 3 };

            var contributions2D = new Contribution[p2D.Length / 4];
            for (int i = 0; i < p2D.Length; i += 4)
            {
                var baseSet = base2D[p2D[i]];
                Contribution? previous = null, current = null;
                for (int k = 0; k < baseSet.Length; k += 3)
                {
                    current = new Contribution(baseSet[k], baseSet[k + 1], baseSet[k + 2]);
                    if (previous == null)
                    {
                        contributions2D[i / 4] = current;
                    }
                    else
                    {
                        previous.Next = current;
                    }
                    previous = current;
                }
                current!.Next = new Contribution(p2D[i + 1], p2D[i + 2], p2D[i + 3]);
            }

            for (var i = 0; i < lookupPairs2D.Length; i += 2)
                lookup2D[lookupPairs2D[i]] = contributions2D[lookupPairs2D[i + 1]];

            perm = new byte[256];
            perm2D = new byte[256];
            perm3D = new byte[256];
            perm4D = new byte[256];
            var source = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                source[i] = (byte)i;
            }
            seed = seed * 6364136223846793005L + 1442695040888963407L;
            seed = seed * 6364136223846793005L + 1442695040888963407L;
            seed = seed * 6364136223846793005L + 1442695040888963407L;
            for (int i = 255; i >= 0; i--)
            {
                seed = seed * 6364136223846793005L + 1442695040888963407L;
                int r = (int)((seed + 31) % (i + 1));
                if (r < 0)
                {
                    r += (i + 1);
                }
                perm[i] = source[r];
                perm2D[i] = (byte)(perm[i] & 0x0E);
                perm3D[i] = (byte)((perm[i] % 24) * 3);
                perm4D[i] = (byte)(perm[i] & 0xFC);
                source[r] = source[i];
            }
        }

        public double Evaluate(double x, double y)
        {
            var stretchOffset = (x + y) * stretch_2d;
            var xs = x + stretchOffset;
            var ys = y + stretchOffset;

            var xsb = fastFloor(xs);
            var ysb = fastFloor(ys);

            var squishOffset = (xsb + ysb) * squish_2d;
            var dx0 = x - (xsb + squishOffset);
            var dy0 = y - (ysb + squishOffset);

            var xins = xs - xsb;
            var yins = ys - ysb;

            var inSum = xins + yins;

            var hash =
               (int)(xins - yins + 1) |
               (int)(inSum) << 1 |
               (int)(inSum + yins) << 2 |
               (int)(inSum + xins) << 4;

            var c = lookup2D[hash];

            var value = 0.0;
            while (c != null)
            {
                var dx = dx0 + c.Dx;
                var dy = dy0 + c.Dy;
                var attn = 2 - dx * dx - dy * dy;
                if (attn > 0)
                {
                    var px = xsb + c.Xsb;
                    var py = ysb + c.Ysb;

                    var i = perm2D[(perm[px & 0xFF] + py) & 0xFF];
                    var valuePart = gradients2D[i] * dx + gradients2D[i + 1] * dy;

                    attn *= attn;
                    value += attn * attn * valuePart;
                }
                c = c.Next;
            }
            return value * norm_2d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int fastFloor(double x)
        {
            var xi = (int)x;
            return x < xi ? xi - 1 : xi;
        }

        private partial class Contribution
        {
            public double Dx, Dy;
            public int Xsb, Ysb;
            public Contribution Next;

            public Contribution(double multiplier, int xsb, int ysb)
            {
                Dx = -xsb - multiplier * squish_2d;
                Dy = -ysb - multiplier * squish_2d;
                Xsb = xsb;
                Ysb = ysb;
            }
        }
    }
}
