using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    private BattleStateMachine BSM;
    public BaseEnemy enemy;

    public enum TurnState
    {
            PROCCESING,
            CHOOSEACTION,
            WAITING,
            ACTION,
            DEAD
    }

    public TurnState currentState;
    //for the Progress bar
    private float cur_cooldown = 0f;
    private float max_cooldown = 10f; 
    //this gameobject
    private Vector3 startposition;
    public GameObject Selector;
    // Start is called before the first frame update
    private bool actionStarted= false;
    public GameObject HeroToAttack;
    private float animSpeed = 10f;

    //alive or not
    private bool alive = true;

    //timeforaction stuff
    void Start()
    {
        Selector.SetActive(false);
        currentState = TurnState.PROCCESING;
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine> ();
        startposition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
            switch(currentState)
            {
            case (TurnState.PROCCESING):
                UpgradeProgressBar();
            break;

            case (TurnState.CHOOSEACTION):
                ChooseAction ();
                currentState = TurnState.WAITING;
            break;

            case (TurnState.WAITING):
            //idle state
            break;

            case (TurnState.ACTION):
                StartCoroutine(TimeForAction ());
            break;

            case (TurnState.DEAD):
                if(!alive)
                {
                    return;
                }
                else
                {
                    this.gameObject.tag = "DeadEnemy";
                    BSM.EnemysInBattle.Remove(this.gameObject);
                    Selector.SetActive(false);
                    if(BSM.EnemysInBattle.Count > 0)
                    {
                        for(int i = 0; i < BSM.PerformList.Count; i++)
                        {
                            if (i != 0)
                                {
                                if(BSM.PerformList[i].AttackersGameObject == this.gameObject)
                                {
                                    BSM.PerformList.Remove(BSM.PerformList[i]);
                                }
                                if(BSM.PerformList[i].AttackersTarget == this.gameObject)
                                {
                                    BSM.PerformList[i].AttackersTarget = BSM.EnemysInBattle[Random.Range(0, BSM.EnemysInBattle.Count)];
                                }
                            }
                        }
                    }
                    this.gameObject.GetComponent<MeshRenderer>().material.color = new Color32(105,105,105, 255);
                    alive = false;
                    BSM.EnemyButtons();
                    BSM.battleStates = BattleStateMachine.PERFORMACTION.CHECKALIVE; 
                }
            break;
            }       
    }

    void UpgradeProgressBar()
    {
        cur_cooldown = cur_cooldown + Time.deltaTime;
        if (cur_cooldown >= max_cooldown)
        {
                currentState = TurnState.CHOOSEACTION;
        }

    }

    void ChooseAction ()
    {
        HandleTurn myAttack = new HandleTurn ();
        myAttack.Attacker = enemy.theName;
        myAttack.Type = "Enemy";
        myAttack.AttackersGameObject = this.gameObject;
        myAttack.AttackersTarget = BSM.HerosInBattle[Random.Range(0, BSM.HerosInBattle.Count)];

        int num = Random.Range(0, enemy.Attacks.Count);
        myAttack.ChosenAttack = enemy.Attacks[num];
        BSM.CollectActions(myAttack);
    }

    private IEnumerator TimeForAction()
    {
        if(actionStarted)
        {
                yield break;
        
        }
        
        actionStarted = true;

        //animate the enemy near the hero to attack
        Vector3 heroPosition = new Vector3(HeroToAttack.transform.position.x - 1.5f, HeroToAttack.transform.position.y, HeroToAttack.transform.position.z);
        while(MoveTowardsEnemy(heroPosition)) {yield return null;}

        //wait abit
        yield return new WaitForSeconds(0.5f);
        //do damage
        DoDamage();
        //animate back to start position
        Vector3 firstPosition = startposition;
        while(MoveTowardsStart(firstPosition)) {yield return null;}


        //remove this performer from the list in BSM
        BSM.PerformList.RemoveAt(0);
        //reset BSM -> wait
        BSM.battleStates = BattleStateMachine.PERFORMACTION.WAIT;
        //end coroutine
        actionStarted = false;
        //reset the enemy state
        cur_cooldown = 0f;
        currentState = TurnState.PROCCESING;
    }

    private bool MoveTowardsEnemy(Vector3 target)
    {
            return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }

    private bool MoveTowardsStart(Vector3 target)
    {
            return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }

    void DoDamage ()
    {
        float calc_damage = enemy.currentATK + BSM.PerformList[0].ChosenAttack.attackDamage;
        HeroToAttack.GetComponent<HeroStateMachine> ().TakeDamage(calc_damage);
    }

    public void TakeDamage(float getDamageAmount) 
    {
        enemy.currentHP -= getDamageAmount;
        if(enemy.currentHP <= 0)
        {
            enemy.currentHP = 0;
            currentState = TurnState.DEAD;
        }
    }
}