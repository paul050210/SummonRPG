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
            // �����ɽ�Ʈ ������ ��ǥ ����
            Ray ray = new Ray(_posTarget, Vector3.down);

            // �浹ü ������ ���� ����
            RaycastHit infoRayCast = new RaycastHit();

            // ���� �浹ü �ֳ�
            if (Physics.Raycast(ray, out infoRayCast, Mathf.Infinity))
            {
                // ������ ��ǥ ���Ϳ� ���� �� �߰�
                _posTarget.y = infoRayCast.point.y;
            }
            // �ذ� ���¸� Move�� ����
            _skullState = SkullState.Move;
        }
        else
        {
            // �ذ� ���¸� GoTarget�� ����
            _skullState = SkullState.GoTarget;
        }
    }

    /// <summary>
    /// Skull move function
    /// </summary>
    void SetMove()
    {
        // ����� ������ �� ������ ����
        Vector3 distance = Vector3.zero;
        // ����
        Vector3 posLookAt = Vector3.zero;

        // �ذ� ����
        switch (_skullState)
        {
            case SkullState.Move:
                // Ÿ���� ������
                if (_posTarget != Vector3.zero)
                {
                    // ��ǥ ��ġ�� �ذ� ��ġ ���� ���ϱ�
                    distance = _posTarget - _skullTransform.position;

                    if (distance.magnitude < _atkRange)
                    {
                        StartCoroutine(SetWait());
                        return;
                    }

                    // �ٶ󺸴� ����
                    posLookAt = new Vector3(_posTarget.x, _skullTransform.position.y, _posTarget.z);
                }
                break;
            case SkullState.GoTarget:
                if (_targetCharacter != null)
                {
                    // Ÿ�ٰ� ����
                    distance = _targetCharacter.transform.position - _skullTransform.position;
                    if (distance.magnitude < _atkRange)
                    {
                        _skullState = SkullState.Atk;
                        return;
                    }
                    // �ٶ��
                    posLookAt = new Vector3(_targetCharacter.transform.position.x, _skullTransform.position.y, _targetCharacter.transform.position.z);
                }
                break;
            default:
                break;
        }

        // �ذ��� �̵��� ������ ũ�Ⱑ ���� ���⸸ ���� ����
        Vector3 direction = distance.normalized;

        // ������ x, z, y��� ���ϴ� ����
        direction = new Vector3(direction.x, 0, direction.z);

        // �̵��� ���� ����
        Vector3 amount = direction * _spdMove * Time.deltaTime;

        // ���� ��ǥ �̵�
        _skullTransform.Translate(amount, Space.World);

        // ĳ���� ����
        _skullTransform.LookAt(posLookAt);
    }

    /// <summary>
    /// ������ ���� �̵� �� �����
    /// </summary>
    /// <returns></returns>
    IEnumerator SetWait()
    {
        // �ذ� ���¸� ������ ����
        _skullState = SkullState.Wait;
        // ����ϴ� �ð�
        float timeWait = Random.Range(1.0f, 3.0f);
        // ��� �ð��� �־������
        yield return new WaitForSeconds(timeWait);
        // ����� ���� �غ���·�
        _skullState = SkullState.Idle;
    }

    /// <summary>
    /// �ִϸ��̼��� ��� �����ִ� �Լ�
    /// </summary>
    void AnimationCtrl()
    {
        // �ذ� ���¿� ���� �ִϸ��̼� ����
        switch (_skullState)
        {
            // ��� �ִϸ��̼�
            case SkullState.Wait:
            case SkullState.Idle:
                _skullAnimation.CrossFade(_idleAnimClip.name);
                break;
            // �̵� �ִϸ��̼�
            case SkullState.GoTarget:
            case SkullState.Move:
                _skullAnimation.CrossFade(_moveAnimClip.name);
                break;
            // ���� �ִϸ��̼�
            case SkullState.Atk:
                _skullAnimation.CrossFade(_atkAnimClip.name);
                break;
            // �׾��� ��
            case SkullState.Die:
                _skullAnimation.CrossFade(_dieAnimClip.name);
                break;
            default:
                break;
        }
    }

    void OnCkTarget(GameObject target)
    {
        // ��ǥ ĳ���Ϳ� �Ķ���ͷ� ����� ������Ʈ �ְ�
        _targetCharacter = target;
        _targetTransform = _targetCharacter.transform;
        _skullState = SkullState.GoTarget;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // �÷��̾ ������ ���
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
    /// �ذ� ���� ���
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
