using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skillEffectCtrl : MonoBehaviour
{
    //�浹ü ���� �ݰ�
    public float radius = 5f;

    //�浹ü �Ŀ�
    public float power = 200f;

    //�浹ü ���� �ø� �Ÿ�
    public float flyingSize = 3f;

    private void Start()
    {
        //Skill Effect ��ġ
        Vector3 posSkillEffect = transform.position;

        //posSkillEffect ��ġ�� �߽����� radius �ݰ濡 �ִ� ��� collider ������ ������
        Collider[] colliders = Physics.OverlapSphere(posSkillEffect, radius);

        //��� collider �˻�
        foreach(Collider collider in colliders)
        {
            //�÷��̾� Tag�� �������� ���� �������� ����.
            if(collider.gameObject.CompareTag("Player") == true)
            {
                continue;
            }

            //Rigidbody component ���� ����
            Rigidbody rigidbody = collider.GetComponent<Rigidbody>();

            if(rigidbody != null)
            {
                //rigidbody �ȿ� �ִ� ���� ����� ����ϰڽ��ϴ�
                rigidbody.AddExplosionForce(power, posSkillEffect, radius, flyingSize);
            }
        }
    }
}
