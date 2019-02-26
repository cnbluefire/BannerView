﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace BannerView.Controls
{
    public class BannerView : FlipView
    {
        #region Composition Resources

        internal Compositor Compositor => Window.Current.Compositor;

        private CompositionPropertySet scrollProps;
        private CompositionPropertySet props;
        private Visual thisVisual;
        private Visual panelVisual;
        private ExpressionAnimation perspectiveMatrixExp;
        private ExpressionAnimation panelPerspectiveExp;

        #endregion Composition Resources

        #region Expression Nodes

        /// <summary>
        /// 计算Index节点
        /// </summary>
        private const string IndexNode = "(target.Offset.X / target.Size.X)";

        /// <summary>
        /// 计算SelectedIndex节点
        /// </summary>
        private const string SelectedIndexNode = "(-scroll.Translation.X / target.Size.X)";

        /// <summary>
        /// 计算Index与SelectedIndex之间距离节点
        /// </summary>
        private const string DistanceNode = "((-scroll.Translation.X - target.Offset.X) / target.Size.X)";

        /// <summary>
        /// 计算Index与SelectedIndex之间距离节点，取值在-1到1之间
        /// </summary>
        private static readonly string ClampedDistanceNode = $"Clamp({DistanceNode}, -1f, 1f)";

        #endregion Expression Nodes

        #region Template Parts

        private ScrollViewer scrollingHost;

        #endregion Template Parts

        #region Ctor
        public BannerView()
        {
            this.DefaultStyleKey = typeof(BannerView);
            RegisterPropertyChangedCallback(FlipView.VerticalContentAlignmentProperty, VerticalContentAlignmentPropertyChanged);
        }

        #endregion Ctor

        #region Property Changed Methods
        private void VerticalContentAlignmentPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            UpdateCenterPoint();
        }

        #endregion Property Changed Methods

        #region Overrides
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            scrollingHost = GetTemplateChild("ScrollingHost") as ScrollViewer;
            this.Loaded += (s, a) => InitComposition();
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new BannerViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is BannerViewItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            CreateAnimation((UIElement)element);
            base.PrepareContainerForItemOverride(element, item);
        }

        #endregion Overrides

        #region Composition

        /// <summary>
        /// 初始化Composition资源
        /// </summary>
        private void InitComposition()
        {
            scrollProps = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollingHost);
            thisVisual = ElementCompositionPreview.GetElementVisual(this);
            panelVisual = ElementCompositionPreview.GetElementVisual(ItemsPanelRoot);
            props = Compositor.CreatePropertySet();
            props.InsertMatrix4x4("perspective", Matrix4x4.Identity);
            props.InsertScalar("ItemsSpacing", 0f);
            props.InsertScalar("PerspectiveSpacing", 0f);
            props.InsertScalar("CenterPointY", 0f);

            UpdateCenterPoint();

            for (int i = 0; i < ItemsPanelRoot.Children.Count; i++)
            {
                CreateAnimation(ItemsPanelRoot.Children[i]);
            }

            UpdateScale();
            UpdatePerspective();
            UpdateItemSpacing();
        }

        /// <summary>
        /// 根据对齐方式更新CenterPoint表达式所需的参数
        /// </summary>
        private void UpdateCenterPoint()
        {
            if (props != null)
            {
                ExpressionAnimation ex = null;
                switch (VerticalContentAlignment)
                {
                    case VerticalAlignment.Top:
                        ex = Compositor.CreateExpressionAnimation("0f");
                        break;
                    case VerticalAlignment.Bottom:
                        ex = Compositor.CreateExpressionAnimation("target.Size.Y");
                        break;
                    default:
                        ex = Compositor.CreateExpressionAnimation("target.Size.Y / 2");
                        break;
                }
                ex.SetReferenceParameter("target", ElementCompositionPreview.GetElementVisual(this));
                props.StartAnimation("CenterPointY", ex);
            }
        }

        #region Create Animations

        private void CreateAnimation(UIElement element)
        {
            if (scrollProps == null) return;

            CreateOffsetAnimation(element);
            CreateCenterPointAnimation(element);
            if (IsScaleEnable)
            {
                CreateScaleAnimation(element);
            }
            if (IsPerspectiveEnable)
            {
                CreateRotationAnimation(element);
            }
        }

        private void CreateScaleAnimation(UIElement element)
        {
            var visual = ElementCompositionPreview.GetElementVisual(element);

            var itemScaleExp = Compositor.CreateExpressionAnimation($"1 - abs({ClampedDistanceNode}) * 0.1f");
            itemScaleExp.SetReferenceParameter("scroll", scrollProps);
            itemScaleExp.SetReferenceParameter("target", visual);

            visual.StartAnimation("Scale.X", itemScaleExp);
            visual.StartAnimation("Scale.Y", itemScaleExp);
        }

        private void CreateOffsetAnimation(UIElement element)
        {
            ElementCompositionPreview.SetIsTranslationEnabled(element, true);
            var visual = ElementCompositionPreview.GetElementVisual(element);

            var itemOffsetExp = Compositor.CreateExpressionAnimation($"{ClampedDistanceNode} * (prop.ItemsSpacing + prop.PerspectiveSpacing)");
            itemOffsetExp.SetReferenceParameter("prop", props);
            itemOffsetExp.SetReferenceParameter("scroll", scrollProps);
            itemOffsetExp.SetReferenceParameter("target", visual);

            visual.StartAnimation("Translation.X", itemOffsetExp);
        }

        private void CreateCenterPointAnimation(UIElement element)
        {
            var visual = ElementCompositionPreview.GetElementVisual(element);

            var itemCenterPointExp = Compositor.CreateExpressionAnimation($"Vector3(({ClampedDistanceNode} + 1) * target.Size.X * 0.5, prop.CenterPointY, 0f)");
            itemCenterPointExp.SetReferenceParameter("prop", props);
            itemCenterPointExp.SetReferenceParameter("scroll", scrollProps);
            itemCenterPointExp.SetReferenceParameter("target", visual);

            visual.StartAnimation("CenterPoint", itemCenterPointExp);
        }

        private void CreateRotationAnimation(UIElement element)
        {
            var visual = ElementCompositionPreview.GetElementVisual(element);

            var itemRotationExp = Compositor.CreateExpressionAnimation($"-0.2 * Pi * {ClampedDistanceNode}");
            itemRotationExp.SetReferenceParameter("scroll", scrollProps);
            itemRotationExp.SetReferenceParameter("target", visual);

            visual.RotationAxis = new Vector3(0f, 1f, 0f);

            visual.StartAnimation("RotationAngle", itemRotationExp);

        }

        #endregion Create Animations

        #endregion Composition

        #region Dependency Properties

        public bool IsPerspectiveEnable
        {
            get { return (bool)GetValue(IsPerspectiveEnableProperty); }
            set { SetValue(IsPerspectiveEnableProperty, value); }
        }
        public bool IsScaleEnable
        {
            get { return (bool)GetValue(IsScaleEnableProperty); }
            set { SetValue(IsScaleEnableProperty, value); }
        }
        public double ItemsSpacing
        {
            get { return (double)GetValue(ItemsSpacingProperty); }
            set { SetValue(ItemsSpacingProperty, value); }
        }

        public static readonly DependencyProperty IsPerspectiveEnableProperty =
            DependencyProperty.Register("IsPerspectiveEnable", typeof(bool), typeof(BannerView), new PropertyMetadata(false, (s, a) =>
            {
                if (a.NewValue != a.OldValue)
                {
                    if (s is BannerView sender)
                    {
                        sender.UpdatePerspective();
                    }
                }
            }));

        public static readonly DependencyProperty IsScaleEnableProperty =
            DependencyProperty.Register("IsScaleEnable", typeof(bool), typeof(BannerView), new PropertyMetadata(false, (s, a) =>
            {
                if (a.NewValue != a.OldValue)
                {
                    if (s is BannerView sender)
                    {
                        sender.UpdateScale();
                    }
                }
            }));

        public static readonly DependencyProperty ItemsSpacingProperty =
            DependencyProperty.Register("ItemsSpacing", typeof(double), typeof(BannerView), new PropertyMetadata(0d, (s, a) =>
            {
                if (a.NewValue != a.OldValue)
                {
                    if (s is BannerView sender)
                    {
                        sender.UpdateItemSpacing();
                    }
                }
            }));

        #endregion Dependency Properties

        #region Update Properties

        private void UpdateItemSpacing()
        {
            if (props != null)
            {
                props.InsertScalar("ItemsSpacing", (float)ItemsSpacing);
            }
        }

        private void UpdateScale()
        {
            if (ItemsPanelRoot != null)
            {
                if (IsScaleEnable)
                {
                    foreach (var item in ItemsPanelRoot.Children)
                    {
                        CreateScaleAnimation(item);
                    }
                }
                else
                {
                    foreach (var item in ItemsPanelRoot.Children)
                    {
                        var visual = ElementCompositionPreview.GetElementVisual(item);
                        visual.StopAnimation("Scale.X");
                        visual.StopAnimation("Scale.Y");
                        visual.Scale = Vector3.One;
                    }
                }
            }
        }

        private void UpdatePerspective()
        {
            if (props == null) return;

            if (IsPerspectiveEnable)
            {
                props.InsertScalar("PerspectiveSpacing", 0f);

                if (perspectiveMatrixExp == null)
                {
                    perspectiveMatrixExp = Compositor.CreateExpressionAnimation("Matrix4x4(1.0f, 0.0f, 0.0f, 0.0f," +
                                                                                "0.0f, 1.0f, 0.0f, 0.0f," +
                                                                                "0.0f, 0.0f, 1.0f, -1.0f / target.Size.X," +
                                                                                "0.0f, 0.0f, 0.0f, 1.0f)");
                    perspectiveMatrixExp.SetReferenceParameter("target", thisVisual);
                }


                if (panelPerspectiveExp == null)
                {
                    panelPerspectiveExp = Compositor.CreateExpressionAnimation("Matrix4x4.CreateFromTranslation(Vector3((scroll.Translation.X - target.Size.X / 2), -target.Size.Y / 2 , 0f)) * " +
                                                                                "prop.perspective * " +
                                                                                "Matrix4x4.CreateFromTranslation(Vector3((target.Size.X / 2 - scroll.Translation.X), target.Size.Y / 2, 0f))");
                    panelPerspectiveExp.SetReferenceParameter("scroll", scrollProps);
                    panelPerspectiveExp.SetReferenceParameter("panel", panelVisual);
                    panelPerspectiveExp.SetReferenceParameter("target", thisVisual);
                    panelPerspectiveExp.SetReferenceParameter("prop", props);

                }

                props.StartAnimation("perspective", perspectiveMatrixExp);
                panelVisual.StartAnimation("TransformMatrix", panelPerspectiveExp);

                foreach (var item in ItemsPanelRoot.Children)
                {
                    CreateRotationAnimation(item);
                }
            }
            else
            {
                props.InsertScalar("PerspectiveSpacing", 20f);

                panelVisual.StopAnimation("TransformMatrix");
                props.StopAnimation("perspective");

                foreach (var item in ItemsPanelRoot.Children)
                {
                    var visual = ElementCompositionPreview.GetElementVisual(item);
                    visual.StopAnimation("RotationAngle");
                    visual.RotationAngle = 0f;
                }
            }

            #endregion Update Properties

        }

    }
}