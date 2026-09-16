//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!

 *    @file     IOSKeyChain.cs
 *    @brief    iOSのキーチェーン制御用ユーティリティ
 *
 *    キーチェーンはアプリケーションを端末からアンインストールしても保持されれる値です。
 *    端末の初期化か、プログラムで消さない限り残り続ける情報の為
 *    ここにユーザー固有情報を保存する事で、このアプリを過去にプレイした事があるかどうかを
 *    判定させることが可能です。
 *    アプリ内で生成したUUIDやサーバーから初回登録時に返されたログインID等を保持し
 *    次回起動時にキーチェーンから値を取得し判定する事で、いわゆるリセットマラソン的な処理を防ぐことが可能になります。
 *
 *    UnityEngine.PlayerPrefsと同じような感覚で、キーと紐づく値でアクセスする事が可能です。
 *    キーと値はアプリケーションごとに保持されるため、異なるアプリケーションで同じキーを使っても
 *    被る事はありません。
 *
 *    この処理はiOS端末環境でのみ利用可能です。
 *    それ以外の環境でアクセスした場合何もしません。
 *    SetDataやDelete系処理は何もせず、GetDataはnullを返し、ContainsKeyはfalseを返します。
 *
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

sealed public class IOSKeyChain
{
#if UNITY_IOS && !UNITY_EDITOR
	
	// KeyChain 関連
	[DllImport("__Internal")]
	private static extern string KeyChainGetData( string key );
	[DllImport("__Internal")]
	private static extern void KeyChainSetData( string key, string value );
	[DllImport("__Internal")]
	private static extern void KeyChainDeleteData( string key );
	[DllImport("__Internal")]
	private static extern void KeyChainDeleteDataAll();
	[DllImport("__Internal")]
	private static extern bool KeyChainContainsKey( string key );

	
#endif // UNITY_IOS && !UNITY_EDITOR

	/// <summary>
	/// キーチェーンに保存されている値の取得
	/// </summary>
	/// <param name="key">対象のキー</param>
	/// <returns>紐づいている値</returns>
	public	static	string	GetData( string key )
	{
#if UNITY_IOS && !UNITY_EDITOR
		return KeyChainGetData( key );
#else
		return null;
#endif	// UNITY_IOS && !UNITY_EDITOR
	}
	
	/// <summary>
	/// キーチェーンへの保存
	/// </summary>
	/// <param name="key">対象のキー</param>
	/// <param name="value">紐づいている値</param>
	public static void SetData( string key, string value )
	{
#if UNITY_IOS && !UNITY_EDITOR
		KeyChainSetData( key, value );
#endif //UNITY_IOS && !UNITY_EDITOR
	}
	
	/// <summary>
	/// キーチェーンに保存されている値の削除
	/// </summary>
	/// <param name="key">削除対象のキー</param>
	public static void DeleteData( string key )
	{
#if UNITY_IOS && !UNITY_EDITOR
		KeyChainDeleteData( key );
#endif	// UNITY_IOS && !UNITY_EDITOR
		
	}
	
	/// <summary>
	/// キーチェーンに保存されている全値の削除
	/// </summary>
	public static void DeleteAllData()
	{
#if UNITY_IOS && !UNITY_EDITOR
		KeyChainDeleteDataAll();
#endif	// UNITY_IOS && !UNITY_EDITOR
	}

	/// <summary>
	/// キーチェーンに指定したキーがあるか判定
	/// </summary>
	/// <param name="key">調査対象のキー</param>
	/// <returns>trueの時、存在する</returns>
	public static bool ContainsKey( string key )
	{
#if UNITY_IOS && !UNITY_EDITOR
		return KeyChainContainsKey( key );
#else
		return false;
#endif	// UNITY_IOS && !UNITY_EDITOR
	}
	
}	// sealed public class UtilOs

