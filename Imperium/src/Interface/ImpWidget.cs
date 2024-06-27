#region

using System;
using Imperium.Types;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Interface;

public abstract class ImpWidget : MonoBehaviour
{
    protected IBinding<ImpTheme> theme;
    protected event Action onOpen;
    protected event Action onClose;
    protected ImpTooltip tooltip { get; private set; }

    internal void Init(IBinding<ImpTheme> themeBinding, ImpTooltip tooltipReference)
    {
        theme = themeBinding;
        tooltip = tooltipReference;
        theme.onUpdate += OnThemeUpdate;

        InitWidget();
    }

    internal void Init(
        IBinding<ImpTheme> themeBinding,
        ImpTooltip tooltipReference,
        ref Action onOpenAction,
        ref Action onCloseAction
    )
    {
        onOpenAction += () => onOpen?.Invoke();
        onCloseAction += () => onClose?.Invoke();

        onOpenAction += OnOpen;
        onCloseAction += OnClose;

        Init(themeBinding, tooltipReference);
    }

    protected virtual void OnOpen()
    {
    }

    protected virtual void OnClose()
    {
    }

    protected virtual void InitWidget()
    {
    }

    protected virtual void OnThemeUpdate(ImpTheme themeUpdate)
    {
    }
}