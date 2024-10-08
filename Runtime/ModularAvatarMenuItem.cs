﻿#if MA_VRCSDK3_AVATARS

using System;
using System.Linq;
using nadena.dev.modular_avatar.core.menu;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace nadena.dev.modular_avatar.core
{
    public enum SubmenuSource
    {
        MenuAsset,
        Children,
    }

    [AddComponentMenu("Modular Avatar/MA Menu Item")]
    [HelpURL("https://modular-avatar.nadena.dev/docs/reference/menu-item?lang=auto")]
    public class ModularAvatarMenuItem : AvatarTagComponent, MenuSource
    {
        public VRCExpressionsMenu.Control Control;
        public SubmenuSource MenuSource;

        public GameObject menuSource_otherObjectChildren;

        /// <summary>
        /// If this MenuItem references a parameter that does not exist, it is created automatically.
        /// In this case, isSynced controls whether the parameter is network synced.
        /// </summary>
        public bool isSynced = true;

        /// <summary>
        ///     If this MenuItem references a parameter that does not exist, it is created automatically.
        ///     In this case, isSaved controls whether the parameter is saved across avatar changes.
        /// </summary>
        public bool isSaved = true;

        /// <summary>
        ///     If this MenuItem references a parameter that does not exist, it is created automatically.
        ///     In this case, isDefault controls whether the parameter is set, by default, to the value for this
        ///     menu item. If multiple menu items reference the same parameter, the last menu item in hierarchy order
        ///     with isDefault = true is selected.
        /// </summary>
        public bool isDefault;

        private void Reset()
        {
            Control = new VRCExpressionsMenu.Control();
            Control.type = VRCExpressionsMenu.Control.ControlType.Toggle;
            Control.value = 1;
            isSaved = true;
            isSynced = true;
            isDefault = false;

            MenuSource = SubmenuSource.Children;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            RuntimeUtil.InvalidateMenu();

            if (Control == null)
            {
                Control = new VRCExpressionsMenu.Control();
            }
        }

        public override void ResolveReferences()
        {
            // no-op
        }

        public void Visit(NodeContext context)
        {
            if (Control == null)
            {
                Control = new VRCExpressionsMenu.Control();
            }

            var cloned = new VirtualControl(Control);
            cloned.subMenu = null;
            cloned.name = gameObject.name;

            FilterSubParameters(cloned);

            if (cloned.type == VRCExpressionsMenu.Control.ControlType.SubMenu)
            {
                switch (this.MenuSource)
                {
                    case SubmenuSource.MenuAsset:
                        cloned.SubmenuNode = context.NodeFor(this.Control.subMenu);
                        break;
                    case SubmenuSource.Children:
                    {
                        var root = this.menuSource_otherObjectChildren != null
                            ? this.menuSource_otherObjectChildren
                            : this.gameObject;

                        cloned.SubmenuNode = context.NodeFor(new MenuNodesUnder(root));
                        break;
                    }
                }
            }

            context.PushControl(cloned);
        }

        private void FilterSubParameters(VirtualControl control)
        {
            var maxSubParams = 0;
            switch (control.type)
            {
                case VRCExpressionsMenu.Control.ControlType.Toggle:
                case VRCExpressionsMenu.Control.ControlType.Button:
                case VRCExpressionsMenu.Control.ControlType.SubMenu:
                default:
                    maxSubParams = 0;
                    break;
                case VRCExpressionsMenu.Control.ControlType.RadialPuppet:
                    maxSubParams = 1;
                    break;
                case VRCExpressionsMenu.Control.ControlType.TwoAxisPuppet:
                    maxSubParams = 2;
                    break;
                case VRCExpressionsMenu.Control.ControlType.FourAxisPuppet:
                    maxSubParams = 4;
                    break;
            }

            if (control.subParameters.Length > maxSubParams)
                control.subParameters = control.subParameters.Take(maxSubParams).ToArray();
        }
    }
}

#endif