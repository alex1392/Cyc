using Microsoft.Xaml.Behaviors;

using System.Collections.Generic;
using System.Windows;

using TriggerBase = Microsoft.Xaml.Behaviors.TriggerBase;

namespace Cyc.Mvvm.Commands {
    /// <summary>
    /// <see cref="FrameworkTemplate"/> for InteractivityElements instance
    /// <remarks>Subclassed for forward compatibility, perhaps one day <see cref="FrameworkTemplate"/> </remarks>
    /// <remarks>will not be partially internal</remarks>
    /// </summary>
    public class InteractivityTemplate : DataTemplate {

    }

    /// <summary>
    /// Holder for interactivity entries
    /// </summary>
    public class InteractivityItems : FrameworkElement {
        private List<Behavior> _behaviors;
        private List<TriggerBase> _triggers;

        /// <summary>
        /// Storage for triggers
        /// </summary>
        public new List<TriggerBase> Triggers {
            get {
                if (_triggers == null) {
                    _triggers = new List<TriggerBase>();
                }

                return _triggers;
            }
        }

        /// <summary>
        /// Storage for Behaviors
        /// </summary>
        public List<Behavior> Behaviors {
            get {
                if (_behaviors == null) {
                    _behaviors = new List<Behavior>();
                }

                return _behaviors;
            }
        }

        #region Template attached property

        public static InteractivityTemplate GetTemplate(DependencyObject obj)
        {
            return (InteractivityTemplate)obj.GetValue(TemplateProperty);
        }

        public static void SetTemplate(DependencyObject obj, InteractivityTemplate value)
        {
            obj.SetValue(TemplateProperty, value);
        }

        public static readonly DependencyProperty TemplateProperty =
            DependencyProperty.RegisterAttached("Template",
            typeof(InteractivityTemplate),
            typeof(InteractivityItems),
            new PropertyMetadata(default(InteractivityTemplate), OnTemplateChanged));

        private static void OnTemplateChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var dt = (InteractivityTemplate)e.NewValue;
#if (!SILVERLIGHT)
            dt.Seal();
#endif
            var ih = (InteractivityItems)dt.LoadContent();
            var bc = Interaction.GetBehaviors(d);
            var tc = Interaction.GetTriggers(d);

            foreach (var behavior in ih.Behaviors) {
                bc.Add(behavior);
            }

            foreach (var trigger in ih.Triggers) {
                tc.Add(trigger);
            }
        }

        #endregion
    }
}