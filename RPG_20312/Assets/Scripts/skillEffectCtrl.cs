using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skillEffectCtrl : MonoBehaviour
{
    //충돌체 검출 반경
    public float radius = 5f;

    //충돌체 파워
    public float power = 200f;

    //충돌체 위로 올릴 거리
    public float flyingSize = 3f;

    private void Start()
    {
        //Skill Effect 위치
        Vector3 posSkillEffect = transform.position;

        //posSkillEffect 위치를 중심으로 radius 반경에 있는 모든 collider 정보를 얻어오자
        Collider[] colliders = Physics.OverlapSphere(posSkillEffect, radius);

        //모든 collider 검색
        foreach(Collider collider in colliders)
        {
            //플레이어 Tag는 제외하자 나를 날릴수는 없다.
            if(collider.gameObject.CompareTag("Player") == true)
            {
                continue;
            }

            //Rigidbody component 존재 여부
            Rigidbody rigidbody = collider.GetComponent<Rigidbody>();

            if(rigidbody != null)
            {
                //rigidbody 안에 있는 폭발 기능을 사용하겠습니다
                rigidbody.AddExplosionForce(power, posSkillEffect, radius, flyingSize);
            }
        }
    }
}
