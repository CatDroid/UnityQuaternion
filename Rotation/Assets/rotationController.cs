using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotationController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }


    private float x = 0;
    private float y = 0;
    private float z = 0;
    private int frameCount = 0; // 30fps   10

    // Update is called once per frame
    void Update()
    {
        if (frameCount++ == 10)
        {
            frameCount = 0;

            if (y < 60)
            {
                y += 5;
            }
            else
            if ( x < 30)
            {
                x += 5;
            }
            else if (z < 45)
            {
                z += 5;
            }

            transform.localEulerAngles = new Vector3(x, y, z);
        }
        
    }
}
