namespace Chess.Game {
	using System.Collections.Generic;
	using System.Collections;
	using UnityEngine;

	[System.Serializable]
	public class Clock : MonoBehaviour {

		public TMPro.TMP_Text timerUI;
		public bool isTurnToMove;
		public int startSeconds;
		public float secondsRemaining;
		public int lowTimeThreshold = 10;
		[Range (0, 1)]
		public float inactiveAlpha = 0.75f;
		[Range (0, 10)]
		public float decimalFontSizeMultiplier = 0.75f;
		public Color lowTimeCol;
		public int bonusSeconds;
		public bool addBonus;
		public int addTime;
		public List<float> startTimes;

		void Start () {
			secondsRemaining = startSeconds;
			addTime = 0;
			startTimes = new List<float>();
		}

		void Update () {
			if (isTurnToMove) {
				addBonus = true;
				if (addTime == 0)
					startTimes.Add(secondsRemaining);
				addTime++;
				secondsRemaining -= Time.deltaTime;
				secondsRemaining = Mathf.Max (0, secondsRemaining);
			}
			int numMinutes = (int) (secondsRemaining / 60);
			int numSeconds = (int) (secondsRemaining - numMinutes * 60);

			timerUI.text = $"{numMinutes:00}:{numSeconds:00}";
			if (secondsRemaining <= lowTimeThreshold) {
				int dec = (int) ((secondsRemaining - numSeconds) * 10);
				float size = timerUI.fontSize * decimalFontSizeMultiplier;
				timerUI.text += $"<size={size}>.{dec}</size>";
			}

			var col = Color.white;
			if ((int) secondsRemaining <= lowTimeThreshold) {
				col = lowTimeCol;
			}
			if (!isTurnToMove) {
				if (addBonus) {
					addBonuSeconds();
					addBonus = false;
				}
				addTime = 0;
				col = new Color (col.r, col.g, col.b, inactiveAlpha);
			}
			timerUI.color = col;
		}

		public void Reset() {
			secondsRemaining = startSeconds;
		}

		public void addBonuSeconds() {
			secondsRemaining += bonusSeconds;
		}

		public void unmakeMove() {
			Debug.Log(startTimes[startTimes.Count - 1]);
			secondsRemaining = startTimes[startTimes.Count - 1];
		}

	}
}