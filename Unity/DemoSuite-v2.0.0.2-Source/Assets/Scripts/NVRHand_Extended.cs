using UnityEngine.Events;
using NewtonVR;

public class NVRHand_Extended : NVRHand {

	public NVRButtons MenuButton = NVRButtons.ApplicationMenu;
	public bool MenuButtonDown { get { return Inputs[MenuButton].PressDown; } }
	public bool MenuButtonUp { get { return Inputs[MenuButton].PressUp; } }
	public bool MenuButtonPressed { get { return Inputs[MenuButton].IsPressed; } }
	public float MenuButtonAxis { get { return Inputs[MenuButton].SingleAxis; } }

	public UnityEvent OnMenuDown;
	public UnityEvent OnMenuUp;
	public UnityEvent OnMenuPressed;

	protected override void Update()
	{
		base.Update();
		UpdateMenuButton();
	}

	protected virtual void UpdateMenuButton()
	{
		if (MenuButtonDown)
		{
			OnMenuDown.Invoke();
		}
		if (MenuButtonUp)
		{
			OnMenuUp.Invoke();
		}
		if (MenuButtonPressed)
		{
			OnMenuPressed.Invoke();
		}
	}
}
