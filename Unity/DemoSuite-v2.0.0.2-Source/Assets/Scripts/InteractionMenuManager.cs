using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionMenuManager : MonoBehaviour {

	public List<GameObject> Menus;

	public void DeactivateAllMenus()
	{
		if (Menus == null) return;
		foreach (var menu in Menus)
		{
			menu.SetActive(false);
		}
	}
	public void ActivateMenu(int index)
	{
		if (Menus == null) return;
		DeactivateAllMenus();
		if (index < Menus.Count)
		{
			Menus[index].SetActive(true);
		}
	}
	public void ActivateMenu(GameObject menu)
	{
		if (Menus == null) return;
		DeactivateAllMenus();
		if (Menus.Contains(menu))
			menu.SetActive(true);
	}
}
