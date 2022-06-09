using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polymorphism : MonoBehaviour
{
    private Warrior warrior = new Warrior(10, 10, 10, 10);
    private Rogue rogue = new Rogue(20, 20, 20, 20);
    private Wizard wizard = new Wizard(30, 30, 30, 30);
    
    private void Start()
    {
        Debug.Log(warrior);
        Debug.Log(rogue);
        Debug.Log(wizard                                                                                                            );
    }
}


public class Character
{
    protected int Hp;
    protected int Mp;
    protected int Spd;
    protected int Atk;
}

public class Warrior : Character
{
    public Warrior(int Hp, int Mp, int Spd, int Atk)
    {
        this.Hp = Hp;
        this.Mp = Mp;
        this.Spd = Spd;
        this.Atk = Atk;
    }
}

public class Rogue : Character
{
    public Rogue(int Hp, int Mp, int Spd, int Atk)
    {
        this.Hp = Hp;
        this.Mp = Mp;
        this.Spd = Spd;
        this.Atk = Atk;
    }
}

public class Wizard : Character
{
    public Wizard(int Hp, int Mp, int Spd, int Atk)
    {
        this.Hp = Hp;
        this.Mp = Mp;
        this.Spd = Spd;
        this.Atk = Atk;
    }
}
