using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnMonster : MonoBehaviour
{
    //������ ���� ������Ʈ
    public GameObject monsterSpawner = null;

    //������ ���͵� ��Ƴ���
    public List<GameObject> monsters = new List<GameObject>();

    //������ ���� �ִ��
    public int spawnMaxCnt = 50;

    //������ ���� ���� ��ǥ (x,z)��ġ
    float rndPos = 100f;

    void Spawn()
    {
        //���� ���� ������ ���� �ִ��
        if(monsters.Count > spawnMaxCnt)
        {
            return;
        }

        //������ ��ġ�� �����Ѵ�. �ʱ� ���̸� 100 ������ x,z�� ����
        Vector3 vecSpawn = new Vector3(Random.Range(-rndPos, rndPos), 1000f, Random.Range(-rndPos, rndPos));

        //������ �ӽ� ���̿��� �Ʒ��������� Raycast�� ���� �������� ���� ���ϱ�
        Ray ray = new Ray(vecSpawn, Vector3.down);

        //Raycast ���� ��������
        RaycastHit raycastHit = new RaycastHit();
        if(Physics.Raycast(ray, out raycastHit, Mathf.Infinity) == true)
        {
            //Raycast ���̸� y������ �缳��
            vecSpawn.y = raycastHit.point.y;
        }

        //������ ���ο� ���͸� Instantiate�� clone�� �����.
        GameObject newMonster = Instantiate(monsterSpawner, vecSpawn, Quaternion.identity);

        //���� ��Ͽ� ���ο� ���͸� �߰�
        monsters.Add(newMonster);
    }

    private void Start()
    {
        //�ݺ������� ���͸� ����� InvokeRepeating
        InvokeRepeating("Spawn", 3f, 5f);
    }
}
