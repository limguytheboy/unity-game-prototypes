using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    //추가
    //다른 섬(동물키우는 섬,풀 씨앗 업그래이드 해주는 섬(풀의 가격 상승시킴)), 섬을 구매, 랭크(지금까지 깍은 잔디 수,가진 돈의 양), 배경

    //애니매이션
    //풀깍이는 모션, 잔디깍이 뒤에 풀이 날리는 모션, 잔디깍는 소리, 잔디깍이 소리,

    GM gm;
    UIM uim;
    float EarthRadius;
    float GrowingSpeed = 1.1f;
    bool touched;
    public float M;
    float c;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GM").GetComponent<GM>();
        uim = GameObject.Find("GM").GetComponent<UIM>();
        EarthRadius = gm.Earth.transform.localScale.x/2;
        GrowingSpeed = gm.GrowingSpeed;
        transform.position = transform.up * EarthRadius;
    }

    // Update is called once per frame
    void Update()
    {
        M = EarthRadius - Vector3.Distance(new Vector3(0, 0, 0), transform.position);
        if (touched == true)
        {
            transform.position = transform.position + transform.up * GrowingSpeed * Time.deltaTime;
        }
        if(Vector3.Distance(new Vector3(0,0,0),transform.position) >= EarthRadius)
        {
            touched = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        ResetValue();
        if(collision.gameObject.CompareTag("Mower"))
        {
            if(Vector3.Distance(new Vector3(0, 0, 0), transform.position) >= EarthRadius-0.65f)
            {
                //풀이 바닥에 박히는 정도
                transform.position = transform.position - (transform.up * 0.436f) + 
                    (transform.up * (EarthRadius - Vector3.Distance(new Vector3(0, 0, 0), transform.position)));
                gm.coin += (EarthRadius - Vector3.Distance(new Vector3(0, 0, 0), transform.position)) / 10 * gm.coinMultiplier;
                
                //challenge count
                uim.ChallengeList[1, 0]++;
                uim.CCC += (EarthRadius - Vector3.Distance(new Vector3(0, 0, 0), transform.position)) / 10 * gm.coinMultiplier;
            }
            
            touched = true;
        }
    }

    void ResetValue()
    {
        EarthRadius = gm.Earth.transform.localScale.x / 2;
        GrowingSpeed = gm.GrowingSpeed;
        transform.position = transform.up * EarthRadius;
    }
}
