using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GongWon.Core;
using GongWon.Net;

namespace GongWon.UI
{
    /// <summary>
    /// UI管理器 — 所有界面管理：开场动画、登录、主菜单、商城、图鉴、角色、地图选择、公告、更新弹窗
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("面板引用")]
        public GameObject introPanel;
        public GameObject loginPanel;
        public GameObject mainMenuPanel;
        public GameObject shopPanel;
        public GameObject galleryPanel;
        public GameObject characterPanel;
        public GameObject mapSelectPanel;
        public GameObject announcementPanel;
        public GameObject updatePanel;
        public GameObject loadingPanel;
        public GameObject gameHUD;
        public GameObject pausePanel;
        public GameObject gameOverPanel;

        [Header("开场动画")]
        public Text introBloodText;
        public Image introBloodDrip;
        public float introDuration = 3.5f;

        [Header("登录")]
        public InputField nameInputField;
        public Button loginButton;

        [Header("主菜单")]
        public Button shopButton;       // 左下角商城
        public Button galleryButton;    // 左下角图鉴
        public Button characterButton;  // 右上角角色
        public Button startGameButton;  // 右下角开始游戏

        [Header("公告")]
        public Text announcementText;
        public Button announcementCloseBtn;

        [Header("更新弹窗")]
        public Text updateVersionText;
        public Text updateDescText;
        public Button updateGoButton;
        public Button updateCancelButton;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            // 注册按钮事件
            if (loginButton != null) loginButton.onClick.AddListener(OnLoginClicked);
            if (shopButton != null) shopButton.onClick.AddListener(ShowShop);
            if (galleryButton != null) galleryButton.onClick.AddListener(ShowGallery);
            if (characterButton != null) characterButton.onClick.AddListener(ShowCharacterSelect);
            if (startGameButton != null) startGameButton.onClick.AddListener(ShowMapSelect);
            if (announcementCloseBtn != null) announcementCloseBtn.onClick.AddListener(CloseAnnouncement);
            if (updateGoButton != null) updateGoButton.onClick.AddListener(OnGoUpdate);
            if (updateCancelButton != null) updateCancelButton.onClick.AddListener(CloseUpdate);

            // 注册网络事件
            AnnouncementSystem.Instance.OnAnnouncementReady += ShowAnnouncement;
            UpdateSystem.Instance.OnUpdateAvailable += ShowUpdatePopup;
        }

        #region 开场动画
        /// <summary>
        /// 播放开场血字动画：공원 带血液流淌
        /// </summary>
        public void PlayIntroBloodText()
        {
            if (introPanel != null) introPanel.SetActive(true);
            StartCoroutine(IntroAnimationCoroutine());
        }

        private IEnumerator IntroAnimationCoroutine()
        {
            // 血字渐显
            if (introBloodText != null)
            {
                introBloodText.text = "공원";
                introBloodText.color = new Color(0.8f, 0f, 0f, 0f);
                float t = 0;
                while (t < 1f)
                {
                    t += Time.deltaTime * 0.8f;
                    introBloodText.color = new Color(0.8f, 0f, 0f, t);
                    yield return null;
                }
            }

            // 血液流淌效果
            if (introBloodDrip != null)
            {
                float drip = 0;
                while (drip < 1f)
                {
                    drip += Time.deltaTime * 0.5f;
                    introBloodDrip.fillAmount = drip;
                    yield return null;
                }
            }

            yield return new WaitForSeconds(introDuration - 2f);

            // 淡出
            if (introPanel != null)
            {
                CanvasGroup cg = introPanel.GetComponent<CanvasGroup>();
                if (cg == null) cg = introPanel.AddComponent<CanvasGroup>();
                float fade = 1f;
                while (fade > 0)
                {
                    fade -= Time.deltaTime * 1.5f;
                    cg.alpha = fade;
                    yield return null;
                }
                introPanel.SetActive(false);
            }
        }
        #endregion

        #region 登录
        public void ShowLoginPanel()
        {
            HideAllPanels();
            if (loginPanel != null) loginPanel.SetActive(true);
        }

        private void OnLoginClicked()
        {
            string playerName = nameInputField != null ? nameInputField.text.Trim() : "";
            if (!string.IsNullOrEmpty(playerName))
            {
                GameManager.Instance.Login(playerName);
            }
        }
        #endregion

        #region 主菜单
        public void ShowMainMenu()
        {
            HideAllPanels();
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        }
        #endregion

        #region 商城
        public void ShowShop()
        {
            if (shopPanel != null) shopPanel.SetActive(true);
            // TODO: 填充商城列表
        }

        public void CloseShop()
        {
            if (shopPanel != null) shopPanel.SetActive(false);
        }
        #endregion

        #region 图鉴
        public void ShowGallery()
        {
            if (galleryPanel != null) galleryPanel.SetActive(true);
            // TODO: 填充怪物图鉴（所有怪物照片和名称）
        }

        public void CloseGallery()
        {
            if (galleryPanel != null) galleryPanel.SetActive(false);
        }
        #endregion

        #region 角色选择
        public void ShowCharacterSelect()
        {
            if (characterPanel != null) characterPanel.SetActive(true);
            // TODO: 显示角色头像圆形按钮（燐无155、久流166、安英194）
        }

        public void CloseCharacterSelect()
        {
            if (characterPanel != null) characterPanel.SetActive(false);
        }

        /// <summary>
        /// 角色头像按钮点击（圆形按钮）
        /// </summary>
        public void OnCharacterSelected(int characterId)
        {
            GongWon.Characters.CharacterManager.Instance?.SwitchCharacter(characterId);
            CloseCharacterSelect();
        }
        #endregion

        #region 地图选择
        public void ShowMapSelect()
        {
            HideAllPanels();
            if (mapSelectPanel != null) mapSelectPanel.SetActive(true);
            // TODO: 显示地图列表（鬼灵之谷、江河地）+ 左侧模式选择（多人/PVP/PVPVE/4队）
        }

        public void OnMapSelected(string mapName, int modeIndex)
        {
            var mapManager = GongWon.Maps.MapManager.Instance;
            if (mapManager != null)
            {
                mapManager.SelectMap(mapName, (GongWon.Maps.MapManager.GameMode)modeIndex);
            }
        }
        #endregion

        #region 公告
        public void ShowAnnouncement(string content)
        {
            if (announcementPanel != null)
            {
                announcementPanel.SetActive(true);
                if (announcementText != null) announcementText.text = content;
            }
        }

        public void CloseAnnouncement()
        {
            if (announcementPanel != null) announcementPanel.SetActive(false);
        }
        #endregion

        #region 更新弹窗
        public void ShowUpdatePopup(string downloadUrl, string version, string desc)
        {
            if (updatePanel != null)
            {
                updatePanel.SetActive(true);
                if (updateVersionText != null) updateVersionText.text = $"v{version}";
                if (updateDescText != null) updateDescText.text = desc;
            }
        }

        private void OnGoUpdate()
        {
            UpdateSystem.Instance?.OpenUpdateUrl();
        }

        public void CloseUpdate()
        {
            if (updatePanel != null) updatePanel.SetActive(false);
        }
        #endregion

        #region 游戏HUD
        public void ShowGameHUD()
        {
            HideAllPanels();
            if (gameHUD != null) gameHUD.SetActive(true);
        }

        public void ShowPause()
        {
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        public void ShowGameOver()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }
        #endregion

        #region 加载
        public void ShowLoading()
        {
            HideAllPanels();
            if (loadingPanel != null) loadingPanel.SetActive(true);
        }
        #endregion

        /// <summary>
        /// 隐藏所有面板
        /// </summary>
        private void HideAllPanels()
        {
            if (introPanel != null) introPanel.SetActive(false);
            if (loginPanel != null) loginPanel.SetActive(false);
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (shopPanel != null) shopPanel.SetActive(false);
            if (galleryPanel != null) galleryPanel.SetActive(false);
            if (characterPanel != null) characterPanel.SetActive(false);
            if (mapSelectPanel != null) mapSelectPanel.SetActive(false);
            if (announcementPanel != null) announcementPanel.SetActive(false);
            if (updatePanel != null) updatePanel.SetActive(false);
            if (loadingPanel != null) loadingPanel.SetActive(false);
            if (gameHUD != null) gameHUD.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (AnnouncementSystem.Instance != null)
                AnnouncementSystem.Instance.OnAnnouncementReady -= ShowAnnouncement;
            if (UpdateSystem.Instance != null)
                UpdateSystem.Instance.OnUpdateAvailable -= ShowUpdatePopup;
        }
    }
}
