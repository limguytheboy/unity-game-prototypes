using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public HoldChecker HC;

    public GameObject Player;

    public TMP_Text TIME;

    public int Min;
    public float Sec;

    public bool Ready;
    public bool Playing;


    // Start is called before the first frame update
    void Start()
    {
        Player.transform.position = new Vector3(3.5f, 1.7f, 3.7f);
        Player.transform.rotation = Quaternion.Euler(0, 270f, 0);

        PlayerScript(false);

        Ready = true;
        Playing = true;
    }

    //To start the game in this game rule, in hierarchy, change GM GameState to Ready, it is probably in play statement


    // Update is called once per frame
    void Update()
    {

    }

    public void GameOver()
    {
        PlayerScript(false);
    }

    void PlayerScript(bool able)
    {
        //Player.GetComponent<PlayerController>().enabled = able;
        //Player.GetComponentInChildren<Transform>().GetComponentInChildren<HoldChecker>().enabled = able;
    }
}
