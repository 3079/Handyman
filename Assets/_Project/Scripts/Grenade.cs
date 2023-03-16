using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


using UnityEngine;

namespace _Project.Scripts
{
    public class Grenade : MonoBehaviour, IInteractable
    {
        [SerializeField] [Range(0, 10f)] private float timer;
        [SerializeField] [Range(0, 100f)] private float radius;
        [SerializeField] [Range(0, 100f)] private float maxDamage;
        [SerializeField] [Range(0, 1000f)] private float blastForce;
        [SerializeField] ContactFilter2D blastContactFilter;
        [SerializeField] private CircleCollider2D blastZone;
        [SerializeField] private GameObject explosion;
        public event Action<IInteractable> OnDestroyed;
        private bool _isTriggered = false;

        void Awake()
        {
            blastZone.radius = radius;
        }
        
        public void Interact()
        {
            if (_isTriggered) return;
                
            _isTriggered = true;
            StartCoroutine(Trigger());
        }

        private IEnumerator Trigger()
        {
            AudioManager.instance.PlaySFX("pin");
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                yield return null;
            }
            AudioManager.instance.PlaySFX("explosion");

            var contacts = new List<Collider2D>();
            blastZone.OverlapCollider(blastContactFilter, contacts);
            contacts = contacts.Where(x => x.gameObject != gameObject).ToList();
            foreach (var contact in contacts)       
            {
                var obj = contact.gameObject;
                var distance = obj.transform.position - transform.position;
                var distanceCoefficient = Mathf.Max(radius - distance.magnitude, 0) / radius;
                
                var damageable = obj.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(maxDamage * distanceCoefficient);
                }

                var damageableRb = obj.GetComponentInParent<Rigidbody2D>();
                if (damageableRb != null)
                {
                    damageableRb.AddForce(distanceCoefficient * blastForce * distance.normalized, ForceMode2D.Impulse);
                }
            }
            Instantiate(explosion, transform.position, Quaternion.identity);
            OnDestroyed?.Invoke(this);
            
            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _isTriggered ? Color.cyan : Color.gray;
            Gizmos.DrawWireSphere(transform.position, radius / 2f);
        }
    }
}

