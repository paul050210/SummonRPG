using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ray : MonoBehaviour
{
    //속성
    //레이캐스트 길이 정하기
    public float distanceRaser = 100f;

    //충돌체 위치정보
    private Transform moveTarget;

    //충돌체까지 거리
    private float distanceTarget;

    //레이어마스크 레이어 설정
    public LayerMask layerMask;

    private void Update()
    {
        Vector3 posStart = transform.position;
        //카메라객체
        //posStart = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));

        //광선이 날아가는 방향
        Vector3 posTarget = transform.forward;

        //정보 받아오기
        RaycastHit infoRayCast;

        /*
        infoRayCast.point : 충돌한 위치
        infoRayCast.normal : 충돌한 상대가 바라본 각
        infoRayCast.distance : 충돌한 위치까지 거리
        */

        Ray ray = new Ray(posStart, posTarget);
        Debug.DrawRay(ray.origin, ray.direction * distanceRaser, Color.red);


        if(Input.GetMouseButtonDown(0))
        {
            if(Physics.Raycast(ray, out infoRayCast, distanceRaser, layerMask))
            {
                Debug.Log("걸림");

                GameObject objTarget = infoRayCast.collider.gameObject;
                objTarget.GetComponent<Renderer>().material.color = Color.red;
                moveTarget = infoRayCast.transform;
                distanceTarget = infoRayCast.distance;

            } else
            {
                Debug.Log("안걸림");
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
