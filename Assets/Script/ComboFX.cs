using System;
using TMPro;
using UnityEngine;
using DG.Tweening;

namespace Script
{
    public class ComboFX : MonoBehaviour
    {
        [SerializeField] TextMeshPro TMP;
        [SerializeField] float animDuration;

        Vector3 m_InitPosition;
        [SerializeField] Transform m_AnimPosition;
        
        float shakeStrength = 0.05f;
        float shakeDuration = 0.5f;
        int shakeVibrato = 5;
        float shakeRandomness = 90f;

        bool m_IsShaking;

        void Awake()
        {
            m_InitPosition = transform.position;
            TMP.alpha = 0;
        }

        public void UpdateCombo(int combo)
        {
            if (combo == 1) Display();
            
            string newText = "combo";
            for (int i = 1; i < combo; i++)
            {
                newText += "+";
                SetShake(combo);
            }

            TMP.text = newText + "!";
        }

        void Display()
        {
            transform.position = m_InitPosition;
            TMP.DOFade(1f, animDuration);
            TMP.transform.DOMove(m_AnimPosition.position, animDuration);
        }

        void SetShake(int combo)
        {
            TMP.transform.DOComplete();
            shakeVibrato += combo;
            m_IsShaking = true;
            DoShake();
        }

        void DoShake()
        {
            if (!m_IsShaking) return;
            TMP.transform.DOMove(m_AnimPosition.position, 0.1f);
            transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, shakeRandomness, false, false)
                .SetEase(Ease.Linear) 
                .OnKill(DoShake);
        }

        public void CancelCombo()
        {
            m_IsShaking = false;
            TMP.transform.DOKill();
            TMP.DOFade(0f, animDuration);
            TMP.transform.DOMove(m_InitPosition, animDuration);
            shakeVibrato = 5;
        }
    }
}
