//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!
 *    @file     DebugController.cs
 *    @brief    デバッグシーン制御
 *
 *    @date     2026/05/01
 *    @author   Sadakazu Motoori
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using SGGames.Game.Sys;

namespace SGGames.Game.Develop
{
    //==========================================================================
    /**
     *    @brief       デバッグシーン上の簡易メニューを制御する.
     */
    //==========================================================================
    public class DebugController : MonoBehaviour
    {
        // デバッグシーン上に表示する簡易メニュー項目.
        [SerializeField] TextMeshProUGUI _item1Text;
        [SerializeField] TextMeshProUGUI _item2Text;
        [SerializeField] TextMeshProUGUI _item3Text;
        [SerializeField] DebugSystemCheckController _systemChecks;

        int _selectedIndex;
        TextMeshProUGUI[] _itemTexts;

        const int kSoundCheckIndex = 2;
        const string kSoundCheckSceneName = "SoundCheckScene";

        private void Awake()
        {
        }

        void Start()
        {
            Run();
        }

        async void Run()
        {
            // Updateで選択状態を切り替えやすいよう、表示対象を配列としてまとめる.
            _itemTexts = new[] { _item1Text, _item2Text, _item3Text };
            RefreshSelection();
            await UniTask.CompletedTask;

            // BGM鳴動テスト
            ISoundManager.Instance?.PlayBGM("BGM01");
        }

        private void Update()
        {
            if (_systemChecks != null && _systemChecks.IsOpen) return;
            if (IWindowManager.Instance != null && !IWindowManager.Instance.CanReceiveInput(transform)) return;

            // 常駐システムは具象クラスではなく、ServiceLocator経由のInterfaceから取得する.
            IPlayerInputManager inputManager = IPlayerInputManager.Instance;
            if (_itemTexts == null || inputManager == null)
            {
                return;
            }

            PlayerInputManager.UIActions inputUI = inputManager.UIAction;
            if (inputUI.AxisUp)
            {
                _selectedIndex = _selectedIndex == 0 ? _itemTexts.Length - 1 : _selectedIndex - 1;
                RefreshSelection();
            }
            else if (inputUI.AxisDown)
            {
                _selectedIndex = _selectedIndex == _itemTexts.Length - 1 ? 0 : _selectedIndex + 1;
                RefreshSelection();
            }
            else if (inputUI.Decide || (inputManager.IsInputBlocked == false && UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame))
            {
                if (_selectedIndex == 0 && _systemChecks != null)
                {
                    _systemChecks.Open();
                }
                else if (_selectedIndex == kSoundCheckIndex && ISceneTransitionManager.Instance != null)
                {
                    ISceneTransitionManager.Instance.RequestSceneChange(kSoundCheckSceneName).Forget();
                }
            }
        }

        void RefreshSelection()
        {
            // 選択中の項目だけ色を変え、現在位置が視覚的に分かるようにする.
            for (int i = 0; i < _itemTexts.Length; i++)
            {
                if (_itemTexts[i] == null)
                {
                    continue;
                }

                _itemTexts[i].color = i == _selectedIndex ? Color.red : Color.white;
            }
        }
    }
}
