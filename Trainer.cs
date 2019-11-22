using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Trainer : MonoBehaviour{

    private const float learning_rate = 0.1f; // Rate at which the new value is updated.
    private const float discount_factor = 0.3f; // Leverages the influence of future states
    private float explore_exploit = 0.0f; // used to determine if the next action will be randomly or based on current knowledge
    private const float l_bound = 0.1f; // lower bound of explore_exploit, used as condition to decrement exploring as training happens
    private const float explore_decrease = 0.05f;

    static private float[,] qtable;
    int act; // action chosen
    int x_A; // x coordinate of the enemy agent
    int y_A; // y coordinate of the enemy agent
    int x_P; // x coordinate of the player
    int y_P; // y coordinate of the player

    // future coordinates for the enemy agent and player
    int next_x_A;
    int next_y_A;
    int next_x_P;
    int next_y_P;

    // current state and future state for the enemy agent
    int currentState;
    int nextState;

    // q-table loaded from Qtable.sav file
    float[,] loadedTable;

    public Trainer() { }

    public void Save()
    {
        SaveLoadQtable.SaveQTable(this);
    }

    public void Load()
    {
        qtable = SaveLoadQtable.LoadQTable();
    }

    public float[,] GetQtable()
    {
        return qtable;
    }

    // Used on first run when there is no saved Q-table
    public void InitializeQTable(int gridSize, int actionSize)
    {
        int stateSize = ((gridSize * 2) - 1) * ((gridSize * 2) - 1);
        qtable = new float[stateSize, actionSize];

        for(int i = 0; i < stateSize; i++)
        {
            for(int j = 0; j < actionSize; j++)
            {
                qtable[i, j] = 0.0f;
            }
        }
    }

    // Next agent action based on exploration and exploitation
    public int ChooseNextAction()
    {
        if(UnityEngine.Random.Range(0f, 1f) < explore_exploit)
        {
            act = UnityEngine.Random.Range(0, 31);
        }
        else
        {
            act = MaxValueIndex(currentState);
        }

        if(l_bound < explore_exploit)
        {
            explore_exploit = explore_exploit - explore_decrease;
        }

        return act;
    }

    // Update Q values on the table
    public void UpdateQval(int cState, int nState, int action, float reward)
    {
        int maxIndex = MaxValueIndex(nState);
        float max_futureAction = qtable[nState, maxIndex];
        qtable[cState, action] += learning_rate * (reward + discount_factor * max_futureAction - qtable[cState, action]);
    }

    // Q-table update at the end of an episode (when the player dies)
    public void LastUpdateQval(int cState, int action, float reward)
    {
        qtable[cState, action] += learning_rate * (reward - qtable[cState, action]);
    }

    public void SetState(int state)
    {
        currentState = state;
    }

    public void SetNextState(int state)
    {
        nextState = state;
    }

    // Returns index of highest value in the state passed
    public int MaxValueIndex(int state)
    {
        int index = 0;
        List<int> sameMaxValues = new List<int>();
        sameMaxValues.Add(index);

        for (int i = 1; i < 32; i++)
        {
            if (qtable[state, index] < qtable[state, i])
            {
                sameMaxValues.Clear();
                index = i;
                sameMaxValues.Add(index);
            }
            if (qtable[state, index] == qtable[state, i])
            {
                sameMaxValues.Add(i);
            }
        }

        int ran = Random.Range(0, sameMaxValues.Count);

        return sameMaxValues[ran];
    }

    // Max index of highest value of the future state passed
    public int MaxFutureValueIndex(int futureState)
    {
        int index = 0;
        List<int> sameMaxValues = new List<int>();
        sameMaxValues.Add(index);
        
        for(int i = 1; i < 32; i++)
        {
            if (qtable[futureState, index] < qtable[futureState, i])
            {
                sameMaxValues.Clear();
                index = i;
                sameMaxValues.Add(index);
            }
            if (qtable[futureState, index] == qtable[futureState, i])
            {
                sameMaxValues.Add(i);
            }
        }

        int ran = Random.Range(0, sameMaxValues.Count);

        return sameMaxValues[ran];
    }

}
