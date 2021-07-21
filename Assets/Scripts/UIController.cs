using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UIController : MonoBehaviour
{
    public static UIController instance;

    private void Awake()
    {
        instance = this;
    }

    public TMP_Text overheatedMessage;
    public Slider weaponTempSlider, healtSlider;

    public GameObject deathScreen;
    public TMP_Text deathText, healthText;

    public TMP_Text killsText, deathsText;

    public GameObject leaderboard;
    public LeaderBoardPlayer leaderBoardPlayerDisplay;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
