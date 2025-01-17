using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class SlotBehaviour : MonoBehaviour
{
    [SerializeField]
    private RectTransform mainContainer_RT;

    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;
    [SerializeField]
    private List<SlotImage> Tempimages;

    [Header("Slots Objects")]
    [SerializeField]
    private GameObject[] Slot_Objects;
    [Header("Slots Elements")]
    [SerializeField]
    private LayoutElement[] Slot_Elements;

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;

    [Header("Line Button Objects")]
    [SerializeField]
    private List<GameObject> StaticLine_Objects;

    [Header("Buttons")]
    [SerializeField]
    private Button SlotStart_Button;
    [SerializeField]
    private Button MaxBet_Button;
    [SerializeField]
    private Button LinePlus_Button;
    [SerializeField]
    private Button LineMinus_Button;
    [SerializeField]
    private Button AutoSpin_Button;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField] private Button Double_button;
    [SerializeField] private Button _buttonBetone;


    [Header("Animated Sprites")]
    [SerializeField]
    private Sprite[] Symbol1;
    [SerializeField]
    private Sprite[] Symbol2;
    [SerializeField]
    private Sprite[] Symbol3;
    [SerializeField]
    private Sprite[] Symbol4;
    [SerializeField]
    private Sprite[] Symbol5;
    [SerializeField]
    private Sprite[] Symbol6;
    [SerializeField]
    private Sprite[] Symbol7;
    [SerializeField]
    private Sprite[] Symbol8;

    [Header("Miscellaneous UI")]
    [SerializeField]
    private int[] Lines_num;
    [SerializeField]
    private TMP_Text Balance_text;
    [SerializeField]
    private TMP_Text TotalBet_text;
    [SerializeField]
    private Image Lines_Image;
    [SerializeField]
    private TMP_Text TotalWin_text;
    [SerializeField]
    private TMP_Text Lines_text;

    [SerializeField]
    private Sprite[] Lines_Sprites;
    private int LineCounter = 0;


    int tweenHeight = 0;

    [SerializeField]
    private GameObject Image_Prefab;
    [SerializeField]
    private GameObject Gamble;

    [SerializeField]
    private PayoutCalculation PayCalculator;

    private List<Tweener> alltweens = new List<Tweener>();

    [SerializeField]
    private List<ImageAnimation> TempList;

    [SerializeField]
    private int IconSizeFactor = 100;
    [SerializeField] private int SpacingFactor;

    private int numberOfSlots = 5;

    [SerializeField]
    int verticalVisibility = 3;

    [SerializeField]
    private SocketIOManager SocketManager;
    [SerializeField]
    private BonusController _bonusManager;
    [SerializeField]
    private UIManager uiManager;
    [SerializeField] private GambleController gambleController;

    private Coroutine AutoSpinRoutine = null;
    private Coroutine tweenroutine = null;
    private bool IsAutoSpin = false;
    private bool IsSpinning = false;
    internal bool CheckPopups = false;
    private int BetCounter = 0;
    private Coroutine FreeSpinRoutine = null;
    private bool IsFreeSpin = false;

    [SerializeField] private AudioController audioController;

    private void Start()
    {

        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

        if (MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
        if (MaxBet_Button) MaxBet_Button.onClick.AddListener(MaxBet);

        if (LinePlus_Button) LinePlus_Button.onClick.RemoveAllListeners();
        if (LinePlus_Button) LinePlus_Button.onClick.AddListener(delegate { ToggleLine(true); });

        if (LineMinus_Button) LineMinus_Button.onClick.RemoveAllListeners();
        if (LineMinus_Button) LineMinus_Button.onClick.AddListener(delegate { ToggleLine(false); });

        if (Lines_Image) Lines_Image.sprite = Lines_Sprites[8];
        LineCounter = 0;

        if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(AutoSpin);

        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(StopAutoSpin);

        if (_buttonBetone) _buttonBetone.onClick.RemoveAllListeners();
        if (_buttonBetone) _buttonBetone.onClick.AddListener(OnBetOne);
    }

    private void AutoSpin()
    {
        if (!IsAutoSpin)
        {

            IsAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());

        }
    }

    internal void SetInitialUI()
    {
        BetCounter = SocketManager.initialData.Bets.Count - 1;
        LineCounter = SocketManager.initialData.LinesCount.Count - 1;
        if (Lines_text) Lines_text.text = SocketManager.initialData.LinesCount[LineCounter].ToString();
        PayCalculator.SetButtonActive(SocketManager.initialData.LinesCount[LineCounter]);
        if (TotalBet_text) TotalBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.haveWon.ToString();
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString();
        uiManager.InitialiseUIData(SocketManager.initUIData.AbtLogo.link, SocketManager.initUIData.AbtLogo.logoSprite, SocketManager.initUIData.ToULink, SocketManager.initUIData.PopLink, SocketManager.initUIData.paylines);
    }

    internal void FreeSpin(int spins)
    {
        if (!IsFreeSpin)
        {

            IsFreeSpin = true;
            ToggleButtonGrp(false);

            if (FreeSpinRoutine != null)
            {
                StopCoroutine(FreeSpinRoutine);
                FreeSpinRoutine = null;
            }
            FreeSpinRoutine = StartCoroutine(FreeSpinCoroutine(spins));

        }
    }

    private void StopAutoSpin()
    {
        if (IsAutoSpin)
        {
            IsAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        }

    }

    private IEnumerator AutoSpinCoroutine()
    {

        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;


        }
    }

    private IEnumerator FreeSpinCoroutine(int spinchances)
    {
        int i = 0;
        while (i < spinchances)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            i++;
        }
        ToggleButtonGrp(true);
        IsFreeSpin = false;
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        ToggleButtonGrp(true);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            tweenroutine = null;
            AutoSpinRoutine = null;
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }

    private void MaxBet()
    {
        if (audioController) audioController.PlayButtonAudio();
        BetCounter = SocketManager.initialData.Bets.Count - 1;
        if (TotalBet_text) TotalBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
    }

    void OnBetOne()
    {
        if (audioController) audioController.PlayButtonAudio();

        if (BetCounter < SocketManager.initialData.Bets.Count - 1)
        {
            BetCounter++;
        }
        else
        {
            BetCounter = 0;
        }

        if (TotalBet_text) TotalBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
    }

    private void ToggleLine(bool inc)
    {
        if (audioController) audioController.PlayButtonAudio();

        if (inc)
        {
            LineCounter++;
            if (LineCounter == Lines_num.Length)
            {
                LineCounter = 0;
            }
        }
        else
        {
            LineCounter--;
            if (LineCounter == -1)
            {
                LineCounter = Lines_num.Length - 1;
            }
        }
        if (Lines_Image) Lines_Image.sprite = Lines_Sprites[LineCounter];
        PayCalculator.DontDestroy.Clear();
        PayCalculator.ResetLines();
        PayCalculator.GeneratePayoutLinesBackend(-1);
        PayCalculator.SetButtonActive(SocketManager.initialData.LinesCount[LineCounter]);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && SlotStart_Button.interactable)
        {
            StartSlots();
        }
    }

    internal void PopulateInitalSlots(int number, List<int> myvalues)
    {
        PopulateSlot(myvalues, number);
    }

    internal void LayoutReset(int number)
    {
        if (Slot_Elements[number]) Slot_Elements[number].ignoreLayout = true;
        if (SlotStart_Button) SlotStart_Button.interactable = true;
    }

    
    private void PopulateSlot(List<int> values, int number)
    {
        if (Slot_Objects[number]) Slot_Objects[number].SetActive(true);
        for (int i = 0; i < values.Count; i++)
        {
            GameObject myImg = Instantiate(Image_Prefab, Slot_Transform[number]);
            images[number].slotImages.Add(myImg.GetComponent<Image>());
            images[number].slotImages[i].sprite = myImages[values[i]];
        }
        for (int k = 0; k < 2; k++)
        {
            GameObject mylastImg = Instantiate(Image_Prefab, Slot_Transform[number]);
            images[number].slotImages.Add(mylastImg.GetComponent<Image>());
            images[number].slotImages[images[number].slotImages.Count - 1].sprite = myImages[values[k]];
        }
        if (mainContainer_RT) LayoutRebuilder.ForceRebuildLayoutImmediate(mainContainer_RT);
        tweenHeight = (values.Count * IconSizeFactor) - 280;
        GenerateMatrix(number);
    }

    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();
        switch (val)
        {
            case 0:
                for (int i = 0; i < Symbol1.Length; i++)
                {
                    animScript.textureArray.Add(Symbol1[i]);
                }
                break;
            case 1:
                for (int i = 0; i < Symbol2.Length; i++)
                {
                    animScript.textureArray.Add(Symbol2[i]);
                }
                break;
            case 2:
                for (int i = 0; i < Symbol3.Length; i++)
                {
                    animScript.textureArray.Add(Symbol3[i]);
                }
                break;
            case 3:
                for (int i = 0; i < Symbol4.Length; i++)
                {
                    animScript.textureArray.Add(Symbol4[i]);
                }
                break;
            case 4:
                for (int i = 0; i < Symbol5.Length; i++)
                {
                    animScript.textureArray.Add(Symbol5[i]);
                }
                break;
            case 5:
                for (int i = 0; i < Symbol6.Length; i++)
                {
                    animScript.textureArray.Add(Symbol6[i]);
                }
                break;
            case 6:
                break;
            case 7:
                for (int i = 0; i < Symbol8.Length; i++)
                {
                    animScript.textureArray.Add(Symbol8[i]);
                }
                break;
        }
    }

    private void StartSlots(bool autoSpin = false)
    {
        if (audioController) audioController.PlayWLAudio("spin");

        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }

        if (SlotStart_Button) SlotStart_Button.interactable = false;
        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }
        PayCalculator.ResetLines();
        tweenroutine = StartCoroutine(TweenRoutine());
    }

   
    private IEnumerator TweenRoutine()
    {
        IsSpinning = true;
        ToggleButtonGrp(false);
        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(0.1f);
        }

        double bet = 0;
        double balance = 0;
        try
        {
            bet = double.Parse(TotalBet_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            balance = double.Parse(Balance_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        balance = balance - bet;

        if (Balance_text) Balance_text.text = balance.ToString("f2");

        SocketManager.AccumulateResult(bet);

        yield return new WaitUntil(() => SocketManager.isResultdone);


        for (int j = 0; j < SocketManager.resultData.ResultReel.Count; j++)
        {
            List<int> resultnum = SocketManager.resultData.FinalResultReel[j]?.Split(',')?.Select(Int32.Parse)?.ToList();
            for (int i = 0; i < 5; i++)
            {
                if (images[i].slotImages[images[i].slotImages.Count - 5 + j]) images[i].slotImages[images[i].slotImages.Count - 5 + j].sprite = myImages[resultnum[i]];
                PopulateAnimationSprites(images[i].slotImages[images[i].slotImages.Count - 5 + j].gameObject.GetComponent<ImageAnimation>(), resultnum[i]);
            }
        }

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < numberOfSlots; i++)
        {
            yield return StopTweening(5, Slot_Transform[i], i);
        }
        yield return new WaitForSeconds(0.3f);

        CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit, SocketManager.resultData.jackpot);
        KillAllTweens();


        CheckPopups = true;

        if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.haveWon.ToString("f2");

        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");

        
        if (SocketManager.resultData.WinAmout >= bet * 5 && SocketManager.resultData.WinAmout < bet * 10)
        {
            uiManager.PopulateWin(1, SocketManager.resultData.WinAmout);
        }
        
        else
        {
            yield return new WaitForSeconds(1f);
            CheckBonusGame();
        }

        print("checkpopups, " + CheckPopups);
        yield return new WaitUntil(() => !CheckPopups);
        if (!IsAutoSpin)
        {
            if (SocketManager.playerdata.currentWining > 1) gambleController.toggleDoubleButton(true);
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            IsSpinning = false;
        }
        
    }

    internal void CallCloseSocket()
    {
        SocketManager.CloseWebSocket();
    }

    internal void CheckBonusGame()
    {
        if (SocketManager.resultData.isBonus)
        {
            _bonusManager.ChestOpen(SocketManager.resultData.BonusResult);
        }
        else
        {
            CheckPopups = false;
        }
    }

    void ToggleButtonGrp(bool toggle)
    {

        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (LinePlus_Button) LinePlus_Button.interactable = toggle;
        if (LineMinus_Button) LineMinus_Button.interactable = toggle;
        if (_buttonBetone) _buttonBetone.interactable = toggle;
        if (MaxBet_Button) MaxBet_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;

    }

    
    internal void updateBalance()
    {
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString();
    }

    private void StartGameAnimation(GameObject animObjects)
    {
        
        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();

        temp.StartAnimation();
        TempList.Add(temp);
    }

    private void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
        }
       
    }

    
    private void CheckPayoutLineBackend(List<int> LineId, List<string> points_AnimString, double jackpot = 0)
    {
        List<int> points_anim = null;
        if (LineId.Count > 0)
        {
            if (audioController) audioController.PlayWLAudio("win");


            for (int i = 0; i < LineId.Count; i++)
            {
                PayCalculator.DontDestroy.Add(LineId[i] - 1);
                PayCalculator.GeneratePayoutLinesBackend(LineId[i] - 1);
            }

            if (jackpot > 0)
            {
                for (int i = 0; i < Tempimages.Count; i++)
                {
                    for (int k = 0; k < Tempimages[i].slotImages.Count; k++)
                    {
                        StartGameAnimation(Tempimages[i].slotImages[k].gameObject);
                    }
                }
            }
            else
            {
                for (int i = 0; i < points_AnimString.Count; i++)
                {
                    points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                    for (int k = 0; k < points_anim.Count; k++)
                    {
                        if (points_anim[k] >= 10)
                        {
                            StartGameAnimation(Tempimages[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject);
                        }
                        else
                        {
                            StartGameAnimation(Tempimages[0].slotImages[points_anim[k]].gameObject);
                        }
                    }
                }
            }
        }
        else
        {

            if (audioController) audioController.PlayWLAudio("lose");
        }


    }

    
    private void GenerateMatrix(int value)
    {
        for (int j = 0; j < 3; j++)
        {
            Tempimages[value].slotImages.Add(images[value].slotImages[images[value].slotImages.Count - 5 + j]);
        }
    }

    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.2f).SetLoops(-1, LoopType.Restart).SetDelay(0);
        tweener.Play();
        alltweens.Add(tweener);
    }

    private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index)
    {
        alltweens[index].Pause();
        int tweenpos = (reqpos * (IconSizeFactor + SpacingFactor)) - (IconSizeFactor + (2 * SpacingFactor));
        alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100 + (SpacingFactor > 0 ? SpacingFactor / 4 : 0), 0.5f).SetEase(Ease.OutElastic);
        yield return new WaitForSeconds(0.2f);
    }


    private void KillAllTweens()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion

}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}

