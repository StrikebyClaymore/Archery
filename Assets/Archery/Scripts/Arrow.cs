using Spine;
using Spine.Unity;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private SkeletonAnimation _skeletonAnimation;
    [SpineAnimation(dataField:"_skeletonAnimation")]
    [SerializeField] private string _idleAnim;
    [SpineAnimation(dataField:"_skeletonAnimation")]
    [SerializeField] private string _attackAnim;
    [SerializeField] private float _rotationSpeed = 10f;

    public void Launch(Vector3 position, Vector3 direction, float force)
    {
        transform.position = position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        _skeletonAnimation.AnimationName = _idleAnim;
        _rb.isKinematic = false;
        _rb.velocity = Vector2.zero;
        _rb.angularVelocity = 0;
        gameObject.SetActive(true);
        Vector2 velocity = direction * force;
        _rb.velocity = velocity;
    }

    private void FixedUpdate()
    {
        if (!_rb.isKinematic && _rb.velocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(_rb.velocity.y, _rb.velocity.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.fixedDeltaTime * _rotationSpeed);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        _rb.isKinematic = true;
        _rb.velocity = Vector2.zero;
        _rb.angularVelocity = 0;
        Attack();
    }

    private void Attack()
    {
        _skeletonAnimation.AnimationName = _attackAnim;
        _skeletonAnimation.AnimationState.End += AttackCompleted;
    }
    
    private void AttackCompleted(TrackEntry track)
    {
        _skeletonAnimation.AnimationState.End -= AttackCompleted;
        _skeletonAnimation.AnimationName = _idleAnim;
        gameObject.SetActive(false);
    }
}