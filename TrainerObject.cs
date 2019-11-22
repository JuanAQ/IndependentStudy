using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerObject : MonoBehaviour {

    Trainer trainer; //Single instance of the Trainer class that contains the Q-table and its respective methods.
    public Transform playerTrans; // Object that contains the player position
    public Transform enemyTrans; // Contains the agent position
    int xdistance;
    int ydistance;
    Vector3 NewPlayerPosition;
    Vector3 NewEnemyPosition;
    // Player and agent coordinates
    int x_P;
    int y_P;
    int x_A;
    int y_A;

    // Used to set up the Qtable for a new run
    void Awake () {
        trainer = new Trainer();
        //trainer.InitializeQTable(41, 32); This is used on the first run when there is no saved Q-table
        trainer.Load();
        LeastVisitedState(trainer.GetQtable());
        SetNewPositions();
        playerTrans.position = NewPlayerPosition;
        enemyTrans.position = NewEnemyPosition;
    }

    // The next two functions increase the speed of training by setting up the game so that the agent is positioned at the least visited state
    void LeastVisitedState(float [,]qtable)
    {
        int maxZeros = 0;
        int countOfZeros = 0;
        int indexOfState = Random.Range(0, 6560);
        for(int i = 0; i < qtable.GetLength(0); i++)
        {
            countOfZeros = 0;
            for(int j = 0; j < qtable.GetLength(1); j++)
            {
                if(qtable[i,j] == 0)
                {
                    countOfZeros++;
                }
            }
            if(countOfZeros > maxZeros)
            {
                indexOfState = i;
                maxZeros = countOfZeros;
            }
        }
        // 81 is equal to (grid * 2) - 1 where the current gridsize is 41
        xdistance = indexOfState / 81;
        ydistance = indexOfState % 81;
    }

    void SetNewPositions()
    {
        if(xdistance < 40)
        {
            x_P = 20;
        }
        else
        {
            x_P = -20;
        }
        if(ydistance < 40)
        {
            y_P = 20;
        }
        else
        {
            y_P = -20;
        }
        x_A = xdistance - 40;
        y_A = ydistance - 40;

        NewPlayerPosition = new Vector3(x_P, 0, y_P);
        NewEnemyPosition = new Vector3(x_A, 0, y_A);
    }
}
