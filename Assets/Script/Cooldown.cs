using UnityEngine;

namespace Script
{
    public class Cooldown
    {
        public delegate void CooldownEvent();
        CooldownEvent m_CoolDownOver;
        CooldownEvent m_CoolDownUpdate;

        float m_Duration;
        public float TimeLeft;
        bool m_Enabled;
        
        /// <summary>
        /// Duration of cooldown.
        /// </summary>
        public float Duration
        {
            get => m_Duration;
            set
            {
                if (value > 0)
                    m_Duration = value;
            }
        }

        /// <summary>
        /// Return percentage of cooldown progress 
        /// </summary>
        public float Progress => Mathf.InverseLerp(m_Duration, 0f, TimeLeft);

        /// <summary>
        /// Returns ‘true’ if the cooldown has ended 
        /// </summary>
        public bool HasEnded => TimeLeft <= 0;

        /// <summary>
        /// Return 'true' if is running
        /// </summary>
        public bool IsRunning => m_Enabled;

        /// <summary>
        /// A simple cooldown class. Calling Update method is needed to work.
        /// </summary>
        /// <param name="duration">Cooldown duration</param>
        /// <param name="cooldownOver">Method called when the cooldown has finished</param>
        /// <param name="cooldownUpdate">Method called when the cooldown has updated</param>
        public Cooldown(float duration, CooldownEvent cooldownOver = null, CooldownEvent cooldownUpdate = null)
        {
            m_Duration = duration;
            m_CoolDownOver = cooldownOver;
            m_CoolDownUpdate = cooldownUpdate;
        }

        public void Start(float newCooldown = 0f)
        {
            if (m_Enabled) return;

            m_Duration = newCooldown > 0f ? newCooldown : m_Duration;
            Reset();
            m_Enabled = true;
        }

        public void Pause()
        {
            m_Enabled = false;
        }

        public void Resume()
        {
            m_Enabled = true;
        }

        /// <summary>
        /// Manually finish the cooldown.
        /// </summary>
        public void Complete()
        {
            TimeLeft = 0;
        }

        /// <summary>
        /// Reset timer.
        /// </summary>
        public void Reset()
        {
            Pause();
            TimeLeft = m_Duration;
        }

        /// <summary>
        /// Reduces remaining time.
        /// </summary>
        /// <param name="value">Time to remove</param>
        public void DecreaseTime(float value)
        {
            TimeLeft -= value;
        }
        
        /// <summary>
        /// Increases remaining time.
        /// </summary>
        /// <param name="value">Time to add</param>
        public void IncreaseTime(float value)
        {
            TimeLeft += value;
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Call this method in Update method to make the cooldown work
        /// </summary>
        public void Update()
        {
            if (!m_Enabled) return;

            TimeLeft -= Time.deltaTime;
            m_CoolDownUpdate?.Invoke();
            
            if (TimeLeft <= 0) OnCooldownFinished();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        void OnCooldownFinished()
        {
            m_Enabled = false;
            m_CoolDownOver?.Invoke();
        }
    }
}
