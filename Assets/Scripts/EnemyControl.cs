using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyControl : MonoBehaviour
{
    public NavMeshAgent enemyAgent;
    public GameObject patrolRoot;
    public Transform[] patrolSpots; //巡逻点位；
    public int nextIndex = 0;

    public ControlPlayer playerControl;
    public float alertDistance = 10; //警戒距离；

    public GameObject bullet;
    public float bulletStartSpeed = 100; //子弹初速度；
    public Transform firePositionTransform;
    public float sightAngle = 30; //视野夹角；
    public bool isFiring; //正在开火；
    public float fireInternal = 0.1f; //开火间隔；

    public ControlBotAnimator enemyAnimator;

    // Start is called before the first frame update
    void Start()
    {
        enemyAgent = this.GetComponent<NavMeshAgent>();
        playerControl = GameObject.FindObjectOfType<ControlPlayer>();
        enemyAnimator = this.GetComponent<ControlBotAnimator>();

        if (patrolRoot)
        {
            patrolSpots = patrolRoot.GetComponentsInChildren<Transform>();
            SetNextDestnation();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(enemyAgent.transform.position, playerControl.enemySightPositionTransform.position) < alertDistance)
        {
            enemyAgent.destination = playerControl.enemySightPositionTransform.position; //跟随玩家；

            //动画：
            enemyAnimator.moveSpeed = enemyAgent.speed;
            enemyAnimator.alerted = true;
        }
        else
        {
            //动画：
            enemyAnimator.moveSpeed = enemyAgent.speed;
            enemyAnimator.alerted = false;
            //自动巡逻：
            if (!enemyAgent.pathPending && enemyAgent.remainingDistance < 5)
            //NavMeshAgent.pathPending:  /是正在计算过程中而尚未就绪的路径吗？/ Is a path in the process of being computed but not yet ready?
            {
                SetNextDestnation();
            }
        }

        FireControl();
    }

    //看向下一个目的地：
    private void SetNextDestnation()
    {
        if (patrolSpots.Length <= 1)
            return;

        enemyAgent.destination = patrolSpots[nextIndex].position;
        nextIndex = (nextIndex + 1) % patrolSpots.Length ;
    }

    //开火控制：
    void FireControl()
    {
        Vector3 playerDirection = (playerControl.enemySightPositionTransform.position - firePositionTransform.position).normalized;
        if (Vector3.Angle(playerDirection, firePositionTransform.forward) < sightAngle)
        {
            if (!isFiring)
            {
                isFiring = true;
                StartCoroutine("Fire", playerDirection);
            }
        }
        else
        {
            if (isFiring)
            {
                isFiring = false;
                StopCoroutine("Fire");
            }
        }
    }

    IEnumerator Fire(Vector3 playerDirection)
    {
        //yield return new WaitForSeconds(fireInternal);
        while (isFiring)
        {
            Quaternion rotation = Quaternion.LookRotation(playerDirection);
            GameObject newBullet = Instantiate(bullet, firePositionTransform.position, rotation);
            newBullet.GetComponent<BulletControl>().SetHost(-1);
            newBullet.GetComponent<Rigidbody>().velocity = newBullet.transform.forward * bulletStartSpeed;
            Destroy(newBullet, 5);

            //动画：
            enemyAnimator.TriggerAttack();

            yield return new WaitForSeconds(fireInternal);
        }
    }
}