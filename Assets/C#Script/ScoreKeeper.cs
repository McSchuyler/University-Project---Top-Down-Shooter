using UnityEngine;
using System.Collections;

public class ScoreKeeper : MonoBehaviour
{

    public static int score { get; private set; } //integer for scoring (other script only can get but not set it)
    float lastEnemyKillTime; //for skilling streak
    int streakCount; //number of streak count
    float streakExpiryTime = 1; //expired time for streak

    void Start()
    {
        Enemy.OnDeathStatic += OnEnemyKilled; //if OnDeathStatic event is executed, run OnEnemyKilled event
        FindObjectOfType<Player>().OnDeath += OnPlayerDeath; //get reference to 
    }

    void OnEnemyKilled()
    {

        if (Time.time < lastEnemyKillTime + streakExpiryTime) //if player is in time for streak
        {
            streakCount++; //plus streak count
        }
        else
        {
            streakCount = 0; //reset streak count
        }

        lastEnemyKillTime = Time.time; //update streak time

        score += 5 + (int)Mathf.Pow(2, streakCount); //more enemes killed in streak, more score
    }

    void OnPlayerDeath() //when player death
    {
        Enemy.OnDeathStatic -= OnEnemyKilled; //removve function from statc event to prevent duplicate
    }

}