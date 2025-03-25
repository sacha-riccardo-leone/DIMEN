using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DIMEN
{
    class Cooldown
    {
        public float Duration { get; }
        private float timer;
        public float Progress { get; private set; }
        private bool filling;
        public bool IsTopView { get; private set; }

        public Cooldown(float duration)
        {
            Duration = duration;
            Reset();
        }

        public void Update(float deltaTime)
        {
            if (IsTopView)
            {
                timer += deltaTime;
                Progress -= deltaTime / Duration;

                if (Progress <= 0.0f)
                {
                    Progress = 0.0f;
                    filling = true;
                }

                if (timer >= Duration)
                {
                    IsTopView = false;
                    filling = true;
                    timer = 0;
                }
            }
            else if (filling)
            {
                Progress += deltaTime / Duration;
                if (Progress >= 1.0f)
                {
                    Progress = 1.0f;
                    filling = false;
                }
            } 
        }

        public void ToggleView()
        {
            if (!IsTopView && Progress >= 1.0f)
            {
                IsTopView = true;
                timer = 0;
            }
            else
            {
                IsTopView = false;
                filling = true;
            }
        }
        public void Reset()
        {
            timer = 0;
            Progress = 1.0f;
            filling = false;
        }
    }
}
