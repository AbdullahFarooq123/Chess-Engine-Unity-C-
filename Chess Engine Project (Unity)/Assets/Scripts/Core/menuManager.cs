using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using Chess.Game;
using Chess;
public class menuManager : MonoBehaviour {
    public Slider whiteDifficultySlider;
    public Slider blackDifficultySlider;
    public bool whiteIsBot { get; set; }
    public bool blackIsBot { get; set; }
    
    public bool isUnlimitedTime;
    public int startMinutes;
    public int bonusSeconds;
    public GameObject helpGameObject;
    GameObject previousImage;
    public int startIndex;
    [SerializeField]
    public GameObject currentShownScreen {
        get; set;
    }

    public void checkForOpenScreen(GameObject parentObject) {
        Button[] buttons = parentObject.GetComponentsInChildren<Button>(true);
        buttons[0].gameObject.SetActive(true);
        buttons[1].gameObject.SetActive(false);
        if (helpGameObject != null) {
            this.helpGameObject.GetComponentsInChildren<CanvasGroup>(true)[startIndex].gameObject.SetActive(false);
            this.helpGameObject.GetComponentInParent<Animator>().ResetTrigger("Show");
            this.helpGameObject.GetComponentInParent<Animator>().SetTrigger("Hide");
            helpGameObject = null;
        }
        if (currentShownScreen != null) {
            currentShownScreen.GetComponent<Animator>().ResetTrigger("Show");
            currentShownScreen.GetComponent<Animator>().SetTrigger("Hide");
            currentShownScreen = null;
        }
    }

    public void setCurrentScreenToNull() {
        currentShownScreen = null;
    }

    public void startGame(VideoPlayer source) {
        StartCoroutine( waitFrame(3,source));
    }

    private IEnumerator waitFrame(int seconds , VideoPlayer source) {
        StartCoroutine(fadeVolume(source));
        startInfo.whiteIsBot = this.whiteIsBot;
        startInfo.blackIsBot = this.blackIsBot;
        startInfo.isUnlimitedTime = this.isUnlimitedTime;
        startInfo.whiteDifficulty = (int)whiteDifficultySlider.value;
        startInfo.blackDifficulty = (int)blackDifficultySlider.value;
        startInfo.startSeconds = startMinutes;
        startInfo.bonusSeconds = bonusSeconds;
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene(1);
    }

    private IEnumerator fadeVolume(VideoPlayer source) {
        float currentTime = 0;
        float start = source.GetDirectAudioVolume(0);
        while (currentTime < 1) {
            currentTime += Time.deltaTime;
            source.SetDirectAudioVolume(0, Mathf.Lerp(start, 0, currentTime / 1));
            yield return null;
        }
        yield break;
    }

    public void setDifficultyText(GameObject gameObject) {
        (gameObject.GetComponentInChildren<TMPro.TMP_Text>()).text = "DIFFICULTY : " + (getDifficultyString((int)(gameObject.GetComponentInChildren<Slider>()).value));
    }

    public void setCustomTime(GameObject gameObject) {
        string prefix = (gameObject.name.Contains("Initial") ? "INITIAL " : "BONUS ") + "TIME ";
        string postfix = (gameObject.name.Contains("Initial") ? " MIN" : " SEC");
        (gameObject.GetComponentInChildren<TMPro.TMP_Text>()).text = prefix + ((int)(gameObject.GetComponentInChildren<Slider>()).value).ToString() + postfix;
        if (gameObject.name.Contains("Initial"))
            startMinutes = (int)(gameObject.GetComponentInChildren<Slider>()).value;
        else
            bonusSeconds = (int)(gameObject.GetComponentInChildren<Slider>()).value;
    }

    private void translateDifficulty(int value, AISettings settings) {
        settings.useThreading = true;
        settings.useBook = true;
        settings.useIterativeDeepening = true;
        settings.useFixedDepthSearch = true;
        settings.useMoveOrdering = true;
        settings.useTranspositionTable = false;
        switch (value) {
            case 1:
                settings.depth = 1;
                settings.useMoveOrdering = false;
                break;
            case 2:
                settings.depth = 2;
                settings.useMoveOrdering = false;
                break;
            case 3:
                settings.depth = 3;
                settings.useMoveOrdering = false;
                break;
            case 4:
                settings.depth = 4;
                break;
            case 5:
                settings.depth = 5;
                settings.useTranspositionTable = true;
                settings.useFixedDepthSearch = false;
                break;
        }

    }

    private string getDifficultyString(int value) {
        switch (value) {
            case 1:
                return "BASIC";
            case 2:
                return "EASY";
            case 3:
                return "MEDIUM";
            case 4:
                return "HARD";
            case 5:
                return "EXPERT";
        }
        return null;
    }

    public void changeTimeButtonText(TMPro.TMP_Text buttonText) {
        buttonText.text = "TIME : " + getTimeText();
    }

    private string getTimeText() {
        if (isUnlimitedTime = (bonusSeconds == 0 && startMinutes == 0))
            return "UNLIMITED";
        else if (bonusSeconds == 0)
            return startMinutes + "MIN";
        else
            return startMinutes + " | " + bonusSeconds;
    }

    public void setStartTime(int time) {
        this.startMinutes = time;
    }

    public void setBonusTime(int time) {
        this.bonusSeconds = time;
    }

    public void Quit() {
        Application.Quit();
    }
    public void setGameObjectToHelp(GameObject gameObject) {
        this.helpGameObject = gameObject;
        this.helpGameObject.GetComponentsInChildren<CanvasGroup>(true)[startIndex = 0].gameObject.SetActive(true);
        this.helpGameObject.GetComponentInParent<Animator>().ResetTrigger("Hide");
        this.helpGameObject.GetComponentInParent<Animator>().SetTrigger("Show");
    }

    public void nextClicked(GameObject gameObject) {
        startIndex++;
        Button[] button = gameObject.GetComponentsInChildren<Button>(true);
        button[0].interactable = (startIndex < helpGameObject.GetComponentsInChildren<CanvasGroup>(true).Length - 1);
        button[0].GetComponent<Animator>().ResetTrigger(startIndex < helpGameObject.GetComponentsInChildren<CanvasGroup>(true).Length - 1 ? "Hide" : "Show");
        button[0].GetComponent<Animator>().SetTrigger(startIndex < helpGameObject.GetComponentsInChildren<CanvasGroup>(true).Length - 1 ? "Show" : "Hide");
        button[1].interactable = (startIndex > 0);
        button[1].GetComponent<Animator>().ResetTrigger(startIndex > 0 ? "Hide" : "Show");
        button[1].GetComponent<Animator>().SetTrigger(startIndex > 0 ? "Show" : "Hide");
        StartCoroutine(animateHelpMenu(this.helpGameObject.GetComponentsInChildren<CanvasGroup>(true)[startIndex - 1].gameObject, this.helpGameObject.GetComponentsInChildren<CanvasGroup>(true)[startIndex].gameObject, button[1], button[0]));
    }

    public void previousClicked(GameObject gameObject) {
        startIndex--;
        Button[] button = gameObject.GetComponentsInChildren<Button>(true);
        button[0].interactable = (startIndex < helpGameObject.GetComponentsInChildren<CanvasGroup>(true).Length - 1);
        button[0].GetComponent<Animator>().ResetTrigger(startIndex < helpGameObject.GetComponentsInChildren<CanvasGroup>(true).Length - 1 ? "Hide" : "Show");
        button[0].GetComponent<Animator>().SetTrigger(startIndex < helpGameObject.GetComponentsInChildren<CanvasGroup>(true).Length - 1 ? "Show" : "Hide");
        button[1].interactable = (startIndex > 0);
        button[1].GetComponent<Animator>().ResetTrigger(startIndex > 0 ? "Hide" : "Show");
        button[1].GetComponent<Animator>().SetTrigger(startIndex > 0 ? "Show" : "Hide");
        StartCoroutine(animateHelpMenu(this.helpGameObject.GetComponentsInChildren<CanvasGroup>(true)[startIndex + 1].gameObject, this.helpGameObject.GetComponentsInChildren<CanvasGroup>(true)[startIndex].gameObject, button[1], button[0]));
    }

    private IEnumerator animateHelpMenu(GameObject leavingObject , GameObject enteringObject , Button previousButton , Button nextButton) {
        previousButton.interactable = (false);
        nextButton.interactable = (false);
        this.helpGameObject.GetComponentInParent<Animator>().ResetTrigger("Show");
        this.helpGameObject.GetComponentInParent<Animator>().SetTrigger("Hide");
        yield return new WaitUntil(() => this.helpGameObject.GetComponentInParent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Image Hidden"));
        leavingObject.SetActive(false);
        enteringObject.SetActive(true);
        this.helpGameObject.GetComponentInParent<Animator>().ResetTrigger("Hide");
        this.helpGameObject.GetComponentInParent<Animator>().SetTrigger("Show");
        yield return new WaitUntil(() => this.helpGameObject.GetComponentInParent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Image Shown"));
        previousButton.interactable = (true);
        nextButton.interactable = (true);
    }
    public void setTileImage(GameObject gameObject) {
        if (previousImage != null)
            previousImage.SetActive(false);
        (previousImage = gameObject).SetActive(true);
    }

}
