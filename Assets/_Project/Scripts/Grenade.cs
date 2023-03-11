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
        private Collider2D _blastZone;

        void Awake()
        {
            _blastZone = gameObject.GetComponent<Collider2D>();
        }
        
        public void Interact()
        {
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
                // var damageable = contact.gameObject.GetComponent<IDamageable>();
                // if (damageable == null) continue;
                // var distance = damageable.gameObject.transform.position - transform.position;
                // damageable.TakeDamage(maxDamage * Mathf.Min(radius - distance, 0) / radius)
            }
            
            Destroy(gameObject);
        }
    }
}

