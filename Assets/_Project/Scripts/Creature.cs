using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts
{
    public abstract class Creature : MonoBehaviour, IDamageable
    {
        [SerializeField] private float health;
        private float _health;

        protected void Awake()
        {
            _health = health;
        }

        public void TakeDamage(float damage)
        {
            _health -= damage;
            if (_health <= 0)
            {
                OnDeath();
            }
        }

        protected void OnDeath()
        {
            Destroy(gameObject);
        }
    }
}
