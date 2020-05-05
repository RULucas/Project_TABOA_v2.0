using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class GameManager : MonoBehaviour
{

    public GameObject fightArena;

    public GameObject player;
    public ActionManager playerActionManager;

    public GameObject boss;
    private CharacterStats bossStats;
    public ActionManager bossActionManager;

    public Slider playerHealth;
    public Slider playerStamina;
    public Slider bossHealth;

    private int fightNumber;
    private string path;
    private string arenaName;

    float timeToDecress = 0;
    float value=5f;

    void Awake()
    {
        arenaName = fightArena.name.Replace(" ","");
        path = "D:/Documentos/Unity/Fight Logs TABFA v2/FighInfo"+arenaName+"0.txt";
        //Create file if it doesn't exist
        if (!File.Exists(path))
            PlayerPrefs.SetInt("FightNumberTABFAv2"+arenaName+"", 0);
    }

    void Start()
    {
        bossStats = boss.GetComponent<CharacterStats>();
        bossActionManager.ResetAttack();
        playerHealth = player.GetComponent<CharacterStats>().health;
        playerStamina= player.GetComponent<CharacterStats>().stamina;
        bossHealth= boss.GetComponent<CharacterStats>().health;
    }

    void Update()
    {
        timeToDecress += value* Time.deltaTime;
        /*if(timeToDecress>=1)
        {
            bossStats.currentHealth -= 1;
            bossStats.health.value -= 1;
            timeToDecress = 0;
        }*/
    }
    
    public void TakeDamage(CharacterStats stats, int damage)
    {
        //stats.currentHealth -= damage;
        stats.currentHealth = stats.currentHealth - (stats.maxHealth * damage / 100);
        stats.damageReceived = damage;
        //stats.health.value -= damage;
        stats.health.value = stats.health.value - (stats.health.value * damage / 100);

        //Debug.Log("DAMAGE DEALT BY "+stats.characterName + " : " + damage);
        /*if (stats.currentHealth <= 0)
        {
            File.AppendAllText(path, stats.characterName);
            fightNumber += 1;
            PlayerPrefs.SetInt("FightNumberTABOAv2", fightNumber);
            //SceneManager.LoadScene("SampleScene");
            if (stats.tag == "Boss")
                boss.GetComponent<BossAgent>().SetReward(-1f);
            else
                boss.GetComponent<BossAgent>().SetReward(1f);
            boss.GetComponent<BossAgent>().EndEpisode();
        }*/
    }

    public void SaveInfo(CharacterStats bossStats, CharacterStats playerStats, BaseAttack attackChosen,float distance)
    {
        //Content of the file 
        //  BOSS HEALT , ATTACK ID (DONE) , ATTACK DAMAGE , DISTANCE TO TARGET , PLAYER HEALTH
        string content = bossStats.currentHealth.ToString() + ";" + attackChosen.attackID.ToString() + ";" + attackChosen.attackDamage.ToString() + ";" + distance.ToString() + ";" + playerStats.currentHealth.ToString() + "\n";
        fightNumber = PlayerPrefs.GetInt("FightNumberTABFAv2" + arenaName + "", 0);
        path = "D:/Documentos/Unity/Fight Logs TABFA v2/FighInfo" + arenaName + "" + fightNumber.ToString() + ".txt";
       // if (!File.Exists(path))
            File.AppendAllText(path, content);
    }
    
}
