using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using GongWon.Core;

namespace GongWon.Net
{
    /// <summary>
    /// 公告系统 — 每次启动读取QQ收藏链接
    /// 第一行：开/关（控制是否显示公告）
    /// 第二行：公告内容
    /// </summary>
    public class AnnouncementSystem : MonoBehaviour
    {
        public static AnnouncementSystem Instance { get; private set; }

        public bool IsAnnouncementEnabled { get; private set; }
        public string AnnouncementContent { get; private set; }
        public bool IsChecked { get; private set; }

        public event Action<string> OnAnnouncementReady;
        public event Action OnAnnouncementDisabled;

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
        /// 检查公告 — 请求QQ收藏链接并按行解析
        /// </summary>
        public IEnumerator CheckAnnouncement()
        {
            IsChecked = false;
            Debug.Log("[AnnouncementSystem] 开始请求公告链接...");

            using (UnityWebRequest request = UnityWebRequest.Get(GameConfig.ANNOUNCEMENT_URL))
            {
                request.timeout = 10;
                request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Linux; Android 10) AppleWebKit/537.36");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[AnnouncementSystem] 请求失败: {request.error}，跳过公告");
                    IsChecked = true;
                    OnAnnouncementDisabled?.Invoke();
                    yield break;
                }

                string htmlContent = request.downloadHandler.text;
                ParseAnnouncementContent(htmlContent);
            }

            IsChecked = true;

            if (IsAnnouncementEnabled && !string.IsNullOrEmpty(AnnouncementContent))
            {
                OnAnnouncementReady?.Invoke(AnnouncementContent);
                Debug.Log($"[AnnouncementSystem] 公告已开启: {AnnouncementContent}");
            }
            else
            {
                OnAnnouncementDisabled?.Invoke();
                Debug.Log("[AnnouncementSystem] 公告未开启或内容为空");
            }
        }

        /// <summary>
        /// 解析QQ收藏页面内容，提取纯文本后按行读取
        /// 第一行：开/关
        /// 第二行：公告内容
        /// </summary>
        private void ParseAnnouncementContent(string html)
        {
            // 提取页面纯文本（去除HTML标签）
            string plainText = ExtractPlainText(html);

            // 按行分割
            string[] lines = plainText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 1)
            {
                IsAnnouncementEnabled = false;
                return;
            }

            // 第一行：开/关
            string firstLine = lines[0].Trim().ToLower();
            IsAnnouncementEnabled = firstLine.Contains("开") || firstLine == "on" || firstLine == "true";

            // 第二行：公告内容
            if (lines.Length >= 2)
            {
                AnnouncementContent = lines[1].Trim();
            }
            else
            {
                AnnouncementContent = "";
            }

            Debug.Log($"[AnnouncementSystem] 解析结果 - 开关:{IsAnnouncementEnabled}, 内容:{AnnouncementContent}");
        }

        /// <summary>
        /// 从HTML中提取纯文本
        /// </summary>
        private string ExtractPlainText(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";

            // 移除script和style标签内容
            html = System.Text.RegularExpressions.Regex.Replace(html, @"<script[^>]*>[\s\S]*?</script>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"<style[^>]*>[\s\S]*?</style>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // 将换行标签转为换行符
            html = html.Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n");
            html = html.Replace("</p>", "\n").Replace("</div>", "\n");

            // 移除所有HTML标签
            html = System.Text.RegularExpressions.Regex.Replace(html, @"<[^>]+>", "");

            // 解码HTML实体
            html = html.Replace("&nbsp;", " ").Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"");

            // 去除多余空白
            html = System.Text.RegularExpressions.Regex.Replace(html, @"[ \t]+", " ");
            html = System.Text.RegularExpressions.Regex.Replace(html, @"\n\s*\n", "\n");

            return html.Trim();
        }
    }
}
