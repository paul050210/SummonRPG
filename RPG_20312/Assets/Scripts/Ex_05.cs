using UnityEngine;
/*public class Ex_04 : MonoBehaviour
{
    private void Start()
    {
        B1 sub = new B1(2022, "Unity");
        sub.PrintB1();
    }
}

class A1
{
    int num;
    protected string name;

    public A1(int num)
    {
        this.num = num;
    }

    public void PrintA1()
    {
        Debug.Log("A1 num: " + num);
    }
}

class B1 : A1
{
    //string name;

    public B1(int num, string name) : base(num)
    {
        //this.name = name;   
    }

    public void PrintB1()
    {
        base.name = "A1";
        base.PrintA1();

        Debug.Log("B1 num : " + base.name);
        Debug.Log("B1 num : " + this.name);
    }
}*/

public class Ex_05 : MonoBehaviour
{
    private void Start()
    {
        PP _p = new PP();
        _p.PrintPP();

        PP aa = new AA();
        //aa.PrintAA();
        aa.PrintPP();

        if(aa is BB)
        {
            Debug.Log("aa는 BB의 객체 입니다");
        }
        else if(aa is AA)
        {
            Debug.Log("aa는 AA의 객체 입니다");
        }

        PP bb = new BB();

        BB cloneBB = bb as BB;
        if(null != cloneBB)
        {
            Debug.Log("---------------------------------");
            Debug.Log("cloneBB는 BB객체를 형식 변환!!");
            cloneBB.PrintBB();
        }

        AA cloneAA = bb as AA;
        if (null == cloneAA)
        {
            Debug.Log("---------------------------------");
            Debug.Log("cloneAA는 AA객체가 아니므로 null!!");

            cloneAA = new AA();
            cloneAA.PrintPP();

            AA asAA = cloneAA as AA;
            asAA.PrintAA();
        }
    }
}

class PP
{
    int num;
    public void PrintPP()
    {
        Debug.Log("num: " + num);
    }
}
class AA : PP
{
    int a;
    public void PrintAA()
    {
        Debug.Log("a: " + a);
    }
}

class BB : PP
{
    int b;
    public void PrintBB()
    {
        Debug.Log("b: " + b);
    }
}
