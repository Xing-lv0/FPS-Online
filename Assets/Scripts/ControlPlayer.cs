using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ControlPlayer : NetworkBehaviour
{
    public int ID; //玩家ID;

    public float rotateSpeed = 180;
    [Range(1, 2)]
    public float rotateRatio = 1;
    public Transform playerTransform;
    public Transform viewTransform;
    private float rotateOffsetX; //x旋转偏移；
    public float xLimit = 90; //角度限制；

    public CharacterController controller;
    public float moveSpeed = 10; //水平移速；

    public float gravity = -9.8f;
    public float verticalVelocity = 0;
    public bool landed = false;
    public Transform landingCheckTransform; //落地；
    public float landingCheckRadius = 0.1f; //落地判定半径；
    public LayerMask floorLayer; //地面层；
    public float jumpHeight = 1; //跳跃高度；

    public ControlBotAnimator controlBotAnimator; //机器人动画控制；
    public Transform enemySightPositionTransform; //AI敌人视野判定点；

    public GameObject mainCameraObject; //主摄像机；

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        if (isLocalPlayer)
        {
            //GameObject mainCameraObject = GameObject.FindGameObjectWithTag("MainCamera");
            mainCameraObject.SetActive(true); //激活摄像头；
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer)
        {
            controller = this.GetComponent<CharacterController>();
            controlBotAnimator = this.GetComponent<ControlBotAnimator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isLocalPlayer)
        {
            RotateControl();
            MoveControl();
        }
    }

    void FixedUpdate()
    {
        
    }

    //视角旋转：
    void RotateControl()
    {
        if (playerTransform == null || viewTransform == null)
            return;

        float offsetX = Input.GetAxis("Mouse X"); //鼠标x横向偏移控制Player绕y轴水平转动；
        float offsetY = Input.GetAxis("Mouse Y"); //鼠标y纵向偏移控制View绕x轴垂直转动；

        playerTransform.Rotate(Vector3.up * offsetX * rotateSpeed * rotateRatio * Time.fixedDeltaTime); //横向；

        //纵向：
        //print(offsetY);
        rotateOffsetX += offsetY * rotateSpeed * rotateRatio * Time.fixedDeltaTime;
        rotateOffsetX = Mathf.Clamp(rotateOffsetX, -xLimit, xLimit);
        Quaternion currentLocalRotation = Quaternion.Euler(new Vector3(rotateOffsetX,viewTransform.localEulerAngles.y, viewTransform.localEulerAngles.z));
        viewTransform.localRotation = currentLocalRotation;
    }

    //位置移动：
    void MoveControl()
    {
        if (controller == null)
            return;

        float hInput = Input.GetAxis("Horizontal"); //横向移动；
        float vInput = Input.GetAxis("Vertical"); //前后移动；

        Vector3 moveVector = Vector3.zero;

        moveVector += this.transform.right * moveSpeed * hInput * Time.deltaTime;
        moveVector += this.transform.forward * moveSpeed * vInput * Time.deltaTime;

        //纵向移动：
        verticalVelocity += gravity * Time.deltaTime;

        moveVector += Vector3.up * verticalVelocity * Time.deltaTime; //加速；

        //落地检测：
        if (landingCheckTransform != null)
        {
            landed = Physics.CheckSphere(landingCheckTransform.position, landingCheckRadius, floorLayer);
            if (landed && verticalVelocity < 0) //检测到落地，且速度向下，则纵向速度减为0；
            {
                landed = true;
                verticalVelocity = 0;
            }
        }

        //跳跃：
        if (landed)
        {
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(2 * jumpHeight * (-gravity));
            }
        }

        controller.Move(moveVector);

        //设置动画参数：
        if (controlBotAnimator)
        {
            controlBotAnimator.moveSpeed = moveSpeed * vInput;
            controlBotAnimator.alerted = vInput != 0 ? true : false;
        }
    }
}
