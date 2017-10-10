using System;
using System.Collections.Generic;
using System.IO;
using Baidu.Aip.Speech;
using JasonLib;

namespace CodeTool.common
{
    public class Speech
    {
        private readonly Asr _asrClient;
        private readonly Tts _ttsClient;

        public Speech()
        {
            _asrClient = new Asr("GXrdsGDVgDKOutcdlo3jNnmq", "29AfMqIO4CN5rOl0cu3lRo2BuAEvqsIG");
            _ttsClient = new Tts("GXrdsGDVgDKOutcdlo3jNnmq", "29AfMqIO4CN5rOl0cu3lRo2BuAEvqsIG");
        }

        // 识别本地文件
        public string AsrData(string fileFullName)
        {
            var data = File.ReadAllBytes(fileFullName);
            var result = _asrClient.Recognize(data, "pcm", 16000);
            return result.ToString();
        }

        // 合成
        public string Tts(string tex, int per = 0, int spd = 5, int vol = 7)
        {
            var fileName = JlMd5.HashPassword(tex + "|per|" + per + "|spd|" + spd + "|vol|" + vol) + ".mp3";
            var fullName = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + JlConfig.GetValue<string>("SaveFilePath") + fileName;

            if (!File.Exists(fullName))
            {
                // 可选参数
                var option = new Dictionary<string, object>()
                {
                    {"spd", spd}, // 语速
                    {"vol", 7}, // 音量
                    {"per", per}  // 发音人
                };
                var result = _ttsClient.Synthesis(tex, option);
                if (result.ErrorCode == 0)  // 或 result.Success
                {
                    File.WriteAllBytes(fullName, result.Data);
                }
            }

            return fileName;
        }

    }
}