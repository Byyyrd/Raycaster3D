using Silk.NET.Maths;
using Silk.NET.Input;
using System.Numerics;
using System.Collections.Generic;
using System;
using System.Net.Mail;
using System.Diagnostics;

namespace Raycaster3D
{
    internal class Raycasting
    {
        private readonly Vector3D<float> COLOR_WHITE = Vector3D<float>.One;
        private readonly Vector3D<float> COLOR_BLACK = Vector3D<float>.Zero;
        private readonly Vector3D<float> COLOR_RED   = new(1f,0f,0f);
        private readonly Vector3D<float> COLOR_GREEN = new(0f, 1f, 0f);
        private readonly Vector3D<float> COLOR_BLUE  = new(0f, 0f, 1f);
        private const float PI = 3.14159265358979323846f;
        private const float PI2 = PI / 2;
        private const float PI3 = 3 * PI / 2;
        private const float OneDegInRad = 0.0174533f;
        private const int squareSize = 64;
        private const int _mapWidth = 10;
        private const int _mapHeigth = 10;
        private readonly int playerWidth = 16;
        private readonly int playerHeigth = 16;
        private Vector2D<float> playerPos = new (300,400);
        private Vector2D<float> playerCenter;
        private float dx = 0,dy = 0;
        private float playerSpeed = 100f;
        private float playerAngle;
        private float viewBobbingAmount = 30;
        private float viewBobbingOffset;
        private float viewBobbing;
        private int numRays = 64;
        private float inputCooldown;
        private readonly int[] _map = {
            2,2,2,2,2,2,2,2,2,2,
            1,0,0,0,0,0,0,0,0,1,
            1,0,0,0,0,0,0,0,0,1,
            1,0,0,0,0,0,0,0,0,1,
            1,0,0,0,2,2,0,0,0,1,
            1,0,0,0,0,0,0,0,0,1,
            1,0,0,0,0,0,0,0,0,1,
            1,0,0,0,0,0,0,0,0,1,
            1,0,0,0,0,0,0,0,0,1,
            1,1,1,1,1,1,1,1,1,1,
        };
        private double timer;
        public Raycasting() {
        
        }
        public void Load()
        {
            playerAngle = PI3;
            dx = MathF.Cos(playerAngle) * playerSpeed;
            dy = MathF.Sin(playerAngle) * playerSpeed;
            OpenGl.AddKeyEvents(Key.W, (dt) => {
                playerPos.X += dx * (float) dt;
                playerPos.Y += dy * (float)dt;
                viewBobbing += (float)dt * PI;
                viewBobbingOffset = MathF.Abs(MathF.Sin(viewBobbing)) * viewBobbingAmount;
            });
            OpenGl.AddKeyEvents(Key.S, (dt) => {
                playerPos.Y -= dy * (float) dt;
                playerPos.X -= dx * (float)dt;
                viewBobbing -= (float)dt * PI;
                viewBobbingOffset = MathF.Abs(MathF.Sin(viewBobbing)) * viewBobbingAmount;

            });
            OpenGl.AddKeyEvents(Key.A, (dt) => {
                playerAngle -= OneDegInRad;
                if (playerAngle < 0)
                    playerAngle += PI * 2;
                dx = MathF.Cos(playerAngle) * playerSpeed;
                dy = MathF.Sin(playerAngle) * playerSpeed;

            });
            OpenGl.AddKeyEvents(Key.D, (dt) => {
                playerAngle += OneDegInRad;
                if (playerAngle >= 2 * PI)
                    playerAngle -= PI * 2;
                dx = MathF.Cos(playerAngle) * playerSpeed;
                dy = MathF.Sin(playerAngle) * playerSpeed;
            });
            OpenGl.AddKeyEvents(Key.Enter, dt =>
            {
                if(inputCooldown > .2)
                {
                    numRays++;
                    numRays %= 65;
                    inputCooldown = 0;
                }
                
            });
        }
        public void Update(double dt)
        {
            timer += dt;
            if(timer > 1)
            {
                Console.WriteLine($"Fps: {(int)(1 / dt)}");
                timer = 0;
            }
            playerCenter.X = playerPos.X + playerWidth / 2;
            playerCenter.Y = playerPos.Y + playerHeigth / 2;
            //LimitAngle(ref playerAngle);
            inputCooldown += (float)dt;
        }


        public void Render(double dt)
        {
            OpenGl.DrawRectangle(new(640, 320), 640, 320, new(.2f, .2f, .2f));
            OpenGl.DrawRectangle(new(640, 0), 640, 320, new(0, 0, .7f));
            CalculateRays();
            Draw2DMap();
            OpenGl.DrawRectangle(playerPos, 16, 16, COLOR_BLUE);
            
        }


        private void LimitAngle(ref float angle)
        {
            if (angle < 0)
                angle += 2 * PI;
            else if (angle > 2 * PI)
                angle -= 2 * PI;
        }


        private float dist(float ax, float ay, float bx, float by, float angle)
        {
            return MathF.Sqrt((bx - ax) * (bx - ax) + (by - ay) * (by - ay));
        }





        private void CalculateRays()
        {
            int depthOfField = 0,mapIndex = 0;
            float x = 0, y = 0;
            float offsetX = 0, offsetY = 0;
            float rayAngle = playerAngle - 32f * OneDegInRad; 
            LimitAngle(ref rayAngle);
            for (float r = 0; r < numRays; r += 1)
            {
                depthOfField = 0;
                // --- Horizantal Lines Collision ---
                float horizontalX = playerCenter.X, horizontalY = playerCenter.Y, horizontalDistance = 1000000;
                int textureIndexHorizontal = 1;
                //Caluculate Ankathete with Gegenkathete and Winkel
                float aTan = -1 / MathF.Tan(rayAngle);
                //Looking Up
                if (rayAngle > PI)
                {
                    offsetY = -64;
                    offsetX = aTan * -offsetY;
                    //Calculate nearest y position dividable by 64 above player
                    y = (((int)playerCenter.Y / 64) * 64) - 0.0001f;
                    //Offset X position according to y Movement along angle
                    x = playerCenter.X + aTan * (playerCenter.Y - y);

                }
                //Looking Down
                if (rayAngle < PI)
                {
                    offsetY = 64;
                    offsetX = aTan * -offsetY;
                    //Calculate nearest y position dividable by 64 below player
                    y = (((int)playerCenter.Y / 64) * 64) + 64;
                    //Offset X position according to y Movement along angle
                    x = playerCenter.X + aTan * (playerCenter.Y - y);
                }
                //Straigth left or Rigth
                if (rayAngle == 0 || rayAngle == PI) { x = playerCenter.X; y = playerCenter.Y; depthOfField = 10; }

                //offset y position by 64 and x positon accordingly to angle until object is hit or is out of bounds
                while (depthOfField < _mapWidth && depthOfField < _mapHeigth)
                {
                    //Calculate Index in map array with scrren koordiantes
                    mapIndex = (int)y / squareSize * _mapWidth + (int)x / squareSize;
                    if (mapIndex > 0 && mapIndex < _mapWidth * _mapHeigth && _map[mapIndex] > 0)
                    {
                        textureIndexHorizontal = _map[mapIndex] - 1;
                        depthOfField = _mapWidth + _mapHeigth;
                        horizontalX = x;
                        horizontalY = y;
                        horizontalDistance = dist(playerCenter.X, playerCenter.Y, horizontalX, horizontalY, rayAngle);
                    }
                    else
                    {
                        x += offsetX;
                        y += offsetY;
                        depthOfField++;
                    }
                }
                
                // --- Vertical Lines Collision ---

                x = 0; y = 0;
                depthOfField = 0;
                float verticalX = playerCenter.X, verticalY = playerCenter.Y, verticalDistance = 1000000;
                int textureIndexVeritcal = 1;

                //Caluculate Gegenkathete with Ankathete and Winkel
                float nTan = -MathF.Tan(rayAngle);
                //Looking Left
                if (rayAngle > PI2 && rayAngle < PI3)
                {
                    offsetX = -64;
                    offsetY = nTan * -offsetX;
                    //Calculate nearest y position dividable by 64 above player
                    x = ((int)playerCenter.X / 64) * 64 - 0.0001f;
                    //Offset X position according to y Movement along angle
                    y = playerCenter.Y + nTan * (playerCenter.X - x);
                }
                //Looking Rigth
                if (rayAngle < PI2 || rayAngle > PI3)
                {
                    offsetX = 64;
                    offsetY = nTan * -offsetX;
                    //Calculate nearest y position dividable by 64 below player
                    x = (((int)playerCenter.X / 64) * 64 ) + 64;
                    //Offset X position according to y Movement along angle
                    y = playerCenter.Y + nTan * (playerCenter.X - x);
                }
                //Straigth Up or Down
                if (rayAngle == PI2 || rayAngle == PI3)
                {
                    x = playerCenter.X;
                    y = playerCenter.Y;
                    depthOfField = 10;
                }

                //offset y position by 64 and x positon accordingly to angle until object is hit or is out of bounds
                while (depthOfField < _mapWidth && depthOfField < _mapHeigth)
                {
                    //Calculate Index in map array with scrren koordiantes
                    mapIndex = (int)(y) / squareSize * _mapWidth + (int)(x) / squareSize;
                    if (mapIndex > 0 && mapIndex < _mapWidth * _mapHeigth && _map[mapIndex] > 0)
                    {
                        textureIndexVeritcal = _map[mapIndex] - 1;
                        depthOfField = _mapWidth + _mapHeigth;
                        verticalX = x;
                        verticalY = y;
                        verticalDistance = dist(playerCenter.X, playerCenter.Y, verticalX, verticalY, rayAngle);
                    }
                    else
                    {
                        x += offsetX;
                        y += offsetY;
                        depthOfField++;
                    }
                }
                Vector3D<float> color = new(0,1,1);
                float distance = 640;
                bool verticalHit = false;
                if (verticalDistance < horizontalDistance) { x = verticalX; y = verticalY; color = new(0.3f, 0.0f, 0.0f); distance = verticalDistance;  verticalHit = true; }
                if (horizontalDistance < verticalDistance) { x = horizontalX; y = horizontalY; color = new(0.5f, 0.0f, 0.0f); distance = horizontalDistance; }
                OpenGl.DrawLine(playerCenter, new(x, y), COLOR_BLUE);
                // ---- Draw 3D ----
                //Remove Fisheye
                float offsetAngle = rayAngle - playerAngle; LimitAngle(ref offsetAngle);
                distance *= MathF.Cos(offsetAngle);

                float lineX = 640f + _mapWidth * (float)r;
                float lineHeigth = squareSize * 640 / distance;
                //if (lineHeigth > 640)
                  //  lineHeigth = 640;
                float lineOffset = 320 - (lineHeigth / 4);

                
                //OpenGl.DrawRectangle(new(lineX, lineOffset), 10f, lineHeigth, color);
                if (verticalHit)
                {
                    bool debug = false;
                    if (r == numRays - 1)
                    {
                        Console.WriteLine(y % squareSize);
                        debug = true;
                    }
                    float relativeX = y % squareSize;
                    if (rayAngle > PI2 && rayAngle < PI3)
                        relativeX = 1 - relativeX;
                    OpenGl.DrawWall(new(lineX, lineOffset), 10f, lineHeigth, relativeX,debug, rayAngle > PI2 && rayAngle < PI3);
                }
                else
                {
                    bool debug = false;
                    if (r == numRays - 1)
                    {
                        Console.WriteLine(x % squareSize);
                        debug = true;
                    }
                    float relativeX = x % squareSize;
                    if (rayAngle > 0 && rayAngle < PI)
                        relativeX = 1 - relativeX;

                    OpenGl.DrawWall(new(lineX, lineOffset), 10f, lineHeigth,relativeX,debug, rayAngle > 0 && rayAngle < PI);
                }

                rayAngle += OneDegInRad; LimitAngle(ref rayAngle);
            }
        }
        
        private void Draw2DMap()
        {
            for (int i = 0; i < _mapHeigth; i++)
            {
                for (int j = 0; j < _mapWidth; j++)
                {
                    int mapIndex = i * (_mapWidth) + j;
                    if (_map[mapIndex] > 0)
                    {

                        OpenGl.DrawRectangle(new(j * squareSize + 1, i * squareSize + 1), squareSize - 2, squareSize - 2, COLOR_WHITE);
                    }
                }

            }
        }
    }
}
