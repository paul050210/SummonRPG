using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserCharacterMove : MonoBehaviour
{
    //캐릭터 직선 이동 속도 (걷기)
    public float walkMoveSpd = 2.0f;

    //캐릭터 직선 이동 속도 (달리기)
    public float runMoveSpd = 3.5f;

    //캐릭터 회전 이동 속도 
    public float rotateMoveSpd = 100.0f;

    //캐릭터 회전 방향으로 몸을 돌리는 속도
    public float rotateBodySpd = 2.0f;

    //캐릭터 이동 속도 증가 값
    public float moveChageSpd = 0.1f;

    //현재 캐릭터 이동 백터 값 
    private Vector3 vecNowVelocity = Vector3.zero;

    //현재 캐릭터 이동 방향 벡터 
    private Vector3 vecMoveDirection = Vector3.zero;

    //CharacterController 캐싱 준비
    private CharacterController controllerCharacter = null;

    //캐릭터 CollisionFlags 초기값 설정
    private CollisionFlags collisionFlagsCharacter = CollisionFlags.None;

    //캐릭터 중력값
    private float gravity = 9.8f;

    //캐릭터 중력 속도 값
    private float verticalSpd = 0f;

    //캐릭터 멈춤 변수 플래그
    private bool stopMove = false;

    [Header("애니메이션 속성")]
    public AnimationClip animationClipIdle = null;
    public AnimationClip animationClipWalk = null;
    public AnimationClip animationClipRun = null;
    public AnimationClip animationClipAtkStep_1 = null;
    public AnimationClip animationClipAtkStep_2 = null;
    public AnimationClip animationClipAtkStep_3 = null;
    public AnimationClip animationClipAtkStep_4 = null;

    //컴포넌트도 필요합니다 
    private Animation animationPlayer = null;


    //캐릭터 상태  캐릭터 상태에 따라 animation을 표현
    public enum PlayerState { None, Idle, Walk, Run, Attack, Skill }

    [Header("캐릭터상태")]
    public PlayerState playerState = PlayerState.None;

    //공격 sub state 추가 
    public enum PlayerAttackState { atkStep_1, atkStep_2, atkStep_3, atkStep_4 }

    //기본 공격 상태 값 추가 
    public PlayerAttackState playerAttackState = PlayerAttackState.atkStep_1;

    //다음 연걔 공격 활성화를 위한 flag
    public bool flagNextAttack = false;

    [Header("전투관련")]
    //공격할 때만 켜지게
    public TrailRenderer AtkTrailRenderer = null;
    //무기에 있는 콜라이더 캐싱
    public CapsuleCollider AtkCollider = null;

    [Header("스킬관련")]
    public AnimationClip skillAnimClip = null;
    public GameObject skillEffect = null;

    // Start is called before the first frame update
    void Start()
    {
        //CharacterController 캐싱
        controllerCharacter = GetComponent<CharacterController>();

        //Animation component 캐싱
        animationPlayer = GetComponent<Animation>();
        //Animation Component 자동 재생 끄기
        animationPlayer.playAutomatically = false;
        //혹시나 재생중인 Animation 있다면? 멈추기
        animationPlayer.Stop();

        //초기 애니메이션을 설정 Enum
        playerState = PlayerState.Idle;

        //animation WrapMode : 재생 모드 설정 
        animationPlayer[animationClipIdle.name].wrapMode = WrapMode.Loop;
        animationPlayer[animationClipWalk.name].wrapMode = WrapMode.Loop;
        animationPlayer[animationClipWalk.name].wrapMode = WrapMode.Loop;
        animationPlayer[animationClipAtkStep_1.name].wrapMode = WrapMode.Once;
        animationPlayer[animationClipAtkStep_2.name].wrapMode = WrapMode.Once;
        animationPlayer[animationClipAtkStep_3.name].wrapMode = WrapMode.Once;
        animationPlayer[animationClipAtkStep_4.name].wrapMode = WrapMode.Once;

        //이벤트 함수 지정 
        SetAnimationEvent(animationClipAtkStep_1, "OnPlayerAttackFinshed");
        SetAnimationEvent(animationClipAtkStep_2, "OnPlayerAttackFinshed");
        SetAnimationEvent(animationClipAtkStep_3, "OnPlayerAttackFinshed");
        SetAnimationEvent(animationClipAtkStep_4, "OnPlayerAttackFinshed");

        SetAnimationEvent(skillAnimClip, "OnSkillAnimFinished");
    }

    // Update is called once per frame
    void Update()
    {
        //캐릭터 이동 
        Move();
        // Debug.Log(getNowVelocityVal());
        //캐릭터 방향 변경 
        vecDirectionChangeBody();

        //현재 상태에 맞추어서 애니메이션을 재생시켜줍니다
        AnimationClipCtrl();

        //플레이어 상태 조건에 맞추어 애니메이션 재생 
        ckAnimationState();

        //왼쪽 마우스 클릭으로 공격 연속공격
        InputAttackCtrll();

        //중력 적용
        setGravity();

        AtkComponent();
    }

    /// <summary>
    /// 이동함수 입니다 캐릭터
    /// </summary>
    void Move()
    {
        if (stopMove == true)
        {
            return;
        }
        Transform CameraTransform = Camera.main.transform;
        //메인 카메라가 바라보는 방향이 월드상에 어떤 방향인가.
        Vector3 forward = CameraTransform.TransformDirection(Vector3.forward);
        forward.y = 0.0f;

        //forward.z, forward.x
        Vector3 right = new Vector3(forward.z, 0.0f, -forward.x);

        //키입력 
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        //케릭터가 이동하고자 하는 방향 
        Vector3 targetDirection = horizontal * right + vertical * forward;

        //현재 이동하는 방향에서 원하는 방향으로 회전 

        vecMoveDirection = Vector3.RotateTowards(vecMoveDirection, targetDirection, rotateMoveSpd * Mathf.Deg2Rad * Time.deltaTime, 1000.0f);
        vecMoveDirection = vecMoveDirection.normalized;
        //캐릭터 이동 속도
        float spd = walkMoveSpd;

        //만약에 playerState가 Run이면 
        if (playerState == PlayerState.Run)
        {
            spd = runMoveSpd;
        }
        else if (playerState == PlayerState.Walk)
        {
            spd = walkMoveSpd;
        }

        // 중력이동
        Vector3 _vecTemp = new Vector3(0f, verticalSpd, 0f);

        // 프레임 이동 양
        Vector3 moveAmount = (vecMoveDirection * spd * Time.deltaTime) + _vecTemp;

        collisionFlagsCharacter = controllerCharacter.Move(moveAmount);


    }


    /// <summary>
    /// 현재 내 케릭터 이동 속도 가져오는 함  
    /// </summary>
    /// <returns>float</returns>
    float getNowVelocityVal()
    {
        //현재 캐릭터가 멈춰 있다면 
        if (controllerCharacter.velocity == Vector3.zero)
        {
            //반환 속도 값은 0
            vecNowVelocity = Vector3.zero;
        }
        else
        {

            //반환 속도 값은 현재 /
            Vector3 retVelocity = controllerCharacter.velocity;
            retVelocity.y = 0.0f;

            vecNowVelocity = Vector3.Lerp(vecNowVelocity, retVelocity, moveChageSpd * Time.fixedDeltaTime);

        }
        //거리 크기
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
            //캐릭터 현재 속도
            float _getVelocitySpd = getNowVelocityVal();
            GUILayout.Label("현재속도 : " + _getVelocitySpd.ToString(), labelStyle);

            //현재 캐릭터 방향 + 크기
            GUILayout.Label("현재벡터 : " + controllerCharacter.velocity.ToString(), labelStyle);

            //현재  재백터 크기 속도
            GUILayout.Label("현재백터 크기 속도 : " + vecNowVelocity.magnitude.ToString(), labelStyle);

        }
    }
    /// <summary>
    /// 캐릭터 몸통 벡터 방향 함수
    /// </summary>
    void vecDirectionChangeBody()
    {
        //캐릭터 이동 시
        if (getNowVelocityVal() > 0.0f)
        {
            //내 몸통  바라봐야 하는 곳은 어디?
            Vector3 newForward = controllerCharacter.velocity;
            newForward.y = 0.0f;

            //내 캐릭터 전면 설정 
            transform.forward = Vector3.Lerp(transform.forward, newForward, rotateBodySpd * Time.deltaTime);

        }
    }


    /// <summary>
    ///  애니메이션 재생시켜주는 함수
    /// </summary>
    /// <param name="clip">애니메이션클립</param>
    void playAnimationByClip(AnimationClip clip)
    {
        //캐싱 animation Component에 clip 할당
        //        animationPlayer.clip = clip;
        animationPlayer.GetClip(clip.name);
        //블랜딩
        animationPlayer.CrossFade(clip.name);
    }

    /// <summary>
    /// 현재 상태에 맞추어 애니메이션을 재생
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
                //공격상태에 맞춘 애니메이션을 재생
                AtkAnimationCrtl();
                break;
            case PlayerState.Skill:
                playAnimationByClip(skillAnimClip);
                stopMove = true;
                break;
        }
    }

    /// <summary>
    ///  현재 상태를 체크해주는 함수
    /// </summary>
    void ckAnimationState()
    {
        //현재 속도 값
        float nowSpd = getNowVelocityVal();

        //현재 플레이어 상태
        switch (playerState)
        {
            case PlayerState.Idle:
                if (nowSpd > 0.0f)
                {
                    playerState = PlayerState.Walk;
                }
                break;
            case PlayerState.Walk:
                //2.0 걷기 max 속도
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
    /// 마우스 왼쪽 버튼으로 공격 하는  함수 
    /// </summary>
    void InputAttackCtrll()
    {
        //마우스 클릭을 하였느냐?
        if (Input.GetMouseButton(0) == true)
        {
            Debug.Log("InputAttackCtrll : " + playerState);
            //플레이어가 공격상태?
            if (playerState != PlayerState.Attack)
            {
                //플레이어가 공격상태가 아니면 공격상태로 변경
                playerState = PlayerState.Attack;

                //공격상태 초기화
                playerAttackState = PlayerAttackState.atkStep_1;
            }
            else
            {
                //플레이어 상태가 공격상태이고 공격 상태가 활성화 일때
                //공격 상태에 따른 분류
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

        //마우스 오른쪽 버튼을 눌렀ㄷ면
        if(Input.GetMouseButtonDown(1) == true)
        {
            //만약 캐릭터 상태가 공격중이면
            if(playerState == PlayerState.Attack)
            {
                //공격상태를 1 기본 상태로
                playerAttackState = PlayerAttackState.atkStep_1;
                flagNextAttack = false;
            }

            //플레이어 상태를 스킬 상태로 변경
            playerState = PlayerState.Skill;
        }
    }

    //스킬 애니메이션 재생이 끝났을 때 이벤트
    void OnSkillAnimFinished()
    {
        //현재 캐릭터 위치저장
        Vector3 pos = transform.position;

        //캐릭터 앞 방향2.0정도 떨어진 거리
        pos += transform.forward * 2f;

        //그 위치에 스킬 이펙트를 붙인다.
        Instantiate(skillEffect, pos, Quaternion.identity);

        //끝났으면 대기 상태로 둔다.
        playerState = PlayerState.Idle;
    }

    /// <summary>
    ///  공격 애니매이션 재생이 끝나면 호출되는 애니매이션 이벤트 함수
    /// </summary>
    void OnPlayerAttackFinshed()
    {
        //만약에 fightNext == true
        if (flagNextAttack == true)
        {
            //fight 초기화
            flagNextAttack = false;

            Debug.Log(playerAttackState);

            //현재 공격 애니매이션 상태에 따른 다음 애니매이션 상태값을 넣기
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
            //공격중이 아닐때 

            //캐릭터는 이동 가능 상태로 변경한다
            stopMove = false;

            //캐릭터는 대기상태
            playerState = PlayerState.Idle;

            //공격모드는 1
            playerAttackState = PlayerAttackState.atkStep_1;
        }
    }

    /// <summary>
    /// 애니매이션 클립 재생이 끝날 때쯤 애니매이션 이벤트 함수를 호출
    /// </summary>
    /// <param name="clip">AnimationClip</param>
    /// <param name="FuncName">event function</param>
    void SetAnimationEvent(AnimationClip animationclip, string funcName)
    {
        //새로운 이벤트 선언
        AnimationEvent newAnimationEvent = new AnimationEvent();

        //해당 이벤트의 호출 함수는 funcName
        newAnimationEvent.functionName = funcName;

        newAnimationEvent.time = animationclip.length - 0.15f;

        animationclip.AddEvent(newAnimationEvent);
    }

    /// <summary>
    /// 공격 애니매이션 재생
    /// </summary>
    void AtkAnimationCrtl()
    {
        //만약 공격상태가?
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
    ///  캐릭터 중력 설정
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