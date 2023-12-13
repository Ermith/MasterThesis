using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityCameraController : MonoBehaviour
{
    private Directions lookDirection = Directions.None;
    public void SetOrientation(Directions dirs)
    {
        transform.localRotation = Quaternion.LookRotation(dirs.ToVector3(), Vector3.up);
    }

    private void Awake()
    {
        if (!lookDirection.None())
            transform.localRotation =
                Quaternion.LookRotation(
                    lookDirection.ToVector3());

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
