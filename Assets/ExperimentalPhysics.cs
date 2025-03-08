using UnityEngine;

namespace V.Math
{
    public struct BounceRay
    {
        public Vector3 origin;      //Origin of the ray
        public Vector3 direction;   //Normalized direction of the ray
        public float distance;      //The distance the ray will run
        public uint bounces;         //The number of bounces
        public bool resetDistance;  //Whether or not the ray's distance should be reset after each bounce

        public override string ToString()
        {
            return $"BounceRay: \n";
        }
        public string ToString(string name)
        {
            return $"BounceRay {name}";
        }

    }
}
public class ExperimentalPhysics : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
