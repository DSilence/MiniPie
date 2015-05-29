using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MiniPie.Controls {
    public sealed class ImageButton : Button {


        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof (ImageSource), typeof (ImageButton), new PropertyMetadata(default(ImageSource)));

        public static readonly DependencyProperty ImageSourceNormalProperty =
            DependencyProperty.Register("ImageSourceNormal", typeof (ImageSource), typeof (ImageButton), new PropertyMetadata(default(ImageSource)));

        public static readonly DependencyProperty ImageSourcePressedProperty =
            DependencyProperty.Register("ImageSourcePressed", typeof (ImageSource), typeof (ImageButton), new PropertyMetadata(default(ImageSource)));
        public static readonly DependencyProperty ImageSourceMouseOverProperty =
            DependencyProperty.Register("ImageSourceMouseOver", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(default(ImageSource)));

        public ImageButton()
        {
            this.FocusVisualStyle = null;
        }

        public ImageSource ImageSourcePressed 
        {
            get { return (ImageSource) GetValue(ImageSourcePressedProperty); }
            set { SetValue(ImageSourcePressedProperty, value); }
        }

        public ImageSource ImageSourceNormal 
        {
            get { return (ImageSource) GetValue(ImageSourceNormalProperty); }
            set { SetValue(ImageSourceNormalProperty, value); }
        }

        public ImageSource ImageSourceMouseOver
        {
            get { return (ImageSource) GetValue(ImageSourceMouseOverProperty); }
            set { SetValue(ImageSourceMouseOverProperty, value);}
        }

        public ImageSource ImageSource {
            get { return (ImageSource) GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            base.OnMouseDown(e);
            ImageSource = ImageSourcePressed;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            ImageSource = IsMouseOver ?
                ImageSourceMouseOver : ImageSourceNormal;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            ImageSource = ImageSourceMouseOver;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            ImageSource = ImageSourceNormal;
        }
    }
}
