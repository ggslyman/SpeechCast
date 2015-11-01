//////////////////////////////////////////////////////////////////////
/*!	@class	AquesTalk2Da

	@brief	規則音声合成エンジン AquesTalk2Da

  音声記号列から音声波形データを生成し、サウンドデバイスに出力する


	@author	N.Yamazaki (Aquest)

	@date	2009/11/28	N.Yamazaki	Creation (from AquesTalkDa.h)
*/
//  COPYRIGHT (C) 2009 AQUEST CORP.
//////////////////////////////////////////////////////////////////////
#if !defined(_AQUESTALK2DA_H_)
#define _AQUESTALK2DA_H_
#ifdef __cplusplus
extern "C"{
#endif

#include <windows.h>

#if defined(AQUESTALK2DA_EXPORTS)
#undef	DllExport
#define DllExport	__declspec( dllexport )
#else
#define DllExport
#endif



/////////////////////////////////////////////
//!	音声を合成して出力（同期タイプ）
//!	音声の再生が終了するまで戻らない
//! @param	koe[in]		音声記号列（NULL終端）
//! @param	iSpeed[in]	発話速度 [%] 50-300 の間で指定
//!	@param	phontDat[in]	phontデータの先頭アドレスを指定します。このDLLのデフォルトPhontを用いるときは０を指定します。
//!	@return	0:正常終了　それ以外：エラーコード
DllExport	int __stdcall AquesTalk2Da_PlaySync(const char *koe, int iSpeed=100, void *phontDat=0);
#ifdef MULTI_STR_CODE
DllExport	int __stdcall AquesTalk2Da_PlaySync_Euc(const char *koe, int iSpeed=100, void *phontDat=0);
DllExport	int __stdcall AquesTalk2Da_PlaySync_Utf8(const char *koe, int iSpeed=100, void *phontDat=0);
DllExport	int __stdcall AquesTalk2Da_PlaySync_Utf16(const unsigned short *koe, int iSpeed=100, void *phontDat=0);
DllExport	int __stdcall AquesTalk2Da_PlaySync_Roman(const char *koe, int iSpeed=100, void *phontDat=0);
#endif



////////////////////////////////////////////////////////////////////////
//  以下、非同期タイプの関数
////////////////////////////////////////////////////////////////////////
typedef void		*H_AQTKDA;	// 音声合成エンジンのハンドル 同期タイプの関数で使用する

/////////////////////////////////////////////
//!	音声合成エンジンのインスタンスを生成（非同期タイプ）
//! @return	音声合成エンジンのハンドルを返す
DllExport	H_AQTKDA __stdcall AquesTalk2Da_Create();

/////////////////////////////////////////////
//!	音声合成エンジンのインスタンスを解放（非同期タイプ）
//! @param	hMe[in]		音声合成エンジンのハンドル AquesTalk2Da_Create()で生成
DllExport	void __stdcall AquesTalk2Da_Release(H_AQTKDA hMe);

/////////////////////////////////////////////
//!	音声を合成して出力（非同期タイプ）
//!	音声波形生成後に、すぐに戻る
//!	hWndを指定すると再生終了後、msgに指定したメッセージがPostされる。
//!	再生終了前にAquesTalk2Da_Play()を呼び出して、連続的に再生させることも可能。
//!	
//! @param	hMe[in]		音声合成エンジンのハンドル AquesTalk2Da_Create()で生成
//! @param	koe[in]		音声記号列（NULL終端）
//! @param	iSpeed[in]	発話速度 [%] 50-300 の間で指定
//!	@param	phontDat[in]	phontデータの先頭アドレスを指定します。このDLLのデフォルトPhontを用いるときは０を指定します。
//! @param	hWnd[in]	終了メッセージ送出先ウィンドウハンドル
//! @param	msg[in]		終了メッセージ
//! @param	dwUser[in]	任意のユーザパラメータ(メッセージのlParam に設定される）
//!	@return	0:正常終了　それ以外：エラーコード
DllExport	int __stdcall AquesTalk2Da_Play(H_AQTKDA hMe, const char *koe, int iSpeed=100, void *phontDat=0, HWND hWnd=0, unsigned long msg=0, unsigned long dwUser=0);
#ifdef MULTI_STR_CODE
DllExport	int __stdcall AquesTalk2Da_Play_Euc(H_AQTKDA hMe, const char *koe, int iSpeed=100, void *phontDat=0, HWND hWnd=0, unsigned long msg=0, unsigned long dwUser=0);
DllExport	int __stdcall AquesTalk2Da_Play_Utf8(H_AQTKDA hMe, const char *koe, int iSpeed=100, void *phontDat=0, HWND hWnd=0, unsigned long msg=0, unsigned long dwUser=0);
DllExport	int __stdcall AquesTalk2Da_Play_Utf16(H_AQTKDA hMe, const unsigned short *koe, int iSpeed=100, void *phontDat=0, HWND hWnd=0, unsigned long msg=0, unsigned long dwUser=0);
DllExport	int __stdcall AquesTalk2Da_Play_Roman(H_AQTKDA hMe, const char *koe, int iSpeed=100, void *phontDat=0, HWND hWnd=0, unsigned long msg=0, unsigned long dwUser=0);
#endif

/////////////////////////////////////////////
//!	再生の中止 
//! AquesTalk2Da_Play()で再生中に、再生を中断する。
//!	再生中(再生待ちを含む）であり、終了メッセージ送出先が指定されていたなら、
//! 終了メッセージがPostされる。
//! @param	hMe[in]		音声合成エンジンのハンドル AquesTalk2Da_Create()で生成
DllExport	void __stdcall AquesTalk2Da_Stop(H_AQTKDA hMe);

/////////////////////////////////////////////
//!	再生中か否か
//! @param	hMe[in]		音声合成エンジンのハンドル AquesTalk2Da_Create()で生成
//! @return 1:再生中 0:再生中でない
DllExport	int __stdcall AquesTalk2Da_IsPlay(H_AQTKDA hMe);


#ifdef __cplusplus
}
#endif
#endif // !defined(_AQUESTALK2DA_H_)
//  ----------------------------------------------------------------------
// !  Copyright AQUEST Corp. 2006- .  All Rights Reserved.                !
// !  An unpublished and CONFIDENTIAL work.  Reproduction, adaptation, or !
// !  translation without prior written permission is prohibited except   !
// !  as allowed under the copyright laws.                                !
//  ----------------------------------------------------------------------
