using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveHandToCursor : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _playerBody;
    [SerializeField] private Transform _forearmJoint;
    [SerializeField] private Transform _armJoint;
    [SerializeField] private float _force;
    [SerializeField] private float _speed;
    private float _maxArmLength;
    private float _forearmLength;
    private float _armLength;
    private Vector2 length;
    private Vector2 direction;
    private Vector2 oldPos;
    private Vector2 speed;
    private bool _colliding = false;

    private Rigidbody2D _rigidbody;
    // [SerializeField] private float _rotationSpeed = 10f;

    private float a;
    private float b;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        
        _forearmLength = (_armJoint.position - _forearmJoint.position).magnitude;
        _armLength = (_rigidbody.position - (Vector2)_armJoint.position).magnitude;
        _maxArmLength = _forearmLength + _armLength;

        a = _forearmLength;
        b = _armLength;
    }

    void Update()
    {
        var position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        length = position - _playerBody.position;
        length = Mathf.Clamp(length.magnitude, 0, _maxArmLength) * length.normalized;

        var rbpos = _rigidbody.position;
        direction = rbpos - _playerBody.position;

        speed = length - direction - oldPos;
        oldPos = length - direction;
        if (_colliding)
        {
            var dir = -speed;
            // var dir = -speed * Time.deltaTime;
            // Debug.Log(-speed);
            _playerBody.AddForce(dir * _force, ForceMode2D.Impulse);
        }

        var theta = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        var c = (direction).magnitude;
        var cosAlpha = Mathf.Clamp((b * b + c * c - a * a) / (2 * b * c), -1, 1);
        var cosBeta = Mathf.Clamp((a * a + c * c - b * b) / (2 * a * c), -1, 1);
        var alpha = Mathf.Acos(cosAlpha) * Mathf.Rad2Deg;
        var beta = Mathf.Acos(cosBeta) * Mathf.Rad2Deg;

        var forearmRotation = Quaternion.AngleAxis(theta + beta, Vector3.forward);
        var armRotation = Quaternion.AngleAxis(theta - alpha, Vector3.forward);

        _forearmJoint.rotation = forearmRotation;
        _armJoint.rotation = armRotation;
    }

    private void FixedUpdate()
    {
        _rigidbody.MovePosition(_playerBody.position + length);
        // _rigidbody.velocity = Vector3.Slerp(_rigidbody.position, length - direction, Time.fixedDeltaTime) * _speed * Time.deltaTime;
        // _rigidbody.MovePosition(Vector3.Slerp(_rigidbody.position, _playerBody.position + length, Time.fixedTime));
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        _colliding = true;
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        _colliding = true;
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        _colliding = false;
    }

    private void OnDrawGizmos()
    {
        // Gizmos.color = Color.red;
        // Gizmos.DrawLine(_playerBody.position, _playerBody.position + length);
        // Gizmos.color = Color.green;
        // Gizmos.DrawLine(_playerBody.position, _rigidbody.position);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_playerBody.position, _playerBody.position - speed );
    }
}
