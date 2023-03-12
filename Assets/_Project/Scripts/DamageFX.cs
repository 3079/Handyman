using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts
{
    public class DamageFX : MonoBehaviour
    {
        private IDamageable _damageable;
        void Awake()
        {
            _damageable = GetComponentInParent<IDamageable>();
            _damageable.OnDamaged += GenerateFX;
        }

        private void GenerateFX(float damage)
        {
            
        }

        private void OnDisable()
        {
            _damageable.OnDamaged -= GenerateFX;
        }
    }
}
