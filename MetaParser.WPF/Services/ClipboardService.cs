﻿using System;
using System.Windows;
using WK.Libraries.SharpClipboardNS;

namespace MetaParser.WPF.Services;

public sealed class ClipboardService
{
    private readonly SharpClipboard clipboard = new();

    public event EventHandler<SharpClipboard.ClipboardChangedEventArgs> ClipboardChanged;

    public ClipboardService()
    {
        clipboard.MonitorClipboard = true;
        clipboard.ClipboardChanged += Clipboard_ClipboardChanged;
    }

    public void SetData(string format, object data) => Clipboard.SetData(format, data);

    public bool ContainsData(string format) => Clipboard.ContainsData(format);

    private void Clipboard_ClipboardChanged(object sender, SharpClipboard.ClipboardChangedEventArgs e)
    {
        OnClipboardChanged(e);
    }

    private void OnClipboardChanged(SharpClipboard.ClipboardChangedEventArgs e)
    {
        ClipboardChanged?.Invoke(this, e);
    }
}
