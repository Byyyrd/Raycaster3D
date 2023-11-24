using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycaster3D
{
    internal class BufferObject<T> where T : unmanaged
    {
        private GL _gl;
        private uint _vbo;
        private BufferTargetARB _target;
        public unsafe BufferObject(GL _pGl,T[] _data,BufferTargetARB _pTarget) 
        {
            _gl = _pGl;
            _target = _pTarget;
            _vbo = _gl.GenBuffer();
            Bind();
            fixed (T* buf = _data)
                _gl.BufferData(_target, (nuint)(_data.Length * sizeof(T)), buf, BufferUsageARB.DynamicDraw);
        }
        
        public void Bind()
        {
            _gl.BindBuffer(_target, _vbo);
        }
    }
}
