using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;

public class SkullCtrl_1 : MonoBehaviour
{
    // Skull state
    public enum SkullState
    {
        None,
        Idle,
        Move,
        Wait,
        GoTarget,
        Atk,
        Damage,
        Die
    }

    [Header("Default Properties")]
    // skull default state
    public SkullState _skullState = SkullState.None;

    // skull move speed
    public float _spdMove = 1.0f;

    // Target seen by skull
    public GameObject _targetCharacter = null;

    // Target's transform seen by skull
    public Transform _targetTransform = null;

    // Target's position seen by skull
    public Vector3 _posTarget = Vector3.zero;

    // Ready to animation cashing
    public Animation _skullAnimation = null;

    // Ready to transform cashing
    public Transform _skullTransform = null;

    [Header("Animation clip")]
    public AnimationClip _idleAnimClip = null;
    public AnimationClip _moveAnimClip = null;
    public AnimationClip _atkAnimClip = null;
    public AnimationClip _damageAnimClip = null;
    public AnimationClip _dieAnimClip = null;

    [Header("Fight properties")]
    // skull HP
    public int _hp = 100;
    // skull attack range
    public float _atkRange = 1.5f;
    // skull is shot effect
    public GameObject _effectDamage = null;

    public GameObject _effectDie = null;

    Tweener effectTweener;
    private SkinnedMeshRenderer skinnedMeshRenderer = null;

    // Functions Called When Attack Animation Ends
    void OnAtkAnimationFinished()
    {
        Debug.Log("Atk animation finished");
    }

    // Function called at the end of hit animation
    void OnDamageAnimationFinished()
    {
        Debug.Log("Damage animation finished");
    }

    // Function Called When Death Animation Ends
    void OnDieAnimationFinished()
    {
        Debug.Log("Die animation finished");
    }

    /// <summary>
    /// Add animation event
    /// </summary>
    /// <param name="clip">Animation Clip</param>
    /// <param name="funcName">Animation call back function name</param>
    void OnAnimationEvent(AnimationClip clip, string funcName)
    {
        // make Animation Event
        AnimationEvent retEvent = new AnimationEvent();
        // Link animation event callback function.
        retEvent.functionName = funcName;
        // Call just before the end of the animation
        retEvent.time = clip.length - 0.1f;
        // Add animation information above
        clip.AddEvent(retEvent);
    }

    void Start()
    {

        // wait mode
        _skullState = SkullState.Idle;

        // cashing
        _skullAnimation = GetComponent<Animation>();
        _skullTransform = GetComponent<Transform>();

        // Animation setting
        _skullAnimation[_idleAnimClip.name].wrapMode = WrapMode.Loop;
        _skullAnimation[_moveAnimClip.name].wrapMode = WrapMode.Loop;

        _skullAnimation[_atkAnimClip.name].wrapMode = WrapMode.Once;
        // _skullAnimation[_damageAnimClip.name].wrapMode = WrapMode.Once;
        // _skullAnimation[_damageAnimClip.name].layer = 10;
        // _skullAnimation[_dieAnimClip.name].wrapMode = WrapMode.Once;
        // _skullAnimation[_dieAnimClip.name].layer = 10;

        // Add Animation Event
        OnAnimationEvent(_atkAnimClip, "OnAtkAnimationFinished");
        // OnAnimationEvent(_damageAnimClip, "OnDamageAnimationFinished");
        // OnAnimationEvent(_dieAnimClip, "OnDieAnimationFinished");
    }

    /// <summary>
    /// Functions that control behavior based on skeletal conditions
    /// </summary>
    void CkState()
    {
        switch (_skullState)
        {
            case SkullState.Idle:
                // Idle -> GoTarget or Move
                SetIdle();
                break;
            case SkullState.GoTarget:
            case SkullState.Move:
                SetMove();
                break;
            case SkullState.Atk:
                SetAtk();
                break;
            default:
                break;
        }
    }

    void Update()
    {
        CkState();
        AnimationCtrl();
    }

    /// <summary>
    /// Behavior when skeletal status is standby
    /// </summary>
    void SetIdle()
    {
        if (_targetCharacter == null)
        {
            // Random move target point
            _posTarget = new Vector3(_skullTransform.position.x + Random.Range(-10.0f, 10.0f), _skullTransform.position.y + 1000f, _skullTransform.position.z + Random.Range(-10.0f, 10.0f));
            // 레이케스트 시작점 목표 방향
            Ray ray = new Ray(_posTarget, Vector3.down);

            // 충돌체 정보를 담을 변수
            RaycastHit infoRayCast = new RaycastHit();

            // 만약 충돌체 있냐
            if (Physics.Raycast(ray, out infoRayCast, Mathf.Infinity))
            {
                // 임의의 목표 벡터에 높이 값 추가
                _posTarget.y = infoRayCast.point.y;
            }
            // 해골 상태를 Move로 변경
            _skullState = SkullState.Move;
        }
        else
        {
            // 해골 상태를 GoTarget로 변경
            _skullState = SkullState.GoTarget;
        }
    }

    /// <summary>
    /// Skull move function
    /// </summary>
    void SetMove()
    {
        // 출발점 도착점 두 벡터의 차이
        Vector3 distance = Vector3.zero;
        // 방향
        Vector3 posLookAt = Vector3.zero;

        // 해골 상태
        switch (_skullState)
        {
            case SkullState.Move:
                // 타겠이 없을때
                if (_posTarget != Vector3.zero)
                {
                    // 목표 위치와 해골 위치 차이 구하기
                    distance = _posTarget - _skullTransform.position;

                    if (distance.magnitude < _atkRange)
                    {
                        StartCoroutine(SetWait());
                        return;
                    }

                    // 바라보는 방향
                    posLookAt = new Vector3(_posTarget.x, _skullTransform.position.y, _posTarget.z);
                }
                break;
            case SkullState.GoTarget:
                if (_targetCharacter != null)
                {
                    // 타겟과 차이
                    distance = _targetCharacter.transform.position - _skullTransform.position;
                    if (distance.magnitude < _atkRange)
                    {
                        _skullState = SkullState.Atk;
                        return;
                    }
                    // 바라봐
                    posLookAt = new Vector3(_targetCharacter.transform.position.x, _skullTransform.position.y, _targetCharacter.transform.position.z);
                }
                break;
            default:
                break;
        }

        // 해골이 이동할 방향은 크기가 없고 방향만 가진 백터
        Vector3 direction = distance.normalized;

        // 방향은 x, z, y사용 안하는 이유
        direction = new Vector3(direction.x, 0, direction.z);

        // 이동량 방향 구함
        Vector3 amount = direction * _spdMove * Time.deltaTime;

        // 월드 좌표 이동
        _skullTransform.Translate(amount, Space.World);

        // 캐릭터 방향
        _skullTransform.LookAt(posLookAt);
    }

    /// <summary>
    /// 임이의 지역 이동 후 대기모드
    /// </summary>
    /// <returns></returns>
    IEnumerator SetWait()
    {
        // 해골 상태를 대기상태 변경
        _skullState = SkullState.Wait;
        // 대기하는 시간
        float timeWait = Random.Range(1.0f, 3.0f);
        // 대기 시간을 넣어줘야함
        yield return new WaitForSeconds(timeWait);
        // 대기한 다음 준비상태로
        _skullState = SkullState.Idle;
    }

    /// <summary>
    /// 애니메이션을 재생 시켜주는 함수
    /// </summary>
    void AnimationCtrl()
    {
        // 해골 상태에 따라서 애니메이션 적용
        switch (_skullState)
        {
            // 대기 애니메이션
            case SkullState.Wait:
            case SkullState.Idle:
                _skullAnimation.CrossFade(_idleAnimClip.name);
                break;
            // 이동 애니메이션
            case SkullState.GoTarget:
            case SkullState.Move:
                _skullAnimation.CrossFade(_moveAnimClip.name);
                break;
            // 공격 애니메이션
            case SkullState.Atk:
                _skullAnimation.CrossFade(_atkAnimClip.name);
                break;
            // 죽었을 때
            case SkullState.Die:
                _skullAnimation.CrossFade(_dieAnimClip.name);
                break;
            default:
                break;
        }
    }

    void OnCkTarget(GameObject target)
    {
        // 목표 캐릭터에 파라메터로 검출된 오브젝트 넣고
        _targetCharacter = target;
        _targetTransform = _targetCharacter.transform;
        _skullState = SkullState.GoTarget;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 플레이어가 공격한 경우
            _hp -= 10;
            if (_hp > 0)
            {
                Instantiate(_effectDamage, other.transform.position, Quaternion.identity);

                _skullAnimation.CrossFade(_damageAnimClip.name);

                effectDamageTween();
            }
            else
            {
                _skullState = SkullState.Die;
            }
        }
    }

    void effectDamageTween()
    {
        if (effectTweener != null && effectTweener.isComplete == false)
        {
            return;
        }
        Color colorTo = Color.red;

        effectTweener = HOTween.To(skinnedMeshRenderer, 0.2f, new TweenParms().Prop(
            "color", colorTo).Loops(1, LoopType.Yoyo).OnStepComplete(OnDamageTweenFinished));
    }

    void OnDamageTweenFinished()
    {

    }

    /// <summary>
    /// 해골 공격 모드
    /// </summary>
    void SetAtk()
    {
        float distance = Vector3.Distance(_targetTransform.position, _skullTransform.position);
        if (distance > _atkRange + 0.5f)
        {
            _skullState = SkullState.GoTarget;
        }
    }

    void SetDie()
    {
        int n = Random.Range(1, 100);
        if(n <= 10)
        {
            CreateItem();
        }
    }

    void CreateItem()
    {

    }
}
