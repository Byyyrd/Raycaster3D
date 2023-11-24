using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycaster3D
{
    internal class VertexArrayObject<TVertex,TIndex> where TIndex : unmanaged where TVertex : unmanaged
    {
        private uint _vao;
        private BufferObject<TVertex> _vbo;
        private BufferObject<TIndex> _ebo;

        private GL _gl;
        public VertexArrayObject(GL _pGl,BufferObject<TVertex> _pVbo,BufferObject<TIndex> _pEbo) {
            _gl = _pGl;
            _vao = _gl.GenVertexArray();
            _vbo = _pVbo;
            _ebo = _pEbo;
            Bind();
            _vbo.Bind();
            _ebo.Bind();

        }
        public void Bind()
        {
            
            _gl.BindVertexArray(_vao);
            _vbo.Bind();
            _ebo.Bind();

        }
        public unsafe void AttributePointer(uint _positionLoc,int _size,uint _stride,int _offset)
        {
            _gl.EnableVertexAttribArray(_positionLoc);
            _gl.VertexAttribPointer(_positionLoc, _size, VertexAttribPointerType.Float, false, _stride, (void*)(_offset * sizeof(TVertex)));

        }
    }
}
