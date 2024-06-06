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
    /// Класс реализует функциональность команды создания звезды 
    /// </summary>
	class CreateCommand : StarCommand
	{		
		/// <summary>
        /// Конструктор. Регистрируются обработчики событий.
        /// </summary>
        /// <param name="plugin"></param>
		/// <param name="document"></param>
		public CreateCommand(Plugin plugin, Document document)
            : base(plugin)
        {
			_document = document;

            /// Инициализация
			this.Initialize += new InitializeEventHandler(Command_Initialize);
			
        }

        /// <summary>
        /// Идентификатор команды
        /// </summary>
		public override int ID {get{return (int)Commands.Create;}}

        /// <summary>
        /// Инициализация команды:
        /// создать звезду со свойствами по умолчанию
        /// обновить automenu и привязать окно свойств
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void Command_Initialize(object sender, InitializeEventArgs e)
        {
            //настройка панели селектора
            SetSelectorPanel();
            //конструируем создаваемый объект (ProxyObject2D)
            Star = new StarObject();
            //устанавливаем начальное состояние
            State = InputState.modePoint;
            //создаём кнопки автоменю
            UpdateAutomenu();
            //создаём окно свойств (вызываем метод базового класса) и задаём обработчик нажатий на кнопки этого окна
            CreatePropertiesWindow();
            this.PropertiesWindow.HeaderButtonPressed += new PropetiesWindowHeaderButtonPressedEventHandler(propertiesWindow_HeaderButtonPressed);
        }

        /// <summary>
        /// Настройка панель селектора 
        /// </summary>
        private void SetSelectorPanel()
        {
            //приостановить на время настройки панели селектора ее отрисовку 
            this.SuspendSelectionFilterNotifications();

            this.ShowSelectionFilterButton(ObjectType.Node, true);
            this.SetSelectionFilterButtonState(ObjectType.Node, true, false);
            this.SetSelectionFilterButtonToolTip(ObjectType.Node, "Использовать привязку к узлам");

            this.ShowSelectionFilterButton(ObjectType.Outline, true);
            this.SetSelectionFilterButtonState(ObjectType.Outline, true, false);
            this.SetSelectionFilterButtonToolTip(ObjectType.Outline, "Использовать привязку к линиям изображения");

            this.ShowSelectionFilterButton(ObjectType.Construction, true);
            this.SetSelectionFilterButtonState(ObjectType.Construction, true, false);
            this.SetSelectionFilterButtonToolTip(ObjectType.Construction, "Использовать привязку к линиям построения");

            //возобновить отрисовку панели
            this.ResumeSelectionFilterNotifications();
        }

        /// <summary>
        /// Создание новой звезды в документе
        /// </summary>
        /// <param name="document"></param>
		protected override void CreateNewObject(Document document)
		{
            //Обязательно открыть блок действий по изменению модели
			document.BeginChanges("Создать звезду");
			
            //Создаём новый внешний объект на основе звезды. Он копирует объект 
			//(создаёт новый ссылочный объект и копирует свойства старого по значению)
			ExternalObject obj = new ExternalObject(Star, document, Owner);
			StarObject proxyStar = obj.VolatileObject as StarObject;
			proxyStar.ApplyReferences(Star);

            //цвет ExternalObject-а изначально - это цвет его ProxyObject2D (A.K.A. внутреннего объекта, VolatileObject)
            obj.Color = Star.Color;

			Star.RevokeReferences();

            //Обязательно закрыть блок действий
			document.EndChanges();
            
            //И обновить список звёзд в окне
            StarsPlugin.UpdateFloatingWindow(document);
		}

        /// <summary>
        /// Обработка верхних кнопок в диалоге
        /// </summary>
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
			//создаём новую звезду, она будет иметь пар-ры предыдущей, т.к. объект Star сохранился
			CreateNewObject(document);

			//переходим в начальное состояние
			State = InputState.modePoint;
			this.PropertiesWindow.EnableHeaderButton(PropertiesWindowHeaderButton.OK, false);
		}
	}
}
