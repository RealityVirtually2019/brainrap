using UnityEngine;

namespace Neurable.Interactions.Samples
{
    public class UtilitySelection : MonoBehaviour
    {
        public GameObject NotificationToSpawn;

        protected GameObject instanceNotification;

        public void Select()
        {
            if (ElicitorToolManager.instance != null) ElicitorToolManager.instance.SpawnSelectionText();
            if (NotificationToSpawn != null) instanceNotification = Instantiate(NotificationToSpawn, transform);
            if (instanceNotification != null) instanceNotification.transform.LookAt(Camera.main.transform);
            if (instanceNotification != null) Destroy(instanceNotification, 1.5f);
        }
    }
}
