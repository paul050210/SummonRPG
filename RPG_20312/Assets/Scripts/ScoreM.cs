using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreM : MonoBehaviour
{
    public static ScoreM scoreM;

    public static ScoreM GetScoreM()
    {
        if(scoreM == null)
        {
            scoreM = FindObjectOfType<ScoreM>();
            if(scoreM == null)
            {
                GameObject container = new GameObject("ABC");
                Instantiate(container);
                scoreM = container.AddComponent<ScoreM>();
                
            }
        }
        return scoreM;
    }

    private void Awake()
    {
        Debug.Log(ScoreM.GetScoreM().GetScore());
    }

    private int score = 0;
    
    public int GetScore()
    {
        return score;
    }

    public void SetScore(int newScore)
    {
        score += newScore;
    }


    private void Start()
    {
        if(scoreM != null)
        {
            if(scoreM != this)
            {
                Destroy(gameObject);
            }
        }
    }


    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            ScoreM.scoreM.SetScore(5);
            Debug.Log(ScoreM.scoreM.GetScore());
        }
    }
}
