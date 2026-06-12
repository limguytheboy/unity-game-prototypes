using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PlayerData
{
    //json file은 배열저장안됨
    public string Name;
    public int age;
    //업그래이드 할때 몇퍼 올릴거,몇번 올릴거,지금까지 올린수,업글비용,업글비용 상승율
    //속도,풀성장속도,지구크기,코인버는비율
    //public float[,] UpgradeList;// = new float[5, 4] { { 5, 5, 2, 5 }, { 150, 100, 20, 300 }, { 0, 0, 0, 0 }, { 10, 8, 13, 15 }, { 25, 30, 100, 10 } };

    //cutted grass count, Upgrade count, coin collected count, Challenge complete count
    //objective, progresive, prize
    //public int[,] ChallengeList;// = new int[3, 4] { { 5000, 50, 100, 5 }, { 0, 0, 0, 0 }, { 50, 20, 10, 100 } };

    //Mower Description
    //purchase cost, Upgrade cost, boost state percentage, Upgrade count, max upgrade
    //public int[,] MDList;// = new int[5, 5] { { 40, 10, 20, 0, 30 }, { 50, 15, 15, 0, 30 }, { 45, 10, 15, 0, 30 }, { 65, 20, 25, 0, 30 }, { 70, 20, 20, 0, 30 } };
}

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    string path;
    string filename = "save";
    PlayerData playerData = new PlayerData();
    void Awake()
    {
        #region singletone
        if (instance == null)
        {
            instance = this;
        }
        else if(instance != true)
        {
            Destroy(instance.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
        #endregion

        path = Application.persistentDataPath + "/";
    }
    // Start is called before the first frame update
    void Start()
    {
        string data = JsonUtility.ToJson(playerData);

        print(path);
        File.WriteAllText(path + filename,data);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
