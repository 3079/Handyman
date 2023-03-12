using System;
using System.Collections;
using System.Collections.Generic;
using Codice.Client.BaseCommands;
using UnityEngine;

namespace _Project.Scripts
{
    public class Grenade : MonoBehaviour, IInteractable
    {
        [SerializeField] [Range(0, 10f)] private float timer;
        [SerializeField] [Range(0, 100f)] private float radius;
        [SerializeField] [Range(0, 100f)] private float maxDamage;
        [SerializeField] [Range(0, 1000f)] private float blastForce;
        [SerializeField]  ContactFilter2D blastContactFilter;
        private bool _isTriggered = false;
        private Collider2D _blastZone;

        void Awake()
        {
            _blastZone = gameObject.GetComponent<Collider2D>();
        }
        
        public void Interact()
        {
            if (_isTriggered) return;
                
            _isTriggered = true;
            StartCoroutine(Trigger());
        }

        private IEnumerator Trigger()
        {
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                yield return null;
            }

            var contacts = new List<Collider2D>();
            _blastZone.OverlapCollider(blastContactFilter, contacts);
            foreach (var contact in contacts)
            {
                var damageableObj = contact.gameObject;
                var damageable = damageableObj.GetComponent<IDamageable>();
                if (damageable == null) continue;
                var distance = damageableObj.transform.position - transform.position;
                var distanceCoefficient = Mathf.Min(radius - distance.magnitude, 0) / radius;
                damageable.TakeDamage(maxDamage * distanceCoefficient);
                
                var damageableRb = damageableObj.GetComponent<Rigidbody2D>();
                if (damageableRb == null) yield break;
                damageableRb.AddForce(distanceCoefficient * blastForce * distance.normalized, ForceMode2D.Impulse);
                Debug.Log(distanceCoefficient * blastForce * distance.normalized);
            }
            
            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _isTriggered ? Color.cyan : Color.gray;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}

