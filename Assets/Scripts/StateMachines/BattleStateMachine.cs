using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleStateMachine : MonoBehaviour
{

    public enum PERFORMACTION
    {
        WAIT,
        TAKEACTION,
        PERFORMACTION,
        CHECKALIVE,
        WIN,
        LOSE
    }
    public PERFORMACTION battleStates;

    public List<HandleTurn> PerformList = new List<HandleTurn> ( );
    public List<GameObject> HerosInBattle = new List<GameObject> ( );
    public List<GameObject> EnemysInBattle = new List<GameObject> ( );


    public enum HeroGUI
    {
        ACTIVATE,
        WAITING,
        INPUT1,
        INPUT2,
        DONE

    }

    public HeroGUI HeroInput;

    public List<GameObject> HerosToManage = new List<GameObject> ();
    private HandleTurn HeroChoice;

    public GameObject enemyButton;
    public Transform Spacer;

    public GameObject ActionPanel;
    public GameObject EnemySelectPanel;
    public GameObject MagicPanel;

    //magic attack
    public Transform ActionSpacer;
    public Transform MagicSpacer;
    public GameObject ActionButton;
    public GameObject MagicButton;
    private List<GameObject> AtkButtons = new List<GameObject> ();

    private List<GameObject> enemyBtns = new List<GameObject>();

    public List<Transform> spawnPoints = new List<Transform>();

    void Awake()
    {
        for(int i = 0; i < GameManager.instance.enemyAmount; i++)
        {
            GameObject NewEnemy = Instantiate(GameManager.instance.enemiesToBattle[i],spawnPoints[i].position, Quaternion.identity) as GameObject;
            NewEnemy.name = NewEnemy.GetComponent<EnemyStateMachine>().enemy.theName+"_"+i;
            NewEnemy.GetComponent<EnemyStateMachine>().enemy.theName = NewEnemy.name; 
            EnemysInBattle.Add(NewEnemy);
        }
    }

    void Start()
    {
        battleStates = PERFORMACTION.WAIT;
        HerosInBattle.AddRange (GameObject.FindGameObjectsWithTag("Hero"));
        HeroInput = HeroGUI.ACTIVATE;

        ActionPanel.SetActive(false);
        EnemySelectPanel.SetActive(false);
        MagicPanel.SetActive(false);

        EnemyButtons ();
    }

    // Update is called once per frame
    void Update()
    {
        switch(battleStates)
        {
        case(PERFORMACTION.WAIT):
            if(PerformList.Count > 0) 
            {
                    battleStates = PERFORMACTION.TAKEACTION;     
            }
         break;

        case(PERFORMACTION.TAKEACTION):
            GameObject performer = GameObject.Find(PerformList[0].Attacker);
            if(PerformList[0].Type == "Enemy")
            {
                    EnemyStateMachine ESM = performer.GetComponent<EnemyStateMachine> ();
                    for(int i = 0; i< HerosInBattle.Count; i++)
                    {
                        if(PerformList[0].AttackersTarget == HerosInBattle[i])
                        {
                            ESM.HeroToAttack = PerformList[0].AttackersTarget;
                            ESM.currentState = EnemyStateMachine.TurnState.ACTION;
                            break;

                        }
                        else
                        {
                            PerformList[0].AttackersTarget = HerosInBattle[Random.Range(0,HerosInBattle.Count)];
                            ESM.HeroToAttack = PerformList[0].AttackersTarget;
                            ESM.currentState = EnemyStateMachine.TurnState.ACTION;
                        }
                    }
            }

            if(PerformList[0].Type == "Hero")
            {
                HeroStateMachine HSM = performer.GetComponent<HeroStateMachine> ();
                HSM.EnemyToAttack = PerformList[0].AttackersTarget;
                HSM.currentState = HeroStateMachine.TurnState.ACTION;
            }
            battleStates = PERFORMACTION.PERFORMACTION;
        break;

        case(PERFORMACTION.PERFORMACTION):

        break;

        case(PERFORMACTION.CHECKALIVE):
        if(HerosInBattle.Count < 1)
        {
            battleStates = PERFORMACTION.LOSE;
            //lose battle
        }
        else if(EnemysInBattle.Count < 1)
        {
            battleStates = PERFORMACTION.WIN;
            //win battle
        }
        else
        {
            //call function
            clearAttackPanel();
            HeroInput = HeroGUI.ACTIVATE;
        }

        break;

        case(PERFORMACTION.WIN):
            for(int i = 0; i < HerosInBattle.Count; i++)
            {
                HerosInBattle[i].GetComponent<HeroStateMachine>().currentState = HeroStateMachine.TurnState.WAITING;
            }
            GameManager.instance.LoadSceneAfterBattle();
            GameManager.instance.GameState = GameManager.GameStates.WORLD_STATE;
            GameManager.instance.enemiesToBattle.Clear();
        break;

        case(PERFORMACTION.LOSE):

        break;
        }

        switch (HeroInput)
        {
            case(HeroGUI.ACTIVATE):
            if(HerosToManage.Count > 0)
            {
                HerosToManage[0].transform.Find("Selector").gameObject.SetActive (true);  
                HeroChoice = new HandleTurn (); 
                ActionPanel.SetActive(true);

                //populate action buttons
                CreateAttackButtons();

                HeroInput = HeroGUI.WAITING;
            }
            break;

            case(HeroGUI.WAITING):

            break;

            case(HeroGUI.DONE):
            HeroInputDone();
            break;

        }
    }

    public void CollectActions(HandleTurn input)
    {
            PerformList.Add (input);
    }

    public void EnemyButtons()
    {
        //cleanup
        foreach(GameObject enemyBtn in enemyBtns)
        {
            Destroy(enemyBtn);
        }
        enemyBtns.Clear();

        foreach (GameObject enemy in EnemysInBattle)
        {
            GameObject newButton = Instantiate (enemyButton) as GameObject;
            EnemySelectButton button = newButton.GetComponent<EnemySelectButton> ();

            EnemyStateMachine cur_enemy = enemy.GetComponent<EnemyStateMachine>();

            Text buttonText = newButton.transform.Find("Text").gameObject.GetComponent<Text> ();
            buttonText.text = cur_enemy.enemy.theName;

            button.EnemyPrefab = enemy; 
            newButton.transform.SetParent(Spacer, false);
            enemyBtns.Add(newButton);

        }
    }

    public void Input1() //attack button
    {
        HeroChoice.Attacker = HerosToManage [0].name;
        HeroChoice.AttackersGameObject = HerosToManage [0];
        HeroChoice.Type = "Hero";
        HeroChoice.ChosenAttack = HerosToManage[0].GetComponent<HeroStateMachine>().hero.Attacks[0];
        ActionPanel.SetActive(false);
        EnemySelectPanel.SetActive(true);

    }

    public void Input2(GameObject chosenEnemy)
    {
        HeroChoice.AttackersTarget = chosenEnemy;
        HeroInput = HeroGUI.DONE;

    }

    void HeroInputDone()
    {
        PerformList.Add(HeroChoice);
        EnemySelectPanel.SetActive(false);
        clearAttackPanel();

        HerosToManage[0].transform.Find("Selector").gameObject.SetActive (false);  
        HerosToManage.RemoveAt (0);
        HeroInput = HeroGUI.ACTIVATE;
    }

    void clearAttackPanel()
    {
        EnemySelectPanel.SetActive(false);
        ActionPanel.SetActive(false);
        MagicPanel.SetActive(false);

        foreach(GameObject Button in AtkButtons)
        {
            Destroy(Button);
        }
        AtkButtons.Clear();
    }

    //creates actionbuttons
    void CreateAttackButtons ()
    {
        GameObject AttackButton = Instantiate(ActionButton) as GameObject;
        Text AttackButtonText = AttackButton.transform.Find("Text").gameObject.GetComponent<Text>();
        AttackButtonText.text = "Attack";
        AttackButton.GetComponent<Button>().onClick.AddListener(()=>Input1());
        AttackButton.transform.SetParent(ActionSpacer, false);
        AtkButtons.Add(AttackButton);

        GameObject MagicAttackButton = Instantiate(ActionButton) as GameObject;
        Text MagicAttackButtonText = MagicAttackButton.transform.Find("Text").gameObject.GetComponent<Text>();
        MagicAttackButtonText.text = "Magic";
        MagicAttackButton.GetComponent<Button>().onClick.AddListener(()=>Input4());
        MagicAttackButton.transform.SetParent(ActionSpacer, false);
        AtkButtons.Add(MagicAttackButton);

        if(HerosToManage[0].GetComponent<HeroStateMachine>().hero.MagicAttacks.Count > 0)
        {
            foreach(BaseAttack magicAtk in HerosToManage[0].GetComponent<HeroStateMachine>().hero.MagicAttacks)
            {
                GameObject magicButton = Instantiate(MagicButton) as GameObject;
                Text magicButtonText = magicButton.transform.Find("Text").gameObject.GetComponent<Text>(); 
                magicButtonText.text = magicAtk.attackName;
                AttacksButton ATB = magicButton.GetComponent<AttacksButton>();
                ATB.magicAttackToPerform = magicAtk;
                magicButton.transform.SetParent(MagicSpacer, false);
                AtkButtons.Add(magicButton);
            }

        }
        else
        {
            MagicAttackButton.GetComponent<Button>().interactable = false;
        }
    }

    public void Input3(BaseAttack chosenMagic) //chosen magic attack
    {
        HeroChoice.Attacker = HerosToManage [0].name;
        HeroChoice.AttackersGameObject = HerosToManage [0];
        HeroChoice.Type = "Hero";

        HeroChoice.ChosenAttack = chosenMagic;
        MagicPanel.SetActive(false);
        EnemySelectPanel.SetActive(true);
    }

    public void Input4()//switching to Magic Attacks
    {
        ActionPanel.SetActive(false);
        MagicPanel.SetActive(true);
    }
}
