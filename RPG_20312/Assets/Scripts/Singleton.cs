using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton : MonoBehaviour
{
    public static Singleton heroKing;

    public string heroName = "";
    public bool isKing = false;

    private void Start()
    {
        if(isKing)
        {
            heroKing = this;
        }
    }

    private void Update()
    {
        Debug.Log("My Name is " + heroName + " king is " + heroKing);
    }
}
