using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Example.src.controller.util
{
    class MathUtil
    {
        public static Vector4 Transform(Vector4 vector, Matrix4x4 matrix)
        {
            return
                new Vector4(
                    (float)
                    (vector.X * (double)matrix.M11 +
                     vector.Y * (double)matrix.M21 +
                     vector.Z * (double)matrix.M31 +
                     vector.W * (double)matrix.M41),
                    (float)
                    (vector.X * (double)matrix.M12 +
                     vector.Y * (double)matrix.M22 +
                     vector.Z * (double)matrix.M32 +
                     vector.W * (double)matrix.M42),
                    (float)
                    (vector.X * (double)matrix.M13 +
                     vector.Y * (double)matrix.M23 +
                     vector.Z * (double)matrix.M33 +
                     vector.W * (double)matrix.M43),
                    (float)
                    (vector.X * (double)matrix.M14 +
                     vector.Y * (double)matrix.M24 +
                     vector.Z * (double)matrix.M34 +
                     vector.W * (double)matrix.M44));
        }
    }
}
