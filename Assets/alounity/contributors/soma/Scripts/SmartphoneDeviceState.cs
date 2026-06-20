using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

/// <summary>
/// SmartphoneDevice の入力状態を表すストラクト。
/// Socket.IO 経由で受け取ったセンサーデータをフィールドとして保持する。
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 24)]
public struct SmartphoneDeviceState : IInputStateTypeInfo
{
    public static FourCC Format => new FourCC('S', 'M', 'P', 'H');
    public FourCC format => Format;

    /// <summary>左オール加速度の大きさ（0〜1 に正規化）</summary>
    [InputControl(name = "boatL", layout = "Axis", format = "FLT")]
    [FieldOffset(0)] public float boatL;

    /// <summary>右オール加速度の大きさ（0〜1 に正規化）</summary>
    [InputControl(name = "boatR", layout = "Axis", format = "FLT")]
    [FieldOffset(4)] public float boatR;

    /// <summary>釣り人の向き（-1〜1 に正規化した方位角）</summary>
    [InputControl(name = "rotate", layout = "Axis", format = "FLT")]
    [FieldOffset(8)] public float rotate;

    /// <summary>キャストジェスチャーの強度（0〜1。前方への振り、rotation.beta が負のとき増加）</summary>
    [InputControl(name = "cast", layout = "Axis", format = "FLT")]
    [FieldOffset(12)] public float cast;

    /// <summary>シェイクジェスチャーの強度（0〜1。0.5 を超えると Button として発火）</summary>
    [InputControl(name = "shake", layout = "Axis", format = "FLT")]
    [FieldOffset(16)] public float shake;

    /// <summary>リールジェスチャーの強度（0〜1。手前への引き、rotation.beta が正のとき増加）</summary>
    [InputControl(name = "reel", layout = "Axis", format = "FLT")]
    [FieldOffset(20)] public float reel;
}
