using Static_Interface.API.UnityExtensions;

namespace Static_Interface.API.Utils
{
    public class TimedDestroy : MonoBehaviour
    {
        public long DestroyTimestamp;
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            var currentTime = TimeUtil.GetCurrentTime();
            if (DestroyTimestamp != 0 && currentTime >= DestroyTimestamp)
            {
                Destroy(gameObject);
            }
        }

        public void DestroyAfter(uint millis)
        {
            DestroyTimestamp = TimeUtil.GetCurrentTime() + millis;
        }
    }
}