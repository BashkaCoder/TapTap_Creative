using UnityEngine;

public class FragmentedHitable : MonoBehaviour, IHitable
{
    [SerializeField] private Transform _wholeModel;
    [SerializeField] private Transform _slicedModel;
    
    public void Hit()
    {
        _wholeModel.gameObject.SetActive(false);
        _slicedModel.gameObject.SetActive(true);
    }   
}