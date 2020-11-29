using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour
{
    private Gyroscope gyro;

    Quaternion mCordBase = Quaternion.Euler(90, 0, 0);
    Quaternion mInitGyro = Quaternion.identity;
    private bool setup = false;

    void Start()
    {
        Debug.LogFormat("transform.forward =" + transform.forward);
        // 0,0,1  
        //  0,-1,0 如果物体旋转过 就按照旋转后局部坐标系的z轴方向(蓝色轴) 的世界坐标系 就是forward

        //transform.rotation = Quaternion.FromToRotation(Vector3.up, transform.forward); 
        // 这个是rotation=0,0,0直接设置成的

        //transform.rotation = Quaternion.FromToRotation(Vector3.up, transform.forward) * transform.rotation;
        // 这个考虑了原来的姿态

        gyro = Input.gyro;
        gyro.enabled = true;

  
  
    }

 
    void Update()
    {
        //if (!setup && gyro.attitude.w != 0)
        //{
            
       //     setup = true;
       //     mInitGyro = Quaternion.Inverse(gyro.attitude);

       //     Debug.LogError("mInitGyro " + mInitGyro);

       // }
        transform.rotation = mCordBase * ConvertRotation(gyro.attitude);
    }

    private static Quaternion ConvertRotation(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }

}
