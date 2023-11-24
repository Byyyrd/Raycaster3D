using Raycaster3D;
Raycasting rc = new Raycasting();
OpenGl.Update = rc.Update;
OpenGl.Render = rc.Render;
OpenGl.Load = rc.Load;
OpenGl.Start();
