using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts
{
    public abstract class Creature : MonoBehaviour, IDamageable
    {
        [SerializeField] protected float health;
        public event Action<float> OnDamaged;
        public event Action OnDeath;
        protected float _health;

        protected void OnEnable()
        {
            OnDamaged += TakeDamage;
            OnDeath += Die;
        }

        protected void Awake()
        {
            _health = health;
        }

        public void TakeDamage(float damage)
        {
            _health -= damage;
            if (_health <= 0)
            {
                OnDeath?.Invoke();
            }
        }

        protected void Die()
        {
            Destroy(gameObject);
        }
        
        protected void OnDisable()
        {
            OnDamaged -= TakeDamage;
            OnDeath -= Die;
        }
    }
}
