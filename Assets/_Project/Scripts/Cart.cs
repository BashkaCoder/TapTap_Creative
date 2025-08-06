using UnityEngine;

public class Cart : MonoBehaviour, IHitable
{
    [SerializeField] private Transform[] _wheels;
    
    public void Hit()
    {
        foreach (var wheel in _wheels)
            Destroy(wheel.GetComponent<FixedJoint>());
    }
}