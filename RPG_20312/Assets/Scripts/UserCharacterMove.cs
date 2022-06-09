using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserCharacterMove : MonoBehaviour
{
    //ĳ���� ���� �̵� �ӵ� (�ȱ�)
    public float walkMoveSpd = 2.0f;

    //ĳ���� ���� �̵� �ӵ� (�޸���)
    public float runMoveSpd = 3.5f;

    //ĳ���� ȸ�� �̵� �ӵ� 
    public float rotateMoveSpd = 100.0f;

    //ĳ���� ȸ�� �������� ���� ������ �ӵ�
    public float rotateBodySpd = 2.0f;

    //ĳ���� �̵� �ӵ� ���� ��
    public float moveChageSpd = 0.1f;

    //���� ĳ���� �̵� ���� �� 
    private Vector3 vecNowVelocity = Vector3.zero;

    //���� ĳ���� �̵� ���� ���� 
    private Vector3 vecMoveDirection = Vector3.zero;

    //CharacterController ĳ�� �غ�
    private CharacterController controllerCharacter = null;

    //ĳ���� CollisionFlags �ʱⰪ ����
    private CollisionFlags collisionFlagsCharacter = CollisionFlags.None;

    //ĳ���� �߷°�
    private float gravity = 9.8f;

    //ĳ���� �߷� �ӵ� ��
    private float verticalSpd = 0f;

    //ĳ���� ���� ���� �÷���
    private bool stopMove = false;

    [Header("�ִϸ��̼� �Ӽ�")]
    public AnimationClip animationClipIdle = null;
    public AnimationClip animationClipWalk = null;
    public AnimationClip animationClipRun = null;
    public AnimationClip animationClipAtkStep_1 = null;
    public AnimationClip animationClipAtkStep_2 = null;
    public AnimationClip animationClipAtkStep_3 = null;
    public AnimationClip animationClipAtkStep_4 = null;

    //������Ʈ�� �ʿ��մϴ� 
    private Animation animationPlayer = null;


    //ĳ���� ����  ĳ���� ���¿� ���� animation�� ǥ��
    public enum PlayerState { None, Idle, Walk, Run, Attack, Skill }

    [Header("ĳ���ͻ���")]
    public PlayerState playerState = PlayerState.None;

    //���� sub state �߰� 
    public enum PlayerAttackState { atkStep_1, atkStep_2, atkStep_3, atkStep_4 }

    //�⺻ ���� ���� �� �߰� 
    public PlayerAttackState playerAttackState = PlayerAttackState.atkStep_1;

    //���� ���� ���� Ȱ��ȭ�� ���� flag
    public bool flagNextAttack = false;

    [Header("��������")]
    //������ ���� ������
    public TrailRenderer AtkTrailRenderer = null;
    //���⿡ �ִ� �ݶ��̴� ĳ��
    public CapsuleCollider AtkCollider = null;

    [Header("��ų����")]
    public AnimationClip skillAnimClip = null;
    public GameObject skillEffect = null;

    // Start is called before the first frame update
    void Start()
    {
        //CharacterController ĳ��
        controllerCharacter = GetComponent<CharacterController>();

        //Animation component ĳ��
        animationPlayer = GetComponent<Animation>();
        //Animation Component �ڵ� ��� ����
        animationPlayer.playAutomatically = false;
        //Ȥ�ó� ������� Animation �ִٸ�? ���߱�
        animationPlayer.Stop();

        //�ʱ� �ִϸ��̼��� ���� Enum
        playerState = PlayerState.Idle;

        //animation WrapMode : ��� ��� ���� 
        animationPlayer[animationClipIdle.name].wrapMode = WrapMode.Loop;
        animationPlayer[animationClipWalk.name].wrapMode = WrapMode.Loop;
        animationPlayer[animationClipWalk.name].wrapMode = WrapMode.Loop;
        animationPlayer[animationClipAtkStep_1.name].wrapMode = WrapMode.Once;
        animationPlayer[animationClipAtkStep_2.name].wrapMode = WrapMode.Once;
        animationPlayer[animationClipAtkStep_3.name].wrapMode = WrapMode.Once;
        animationPlayer[animationClipAtkStep_4.name].wrapMode = WrapMode.Once;

        //�̺�Ʈ �Լ� ���� 
        SetAnimationEvent(animationClipAtkStep_1, "OnPlayerAttackFinshed");
        SetAnimationEvent(animationClipAtkStep_2, "OnPlayerAttackFinshed");
        SetAnimationEvent(animationClipAtkStep_3, "OnPlayerAttackFinshed");
        SetAnimationEvent(animationClipAtkStep_4, "OnPlayerAttackFinshed");

        SetAnimationEvent(skillAnimClip, "OnSkillAnimFinished");
    }

    // Update is called once per frame
    void Update()
    {
        //ĳ���� �̵� 
        Move();
        // Debug.Log(getNowVelocityVal());
        //ĳ���� ���� ���� 
        vecDirectionChangeBody();

        //���� ���¿� ���߾ �ִϸ��̼��� ��������ݴϴ�
        AnimationClipCtrl();

        //�÷��̾� ���� ���ǿ� ���߾� �ִϸ��̼� ��� 
        ckAnimationState();

        //���� ���콺 Ŭ������ ���� ���Ӱ���
        InputAttackCtrll();

        //�߷� ����
        setGravity();

        AtkComponent();
    }

    /// <summary>
    /// �̵��Լ� �Դϴ� ĳ����
    /// </summary>
    void Move()
    {
        if (stopMove == true)
        {
            return;
        }
        Transform CameraTransform = Camera.main.transform;
        //���� ī�޶� �ٶ󺸴� ������ ����� � �����ΰ�.
        Vector3 forward = CameraTransform.TransformDirection(Vector3.forward);
        forward.y = 0.0f;

        //forward.z, forward.x
        Vector3 right = new Vector3(forward.z, 0.0f, -forward.x);

        //Ű�Է� 
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        //�ɸ��Ͱ� �̵��ϰ��� �ϴ� ���� 
        Vector3 targetDirection = horizontal * right + vertical * forward;

        //���� �̵��ϴ� ���⿡�� ���ϴ� �������� ȸ�� 

        vecMoveDirection = Vector3.RotateTowards(vecMoveDirection, targetDirection, rotateMoveSpd * Mathf.Deg2Rad * Time.deltaTime, 1000.0f);
        vecMoveDirection = vecMoveDirection.normalized;
        //ĳ���� �̵� �ӵ�
        float spd = walkMoveSpd;

        //���࿡ playerState�� Run�̸� 
        if (playerState == PlayerState.Run)
        {
            spd = runMoveSpd;
        }
        else if (playerState == PlayerState.Walk)
        {
            spd = walkMoveSpd;
        }

        // �߷��̵�
        Vector3 _vecTemp = new Vector3(0f, verticalSpd, 0f);

        // ������ �̵� ��
        Vector3 moveAmount = (vecMoveDirection * spd * Time.deltaTime) + _vecTemp;

        collisionFlagsCharacter = controllerCharacter.Move(moveAmount);


    }


    /// <summary>
    /// ���� �� �ɸ��� �̵� �ӵ� �������� ��  
    /// </summary>
    /// <returns>float</returns>
    float getNowVelocityVal()
    {
        //���� ĳ���Ͱ� ���� �ִٸ� 
        if (controllerCharacter.velocity == Vector3.zero)
        {
            //��ȯ �ӵ� ���� 0
            vecNowVelocity = Vector3.zero;
        }
        else
        {

            //��ȯ �ӵ� ���� ���� /
            Vector3 retVelocity = controllerCharacter.velocity;
            retVelocity.y = 0.0f;

            vecNowVelocity = Vector3.Lerp(vecNowVelocity, retVelocity, moveChageSpd * Time.fixedDeltaTime);

        }
        //�Ÿ� ũ��
        return vecNowVelocity.magnitude;
    }

    /// <summary>
	/// GUI SKin
	/// </summary>
    private void OnGUI()
    {
        if (controllerCharacter != null && controllerCharacter.velocity != Vector3.zero)
        {
            var labelStyle = new GUIStyle();
            labelStyle.fontSize = 50;
            labelStyle.normal.textColor = Color.white;
            //ĳ���� ���� �ӵ�
            float _getVelocitySpd = getNowVelocityVal();
            GUILayout.Label("����ӵ� : " + _getVelocitySpd.ToString(), labelStyle);

            //���� ĳ���� ���� + ũ��
            GUILayout.Label("���纤�� : " + controllerCharacter.velocity.ToString(), labelStyle);

            //����  ����� ũ�� �ӵ�
            GUILayout.Label("������� ũ�� �ӵ� : " + vecNowVelocity.magnitude.ToString(), labelStyle);

        }
    }
    /// <summary>
    /// ĳ���� ���� ���� ���� �Լ�
    /// </summary>
    void vecDirectionChangeBody()
    {
        //ĳ���� �̵� ��
        if (getNowVelocityVal() > 0.0f)
        {
            //�� ����  �ٶ���� �ϴ� ���� ���?
            Vector3 newForward = controllerCharacter.velocity;
            newForward.y = 0.0f;

            //�� ĳ���� ���� ���� 
            transform.forward = Vector3.Lerp(transform.forward, newForward, rotateBodySpd * Time.deltaTime);

        }
    }


    /// <summary>
    ///  �ִϸ��̼� ��������ִ� �Լ�
    /// </summary>
    /// <param name="clip">�ִϸ��̼�Ŭ��</param>
    void playAnimationByClip(AnimationClip clip)
    {
        //ĳ�� animation Component�� clip �Ҵ�
        //        animationPlayer.clip = clip;
        animationPlayer.GetClip(clip.name);
        //����
        animationPlayer.CrossFade(clip.name);
    }

    /// <summary>
    /// ���� ���¿� ���߾� �ִϸ��̼��� ���
    /// </summary>
    void AnimationClipCtrl()
    {
        switch (playerState)
        {
            case PlayerState.Idle:
                playAnimationByClip(animationClipIdle);
                break;
            case PlayerState.Walk:
                playAnimationByClip(animationClipWalk);
                break;
            case PlayerState.Run:
                playAnimationByClip(animationClipRun);
                break;
            case PlayerState.Attack:
                stopMove = true;
                //���ݻ��¿� ���� �ִϸ��̼��� ���
                AtkAnimationCrtl();
                break;
            case PlayerState.Skill:
                playAnimationByClip(skillAnimClip);
                stopMove = true;
                break;
        }
    }

    /// <summary>
    ///  ���� ���¸� üũ���ִ� �Լ�
    /// </summary>
    void ckAnimationState()
    {
        //���� �ӵ� ��
        float nowSpd = getNowVelocityVal();

        //���� �÷��̾� ����
        switch (playerState)
        {
            case PlayerState.Idle:
                if (nowSpd > 0.0f)
                {
                    playerState = PlayerState.Walk;
                }
                break;
            case PlayerState.Walk:
                //2.0 �ȱ� max �ӵ�
                if (nowSpd > 2.0f)
                {
                    playerState = PlayerState.Run;
                }
                else if (nowSpd < 0.01f)
                {
                    playerState = PlayerState.Idle;
                }
                break;
            case PlayerState.Run:
                if (nowSpd < 0.5f)
                {
                    playerState = PlayerState.Walk;
                }

                if (nowSpd < 0.01f)
                {
                    playerState = PlayerState.Idle;
                }
                break;
            case PlayerState.Attack:
                break;
            case PlayerState.Skill:
                break;
        }
    }

    /// <summary>
    /// ���콺 ���� ��ư���� ���� �ϴ�  �Լ� 
    /// </summary>
    void InputAttackCtrll()
    {
        //���콺 Ŭ���� �Ͽ�����?
        if (Input.GetMouseButton(0) == true)
        {
            Debug.Log("InputAttackCtrll : " + playerState);
            //�÷��̾ ���ݻ���?
            if (playerState != PlayerState.Attack)
            {
                //�÷��̾ ���ݻ��°� �ƴϸ� ���ݻ��·� ����
                playerState = PlayerState.Attack;

                //���ݻ��� �ʱ�ȭ
                playerAttackState = PlayerAttackState.atkStep_1;
            }
            else
            {
                //�÷��̾� ���°� ���ݻ����̰� ���� ���°� Ȱ��ȭ �϶�
                //���� ���¿� ���� �з�
                switch (playerAttackState)
                {
                    case PlayerAttackState.atkStep_1:
                        if (animationPlayer[animationClipAtkStep_1.name].normalizedTime > 0.01f)
                        {
                            flagNextAttack = true;
                        }
                        break;
                    case PlayerAttackState.atkStep_2:
                        if (animationPlayer[animationClipAtkStep_2.name].normalizedTime > 0.05f)
                        {
                            flagNextAttack = true;
                        }
                        break;
                    case PlayerAttackState.atkStep_3:
                        if (animationPlayer[animationClipAtkStep_3.name].normalizedTime > 0.5f)
                        {
                            flagNextAttack = true;
                        }
                        break;
                    case PlayerAttackState.atkStep_4:
                        if (animationPlayer[animationClipAtkStep_4.name].normalizedTime > 0.5f)
                        {
                            flagNextAttack = true;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        //���콺 ������ ��ư�� ��������
        if(Input.GetMouseButtonDown(1) == true)
        {
            //���� ĳ���� ���°� �������̸�
            if(playerState == PlayerState.Attack)
            {
                //���ݻ��¸� 1 �⺻ ���·�
                playerAttackState = PlayerAttackState.atkStep_1;
                flagNextAttack = false;
            }

            //�÷��̾� ���¸� ��ų ���·� ����
            playerState = PlayerState.Skill;
        }
    }

    //��ų �ִϸ��̼� ����� ������ �� �̺�Ʈ
    void OnSkillAnimFinished()
    {
        //���� ĳ���� ��ġ����
        Vector3 pos = transform.position;

        //ĳ���� �� ����2.0���� ������ �Ÿ�
        pos += transform.forward * 2f;

        //�� ��ġ�� ��ų ����Ʈ�� ���δ�.
        Instantiate(skillEffect, pos, Quaternion.identity);

        //�������� ��� ���·� �д�.
        playerState = PlayerState.Idle;
    }

    /// <summary>
    ///  ���� �ִϸ��̼� ����� ������ ȣ��Ǵ� �ִϸ��̼� �̺�Ʈ �Լ�
    /// </summary>
    void OnPlayerAttackFinshed()
    {
        //���࿡ fightNext == true
        if (flagNextAttack == true)
        {
            //fight �ʱ�ȭ
            flagNextAttack = false;

            Debug.Log(playerAttackState);

            //���� ���� �ִϸ��̼� ���¿� ���� ���� �ִϸ��̼� ���°��� �ֱ�
            switch (playerAttackState)
            {

                case PlayerAttackState.atkStep_1:
                    playerAttackState = PlayerAttackState.atkStep_2;
                    break;
                case PlayerAttackState.atkStep_2:
                    playerAttackState = PlayerAttackState.atkStep_3;
                    break;
                case PlayerAttackState.atkStep_3:
                    playerAttackState = PlayerAttackState.atkStep_4;
                    break;
                case PlayerAttackState.atkStep_4:
                    playerAttackState = PlayerAttackState.atkStep_1;
                    break;
            }
        }
        else
        {
            //�������� �ƴҶ� 

            //ĳ���ʹ� �̵� ���� ���·� �����Ѵ�
            stopMove = false;

            //ĳ���ʹ� ������
            playerState = PlayerState.Idle;

            //���ݸ��� 1
            playerAttackState = PlayerAttackState.atkStep_1;
        }
    }

    /// <summary>
    /// �ִϸ��̼� Ŭ�� ����� ���� ���� �ִϸ��̼� �̺�Ʈ �Լ��� ȣ��
    /// </summary>
    /// <param name="clip">AnimationClip</param>
    /// <param name="FuncName">event function</param>
    void SetAnimationEvent(AnimationClip animationclip, string funcName)
    {
        //���ο� �̺�Ʈ ����
        AnimationEvent newAnimationEvent = new AnimationEvent();

        //�ش� �̺�Ʈ�� ȣ�� �Լ��� funcName
        newAnimationEvent.functionName = funcName;

        newAnimationEvent.time = animationclip.length - 0.15f;

        animationclip.AddEvent(newAnimationEvent);
    }

    /// <summary>
    /// ���� �ִϸ��̼� ���
    /// </summary>
    void AtkAnimationCrtl()
    {
        //���� ���ݻ��°�?
        switch (playerAttackState)
        {
            case PlayerAttackState.atkStep_1:
                playAnimationByClip(animationClipAtkStep_1);
                break;
            case PlayerAttackState.atkStep_2:
                playAnimationByClip(animationClipAtkStep_2);
                break;
            case PlayerAttackState.atkStep_3:
                playAnimationByClip(animationClipAtkStep_3);
                break;
            case PlayerAttackState.atkStep_4:
                playAnimationByClip(animationClipAtkStep_4);
                break;
        }
    }

    /// <summary>
    ///  ĳ���� �߷� ����
    /// </summary>
    void setGravity()
    {
        if ((collisionFlagsCharacter & CollisionFlags.CollidedBelow) != 0)
        {
            verticalSpd = 0f;
        }
        else
        {
            verticalSpd -= gravity * Time.deltaTime;
        }
    }

    void AtkComponent()
    {
        switch (playerState)
        {
            case PlayerState.Attack:
            case PlayerState.Skill:
                AtkTrailRenderer.enabled = true;
                AtkCollider.enabled = true;
                break;
            default:
                AtkTrailRenderer.enabled = false;
                AtkCollider.enabled = false;
                break;
        }
    }
}