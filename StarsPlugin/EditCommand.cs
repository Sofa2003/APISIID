using System;
using System.Runtime.InteropServices;
using TFlex;
using TFlex.Model;
using TFlex.Model.Model2D;
using TFlex.Drawing;
using TFlex.Command;

namespace Stars
{
	/// <summary>
	/// Класс реализует функциональность команды редактирования звезды
	/// </summary>
    class EditCommand : StarCommand
    {
		/// <summary>
		/// Конструктор. Регистрируются обработчики событий
		/// </summary>
		/// <param name="plugin"></param>
		/// <param name="owner"></param>
		/// <param name="doc"></param>
        public EditCommand(Plugin plugin, ExternalObject owner, Document doc) : base(plugin)
        {
			_document = doc;
			Star = new StarObject();

			//Инициализация
            this.Initialize += new InitializeEventHandler(Command_Initialize);

            //конструируем создаваемый объект (ProxyObject2D) и заносим его
			//(копию внутреннего объекта) в Star (рабочий объект)
            StarObject tmpStar = owner.ConstObject as StarObject;
			//Привязки к переменным сохранятся, но автономно, не через механизм связи с внешним объектом
			Star.Assign(tmpStar);

			//запоминаем владельца
            _owner = owner;
        }

        /// <summary>
		/// Для внутреннего использования.
        /// </summary>
        private ExternalObject _owner;

        /// <summary>
		/// Для внутреннего использования.
		/// </summary>
		/// <remarks>
		/// Инициализация команды:
		/// создать звезду со св-вами по умолчанию
		/// обновить automenu и привязать окно свойств
		/// </remarks>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void Command_Initialize(object sender, InitializeEventArgs e)
        {
            _owner.GetAttributes().SetIntAttribute("Hidden", 1, false);
			_document.Regenerate(new RegenerateOptions());

            //устанавливаем начальное состояние
            State = InputState.modePoint;
            //создаём кнопки автоменю
            UpdateAutomenu();
            //создаём окно свойств (вызываем метод базового класса) и задаём обработчик нажатий на кнопки этого окна
            CreatePropertiesWindow();
            this.PropertiesWindow.HeaderButtonPressed += new PropetiesWindowHeaderButtonPressedEventHandler(propertiesWindow_HeaderButtonPressed);
        }

		/// <summary>
		///
		/// </summary>
		/// /// <remarks>
		/// Создание новой звезды в документе.
		/// </remarks>
		/// <param name="document"></param>
		protected override void CreateNewObject(Document document)
		{
            //Обязательно открыть блок действий по изменению модели.
            _owner.GetAttributes().DeleteAttribute("Hidden", false);
            document.BeginChanges("Изменить звезду");

            //Меняем ProxyObject2D внутри нашего внешнего объекта:
            //1) берём указатель (ссылку) на звезду внутри ExternalObject
            StarObject proxyStar = _owner.VolatileObject as StarObject;
            //2) по значению меняем её св-ва, а также передаём ссылки на переменные Holder-ам объекта proxyStar
            proxyStar.Assign(Star);
			proxyStar.ApplyReferences(Star);

            //цвет ExternalObject-а изначально - это цвет его ProxyObject2D (A.K.A. внутреннего объекта, VolatileObject)
            _owner.Color = Star.Color;

            //Обязательно закрыть блок действий
			document.EndChanges();
            _owner.GetAttributes().SetIntAttribute("Hidden", 1, false);

            //И обновить список звёзд в окне
            StarsPlugin.UpdateFloatingWindow(document);
		}

		/// <summary>
		/// Для внутреннего использования.
		/// </summary>
		/// <remarks>
		/// Выход из команды; вызывается после Terminate.
		/// </remarks>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected override void Command_Exit(object sender, ExitEventArgs e)
		{
			//перестаём прятать ExternalObject
			_owner.GetAttributes().DeleteAttribute("Hidden", false);
			_owner.MarkChanged();
			e.Document.Regenerate(new RegenerateOptions());
		}

		/// <summary>
		/// Для внутреннего использования.
		/// </summary>
		/// <remarks>
		/// Обработка верхних кнопок в диалоге.
		/// </remarks>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void propertiesWindow_HeaderButtonPressed(object sender, PropetiesWindowHeaderButtonPressedEventArgs e)
        {
			switch (e.Button)
			{
				case PropertiesWindowHeaderButton.Cancel:
					Terminate();
					break;

				case PropertiesWindowHeaderButton.OK:
					StarApproved(_document);

					break;
			}

        }

		/// <summary>
		/// Перегружаем обработчик нажатия галочки
		/// </summary>
		/// <param name="document"></param>
		protected override void StarApproved(Document document)
		{
			//создаём новую звезду
			CreateNewObject(document);

			Terminate();

			//снова отключаем кнопку подтверждения
			this.PropertiesWindow.EnableHeaderButton(PropertiesWindowHeaderButton.OK, false);
		}
    }
}
