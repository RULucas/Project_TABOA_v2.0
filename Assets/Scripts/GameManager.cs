using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class GameManager : MonoBehaviour
{

    #region Singleton 

    public static GameManager instance;

    void Awake()
    {
        instance = this;
        path = "D:/Documentos/Unity/Fight Logs TABOA v2/FighInfo0.txt";
        //Create file if it doesn't exist
        if (!File.Exists(path))
            PlayerPrefs.SetInt("FightNumberTABOAv2", 0);
    }

    #endregion

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

    float timeToDecress = 0;
    float value=0.5f;

    void Start()
    {
        bossStats = boss.GetComponent<CharacterStats>();
        bossActionManager.ResetAttack();
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
        stats.currentHealth -= damage;
        stats.damageReceived = damage;
        stats.health.value -= damage;

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
        fightNumber = PlayerPrefs.GetInt("FightNumberTABOAv2", 0);
        path = "D:/Documentos/Unity/Fight Logs TABOA v2/FighInfo" + fightNumber.ToString() + ".txt";
       // if (!File.Exists(path))
            File.AppendAllText(path, content);
    }
}
