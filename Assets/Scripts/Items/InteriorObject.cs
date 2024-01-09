using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    /// <summary>
    /// uses the semi-implicit euler method. faster, but not always stable.
    /// see http://allenchou.net/2015/04/game-math-more-on-numeric-springing/
    /// </summary>
    public class InteriorObject : MonoBehaviour
    {
    // PRAGMA MARK - Public Interface

    public float Amount
    {
        get { return this._currentAmount; }
    }
    public float ValueY
    {
        get { return this._currentValueY; }
    }

    public float ValueZ
    {
        get { return this._currentValueZ; }
    }

    public float ValueX
    {
        get { return this._currentValueX; }
    }

    public float rotationValueY
    {
        get { return this._currentRotValueY; }
    }

    public float TargetValueY
    {
        get { return this._targetValueY; }
    }

    public float TargetValueX
    {
        get { return this._targetValueX; }
    }

       public float TargetValueZ
       {
            get { return this._targetValueZ; }
       }

        public void SetTargetValueY(float targetValue)
        {
            this._targetValueY = targetValue;
            this._velocityY = 0.0f;
        }

    public void SetTargetValueX(float targetValue)
    {
        this._targetValueX = targetValue;
        this._velocityX = 0.0f;
    }

    public void SetTargetValueZ(float targetValue)
    {
        this._targetValueZ = targetValue;
        this._velocityZ = 0.0f;
    }

    public void SetTargetRotationValueY(float targetValue)
    {
        this._targetRotValueY += targetValue;
        this._rotVelocity = 0.0f;

    }

    public void SetTargetRotationValueY(bool up)
    {
        if (up) {

            this._targetRotValueY += 90;
            this._rotVelocity = 0.0f;              
        
            if (objectDirection >= 4)
                objectDirection = 1;
            else
                objectDirection += 1;

        }
        else
        {
            this._targetRotValueY -= 90;
            this._rotVelocity = 0.0f;

            if (objectDirection <= 1)
                objectDirection = 4;
            else
                objectDirection -= 1;

        }


    }

    public void ResetValues(float val)
        {

            Debug.Log("VALUES RESET");
            this._currentValueX = val;
            this._currentValueY = val;
            this._currentValueZ = val;
            this._currentRotValueY = val;
        

            this._targetValueX = val;
            this._targetValueY = val;
            this._targetValueZ = val;


            this._velocityY = 0.0f;
            this._velocityX = 0.0f;
        
            this._velocityZ = 0.0f;
    }


        // PRAGMA MARK - Internal
        [Header("Properties")]
        // lower values are less damped and higher values are more damped resulting in less springiness.
        // should be between 0.01f, 1f to avoid unstable systems.
        [SerializeField, Range(0.01f, 1.0f)] private float _dampingRatio;
        // An angular frequency of 2pi (radians per second) means the oscillation completes one
        // full period over one second, i.e. 1Hz. should be less than 35 or so to remain stable
        [SerializeField] private float _angularFrequency;

        [Header("Read-Only")]
        [SerializeField] public float _currentValueX = 0.0f;
        [SerializeField] public float _currentValueY = 0.0f;        
        [SerializeField] public float _currentValueZ = 0.0f;
        [SerializeField] public float _currentRotValueY = 0.0f;
    

        [SerializeField] public float _targetValueY = 0.0f, _targetValueX = 0.0f , _targetValueZ = 0.0f;
        [SerializeField] public float _targetRotValueY = 0.0f;
    
        [SerializeField] private int _currentAmount = 1;


        [SerializeField] private float _velocityX = 0.0f;
        [SerializeField] private float _velocityY = 0.0f;        
        [SerializeField] private float _velocityZ = 0.0f;
        [SerializeField] private float _rotVelocity = 0.0f;
        public float speedY = 1;
        

        public enum occupationSlot { upper , lower , both , wall, floor, extra}

        public occupationSlot occupiedSlot;

        
        //FOR WALL
        public int objectDirection = 1;// ég

        public List<GridStats> currentlyOccupiedTiles;
        public List<Vector2> gridOccupation;

        public bool hasExtraSlot = false;
        public Transform extraSlotHeight;
        public List<InteriorObject> extraObjects;
        public Dictionary<int, InteriorObject> _extraObjects;

    private void Start()
    {
        extraObjects = new List<InteriorObject>();
        currentlyOccupiedTiles = new List<GridStats>();
        _extraObjects = new Dictionary<int, InteriorObject>();
        
    }

    void Update()
    {

        //Maybe  should update position only when new grid is highlighted.
            
        this._velocityY += (-2.0f * Time.deltaTime * this._dampingRatio * this._angularFrequency * this._velocityY) + (Time.deltaTime * this._angularFrequency * this._angularFrequency * (this._targetValueY - this._currentValueY));           
        this._currentValueY += Time.deltaTime * this._velocityY * speedY;

        this._velocityX += (-2.0f * Time.deltaTime  * this._angularFrequency * this._velocityX) + (Time.deltaTime * this._angularFrequency * this._angularFrequency * (this._targetValueX - this._currentValueX));
        this._currentValueX += Time.deltaTime * this._velocityX;

        this._velocityZ += (-2.0f * Time.deltaTime  * this._angularFrequency * this._velocityZ) + (Time.deltaTime * this._angularFrequency * this._angularFrequency * (this._targetValueZ - this._currentValueZ));
        this._currentValueZ += Time.deltaTime * this._velocityZ;

        this._rotVelocity += (-2.0f * Time.deltaTime * this._dampingRatio * this._angularFrequency * this._rotVelocity) + (Time.deltaTime * this._angularFrequency * this._angularFrequency * (this._targetRotValueY - this._currentRotValueY));
        this._currentRotValueY += Time.deltaTime * this._rotVelocity;

        this.transform.position = new Vector3(_currentValueX, _currentValueY, _currentValueZ);
        this.transform.rotation = Quaternion.Euler(0, _currentRotValueY, 0);

    }

    public void SetModeTransparent(bool transparent)
    {

        if(transparent)
            MapEditor.instance.GetComponent<DisplayController>().ChangeTransparency(true, GetComponentInChildren<MeshRenderer>().material);
        else
            MapEditor.instance.GetComponent<DisplayController>().ChangeTransparency(false, GetComponentInChildren<MeshRenderer>().material);

    }
}





public static class FloatSpringExtensions
    {
        public static void SetTargetValueIfDifferent(this InteriorObject spring, float targetValue)
        {
            if (spring.TargetValueY == targetValue)
            {
                return;
            }

            spring.SetTargetValueY(targetValue);
        }
    }



    
    
