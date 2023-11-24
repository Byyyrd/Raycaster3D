using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycaster3D
{
    internal class TextureTransformation
    {
        private GL _gl;
        private uint _program;
        public Vector2D<float> Position;
        private Vector2D<float> _transform;
        public TextureTransformation(GL _pGl, uint _pProgram)
        {
            _gl = _pGl;
            _program = _pProgram;
            Position = Vector2D<float>.Zero;
        }
        public unsafe void Use()
        {
            _transform = Position;
            int transformLoc = _gl.GetUniformLocation(_program, "textureTransform");
            fixed (Vector2D<float>* vec = &_transform)
                _gl.Uniform2(transformLoc,1, (float*)vec);
        }
        public static unsafe void UseNone(GL _gl,uint _program)
        {
            Vector2D<float> _nullTransform = Vector2D<float>.Zero;
            int transformLoc = _gl.GetUniformLocation(_program, "textureTransform");
            _gl.Uniform2(transformLoc, 1, (float*)&_nullTransform);
        }
    }
}
