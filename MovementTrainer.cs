using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MovementTrainer : MonoBehaviour {

    Rigidbody rb; // Stores object used for physics handling.

    private const float speed = 2.5f; // Movement speed for the enemy object
    private Vector3 currentDirection; // Direction the enemy object moves to

    private int x_distance;
    private int y_distance;
    private int grid_size = 41; // Fixed grid size of the game map

    public PlayerHealth playerHealth;
    public Transform playerPosition; // Holds transform object of the player
    private int agentX; // X Position of the Agent
    private int agentY; // Y Position of the Agent
    private int playerX; // X Position of the Player
    private int playerY; // Y Position of the Player

    private IEnumerator coroutine; // Used to call a method and yield return by specified seconds, creating a discrete timeline

    Trainer trainer; // Q_learning class inside Trainer

    EnemyAttack enemyShoot;
    public Transform projectileSpawnPoint;
    public GameObject projectileObject;

    EnemyHealth enemyHealth; // Determines rewards at the end of each move
    int reward; // Stores the reward after each action
    int action; // Stores the action chosen by the agent;
    int state;

    int health = 20;

    // Used to choose aim direction
    // The gunCenter is a child of the enemy game object. In order to change the angle it is pointing at,
    // the child's transform must be stored in a variable and the method called inside this script.
    // This is because Unity does not allow sharing variables between scripts.
    // My intention at first was to implement a script for the gun center and use aim direction methods in it
    // while the Enemy's main script handled choosing where to aim and passing it to the script.
    Transform gunCenter;
    Transform East;
    Transform West;
    Transform North;
    Transform South;
    Transform NorthEast;
    Transform NorthWest;
    Transform SouthEast;
    Transform SouthWest;

    void Awake()
    {
        trainer = new Trainer();

        GameObject Player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = Player.GetComponent<PlayerHealth>();
        playerPosition = Player.transform;

        enemyShoot = new EnemyAttack();
        enemyShoot.SetProjectileSpawnPoint(projectileSpawnPoint);
        enemyShoot.SetProjectileObject(projectileObject);
    
        rb = GetComponent<Rigidbody>();

        //Storing each children to handle aim direction methods on gunCenter
        gunCenter = this.gameObject.transform.GetChild(0);
        East = this.gameObject.transform.GetChild(1);
        West = this.gameObject.transform.GetChild(2);
        North = this.gameObject.transform.GetChild(3);
        South = this.gameObject.transform.GetChild(4);
        NorthEast = this.gameObject.transform.GetChild(5);
        NorthWest = this.gameObject.transform.GetChild(6);
        SouthEast = this.gameObject.transform.GetChild(7);
        SouthWest = this.gameObject.transform.GetChild(8);

        // Initiate enemy movement
        int ran = UnityEngine.Random.Range(0, 4);
        if(ran == 0)
        {
            currentDirection = Vector3.forward;
        }
        else if(ran == 1)
        {
            currentDirection = Vector3.right;
        }
        else if(ran == 2)
        {
            currentDirection = Vector3.back;
        }
        else
        {
            currentDirection = Vector3.left;
        }

        // Initiate action choosing loop
        coroutine = NewChooseDirection();
        StartCoroutine(coroutine);
    }

    void FixedUpdate()
    {
        Vector3 tempVect = currentDirection;
        tempVect = tempVect.normalized * speed * Time.deltaTime;
        rb.MovePosition(transform.position + tempVect);

    }

    void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    
    void OnTriggerEnter(Collider target)
    {
        //If enemy is hit by gameObject projectile, take damage and get negative reward
        if (target.transform.gameObject.name == "playerprojectile(Clone)")
        {
            health -= 5;
            reward = -3;
        }
    }

    // Determines the x,y values for both agent and player to determine the state.
    // In Unity the y value represents the vertical axis (height). The "y" value is the z axis.
    // Border positions in the map are rounded up/down since Unity has a decimal representation of coordinates
    // and sometimes collision detection is not precise.
    void DeterminePosition()
    {
        agentX = (int)Math.Floor(transform.position.x);
        if (agentX < -20)
        {
            agentX = -20;
        }
        else if(agentX > 20)
        {
            agentX = 20;
        }
        
        agentY = (int)Math.Floor(transform.position.z);
        if (agentY < -20)
        {
            agentY = -20;
        }
        else if (agentY > 20)
        {
            agentY = 20;
        }
        
        playerX = (int)Math.Floor(playerPosition.position.x);
        if (playerX < -20)
        {
            playerX = -20;
        }
        else if (playerX > 20)
        {
            playerX = 20;
        }
        
        playerY = (int)Math.Floor(playerPosition.position.z);
        if (playerY < -20)
        {
            playerY = -20;
        }
        else if (playerY > 20)
        {
            playerY = 20;
        }
        
    }

    // We need to determine how many units up/down and right/left the player is from the enemy
    // The state will be represented as (x, y), where x is the horizontal steps and y the vertical steps
    // that describe the distance between the player and the enemy where the steps are counted from the enemy to
    // the player.
    // We will have a one dimensional array that represents all the (x, y) pairs.
    // DetermineState() will take an (x, y) pair and output the state that represents that pair.
    // The formula that is used to map them is x + (y * ((gridsize * 2)  - 1)), where n is the grid size.

    int DetermineState()
    {
        x_distance = agentX - playerX + grid_size - 1;
        y_distance = agentY - playerY + grid_size - 1;

        int state = x_distance + (y_distance * ((grid_size * 2) - 1));
        
        return state;
    }

    // Determines the state based on player(x, y) and agent (x, y)
    int EvaluateState()
    {
        DeterminePosition();
        int state = DetermineState();
        trainer.SetState(state);
        return state;
    }

    // Determines the state based on player(x, y) and agent (x, y)
    // Future state is needed in the bellman equation. This state is calculated right before a new cycle begins.
    int EvaluateFutureState()
    {
        DeterminePosition();
        int state = DetermineState();
        trainer.SetNextState(state);
        return state;
    }
    
    // Core sequence of methods to update the q table.
    // ChooseDirection() determines the next action based on the q table
    // EvaluateState() is used to store the state in which the action is chosen
    // EvaluateReward() returns the reward based on the enemy health after the action is performed
    IEnumerator NewChooseDirection()
    {
        while (playerHealth.health > 0)
        {
            state = EvaluateState();
            action = ChooseDirection();
            GameObject projectileInstance = enemyShoot.Shoot();
            // The projectile is passed teh state and action in case it hits the player past this timestep
            Projectile projectileScript = projectileInstance.GetComponent<Projectile>();
            projectileScript.SetState(state);
            projectileScript.SetAction(action);
            yield return new WaitForSeconds(0.4f);
            if(playerHealth.health > 0)
            {
                int futureState = EvaluateFutureState();
                projectileScript.SetNextState(futureState);
                if (reward != -3)
                {
                    reward = -1;
                }
                trainer.UpdateQval(state, futureState, action, reward);
            }
            else
            {
                if (reward != -3)
                {
                    reward = -1;
                }
                trainer.LastUpdateQval(state, action, reward);
            }
            
        }
    }  

    // Choose action by accessing the q table and returns the index of the action in the table
    int ChooseDirection()
    {
        int chosenAction = trainer.ChooseNextAction();
        int aimDirection = chosenAction % 8;
        int moveDirection = Math.Floor(chosenAction / 8);

        switch(aimDirection){
            case 0:
                gunCenter.LookAt(North);
                break;
            case 1:
                gunCenter.LookAt(South);
                break;
            case 2:
                gunCenter.LookAt(East);
                break;
            case 3:
                gunCenter.LookAt(West);
                break;
            case 4:
                gunCenter.LookAt(NorthEast);
                break;
            case 5:
                gunCenter.LookAt(NorthWest);
                break;
            case 6:
                gunCenter.LookAt(SouthEast);
                break;
            default:
                gunCenter.LookAt(SouthWest);
        }

        switch (moveDirection) {
            case 0:
                MoveForward();
                break;
            case 1:
                MoveLeft();
                break;
            case 2:
                MoveRight();
                break;
            default:
                MoveBack();
                break;
        }

        return chosenAction;
    }

    // Descriptive function names for movement
    void MoveForward()
    {
        currentDirection = Vector3.forward;
    }

    void MoveRight()
    {
        currentDirection = Vector3.right;
    }

    void MoveLeft()
    {
        currentDirection = Vector3.left;
    }

    void MoveBack()
    {
        currentDirection = Vector3.back;
    }

}
