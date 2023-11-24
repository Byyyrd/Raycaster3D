using Silk.NET.Maths;
using Silk.NET.OpenGL;


namespace Raycaster3D
{
    internal class Transformation
    {
        private GL _gl;
        private uint _program;
        public float Rotation;
        public Vector3D<float> Scale;
        public Vector3D<float> Position;
        private Matrix4X4<float> _transform;
        public Transformation(GL _pGl,uint _pProgram)
        {
            _gl = _pGl;
            _program = _pProgram;
            Scale = Vector3D<float>.One;
            Position = Vector3D<float>.Zero;
            Rotation = 0;
        }
        public unsafe void Use()
        {
            _transform = Matrix4X4<float>.Identity;
            _transform *= Matrix4X4.CreateTranslation(Position);
            _transform *= Matrix4X4.CreateScale(Scale);
            _transform *= Matrix4X4.CreateRotationZ(Rotation, Position * Scale);

            int transformLoc = _gl.GetUniformLocation(_program, "transform");
            fixed (Matrix4X4<float>* mat = &_transform)
                _gl.UniformMatrix4(transformLoc, 1, false, (float*)mat);
        }
        public static unsafe void UseNone(GL _gl,uint _program)
        {
            Matrix4X4<float> transform = Matrix4X4<float>.Identity;
            
            int transformLoc = _gl.GetUniformLocation(_program, "transform");
            _gl.UniformMatrix4(transformLoc, 1, false, (float*)&transform);
        }
        public static Matrix4X4<float> CreateMatrix(Vector3D<float> Position, Vector3D<float> Scale)
        {
            Matrix4X4<float> transform = Matrix4X4<float>.Identity;
            transform *= Matrix4X4.CreateScale(Scale);
            transform *= Matrix4X4.CreateTranslation(Position);
            return transform;
        } 
    }
    
}
