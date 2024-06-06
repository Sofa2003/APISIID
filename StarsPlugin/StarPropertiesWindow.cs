using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.Command;
using TFlex.Dialogs;
using TFlex.Model;

namespace Stars
{
    public class StarPropertiesControls
    {
        public ControlsWindowForm MainGroup { get; private set; }
        public ControlsWindowForm AdditionalGroup { get; private set; }

        public NumericInputControl InputNumber { get; private set; }
        public NumericInputControl InputR1 { get; private set; }
        public NumericInputControl InputR2 { get; private set; }
        public NumericInputControl InputX { get; private set; }
        public NumericInputControl InputY { get; private set; }
        public NumericInputControl InputThickness { get; private set; }
        public NumericInputControl InputAngle { get; private set; }
        public ToggleControl CheckFill { get; private set; }
        public ButtonControl ButtonColor { get; private set; }

        public StarPropertiesControls()
        {
            // Первая вкладка - "Основные свойства"
            MainGroup = new ControlsWindowForm("propMain");
            MainGroup.Caption = "Основные свойства";

            InputNumber = new NumericInputControl("inputNum");
            InputNumber.Label = "Количесто лучей";
            InputNumber.IsIntegral = true;
            InputNumber.Range = new Range(1, 10);
            InputNumber.HasUpDown = true;
            InputNumber.AllowVariable = true;
            MainGroup.Controls.Add(InputNumber);

            InputX = new NumericInputControl("inputX");
            InputX.Label = "Центр X";
            InputX.AllowVariable = true;
            MainGroup.Controls.Add(InputX);

            InputY = new NumericInputControl("inputY");
            InputY.Label = "Центр Y";
            InputY.AllowVariable = true;
            MainGroup.Controls.Add(InputY);

            InputR1 = new NumericInputControl("inputR1");
            InputR1.Label = "Радиус 1";
            InputR1.AllowVariable = true;
            MainGroup.Controls.Add(InputR1);

            InputR2 = new NumericInputControl("inputR2");
            InputR2.Label = "Радиус 2";
            InputR2.AllowVariable = true;
            MainGroup.Controls.Add(InputR2);

            // Вторая вкладка - "Дополнительные свойства"
            AdditionalGroup = new ControlsWindowForm("propAdditional");
            AdditionalGroup.Caption = "Дополнительные свойства";

            InputThickness = new NumericInputControl("inputThickness");
            InputThickness.AllowVariable = true;
            InputThickness.Label = "Толщина линии";
            AdditionalGroup.Controls.Add(InputThickness);

            InputAngle = new NumericInputControl("inputAngle");
            InputAngle.Label = "Начальный угол";
            InputAngle.AllowVariable = true;
            AdditionalGroup.Controls.Add(InputAngle);

            ButtonColor = new ButtonControl("buttonColor");
            ButtonColor.Label = "Цвет звезды";
            ButtonColor.Text = "Выбрать...";
            AdditionalGroup.Controls.Add(ButtonColor);

            CheckFill = new ToggleControl("checkFill");
            CheckFill.Text = "Заливка";
            AdditionalGroup.Controls.Add(CheckFill);
        }

       
    }


    internal class StarPropertiesWindow : PropertiesWindow
    {
        private StarCommand _command;
        private Document _document;
        private StarPropertiesControls _ui;
        

        public StarPropertiesWindow(StarCommand command, Document document)
        {
            _command = command;
            _document = document;

            _command.ShowCursor += OnShowCommandCursor;
            _command.InputStateChanged += OnInputStateChanged;

            Caption = "Свойства звезды";

            _ui = new StarPropertiesControls();
            _ui.InputNumber.ValueChanged += InputNumber_OnValueChanged;
            _ui.InputNumber.Activated += OnInputControlActivated;
            _ui.InputX.ValueChanged += InputX_OnValueChanged;
            _ui.InputX.Activated += OnInputControlActivated;
            _ui.InputY.ValueChanged += InputY_OnValueChanged;
            _ui.InputY.Activated += OnInputControlActivated;
            _ui.InputR1.ValueChanged += InputR1_OnValueChanged;
            _ui.InputR1.Activated += OnInputControlActivated;
            _ui.InputR2.ValueChanged += InputR2_OnValueChanged;
            _ui.InputR2.Activated += OnInputControlActivated;
            _ui.InputThickness.ValueChanged += InputThickness_OnValueChanged;
            _ui.InputAngle.ValueChanged += InputAngle_OnValueChanged;
            _ui.ButtonColor.Clicked += ButtonColor_OnClick;
            _ui.CheckFill.CheckedChanged += CheckFill_OnCheckedChanged;
            AppendBaseForm(_ui.MainGroup);
            AppendBaseForm(_ui.AdditionalGroup);

            ActivateControl();
        }

        private void ButtonColor_OnClick(object sender, BaseEventArgs e)
        {
            using (var cDialog = new System.Windows.Forms.ColorDialog())
            {
                if (cDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _command.Star.Color = TFlex.Drawing.StandardColors.IndexFromColor(cDialog.Color);
                    _document.Redraw();
                }
            }
        }

        private void CheckFill_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            _command.Star.Fill = (e.Arg == CheckState.Checked);
            _document.Redraw();
        }

        private void ActivateControl()
        {
            NumericInputControl control = null;
            switch (_command.State)
            {
                case StarCommand.InputState.modePoint: control = _ui.InputX; break;
                case StarCommand.InputState.modeR1:   control = _ui.InputR1; break;
                case StarCommand.InputState.modeR2:   control = _ui.InputR2; break;               
            }
            if(control != null)
                using (control.SuppressEvents())
                    control.Activate();
        }

        private void OnInputStateChanged(object sender, EventArgs e)
        {
            ActivateControl();
        }

        private void OnShowCommandCursor(object sender, MouseEventArgs e)
        {
            using (_ui.InputNumber.SuppressEvents())
            using (_ui.InputR1.SuppressEvents())
            using (_ui.InputR2.SuppressEvents())
            using (_ui.InputX.SuppressEvents())
            using (_ui.InputY.SuppressEvents())
            using (_ui.InputAngle.SuppressEvents())
            using (_ui.InputThickness.SuppressEvents())
            {
                _ui.InputNumber.SetValue(_command.Star.Number, _command.Star.VarNumber.Value);
                _ui.InputR1.SetValue(_command.Star.R1, _command.Star.VarR1.Value);
                _ui.InputR2.SetValue(_command.Star.R2, _command.Star.VarR2.Value);
                _ui.InputX.SetValue(_command.Star.X, _command.Star.VarX.Value);
                _ui.InputY.SetValue(_command.Star.Y, _command.Star.VarY.Value);
                _ui.InputThickness.SetValue(_command.Star.Thickness, _command.Star.VarThickness.Value);
                _ui.InputAngle.SetValue(_command.Star.Angle, _command.Star.VarAngle.Value);
            }
        }

        private void OnInputControlActivated(object sender, BaseEventArgs e)
        {
            _command.State = StarCommand.InputState.modeWait;
        }

        private void InputNumber_OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.IsValid)
            {
                _command.Star.Number = (int)e.Value;
                _command.Star.VarAngle.Value = e.Variable;
                _document.Redraw();
            }
        }

        private void InputR1_OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.IsValid)
            {
                _command.Star.R1 = (int)e.Value;
                _command.Star.VarR1.Value = e.Variable;
                _document.Redraw();
            }
        }

        private void InputR2_OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.IsValid)
            {
                _command.Star.R2 = (int)e.Value;
                _command.Star.VarR2.Value = e.Variable;
                _document.Redraw();
            }
        }

        private void InputX_OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.IsValid)
            {
                _command.Star.X = (int)e.Value;
                _command.Star.VarX.Value = e.Variable;
                _document.Redraw();
            }
        }

        private void InputY_OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.IsValid)
            {
                _command.Star.Y = (int)e.Value;
                _command.Star.VarY.Value = e.Variable;
                _document.Redraw();
            }
        }

        private void InputThickness_OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.IsValid)
            {
                _command.Star.Thickness = e.Value;
                _command.Star.VarThickness.Value = e.Variable;
                _document.Redraw();
            }
        }

        private void InputAngle_OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.IsValid)
            {
                _command.Star.Angle = e.Value;
                _command.Star.VarAngle.Value = e.Variable;
                _document.Redraw();
            }
        }
    }
}
