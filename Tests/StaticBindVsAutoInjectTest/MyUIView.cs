using LiteFramework.Core.Utility;
using UnityEngine;
using UnityEngine.UI;


namespace LiteFramework.Tests
{
    public class MyUIView : MonoBehaviour
    {
        [Autowrited] public Button BtnPlay;
        [Autowrited] public Button BtnExit;
        [Autowrited] public Image ImgIcon;
        [Autowrited] public Text TxtName;
        [Autowrited] public Text TxtScore;
        [Autowrited] public Slider SldProgress;
        [Autowrited] public Toggle TogOption;
        [Autowrited] public GameObject PanelInfo;
        [Autowrited] public RectTransform RectContent;
        [Autowrited] public CanvasGroup CanvasGroup;

        public void StaticBind(Transform root)
        {
            BtnPlay = root.Find("BtnPlay").GetComponent<Button>();
            BtnExit = root.Find("BtnExit").GetComponent<Button>();
            ImgIcon = root.Find("ImgIcon").GetComponent<Image>();
            TxtName = root.Find("TxtName").GetComponent<Text>();
            TxtScore = root.Find("TxtScore").GetComponent<Text>();
            SldProgress = root.Find("SldProgress").GetComponent<Slider>();
            TogOption = root.Find("TogOption").GetComponent<Toggle>();
            PanelInfo = root.Find("PanelInfo").gameObject;
            RectContent = root.Find("RectContent").GetComponent<RectTransform>();
            CanvasGroup = root.Find("CanvasGroup").GetComponent<CanvasGroup>();
        }

        public void Clear()
        {
            BtnPlay = null;
            BtnExit = null;
            ImgIcon = null;
            TxtName = null;
            TxtScore = null;
            SldProgress = null;
            TogOption = null;
            PanelInfo = null;
            RectContent = null;
            CanvasGroup = null;
        }
    }

}