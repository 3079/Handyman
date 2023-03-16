using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;


namespace _Project.Scripts
{
    public class DamageFX : MonoBehaviour
    {
        [SerializeField] private GameObject damagePopUp;
        private IDamageable _damageable;
        void Awake()
        {
            _damageable = GetComponentInParent<IDamageable>();
            _damageable.OnDamaged += GenerateFX;
        }

        private void GenerateFX(float damage)
        {
            var damageRounded = Mathf.RoundToInt(damage);
            if (damageRounded == 0) return;
            var dir = new Vector3(Random.value, Random.value, Random.value);
            var obj = Instantiate(damagePopUp, transform.position + dir.normalized, Quaternion.identity);
            obj.GetComponent<TMP_Text>().text = damageRounded.ToString();
            obj.GetComponent<DamagePopUp>().SetDirection(dir);
        }

        private void OnDisable()
        {
            _damageable.OnDamaged -= GenerateFX;
        }
    }
}
