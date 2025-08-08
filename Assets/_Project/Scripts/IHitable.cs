using UnityEngine;

public interface IHitable
{
    public bool IsUsed { get; set; }
    void Hit(GameObject hitter);
}