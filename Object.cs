using Raycaster3D;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;

namespace Raycaster3D
{
    internal class Object
    {
        // TODO: Make Object display at centere not bottom left
        public Transformation Transformation;
        protected Texture? _texture;
        protected GL _gl;
        protected uint _program;
        protected uint _stride;
        protected VertexArrayObject<float, uint> _vao;
        protected BufferObject<float> _vbo;
        protected BufferObject<uint> _ebo;

        protected float[] _vertices = {
        // positions          // texture coords
         0.5f,  0.5f, 0.0f,     1.0f, 1.0f, // top right
         0.5f, -0.5f, 0.0f,     1.0f, 0.0f, // bottom right
        -0.5f, -0.5f, 0.0f,     0.0f, 0.0f, // bottom left
        -0.5f,  0.5f, 0.0f,     0.0f, 1.0f  // top left 
        };
        protected uint[] _indices =
        {
                0u, 1u, 3u,
                1u, 2u, 3u
        };
        public Object(GL _pGl, uint _pProgram, uint _pStride, float[]? _pVertices = null)
        {
            if (_pVertices != null)
            {
                _vertices = _pVertices;
            }
            _gl = _pGl;
            _program = _pProgram;
            _stride = _pStride;
            _gl.UseProgram(_program);
            NewTransformation();

            _vbo = new BufferObject<float>(_gl, _vertices, BufferTargetARB.ArrayBuffer);

            _ebo = new BufferObject<uint>(_gl, _indices, BufferTargetARB.ElementArrayBuffer);

            _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);
            _vao.AttributePointer(0, 3, _stride, 0);
            _vao.AttributePointer(1, 2, _stride, 3);
            _vao.Bind();
        }
        public virtual void Use()
        {
            Transformation.Use();
            _texture?.Bind();
            _vao.Bind();
            _vbo.Bind();
            _ebo.Bind();
        }
        public virtual void NewTexture(uint _textureLoc, string path)
        {
            _texture = new Texture(_gl, _program, _stride, path);
            _texture?.Use(_textureLoc);
            _texture?.CreateTexture();
            if (_texture != null)
            {
                Transformation.Scale.X = _texture.Width / (float)OpenGl.WINDOW_WIDTH;
                Transformation.Scale.Y = _texture.Heigth / (float)OpenGl.WINDOW_HEIGTH;
            }
        }
        [MemberNotNull(nameof(Transformation))]
        public virtual void NewTransformation()
        {
            Transformation = new Transformation(_gl, _program);
            Transformation.Use();
        }

        protected void NewVAO()
        {
            _vbo = new BufferObject<float>(_gl, _vertices, BufferTargetARB.ArrayBuffer);

            _ebo = new BufferObject<uint>(_gl, _indices, BufferTargetARB.ElementArrayBuffer);

            _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);
            _vao.AttributePointer(0, 3, _stride, 0);
            _vao.AttributePointer(1, 2, _stride, 3);
        }



        public void Scale(float _x = 1, float _y = 1, float _z = 1)
        {
            Transformation.Scale *= new Vector3D<float>(_x, _y, _z);
        }
        public void Move(float _x = 0, float _y = 0, float _z = 0)
        {
            Transformation.Position += new Vector3D<float>(_x, _y, _z);
        }
        public void Rotate(float _rotation)
        {
            Transformation.Rotation += _rotation;
        }


        public void SetScale(float _x = 1, float _y = 1, float _z = 1)
        {
            Transformation.Scale = new Vector3D<float>(_x, _y, _z);
        }
        public void SetPosition(float _x = 0, float _y = 0, float _z = 0)
        {
            Transformation.Position = new Vector3D<float>(_x, _y, _z);
        }
        public void SetRotation(float _rotation)
        {
            Transformation.Rotation = _rotation;
        }
    }
}
