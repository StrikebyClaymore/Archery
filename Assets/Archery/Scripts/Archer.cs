using Spine;
using Spine.Unity;
using UnityEngine;

public class Archer : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private SkeletonAnimation _skeletonAnimation;
    [SerializeField] private LineRenderer _shootStrengthLine;
    [SpineBone(dataField:"_skeletonAnimation")]
    [SerializeField] private string _aimBoneName;
    [SpineIkConstraint(dataField:"_skeletonAnimation")]
    [SerializeField] private string _armIKName;
    [SpineBone(dataField:"_skeletonAnimation")]
    [SerializeField] private string _arrowBoneName;
    [SpineAnimation(dataField:"_skeletonAnimation")]
    [SerializeField] private string _idleAnim;
    [SpineAnimation(dataField:"_skeletonAnimation")]
    [SerializeField] private string _targetAnim;
    [SpineAnimation(dataField:"_skeletonAnimation")]
    [SerializeField] private string _attackAnim;
    [SerializeField] private Arrow _arrow;
    [SerializeField] private GameObject _trajectoryObjectsParent;
    [SerializeField] private Transform[] _trajectoryObjects;
    [SerializeField] private int _shootStrengthMultiplier = 2;
    private const float Gravity = -9.8f;
    private Bone _aimBone;
    private Bone _arrowBone;
    private IkConstraint _armIK;
    private Vector3 _aimStartPosition;
    private Vector3 _aimDirection;
    private Vector3 _arrowSpawnPoint;
    private float _shootStrength;
    private bool _aiming;

    private void Start()
    {
        Skeleton skeleton = _skeletonAnimation.Skeleton;
        _aimBone = skeleton.FindBone(_aimBoneName);
        _arrowBone = skeleton.FindBone(_arrowBoneName);
        _armIK = skeleton.FindIkConstraint(_armIKName);
        _skeletonAnimation.AnimationName = _idleAnim;
        _shootStrengthLine.positionCount = 2;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            StartAim();
        else if (Input.GetMouseButtonUp(0))
            StopAim();
        Aim();
    }

    private void StartAim()
    {
        _skeletonAnimation.AnimationName = _targetAnim;
        _aimStartPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        _aimStartPosition.z = 0;
        _armIK.Mix = 1f;
        _shootStrength = 0;
        _shootStrengthLine.gameObject.SetActive(true);
        _trajectoryObjectsParent.SetActive(true);
        _aiming = true;
    }

    private void StopAim()
    {
        _trajectoryObjectsParent.SetActive(false);
        _shootStrengthLine.gameObject.SetActive(false);
        _shootStrengthLine.SetPosition(0, Vector3.zero);
        _shootStrengthLine.SetPosition(1, Vector3.zero);
        _armIK.Mix = 0f;
        _aiming = false;
        Shoot();
    }

    private void Shoot()
    {
        _skeletonAnimation.AnimationName = _attackAnim;
        _arrow.Launch(_arrowSpawnPoint, _aimDirection, _shootStrength);
        _skeletonAnimation.AnimationState.End += ShootCompleted;
    }
    
    private void ShootCompleted(TrackEntry track)
    {
        _skeletonAnimation.AnimationState.End -= ShootCompleted;
        _skeletonAnimation.AnimationName = _idleAnim;
    }

    private void Aim()
    {
        if(!_aiming)
            return;
        Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        worldPosition.z = 0;
        Vector2 offset = (_aimStartPosition - worldPosition);
        _aimDirection = offset.normalized;
        _arrowSpawnPoint = new Vector3(_arrowBone.WorldX, _arrowBone.WorldY, 0) + _skeletonAnimation.transform.position;
        _shootStrength = offset.magnitude * _shootStrengthMultiplier;
        AimBone(_aimBone, offset);
        DrawShootStrength(worldPosition);
        DrawTrajectory();
    }

    private void AimBone(Bone bone, Vector2 offset, float weight = 1f)
    {
        float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
        bone.Rotation = Mathf.Lerp(bone.Rotation, angle, weight);
    }

    private void DrawShootStrength(Vector2 position)
    {
        _shootStrengthLine.SetPosition(0, _aimStartPosition);
        _shootStrengthLine.SetPosition(1, position);
    }
    
    private void DrawTrajectory()
    {
        Vector2 velocity = _aimDirection * _shootStrength;
        for (int i = 0; i < _trajectoryObjects.Length; i++)
        {
            float t = (i + 1) / (float)_trajectoryObjects.Length;
            Vector3 point = _arrowSpawnPoint;
            point.x += velocity.x * t;
            point.y += velocity.y * t + 0.5f * Gravity * t * t;
            _trajectoryObjects[i].position = point;
        }
    }
}
