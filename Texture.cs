using Silk.NET.OpenGL;
using StbImageSharp;
using System.IO;


namespace Raycaster3D
{
    
    internal unsafe class Texture
    {
        private uint _texture;
        private GL _gl;
        private uint _stride;
        private uint _program;
        private string _path;
        public int Width{ get; private set; }
        public int Heigth { get; private set; }
        public Texture(GL _pGl,uint _pProgram,uint _pStride,string _pPath) {
            _gl = _pGl;
            _program = _pProgram;
            _stride = _pStride;
            _path = _pPath;
        }
        public void CreateTexture()
        {
            _texture = _gl.GenTexture();
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, _texture);

            StbImage.stbi_set_flip_vertically_on_load(1);
            ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\resources\\" + _path), ColorComponents.RedGreenBlueAlpha);
            Width = result.Width;
            Heigth = result.Height;

            fixed (byte* ptr = result.Data)
            {

                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)result.Width,
                (uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
            }

            _gl.TextureParameter(_texture, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            _gl.TextureParameter(_texture, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            _gl.TextureParameter(_texture, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            _gl.TextureParameter(_texture, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            _gl.GenerateMipmap(TextureTarget.Texture2D);

            _gl.BindTexture(TextureTarget.Texture2D, 0);

            int location = _gl.GetUniformLocation(_program, "uTexture");
            _gl.Uniform1(location, 0);


            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        }
        public void Use(uint textureLoc)
        {
            _gl.EnableVertexAttribArray(textureLoc);
            _gl.VertexAttribPointer(textureLoc, 2, VertexAttribPointerType.Float, false, _stride, (void*)(3 * sizeof(float)));

        }
        public void Bind()
        {
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, _texture);
        }
        public static Texture GenerateTexture(GL _gl, uint _program,uint _stride, uint _textureLoc, string path)
        {
            Texture texture = new (_gl, _program, _stride, path);
            texture.Use(_textureLoc);
            texture.CreateTexture();
            return texture;
        }
    }
}
