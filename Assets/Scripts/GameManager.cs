using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    //Class Random Monster

    public RegionData curRegion;
 

    //SpawnPoints
    public string nextSpawnPoint;

    //Hero
    public GameObject heroCharacter;
    
    //Position
    public Vector3 nextHeroPosition;
    public Vector3 lastHeroPosition;

    //Scenes
    public string NextScene;
    public string LastScene;

    //Bools
    public bool isWalking = false;
    public bool canGetEncounter = false;
    public bool gotAttacked = false;

    //Enums
    public enum GameStates
    {
        WORLD_STATE,
        TOWN_STATE,
        BATTLE_STATE,
        IDLE,
    } 

    //Battle
    public List<GameObject> enemiesToBattle = new List<GameObject>();
    public int enemyAmount;

    public GameStates GameState;

    void Awake()
    {
        //check if instance exist
        if(instance == null)
        {
            //if not set the instance to this
            instance = this;

        }
        //if it exist but is not this instance
        else if(instance != this)
        {
            //destroy it
            Destroy(gameObject);
        }
        //set this to be not destroyable
        DontDestroyOnLoad(gameObject);
        if(!GameObject.Find("HeroCharacter"))
        {
            GameObject Hero = Instantiate(heroCharacter, nextHeroPosition,Quaternion.identity) as GameObject;
            Hero.name = "HeroCharacter";
        }
    }

    public void Update()
    {
        switch(GameState)
        {
        case(GameStates.WORLD_STATE):
            if(isWalking)
            {
                RandomEncounter();
            }
            if(gotAttacked == true)
            {
                GameState = GameStates.BATTLE_STATE;
            }
        break;
        case(GameStates.TOWN_STATE):

        break;
        case(GameStates.BATTLE_STATE):
            //Load Battle Scene
            StartBattle();
            //Go Idle
            GameState = GameStates.IDLE;
        break;
        case(GameStates.IDLE):
        
        break;
        }
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(NextScene);
    }

    public void LoadSceneAfterBattle()
    {
        SceneManager.LoadScene(LastScene);
    }

    void RandomEncounter()
    {
        if(isWalking && canGetEncounter)
        {
            if(Random.Range(0, 1000) < 10)
            {
                // Debug.Log("I got an encounter.");
                gotAttacked = true;
            }
        }
    }

    void StartBattle()
    {
        //Amount of Enemies
        enemyAmount = Random.Range(1, curRegion.maxAmountEnemies+1);
        //Which enemies
        for(int i = 0; i < enemyAmount; i++)
        {
            enemiesToBattle.Add(curRegion.possibleEnemies[Random.Range(0,curRegion.possibleEnemies.Count)]);

        }
        //Hero
        lastHeroPosition = GameObject.Find("HeroCharacter").gameObject.transform.position;
        nextHeroPosition = lastHeroPosition;
        LastScene = SceneManager.GetActiveScene().name;

        //Load Level
        SceneManager.LoadScene(curRegion.BattleScene);

        //Reset Hero
        isWalking = false;
        gotAttacked = false;
        canGetEncounter = false;
    }

}
