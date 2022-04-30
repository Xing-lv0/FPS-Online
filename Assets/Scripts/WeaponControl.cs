using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class WeaponControl : NetworkBehaviour
{
    public int ID; //武器所有者ID;
    public GameObject bullet; //子弹；
    public Transform bulletStartTransform; //子弹初始状态；
    public float bulletStartSpeed = 100; //子弹初速度；
    public bool keepFire = false; //连射；
    public float fireInterval = 0.1f; //连射间隔；

    //装填子弹：
    public float bulletLimit = 30; //弹匣容量；
    public float currentBulletCount = 30; //当前剩余子弹数；
    public float reloadTime = 2; //换弹动作时长；
    public float timeToReload = 0.5f; //停止开火后开始填充子弹的时间；
    public Slider bulletSlider;

    //后坐力动画：
    //public bool haveRecoil = false;
    public Transform defaultPositionTransform; //武器初始位置；
    public Transform recoilPositionTransform; //后坐力推至的位置；
    public float lerpRatio = 0.2f; //插值参数；

    //音效：
    public AudioSource shotAudio;

    //机瞄视角控制：
    public Camera mainCamera;
    public Camera weaponCamera;
    public Vector3 weaponCameraDefaultPosition;
    public Vector3 weaponCameraAimPosition;
    public float defaultFOV = 60;
    public float aimFOV = 30;
    public float viewLerpRatio = 0.2f;

    public NetworkIdentity parentID; //玩家ID；

    // Start is called before the first frame update
    void Start()
    {
        parentID = transform.parent.parent.parent.parent.GetComponent<NetworkIdentity>();
        if (parentID.isLocalPlayer)
        {
            bullet = GameObject.FindWithTag("BulletInstance");
            bulletSlider = GameObject.FindWithTag("Player_Bullet_Slider").GetComponent<Slider>();
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            weaponCamera = GameObject.FindGameObjectWithTag("WeaponCamera").GetComponent<Camera>();
            currentBulletCount = bulletLimit;

            if (bulletSlider)
            {
                bulletSlider.maxValue = bulletLimit;
                bulletSlider.value = currentBulletCount;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (parentID.isLocalPlayer)
        {
            FireControl();
            ViewChange();
        }

        //Ray ray = new Ray(bulletStartTransform.position, bulletStartTransform.forward);
        //Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);
    }

    //控制开火：
    void FireControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            keepFire = true;
            StartCoroutine("Fire");
        }
        if (Input.GetMouseButtonUp(0))
        {
            keepFire = false;
            StopCoroutine("Fire");

            //重装子弹；
            StopCoroutine("Reload");
            StartCoroutine("Reload");
        }
    }

    //协程开火：
    IEnumerator Fire()
    {
        while (keepFire && currentBulletCount >= 1)
        {
            if (bullet != null && bulletStartTransform != null)
            {
                //生成子弹：
                GameObject newBullet = Instantiate(bullet, bulletStartTransform.position, bulletStartTransform.rotation);
                newBullet.GetComponent<BulletControl>().SetHost(ID);
                newBullet.GetComponent<Rigidbody>().velocity = newBullet.transform.forward * bulletStartSpeed;

                PlayShotAudio(); //枪声；

                //后坐力动画：
                //haveRecoil = true;
                StopCoroutine("RecoilAnimation");
                StartCoroutine("RecoilAnimation");

                currentBulletCount -= 1;
                if (bulletSlider)
                {
                    bulletSlider.value = currentBulletCount;
                }

                Destroy(newBullet, 5);
            }
            yield return new WaitForSeconds(fireInterval); //中断函数，等待；
        }
    }

    //重装子弹：
    IEnumerator Reload()
    {
        yield return new WaitForSeconds(timeToReload);

        while (!keepFire && currentBulletCount < bulletLimit)
        {
            currentBulletCount += bulletLimit / reloadTime * Time.deltaTime;
            if (bulletSlider)
            {
                bulletSlider.value = currentBulletCount;
            }

            yield return null;
        }
    }

    //协程后坐力动画：
    IEnumerator RecoilAnimation()
    {
        yield return null;

        if (defaultPositionTransform != null && recoilPositionTransform != null) 
        {
            //后坐：
            //if (haveRecoil)
            {
                while (this.transform.localPosition != recoilPositionTransform.localPosition)
                {
                    //print("this:" + this.transform.localPosition);
                    //print("default:" + recoilPositionTransform.localPosition);
                    //print("recoil:" + recoilPositionTransform.localPosition);
                    this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, recoilPositionTransform.localPosition, lerpRatio * 4);
                    yield return null;
                }
            }

            //恢复:
            while (this.transform.localPosition != defaultPositionTransform.localPosition)
            {
                this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, defaultPositionTransform.localPosition, lerpRatio);
                yield return null;
            }
        }
    }

    private void PlayShotAudio()
    {
        if (shotAudio)
            shotAudio.Play();
    }

    private void ViewChange()
    {
        if (Input.GetMouseButtonDown(1))
        {
            StopCoroutine("ToDefaultView");
            StartCoroutine("ToAimView");
        }

        if (Input.GetMouseButtonUp(1))
        {
            StopCoroutine("ToAimView");
            StartCoroutine("ToDefaultView");
        }
    }

    //切换到瞄镜头；
    IEnumerator ToAimView()
    {
        //print("to AimPosition: " + weaponCameraAimPosition);
        while (weaponCamera.transform.localPosition != weaponCameraAimPosition)
        {
            weaponCamera.transform.localPosition = Vector3.Lerp(weaponCamera.transform.localPosition,
                weaponCameraAimPosition, viewLerpRatio);
            weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, aimFOV, viewLerpRatio);
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, aimFOV, viewLerpRatio);

            yield return null; //等待1帧；
        }
    }

    //切换到普通镜头；
    IEnumerator ToDefaultView()
    {
        //print("to DefaultPosition: " + weaponCameraDefaultPosition);
        while (weaponCamera.transform.localPosition != weaponCameraDefaultPosition)
        {
            weaponCamera.transform.localPosition = Vector3.Lerp(weaponCamera.transform.localPosition,
                weaponCameraDefaultPosition, viewLerpRatio);
            weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, defaultFOV, viewLerpRatio);
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, defaultFOV, viewLerpRatio);

            yield return null; //等待1帧；
        }
    }
}
