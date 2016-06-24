using Static_Interface.API.PlayerFramework;
using UnityEngine;

namespace Static_Interface.API.NetworkFramework
{
    public interface IPositionValidator
    {
        bool ValidatePosition(Identity sender, Transform transform, Vector3 deltaPosition, Vector3 deltaVelocity);
    }
}