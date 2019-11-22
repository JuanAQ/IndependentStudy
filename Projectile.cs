using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour 
{
    int state;
    int action;
    int nextState;

    PlayerHealth playerHealth;

    // Position located under the map to hide projectile while Q-table update happens.
    Vector3 positionAfterCollision = new Vector3(0f, -20f, 0f);
   
    // The projectile gets destroyed after 10 seconds of being instantiated
    void Awake()
	{
		Destroy(gameObject, 10.0f);
        GameObject Player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = Player.GetComponent<PlayerHealth>();
	}

    // If the projectile collides with the player, an update to the Q-table is performed.
	void OnTriggerEnter(Collider col)
	{
        if(col.transform.gameObject.name == "Player" & playerHealth.health > 0)
        {
            transform.position = positionAfterCollision;
            StartCoroutine(UpdateQtable());
        }
        else if(col.transform.gameObject.name == "Player" & playerHealth.health <= 0)
        {
            
            transform.position = positionAfterCollision;
            Trainer trainer = new Trainer();
            trainer.LastUpdateQval(state, action, 100);       
            Destroy(gameObject);
        }
        else if(col.transform.gameObject.tag == "Wall")
        {
            Destroy(gameObject);
        }
	}
  
    // Update Q-value and destroy projectile
    private IEnumerator UpdateQtable()
    {
        yield return new WaitForSeconds(0.4f);
        Trainer trainer = new Trainer();
        trainer.UpdateQval(state, nextState, action, 100);
        Destroy(gameObject);
    }

    public void SetState(int s)
    {
        state = s;
    }

    public void SetAction(int a)
    {
        action = a;
    }

    public void SetNextState(int ns)
    {
        nextState = ns;
    }
}
