using System.Collections.Generic;
using UnityEngine;

using R3;
using R3.Triggers;

using Cysharp.Threading.Tasks;

using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SGGames.Game.Sys
{
    // UISelectCursorTargetが選択された時に通知(子から親)
    // フォーカスが移動した時、親UIへカーソル対象のRectTransformを通知するためのInterface。
    public interface INotifySelectedCursorTarget
    {
        void OnFocusedCursotTarget(RectTransform target);
    }


    //==================================================================
    /// <summary>
    /// UIで選択可能物となる
    /// ・UISelectableGroupの参加にいる必要がある。
    /// </summary>
    //==================================================================
    public class UISelectable : Selectable, IPointerClickHandler/*, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler*/, IPointerMoveHandler
    {
        // UnityのSelectableを拡張し、ゲームパッド/キーボード/マウス共通の選択対象として扱う。
        [Header("=======================\nここからUISelectable\n=======================")]

        [SerializeField] int _priority;
        public int Priority => _priority;

        // ID
        [Header("ID")]
        [SerializeField] int _IDInt;
        [SerializeField] string _IDString;
        public int IDInt { get => _IDInt; set => _IDInt = value; }
        public string IDString { get => _IDString; set => _IDString = value; }
        public object IDRef { get; set; } = null;

        // UnityEvent(あまり使わんように)
        [Header("決定時実行イベント")]
        [SerializeField] UISelectableDecideEvent _onDecideEvent;

        [Header("マウスが乗るとフォーカスする")]
        [SerializeField] bool _focusOnMouseHover = true;

        [SerializeField] DecideTypes _decideType;
        [Tooltip("マウス専用")]
        [SerializeField] bool _onlyMouse = false;

        [Header("各エフェクトエフェクト")]
        [SerializeField] GameObject _focusEffectObj;
        [SerializeField] GameObject _highlightEffectObj;
        [SerializeField] GameObject _disableEffectObj;

    //    [Header("サウンド")]
    //    [SerializeField] ISoundManager.UISEs _uiseDecide = ISoundManager.UISEs.Decide01;

        [Header("テキストへの参照")]
        [SerializeField] TMPro.TextMeshProUGUI _uiText;
        public TMPro.TextMeshProUGUI UIText => _uiText;

        [Header("TMProなどの既存のSelectableにフォーカスを入れる")]
        [SerializeField] Selectable _focusSelectable;

        /*
        // 拡張機能
        [SerializeReference, SubclassSelector] ExtFeatureBase _extFeature;

        // 拡張Transition
        [SerializeReference, SubclassSelector] ExtTransitionBase _extTransition;
        */

        // 所属先グループを取得
        UISelectableGroup _nowOwnerGroup = null;
        // 実行中は所属中のGroup、Editor上では親階層やGlobalGroupから所属先を推定する。
        public UISelectableGroup GetOwnerGroup()
        {
            if (Application.isPlaying)
            {
                return _nowOwnerGroup;
            }
            else
            {
                var gp = GetComponentInParent<UISelectableGroup>(true);
                if (gp != null) return gp;

                return UISelectableGroup.s_GlobalGroup;

            }
        }

        // 一時保存用
        UISelectableGroup _saveOwnerGroup;

        // フォーカス時のイブジェクト表示を使用してる？
        public bool UseFocusEffectObject => _focusEffectObj != null;

        // アクション結果
        Actions _nowAction;

        public Actions NowAction
        {
            get => _nowAction;
            set => _nowAction = value;
        }

        public enum Actions
        {
            None,
            Decide,     // 決定された
            Menu,       // メニュー表示された
            CantDecide, // 決定できない(interactableがoff)
        }

        public enum DecideTypes
        {
            Down,
            Click,
        }

        // ユーザーデータ
        public object UserData { get; set; } = null;

        // フォーカスされているか
        public bool IsFocused { get; private set; }

        // フォーカスフラグの操作
        // 選択状態の見た目とUnity UI上のSelect状態をまとめて切り替える。
        public void SetFocusedFlag(bool enable)
        {
            if (IsFocused == enable) return;

            IsFocused = enable;

            // フォーカスエフェクト
            if (_focusEffectObj != null)
            {
                _focusEffectObj.SetActive(enable);
    //            if(_extTransition != null) _extTransition.Enable(enable, this);
            }

            if (enable)
            {
                // Unity UI 選択
                if (_focusSelectable == null)
                {
                    Select();
                }
                else
                {
                    _focusSelectable.Select();
                }

                // 通知
                if (enable)
                {
                    var notify = GetComponentInParent<INotifySelectedCursorTarget>();
                    if (notify != null)
                    {
                        notify.OnFocusedCursotTarget(transform as RectTransform);
                    }
                }
            }
        }





        //==========================================
        //
        // 通知イベント
        //
        //==========================================
        // 非同期処理中か
        bool _nowProcessing;

        [System.NonSerialized]
        public Subject<Actions> _onNotifyEvent = new();

        // イベント購読
        //  例)
        //  _fashionSwitch.SubscribeNotifyEvent(async action =>
        //  {
        //  });
        // 決定やメニューなどの操作通知を、非同期処理として購読できるようにする。
        public System.IDisposable SubscribeNotifyEvent(System.Func<Actions, UniTask> selectable)
        {
            return _onNotifyEvent.Subscribe(async s =>
            {
                _nowProcessing = true;
                try
                {
                    await selectable(s);
                }
                catch
                {
                }
                _nowProcessing = false;
            });
        }
        // イベント発行
        public async UniTask NotifyEvent(Actions action)
        {
            _onNotifyEvent.OnNext(action);
            // 待つ
            if (_nowProcessing)
            {
                await UniTask.WaitWhile(() => _nowProcessing);
            }
        }

        // 親へ選択変更通知を強制発行する(フォーカス時のみ有効)
        public void ForceNotifyChangeSelectedEvent()
        {
            if (_nowOwnerGroup == null) return;
            if (_nowOwnerGroup.CurrentSelected != this) return;

            _nowOwnerGroup.OnSelectChanged.OnNext(this);
        }

        //==========================================
        //
        // 選択・実行関係
        //
        //==========================================

        // Selectableクラスからのコピー
        // ・UISelectable以外には繋がない
        // 同じUISelectableGroupに属する候補だけを対象に、指定方向の移動先を探す。
        public new Selectable FindSelectable(Vector3 dir)
        {
            if (_onlyMouse) return null;
            var ownerGp = GetOwnerGroup();

            dir = dir.normalized;
            Vector3 localDir = Quaternion.Inverse(transform.rotation) * dir;
            Vector3 pos = transform.TransformPoint(GetPointOnRectEdge(transform as RectTransform, localDir));
            float maxScore = Mathf.NegativeInfinity;
            float maxFurthestScore = Mathf.NegativeInfinity;
            float score = 0;

            bool wantsWrapAround = navigation.wrapAround && (navigation.mode == Navigation.Mode.Vertical || navigation.mode == Navigation.Mode.Horizontal);

            Selectable bestPick = null;
            Selectable bestFurthestPick = null;

            for (int i = 0; i < s_SelectableCount; ++i)
            {
                UISelectable sel = s_Selectables[i] as UISelectable;

                if (sel == null) continue;

                // マウス専用なら無視
                if (sel._onlyMouse) continue;

                // 同じ親の管理下か？
                if (sel.GetOwnerGroup() != ownerGp) continue;

                if (sel == this)
                    continue;

                if (sel.navigation.mode == Navigation.Mode.None)
    //            if (!sel.IsInteractable() || sel.navigation.mode == Navigation.Mode.None)
                    continue;

    #if UNITY_EDITOR
                // Apart from runtime use, FindSelectable is used by custom editors to
                // draw arrows between different selectables. For scene view cameras,
                // only selectables in the same stage should be considered.
                if (Camera.current != null && !UnityEditor.SceneManagement.StageUtility.IsGameObjectRenderedByCamera(sel.gameObject, Camera.current))
                    continue;
    #endif

                var selRect = sel.transform as RectTransform;
                Vector3 selCenter = selRect != null ? (Vector3)selRect.rect.center : Vector3.zero;
                Vector3 myVector = sel.transform.TransformPoint(selCenter) - pos;

                // Value that is the distance out along the direction.
                float dot = Vector3.Dot(dir, myVector);

                // If element is in wrong direction and we have wrapAround enabled check and cache it if furthest away.
                if (wantsWrapAround && dot < 0)
                {
                    score = -dot * myVector.sqrMagnitude;

                    if (score > maxFurthestScore)
                    {
                        maxFurthestScore = score;
                        bestFurthestPick = sel;
                    }

                    continue;
                }

                // Skip elements that are in the wrong direction or which have zero distance.
                // This also ensures that the scoring formula below will not have a division by zero error.
                if (dot <= 0)
                    continue;

                // This scoring function has two priorities:
                // - Score higher for positions that are closer.
                // - Score higher for positions that are located in the right direction.
                // This scoring function combines both of these criteria.
                // It can be seen as this:
                //   Dot (dir, myVector.normalized) / myVector.magnitude
                // The first part equals 1 if the direction of myVector is the same as dir, and 0 if it's orthogonal.
                // The second part scores lower the greater the distance is by dividing by the distance.
                // The formula below is equivalent but more optimized.
                //
                // If a given score is chosen, the positions that evaluate to that score will form a circle
                // that touches pos and whose center is located along dir. A way to visualize the resulting functionality is this:
                // From the position pos, blow up a circular balloon so it grows in the direction of dir.
                // The first Selectable whose center the circular balloon touches is the one that's chosen.
                score = dot / myVector.sqrMagnitude;

                if (score > maxScore)
                {
                    maxScore = score;
                    bestPick = sel;
                }
            }

            if (wantsWrapAround && null == bestPick) return bestFurthestPick;

            return bestPick;
        }

        private static Vector3 GetPointOnRectEdge(RectTransform rect, Vector2 dir)
        {
            if (rect == null)
                return Vector3.zero;
            if (dir != Vector2.zero)
                dir /= Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
            dir = rect.rect.center + Vector2.Scale(rect.rect.size, dir * 0.5f);
            return dir;
        }

        public override Selectable FindSelectableOnLeft()
        {
            if (navigation.mode == Navigation.Mode.Explicit)
            {
                return navigation.selectOnLeft;
            }
            if ((navigation.mode & Navigation.Mode.Horizontal) != 0)
            {
                return FindSelectable(transform.rotation * Vector3.left);
            }
            return null;
        }
        public override Selectable FindSelectableOnRight()
        {
            if (navigation.mode == Navigation.Mode.Explicit)
            {
                return navigation.selectOnRight;
            }
            if ((navigation.mode & Navigation.Mode.Horizontal) != 0)
            {
                return FindSelectable(transform.rotation * Vector3.right);
            }
            return null;
        }
        public override Selectable FindSelectableOnUp()
        {
            if (navigation.mode == Navigation.Mode.Explicit)
            {
                return navigation.selectOnUp;
            }
            if ((navigation.mode & Navigation.Mode.Vertical) != 0)
            {
                return FindSelectable(transform.rotation * Vector3.up);
            }
            return null;
        }
        public override Selectable FindSelectableOnDown()
        {
            if (navigation.mode == Navigation.Mode.Explicit)
            {
                return navigation.selectOnDown;
            }
            if ((navigation.mode & Navigation.Mode.Vertical) != 0)
            {
                return FindSelectable(transform.rotation * Vector3.down);
            }
            return null;
        }


        public bool SelectLeft()
        {
            if (!IsActive()) return false;
    //        if (!IsActive() || !IsInteractable()) return false;

            /*
            // 特殊処理
            if (_extFeature != null)
            {
                if (_extFeature.OnPressLeft()) return true;
            }
            */

            // 選択
            var sel = FindSelectableOnLeft();
            if (sel is UISelectable selectable)
            {
                var ownerGp = GetOwnerGroup();
                if (ownerGp != null)
                {
                    ownerGp.CurrentSelected = selectable;
                    return true;
                }
            }
            return false;
        }
        public bool SelectRight()
        {
            if (!IsActive()) return false;
    //        if (!IsActive() || !IsInteractable()) return false;

            /*
            // 特殊処理
            if (_extFeature != null)
            {
                if (_extFeature.OnPressRight()) return true;
            }
            */

            // 選択
            var sel = FindSelectableOnRight();
            if (sel is UISelectable selectable)
            {
                var ownerGp = GetOwnerGroup();
                if (ownerGp != null)
                {
                    ownerGp.CurrentSelected = selectable;
                    return true;
                }
            }
            return false;
        }
        public bool SelectUp()
        {
            if (!IsActive()) return false;
    //        if (!IsActive() || !IsInteractable()) return false;

            /*
            // 特殊処理
            if (_extFeature != null)
            {
                if (_extFeature.OnPressUp()) return true;
            }
            */

            // 選択
            var sel = FindSelectableOnUp();
            if (sel is UISelectable selectable)
            {
                var ownerGp = GetOwnerGroup();
                if (ownerGp != null)
                {
                    ownerGp.CurrentSelected = selectable;
                    return true;
                }
            }
            return false;
        }
        public bool SelectDown()
        {
            if (!IsActive()) return false;
    //        if (!IsActive() || !IsInteractable()) return false;

            /*
            // 特殊処理
            if (_extFeature != null)
            {
                if (_extFeature.OnPressDown()) return true;
            }
            */

            // 選択
            var sel = FindSelectableOnDown();
            if (sel is UISelectable selectable)
            {
                var ownerGp = GetOwnerGroup();
                if (ownerGp != null)
                {
                    ownerGp.CurrentSelected = selectable;
                    return true;
                }
            }
            return false;
        }

        /*
        public async UniTask<bool> Decide(bool isGamepad)
        {
            if (_extFeature == null) return false;
            if (isGamepad)
            {
                return await _extFeature.OnDecideGamepad(this);
            }

            return false;
        }
        */

        // 決定時の特別な実行処理
        // 決定入力時の通知、UnityEvent、必要なら外部の非同期処理まで順番に実行する。
        public async UniTask ExecDecideProc(bool isGamepad)
        {
            if (!IsActive() || !IsInteractable()) return;

            var cancelToken = this.GetCancellationTokenOnDestroy();

            /*
            // SE
            ISoundManager.Instance.PlaySE_2D(
                ISoundManager.Instance.GetUISE(_uiseDecide)
            );
            */

            // 決定時 通知
            await NotifyEvent(Actions.Decide);

            /*
            // 特殊処理
            if (_extFeature != null)
            {
                // ゲームパッド専用
                if (isGamepad)
                {
                    if (await _extFeature.OnDecideGamepad(this))
                    {
                        return;
                    }
                }
                // 
                if (await _extFeature.OnDecide(this))
                {
                    return;
                }
            }
            */

            // イベント実行
            {
                EventArg arg = new();
                arg.Target = this;
                _onDecideEvent.Invoke(arg);

                // 待機
                while (arg.NowProcessing)
                {
                    if (cancelToken.IsCancellationRequested) return;
                    await UniTask.DelayFrame(1);
                }
            }

        }

        public void ClearAction()
        {
            _nowAction = Actions.None;
        }



        //==========================================
        //
        // 
        //
        //==========================================
        protected override void Awake()
        {
            base.Awake();

        }

        protected override void Start()
        {
            base.Start();

            if (Application.isPlaying)
            {
                if (IsFocused == false)
                {
                    // フォーカルエフェクトOFF
                    if (_focusEffectObj != null) _focusEffectObj.SetActive(false);
                    if (_highlightEffectObj != null) _highlightEffectObj.SetActive(false);
                }

                // マウス専用時は、Gamepad操作で非表示にする
                if (_onlyMouse)
                {
                    IPlayerInputManager.Instance.OnChangeDevice.Subscribe(type =>
                    {
                        if (type == PlayerInputManager.DevideTypes.Keyboard)
                        {
                            gameObject.SetActive(true);
                        }
                        else
                        {
                            gameObject.SetActive(false);
                        }
                    }).AddTo(this);
                }

                if (_focusSelectable != null)
                {
                    _focusSelectable.OnSelectAsObservable().Subscribe(_ =>
                    {
                        if (Cursor.visible == false) return;

                        if (!IsActive()) return;
    //                    if (!IsActive() || !IsInteractable()) return;

                        var selGroup = GetOwnerGroup();
                        if (selGroup == null) return;
                        if (selGroup.isActiveAndEnabled == false) return;

                        selGroup.CurrentSelected = this;

                    }).AddTo(this);
                }

                /*
                // イベントを親へ伝播させないために、EventTriggerを使用する
                EventTrigger trigger = GetComponent<EventTrigger>();
                if (trigger != null)// trigger = gameObject.AddComponent<EventTrigger>();
                {
                    EventTrigger.Entry entry = new EventTrigger.Entry();
                    entry.eventID = EventTriggerType.PointerEnter;
                    entry.callback.AddListener(eventData =>
                    {
                        if (Cursor.visible == false) return;

                        if (!IsActive() || !IsInteractable()) return;

                        if (Application.isPlaying == false) return;

                        var selGroup = GetOwnerGroup();
                        if (selGroup == null) return;
                        if (selGroup.isActiveAndEnabled == false) return;

                        if (_focusOnMouseHover)
                        {
                            selGroup.CurrentSelected = this;
                        }

                        DebugLogger.Log($"Enter {name}", DebugLogger.Colors.orange);
                    });
                    trigger.triggers.Add(entry);
                }
                */
            }
        }

        protected override void OnEnable()
        {
            // 有効化されたら親Groupへ参加し、カーソル移動や決定処理の対象に入る。
            base.OnEnable();

            if (Application.isPlaying)
            {
                // 新グループへ登録
                var gp = GetComponentInParent<UISelectableGroup>(true);
                if (gp != null)
                {
                    _nowOwnerGroup = gp;
                    gp.Join(this);
                }
            }
        }
        protected override void OnDisable()
        {
            // 無効化されたら所属Groupから外し、選択候補に残らないようにする。
            base.OnDisable();


            if (Application.isPlaying)
            {
                // 登録グループから解除
                if (_nowOwnerGroup != null)
                {
                    _nowOwnerGroup.Leave(this);
                    _nowOwnerGroup = null;
                }
            }
        }
        /*
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (Application.isPlaying)
            {
                // 登録グループから解除
                if (_nowOwnerGroup != null)
                {
                    _nowOwnerGroup.Leave(this);
                    _nowOwnerGroup = null;
                }
            }
        }
        */

        protected override void OnBeforeTransformParentChanged()
        {
            base.OnBeforeTransformParentChanged();

            if (Application.isPlaying)
            {
                _saveOwnerGroup = _nowOwnerGroup;

                /*
                // 登録グループから解除
                if (_nowOwnerGroup != null)
                {
                    _nowOwnerGroup.Leave(this);
                    _nowOwnerGroup = null;
                }
                */
            }

        }
        // 親変更時
        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();

            if (Application.isPlaying && isActiveAndEnabled)
            {
                // 新グループ検索
                var gp = GetComponentInParent<UISelectableGroup>(true);

                // グループ変更なし
                if(gp == _saveOwnerGroup)
                {
                    _saveOwnerGroup = null;
                    return;
                }

                // 変更あり 前回のグループから脱退
                if(_saveOwnerGroup != null)
                {
                    _saveOwnerGroup.Leave(this);
                    _saveOwnerGroup = null;
                }

                _nowOwnerGroup = gp;

                // 新しいグループへ参加
                if (_nowOwnerGroup != null)
                {
                    gp.Join(this);
                }
            }
        }


        //==========================================
        //
        // イベント関係
        //
        //==========================================

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (!gameObject.activeInHierarchy)
                return;

            //        DebugLogger.Log($"{state}", DebugLogger.Colors.yellow);

            switch (state)
            {
                case SelectionState.Normal:
                    //                if (_focusEffectObj != null) _focusEffectObj.SetActive(false);
                    if (_highlightEffectObj != null) _highlightEffectObj.SetActive(false);
                    break;
                case SelectionState.Highlighted:
                    //                if (_focusEffectObj != null) _focusEffectObj.SetActive(false);
                    if (_highlightEffectObj != null) _highlightEffectObj.SetActive(true);
                    break;
                case SelectionState.Pressed:
                    break;
                case SelectionState.Selected:
                    //                if (_focusEffectObj != null) _focusEffectObj.SetActive(true);
                    if (_highlightEffectObj != null) _highlightEffectObj.SetActive(false);
                    break;
                case SelectionState.Disabled:
                    //                if (_focusEffectObj != null) _focusEffectObj.SetActive(false);
                    if (_highlightEffectObj != null) _highlightEffectObj.SetActive(false);
                    break;
                default:
                    //                if (_focusEffectObj != null) _focusEffectObj.SetActive(false);
                    if (_highlightEffectObj != null) _highlightEffectObj.SetActive(false);
                    break;
            }

        }

        /*
        public override void OnSelect(BaseEventData eventData)
        {
            if (Cursor.visible == false) return;
            base.OnSelect(eventData);

            SetFocusedFlag(true);

            DebugLogger.Log($"OnSelect", DebugLogger.Colors.green);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            SetFocusedFlag(false);

            DebugLogger.Log($"OnDeselect", DebugLogger.Colors.green);
        }
        */

        /*
        public override void OnPointerEnter(PointerEventData eventData)
        {
        }
        */

        List<RaycastResult> _raycastResults = new();

        // マウス移動で項目に触れた時、必要に応じてこの項目へフォーカスを移す。
        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
        {
            if (Cursor.visible == false) return;

            if (!IsActive()) return;
    //        if (!IsActive() || !IsInteractable()) return;

            if (Application.isPlaying == false) return;

            var selGroup = GetOwnerGroup();
            if (selGroup == null) return;
            if (selGroup.isActiveAndEnabled == false) return;

            /*
            _raycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, _raycastResults);
            if (_raycastResults.Count >= 1 && _raycastResults[0].gameObject != gameObject) return;
            */

            if (_focusOnMouseHover)
            {
                selGroup.CurrentSelected = this;
            }

        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (Cursor.visible == false) return;
            base.OnPointerExit(eventData);

            if (!IsActive()) return;
    //        if (!IsActive() || !IsInteractable()) return;

            if (Application.isPlaying == false) return;

            var selGroup = GetOwnerGroup();
            if (selGroup == null) return;
            if (selGroup.isActiveAndEnabled == false) return;

            if (selGroup.CurrentSelected == this)
            {
                selGroup.CurrentSelected = null;
            }
            //]        SetFocusedFlag(false);
        }

        // 決定(押す)
        // 押した瞬間に決定扱いする設定の場合、ここで操作結果を一時保存する。
        public override void OnPointerDown(PointerEventData eventData)
        {
            if (Cursor.visible == false) return;

    //        if (!IsActive() || !IsInteractable()) return;
            if (!IsActive()) return;

            if (Application.isPlaying == false) return;

            var selGroup = GetOwnerGroup();
            if (selGroup == null) return;
            if (selGroup.isActiveAndEnabled == false) return;

            if (_decideType != DecideTypes.Down) return;

            // 現在選択中でないなら、無視
            if (selGroup.CurrentSelected != this) return;

            // 決定不可
            if (IsInteractable() == false)
            {
                _nowAction = Actions.CantDecide;
                return;
            }

            base.OnPointerDown(eventData);

            // 決定
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _nowAction = Actions.Decide;
                //            _onNotifyEvent.OnNext(_nowAction);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                _nowAction = Actions.Menu;
                //            _onNotifyEvent.OnNext(_nowAction);
            }
        }

        // 決定(クリック)
        // クリック完了時に決定扱いする設定の場合、ここで操作結果を一時保存する。
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (Cursor.visible == false) return;

    //        if (!IsActive() || !IsInteractable()) return;
            if (!IsActive()) return;

            if (Application.isPlaying == false) return;

            var selGroup = GetOwnerGroup();
            if (selGroup == null) return;
            if (selGroup.isActiveAndEnabled == false) return;

            if (_decideType != DecideTypes.Click) return;

            // 現在選択中でないなら、無視
            if (selGroup.CurrentSelected != this) return;

            // 決定不可
            if (IsInteractable() == false)
            {
                _nowAction = Actions.CantDecide;
                return;
            }

            // 決定
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _nowAction = Actions.Decide;
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                _nowAction = Actions.Menu;
            }
        }

        // 決定Unityイベント用(あまり使わないように)
        public class EventArg
        {
            // UnityEventへ渡す決定イベント情報。非同期処理中かどうかも呼び出し側へ伝える。
            public UISelectable Target { get; set; }

            /// <summary>
            /// asyncの場合は、呼び出し関数内でこれをtrueにしてから処理を開始し、最後にFalseにする
            /// </summary>
            public bool NowProcessing { get; set; } = false;
        }

        [System.Serializable]
        public class UISelectableDecideEvent : UnityEngine.Events.UnityEvent<EventArg> { }

        //==========================================
        //
        // 特殊処理
        //
        //==========================================

        // 
        [System.Serializable]
        public abstract class ExtFeatureBase
        {
            public virtual bool TestMode => false;

            public virtual bool OnPressLeft() => false;
            public virtual bool OnPressRight() => false;
            public virtual bool OnPressUp() => false;
            public virtual bool OnPressDown() => false;

            // ゲームパッド専用 決定
            public virtual UniTask<bool> OnDecideGamepad(UISelectable owner) => default;

            // 決定(OnDecideGamepadと両方発動)
            // 戻り値：true 後の処理(イベント実行)は実行しない
            public virtual UniTask<bool> OnDecide(UISelectable owner) => default;
        }

        // スライダー
        [System.Serializable]
        public class Ext_Slider : ExtFeatureBase
        {
            [SerializeField] Slider _slider;
            [SerializeField] float _operationValue;

            public override bool TestMode => false;

            public override bool OnPressLeft()
            {
                if (_slider == null) return false;

                if (_slider.direction == Slider.Direction.LeftToRight)
                {
                    _slider.value -= _operationValue;
                    return true;
                }
                else if (_slider.direction == Slider.Direction.RightToLeft)
                {
                    _slider.value += _operationValue;
                    return true;
                }
                return false;
            }
            public override bool OnPressRight()
            {
                if (_slider == null) return false;
                if (_slider.direction == Slider.Direction.LeftToRight)
                {
                    _slider.value += _operationValue;
                    return true;
                }
                else if (_slider.direction == Slider.Direction.RightToLeft)
                {
                    _slider.value -= _operationValue;
                    return true;
                }
                return true;
            }

            /* 決定で別処理を動かすとき
            public override async UniTask<bool> OnDecideGamepad(UISelectable owner)
            {
                var cancelToken = owner.GetCancellationTokenOnDestroy();
                var inputProvider = UIInputProvider.Singleton;

                while(cancelToken.IsCancellationRequested == false)
                {
                    if (inputProvider.IsPressedAction(GameInputActionTypes.UI_SelectLeft))
                    {
                        OnPressLeft();
                    }
                    if (inputProvider.IsPressedAction(GameInputActionTypes.UI_SelectRight))
                    {
                        OnPressRight();
                    }

                    if (inputProvider.IsPressedAction(GameInputActionTypes.UI_Cancel))
                    {
                        await UniTask.DelayFrame(1);
                        return false;
                    }

                    await UniTask.DelayFrame(1);
                }

                return false;
            }
            */
        }

        //==========================================
        //
        // 特殊Transition
        //
        //==========================================
        /*

        [System.Serializable]
        public abstract class ExtTransitionBase
        {

            public abstract void Enable(bool enable, UISelectable owner);
        }

        [System.Serializable]
        public class ExtTransition_Scale : ExtTransitionBase
        {
            [SerializeField] float _maxScale = 1.03f;
            [SerializeField] float _duration = 0.2f;

            Tween _tween;

            public override void Enable(bool enable, UISelectable owner)
            {
                if (enable)
                {
                    _tween.Kill();
                    _tween = owner.transform.DOScale(1.03f, 0.2f).SetUpdate(true);
                }
                else
                {
                    _tween.Kill();
                    _tween = owner.transform.DOScale(1.0f, 0.2f).SetUpdate(true);
                }
            }
        }
        */
    }
}