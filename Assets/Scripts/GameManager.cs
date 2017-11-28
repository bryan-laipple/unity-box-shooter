using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour {

	// singleton
	private static GameManager instance;

	// keep stats between levels
	private static int score = 0;
	private static float timeRemaining = 0.0f;
	private static int ammo = 0;

	private bool levelOver = false;

	// public variables
	public bool bonusLevel = false;
	public bool finalLevel = false;
	public bool canBeatLevel = false;
	public int beatLevelScore = 0;

	public float startTime = 5.0f;
	public int startScore = 0;

	public bool canRunOutOfAmmo = false;
	public int startAmmo = 0;

	public Color levelColor = Color.white;
	
	public Text mainScoreDisplay;
	public Text mainTimerDisplay;
	public Text mainAmmoDisplay;

	public GameObject gameOverScoreOutline;

	public AudioSource musicAudioSource;

	public GameObject playAgainButtons;
	public string playAgainLevelToLoad;

	public GameObject nextLevelButtons;
	public string nextLevelToLoad;

	// setup the game
	void Start () {

		// get a reference to the GameManager for static methods
		if (instance == null) {
			instance = this;
		}

		// set the current time to the startTime specified + any time from previous level
		timeRemaining += startTime;

		// init score to previous level score and init UI
		score += startScore;
		updateScoreText();

		// init ammo to startAmmo specified + any unused from previous level and init UI
		if (canRunOutOfAmmo) {
			ammo += startAmmo;
			updateAmmoText ();
		} else {
			// infinity
			setAmmoText ("\u221E", 40);
		}

		deactivateEndLevelUI ();

		initLookAndFeel ();
	}

	// this is the main game event loop
	void Update () {
		if (!levelOver) {
			if (isLevelBeat()) {
				BeatLevel ();
			} else if (isGameOver()) {
				EndGame ();
			} else {
				// game playing state, so update the timer
				updateTime (-Time.deltaTime);
			}
		}
	}

	void EndGame() {
		endLevel (bonusLevel || false);
	}

	void BeatLevel() {
		endLevel (true);
	}

	// public function that can be called to determine if the game is being managed
	public static bool IsPresent() {
		return instance ? true : false;
	}

	// public function that can be called to determine if the game is over
	public static bool IsLevelOver() {
		return instance && instance.levelOver;
	}

	public static void FireAmmo() {
		if (IsPresent ()) {
			instance.updateAmmo (-1);
		}
	}

	// public function that can be called to update the score or time
	public static void TargetHit (int scoreAmount, float timeAmount, int ammoAmount) {
		if (IsPresent ()) {
			instance.updateScore (scoreAmount);
			instance.updateTime (timeAmount);
			instance.updateAmmo (ammoAmount);
		}
	}		

	// public function that can be called to restart the game
	public static void RestartGame () {
		// we are just loading a scene (or reloading this scene)
		// which is an easy way to restart the level
		if (IsPresent ()) {
			clearStats ();
			SceneManager.LoadScene (instance.playAgainLevelToLoad);
		}
	}

	// public function that can be called to go to the next level of the game
	public static void NextLevel () {
		// we are just loading the specified next level (scene)
		if (IsPresent ()) {
			if (instance.finalLevel) {
				clearStats ();
			}
			SceneManager.LoadScene (instance.nextLevelToLoad);
		}
	}

	private bool isLevelBeat() {
		// level beat when necessary score reached
		return canBeatLevel && (score >= beatLevelScore);
	}

	private bool isGameOver() {
		// game over if no more time or ammo no more
		return timeRemaining <= 0.0f || (canRunOutOfAmmo && ammo <= 0);
	}

	private static void clearStats() {
		score = 0;
		timeRemaining = 0.0f;
		ammo = 0;
	}

	private void initLookAndFeel() {
		setScoreTextColor (levelColor);
		setGameOverScoreOutlineColor (levelColor);
		initSkyboxColor ();
		initBumberColor ();
	}

	private void initSkyboxColor() {
		Color skyboxColor = new Color ();
		skyboxColor.r = levelColor.r;
		skyboxColor.g = levelColor.g;
		skyboxColor.b = levelColor.b;
		skyboxColor.a = 128.0f;

		// skybox.material is a shared material and we don't want to modify the prefab, so just clone it
		GameObject camera = GameObject.FindGameObjectWithTag ("MainCamera");
		Skybox originalSkybox = camera.GetComponent<Skybox> ();
		Material levelSkyboxMaterial = Material.Instantiate (originalSkybox.material);
		levelSkyboxMaterial.SetColor ("_Tint", skyboxColor);
		Skybox.Destroy (originalSkybox);
		Skybox levelSkybox = camera.AddComponent<Skybox>();
		levelSkybox.material = levelSkyboxMaterial;
	}

	private void initBumberColor() {
		float h;
		float s;
		float v;
		Color.RGBToHSV (levelColor, out h, out s, out v);
		Color emissionColor = Color.HSVToRGB (h, s, 1.0f);
		Color bumperColor = Color.HSVToRGB (h, s, v * 0.5f);

		GameObject[] bumpers = GameObject.FindGameObjectsWithTag ("Bumper");
		foreach (GameObject bumper in bumpers) {
			Material material = bumper.GetComponent<MeshRenderer> ().material;
			material.SetColor ("_Color", bumperColor);
			material.SetColor ("_EmissionColor", emissionColor);
		}
	}

	private void setScoreTextColor(Color color) {
		if (mainScoreDisplay) {
			mainScoreDisplay.color = color;
		}
	}

	private void updateScoreText() {
		if (mainScoreDisplay) {
			mainScoreDisplay.text = score.ToString();
		}
	}

	private void updateTimerText() {
		if (mainTimerDisplay) {
			mainTimerDisplay.text = timeRemaining.ToString("0.00");
		}
	}

	private void updateAmmoText() {
		if (mainAmmoDisplay) {
			mainAmmoDisplay.text = ammo.ToString();
		}
	}

	private void setAmmoText(string text, int size) {
		if (mainAmmoDisplay) {
			mainAmmoDisplay.text = text;
			mainAmmoDisplay.fontSize = size;
		}
	}

	private void setGameOverScoreOutlineColor(Color color) {
		if (gameOverScoreOutline) {
			Image image = gameOverScoreOutline.GetComponent<Image> ();
			if (image) {
				image.color = color;
			}
		}
	}

	private void endLevel(bool levelComplete) {
		// game is over
		levelOver = true;

		// repurpose the timer to display a message to the player
		string text = "GAME OVER";
		if (levelComplete) {
			text = "LEVEL COMPLETE";
			if (finalLevel) {
				text = "GAME COMPLETE";
			}
		}
		mainTimerDisplay.text = text;

		// activate UI based on levelComplete
		activateEndLevelUI (levelComplete);

		// reduce the pitch of the background music, if it is set 
		if (musicAudioSource) {
			musicAudioSource.pitch = 0.5f; // slow down the music
		}
	}

	private void activateEndLevelUI(bool levelComplete) {
		setActiveStateOfEndLevelUI (true, levelComplete);
	}

	private void deactivateEndLevelUI() {
		setActiveStateOfEndLevelUI (false, false);
	}

	private void setActiveStateOfEndLevelUI(bool active, bool levelComplete) {
		// if set, set active state for gameOverScoreOutline
		if (gameOverScoreOutline) {
			gameOverScoreOutline.SetActive (active);
		}

		// if set, set active state for playAgainButtons
		if (playAgainButtons) {
			playAgainButtons.SetActive (active && !levelComplete);
		}

		// if set, set active state for nextLevelButtons
		if (nextLevelButtons) {
			nextLevelButtons.SetActive (active && levelComplete);
		}
	}

	private void updateScore (int scoreAmount) {
		// increase the score by the scoreAmount and update the text UI
		score += scoreAmount;
		updateScoreText ();
	}

	private void updateTime (float timeAmount) {
		// increase the time by the timeAmount
		timeRemaining += timeAmount;

		// don't let it go negative
		if (timeRemaining < 0) {
			timeRemaining = 0.0f;
		}

		// update the text UI
		updateTimerText ();
	}

	private void updateAmmo (int ammoAmount) {
		if (canRunOutOfAmmo) {
			// increase the ammo by the ammoAmount and update the text UI
			ammo += ammoAmount;

			// don't let it go negative
			if (ammo < 0) {
				ammo = 0;
			}

			updateAmmoText ();
		}
	}
}
