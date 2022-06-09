using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ray : MonoBehaviour
{
    //�Ӽ�
    //����ĳ��Ʈ ���� ���ϱ�
    public float distanceRaser = 100f;

    //�浹ü ��ġ����
    private Transform moveTarget;

    //�浹ü���� �Ÿ�
    private float distanceTarget;

    //���̾��ũ ���̾� ����
    public LayerMask layerMask;

    private void Update()
    {
        Vector3 posStart = transform.position;
        //ī�޶�ü
        //posStart = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));

        //������ ���ư��� ����
        Vector3 posTarget = transform.forward;

        //���� �޾ƿ���
        RaycastHit infoRayCast;

        /*
        infoRayCast.point : �浹�� ��ġ
        infoRayCast.normal : �浹�� ��밡 �ٶ� ��
        infoRayCast.distance : �浹�� ��ġ���� �Ÿ�
        */

        Ray ray = new Ray(posStart, posTarget);
        Debug.DrawRay(ray.origin, ray.direction * distanceRaser, Color.red);


        if(Input.GetMouseButtonDown(0))
        {
            if(Physics.Raycast(ray, out infoRayCast, distanceRaser, layerMask))
            {
                Debug.Log("�ɸ�");

                GameObject objTarget = infoRayCast.collider.gameObject;
                objTarget.GetComponent<Renderer>().material.color = Color.red;
                moveTarget = infoRayCast.transform;
                distanceTarget = infoRayCast.distance;

            } else
            {
                Debug.Log("�Ȱɸ�");
            }
        }

        if(Input.GetMouseButtonUp(0))
        {
            if(moveTarget != null)
            {
                moveTarget.GetComponent<Renderer>().material.color = Color.blue;
            }
            moveTarget = null;
        }

        if(moveTarget != null)
        {
            moveTarget.position = ray.origin + ray.direction * distanceTarget;
        }
    }
}
