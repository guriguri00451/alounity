using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// スマホのセンサーデータを Unity InputSystem に公開するカスタムデバイス。
/// Socket.IO で受け取ったデータを InputSystem.QueueStateEvent で流し込む。
/// </summary>
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
[InputControlLayout(stateType = typeof(SmartphoneDeviceState), displayName = "Smartphone Controller")]
public class SmartphoneDevice : InputDevice
{
    /// <summary>左オールの推進力（0〜1）</summary>
    public AxisControl boatL { get; private set; }
    /// <summary>右オールの推進力（0〜1）</summary>
    public AxisControl boatR { get; private set; }
    /// <summary>釣り人の向き（-1〜1 の方位角）</summary>
    public AxisControl rotate { get; private set; }
    /// <summary>キャストジェスチャーの強度（0〜1）</summary>
    public AxisControl cast { get; private set; }
    /// <summary>シェイクジェスチャーの強度（0〜1）</summary>
    public AxisControl shake { get; private set; }
    /// <summary>リールジェスチャーの強度（0〜1）</summary>
    public AxisControl reel { get; private set; }

    public static SmartphoneDevice Current { get; private set; }

#if UNITY_EDITOR
    // エディタ起動時にレイアウトを登録する（バインディングの解決が正しく行われるようにするため）
    static SmartphoneDevice() => RegisterLayout();
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterLayout()
    {
        InputSystem.RegisterLayout<SmartphoneDevice>();
    }

    /// <summary>コントロールをキャッシュする。InputSystem の内部初期化後に呼ばれる。</summary>
    protected override void FinishSetup()
    {
        base.FinishSetup();
        boatL = GetChildControl<AxisControl>("boatL");
        boatR = GetChildControl<AxisControl>("boatR");
        rotate = GetChildControl<AxisControl>("rotate");
        cast = GetChildControl<AxisControl>("cast");
        shake = GetChildControl<AxisControl>("shake");
        reel = GetChildControl<AxisControl>("reel");
    }

    public override void MakeCurrent()
    {
        base.MakeCurrent();
        Current = this;
    }

    protected override void OnRemoved()
    {
        base.OnRemoved();
        if (Current == this)
            Current = null;
    }
}
