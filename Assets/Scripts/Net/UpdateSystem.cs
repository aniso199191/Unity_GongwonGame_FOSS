using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using GongWon.Core;

namespace GongWon.Net
{
    /// <summary>
    /// 更新系统 — 每次启动读取QQ收藏链接
    /// 第一行：开/关（控制是否强制更新）
    /// 第二行：下载链接
    /// 第三行：最新版本号
    /// </summary>
    public class UpdateSystem : MonoBehaviour
    {
        public static UpdateSystem Instance { get; private set; }

        public bool IsUpdateEnabled { get; private set; }
        public string UpdateDownloadUrl { get; private set; }
        public string LatestVersion { get; private set; }
        public bool NeedUpdate { get; private set; }
        public bool IsChecked { get; private set; }

        public event Action<string, string, string> OnUpdateAvailable; // (下载链接, 最新版本, 更新描述)
        public event Action OnUpdateNotNeeded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 检查更新 — 请求QQ收藏链接并按行解析
        /// </summary>
        public IEnumerator CheckUpdate()
        {
            IsChecked = false;
            NeedUpdate = false;
            Debug.Log("[UpdateSystem] 开始请求更新链接...");

            using (UnityWebRequest request = UnityWebRequest.Get(GameConfig.UPDATE_URL))
            {
                request.timeout = 10;
                request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Linux; Android 10) AppleWebKit/537.36");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[UpdateSystem] 请求失败: {request.error}，跳过更新检查");
                    IsChecked = true;
                    OnUpdateNotNeeded?.Invoke();
                    yield break;
                }

                string htmlContent = request.downloadHandler.text;
                ParseUpdateContent(htmlContent);
            }

            IsChecked = true;

            if (IsUpdateEnabled && NeedUpdate)
            {
                string updateDesc = $"发现新版本快去更新 v{LatestVersion}";
                OnUpdateAvailable?.Invoke(UpdateDownloadUrl, LatestVersion, updateDesc);
                Debug.Log($"[UpdateSystem] 需要更新! 最新版本:{LatestVersion}, 下载链接:{UpdateDownloadUrl}");
            }
            else
            {
                OnUpdateNotNeeded?.Invoke();
                Debug.Log("[UpdateSystem] 无需更新或更新未开启");
            }
        }

        /// <summary>
        /// 解析更新页面内容
        /// 第一行：开/关
        /// 第二行：下载链接
        /// 第三行：版本号
        /// </summary>
        private void ParseUpdateContent(string html)
        {
            string plainText = ExtractPlainText(html);
            string[] lines = plainText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 1)
            {
                IsUpdateEnabled = false;
                return;
            }

            // 第一行：开/关
            string firstLine = lines[0].Trim().ToLower();
            IsUpdateEnabled = firstLine.Contains("开") || firstLine == "on" || firstLine == "true";

            if (!IsUpdateEnabled) return;

            // 第二行：下载链接
            if (lines.Length >= 2)
            {
                UpdateDownloadUrl = lines[1].Trim();
            }

            // 第三行：版本号
            if (lines.Length >= 3)
            {
                LatestVersion = lines[2].Trim();
                // 比较版本号，判断是否需要更新
                NeedUpdate = CompareVersion(LatestVersion, GameConfig.APP_VERSION) > 0;
            }

            Debug.Log($"[UpdateSystem] 解析结果 - 开关:{IsUpdateEnabled}, 链接:{UpdateDownloadUrl}, 版本:{LatestVersion}, 需更新:{NeedUpdate}");
        }

        /// <summary>
        /// 版本号比较：返回1表示v1>v2，-1表示v1<v2，0表示相等
        /// </summary>
        private int CompareVersion(string v1, string v2)
        {
            try
            {
                string[] parts1 = v1.TrimStart('v', 'V').Split('.');
                string[] parts2 = v2.TrimStart('v', 'V').Split('.');
                int length = Mathf.Max(parts1.Length, parts2.Length);
                for (int i = 0; i < length; i++)
                {
                    int num1 = i < parts1.Length ? int.Parse(parts1[i]) : 0;
                    int num2 = i < parts2.Length ? int.Parse(parts2[i]) : 0;
                    if (num1 > num2) return 1;
                    if (num1 < num2) return -1;
                }
                return 0;
            }
            catch
            {
                return string.Compare(v1, v2, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// 跳转到浏览器下载更新
        /// </summary>
        public void OpenUpdateUrl()
        {
            if (!string.IsNullOrEmpty(UpdateDownloadUrl))
            {
                Application.OpenURL(UpdateDownloadUrl);
                Debug.Log($"[UpdateSystem] 跳转到浏览器: {UpdateDownloadUrl}");
            }
        }

        /// <summary>
        /// 从HTML中提取纯文本
        /// </summary>
        private string ExtractPlainText(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            html = System.Text.RegularExpressions.Regex.Replace(html, @"<script[^>]*>[\s\S]*?</script>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"<style[^>]*>[\s\S]*?</style>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = html.Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n");
            html = html.Replace("</p>", "\n").Replace("</div>", "\n");
            html = System.Text.RegularExpressions.Regex.Replace(html, @"<[^>]+>", "");
            html = html.Replace("&nbsp;", " ").Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"");
            html = System.Text.RegularExpressions.Regex.Replace(html, @"[ \t]+", " ");
            html = System.Text.RegularExpressions.Regex.Replace(html, @"\n\s*\n", "\n");
            return html.Trim();
        }
    }
}
