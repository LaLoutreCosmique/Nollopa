using System;
using TMPro;
using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;

namespace Script
{
    public class KeyFX : MonoBehaviour
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Sprite upSprite;
        [SerializeField] Sprite downSprite;
        [SerializeField] TextMeshPro charSlot;
        [SerializeField] float animDuration;
        [SerializeField] ParticleSystem destroyParticle;

        const float PressedOffset = -0.14f;
        
        public KeyFX Init(char character, Vector3 endAnim)
        {
            charSlot.text = character.ToString();
            transform.DOMove(endAnim, animDuration);
            return this;
        }

        public void Press()
        {
            charSlot.rectTransform.localPosition = new Vector3(0, PressedOffset, -0.05f);
            charSlot.alpha = 0.3f;
            spriteRenderer.sprite = downSprite;
        }

        public void Release()
        {
            charSlot.rectTransform.localPosition = new Vector3(0, 0, -0.05f);
            charSlot.alpha = 1f;
            spriteRenderer.sprite = upSprite;
        }

        void OnDestroy()
        {
            Instantiate(destroyParticle, transform.position, quaternion.identity);
        }
    }
}
