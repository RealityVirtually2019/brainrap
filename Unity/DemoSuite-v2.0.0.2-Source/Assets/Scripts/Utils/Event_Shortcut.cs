using UnityEngine;
using UnityEngine.Events;

/*
 Let you press a Key to activate a Flicker Object and execute its actions
*/

public class Event_Shortcut : MonoBehaviour
{

    public KeyCode shortcutKey = KeyCode.None;
    public UnityEvent eventKey;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(shortcutKey))
        {
            if (eventKey != null)
            {
                eventKey.Invoke();
            }
        }
    }
}
