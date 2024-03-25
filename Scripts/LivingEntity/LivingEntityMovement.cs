//#define DEBUG_LEM_LOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * LivingEntity의 움직임을 관리하는 Class
 */
[RequireComponent(typeof(Rigidbody))]
public class LivingEntityMovement : MonoBehaviour
{
    private Rigidbody rigid;

    [SerializeField] float originSpeed = 4f;
    [SerializeField] float speed = 4f;              //캐릭터 이동 속도
    [SerializeField] float roXSpeed = 5f;           //좌우 감도
    [SerializeField] float roYSpeed = 0.1f;         //상하 감도
    [SerializeField] float JumpForce = 3000f;       //점프에 더해주는 힘의 값

    //에임을 위아래로 올릴 수 있는 범위 각도 설정
    [SerializeField] float maxUpDegree = 30.0f;
    [SerializeField] float maxDownDegree = 40.0f;
    //aimPos.y의 값 범위
    [SerializeField] private float maxY;
    [SerializeField] private float minY;

    [SerializeField] Transform AimPos;              //캐릭터가 겨냥하는 에임의 위치

    float originAimY;                               //에임의 원 Y위치

    Vector3 velocity = Vector3.zero;

    protected float roX = 0f;     //마우스 움직임(좌우)에 따른 캐릭터 좌우 회전
    float roY = 0f;     //마우스 움직임(상하)에 따른 에임 위아래 이동

    public bool SpeedUpState { get { return speed == originSpeed; } }

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        if (rigid) rigid.constraints = RigidbodyConstraints.FreezeRotation;

#if DEBUG_LEM_LOG
        float initAimLocalY = AimPos.localPosition.y;
        Debug.Log(string.Format("Aim InitPos x : {0}, y : {1}, z : {2}", AimPos.localPosition.x, AimPos.localPosition.y, AimPos.localPosition.z));
#endif
    }

    protected virtual void FixedUpdate()
    {
        //rigidbody를 사용해 velocity값으로 캐릭터 이동
        rigid.velocity = new Vector3(velocity.x, rigid.velocity.y, velocity.z);

        //rigidbody를 사용해 캐릭터 좌우 회전 roX
        rigid.rotation *= Quaternion.Euler(0.0f, roX, 0.0f);

        float clampY = Mathf.Clamp(roY + AimPos.localPosition.y, minY, maxY);
        //마우스 움직임에 따른 에임 상하 위치 조절 -> LivingEntity의 Spine을 바라보게 함으로 상하 시야 조정
        if (AimPos) AimPos.localPosition = new Vector3(AimPos.localPosition.x, clampY, AimPos.localPosition.z);

        roY = 0;
    }

    public void Initialize()
    {
        SetStart();
        if (rigid)
        {
            rigid.rotation = Quaternion.Euler(0.0f, roX, 0.0f);
            rigid.velocity = velocity;
        }
        if (AimPos)
        {
            //에임을 위아래로 올릴 수 있는 범위를 지정.
            float tan = Mathf.Tan(maxUpDegree * Mathf.PI / 180);
            maxY = AimPos.localPosition.z * tan;
            tan = Mathf.Tan(maxDownDegree * Mathf.PI / 180);
            minY = AimPos.localPosition.z * -tan;
            originAimY = AimPos.localPosition.y;
        }
    }

    public void SetStart()
    {
        Stop();
        speed = originSpeed;
    }

    public void Stop()
    {
        velocity = Vector3.zero;
        roX = 0.0f;
        roY = 0.0f;
    }

    public void Move(Vector2 v)
    {
        velocity = ((transform.forward * v.y) + (transform.right * v.x)) * speed;
    }


    /*
     * Buff Potion중 Speed 증가 메서드
     * isStart (true : 증가, false : origin)
     */
    public void SpeedUp(bool isStart = true, float percentage = 0.0f)
    {
        speed = isStart ? speed + originSpeed * percentage : originSpeed;
    }

    //에임의 위치를 초기화
    public void InitAim()
    {
        AimPos.localPosition = new Vector3(AimPos.localPosition.x, 1.6f, AimPos.localPosition.z);
    }

    //마우스 포인터 위치의 변화량을 받아 지정한 Speed에 맞게 시야를 회전하는데 사용
    public void Rotate(Vector2 v)
    {
        roX = v.x * Time.deltaTime * roXSpeed;
        roY = v.y * Time.deltaTime * roYSpeed;
    }

    //각도만큼 Y축을 기준으로 회전
    //Aim 좌우
    public void RotateXByAngle(float angle)
    {
        transform.Rotate(Vector3.up * angle);
    }

    //dirToTarget : 타겟의 방향
    //sign        : target과 각도의 부호
    public void LookTarget(Vector3 dirToTarget, bool sign)
    {
        transform.rotation = Quaternion.LookRotation(new Vector3(dirToTarget.x, 0, dirToTarget.z).normalized);
        float angle = Vector3.Angle(dirToTarget.normalized, transform.forward);
        angle = sign ? angle : -angle;
        float localY = originAimY + AimPos.localPosition.z * Mathf.Tan(angle * Mathf.PI / 180);
        if (AimPos) AimPos.localPosition = new Vector3(AimPos.localPosition.x, Mathf.Clamp(localY, minY, maxY), AimPos.localPosition.z);
    }

    //현재 이동 중인 방향 v를 받아 점프
    public void Jump(Vector2 v)
    {
        Vector3 force = (transform.forward * v.y + transform.right * v.x + transform.up) * JumpForce;
        rigid.AddForce(force);
    }

    //착지 시 움직임을 초기화
    public void Land()
    {
        velocity = Vector3.zero;
    }

}
