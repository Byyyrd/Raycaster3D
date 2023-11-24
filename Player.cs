using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycaster3D
{
    internal class Player : Object
    {
        private Texture[] _textures;
        public uint SpritePosition;
        public TextureTransformation TextTransformation;
        public Player(GL _pGl, uint _pProgram,uint _stride,int _spriteAmount, float[]? _vertices = null) : base(_pGl, _pProgram, _stride,_vertices)
        {
            _textures = new Texture[_spriteAmount];
        }
        public override void NewTexture(uint _textureLoc, string path)
        {
            _textures[_textureLoc] = new Texture(_gl, _program, _stride, path);
            _textures[_textureLoc].Use(_textureLoc + 1);
            _textures[_textureLoc].CreateTexture();
            if (_textures[_textureLoc] != null)
            {
                Transformation.Scale.X = _textures[_textureLoc].Width / OpenGl.WINDOW_WIDTH;
                Transformation.Scale.Y = _textures[_textureLoc].Heigth / OpenGl.WINDOW_HEIGTH;
            }
        }
        public override void NewTransformation()
        {
            base.NewTransformation();
            TextTransformation = new TextureTransformation(_gl, _program);
            TextTransformation.Use();
        }
        public override void Use()
        {
            if (_textures[SpritePosition] != null)
            {
                Transformation.Scale.X = _textures[SpritePosition].Width / OpenGl.WINDOW_WIDTH / 15f;
                Transformation.Scale.Y = _textures[SpritePosition].Heigth / OpenGl.WINDOW_HEIGTH / 1f;
            }
            Transformation.Use();
            TextTransformation.Use();
            _textures[SpritePosition].Bind();
            _vao.Bind();
        }
    }
}
