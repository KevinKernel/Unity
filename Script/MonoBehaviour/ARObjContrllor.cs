/*
 *
 * 单指触摸物体旋转
 * 双指控制物体的缩放
 * 
 * 使用FingerGestures插件
 *
 * 2014-7-24
 * 
 * V1.0
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ARObjContrllor : MonoBehaviour
{

    public GameObject obj;
    public Transform posTarget;

    //Scale
    public bool allowPinchZoom = true;
    public float scaleSensitivity = 5.0f;
    public float startScale = 15.0f;
    public float minScale = 10.0f;
    public float maxScale = 30.0f;
    public float smoothScaleSpeed = 5.0f;
    private float scale = 10.0f;
    private float idealScale = 0;


    //Rotate
    public bool OnlyRotateWhenDragStartsOnObject = false;
    public float smoothRotateSpeed = 10.0f;
    private float yaw = 0;
    private float idealYaw = 0;
    public float rotateSensitivity = 45.0f;

    float nextDragTime = 0;



    public float Yaw
    {
        get { return yaw; }
    }

    public float IdealYaw
    {
        get { return idealYaw; }
        set { idealYaw = value; }
    }


    public float IdealScale
    {
        get { return idealScale; }
        set { idealScale = Mathf.Clamp(value, minScale, maxScale); }
    }

    void Start()
    {
        InstallGestureRecognizers();
        scale = IdealScale = startScale;

        Vector3 angles = transform.eulerAngles;
        yaw = IdealYaw = angles.y;
    }

    void LateUpdate()
    {

        Apply();
        
    }

    //FingerGestures Function
    void OnDrag( DragGesture gesture )
    {
      
        // don't rotate unless the drag started on our target object
        if( OnlyRotateWhenDragStartsOnObject )
        {
            
            if( gesture.Phase == ContinuousGesturePhase.Started )
            {
                if( !gesture.Recognizer.Raycaster )
                {
                    Debug.LogWarning( "The drag recognizer on " + gesture.Recognizer.name + " has no ScreenRaycaster component set. This will prevent OnlyRotateWhenDragStartsOnObject flag from working." );
                    OnlyRotateWhenDragStartsOnObject = false;
                    return;
                }

                if( posTarget && !posTarget.collider )
                {
                    Debug.LogWarning( "The target object has no collider set. OnlyRotateWhenDragStartsOnObject won't work." );
                    OnlyRotateWhenDragStartsOnObject = false;
                    return;
                }
            }

            if( !posTarget || gesture.StartSelection != posTarget.gameObject )
                return;
        }
     

        // wait for drag cooldown timer to wear off
        //  used to avoid dragging right after a pinch or pan, when lifting off one finger but the other one is still on screen
        if( Time.time < nextDragTime )
            return;
        

        if( posTarget )
        {
            IdealYaw += gesture.DeltaMove.x.Centimeters() * rotateSensitivity;
        }
        
    }
    void OnPinch(PinchGesture gesture)
    { 
        if( allowPinchZoom )
        {
            IdealScale += gesture.Delta.Centimeters() * scaleSensitivity;
            nextDragTime = Time.time + 0.25f;
        }
    }

    void InstallGestureRecognizers()
    {
        List<GestureRecognizer> recogniers = new List<GestureRecognizer>( GetComponents<GestureRecognizer>() );
        DragRecognizer drag = recogniers.Find( r => r.EventMessageName == "OnDrag" ) as DragRecognizer;
        PinchRecognizer pinch = recogniers.Find( r => r.EventMessageName == "OnPinch" ) as PinchRecognizer;

        // check if we need to automatically add a screenraycaster
        if( OnlyRotateWhenDragStartsOnObject )
        {
            ScreenRaycaster raycaster = gameObject.GetComponent<ScreenRaycaster>();

            if( !raycaster )
                raycaster = gameObject.AddComponent<ScreenRaycaster>();
        }

        if( !drag )
        {
            drag = gameObject.AddComponent<DragRecognizer>();
            drag.RequiredFingerCount = 1;
            drag.IsExclusive = true;
            drag.MaxSimultaneousGestures = 1;
            drag.SendMessageToSelection = GestureRecognizer.SelectionType.None;
        }

        if( !pinch )
            pinch = gameObject.AddComponent<PinchRecognizer>();

    }
    void Apply()
    {

        scale = Mathf.Lerp(scale, idealScale, Time.deltaTime * smoothScaleSpeed);
        yaw = Mathf.Lerp( yaw, IdealYaw, Time.deltaTime * smoothRotateSpeed );

        obj.transform.localScale = Vector3.one * scale;
        obj.transform.rotation = Quaternion.Euler(0, -yaw, 0);
        
    }

}