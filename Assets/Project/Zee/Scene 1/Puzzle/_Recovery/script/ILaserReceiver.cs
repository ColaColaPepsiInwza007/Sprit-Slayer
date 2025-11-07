using UnityEngine;

public interface ILaserReceiver
{
    void OnLaserHit(Vector3 hitPoint, Vector3 inDirection, int depth);
}

public class LaserTarget : MonoBehaviour, ILaserReceiver
{
    public void OnLaserHit(Vector3 hitPoint, Vector3 inDirection, int depth)
    {
        // ??????????? ???? ????????? ??????? ???
        // Debug.Log("Target hit!");
    }
}
