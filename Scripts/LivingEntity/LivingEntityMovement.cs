//#define DEBUG_LEM_LOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * LivingEntity�� �������� �����ϴ� Class
 */
[RequireComponent(typeof(Rigidbody))]
public class LivingEntityMovement : MonoBehaviour
{
    private Rigidbody rigid;

    [SerializeField] float originSpeed = 4f;
    [SerializeField] float speed = 4f;              //ĳ���� �̵� �ӵ�
    [SerializeField] float roXSpeed = 5f;           //�¿� ����
    [SerializeField] float roYSpeed = 0.1f;         //���� ����
    [SerializeField] float JumpForce = 3000f;       //������ �����ִ� ���� ��

    //������ ���Ʒ��� �ø� �� �ִ� ���� ���� ����
    [SerializeField] float maxUpDegree = 30.0f;
    [SerializeField] float maxDownDegree = 40.0f;
    //aimPos.y�� �� ����
    [SerializeField] private float maxY;
    [SerializeField] private float minY;

    [SerializeField] Transform AimPos;              //ĳ���Ͱ� �ܳ��ϴ� ������ ��ġ

    float originAimY;                               //������ �� Y��ġ

    Vector3 velocity = Vector3.zero;

    protected float roX = 0f;     //���콺 ������(�¿�)�� ���� ĳ���� �¿� ȸ��
    float roY = 0f;     //���콺 ������(����)�� ���� ���� ���Ʒ� �̵�

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
        //rigidbody�� ����� velocity������ ĳ���� �̵�
        rigid.velocity = new Vector3(velocity.x, rigid.velocity.y, velocity.z);

        //rigidbody�� ����� ĳ���� �¿� ȸ�� roX
        rigid.rotation *= Quaternion.Euler(0.0f, roX, 0.0f);

        float clampY = Mathf.Clamp(roY + AimPos.localPosition.y, minY, maxY);
        //���콺 �����ӿ� ���� ���� ���� ��ġ ���� -> LivingEntity�� Spine�� �ٶ󺸰� ������ ���� �þ� ����
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
            //������ ���Ʒ��� �ø� �� �ִ� ������ ����.
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
     * Buff Potion�� Speed ���� �޼���
     * isStart (true : ����, false : origin)
     */
    public void SpeedUp(bool isStart = true, float percentage = 0.0f)
    {
        speed = isStart ? speed + originSpeed * percentage : originSpeed;
    }

    //������ ��ġ�� �ʱ�ȭ
    public void InitAim()
    {
        AimPos.localPosition = new Vector3(AimPos.localPosition.x, 1.6f, AimPos.localPosition.z);
    }

    //���콺 ������ ��ġ�� ��ȭ���� �޾� ������ Speed�� �°� �þ߸� ȸ���ϴµ� ���
    public void Rotate(Vector2 v)
    {
        roX = v.x * Time.deltaTime * roXSpeed;
        roY = v.y * Time.deltaTime * roYSpeed;
    }

    //������ŭ Y���� �������� ȸ��
    //Aim �¿�
    public void RotateXByAngle(float angle)
    {
        transform.Rotate(Vector3.up * angle);
    }

    //dirToTarget : Ÿ���� ����
    //sign        : target�� ������ ��ȣ
    public void LookTarget(Vector3 dirToTarget, bool sign)
    {
        transform.rotation = Quaternion.LookRotation(new Vector3(dirToTarget.x, 0, dirToTarget.z).normalized);
        float angle = Vector3.Angle(dirToTarget.normalized, transform.forward);
        angle = sign ? angle : -angle;
        float localY = originAimY + AimPos.localPosition.z * Mathf.Tan(angle * Mathf.PI / 180);
        if (AimPos) AimPos.localPosition = new Vector3(AimPos.localPosition.x, Mathf.Clamp(localY, minY, maxY), AimPos.localPosition.z);
    }

    //���� �̵� ���� ���� v�� �޾� ����
    public void Jump(Vector2 v)
    {
        Vector3 force = (transform.forward * v.y + transform.right * v.x + transform.up) * JumpForce;
        rigid.AddForce(force);
    }

    //���� �� �������� �ʱ�ȭ
    public void Land()
    {
        velocity = Vector3.zero;
    }

}
