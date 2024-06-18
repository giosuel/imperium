using System;
using Imperium.Types;
using Imperium.Util.Binding;
using UnityEngine;

namespace Imperium.Interface;

public abstract class ImpWidget : MonoBehaviour
{
    protected IBinding<ImpTheme> theme;
    protected Action onOpen;
    protected Action onClose;

    protected ICloseable parent;

    internal void Init(
        ICloseable parentInterface,
        IBinding<ImpTheme> themeBinding,
        ref Action onOpenAction,
        ref Action onCloseAction
    )
    {
        parent = parentInterface;
        theme = themeBinding;
        theme.onUpdate += OnThemeUpdate;

        onOpen = onOpenAction;
        onClose = onCloseAction;

        onOpenAction += OnOpen;
        onCloseAction += OnClose;

        InitWidget();
    }

    protected virtual void OnOpen()
    {
    }

    protected virtual void OnClose()
    {
    }

    protected abstract void InitWidget();

    protected virtual void OnThemeUpdate(ImpTheme themeUpdate)
    {
    }
}