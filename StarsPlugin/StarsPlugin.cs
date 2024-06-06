using System;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;

using TFlex;
using TFlex.Model;
using TFlex.Model.Model2D;
using TFlex.Command;
using TFlex.Drawing;

//Данный файл реализует функциональность приложения.
//Регистрируются команды приложения, иконки команд, пункты меню, панель с кнопками,
//плавающее окно, обработчики событий от документов.

namespace Stars
{
    /// <summary>
	/// Для создания приложения необходимо иметь класс, порождённый от PluginFactory
	/// </summary>
	public class Factory : PluginFactory
	{
        /// <summary>
        /// Необходимо также переопределить данный метод для создания объекта
        /// </summary>
		public override Plugin CreateInstance()
		{
			return new StarsPlugin(this);
		}

        /// <summary>
		/// Уникальный GUID приложения. Он должен быть обязательно разным у разных приложений
		/// </summary>
        public override Guid ID
		{
			get
			{
				return new Guid("{32F0C0D7-F516-4b69-837F-F146265981EB}");
			}
		}

        /// <summary>
		/// Имя приложения
		/// </summary>
		public override string Name
		{
			get
			{
				return "T-FLEX Звёзды";
			}
		}
	};

    /// <summary>
	/// Команды приложения в панели и главном меню
	/// </summary>
	enum Commands
	{
		Create = 1, //Команда создания звезды
		ShowWindow, //Показать/спрятать плавающее окно
	};

	/// <summary>
	/// Команды объектов в контекстном меню
	/// </summary>
	enum ObjectCommands
	{
        Fill = Commands.ShowWindow + 1,
	};

	/// <summary>
	/// Команды автоменю
	/// </summary>
	enum AutomenuCommands
	{
		Number,
		Fill = Number + 8,
	};

	/// <summary>
	/// ID иконок объектов, генерируемых приложением
	/// </summary>
	enum ObjectTypeIcons
	{
		StarObject
	};

	/// <summary>
	/// Класс приложения. Регистрируем команды, обработчики событий.
	/// Обрабатываем события, приходящие от различных меню.
	/// </summary>
	public class StarsPlugin : Plugin
	{
		/// <summary>
		/// флаг удаления 1го из цепочки объектов,
		/// нужен для своевременного вызова BeginInvoke()
		/// </summary>
		private bool firstObjectDeleted;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="factory"></param>
		public StarsPlugin(Factory factory) : base(factory)
		{
			firstObjectDeleted = false;
        }

       	/// <summary>
		/// Это плавающее окно, в которое будет выводиться список звёзд в документе
		/// </summary>
        private static PluginFloatingWindow FloatingWindow;

        /// <summary>
		/// Это его клиентская часть, отображающая список звёзд
		/// </summary>
        private static StarsFloatingWindow FloatingWindowClient;

		/// <summary>
		/// Загрузка иконок
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		System.Drawing.Icon LoadIconResource(string name)
		{
			System.IO.Stream stream = GetType().Assembly.
				GetManifestResourceStream("Stars.Resource_Files." + name + ".ico");
			return new System.Drawing.Icon(stream);
		}

		/// <summary>
		///Данная инициализация вызывается в момент загрузки приложения.
        ///В данном приложении здесь ничего делать не нужно. Вся инициализация делается в OnCreateTools
		/// </summary>
		protected override void OnInitialize()
		{
			base.OnInitialize();

            //Регистрируем иконки для кнопок в автоменю
            for (int i = 0; i < 8; ++i)
            {
                // Это для выбора количества лучей через автоменю
                String hint = String.Format(i < 2 ? "{0} луча" : "{0} лучей", i + 3);
                int cmdID = (int)AutomenuCommands.Number + i;
                RegisterAutomenuCommand(cmdID, hint, LoadIconResource("Fill"));
            }

            RegisterAutomenuCommand((int)AutomenuCommands.Fill, "Заливка", LoadIconResource("Fill"));
		}

		/// <summary>
		/// Этот метод вызывается в тот момент, когда следует зарегистрировать команды,
		/// Создать панель, вставить пункты меню
		/// </summary>
		protected override void OnCreateTools()
		{
			//base.OnCreateTools();

            RegisterCommand((int)Commands.Create, "Создание звёзд", LoadIconResource("CreateStars"), LoadIconResource("CreateStars")); // Регистрируем команду создания
            RegisterCommand((int)Commands.ShowWindow, "Показать окно", LoadIconResource("StarsWindow"), LoadIconResource("StarsWindow")); // Регистрируем команду показа окна

			//Регистрируем команды контекстного меню объекта звезды
            RegisterObjectCommand((int)ObjectCommands.Fill, "Заливка", LoadIconResource("Fill"), LoadIconResource("Fill")); // Регистрируем команду заливки для контекстного меню

			//Регистрируем иконку звезды
            RegisterObjectTypeIcon((int)ObjectTypeIcons.StarObject, LoadIconResource("StarObject"));

            //Добавляем пункты и подпункты меню
            TFlex.Menu submenu = new TFlex.Menu();
            submenu.CreatePopup();
			submenu.Append((int)Commands.Create, "&Создать", this);
			submenu.Append((int)Commands.ShowWindow, "&Показать окно", this);
            TFlex.Application.ActiveMainWindow.InsertPluginSubMenu("Звёзды",submenu, MainWindow.InsertMenuPosition.PluginSamples, this);

            //Создаём панель с кнопками "Звёзды"
            int[] cmdIDs = new int[] { (int)Commands.Create,(int)Commands.ShowWindow };
			CreateToolbar("Звёзды", cmdIDs);
			CreateMainBarPanel("Звёзды", cmdIDs, this.ID, true);

			//Добавляем кнопки во вкладку "Приложения" в ленте
			TFlex.RibbonGroup ribbonGroup = TFlex.RibbonBar.ApplicationsTab.AddGroup("Звёзды");
			ribbonGroup.AddButton((int)Commands.Create, this);
			ribbonGroup.AddButton((int)Commands.ShowWindow, this);

            //Создаём плавающее окно со списком звёзд
            FloatingWindow = CreateFloatingWindow(0, "Звёзды");
            FloatingWindow.Caption = "Звёзды"; // Его название
            FloatingWindow.Icon = LoadIconResource("StarObject");//Его иконка
            FloatingWindow.Visible = false; //Пока гасим

			//На случай если плагин был подключён, когда документ был уже создан и открыт (все места для AttachPlugin() пропущены),
			//просто подключаем плагин к текущему документу
			if (TFlex.Application.ActiveDocument != null)
				TFlex.Application.ActiveDocument.AttachPlugin(this);
		}

		/// <summary>
		/// Обработка команд от панели и главного меню
		/// </summary>
		/// <param name="document"></param>
		/// <param name="id"></param>
		protected override void OnCommand(Document document, int id)
		{
			switch ((Commands)id)
			{
				default:
					base.OnCommand(document, id);
					break;
				case Commands.Create://Команда создания звезды
                    FloatingWindow.Visible = true; //Сразу показываем плавающее окно
                    CreateCommand сommand = new CreateCommand(this, document);
                    сommand.Run(document.ActiveView);
					break;
				case Commands.ShowWindow: //Команда показа плавающего окна
                    FloatingWindow.Visible = !FloatingWindow.Visible;
					break;
			}
		}

		/// <summary>
		/// Здесь можно блокировать команды и устанавливать галочки
		/// </summary>
		/// <param name="cmdUI"></param>
		protected override void OnUpdateCommand(CommandUI cmdUI)
		{
			if (cmdUI == null)
				return;

			if (cmdUI.Document == null)
			{
				cmdUI.Enable(false);
				return;
			}
            if (cmdUI.ID == (int)Commands.ShowWindow)
            {
                cmdUI.SetCheck(FloatingWindow.Visible);
            }

			cmdUI.Enable();
		}

		/// <summary>
		/// Эта функция должна создавать объекты приложения, соответствующие типу объекта
        /// Если появляется новый класс объекта, то нужно заводить новый TypeID
		/// </summary>
		/// <param name="Document"></param>
		/// <param name="OwnerHandle"></param>
		/// <param name="TypeID"></param>
		/// <returns></returns>
		protected override ProxyObject CreateObject(Document Document,IntPtr OwnerHandle, int TypeID)
		{
			switch (TypeID)
			{
				case 1:
					return new StarObject(OwnerHandle);
			}
			return null;
        }

		/// <summary>
		/// Подписываемся на обработку событий при создании нового объекта
		/// </summary>
		/// <param name="args"></param>
		protected override void ObjectCreatedEventHandler(ObjectEventArgs args)
		{
			ExternalObject obj;

			if(args.Object.IsKindOf(ObjectType.External))
			{
				obj = args.Object as ExternalObject;
				if(obj.ConstObject.Plugin == this)
				FloatingWindowClient.DelayedUpdate(args.Document, ref firstObjectDeleted);
			}
		}

        /// <summary>
		/// Подписываемся на обработку событий при создании нового документа
		/// </summary>
		/// <param name="args"></param>
        protected override void NewDocumentCreatedEventHandler(DocumentEventArgs args)
        {
            //AttachPlugin нужно вызвать обязательно, иначе от данного документа не будут приходить уведомления о событиях
            args.Document.AttachPlugin(this);
        }

		/// <summary>
		/// Подписываемся на обработку событий после открытия документа
		/// </summary>
		/// <param name="args"></param>
		protected override void DocumentOpenEventHandler(DocumentEventArgs args)
		{
			//Событие приходить после открытия документа
			args.Document.AttachPlugin(this);
			UpdateFloatingWindow(args.Document);
		}

		/// <summary>
		/// Подписываемся на обработку событий при закрытии документа
		/// </summary>
		/// <param name="args"></param>
        protected override void ClosingDocumentEventHandler(DocumentEventArgs args)
        {
            //Данное событие вызывается при закрытии документа
            //Здесь нам следует очистить окно со списком звёзд
            UpdateFloatingWindow(null);
        }

		/// <summary>
		/// Подписываемся на обработку событий при активизации вида документа
		/// </summary>
		/// <param name="args"></param>
        protected override void ViewActivatedEventHandler(ViewEventArgs args)
        {
            //Данное событие вызывается при активизации вида документа
            //Обновим список звёзд
			UpdateFloatingWindow(args.Document);
        }

		/// <summary>
		///Здесь мы должны создать клиентсую часть плавающего окна так как мы
        ///попросили сделать это в вызове CreateFloatingWindow(0, "Звёзды");
        ///Если окон должно быть несколько, то нужно id будет соответствовать первому параметру в вызове
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
        protected override System.Windows.Forms.Control CreateFloatingWindowControl(uint id)
        {
            FloatingWindowClient = new Stars.StarsFloatingWindow(this);
            return FloatingWindowClient;
        }

		/// <summary>
		/// Подписываемся на удаление объекта и обрабатываем его
		/// </summary>
		/// <param name="args"></param>
		protected override void ObjectDeletedEventHandler(ObjectEventArgs args)
		{
			if (!firstObjectDeleted) firstObjectDeleted = true;

			FloatingWindowClient.DelayedUpdate(args.Document, ref firstObjectDeleted);
		}

		/// <summary>
		/// Мгновенное обновление списка звёзд
		/// </summary>
		/// <param name="doc"></param>
        public static void UpdateFloatingWindow(TFlex.Model.Document doc)
        {
            FloatingWindowClient.UpdateContents(doc);
        }

        //Методы для экспорта/импорта
        //-------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Это событие вызывается при показе диалоге экспорта текущего документа
		/// </summary>
		/// <param name="args"></param>
		protected override void ShowingExportDialogEventHandler(ShowingImportExportDialogEventArgs args)
		{
			args.AddFilter("Файлы звёзд (*.str)|*.str|", 1);
		}

		/// <summary>
		/// Это событие вызывается если пользователь выбрал тип экспорта, добавленный в предыдущей функции
		/// </summary>
		/// <param name="args"></param>
		protected override void ExportDialogShownEventHandler(ImportExportDialogShownEventArgs args)
		{
			this.ExportToXml(args.FilePath, args);
		}

		/// <summary>
		/// Это событие вызывается при показе диалоге импорта текущего документа
		/// </summary>
		/// <param name="args"></param>
		protected override void ShowingImportDialogEventHandler(ShowingImportExportDialogEventArgs args)
		{
			args.AddFilter("Файлы звёзд (*.str)|*.str|", 1);
		}

		/// <summary>
		/// Это событие вызывается если пользователь выбрал тип импорта, добавленный в предыдущей функции
		/// </summary>
		/// <param name="args"></param>
		protected override void ImportDialogShownEventHandler(ImportExportDialogShownEventArgs args)
		{
			this.ImportFromXml(args.FilePath, args);
		}

		/// <summary>
		/// Десериализовать экземпляры класса <see cref="StarObjectCollection"> из .xml-файла
		/// </summary>
		/// <param name="_fileName"></param>
		/// <param name="args"></param>
        public void ImportFromXml(string _fileName, ImportExportDialogShownEventArgs args)
        {
              string FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _fileName);

              XmlSerializer[] serializers = XmlSerializer.FromTypes(new Type[] {
                typeof(StarObjectCollection),
              });

              XmlSerializer ser = serializers[0];

              if (!File.Exists(FileName))
                  return;

            StarObjectCollection result = null;
            using (FileStream fs = new FileStream(FileName, System.IO.FileMode.Open, FileAccess.Read))
            {
                result = ser.Deserialize(fs) as StarObjectCollection;
                fs.Close();
            }

            if (result == null || result.Count == 0)
                return;

            Document doc = args.Document;

            //Обязательно открыть блок действий по изменению модели
            doc.BeginChanges("Создать звезду");

            //создадим объекты TFlex (ExternalObject) для каждой из импортированных звёзд
            foreach (StarObject star in result)
            {
                //Создаём новый внешний объект на основе звезды. Он копирует объект
                ExternalObject obj = new ExternalObject(star, doc, this);

                //Так что изменять мы уже должны внутренний (прокси-) объект ExternalObject-а, а не исходный объект Star
                StarObject newStar = obj.VolatileObject as StarObject;
                //Узел привязки (может быть и нулевым)
                newStar.FixingNode.Value = null;
				newStar.R1Node.Value = null;
				newStar.R2Node.Value = null;
                //цвет ExternalObject-а изначально - это цвет его ProxyObject2D (A.K.A. внутреннего объекта, VolatileObject)
                obj.Color = newStar.Color;
            }

            //Обязательно закрыть блок действий
            doc.EndChanges();
            //И обновить список звёзд в окне
            StarsPlugin.UpdateFloatingWindow(doc);

        }

		/// <summary>
		/// Сериализовать экземпляры класса <see cref="StarObject"> в .xml-файл
		/// </summary>
		/// <param name="_fileName"></param>
		/// <param name="args"></param>
        public void ExportToXml(string _fileName, ImportExportDialogShownEventArgs args)
        {
            //preparing XML serializer
            string FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _fileName);

            bool starHasBeenFound = false;
            StarObjectCollection collection = new StarObjectCollection();
            StarObject star = null;
            ExternalObject wrap;
            foreach(object obj in args.Document.Get2DObjects())
            {
                wrap = obj as ExternalObject;
                if (wrap != null)
                    star = wrap.ConstObject as StarObject;
                if (star != null)
                {
                    starHasBeenFound = true;
                    collection.Add(star);
                }
            }

            XmlSerializer[] serializers = XmlSerializer.FromTypes(new Type[] {
                typeof(StarObjectCollection),
            });

            XmlSerializerNamespaces xmlnsEmpty = new XmlSerializerNamespaces();
            xmlnsEmpty.Add(string.Empty, string.Empty);

            XmlSerializer ser = serializers[0];

            if (starHasBeenFound)
            {
                using (FileStream fs = new FileStream(FileName, System.IO.FileMode.Create))
                {

                    ser.Serialize(fs, collection, xmlnsEmpty);
                    fs.Close();
                }
            }
            else
				MessageBox.Show("В текущем документе звёзд не найдено.", "Сообщение", MessageBoxButtons.OK);
        }
    }
}
