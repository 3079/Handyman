using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Scripts;
using UnityEngine;

namespace _Project.Scripts
{
    public class GrenadeSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject grenade;
        [SerializeField] private GameObject grenadePrefab;
        private Grenade _grenade;

        private void Awake()
        {
            _grenade = grenade.GetComponent<Grenade>();
            _grenade.OnDestroyed += SpawnGrenade;
        }

        private void SpawnGrenade(IInteractable interactable)
        {
            interactable.OnDestroyed -= SpawnGrenade;
            var newGrenade = Instantiate(grenadePrefab, transform.position, Quaternion.identity);
            newGrenade.GetComponent<Grenade>().OnDestroyed += SpawnGrenade;
        }
    }
}
