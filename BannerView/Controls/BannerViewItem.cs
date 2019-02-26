using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Shapes;

namespace BannerView.Controls
{
    public class BannerViewItem : FlipViewItem
    {
        private Canvas shadowHost;
        private Rectangle backgroundRect;

        private DropShadow dropShadow;
        internal Compositor Compositor;

        public BannerViewItem()
        {
            this.DefaultStyleKey = typeof(BannerViewItem);
            RegisterPropertyChangedCallback(FlipViewItem.IsSelectedProperty, IsSelectedPropertyChanged);
            this.SizeChanged += (s, a) => UpdateShadow();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            shadowHost = GetTemplateChild("ShadowHost") as Canvas;
            backgroundRect = GetTemplateChild("BackgroundRect") as Rectangle;

            InitComposition();
        }

        private void IsSelectedPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (dropShadow != null)
            {
                dropShadow.BlurRadius = IsSelected ? 8f : 0f;
            }
        }

        private void InitComposition()
        {
                Compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            //if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 4))
            //{
            //    Compositor = Window.Current.Compositor;
            //}
            //else
            //{
            //    Compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            //}

            var visual = Compositor.CreateSpriteVisual();
            var sizebind = Compositor.CreateExpressionAnimation("rect.Size");
            sizebind.SetReferenceParameter("rect", ElementCompositionPreview.GetElementVisual(backgroundRect));
            visual.StartAnimation("Size", sizebind);

            dropShadow = Compositor.CreateDropShadow();
            dropShadow.Color = Colors.Black;
            dropShadow.Opacity = 1f;
            dropShadow.Offset = Vector3.Zero;
            dropShadow.BlurRadius = IsSelected ? 8f : 0f;

            dropShadow.ImplicitAnimations = Compositor.CreateImplicitAnimationCollection();
            var blur_an = Compositor.CreateScalarKeyFrameAnimation();
            blur_an.InsertExpressionKeyFrame(1f, "this.FinalValue");
            blur_an.Duration = TimeSpan.FromSeconds(0.2d);
            blur_an.Target = "BlurRadius";
            dropShadow.ImplicitAnimations["BlurRadius"] = blur_an;

            visual.Shadow = dropShadow;

            ElementCompositionPreview.SetElementChildVisual(shadowHost, visual);
            UpdateShadow();
        }

        private void UpdateShadow()
        {
            if (dropShadow == null) return;
            dropShadow.Mask = backgroundRect.GetAlphaMask();
        }
    }
}
