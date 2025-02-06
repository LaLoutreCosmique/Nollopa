using System;
using TMPro;
using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace Script
{
    public class KeyFX : MonoBehaviour
    {
        public Player.NerfColor color;
        public bool PressTwice => m_PressTwice;
        public bool IsPressed => m_CurrentDown == spriteRenderer.sprite;
        public bool HasMoved => transform.position != m_InitPos;
        
        [SerializeField] SpriteRenderer spriteRenderer;
        
        [Header("Black key")]
        [SerializeField] Sprite upBlack;
        [SerializeField] Sprite downBlack;
        
        [Header("Green key")]
        [SerializeField] Sprite upGreen;
        [SerializeField] Sprite downGreen;
        
        [Header("Red key")]
        [SerializeField] Sprite upRed;
        [SerializeField] Sprite downRed;
        
        [Header("Orange key")]
        [SerializeField] Sprite upOrange;
        [SerializeField] Sprite downOrange;
        
        [Header("Settings")]
        [SerializeField] TextMeshPro charSlot;
        [SerializeField] float animDuration;
        [SerializeField] ParticleSystem destroyParticle;

        Sprite m_CurrentUp;
        Sprite m_CurrentDown;

        const float PressedOffset = -0.14f;
        bool m_PressTwice;
        bool m_IsShaking;
        Vector3 m_InitPos;
        
        public KeyFX Init(char character, Vector3 endAnim, Player.NerfColor color)
        {
            this.color = color;
            SetSpriteColor(color);

            m_InitPos = endAnim;
            spriteRenderer.sprite = m_CurrentUp;
            charSlot.text = character.ToString();
            transform.DOMove(endAnim, animDuration);
            return this;
        }

        public void Press()
        {
            charSlot.rectTransform.localPosition = new Vector3(0, PressedOffset, -0.05f);
            charSlot.alpha = 0.3f;
            spriteRenderer.sprite = m_CurrentDown;

            if (m_IsShaking) StopShake();
        }

        public void Release()
        {
            if (color == Player.NerfColor.Green) m_PressTwice = true;
            charSlot.rectTransform.localPosition = new Vector3(0, 0, -0.05f);
            charSlot.alpha = 1f;
            spriteRenderer.sprite = m_CurrentUp;
            
            if (m_IsShaking) StopShake();
        }

        void OnDestroy()
        {
            Instantiate(destroyParticle, transform.position, quaternion.identity);
        }

        void SetSpriteColor(Player.NerfColor color)
        {
            switch (color)
            {
                case Player.NerfColor.Green:
                    m_PressTwice = true;
                    m_CurrentUp = upGreen;
                    m_CurrentDown = downGreen;
                    break;
                case Player.NerfColor.Red:
                    m_CurrentUp = upRed;
                    m_CurrentDown = downRed;
                    break;
                case Player.NerfColor.Orange:
                    m_CurrentUp = upOrange;
                    m_CurrentDown = downOrange;
                    break;
                case Player.NerfColor.Black:
                    m_CurrentUp = upBlack;
                    m_CurrentDown = downBlack;
                    break;
                default:
                    Debug.LogError("COLOR NOT FOUND");
                    break;
            }
        }

        public void StartShake()
        {
            m_PressTwice = false;
            m_IsShaking = true;
            DoShake();
        }

        void DoShake()
        {
            if (!m_IsShaking) return;
            transform.DOShakePosition(0.05f, 0.09f, 1, 90f, false, false)
                .SetEase(Ease.Linear) 
                .OnKill(DoShake);
        }

        void StopShake()
        {
            m_IsShaking = false;
            transform.position = m_InitPos;
        }
    }
}
