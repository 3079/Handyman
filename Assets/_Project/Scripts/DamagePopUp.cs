using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _Project.Scripts
{
    public class DamagePopUp : MonoBehaviour
    {
        [SerializeField] private Color color;
        [SerializeField] [Range(0f, 10f)] private float timer;
        [SerializeField] private float speed;
        private Vector2 _direction;
        private float _time;
        private TMP_Text text;

        void Awake()
        {
            text = gameObject.GetComponent<TMP_Text>();
            text.color = color;
            _time = timer;
        }

        void Update()
        {
            _time -= Time.deltaTime;
            transform.position += (Vector3) (speed * Time.deltaTime * _direction);
            var k = Mathf.Max(_time / timer, 0);
            Color c = text.color;
            c.a = k;
            text.color = c;
            c = text.outlineColor;
            c.a = k;
            text.outlineColor = c;
            if (_time <= 0)
                Destroy(gameObject);
        }

        public void SetDirection(Vector2 direction)
        {
            _direction = direction.normalized;
        }
    }
}
