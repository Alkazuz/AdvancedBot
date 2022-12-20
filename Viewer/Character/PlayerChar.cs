using AdvancedBot.client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.Viewer.Character
{
    public class PlayerChar
    {
        private Cube head;
        private Cube body;
        private Cube arm0; //right arm
        private Cube arm1; //left arm
        private Cube leg0; //right leg
        private Cube leg1; //left leg

        public PlayerChar()
        {
            head = new Cube(0, 0).AddBox(-4.0f, -8.0f, -4.0f, 8, 8, 8);
            body = new Cube(16, 16).AddBox(-4.0f, 0.0f, -2.0f, 8, 12, 4);
            arm0 = new Cube(40, 16).AddBox(-3.0f, -2.0f, -2.0f, 4, 12, 4).SetPos(-5.0f, 2.0f, 0.0f);
            arm1 = new Cube(40, 16).AddBox(-1.0f, -2.0f, -2.0f, 4, 12, 4).SetPos(5.0f, 2.0f, 0.0f);
            leg0 = new Cube(0, 16).AddBox(-2.0f, 0.0f, -2.0f, 4, 12, 4).SetPos(-2.0f, 12.0f, 0.0f);
            leg1 = new Cube(0, 16).AddBox(-2.0f, 0.0f, -2.0f, 4, 12, 4).SetPos(2.0f, 12.0f, 0.0f);
        }
        public void Render(CharInfo info)
        {
            double time = DateTime.Now.Ticks / (10000000.0 / 20.0);

            const float c = 180.0f / (float)Math.PI;
            //head.RotY =  / c;
            head.RotX = info.Pitch / c;

            float run = info.Run;

            arm0.RotX = (float)Math.Cos(time * 0.6662 + Math.PI) * 2.0f * run;
            arm0.RotZ = (float)(Math.Cos(time * 0.2312) + 1.0) * run;
            arm1.RotX = (float)Math.Cos(time * 0.6662) * 2.0f * run;
            arm1.RotZ = (float)(Math.Cos(time * 0.2812) - 1.0) * run;
            leg0.RotX = (float)Math.Cos(time * 0.6662) * 1.4f * run;
            leg1.RotX = (float)Math.Cos(time * 0.6662 + Math.PI) * 1.4F * run;

            arm0.RotZ += (float)Math.Cos(time * 0.090) * 0.05f + 0.05f;
            arm1.RotZ -= (float)Math.Cos(time * 0.090) * 0.05f + 0.05f;
            arm0.RotX += (float)Math.Sin(time * 0.067) * 0.05f;
            arm1.RotX -= (float)Math.Sin(time * 0.067) * 0.05f;

            GL.glColor3f(1.0f, 1.0f, 1.0f);
            GL.glPushMatrix();
            GL.glTranslated(info.X, info.Y, info.Z);

            const float size = 0.058333334f;
            GL.glScalef(size, -size, size);

            GL.glTranslated(0.0, 0.0, 0.0);
            GL.glRotatef(180.0f - info.Yaw, 0.0f, 1.0f, 0.0f);

            head.Render();
            body.Render();
            arm0.Render();
            arm1.Render();
            leg0.Render();
            leg1.Render();

            GL.glPopMatrix();
        }

        public struct CharInfo
        {
            public double X, Y, Z;
            public float Yaw, Pitch;
            public float Run;
        }
    }
}
