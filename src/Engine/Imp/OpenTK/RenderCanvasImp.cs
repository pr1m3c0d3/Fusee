﻿using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
#if ANDROID
using Android.App;
using Android.Content;
using OpenTK.Platform.Android;
using OpenTK.Graphics.ES20;
#else
using System.Drawing;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
#endif

using MathHelper = OpenTK.MathHelper;

namespace Fusee.Engine
{
    public class RenderCanvasImp : IRenderCanvasImp
    {
        public int Width { get { return _width; }}
        internal int _width;
        public int Height { get { return _height; } }
        internal int _height;
        private Dictionary<string, object> _globals;

        public double DeltaTime
        {
            get
            {
                return _gameWindow.DeltaTime; 
            }
        }

        internal RenderCanvasGameWindow _gameWindow;

        public RenderCanvasImp(Dictionary<string, object> globals)
        {
            _globals = globals;
            _gameWindow = new RenderCanvasGameWindow(this, _globals);
        }

        public void Present()
        {
            if (_gameWindow != null)
                _gameWindow.SwapBuffers();
        }

        public void Run()
        {
            if (_gameWindow != null)
            {
#if ANDROID
                _gameWindow.Run(30.0);
#else
                _gameWindow.Run(30.0, 0.0);
#endif

            }
        }

        public void Pause()
        {
#if ANDROID
            _gameWindow.Pause();
#endif
        }

        public void Resume()
        {
#if ANDROID
            _gameWindow.Resume();
#endif
        }

        public event EventHandler<InitEventArgs> Init;
        public event EventHandler<RenderEventArgs> Render;
        public event EventHandler<ResizeEventArgs> Resize;

        internal void DoInit()
        {
            if (Init != null)
                Init(this, new InitEventArgs());
        }

        internal void DoRender()
        {
            if (Render != null)
                Render(this, new RenderEventArgs());
        }

        internal void DoResize()
        {
            if (Resize != null)
                Resize(this, new ResizeEventArgs());
        }
    }

#if ANDROID
    class RenderCanvasGameWindow : AndroidGameView
#else
    class RenderCanvasGameWindow : GameWindow   
#endif
    {
        private RenderCanvasImp _renderCanvasImp;
        private double _deltaTime;
        public double DeltaTime
        {
            get { return _deltaTime; }
        }

        public RenderCanvasGameWindow(RenderCanvasImp renderCanvasImp, Dictionary<string, object> globals)
#if ANDROID
            : base((Context) globals["Context"])
#else
            : base(1280, 720, new GraphicsMode(32,24,0,8) /*GraphicsMode.Default*/, "Fusee Engine")
#endif
        {
            _renderCanvasImp = renderCanvasImp;
#if ANDROID
            ((Activity) globals["Context"]).SetContentView(this);
#endif
        }

        protected override void OnLoad(EventArgs e)
        {
            // Check for necessary capabilities:
#if ANDROID
            string version = GL.GetString((All) StringName.Version);
#else
            string version = GL.GetString(StringName.Version);
#endif
            int major = (int)version[0];
            int minor = (int)version[2];
            if (major < 2)
            {
#if ANDROID
                // TODO: Exit on Android
#else
                MessageBox.Show("You need at least OpenGL 2.0 to run this example. Aborting.", "GLSL not supported",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                this.Exit();
#endif
            }

#if ANDROID
            GL.ClearColor(0, 0, 0.2f, 1);
            GL.Enable((All) EnableCap.DepthTest);
#else
            GL.ClearColor(Color.MidnightBlue);
            GL.Enable(EnableCap.DepthTest);
#endif
            _renderCanvasImp.DoInit();
        }

        protected override void OnUnload(EventArgs e)
        {

            // if (_renderCanvasImp != null)
            //     _renderCanvasImp.Dispose();      
        }


        protected override void OnResize(EventArgs e)
        {
            if (_renderCanvasImp != null)
            {
                _renderCanvasImp._width = Width;
                _renderCanvasImp._height = Height;
                _renderCanvasImp.DoResize();
            }

            /*
            GL.Viewport(0, 0, Width, Height);

            float aspect_ratio = Width / (float)Height;
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect_ratio, 1, 64);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perpective);
             * */
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
#if ANDROID
                // TODO: Exit on Android
#else           
            if (Keyboard[OpenTK.Input.Key.Escape])
                this.Exit();

            if (Keyboard[OpenTK.Input.Key.F11])
                if (WindowState != WindowState.Fullscreen)
                    WindowState = WindowState.Fullscreen;
                else
                    WindowState = WindowState.Normal;
#endif
            }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _deltaTime = e.Time;
            if (_renderCanvasImp != null)
            {
                _renderCanvasImp.DoRender();
            }
        }
    }
}
