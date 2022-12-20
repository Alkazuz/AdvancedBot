using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace AdvancedBot.Viewer
{
    public class WGL
    {
        private IntPtr hdc = IntPtr.Zero;
        private IntPtr hrc = IntPtr.Zero;
        private IntPtr hWnd;

        private WGL() { }
        public static WGL Create(Control c)
        {
            if (!c.IsHandleCreated)
                throw new ArgumentException("Control handle was not created");

            IntPtr handle = c.Handle;

            WGL gl = new WGL();
            gl.hWnd = handle;
            gl.hdc = GetDC(handle);

            if (gl.hdc == IntPtr.Zero)
                throw new Exception("Device context not available");

            PIXELFORMATDESCRIPTOR pixelFormat = new PIXELFORMATDESCRIPTOR();
            pixelFormat.nSize = (ushort)Marshal.SizeOf<PIXELFORMATDESCRIPTOR>();
            pixelFormat.nVersion = 1;
            pixelFormat.dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER;
            pixelFormat.iPixelType = PFD_TYPE_RGBA;
            pixelFormat.cColorBits = 32;
            pixelFormat.cDepthBits = 32;
            pixelFormat.iLayerType = PFD_MAIN_PLANE;

            int pFormat = ChoosePixelFormat(gl.hdc, pixelFormat);

            if (pFormat == 0) {
                ReleaseDC(handle, gl.hdc);
                gl.hdc = IntPtr.Zero;
                throw new Exception("Pixel format not available");
            }

            if (!SetPixelFormat(gl.hdc, pFormat, pixelFormat)) {
                ReleaseDC(handle, gl.hdc);
                throw new Exception("Could not set pixel format");
            }

            gl.hrc = wglCreateContext(gl.hdc);
            if (gl.hrc == IntPtr.Zero) {
                ReleaseDC(handle, gl.hdc);
                throw new Exception("Could not create rendering context (Code " + Marshal.GetLastWin32Error().ToString() + ")");
            }

            wglMakeCurrent(gl.hdc, gl.hrc);

            GL.Setup();
            return gl;
        }

        public void Flush()
        {
            glFlush();
            SwapBuffers(hdc);
        }
        public void Destroy()
        {
            wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            wglDeleteContext(hrc);
            ReleaseDC(hWnd, hdc);
        }

        [StructLayout(LayoutKind.Sequential)]
        private class PIXELFORMATDESCRIPTOR
        {
            public ushort nSize;
            public ushort nVersion;
            public uint dwFlags;
            public byte iPixelType;
            public byte cColorBits;
            public byte cRedBits;
            public byte cRedShift;
            public byte cGreenBits;
            public byte cGreenShift;
            public byte cBlueBits;
            public byte cBlueShift;
            public byte cAlphaBits;
            public byte cAlphaShift;
            public byte cAccumBits;
            public byte cAccumRedBits;
            public byte cAccumGreenBits;
            public byte cAccumBlueBits;
            public byte cAccumAlphaBits;
            public byte cDepthBits;
            public byte cStencilBits;
            public byte cAuxBuffers;
            public byte iLayerType;
            public byte bReserved;
            public uint dwLayerMask;
            public uint dwVisibleMask;
            public uint dwDamageMask;
        }

        private const byte PFD_TYPE_RGBA = 0;
        private const byte PFD_TYPE_COLORINDEX = 1;
        private const uint PFD_DOUBLEBUFFER = 1;
        private const uint PFD_DRAW_TO_WINDOW = 4;
        private const uint PFD_SUPPORT_OPENGL = 32;
        private const byte PFD_MAIN_PLANE = 0;

        [DllImport("gdi32.dll")] private static extern int ChoosePixelFormat(IntPtr hDC, PIXELFORMATDESCRIPTOR ppfd);
        [DllImport("gdi32.dll")] private static extern bool SetPixelFormat(IntPtr hDC, int iPixelFormat, PIXELFORMATDESCRIPTOR ppfd);
        [DllImport("gdi32.dll")] private static extern bool SwapBuffers(IntPtr hDC);
        [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("opengl32.dll")] private static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hrc);
        [DllImport("opengl32.dll")] private static extern bool wglDeleteContext(IntPtr hdc);
        [DllImport("opengl32.dll")] private static extern IntPtr wglCreateContext(IntPtr hdc);
        [DllImport("opengl32.dll")] private static extern void glFlush();
    }
}
