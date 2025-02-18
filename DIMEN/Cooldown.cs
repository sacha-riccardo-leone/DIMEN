using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace DIMEN
{
    class Cooldown
    {
        public float Duration;
        public float Timer;
        public float TotalCooldown;
        public float Progress;
        public bool Filling;

        public Cooldown(float duration)
        {
            Duration = duration;
            Timer = 0.0f;
            TotalCooldown = duration/2;
            Progress = 1.0f;
            Filling = false;
        }
        public void Update() 
        {


        }
    }
}
