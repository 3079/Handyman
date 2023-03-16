using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts
{
    public class Sword : MonoBehaviour
    {
        [SerializeField] [Range(0f, 100f)] private float maxSpeedModifier;
        [SerializeField] [Range(0f, 100f)] private float baseDamage;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = gameObject.GetComponentInParent<Rigidbody2D>();
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            var obj = col.gameObject;
            var damageable = obj.GetComponent<IDamageable>();
            var damageableRb = obj.GetComponent<Rigidbody2D>();

            if (damageableRb == null || damageable == null) return;

            var relativeSpeed = _rb.velocity - damageableRb.velocity;
            var speedModifier = Mathf.Clamp(relativeSpeed.magnitude, 0f, maxSpeedModifier);

            var damage = speedModifier * baseDamage;
            
            damageable.TakeDamage(damage);
        }
    }
}
