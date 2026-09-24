//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!
 *    @file     SoundCheckController.cs
 *    @brief    サウンドチェックシーン制御
 *
 *    @date     2026/05/08
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
     *    @brief       サウンドチェックシーン上の簡易メニューを制御する.
     */
    //==========================================================================
    public class SoundCheckController : MonoBehaviour
    {
        // サウンドチェックシーン上に表示する簡易メニュー項目.
        [SerializeField] TextMeshProUGUI _item1Text;
        [SerializeField] TextMeshProUGUI _item2Text;

        int _selectedIndex;
        TextMeshProUGUI[] _itemTexts;

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
            _itemTexts = new[] { _item1Text, _item2Text };
            RefreshSelection();
            await UniTask.CompletedTask;

            // BGM鳴動テスト
            // ISoundManager.Instance.PlayBGM("BGM01");
        }

        private void Update()
        {
            // 常駐システムは具象クラスではなく、ServiceLocator経由のInterfaceから取得する.
            IPlayerInputManager inputManager = IPlayerInputManager.Instance;
            if (_itemTexts == null || inputManager == null)
            {
                return;
            }

            PlayerInputManager.UIActions inputUI = inputManager.UIAction;
            if (inputUI.AxisUp)
            {
                _selectedIndex = _selectedIndex == 0 ? 1 : 0;
                RefreshSelection();
            }
            else if (inputUI.AxisDown)
            {
                _selectedIndex = _selectedIndex == 0 ? 1 : 0;
                RefreshSelection();
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