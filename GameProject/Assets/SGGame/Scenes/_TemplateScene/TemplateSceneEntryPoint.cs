//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!
 *    @file     TemplateSceneEntryPoint.cs
 *    @brief    テンプレートシーン用エントリポイント
 *
 *    @date     2026/05/01
 *    @author   Sadakazu Motoori
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MackySoft.Navigathena;
using MackySoft.Navigathena.SceneManagement;

//==========================================================================
/**
 *    @brief       テンプレートシーン用のSceneEntryPoint.
 *
 *    実際のシーンでは、必要な処理だけを各ライフサイクルへ追加します.
 */
//==========================================================================
public sealed class TemplateSceneEntoryPoint : SceneEntryPointBase
{
    //==========================================================================
    /**
     *    @brief       シーン初期化時の処理.
     */
    //==========================================================================
    protected override async UniTask OnInitialize(ISceneDataReader reader, IProgress<IProgressDataStore> progress, CancellationToken cancellationToken)
    {
        await UniTask.DelayFrame(1);
    }

    protected override async UniTask OnEnter(ISceneDataReader reader, CancellationToken cancellationToken)
    {
        await UniTask.DelayFrame(1);
    }

    protected override async UniTask OnExit(ISceneDataWriter writer, CancellationToken cancellationToken)
    {
        await UniTask.DelayFrame(1);
    }

    protected override async UniTask OnFinalize(ISceneDataWriter writer, IProgress<IProgressDataStore> progress, CancellationToken cancellationToken)
    {
        await UniTask.DelayFrame(1);
    }

    public void OnButton01(string aa)
    {
        if(UnityEngine.InputSystem.Keyboard.current.aKey.wasPressedThisFrame)
        {
        }
    }
}
