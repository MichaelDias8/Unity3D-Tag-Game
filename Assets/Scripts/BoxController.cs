using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour {

    public void Update()
    {
        //move the box move back and forth in x direction 10 units 
        transform.position = new Vector3(Mathf.PingPong(Time.time * 5, 10) -25, transform.position.y, transform.position.z);
    }
}
