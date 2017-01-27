﻿/* 
 * Purpose: Defining the Enemies
 * Author: Fabian Subat
 * Date: 10.01.2016 - TBA
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    [Range(0.1f, 100.0f)]
    private float rotationSpeed = 3.0f;

    [SerializeField]
    [Range(0.1f, 100.0f)]
    private float moveSpeed = 3.0f;

    [SerializeField]
    [Range(0.1f, 100.0f)]
    private float attackRange = 6.0f;

    [SerializeField]
    [Range(0.1f, 100.0f)]
    private float viewRange = 15.0f;
    
    enum enemyTypes
    {
        Crawler = 1,
        Mosquito = 2,
        Mantis = 3
    }

    [SerializeField]
    enemyTypes enemyType;

    [SerializeField]
    [Range(0.1f, 100f)]
    private float switchDelay = 3f;
    private float elapsedSwitchDelay = 0f;

    private GameObject currentTarget;
    private GameObject possibleTarget;
    private float distanceToPlayer;
    private GameObject targetPlayer;

    public Slider HealthSlider;
    private float enemyfront;

    private bool isValidTarget = false;
    private DamageAbleObject dmgobjct;
    public GameObject PrimaryWeapon;
    private Weapon primaryWeapon;
    private MoveScript moveScript;

    public UnityEvent onEnemyDeath;
    private Vector3 movement;

    // Use this for initialization
    void Start()
    {
        dmgobjct = GetComponent<DamageAbleObject>();
        if(dmgobjct != null)
            dmgobjct.OnDeath += Dmgobjct_OnDeath;

        if(PrimaryWeapon != null)
            primaryWeapon = Instantiate(PrimaryWeapon, transform).GetComponent<Weapon>();
        
        moveScript = GetComponent<MoveScript>();
        if(moveScript != null)
            moveScript.AddGravity = false;

        SetUI();
    }

    /// <summary>
    /// Called as the Entity gets destroyed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Dmgobjct_OnDeath(object sender, System.EventArgs e)
    {
        Destroy(gameObject);
        onEnemyDeath.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        CheckAlivePlayers();
        SetUI();

        if (isValidTarget) //On player found
        {
            //Calculate Distance between Enemyinstance and Player and switch target if delay allows
            GameObject nearestTarget = FindClosestPlayer();
            if (currentTarget == null)
            {
                currentTarget = nearestTarget;
            }
            else if (currentTarget != nearestTarget && possibleTarget != nearestTarget)
            {
                possibleTarget = nearestTarget;
                elapsedSwitchDelay = 0f;
            }
            else if(nearestTarget == possibleTarget)
            {
                elapsedSwitchDelay += Time.deltaTime;
                if(elapsedSwitchDelay >= switchDelay)
                {
                    currentTarget = possibleTarget;
                }
            }

            distanceToPlayer = Vector3.Distance(currentTarget.transform.position, transform.position);

            //Look at Player
            transform.rotation = Quaternion.Slerp(transform.rotation
                                                 , Quaternion.LookRotation(currentTarget.transform.position - transform.position)
                                                 , rotationSpeed * Time.deltaTime);

            //Follow Player
            if (attackRange < distanceToPlayer && distanceToPlayer < viewRange)
            {
                movement = transform.forward * moveSpeed * Time.deltaTime * 100;
                moveScript.Move(movement);
            }
            else if (distanceToPlayer < viewRange)
            {
                targetPlayer = null;
                isValidTarget = false;
            }
            if (distanceToPlayer < attackRange)
            {
                //TryShoot();
                if(primaryWeapon != null)
                    primaryWeapon.PrimaryAttack(transform.position, transform.forward, enemyfront);
            }

            enemyfront = transform.eulerAngles.y;
        }
    }

    private void FixedUpdate()
    {
        //physics.velocity = movement + Physics.gravity;
        //movement = Vector3.zero;
    }

    private void SetUI()
    {
        HealthSlider.value = dmgobjct.Health;
    }

    /// <summary>
    /// Called as soon as the targetplayer is in attack range.
    /// </summary>
    private void TryShoot()
    {
        //Maybe TODO : Create Specific Enemy Weapon Prefab
        if (primaryWeapon != null)
        {
            primaryWeapon.PrimaryAttack(transform.position, transform.forward, enemyfront);
        }
    }


    private GameObject FindClosestPlayer()
    {
        GameObject[] availablePlayers;
        availablePlayers = GameObject.FindGameObjectsWithTag("Player");
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject checkedTargetPlayer in availablePlayers)
        {
            Vector3 diff = checkedTargetPlayer.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                targetPlayer = checkedTargetPlayer;
                distance = curDistance;
            }
        }
        return targetPlayer;
    }

    private void CheckAlivePlayers()
    {
        if (targetPlayer != null)
            return;

        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");
        if (Players != null)
        {
            for (int i = 0; i < Players.Length; i++)
            {
                Player checkedAlivePlayer = Players[i].GetComponent<Player>();
                if (!checkedAlivePlayer.IsDead)
                {
                    isValidTarget = true;
                    //targetPlayer = Players[i];
                    return;
                }
                else
                {
                    isValidTarget = false;
                }

            }
        }
    }
}
