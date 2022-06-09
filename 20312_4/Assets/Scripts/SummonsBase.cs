using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SummonsBase : MonoBehaviour
{
    public enum SummonState
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

    public SummonState _summonState = SummonState.None;
    public float _spdMove = 1.0f;
    public GameObject _targetCharacter = null;
    public GameObject _playerCharacter = null;
    public Transform _targetTransform = null;
    public Transform _playerTransform = null;
    public Vector3 _posTarget = Vector3.zero;
    public Transform _summonTransform = null;

    [Header("Fight properties")]
    public int _hp = 100;
    public float _atkRange = 1.5f;
    public GameObject _effectDamage = null;

    public GameObject _effectDie = null;

    void CkState()
    {
        switch(_summonState)
        {
            case SummonState.Idle:
                SetIdle();
                break;
            case SummonState.GoTarget:
            case SummonState.Move:
                SetMove();
                break;
            case SummonState.Atk:
                SetAtk();
                break;
            default:
                break;
        }
    }

    void SetIdle()
    {
        if(_targetCharacter == null)
        {
            Ray ray = new Ray(_posTarget, Vector3.down);

            RaycastHit infoRayCast = new RaycastHit();

            if(Physics.Raycast(ray, out infoRayCast, Mathf.Infinity))
            {
                _posTarget.y = infoRayCast.point.y;
            }

            _summonState = SummonState.Move;
        }
        else
        {
            _summonState = SummonState.GoTarget;
        }
    }

    void SetMove()
    {
        // 출발점 도착점 두 벡터의 차이
        Vector3 distance = Vector3.zero;
        // 방향
        Vector3 posLookAt = Vector3.zero;

        // 해골 상태
        switch (_summonState)
        {
            case SummonState.Move:
                // 타겠이 없을때
                if (_posTarget != Vector3.zero)
                {
                    // 목표 위치와 해골 위치 차이 구하기
                    distance = _posTarget - transform.position;

                    if (distance.magnitude < _atkRange)
                    {
                        StartCoroutine(SetWait());
                        return;
                    }

                    // 바라보는 방향
                    posLookAt = new Vector3(_posTarget.x, _summonTransform.position.y, _posTarget.z);
                }
                break;
            case SummonState.GoTarget:
                if (_targetCharacter != null)
                {
                    // 타겟과 차이
                    distance = _targetCharacter.transform.position - _summonTransform.position;
                    if (distance.magnitude < _atkRange)
                    {
                        _summonState = SummonState.Atk;
                        return;
                    }
                    // 바라봐
                    posLookAt = new Vector3(_targetCharacter.transform.position.x, _summonTransform.position.y, _targetCharacter.transform.position.z);
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
        _summonTransform.Translate(amount, Space.World);

        // 캐릭터 방향
        _summonTransform.LookAt(posLookAt);
    }

    void SetAtk()
    {

    }

    IEnumerator SetWait()
    {
        yield return new WaitForSeconds(1f);
    }
}