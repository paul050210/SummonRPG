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
        // ����� ������ �� ������ ����
        Vector3 distance = Vector3.zero;
        // ����
        Vector3 posLookAt = Vector3.zero;

        // �ذ� ����
        switch (_summonState)
        {
            case SummonState.Move:
                // Ÿ���� ������
                if (_posTarget != Vector3.zero)
                {
                    // ��ǥ ��ġ�� �ذ� ��ġ ���� ���ϱ�
                    distance = _posTarget - transform.position;

                    if (distance.magnitude < _atkRange)
                    {
                        StartCoroutine(SetWait());
                        return;
                    }

                    // �ٶ󺸴� ����
                    posLookAt = new Vector3(_posTarget.x, _summonTransform.position.y, _posTarget.z);
                }
                break;
            case SummonState.GoTarget:
                if (_targetCharacter != null)
                {
                    // Ÿ�ٰ� ����
                    distance = _targetCharacter.transform.position - _summonTransform.position;
                    if (distance.magnitude < _atkRange)
                    {
                        _summonState = SummonState.Atk;
                        return;
                    }
                    // �ٶ��
                    posLookAt = new Vector3(_targetCharacter.transform.position.x, _summonTransform.position.y, _targetCharacter.transform.position.z);
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
        _summonTransform.Translate(amount, Space.World);

        // ĳ���� ����
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