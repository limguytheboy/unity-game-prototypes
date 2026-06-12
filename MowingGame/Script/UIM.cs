using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;

public class UIM : MonoBehaviour
{
    public TMP_Text Coin;
    public TMP_Text Ruby;
    public GM gm;

    public GameObject Model;

    public float CCC;
    bool CMO;

    byte PorU;

    //Upgrade:0,Challenge:1,boost:2
    public GameObject[] Menu = new GameObject[3];

    //Upgrade menu upgrade cost text
    public TMP_Text[] UMT = new TMP_Text[4];

    //Upgrade menu upgraded time count text
    public TMP_Text[] UCT = new TMP_Text[4];
    
    //Challenge menu collect prise text
    public TMP_Text[] CMT = new TMP_Text[4];

    //Challenge menu remeaning time count text
    public TMP_Text[] CCT = new TMP_Text[4];

    //Mower Description
    public TMP_Text[] MD = new TMP_Text[5];

    //Mower Purchase and Upgrade text
    public TMP_Text[] MPU = new TMP_Text[5];

    public GameObject[] MowerList = new GameObject[5];

    //업그래이드 할때 몇퍼 올릴거,몇번 올릴거,지금까지 올린수,업글비용,업글비용 상승율
    //속도,풀성장속도,지구크기,코인버는비율
    public float[,] UpgradeList = new float[5, 4]{ { 5 , 5 , 2 , 5 } , { 150 , 100 , 20 , 300 } , { 0 , 0 , 0 , 0} , { 10, 8, 13, 15 }, { 25, 30, 100, 10 } };

    //cutted grass count, Upgrade count, coin collected count, Challenge complete count
    //objective, progresive, prize
    public int[,] ChallengeList = new int[3, 4]{ { 5000, 50, 100, 5 }, { 0, 0, 0, 0 }, { 50 , 20 , 10 , 100} };

    //Mower Description
    //purchase cost, Upgrade cost, boost state percentage, Upgrade count, max upgrade
    public int[,] MDList = new int[5,5] { { 40, 10, 20, 0, 30 },{ 50, 15, 15, 0, 30 },{ 45, 10, 15, 0, 30 },{ 65, 20, 25, 0, 30 },{ 70, 20, 20, 0, 30 } };

    // Start is called before the first frame update
    void Start()
    {
        Coin.text = gm.coin.ToString();
        Ruby.text = gm.Ruby.ToString();

        //upgrade menu setup
        for (int i = 0; i < 4; i++)
            UMT[i].text = "$ " + UpgradeList[3, i].ToString("F2");
        for(int i = 0; i < 4; i++)
            UCT[i].text = " " + UpgradeList[2, i] + " / " + UpgradeList[1, i];

        //challenge menu setup
        for (int i = 0; i < 4; i++)
            CMT[i].text = "R " + ChallengeList[2, i].ToString("F2");
        for (int i = 0; i < 4; i++)
            CCT[i].text = " " + ChallengeList[1, i] + " / " + ChallengeList[0, i];

        //Mower menu Setup
        for (int i = 0; i < 5; i++)
        {
            if (MDList[i, 0] > 0)
                MPU[i].text = "R " + MDList[i, 1];
            else
                MPU[i].text = "R " + MDList[i, 0];
        }
            
        for (int i = 0; i < 5; i++)
        {
            if (MDList[i, 0] > 0)
            {
                MD[i].text = " " + MDList[i,2];
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Coin.text = "$ " + gm.coin.ToString("F2");
        Ruby.text = "R " + gm.Ruby.ToString("F2");
        if(CMO)
        {
            ChallengeList[1, 2] = Mathf.FloorToInt(CCC);
            for (int i = 0; i < 4; i++)
                CMT[i].text = "R " + ChallengeList[2, i].ToString("F2");
            for (int i = 0; i < 4; i++)
                CCT[i].text = " " + ChallengeList[1, i] + " / " + ChallengeList[0, i];
        }
        //for (int i = 0; i < 4; i++)
        //    Debug.Log("1  " + ChallengeList[1, i] + " / " + ChallengeList[0, i]);
    }

    public void OpenUpgradeMenu(bool B)
    {
        if(B)
            Menu[0].SetActive(true);
        else if(!B)
            Menu[0].SetActive(false);
    } 
    
    public void OpenChallengeMenu(bool B)
    {
        if(B)
        {
            Menu[1].SetActive(true);
            CMO = true;
        }
        else if(!B)
        {
            Menu[1].SetActive(false);
            CMO = false;
        }
            
    }
    
    public void OpenMowerMenu(bool B)
    {
        if(B)
            Menu[2].SetActive(true);
        else if(!B)
            Menu[2].SetActive(false);
    }

    //업그레이드 매뉴 목록
    //잔디깍기의 속도,지구의 크기,풀의 가격,풀이 자라는 속도
    //
    //잔디깍기의 풀자르는 범위(잔디깍기 모양 업그래이드)

    public void Upgrades(int mul)
    {
        if (UpgradeList[3, mul] <= gm.coin && UpgradeList[1, mul] > UpgradeList[2, mul])
        {
            gm.coin -= UpgradeList[3, mul];
            UpgradeList[3, mul] *= (100 + UpgradeList[4, mul]) / 100;
            switch(mul)
            {
                case 0:
                    //mower speed
                    gm.MowerSpeed *= (100 + UpgradeList[0, mul])/100;
                    break;
                case 1:
                    //grass grow speed
                    gm.GrowingSpeed *= (100 + UpgradeList[0, mul]) / 100;
                    break;
                case 2:
                    //increase earth size
                    gm.Earth.transform.localScale *= (100 + UpgradeList[0, mul]) / 100;
                    Model.transform.localPosition = new Vector3(0,0, -gm.EarthRadius*1.0495f);
                    gm.coinMultiplier *= (100 + UpgradeList[0, mul] * 10) / 100;
                    break;
                case 3:
                    //coin multiplier
                    gm.coinMultiplier *= (100 + UpgradeList[0, mul]) / 100;
                    break;
                default:
                    break;
            }
            UpgradeList[2, mul]++;
            UCT[mul].text = " " + UpgradeList[2, mul] + " / " + UpgradeList[1, mul];
            UMT[mul].text = "$ " + UpgradeList[3, mul].ToString("F2");
            ChallengeList[1, 1]++;
        }        
    }

    public void Challengs(int num)
    {
        CCT[num].text = " " + ChallengeList[1, num] + " / " + ChallengeList[0, num];
        if (ChallengeList[1, num] >= ChallengeList[0, num])
        {
            ChallengeList[0, num] = ChallengeList[0, num] * 2;
            ChallengeList[1, num] = 0;
            gm.Ruby += ChallengeList[2, num];

            CMT[num].text = "R " + ChallengeList[2, num].ToString("F2");
            ChallengeList[1, 3]++;
        }
    }

    public void Mower(int num)
    {
        for (int i = 0; i < 5; i++)
        {
            if (MDList[i, 0] > 0)
                MPU[i].text = "R " + MDList[i, 1];
            else
                MPU[i].text = "R " + MDList[i, 0];
        }

        if (MDList[num, 3] > 0 && MDList[num,3] < MDList[num,4] && MDList[num, 0] <= gm.Ruby)
        {
            PorU = 1;
        }
        else if (MDList[num, 3] == 0 && MDList[num, 3] < MDList[num, 4] && MDList[num, 0] <= gm.Ruby)
        {
            PorU = 0;
        }
        else
        {
            PorU = 3;
        }

        if (PorU != 3)
        {
            switch(num)
            {
                case 0:
                    Debug.Log("1");
                    break;
                case 1:
                    Debug.Log("2");
                    break;
                case 2:
                    Debug.Log("3");
                    break;
                case 3:
                    Debug.Log("4");
                    break;
                case 5:
                    Debug.Log("5");
                    break;
            }

            gm.Ruby -= MDList[num, 0];
            MDList[num, 3]++;

            for (int i = 0; i < 5; i++)
            {
                if (MDList[i, 0] > 0)
                    MPU[i].text = "$ " + MDList[i, 1];
                else
                    MPU[i].text = "$ " + MDList[i, 0];
            }

            for (int i = 0; i < 5; i++)
                MD[i].text = " " + MDList[i, 2];
        }
    }
}
