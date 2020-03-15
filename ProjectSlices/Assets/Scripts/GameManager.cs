using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GetScript Get;

    public bool GameOver;
    public float SpeedMovement;
    public int BladeSpeedMovement;
    private int score;

    public bool GameModeBlade;
    public bool GameModeArrow;

    private void Start()
    {
       // Time.timeScale = 0;
        Initialize();
    }

    private void Update()
    {
        if (Get.CameraPosition.GetControl() == false)
            Get.CameraPosition.CameraPosChange();

        ScoreAccordedSpeed();
        GameOverFunction();
        // Yerini değiştir!
        GameModeChange();
    }

    private void Initialize()
    {
        Get.UIManager.Start.onClick.AddListener(() => GamePlay());
        Get.UIManager.Restart.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));

        GameModeBlade = true;
        GameModeArrow = false;

        GameModeChange();
    }

    private void GameModeChange()
    {
        if (GameModeBlade)
        {
            Get.BladeControl.Blade.SetActive(true);
            Get.BladeControl.enabled = true;
        }
        else if(GameModeArrow)
        {
            Get.BladeControl.Blade.SetActive(false);
            Get.BladeControl.enabled = false;
        }
    }

    private void GameOverFunction()
    {
        if (GameOver)
        {
            Get.BladeControl.mBladeTR.rotation = Quaternion.Euler(0, 0, 90);
            Get.UIManager.GameOver.SetActive(true);
            Time.timeScale = 0;
        }
    }

    private void GamePlay()
    {
        Get.UIManager.ClickPanel.SetActive(false);
        Time.timeScale = 1;
    }


    private void ScoreAccordedSpeed()
    {
        score = Get.UIManager.Score;

        if (score < 150)
        {
            SetSpeed(1, 300);
            Get.FruitSpawn.SpawnTime = 1;
        }
        else if (score < 500)
        {
            SetSpeed(1.2f, 500);

            Get.FruitSpawn.SpawnTime = 0.8f;


        }
        else if (score < 1000)
        {
            SetSpeed(1.4f, 750);
            Get.FruitSpawn.SpawnTime = 0.6f;


        }
        else if (score < 2000)
        {
            SetSpeed(1.6f, 1000);
            Get.FruitSpawn.SpawnTime = 0.5f;


        }
        else if (score < 5000)
        {
            SetSpeed(1.8f, 1500);
            Get.FruitSpawn.SpawnTime = 0.3f;

        }
        else
        {
            SetSpeed(2, 5000);
            Get.FruitSpawn.SpawnTime = 0.2f;
        }

    }

    private void SetSpeed(float elementSpeed, int bladeSpeed)
    {
        if (SpeedMovement != elementSpeed)
            SpeedMovement = elementSpeed;
        if (bladeSpeed != BladeSpeedMovement)
            BladeSpeedMovement = bladeSpeed;
    }
}
