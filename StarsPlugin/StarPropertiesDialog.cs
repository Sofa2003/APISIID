using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.Dialogs;
namespace Stars
{
    class StarPropertiesDialog : ModalDialog
    {
        public StarObject Star { get; protected set; }
        public StarPropertiesControls _ui;

        public StarPropertiesDialog(StarObject star)
            : base("Properties")
        {
            Star = star;
            _ui = new StarPropertiesControls();

            _ui.InputNumber.SetValue(Star.Number, Star.VarNumber.Value);
            _ui.InputR1.SetValue(Star.R1, Star.VarR1.Value);
            _ui.InputR2.SetValue(Star.R2, Star.VarR2.Value);
            _ui.InputX.SetValue(Star.X, Star.VarX.Value);
            _ui.InputY.SetValue(Star.Y, Star.VarY.Value);
            _ui.InputThickness.SetValue(Star.Thickness, Star.VarThickness.Value);
            _ui.InputAngle.SetValue(Star.Angle, Star.VarAngle.Value);
            _ui.CheckFill.IsChecked = Star.Fill;
            _ui.ButtonColor.Clicked += ButtonColor_Clicked;

            var page = new ModalDialogPage("General");
            page.Forms.Add(_ui.MainGroup);
            page.Forms.Add(_ui.AdditionalGroup);
            Pages.Add(page);
        }

        private void ButtonColor_Clicked(object sender, BaseEventArgs e)
        {
            using (var cDialog = new System.Windows.Forms.ColorDialog())
            {
                if (cDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Star.Color = TFlex.Drawing.StandardColors.IndexFromColor(cDialog.Color);
                }
            }
        }

        public override bool ShowDialog()
        {
            bool ret = base.ShowDialog();
            Star.Number = (int)_ui.InputNumber.Value;
            Star.VarNumber.Value = _ui.InputNumber.Variable;
            Star.R1 = _ui.InputR1.Value;
            Star.VarR1.Value = _ui.InputR1.Variable;
            Star.R2 = _ui.InputR2.Value;
            Star.VarR2.Value = _ui.InputR2.Variable;
            Star.X = _ui.InputX.Value;
            Star.VarX.Value = _ui.InputX.Variable;
            Star.Y = _ui.InputY.Value;
            Star.VarY.Value = _ui.InputY.Variable;
            Star.Thickness = _ui.InputThickness.Value;
            Star.VarThickness.Value = _ui.InputThickness.Variable;
            Star.Angle = _ui.InputAngle.Value;
            Star.VarAngle.Value = _ui.InputAngle.Variable;
            Star.Fill = _ui.CheckFill.IsChecked.Value;
            return ret;
        }

    }
}
