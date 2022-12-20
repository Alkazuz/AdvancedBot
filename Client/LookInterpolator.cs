using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedBot.client
{
    public class LookInterpolator
    {
        public float SrcYaw, SrcPitch;
        public float DstYaw, DstPitch;
        public float Duration;
        public Entity Player;
        public long Start;

        public LookInterpolator(Entity e, float dstYaw, float dstPitch, float dur)
        {
            Player = e;
            DstYaw = Utils.WrapAngleTo360(dstYaw);
            DstPitch = Utils.WrapAngleTo360(dstPitch);
            SrcYaw = Utils.WrapAngleTo360(e.Yaw);
            SrcPitch = Utils.WrapAngleTo360(e.Pitch);
            Duration = dur;
            Start = Utils.GetTimestamp();
        }
        public void InterpolateToPlayer()
        {
            float t = (Utils.GetTimestamp() - Start) / (Duration * 1000.0f);
            if (t >= 1.0f) t = 1.0f;
            Player.Yaw = Utils.WrapAngleTo180(Lerp(SrcYaw, DstYaw, t));
            Player.Pitch = Utils.WrapAngleTo180(Lerp(SrcPitch, DstPitch, t));
        }
        public bool Finished {
            get {
                return (Utils.GetTimestamp() - Start) / 1000.0f > Duration;
            }
        }

        //linear interpolator
        public float Lerp(float a, float b, float t) {
            return a + (b - a) * t;
        }
    }
}
