//#define EDITMODE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Cinemachine;

public enum SCENE
{
    TITLE,
    STAGE1,
    STAGE2,
    END
}

/*
 * ��ü�� �ν��Ͻ��� 1���� �����ϱ� ���� Singleton pattern���� �ۼ� 
 */
public sealed class GameMgr : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField] PlayerInput input;
    [SerializeField] Player player;

    [Header("Manager")]
    [SerializeField] GUIMgr uiMgr;
    [SerializeField] PoolMgr poolMgr;
    [SerializeField] EnemyMgr enemyMgr;
    [SerializeField] SoundMgr soundMgr;

    [Header("Pooling Object Prefab")]
    [SerializeField] BulletHole bulletHole;
    [SerializeField] HitMark    hitMark;

    [Header("Camera")]
    [SerializeField] CinemachineBrain brainCam;
    [SerializeField] CinemachineVirtualCamera fpsCam;
    [SerializeField] CinemachineVirtualCamera endingCam;
    [SerializeField] CinemachineVirtualCamera cheatCam;

    [Header("Position")]
    [SerializeField] Transform cheatPos;

    public GUIMgr UIMgr => uiMgr;
    public PoolMgr PoolMgr => poolMgr;
    public SoundMgr SoundMgr => soundMgr;

    public Transform BulletTr;      //Bullet Prefab Parent Transform
    public Transform HitMarkTr;     //HitMark Prefab Parent Transform

    static GameMgr instance = null;

    [SerializeField] int SceneNum = 0;  //Scene Number

    [SerializeField] SystemSound systemSound;

        Coroutine playCoroutine   = null;     //Play(Title�� �ƴ� Stage���� TimeScale != 0�� �� �ݺ��Ǵ� �ڷ�ƾ
    Coroutine endingCoroutine = null;   //Ending�� ����Ǵ� ���� ����ϴ� �ڷ�ƾ

    public static GameMgr Instance 
    {
        get
        {
            if(!instance)
            {
                instance = FindObjectOfType<GameMgr>();
                if (!instance) instance = new GameObject("GameManager").AddComponent<GameMgr>();

                instance.Initialize();
                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }
    
    private void Awake() { if (this != Instance) Destroy(gameObject); }

    private void Initialize()
    {
        //UIMgr Initialize�� Player�� �ʿ��ϹǷ� Player -> UIMgr ������ Init
        player.Initialize();
        UIMgr.Initialize(player);
        PoolMgr.Initialize();
        enemyMgr.Initialize(player);
        
        BulletTr = PoolMgr.CreatePool(bulletHole, 5);
        HitMarkTr = PoolMgr.CreatePool(hitMark, 5);

        SceneManager.sceneLoaded += SceneLoaded;
        enemyMgr.OnClearEvent += NextStage;

        InputInitialize();

        systemSound.Initialize();

        //���� ���� Title�� ����
        TitleSet();
    }

    //Game�� �ʿ��� ��� Input ���
    private void InputInitialize()
    {
        input.SwitchCurrentActionMap("ItemMenu");
        //���� �ݱ�
        input.actions["Close"].started += CloseItemMenu;

        input.SwitchCurrentActionMap("Player");
        input.actions["Move"].performed += player.Move;
        input.actions["Move"].canceled += player.Move;

        //���콺 �þ� ȸ��
        input.actions["Look"].performed += player.Rotate;
        input.actions["Look"].canceled += player.Rotate;

        //�����̽��� ����
        input.actions["Jump"].started += player.Jump;

        //���콺 ��Ŭ�� �߻�
        input.actions["Fire"].started += player.Fire;
        input.actions["Fire"].canceled += player.Fire;

        //R ������
        input.actions["Reload"].started += player.Reload;

        //F1 ���� ���� ź ����
        input.actions["RemoveHole"].started += ResetHole;

        //Tab ItemMenu ����
        input.actions["GoItemMenu"].started += OpenItemMenu;

        //Esc PauseMenu ����
        input.actions["Pause"].started += OpenPauseMenu;

        input.actions["Capture"].started += Capture;

        input.actions["Cheat1"].started += LeaveOneEnemy;
        
        input.actions["CheatView"].started += CheatViewOn;
        input.actions["CheatViewOff"].started += CheatViewOff;

        input.actions["CheatPos"].started += CheatPos;


        input.currentActionMap.Disable();
    }

    //Ÿ��Ʋ ���� ����
    //���� ���۽�, (Any Stage -> Ending -> Quit), (Any Stage -> Pause Menu -> Quit)
    public void TitleSet()
    {
        systemSound.PlayBGM();
        CommonStartSet();
        if (SceneManager.GetActiveScene().buildIndex != SceneNum) SceneManager.LoadScene(SceneNum);
        input.SwitchCurrentActionMap("Player");
        input.currentActionMap.Disable();
        UIMgr.TitleActive(true);
        UIMgr.PlayUIActive(false);
        MenuVisibleSet(true);
    }

    //�������� ���� ����
    //�������� 1���� �����ϴ� Method
    //(Title -> Stage1), (Stage? -> Ending -> Retry -> Stage1)
    public void StartStageSet()
    {
        UIMgr.TitleActive(false);
        systemSound.StopBGM();
        CommonStartSet();
        NextStage();
    }

    //Title Set, Start Set���� ���������� ���Ǵ� method
    //ù ȣ��� �ƴ϶� Retry, Quit�� ����Ͽ� Scene Number Set, EndingCheck, CameraView Setting...
    public void CommonStartSet()
    {
        SceneNum = (int)SCENE.TITLE;
        if(endingCoroutine != null)
        {
            StopCoroutine(endingCoroutine);
            endingCoroutine = null;
        }
        CameraPlayerView();
        UIMgr.ClearMessage();
        systemSound.StopEffect();
    }

    //Title -> StartStage -> NextStage
    //���� ���������� �̵�
    //++SceneNum >= (int)SCENE.END : ������ �������� Ŭ���� -> GameFinish ȣ��
    public void NextStage()
    {
        if(++SceneNum < (int)SCENE.END)
        {
            StartCoroutine(LoadSceneAsync());
        }
        else
        {
            GameFinish(true); //true == Game Clear
        }
    }

    private IEnumerator LoadSceneAsync()
    {
        if (SceneNum > (int)SCENE.STAGE1)
        {
            UIMgr.ShowMessage(MESSAGETYPE.LARGE, string.Format("Stage {0} Clear!", SceneNum - 1));
            yield return new WaitForSecondsRealtime(3.0f);
        }
        else
        {
            StartGame();
        }

        player.SetActive(false);
        input.currentActionMap.Disable();
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneNum);
        operation.allowSceneActivation = false;
        float timer = 0.0f;
        UIMgr.LoadingUIActive(true);
        yield return new WaitForSecondsRealtime(1.0f);

        while (!operation.isDone)
        {
            timer += Time.deltaTime;

            if (operation.progress < 0.9f)
            {
                if (UIMgr.UpdateLoadingProgress(operation.progress, timer) > operation.progress)
                {
                    timer = 0f;
                }
            }
            else
            {
                if (UIMgr.UpdateLoadingProgress(1f, timer) >= 1.0f)
                {
                    operation.allowSceneActivation = true;            
                    break;
                }
            }

            yield return null;
        }
        UIMgr.LoadingUIActive(false);
        input.currentActionMap.Enable();
    }

    //SceneManager.LoadScene += SceneLoaded
    //Stage���� Scene�� �Ѿ �� �ʿ��� Method
    //Title�� ���� �Ѿ ���� �ʿ� X
    public void SceneLoaded(Scene scene, LoadSceneMode load)
    {
        //Title Scene���� �ʿ�X
        if (scene.buildIndex == (int)SCENE.TITLE) return;

        enemyMgr.SetEnemy(player, SceneNum - 1);
        if (GameObject.FindGameObjectWithTag("Start").TryGetComponent<Transform>(out Transform startTr))
        {
            player.transform.position = startTr.position;
            player.transform.rotation = startTr.rotation;
            player.SetActive(true);
            Time.timeScale = 1;
        }
        UIMgr.ShowMessage(MESSAGETYPE.ALERT, "Stage�� ��� ���� óġ�ϼ���");
        systemSound.StageStart();
    }

    //Ending(Clear or Over)���� ����Ǵ� method
    public void GameFinish(bool isClear = false)
    {
        if (endingCoroutine != null) return;
        endingCoroutine = StartCoroutine(EndingRoutine(isClear));
    }

    [SerializeField] int endingTime = 5;

    private IEnumerator EndingRoutine(bool isClear)
    {
        //1. Play Coroutine�� ���߰� Camera�� EndingView�� ������ �� n�ʰ� ���.
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }
        input.currentActionMap.Disable();
        CameraEndingView();
        yield return new WaitForSecondsRealtime(3f);

        //2. Ending UI�� �����Ű�� endingTime�� ��ŭ ���.
        UIMgr.StartEndingUI(isClear, endingTime);
        systemSound.GameFinish(isClear);
        int sec = endingTime;
        MenuVisibleSet(true);
        //2-A. 1�ʿ� �ѹ��� ���� �ʸ� Update
        while (sec >= 0)
        {
            yield return new WaitForSecondsRealtime(1f);
            UIMgr.UpdateEnding(--sec);
        }
        
        TitleSet();
        UIMgr.EndingUIActive(false);
    }

    //Title Scene���� Quit ���� ����.
    public void Quit()
    {
#if EDITMODE
        //������ ����
        UnityEditor.EditorApplication.isPlaying = false;
#else
        //���� ���� ����
        Application.Quit();
#endif
    }

    
    //Title���� ù Stage �� �� Play Setting...
    void StartGame()
    {
        player.StartSet();
        MenuVisibleSet(false);
        UIMgr.PlayUIActive(true);
        input.currentActionMap.Enable();
        playCoroutine = StartCoroutine(PlayRoutine());
    }

    //������ �÷��̵Ǵ� ���� �����Ǿ� �� Coroutine
    private IEnumerator PlayRoutine()
    {
        while (true)
        {
            player.inventory.UpdateMoney(1);
            yield return new WaitForSeconds(0.1f);
        }
    }

    //Pause Menu
    /*
     * Continue -> ���� ��� ����(Close Pause Menu)
     * Option -> �ɼ� UI ����
     * Quit -> Title Scene���� ����
     * 
     * Close -> ���� ��� ����(Close Pause Menu)
     */

    //PauseMenu On
    void OpenPauseMenu(InputAction.CallbackContext context)
    {
        UIMgr.PauseMenuActive(true);
        input.currentActionMap.Disable();
        MenuVisibleSet(true);
    }

    //PauseMenu Off
    public void ClosePauseMenu()
    {
        UIMgr.PauseMenuActive(false);
        input.currentActionMap.Enable();
        MenuVisibleSet(false);
    }

    //Item Menu On
    void OpenItemMenu(InputAction.CallbackContext context)
    {
        UIMgr.ItemMenuActive(true);
        input.SwitchCurrentActionMap("ItemMenu");
        MenuVisibleSet(true);
    }

    //Item Menu OFF
    void CloseItemMenu(InputAction.CallbackContext context)
    {
        UIMgr.ItemMenuActive(false);
        input.SwitchCurrentActionMap("Player");
        MenuVisibleSet(false);
    }

    public void OptionActive(bool isOn)
    {
        UIMgr.OptionMenuActive(isOn);

        if(!isOn)
        {
            if (SceneManager.GetActiveScene().buildIndex == (int)SCENE.TITLE && !isOn) TitleSet();
            else ClosePauseMenu();
        }
        else
        {
            UIMgr.TitleActive(false);
            UIMgr.PauseMenuActive(false);
        }
    }

    //�÷��̾� FPS View�� ī�޶� ��ȯ
    void CameraPlayerView()
    {
        brainCam.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
        endingCam.Priority = fpsCam.Priority - 1;
    }

    //�÷��̾� �տ��� �÷��̾ �ٶ󺸴� Ending View�� ��ȯ
    void CameraEndingView()
    {
        brainCam.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        endingCam.Priority = fpsCam.Priority + 1;
    }



    //Menu�� ���� ���� �� ���������� �����ϴ� Set
    void MenuVisibleSet(bool visible)
    {
        if(visible)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Time.timeScale = 0;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1;
        }
        Cursor.visible = visible;
    }

    //���� ���� Bullet Hole ���� (Return Pool)
    void ResetHole(InputAction.CallbackContext context)
    {
        PoolMgr.ReturnToPool(bulletHole.name);
    }

    //UI ��ư Ŭ���� Sound ���
    public void ButtonClick()
    {
        systemSound.Click();
    }

    /*--------------���� �׽�Ʈ�� �����ϱ� ���� �ۼ��� ġƮŰ--------------*/
    public void Capture(InputAction.CallbackContext context)
    {
        ScreenCapture.CaptureScreenshot(System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm") +"Capture.png");
    }

    public void LeaveOneEnemy(InputAction.CallbackContext context)
    {
        enemyMgr.LeaveOneEnemy();
    }

    void CheatViewOn(InputAction.CallbackContext context)
    {
        brainCam.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        cheatCam.Priority = 50;
        UIMgr.PlayUIActive(false);
    }

    void CheatViewOff(InputAction.CallbackContext context)
    {
        brainCam.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
        cheatCam.Priority = 0;
        UIMgr.PlayUIActive(true);
    }

    void CheatPos(InputAction.CallbackContext context)
    {
        player.transform.position = cheatPos.position;
    }
}
