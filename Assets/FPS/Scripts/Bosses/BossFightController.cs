using System;
using System.Collections;
using System.Collections.Generic;
using FPS.Scripts.Bosses;
using UnityEngine;

public class BossFightController : MonoBehaviour
{
    [SerializeField]
    private BossAttackPattern attackPattern;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (attackPattern.CanAttack())
        {
            attackPattern.Attack();
        }
    }
}
