using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace _Project.Scripts
{
    public class MoveHandToCursor : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _playerBody;
        [SerializeField] private Transform _forearmJoint;
        [SerializeField] private Transform _armJoint;
        [SerializeField] private Collider2D _handCollider;
        [SerializeField] private Collider2D _handTrigger;
        [SerializeField] private ContactFilter2D _interactibleFilter;
        [SerializeField] [Range(0, 10000f)] private float _handSpeed;
        [SerializeField] [Range(0, 10000f)] private float _forceMultiplier;
        [SerializeField] [Range(0f, 2f)] private float _horizontalForceModifier;
        [SerializeField] [Range(0f, 2f)] private float _verticalForceModifier;
        [SerializeField] private Sprite _palmOpen;
        [SerializeField] private Sprite _palmClosed;
        private Rigidbody2D _rigidbody;
        private SpriteRenderer _spriteRenderer;
        private float _maxArmLength;
        private float _forearmLength;
        private float _armLength;
        private float a;
        private float b;
        private Vector2 _bodyToMouse;
        private Vector2 _bodyToHand;
        private Vector2 _handToMouse;
        private bool _isColliding;

        private bool _isHoldingObject;
        private Rigidbody2D _heldObjectRb;
        private FixedJoint2D _heldObjectFixedJoint;
        private HingeJoint2D _heldObjectHingeJoint;
        private AnchoredJoint2D _heldObjectJoint;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();

            _forearmLength = (_armJoint.position - _forearmJoint.position).magnitude;
            _armLength = (_rigidbody.position - (Vector2) _armJoint.position).magnitude;
            _maxArmLength = _forearmLength + _armLength;

            a = _forearmLength;
            b = _armLength;

            _heldObjectFixedJoint = GetComponent<FixedJoint2D>();
            _heldObjectHingeJoint = GetComponent<HingeJoint2D>();
            // _heldObjectJoint = GetComponent<HingeJoint2D>();
            _heldObjectFixedJoint.enabled = false;
            _heldObjectHingeJoint.enabled = false;
            _heldObjectFixedJoint.connectedBody = null;
            _heldObjectHingeJoint.connectedBody = null;

            _spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        }

        void Update()
        {
            CalculateDistances();

            CalculateInverseKinematics();

            // Debug.Log(_handToMouse.magnitude);
            // if (_handToMouse.magnitude <= 0.05f || _rigidbody.velocity.magnitude <= 0.05f)
            // {
                // _rigidbody.MovePosition(_playerBody.position + _bodyToMouse);
                // _rigidbody.velocity = Vector3.zero;
            // }

            if (Input.GetMouseButton(0))
            {
                if (!_isHoldingObject)
                {
                    List<Collider2D> contacts = new List<Collider2D>();
                    _handTrigger.OverlapCollider(_interactibleFilter, contacts);
                    contacts = contacts.Where(x => x.gameObject != gameObject).ToList();
                    if (contacts.Count != 0)
                    {
                        var heldObject = contacts[0].gameObject;
                        _heldObjectRb = heldObject.GetComponentInParent<Rigidbody2D>();
                        _heldObjectJoint = _heldObjectRb.mass >= _playerBody.mass
                            ? _heldObjectHingeJoint
                            : _heldObjectFixedJoint;
                        _heldObjectJoint.enabled = true;
                        _heldObjectJoint.connectedBody = _heldObjectRb;
                        _isHoldingObject = true;
                        _heldObjectJoint.autoConfigureConnectedAnchor = true;
                        _rigidbody.gravityScale = 0;
                        var interactable = heldObject.GetComponentInParent<IInteractable>();
                        if (interactable != null)
                        {
                            interactable.Interact();
                            interactable.OnDestroyed += ClearHeldObject;
                        }
                    }
                    else
                    {
                        _rigidbody.gravityScale = 1;
                    }
                }
                else
                {
                    _heldObjectJoint.autoConfigureConnectedAnchor = false;
                }
                _spriteRenderer.sprite = _palmClosed;
            }
            else
            {
                _rigidbody.gravityScale = 1;
                _heldObjectRb = null;
                if (_heldObjectJoint != null)
                {
                    _heldObjectJoint.connectedBody = null;
                    _heldObjectJoint.enabled = false;
                    // _heldObjectJoint = null;
                }
                _isHoldingObject = false;
                _spriteRenderer.sprite = _palmOpen;
            }
        }

        private void CalculateDistances()
        {
            var position = (Vector2) Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _bodyToMouse = position - _playerBody.position;
            _bodyToMouse = Mathf.Clamp(_bodyToMouse.magnitude, 0, _maxArmLength) * _bodyToMouse.normalized;

            var rbpos = _rigidbody.position;
            _bodyToHand = rbpos - _playerBody.position;
            _handToMouse = _bodyToMouse - _bodyToHand;
            
            // var gamma = 180 - alpha - beta;
            // var springForce = gamma - oldElbowAngle;
            // oldElbowAngle = gamma;
        }

        private void CalculateInverseKinematics()
        {
            var theta = Mathf.Atan2(_bodyToHand.y, _bodyToHand.x) * Mathf.Rad2Deg;

            var c = _bodyToHand.magnitude;
            var cosAlpha = Mathf.Clamp((b * b + c * c - a * a) / (2 * b * c), -1, 1);
            var cosBeta = Mathf.Clamp((a * a + c * c - b * b) / (2 * a * c), -1, 1);
            var alpha = Mathf.Acos(cosAlpha) * Mathf.Rad2Deg;
            var beta = Mathf.Acos(cosBeta) * Mathf.Rad2Deg;

            var forearmRotation = Quaternion.AngleAxis(theta + beta, Vector3.forward);
            var armRotation = Quaternion.AngleAxis(theta - alpha, Vector3.forward);

            transform.rotation = Quaternion.AngleAxis(theta, Vector3.forward);
            _forearmJoint.rotation = forearmRotation;
            _armJoint.rotation = armRotation;
        }

        private void ApplyForces()
        {
            var normalizedDirection = -_handToMouse.normalized;
            var linearForce = _forceMultiplier * Mathf.Clamp(_handToMouse.magnitude, 0, _maxArmLength) *
                              normalizedDirection;
            var verticalForce = linearForce.y * _verticalForceModifier;
            var horizontalForce = linearForce.x * _horizontalForceModifier;
            var force = verticalForce * Vector2.up + horizontalForce * Vector2.right;

            _playerBody.AddForce(force * Time.fixedDeltaTime, ForceMode2D.Force);

            if (_isHoldingObject)
            {
                _heldObjectRb.AddForce(-force * Time.fixedDeltaTime, ForceMode2D.Force);
                // _rigidbody.AddForce((_rigidbody.mass / _rigidbody.mass + _heldObjectRb.mass) * _handSpeed * Time.fixedDeltaTime * _handToMouse.normalized, ForceMode2D.Force);
                _rigidbody.AddForce(_rigidbody.mass / (_rigidbody.mass + _heldObjectRb.mass) * _handSpeed * Time.fixedDeltaTime * _handToMouse.normalized, ForceMode2D.Force);
                // _heldObjectRb.MovePosition(_rigidbody.position);
                // _heldObjectJoint.anchor = transform.InverseTransformPoint(transform.position);
            }
        }

        private void FixedUpdate()
        {
            // if (_handToMouse.magnitude <= 0.01f) return;
            // _rigidbody.AddForce(_handSpeed * Time.fixedDeltaTime * _handToMouse, ForceMode2D.Force);
            if (!_isHoldingObject) _rigidbody.MovePosition(_playerBody.position + _bodyToMouse); // allows sliding when contacting objects
            if (_isColliding || _isHoldingObject) ApplyForces();
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            _isColliding = true;
            var dot = Mathf.Max(Vector2.Dot(Vector2.up, col.GetContact(0).normal), 0f);
            _playerBody.velocity *= 1 - dot;
            _playerBody.gravityScale = 1 - dot;
        }

        private void OnCollisionStay2D(Collision2D col)
        {
            _isColliding = true;
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            _isColliding = false;
            _playerBody.gravityScale = 1;
        }

        private void ClearHeldObject(IInteractable interactable)
        {
            if (_isHoldingObject)
            {
                interactable.OnDestroyed -= ClearHeldObject;
                _rigidbody.gravityScale = 1;
                
                _heldObjectRb = null;
                if (_heldObjectJoint != null)
                {
                    _heldObjectJoint.connectedBody = null;
                    _heldObjectJoint.enabled = false;
                }
                _isHoldingObject = false;
            }
        }

        private void OnDrawGizmos()
        {
            // Gizmos.color = Color.red;
            // Gizmos.DrawSphere((Vector2) Camera.main.ScreenToWorldPoint(Input.mousePosition), 0.1f);
            // Gizmos.DrawLine(_playerBody.position, _playerBody.position + _bodyToMouse);
            // Gizmos.color = Color.green;
            // Gizmos.DrawLine(_playerBody.position, _rigidbody.position);
            // Gizmos.color = Color.blue;
            // Gizmos.DrawLine(_playerBody.position, _playerBody.position - _handToMouse);
        }
    }
}