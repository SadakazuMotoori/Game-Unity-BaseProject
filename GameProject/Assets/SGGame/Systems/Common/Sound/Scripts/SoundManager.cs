//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!
 *    @file     SoundManager.cs
 *    @brief    サウンド管理
 *
 *    @date     2026/05/01
 *    @author   Sadakazu Motoori
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
using UnityEngine;
using CriWare;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

using SGSys;

namespace SGGames.Game.Sys
{
    //==========================================================================
    /**
     *    @brief       サウンド再生カテゴリ.
     */
    //==========================================================================
    public enum SoundCategory
    {
        BGM,
        SE,
        EnvironmentSE,
        Voice,
        System,

        _Count_,
    }

    //==========================================================================
    /**
     *    @brief       サウンド管理サービスの公開窓口.
     */
    //==========================================================================
    public interface ISoundManager : IService<ISoundManager>
    {
        UniTask WaitUntilReady(CancellationToken cancellationToken);
        void Play(SoundCategory category, string cueName);
        void Stop(SoundCategory category);
        void PlayBGM(string cueName);
        void StopBGM();
        void PlaySE_2D(string cueName);
        void StopSE();
        void SetVolume(SoundCategory category, float volume);
    }

    //==========================================================================
    /**
     *    @brief       CRI Atomを使ったサウンド管理クラス.
     */
    //==========================================================================
    [DefaultExecutionOrder(-1)]
    public class SoundManager : MonoBehaviour, ISoundManager
    {
        [Header("ADX")]
        [SerializeField] string _acfFile = "";

        CriAtom _criAtom;
        CriAtomSource[] _sources;

        readonly struct CueSheetDefinition
        {
            public readonly SoundCategory Category;
            public readonly string CueSheetName;
            public readonly string AcbFile;
            public readonly string AwbFile;

            public CueSheetDefinition(SoundCategory category, string cueSheetName, string acbFile, string awbFile)
            {
                Category = category;
                CueSheetName = cueSheetName;
                AcbFile = acbFile;
                AwbFile = awbFile;
            }
        }

        static readonly CueSheetDefinition[] s_cueSheetDefinitions =
        {
            new(SoundCategory.BGM, "BGM", "BGM.acb", "BGM.awb"),
            new(SoundCategory.SE, "SE", "SE.acb", "SE.awb"),
            new(SoundCategory.EnvironmentSE, "EnvironmentSE", "EnvironmentSE.acb", "EnvironmentSE.awb"),
            new(SoundCategory.Voice, "Voice", "Voice.acb", "Voice.awb"),
            new(SoundCategory.System, "System", "System.acb", "System.awb"),
        };

        void Awake()
        {
            UnityEngine.Object currentManager = ISoundManager.Instance as UnityEngine.Object;
            if (currentManager != null && currentManager != this)
            {
                Destroy(gameObject);
                return;
            }

            InitializeCriAtom();
            InitializeSources();

            ServiceLocator<ISoundManager>.Register(this);
        }

        void OnDestroy()
        {
            UnityEngine.Object currentManager = ISoundManager.Instance as UnityEngine.Object;
            if (currentManager == this)
            {
                ServiceLocator<ISoundManager>.Unregister();
            }
        }

        public async UniTask WaitUntilReady(CancellationToken cancellationToken)
        {
            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.GetCancellationTokenOnDestroy());
            await UniTask.WaitUntil(() => !CriAtom.CueSheetsAreLoading, cancellationToken: linkedCancellation.Token);
        }

        public void PlayBGM(string cueName)
        {
            Play(SoundCategory.BGM, cueName);
        }

        public void StopBGM()
        {
            Stop(SoundCategory.BGM);
        }

        public void PlaySE_2D(string cueName)
        {
            Play(SoundCategory.SE, cueName);
        }

        public void StopSE()
        {
            Stop(SoundCategory.SE);
        }

        public void Play(SoundCategory category, string cueName)
        {
            if (string.IsNullOrEmpty(cueName)) return;

            CriAtomSource source = GetSource(category);
            if (source == null || _criAtom == null) return;

            source.cueSheet = GetCueSheetName(category);
            CriAtomCueSheet cueSheet = System.Array.Find(_criAtom.cueSheets, sheet => sheet.name == source.cueSheet);
            if (cueSheet?.acb == null || !cueSheet.acb.Exists(cueName)) return;

            source.loop = GetDefaultLoop(category);
            source.Play(cueName);
        }

        public void Stop(SoundCategory category)
        {
            CriAtomSource source = GetSource(category);
            if (source == null) return;

            source.Stop();
        }

        public void SetVolume(SoundCategory category, float volume)
        {
            CriAtomSource source = GetSource(category);
            if (source == null) return;

            source.volume = Mathf.Clamp01(volume);
        }

        void InitializeCriAtom()
        {
            CriAtomCueSheet[] cueSheets = CreateCueSheets();

            if (_criAtom == null)
            {
                GameObject criAtomObject = new GameObject("CriAtom");
                criAtomObject.transform.SetParent(transform);
                criAtomObject.SetActive(false);

                _criAtom = criAtomObject.AddComponent<CriAtom>();
                _criAtom.acfFile = _acfFile;
                _criAtom.cueSheets = cueSheets;
                criAtomObject.SetActive(true);
                return;
            }

            _criAtom.acfFile = _acfFile;
            _criAtom.cueSheets = cueSheets;
        }

        void InitializeSources()
        {
            _sources = new CriAtomSource[(int)SoundCategory._Count_];

            for (int i = 0; i < s_cueSheetDefinitions.Length; i++)
            {
                CueSheetDefinition definition = s_cueSheetDefinitions[i];
                GameObject sourceObject = new GameObject($"{definition.Category}Source");
                sourceObject.transform.SetParent(transform);

                CriAtomSource source = sourceObject.AddComponent<CriAtomSource>();
                source.playOnStart = false;
                source.use3dPositioning = false;
                source.cueSheet = definition.CueSheetName;
                source.loop = GetDefaultLoop(definition.Category);

                _sources[(int)definition.Category] = source;
            }
        }

        CriAtomCueSheet[] CreateCueSheets()
        {
            List<CriAtomCueSheet> cueSheets = new();
            string streamingAssetsPath = CriWare.Common.streamingAssetsPath;
            bool canCheckFiles = !string.IsNullOrEmpty(streamingAssetsPath) && !streamingAssetsPath.Contains("://");

            for (int i = 0; i < s_cueSheetDefinitions.Length; i++)
            {
                CueSheetDefinition definition = s_cueSheetDefinitions[i];
                if (canCheckFiles && !File.Exists(Path.Combine(streamingAssetsPath, definition.AcbFile))) continue;

                string awbFile = definition.AwbFile;
                if (canCheckFiles && !File.Exists(Path.Combine(streamingAssetsPath, awbFile))) awbFile = "";

                cueSheets.Add(new CriAtomCueSheet
                {
                    name = definition.CueSheetName,
                    acbFile = definition.AcbFile,
                    awbFile = awbFile,
                });
            }

            if (cueSheets.Count == 0)
            {
                DebugLog.Warning(SystemConst.DebugGroup.System, "SoundManager : キューシートが未配置のため、音声は再生されません。");
            }

            return cueSheets.ToArray();
        }

        CriAtomSource GetSource(SoundCategory category)
        {
            int index = (int)category;
            if (_sources == null) return null;
            if (index < 0 || index >= _sources.Length) return null;

            return _sources[index];
        }

        string GetCueSheetName(SoundCategory category)
        {
            for (int i = 0; i < s_cueSheetDefinitions.Length; i++)
            {
                if (s_cueSheetDefinitions[i].Category == category)
                {
                    return s_cueSheetDefinitions[i].CueSheetName;
                }
            }

            return "";
        }

        bool GetDefaultLoop(SoundCategory category)
        {
            return category == SoundCategory.BGM || category == SoundCategory.EnvironmentSE;
        }
    }
}