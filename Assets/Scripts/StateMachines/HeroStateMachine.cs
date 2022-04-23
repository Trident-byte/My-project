using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random=UnityEngine.Random;

public class HeroStateMachine : MonoBehaviour
{
    public BaseHero hero;
    
    public enum TurnState
    {
            PROCCESING,
            ADDTOLIST,
            WAITING,
            SELECTING,
            ACTION,
            DEAD
    }

    private BattleStateMachine BSM;
    public TurnState currentState;
    //for the Progress bar
    private float cur_cooldown = 0f;
    private float max_cooldown = 5f; 
    public Image ProgressBar;
    public GameObject Selector;
    //IEnumerator
    public GameObject EnemyToAttack;
    private bool actionStarted = false;
    private Vector3 startPosition;
    private float animSpeed = 10;

    //dead or alive
    private bool alive = true;

    //HeroPanel
    private HeroPanelStats stats;
    public GameObject HeroPanel;
    private Transform HeroSpacer;


    // Start is called before the first frame update
    void Start()
    {
            //find spacer
            HeroSpacer = GameObject.Find("BattleCanvas").transform.Find("HeroPanel").transform.Find("HeroSpacer");
            //create panel, fill in info
            CreateHeroPanel();

            startPosition = transform.position;
            cur_cooldown = Random.Range (0, 2.5f);
            Selector.SetActive(false);
            BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine> ();
            currentState = TurnState.PROCCESING;

    }

    // Update is called once per frame
    void Update()
    {
            switch(currentState)
            {
            case (TurnState.PROCCESING):
                UpgradeProgressBar();
            break;
            
            case (TurnState.ADDTOLIST):
                BSM.HerosToManage.Add (this.gameObject);
                currentState = TurnState.WAITING;

            break;

            case (TurnState.WAITING):
                //idle
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
                //change tag
                this.gameObject.tag = "DeadHero";
                //not attackable by enemy
                BSM.HerosInBattle.Remove(this.gameObject);
                //not managable
                BSM.HerosToManage.Remove(this.gameObject);
                //deactivate the selector
                Selector.SetActive(false);
                //reset GUI
                BSM.ActionPanel.SetActive(false);
                BSM.EnemySelectPanel.SetActive(false);
                //remove item from perform list
                if(BSM.HerosInBattle.Count > 0)
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
                                BSM.PerformList[i].AttackersTarget = BSM.HerosInBattle[Random.Range(0, BSM.HerosInBattle.Count)];
                            }
                        }
                    }
                }
                //change color / play death animation
                this.gameObject.GetComponent<MeshRenderer>().material.color = new Color32(105,105,105,255);
                //reset heroinput
                BSM.battleStates = BattleStateMachine.PERFORMACTION.CHECKALIVE;
                alive = false;
                
            }

            break;
            }
    }

    void UpgradeProgressBar()
    {
        cur_cooldown = cur_cooldown + Time.deltaTime;
        float calc_cooldown = cur_cooldown/max_cooldown;
        ProgressBar.transform.localScale = new Vector3(Mathf.Clamp(calc_cooldown, 0, 1), ProgressBar.transform.localScale.y, ProgressBar.transform.localScale.x);
        if (cur_cooldown >= max_cooldown)
        {
                currentState = TurnState.ADDTOLIST;
        }
    }

    private IEnumerator TimeForAction()
    {
        if(actionStarted)
        {
                yield break;
        
        }
        
        actionStarted = true;

        //animate the enemy near the hero to attack
        Vector3 enemyPosition = new Vector3(EnemyToAttack.transform.position.x + 1.5f, EnemyToAttack.transform.position.y, EnemyToAttack.transform.position.z);
        while(MoveTowardsEnemy(enemyPosition)) {yield return null;}

        //wait abit
        yield return new WaitForSeconds(0.5f);
        //do damage
        DoDamage ();
        //animate back to start position
        Vector3 firstPosition = startPosition;
        while(MoveTowardsStart(firstPosition)) {yield return null;}


        //remove this performer from the list in BSM
        BSM.PerformList.RemoveAt(0);
        //reset BSM -> wait
        if(BSM.battleStates != BattleStateMachine.PERFORMACTION.WIN && BSM.battleStates != BattleStateMachine.PERFORMACTION.LOSE)
        {
            BSM.battleStates = BattleStateMachine.PERFORMACTION.WAIT;
            //end coroutine
            actionStarted = false;
            //reset the enemy state
            cur_cooldown = 0f;
            currentState = TurnState.PROCCESING;
        }
        else
        {
            currentState = TurnState.WAITING;
        }
    }

    private bool MoveTowardsEnemy(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }

    private bool MoveTowardsStart(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }

    public void TakeDamage(float getDamageAmount) 
    {
        hero.currentHP -= getDamageAmount;
        if(hero.currentHP <= 0)
        {
            hero.currentHP = 0;
            currentState = TurnState.DEAD;
        }
        UpdateHeroPanel();
    }

    //do damage
    void DoDamage ()
    {
        float calc_damage = hero.currentATK + BSM.PerformList[0].ChosenAttack.attackDamage;
        EnemyToAttack.GetComponent<EnemyStateMachine> ().TakeDamage(calc_damage);
    }

    //create a hero panel
    void CreateHeroPanel()
    {
        HeroPanel = Instantiate(HeroPanel) as GameObject;
        stats = HeroPanel.GetComponent<HeroPanelStats>();
        stats.HeroName.text = hero.theName;
        stats.HeroHP.text = "HP: " + hero.currentHP;
        stats.HeroMP.text = "MP: " + hero.currentMP;

        ProgressBar = stats.ProgressBar;
        HeroPanel.transform.SetParent(HeroSpacer, false);


    }
    //updates stats when damaged/healed
    void UpdateHeroPanel()
    {
        stats.HeroHP.text = "HP: " + hero.currentHP;
        stats.HeroMP.text = "MP: " + hero.currentMP;
    }
}
