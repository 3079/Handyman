using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts
{
    public class ExplosionFX : MonoBehaviour
    {
        [SerializeField] [Range(0f, 10f)] private float timer;
        [SerializeField] [Range(0f, 10f)] private float scaleSpeed;
        private float _time;
        private SpriteRenderer _spriteRenderer;

        void Awake()
        {
            _time = timer;
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            _time -= Time.deltaTime;
            transform.localScale += Vector3.one * (scaleSpeed * Time.deltaTime);
            var k = Mathf.Max(_time / timer, 0);
            var c = _spriteRenderer.color;
            c.a = k;
            _spriteRenderer.color = c;

            if (_time <= 0)
                Destroy(gameObject);
        }
    }
}
