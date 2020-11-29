using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroController : MonoBehaviour
{
    private Gyroscope gyro;

    // 相机初始的态势
    Quaternion cameraBase = Quaternion.Euler(90, 0, 0);

    void Start()
    {

        // 从Input中获取陀螺仪对象
        gyro = Input.gyro;
        // 开启陀螺仪
        gyro.enabled = true;
        // 获取陀螺仪的态势数据
        Debug.Log(gyro.attitude);
    }

    void Update()
    {
        // 陀螺仪的四元数 左乘 camera初始位置 
        //transform.rotation = cameraBase *  ConvertRotation(gyro.attitude) ;
    }

    // 将旋转从右手坐标系转换到左手坐标系
    private static Quaternion ConvertRotation(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }
}