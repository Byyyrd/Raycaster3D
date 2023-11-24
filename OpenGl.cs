using System.Drawing;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using System;

namespace Raycaster3D
{
    internal class OpenGl
    {
        private static IWindow _window;
        private static GL _gl;
        private static uint _program,_texturedProgram;
        private static uint _stride;
        private static Dictionary<Key,KeyHandler> _keys = new();
        public const float WINDOW_WIDTH = 1280;
        public const float WINDOW_HEIGTH = 640;
        private static List<float> _pointVertices = new();
        private static List<uint> _pointIndices = new();
        private static List<float> _lineVertices = new();
        private static List<uint> _lineIndices = new();
        private static List<float> _triangleVertices = new();
        private static List<uint> _triangleIndices = new();
        private static float _animationTime, _animationCooldown = .1f,_animationFrames = 0;


        private static SpritesheetRenderer _gun;
        private static List<SpritesheetRenderer> _walls = new();
        private static List<float> _wallVertices = new();
        private static List<uint> _wallIndices = new();
        private static Texture _brickWallTexture,_metalWallTexture, _64x64Texture;
        private const float _brickWallTilingX = 380f / 10f; 
        private static Transformation _wallTransformation;
        private static List<int> _wallTextureIdentifiers = new();


        public static Action? Load;
        public static Action<double>? Update;
        public static Action<double>? Render;

        public static void Start() {
            WindowOptions options = WindowOptions.Default with
            {
                Size = new Vector2D<int>((int)WINDOW_WIDTH,(int) WINDOW_HEIGTH),
                Title = "Simple Raycaster"
            };
            _window = Window.Create(options);
            _window.Load += OnLoad;
            _window.Update += OnUpdate;
            _window.Render += OnRender;
            _window.Run();
        }
        private unsafe static void OnLoad() {
            SetupInputHandeling();
            _gl = _window.CreateOpenGL();
            _gl.ClearColor(Color.Black);
            _program = _gl.CreateProgram();
            _gl.UseProgram(_program);
            Shader.Use(_gl, _program);

            _texturedProgram = _gl.CreateProgram();
            _gl.UseProgram(_texturedProgram);
            _gl.Enable(EnableCap.ProgramPointSize);
            _gl.PointSize(10);
            _stride = 5 * sizeof(float);
            TexturedShader.Use(_gl, _texturedProgram);

            _brickWallTexture = Texture.GenerateTexture(_gl, _texturedProgram, _stride, 0, "Wall.png");
            _metalWallTexture = Texture.GenerateTexture(_gl, _texturedProgram, _stride, 0, "Metal_Wall.png");
            _64x64Texture = Texture.GenerateTexture(_gl, _texturedProgram, _stride, 0, "mossy.png");
            _wallTransformation = new Transformation(_gl, _texturedProgram);
            _wallTransformation.Use();
            _wallTransformation.Scale.X = _brickWallTexture.Width / WINDOW_WIDTH / _brickWallTilingX * 2;
            _wallTransformation.Scale.Y = _brickWallTexture.Heigth / WINDOW_HEIGTH / 1f * 2;

            _gun = new(_gl, _texturedProgram, _stride, 1, 15, 1);
            _gun.NewTexture(0, "ChaingunSpriteSheetScaled.png");
            _gun.SpritePosition = 0;

            _gun.SetPosition(.6f, -.54f, 0);            
            Load?.Invoke();
        }

        private static void OnUpdate(double deltaTime) {
            foreach (KeyHandler key in _keys.Values)
            {
                if (key.IsDown)
                    key.KeyAction?.Invoke(deltaTime);
            }
            Update?.Invoke(deltaTime);
            _animationTime += (float) deltaTime;
            if (_animationFrames > 12)
                _gun.TextTransformation.Position.X = 0;


            if (_animationFrames <= 12 &&_animationTime > _animationCooldown )
            {
                _gun.NextSpriteX();
                _animationTime = 0;

                _animationFrames++;
            }
        }

        private unsafe static void OnRender(double deltaTime) {
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            Render?.Invoke(deltaTime);

           

            _gl.UseProgram(_program);
            _stride = 6 * sizeof(float);


            //Setup Vao and Buffers for triangle drawing
            BufferObject<float> vbo = new(_gl, _triangleVertices.ToArray(), BufferTargetARB.ArrayBuffer);
            BufferObject<uint> ebo = new(_gl, _triangleIndices.ToArray(), BufferTargetARB.ElementArrayBuffer);
            VertexArrayObject<float, uint> vao = new(_gl, vbo, ebo);
            vao.AttributePointer(0, 3, _stride, 0);
            vao.AttributePointer(1, 3, _stride, 3);
            //Draw triangles
            vao.Bind();
            _gl.DrawElements(PrimitiveType.Triangles, (uint)_triangleIndices.Count, DrawElementsType.UnsignedInt, (void*)(0 * sizeof(uint)));

            //Setup Vao and Buffers for line drawing
            vbo = new(_gl, _lineVertices.ToArray(), BufferTargetARB.ArrayBuffer);
            ebo = new(_gl, _lineIndices.ToArray(), BufferTargetARB.ElementArrayBuffer);
            vao = new(_gl, vbo, ebo);
            vao.AttributePointer(0, 3, _stride, 0);
            vao.AttributePointer(1, 3, _stride, 3);
            //Draw lines
            vao.Bind();
            _gl.DrawElements(PrimitiveType.Lines, (uint)_lineIndices.Count, DrawElementsType.UnsignedInt, (void*)(0 * sizeof(uint)));

            //Setup Vao and Buffers for point drawing
            vbo = new(_gl, _pointVertices.ToArray(), BufferTargetARB.ArrayBuffer);
            ebo = new(_gl, _pointIndices.ToArray(), BufferTargetARB.ElementArrayBuffer);
            vao = new(_gl, vbo, ebo);
            vao.AttributePointer(0, 3, _stride, 0);
            vao.AttributePointer(1, 3, _stride, 3);
            //Draw points
            vao.Bind();
            _gl.DrawElements(PrimitiveType.Points, (uint)_pointIndices.Count, DrawElementsType.UnsignedInt, (void*)(0 * sizeof(uint)));

            //Clear point data
            _pointVertices.Clear();
            _pointIndices.Clear();
            //Clear line data
            _lineVertices.Clear();
            _lineIndices.Clear();
            //Clear triangle data
            _triangleVertices.Clear();
            _triangleIndices.Clear();

            // --- Draw Sprites ---
            _gl.UseProgram(_texturedProgram);
            _stride = 5 * sizeof(float);
            
            // --Draw Walls--
            vbo = new(_gl, _wallVertices.ToArray(), BufferTargetARB.ArrayBuffer);
            ebo = new(_gl, _wallIndices.ToArray(), BufferTargetARB.ElementArrayBuffer);
            vao = new(_gl, vbo, ebo);
            vao.AttributePointer(0, 3, _stride, 0);
            vao.AttributePointer(1, 2, _stride, 3);
            vao.Bind();
            //_wallTransformation.Use();
            TextureTransformation.UseNone(_gl, _texturedProgram);
            //_wallTextureIdentifiers.Sort();
            //uint index = (uint) _wallTextureIdentifiers.TakeWhile(i => i == 0).Count();
            Transformation.UseNone(_gl,_texturedProgram);
            //_metalWallTexture.Bind();
            //_gl.DrawElements(PrimitiveType.Triangles, index * 3u, DrawElementsType.UnsignedInt, (void*)0);
            //_brickWallTexture.Bind();
            //_gl.DrawElements(PrimitiveType.Triangles, (uint)(_wallIndices.Count - index * 3u), DrawElementsType.UnsignedInt, (void*)(sizeof(uint) * index * 3u));
            _64x64Texture.Bind();
            _gl.DrawElements(PrimitiveType.Triangles,(uint) _wallIndices.Count, DrawElementsType.UnsignedInt, (void*)0);

            _gun.Use();
            _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);

            // Clear wall data
            _wallVertices.Clear();
            _wallIndices.Clear();
            //Unbind Buffers and Vao
            _gl.BindVertexArray(0);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        }
        public static void DrawPoint(Vector2D<float> pos, Vector3D<float> color)
        {
            pos = WindowPosToScreenPos(pos);
            
            _pointIndices.AddRange(new uint[] { (uint)(0 + _pointVertices.Count / 6) });
            _pointVertices.AddRange(new float[]{
                pos.X,pos.Y,0f,color.X,color.Y,color.Z,
            });
        }
        public static void DrawLine(Vector2D<float> pos1, Vector2D<float> pos2, Vector3D<float> color)
        {
            pos1 = WindowPosToScreenPos(pos1);
            pos2 = WindowPosToScreenPos(pos2);
            _lineIndices.AddRange(new uint[] { (uint)(0 + _lineVertices.Count / 6), (uint)(1 + _lineVertices.Count / 6) });
            _lineVertices.AddRange(new float[]{
                pos1.X,pos1.Y,0f,color.X,color.Y,color.Z,
                pos2.X,pos2.Y,0f,color.X,color.Y,color.Z,
            });
        }
        public static void DrawRectangle(Vector2D<float> pos,float width,float heigth, Vector3D<float> color)
        {
            pos = WindowPosToScreenPos(pos);
            width = ValueToScreenValue(WINDOW_WIDTH,width);
            heigth = ValueToScreenValue(WINDOW_HEIGTH, heigth);
            _triangleIndices.AddRange(new uint[] {
                (uint)(0 + _triangleVertices.Count / 6), (uint)(3 + _triangleVertices.Count / 6), (uint)(2 + _triangleVertices.Count / 6),
                (uint)(2 + _triangleVertices.Count / 6), (uint)(1 + _triangleVertices.Count / 6), (uint)(0 + _triangleVertices.Count / 6) });
            _triangleVertices.AddRange(new float[]{
                pos.X        ,pos.Y         ,0f,color.X,color.Y,color.Z,
                pos.X        ,pos.Y - heigth,0f,color.X,color.Y,color.Z,
                pos.X + width,pos.Y - heigth,0f,color.X,color.Y,color.Z,
                pos.X + width,pos.Y         ,0f,color.X,color.Y,color.Z,
            });
        }
        public static void DrawWall(Vector2D<float> pos,float heigth,float animationFrame,int texture)
        {
            pos = WindowPosToScreenPos(pos);
            heigth = ValueToScreenValue(WINDOW_HEIGTH, heigth);
            float width = ValueToScreenValue(WINDOW_WIDTH, 10f);
            Vector4D<float>[] positions = new Vector4D<float>[4];
            positions[0] = new(1f, 0.0f,0f,1f);
            positions[1] = new(1.0f, - 1f - heigth,0f,1f);
            positions[2] = new(0.0f, -1f - heigth ,0f,1f);
            positions[3] = new(0f,  0f, 0f, 1f);
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = 
                    Vector4D.Transform(positions[i] , Transformation.CreateMatrix(
                    new(pos.X, pos.Y , 0.0f),
                    new Vector3D<float>(_brickWallTexture.Width / WINDOW_WIDTH / _brickWallTilingX , _brickWallTexture.Heigth / WINDOW_HEIGTH , 1f)
                    ));
            }
            float[] wallVertices = new float[]{
            // positions                                        // texture coords
                positions[0].X,positions[0].Y , 0.0f,     1f/_brickWallTilingX + animationFrame , 1f - .1f, // top right
                positions[1].X,positions[1].Y , 0.0f,     1f/_brickWallTilingX + animationFrame , 0.0f    , // bottom right
                positions[2].X,positions[2].Y , 0.0f,     0.0f   + animationFrame , 0.0f    , // bottom left
                positions[3].X,positions[3].Y , 0.0f,     0.0f   + animationFrame , 1f - .1f  // top left 
            };
            
            //Console.WriteLine(string.Join(" ", wallVertices)); Console.WriteLine("\n\n\n");
            uint[] wallIndices = {
                0u + (uint)_wallVertices.Count / 5, 1u + (uint)_wallVertices.Count / 5, 3u + (uint)_wallVertices.Count / 5,
                1u + (uint)_wallVertices.Count / 5, 2u + (uint)_wallVertices.Count / 5, 3u + (uint)_wallVertices.Count / 5
            };
            _wallTextureIdentifiers.Add(texture);
            _wallIndices.AddRange(wallIndices);
            _wallVertices.AddRange(wallVertices);
        }
        public static void DrawWall(Vector2D<float> pos, float width, float heigth, float relativeX,bool debug,bool flip)
        {
            pos = WindowPosToScreenPos(pos);
            //heigth = ValueToScreenValue(WINDOW_HEIGTH, heigth);
            //width = ValueToScreenValue(WINDOW_WIDTH, width);
            Vector4D<float>[] positions = new Vector4D<float>[4];
            Vector3D<float>[] uvs = new Vector3D<float>[4];
            positions[0] = new(1.0f,  0.0f, 0f, 1f);
            positions[1] = new(1.0f, -1.0f, 0f, 1f);
            positions[2] = new(0.0f, -1.0f, 0f, 1f);
            positions[3] = new(0.0f,  0.0f, 0f, 1f);

            uvs[0] = new(relativeX / _64x64Texture.Width + width/64f, 1.0f , 0 );
            uvs[1] = new(relativeX / _64x64Texture.Width + width/64f, 0.0f , 0 );
            uvs[2] = new(relativeX / _64x64Texture.Width         , 0.0f , 0 );
            uvs[3] = new(relativeX / _64x64Texture.Width         , 1.0f, 0);
            
            float scaleX = (_64x64Texture.Width / (WINDOW_WIDTH / 2)) * (width / _64x64Texture.Width);
            float scaleY = (_64x64Texture.Heigth / WINDOW_HEIGTH) * (heigth / _64x64Texture.Heigth);
            for (int i = 0; i < positions.Length; i++)
            {
                Matrix4X4<float> matrix = Transformation.CreateMatrix(
                    new(pos.X, pos.Y, 0.0f),
                    new Vector3D<float>(scaleX, scaleY, 1f)
                    );

                positions[i] = Vector4D.Transform(positions[i], matrix);
                
            }

            
            
            float[] wallVertices = new float[]{
                // positions                              // texture coords
                positions[0].X,positions[0].Y , 0.0f,     relativeX/_64x64Texture.Width, 1.0f, // top right
                positions[1].X,positions[1].Y , 0.0f,     relativeX/_64x64Texture.Width , 0.0f, // bottom right
                positions[2].X,positions[2].Y , 0.0f,     relativeX/_64x64Texture.Width, 0.0f, // bottom left
                positions[3].X,positions[3].Y , 0.0f,     relativeX/_64x64Texture.Width, 1.0f  // top left 
            };


            //Console.WriteLine(string.Join(" ", wallVertices)); Console.WriteLine("\n\n\n");
            uint[] wallIndices = {
                0u + (uint)_wallVertices.Count / 5, 1u + (uint)_wallVertices.Count / 5, 3u + (uint)_wallVertices.Count / 5,
                1u + (uint)_wallVertices.Count / 5, 2u + (uint)_wallVertices.Count / 5, 3u + (uint)_wallVertices.Count / 5
            };
            if(_wallVertices.Count >= 17)
            {

                if (relativeX / _64x64Texture.Width < _wallVertices[_wallVertices.Count - 17] || relativeX / _64x64Texture.Width < _wallVertices[_wallVertices.Count - 12])
                {
                    //If texture extends over two squres(bzw. Texture Width) match uvs to cover end of old and start of new (bzw offset by one because texture is on repeat)
                    //Top Rigth UV
                    _wallVertices[_wallVertices.Count - 17] = 1 + relativeX / _64x64Texture.Width;
                    //Bottom Rigth Uv
                    _wallVertices[_wallVertices.Count - 12] = 1 + relativeX / _64x64Texture.Width;
                }
                else //Case inside one square
                {
                    //Top Rigth UV
                    _wallVertices[_wallVertices.Count - 17] = relativeX / _64x64Texture.Width;
                    //Bottom Rigth Uv
                    _wallVertices[_wallVertices.Count - 12] = relativeX / _64x64Texture.Width;
                }


                
                
            }
           
            //_wallTextureIdentifiers.Add(texture);
            _wallIndices.AddRange(wallIndices);
            _wallVertices.AddRange(wallVertices);
        }

        private static Vector2D<float> WindowPosToScreenPos(Vector2D<float> pos)
        {
            float x = Lerp(-1, 1,Normalize(0,WINDOW_WIDTH,pos.X));
            float y = Lerp(-1, 1, Normalize(0, WINDOW_HEIGTH, pos.Y));
            return new Vector2D<float>(x, -y);
        }
        private static float ValueToScreenValue(float max,float value)
        {
            return Lerp(0, 2, Normalize(0, max, value));
        }

        public static float Lerp(float min,float max,float value)
        {
            return min + (max - min)*value;
        } 
        private static float Normalize(float min, float max, float value)
        {
            return value / max - min / max;
        }
        private static void SetupKeyEvents()
        {
            _keys.Add(Key.Space, new KeyHandler((dt) => {
                _animationFrames = 0;
                    
            }));
            _keys.Add(Key.Escape, new KeyHandler((dt) => { _window.Close(); }));

        }
        public static void AddKeyEvents(Key key,Action<double> action)
        {
            _keys.Add(key, new KeyHandler(action));
        }
        private static void SetupInputHandeling()
        {
            IInputContext input = _window.CreateInput();
            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += KeyDown;
                input.Keyboards[i].KeyUp += KeyUp;
            }
            SetupKeyEvents();
        }
        

        private static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            _keys.TryGetValue(key, out KeyHandler? keyHandler);
            keyHandler?.SetIsDown(true);
        }
        private static void KeyUp(IKeyboard keyboard, Key key, int keyCode)
        {
            _keys.TryGetValue(key, out KeyHandler? keyHandler);
            keyHandler?.SetIsDown(false);
        }
        private class KeyHandler
        {
            public Action<double>? KeyAction { get; set; }
            public bool IsDown { get; set; }
            public KeyHandler() { }
            public KeyHandler(Action<double> _keyAction)
            {
                KeyAction = _keyAction;  
            }
            public void SetIsDown(bool value)
            {
                IsDown = value;
            }
        }
    }
}
