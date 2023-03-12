using System;

namespace _Project.Scripts
{
    public interface IDamageable
    {
        public event Action<float> OnDamaged; 
        public void TakeDamage(float damage);
    }
}
