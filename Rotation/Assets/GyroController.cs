using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroController : MonoBehaviour
{
    public static bool gyroAvaiable;

    private Quaternion calibration = Quaternion.identity;
    private Quaternion cameraBase = Quaternion.identity;
    private bool debug = true;
   
    private bool gyroEnabled = true;
    private Quaternion gyroInitialRotation;
    //public static bool gyroOff;
    private Quaternion initialRotation;

    private const float lowPassFilterFactor = 0.1f; // Quaternion.Slerp
    // private Quaternion offsetRotation;

    private Quaternion referanceRotation = Quaternion.identity;


    private Quaternion baseOrientation = Quaternion.Euler(90f, 0f, 0f);
    private Quaternion baseOrientationRotationFix = Quaternion.identity;


    // Methods
    private void AttachGyro()
    {
        this.gyroEnabled = true;
        this.ResetBaseOrientation();
        this.UpdateCalibration(true);
        this.UpdateCameraBaseRotation(true);
        this.RecalculateReferenceRotation();
    }

    private void Awake()
    {
        gyroAvaiable = SystemInfo.supportsGyroscope;

        Debug.Log("gyroAvaiable is " + gyroAvaiable);
    }

    private static Quaternion ConvertRotation(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w); // 右手坐标系转左手坐标系
    }

    private void DetachGyro()
    {
        this.gyroEnabled = false;
    }

    private readonly Quaternion baseIdentity = Quaternion.Euler(90f, 0f, 0f);
    private readonly Quaternion landscapeRight = Quaternion.Euler(0, 0, 90);
    private readonly Quaternion landscapeLeft = Quaternion.Euler(0, 0, -90);
    private readonly Quaternion upsideDown = Quaternion.Euler(0, 0, 180);

    private Quaternion GetRotFix()
    {
#if UNITY_3_5
        if (Screen.orientation == ScreenOrientation.Portrait)
            return Quaternion.identity;
        if (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.Landscape)
            return landscapeLeft;     
        if (Screen.orientation == ScreenOrientation.LandscapeRight)
            return landscapeRight;
        if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
            return upsideDown;
        return Quaternion.identity;
#else
        return Quaternion.identity;
#endif
    }

    private void ResetBaseOrientation()
    {
        this.baseOrientationRotationFix = this.GetRotFix();
        this.baseOrientation = this.baseOrientationRotationFix * this.baseIdentity;
        //  手机一般竖着的方向 与 陀螺仪水平 方向 之间的旋转
    }

    // ????
    private void UpdateCalibration(bool onlyHorizontal)
    {
        if (onlyHorizontal)
        {
            Quaternion temp = Input.gyro.attitude;
            float result = temp.x * temp.x + temp.y * temp.y + temp.z * temp.z + temp.w * temp.w;
            result = Mathf.Sqrt(result);
            Debug.LogError("result = " + result); //  陀螺仪给的是单位四元素  

            Vector3 toDirection = Input.gyro.attitude * -Vector3.forward; // 单位四元素  * 单位向量  结果不是单位的~~ 
            Debug.LogWarning(" toDirection " + toDirection); // 这个不一定是单位向量 ?? 

            toDirection.z = 0f; // XOY 平面 

            if (toDirection == Vector3.zero)
            {
                Debug.LogError("UpdateCalibration ------------- Zero  ");
                this.calibration = Quaternion.identity;
            }
            else
            {
                this.calibration = Quaternion.FromToRotation(this.baseOrientationRotationFix * Vector3.up , toDirection);
                Debug.LogError("UpdateCalibration ----------- this.baseOrientationRotationFix * Vector3.up = " + (this.baseOrientationRotationFix * Vector3.up));

                Vector3 one;
                one.x = 0.3F;
                one.y = 0.9F;
                one.z = 0F;

                Vector3 two;
                two.x = 0.0F;
                two.y = 1.0F;
                two.z = 0.0F; // FromToRotation 返回也不一定是单位四元素

                Debug.LogWarning("FromToRotation = " + Quaternion.FromToRotation(one, two));
            }
            // 一开始陀螺仪没有打开 就只是 0,0,0,1 
            // 打开之后 点击UpdateCalibration  会打印不同的值 比如
            // toDirection (0.3, 0.9, 0.0) 
            // this.baseOrientationRotationFix * Vector3.up = (0.0, 1.0, 0.0)
            // this.calibration (0.0, 0.0, -0.2, 1.0) -> 这个不是单位四元数 ?? 而且w=1.0 代表旋转角是0?
        }
        else
        {
            this.calibration = Input.gyro.attitude;
        }

        Debug.LogWarning(" this.calibration " + this.calibration);
    }


    private void UpdateCameraBaseRotation(bool onlyHorizontal)
    {
        Debug.LogErrorFormat("UpdateCameraBaseRotation {0} ", onlyHorizontal);

        if (onlyHorizontal)
        {
            Vector3 forward = transform.forward;

            Debug.LogErrorFormat("transform.forward is  {0} {1} {2}",
                forward.x,
                forward.y,
                forward.z); // 不一定是 0,0,1 如果在反射面板调整了摄像机初始角度

            forward.y = 0f; // XOZ 平面  不包含高度 只计算在水平面的转角  

            if (forward == Vector3.zero)
            {
                this.cameraBase = Quaternion.identity;
            }
            else
            {
                // 世界坐标系中的向前方向 与  摄像机在水平面上往前的方向 的旋转  
                this.cameraBase = Quaternion.FromToRotation(Vector3.forward, forward);

                Debug.LogErrorFormat("cameraBase  is  {0} {1} {2} {3}",
                     this.cameraBase.x,
                     this.cameraBase.y,
                     this.cameraBase.z,
                     this.cameraBase.w);
            }
        }
        else
        {
            this.cameraBase = base.transform.rotation;
        }
    }

    private void RecalculateReferenceRotation()
    {
        //this.referanceRotation = Quaternion.Inverse(this.baseOrientation) * Quaternion.Inverse(this.calibration);
        this.referanceRotation = this.baseOrientation *  this.calibration ;
    }


    protected void Start()
    {
         enabled = true;

        Input.gyro.enabled = true;
        
        this.AttachGyro();

        this.initialRotation =   transform.localRotation;
        this.gyroInitialRotation = Input.gyro.attitude;

        Debug.Log("eanble  gyro " + Input.gyro.enabled);
    }

    private void Update()
    {
        // gyroOff = PlayerPrefs.GetInt("gyro-off") == 1;
        // Debug.Log("gyroOff is " + gyroOff + " this.gyroEnabled " + this.gyroEnabled);

        //Debug.LogWarningFormat("gyro identity is {0} {1} {2} {3}",
        //      Quaternion.identity.x,
        //      Quaternion.identity.y,
        //      Quaternion.identity.z,
        //      Quaternion.identity.w
        //      );
        // 0 0 0 w=1

        if (this.gyroEnabled)
        {

            //Debug.LogErrorFormat("gyro attitude is  {0} {1} {2} {3}",
            //    Input.gyro.attitude.x,
            //     Input.gyro.attitude.y,
            //    Input.gyro.attitude.z,
            //    Input.gyro.attitude.w
            //    );

            // base.transform.localRotation = Quaternion.Slerp(
            //     base.transform.localRotation, 
            //      this.cameraBase * (
            //         ConvertRotation(this.referanceRotation * Input.gyro.attitude) * this.GetRotFix()
            //      ), 
            //      0.5f);//0.1f lowPassFilterFactor

            // base.transform.localRotation = this.cameraBase * (
            //           ConvertRotation(this.referanceRotation * Input.gyro.attitude) * this.GetRotFix()
            //          );
            base.transform.localRotation = this.cameraBase * this.referanceRotation *  (
                      ConvertRotation( Input.gyro.attitude)
                    );
            // this.cameraBase  如果没有了这个 那么场景中调整摄像机的旋转角度就没有作用了
        }
    }

 
    protected void OnGUI()
    {
        if (!debug) return;

        GUILayout.Label("Orientation: " + Screen.orientation);
        GUILayout.Label("Calibration: " + calibration);
        GUILayout.Label("Camera base: " + cameraBase);
        GUILayout.Label("input.gyro.attitude: " + Input.gyro.attitude);
        GUILayout.Label("transform.rotation: " + transform.rotation);
        if (GUILayout.Button("On/off gyro: " + Input.gyro.enabled, GUILayout.Height(100)))
        {
            Input.gyro.enabled = !Input.gyro.enabled;
        }
        if (GUILayout.Button("On/off gyro controller: " + gyroEnabled, GUILayout.Height(100)))
        {
            if (gyroEnabled)
            {
                DetachGyro();
            }
            else
            {
                AttachGyro();
            }
        }
        if (GUILayout.Button("Update gyro calibration (Horizontal only)", GUILayout.Height(80)))
        {
            UpdateCalibration(true);
        }
        if (GUILayout.Button("Update camera base rotation (Horizontal only)", GUILayout.Height(80)))
        {
            UpdateCameraBaseRotation(true);
        }
        if (GUILayout.Button("Reset base orientation", GUILayout.Height(80)))
        {
            ResetBaseOrientation();
        }
        if (GUILayout.Button("Reset camera rotation", GUILayout.Height(80)))
        {
            transform.rotation = Quaternion.identity;
        }
       
    }
 
}