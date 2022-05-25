// 2022-05-24 BeXide
// by Y.Hayashi

using System;

namespace SharpNoise.Modules
{
    /// <summary>
    /// Noise module that blend seams
    /// テクスチャの継ぎ目を目立たなくする
    /// </summary>
    /// <remarks>
    /// This noise module requires one source module.
    /// </remarks>
    [Serializable]
    public class Seamless : Module
    {
        /// <summary>
        /// Gets or sets the first source module
        /// </summary>
        public Module Source0
        {
            get { return SourceModules[0]; }
            set { SourceModules[0] = value; }
        }

        /// <summary>
        /// 継ぎ目をブレンドする厚さ
        /// </summary>
        public double SeamThickness => 0.2;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Seamless()
            : base(1)
        {
        }

        /// <summary>
        /// See the documentation on the base class.
        /// <seealso cref="Module"/>
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <returns>Returns the computed value</returns>
        public override double GetValue(double x, double y, double z)
        {
            x = Repeat(x);
            y = Repeat(y);
            z = Repeat(z);

            double value = GetSeamlessXYZ(x, y, z);
            return value;
        }

        private double GetSeamlessX(double x, double y, double z)
        {
            double mult = 1.0 / (1.0 + SeamThickness * 2.0);
            double nx = x * mult;
            double value = SourceModules[0].GetValue(Repeat(nx), y, z);

            if (x < -0.5 + SeamThickness)
            {
                double nextX = (x + 1.0) * mult;
                double t = (x + 0.5 + SeamThickness) / (SeamThickness * 2.0);
                value = Lerp(SourceModules[0].GetValue(Repeat(nextX), y, z), value, t);
            }
            else if (x > 0.5 - SeamThickness)
            {
                double nextX = (x - 1.0) * mult;
                double t = 1.0 - (x - (0.5 - SeamThickness)) / (SeamThickness * 2.0);
                value = Lerp(SourceModules[0].GetValue(Repeat(nextX), y, z), value, t);
            }

            return value;
        }

        private double GetSeamlessXY(double x, double y, double z)
        {
            double mult = 1.0 / (1.0 + SeamThickness * 2.0);
            double ny = y * mult;
            double value = GetSeamlessX(x, ny, z);

            if (y < -0.5 + SeamThickness)
            {
                double nextY = (y + 1.0) * mult;
                double t = (y + 0.5 + SeamThickness) / (SeamThickness * 2.0);
                value = Lerp(GetSeamlessX(x, Repeat(nextY), z), value, t);
            }
            else if (y > 0.5 - SeamThickness)
            {
                double nextY = (y - 1.0) * mult;
                double t = 1.0 - (y - (0.5 - SeamThickness)) / (SeamThickness * 2.0);
                value = Lerp(GetSeamlessX(x, Repeat(nextY), z), value, t);
            }

            return value;
        }

        private double GetSeamlessXYZ(double x, double y, double z)
        {
            double mult = 1.0 / (1.0 + SeamThickness * 2.0);
            double nz = z * mult;
            double value = GetSeamlessXY(x, y, nz);

            if (z < -0.5 + SeamThickness)
            {
                double nextZ = (z + 1.0) * mult;
                double t = (z + 0.5 + SeamThickness) / (SeamThickness * 2.0);
                value = Lerp(GetSeamlessXY(x, y, Repeat(nextZ)), value, t);
            }
            else if (z > 0.5 - SeamThickness)
            {
                double nextZ = (z - 1.0) * mult;
                double t = 1.0 - (z - (0.5 - SeamThickness)) / (SeamThickness * 2.0);
                value = Lerp(GetSeamlessXY(x, y, Repeat(nextZ)), value, t);
            }

            return value;
        }

        private double Repeat(double v)
        {
            return (v + 0.5) % 1.0 - 0.5;
        }

        private double Lerp(double a, double b, double t)
        {
            return a * (1.0 - t) + b * t;
        }
    }
}
