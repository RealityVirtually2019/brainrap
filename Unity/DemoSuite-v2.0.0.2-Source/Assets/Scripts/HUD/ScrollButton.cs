using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollButton : MonoBehaviour {

	public enum Direction {LEFT, RIGHT};
	public Direction direction = Direction.LEFT;
	public Scrollbar scrollbar;

	public void MakeSelection(){
		if (direction == Direction.LEFT){
			if (scrollbar.value - 0.5f < 0){
				scrollbar.value = 0;
			}
			else{
				scrollbar.value -= 0.5f;
			}
		}
		else{
			if (scrollbar.value + 0.5f > 1){
				scrollbar.value = 1;
			}
			else{
				scrollbar.value += 0.5f;
			}
		}
	}
}
