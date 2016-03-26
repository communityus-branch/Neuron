using UnityEngine;

namespace Static_Interface.API.NetworkFramework
{
    public interface IPositionValidator
    {
        bool ValidatePosition(Transform transform, Vector3 deltaPosition, Vector3 deltaVelocity);
    }
}