using System.Xml.Serialization;
using UnityEngine;

public class FollowingSprite : MonoBehaviour
{
    [SerializeField] private Camera spriteCam;

    [SerializeField] private GameObject target;

    [SerializeField] private RenderTexture rt;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Camera cam = Camera.main;

        PositionCamera(cam);

        CaptureImage();

        ScaleSprite(cam);

        //TestAngle(cam);
    }

    private void PositionCamera(Camera cam)
    {
        //As we know that the forward and right directions are perpendicular and will form a right angle triangle, we can use trigonometry to find the point of the camera.
        Vector3 dist = transform.position - cam.transform.position;

        float angle = Vector3.Angle(transform.right, dist);

        //We now have the hypotenuse, and want to find the adjecent, therefore we will use cos
        float offset = Mathf.Cos(angle * Mathf.PI / 180.0f) * dist.magnitude;
        
        spriteCam.transform.position = cam.transform.position + cam.transform.right * offset;

        spriteCam.transform.rotation = cam.transform.rotation;
    }

    private void CaptureImage()
    {
        spriteCam.targetTexture = rt;
    }

    private void ScaleSprite(Camera cam)
    {
        //We take the forward of the main camera and we flip it to get the rotation that the sprite needs to have to face the camera
        Vector3 dir = cam.transform.forward;

        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        //transform.localScale = target.GetComponent<Renderer>().bounds.extents * 2;
        //target.GetComponent<Renderer>().bounds.extents
    }

    private void TestAngle(Camera cam)
    {
        Debug.DrawLine(cam.transform.position, transform.position);
        Debug.DrawLine(transform.position, spriteCam.transform.position);
        Debug.DrawLine(cam.transform.position, spriteCam.transform.position);

        Vector3 dir1 = transform.position - spriteCam.transform.position;
        Vector3 dir2 = cam.transform.position - spriteCam.transform.position;

        float angle = Vector3.Angle(dir1, dir2);

        if (Mathf.RoundToInt(angle) != 90)
            print("Error: Function is not working!");
        else
            print("Function is working!");
    }
}
