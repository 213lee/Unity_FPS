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
 * 객체의 인스턴스를 1개로 고정하기 위해 Singleton pattern으로 작성 
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

        Coroutine playCoroutine   = null;     //Play(Title이 아닌 Stage에서 TimeScale != 0일 때 반복되는 코루틴
    Coroutine endingCoroutine = null;   //Ending이 진행되는 동안 사용하는 코루틴

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
        //UIMgr Initialize에 Player가 필요하므로 Player -> UIMgr 순으로 Init
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

        //게임 시작 Title로 설정
        TitleSet();
    }

    //Game에 필요한 모든 Input 등록
    private void InputInitialize()
    {
        input.SwitchCurrentActionMap("ItemMenu");
        //상점 닫기
        input.actions["Close"].started += CloseItemMenu;

        input.SwitchCurrentActionMap("Player");
        input.actions["Move"].performed += player.Move;
        input.actions["Move"].canceled += player.Move;

        //마우스 시야 회전
        input.actions["Look"].performed += player.Rotate;
        input.actions["Look"].canceled += player.Rotate;

        //스페이스바 점프
        input.actions["Jump"].started += player.Jump;

        //마우스 좌클릭 발사
        input.actions["Fire"].started += player.Fire;
        input.actions["Fire"].canceled += player.Fire;

        //R 재장전
        input.actions["Reload"].started += player.Reload;

        //F1 벽에 박힌 탄 제거
        input.actions["RemoveHole"].started += ResetHole;

        //Tab ItemMenu 열기
        input.actions["GoItemMenu"].started += OpenItemMenu;

        //Esc PauseMenu 열기
        input.actions["Pause"].started += OpenPauseMenu;

        input.actions["Capture"].started += Capture;

        input.actions["Cheat1"].started += LeaveOneEnemy;
        
        input.actions["CheatView"].started += CheatViewOn;
        input.actions["CheatViewOff"].started += CheatViewOff;

        input.actions["CheatPos"].started += CheatPos;


        input.currentActionMap.Disable();
    }

    //타이틀 들어가는 설정
    //게임 시작시, (Any Stage -> Ending -> Quit), (Any Stage -> Pause Menu -> Quit)
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

    //스테이지 시작 설정
    //스테이지 1으로 시작하는 Method
    //(Title -> Stage1), (Stage? -> Ending -> Retry -> Stage1)
    public void StartStageSet()
    {
        UIMgr.TitleActive(false);
        systemSound.StopBGM();
        CommonStartSet();
        NextStage();
    }

    //Title Set, Start Set에서 공통적으로 사용되는 method
    //첫 호출뿐 아니라 Retry, Quit을 고려하여 Scene Number Set, EndingCheck, CameraView Setting...
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
    //다음 스테이지로 이동
    //++SceneNum >= (int)SCENE.END : 마지막 스테이지 클리어 -> GameFinish 호출
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
    //Stage에서 Scene이 넘어갈 때 필요한 Method
    //Title로 씬이 넘어갈 경우는 필요 X
    public void SceneLoaded(Scene scene, LoadSceneMode load)
    {
        //Title Scene에서 필요X
        if (scene.buildIndex == (int)SCENE.TITLE) return;

        enemyMgr.SetEnemy(player, SceneNum - 1);
        if (GameObject.FindGameObjectWithTag("Start").TryGetComponent<Transform>(out Transform startTr))
        {
            player.transform.position = startTr.position;
            player.transform.rotation = startTr.rotation;
            player.SetActive(true);
            Time.timeScale = 1;
        }
        UIMgr.ShowMessage(MESSAGETYPE.ALERT, "Stage의 모든 적을 처치하세요");
        systemSound.StageStart();
    }

    //Ending(Clear or Over)직후 수행되는 method
    public void GameFinish(bool isClear = false)
    {
        if (endingCoroutine != null) return;
        endingCoroutine = StartCoroutine(EndingRoutine(isClear));
    }

    [SerializeField] int endingTime = 5;

    private IEnumerator EndingRoutine(bool isClear)
    {
        //1. Play Coroutine을 멈추고 Camera를 EndingView로 설정한 후 n초간 대기.
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }
        input.currentActionMap.Disable();
        CameraEndingView();
        yield return new WaitForSecondsRealtime(3f);

        //2. Ending UI를 실행시키고 endingTime초 만큼 대기.
        UIMgr.StartEndingUI(isClear, endingTime);
        systemSound.GameFinish(isClear);
        int sec = endingTime;
        MenuVisibleSet(true);
        //2-A. 1초에 한번씩 남은 초를 Update
        while (sec >= 0)
        {
            yield return new WaitForSecondsRealtime(1f);
            UIMgr.UpdateEnding(--sec);
        }
        
        TitleSet();
        UIMgr.EndingUIActive(false);
    }

    //Title Scene에서 Quit 게임 종료.
    public void Quit()
    {
#if EDITMODE
        //에디터 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
        //빌드 게임 종료
        Application.Quit();
#endif
    }

    
    //Title에서 첫 Stage 들어갈 때 Play Setting...
    void StartGame()
    {
        player.StartSet();
        MenuVisibleSet(false);
        UIMgr.PlayUIActive(true);
        input.currentActionMap.Enable();
        playCoroutine = StartCoroutine(PlayRoutine());
    }

    //게임이 플레이되는 동안 유지되야 할 Coroutine
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
     * Continue -> 게임 계속 진행(Close Pause Menu)
     * Option -> 옵션 UI 실행
     * Quit -> Title Scene으로 복귀
     * 
     * Close -> 게임 계속 진행(Close Pause Menu)
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

    //플레이어 FPS View로 카메라 전환
    void CameraPlayerView()
    {
        brainCam.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
        endingCam.Priority = fpsCam.Priority - 1;
    }

    //플레이어 앞에서 플레이어를 바라보는 Ending View로 전환
    void CameraEndingView()
    {
        brainCam.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        endingCam.Priority = fpsCam.Priority + 1;
    }



    //Menu를 열고 닫을 때 공통적으로 수행하는 Set
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

    //벽에 박힌 Bullet Hole 제거 (Return Pool)
    void ResetHole(InputAction.CallbackContext context)
    {
        PoolMgr.ReturnToPool(bulletHole.name);
    }

    //UI 버튼 클릭시 Sound 재생
    public void ButtonClick()
    {
        systemSound.Click();
    }

    /*--------------게임 테스트를 진행하기 위해 작성한 치트키--------------*/
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
