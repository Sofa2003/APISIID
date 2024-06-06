using System;
using System.IO;
using System.Drawing;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Windows.Forms;

using TFlex;
using TFlex.Model;
using TFlex.Model.Model2D;
using TFlex.Drawing;

//---------------------------------------------------------------------------------------------------------
//Данный файл реализует функциональность объекта "звезда" и коллекции "звёзд"
//Должен быть порождён от класса ProxyObject2D
//Некоторые методы нужно обязательно перекрыть, чтобы работало сохранение в файл,
//отмена-повтор, и т.д.
//Специальные тэги необходимо для использования встроенного в платформу .NET механизма сериализации в .xml

namespace Stars
{
	//------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Класс является псевдонимом для Generic класса списка звёзд
	/// </summary>
    [XmlRoot(ElementName = "StarObjectCollection", Namespace = "", IsNullable = false)]
    public class StarObjectCollection : List<StarObject>
    {
        public StarObjectCollection()
        {
        }
    }

	//------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Класс "звезды"
    /// </summary>
    [XmlRoot(ElementName = "StarObject", Namespace = "Stars", IsNullable = false)]
	public class StarObject : ProxyObject2D
	{
		/// <summary>
		/// Вспомогательная функция рассчёта гипотенузы
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
        static double CalculateHypotenuse(double x, double y) { return Math.Sqrt(x * x + y * y);  }

		//Параметры звезды------------------------------------------------------------------------------------

        //Ссылки на другие объекты модели (например, узлы или другие звёзды) ни в
        //коем случае нельзя хранить здесь же, как объекты класса. Для этого существует
        //специальный механизм (см класс FixingNode)

		/// <summary>
		/// Перечисление ссылок
		/// </summary>
		enum References
		{
			FixingNode = 0, Number, X, Y, R1, R2, Angle, Thickness, R1Node, R2Node
		};

		//Ссылочные параметры--------------------------------------------------------------------------------

		/// <summary>
		/// Ссылку на исходный узел храним при помощи механизма ссылок родительского объекта
		/// </summary>
		[XmlIgnore]
		public ReferenceHolder<Node> FixingNode;

		[XmlIgnore]
		public ReferenceHolder<Node> R1Node;

		[XmlIgnore]
		public ReferenceHolder<Node> R2Node;

		/// <summary>
		/// Ссылку на переменную абсциссы храним при помощи механизма ссылок родительского объекта
		/// </summary>
		[XmlIgnore]
		internal ReferenceHolder<Variable> VarX;

		/// <summary>
		/// Ссылку на переменную ординаты храним при помощи механизма ссылок родительского объекта
		/// </summary>
		[XmlIgnore]
		internal ReferenceHolder<Variable> VarY;

		/// <summary>
		/// Ссылку на переменную 1го радиуса храним при помощи механизма ссылок родительского объекта
		/// </summary>
		[XmlIgnore]
		internal ReferenceHolder<Variable> VarR1;

		/// <summary>
		/// Ссылку на переменную 2го радиуса храним при помощи механизма ссылок родительского объекта
		/// </summary>
		[XmlIgnore]
		internal ReferenceHolder<Variable> VarR2;

		/// <summary>
		/// Ссылку на переменную числа лучей храним при помощи механизма ссылок родительского объекта
		/// </summary>
		[XmlIgnore]
		internal ReferenceHolder<Variable> VarNumber;

		/// <summary>
		/// Ссылку на переменную начального угла поворота храним при помощи механизма ссылок родительского объекта
		/// </summary>
		[XmlIgnore]
		internal ReferenceHolder<Variable> VarAngle;

		/// <summary>
		/// Ссылку на переменную толщины линий храним при помощи механизма ссылок родительского объекта
		/// </summary>
		[XmlIgnore]
		internal ReferenceHolder<Variable> VarThickness;

		//Параметры-значения-----------------------------------------------------------------------------------

		/// <summary>
		/// горизонтальная координата
		/// </summary>
        [XmlElement]
        public double X
		{ get; set; }

		/// <summary>
		/// вертикальная координата
		/// </summary>
        [XmlElement]
        public double Y { get; set; }

		/// <summary>
		/// первый радиус звезды
		/// </summary>
        [XmlElement]
        public double R1 { get; set; }

		/// <summary>
		/// второй радиус звезды
		/// </summary>
		[XmlElement]
        public double R2 { get; set; }

		/// <summary>
		/// Начальный угол
		/// </summary>
        [XmlElement]
        public double Angle { get; set; }

		/// <summary>
		/// Толщина линий
		/// </summary>
        [XmlElement]
        public double Thickness { get; set; }

		/// <summary>
		/// Количество лучей
		/// </summary>
        [XmlElement]
        public int Number { get; set; }

		/// <summary>
		/// Нужно ли заливать
		/// </summary>
        [XmlElement]
        public bool Fill { get; set; }

		/// <summary>
		/// цвет границы и заливки
		/// </summary>
        [XmlElement]
        public int Color { get; set; }

		//------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Конструктор
		/// </summary>
		public StarObject()
		{
			X = 0;
			Y = 0;
			R1 = 10;
			R2 = 20;
			Number = 5;
			Angle = 0;
			Thickness = 1;
			Fill = false;
            Color = (int)TFlex.Drawing.Color.Black;

			FixingNode =	new ReferenceHolder<Node>((int)References.FixingNode, this);
			R1Node = new ReferenceHolder<Node>((int)References.R1Node, this);
			R2Node = new ReferenceHolder<Node>((int)References.R2Node, this);
			VarX =			new ReferenceHolder<Variable>((int)References.X, this);
			VarY =			new ReferenceHolder<Variable>((int)References.Y, this);
			VarR1 =			new ReferenceHolder<Variable>((int)References.R1, this);
			VarR2 =			new ReferenceHolder<Variable>((int)References.R2, this);
			VarNumber =		new ReferenceHolder<Variable>((int)References.Number, this);
			VarAngle =		new ReferenceHolder<Variable>((int)References.Angle, this);
			VarThickness =	new ReferenceHolder<Variable>((int)References.Thickness, this);
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="OwnerHandle"></param>
        public StarObject(IntPtr OwnerHandle)
            : base(OwnerHandle)
		{
			X = 0;
			Y = 0;
			R1 = 10;
			R2 = 20;
			Number = 5;
			Angle = 0;
			Thickness = 1;
			Fill = false;
            Color = (int)TFlex.Drawing.Color.Black;

			FixingNode = new ReferenceHolder<Node>((int)References.FixingNode, this);
			R1Node = new ReferenceHolder<Node>((int)References.R1Node, this);
			R2Node = new ReferenceHolder<Node>((int)References.R2Node, this);
			VarX = new ReferenceHolder<Variable>((int)References.X, this);
			VarY = new ReferenceHolder<Variable>((int)References.Y, this);
			VarR1 = new ReferenceHolder<Variable>((int)References.R1, this);
			VarR2 = new ReferenceHolder<Variable>((int)References.R2, this);
			VarNumber = new ReferenceHolder<Variable>((int)References.Number, this);
			VarAngle = new ReferenceHolder<Variable>((int)References.Angle, this);
			VarThickness = new ReferenceHolder<Variable>((int)References.Thickness, this);
		}

		/// <summary>
		/// Выполняет сравнение объекта с исходным. Нужно перекрыть обязательно
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		protected override bool Equals(ProxyObject obj)
		{
			StarObject other = obj as StarObject;

			if (other == null)
				return false;

            return X == other.X &&
                Y == other.Y &&
                R1 == other.R1 &&
                R2 == other.R2 &&
                Number == other.Number &&
                Angle == other.Angle &&
                Thickness == other.Thickness &&
                Fill == other.Fill &&
                Color == other.Color;
		}

		/// <summary>
		/// ID типа объекта. По нему приложение создаёт объект например при чтении его из файла
		/// </summary>
		protected override int TypeID {get{return 1;}}

		/// <summary>
		/// Имя типа объекта. Используется в разных местах
		/// </summary>
		protected override string TypeName {get{return "Звезда";}}

		/// <summary>
		/// ID иконки объекта
		/// </summary>
		protected override int IconID {get{return (int)ObjectTypeIcons.StarObject;}}

		/// <summary>
		/// Может быть важно при чтении старых файлов, когда меняется формат файла
		/// </summary>
        protected override int Version {get{return 0;}}

		/// <summary>
		/// Данный метод должен считать данные из потока, который система создаёт в файле
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="Version"></param>
        protected override void Read(Stream stream, int Version)
		{
			BinaryReader reader = new BinaryReader(stream);

			X = reader.ReadDouble();
			Y = reader.ReadDouble();
			R1 = reader.ReadDouble();
			R2 = reader.ReadDouble();
			Number = reader.ReadInt32();
			Angle = reader.ReadDouble();
			Thickness = reader.ReadDouble();
			Fill = reader.ReadBoolean();
            Color = reader.ReadInt32();
		}

		/// <summary>
		/// Этот метод должем записать данные объекта.
        ///Ссылки на другие модели сохраняются сами, при помощи механизма ссылок
		/// </summary>
		/// <param name="stream"></param>
		protected override void Write(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(X);
			writer.Write(Y);
			writer.Write(R1);
			writer.Write(R2);
			writer.Write(Number);
			writer.Write(Angle);
			writer.Write(Thickness);
			writer.Write(Fill);
            writer.Write(Color);
		}

		/// <summary>
		/// Вспомогательная функция, генерирующая границы звезды
		/// </summary>
		/// <returns></returns>
		Polyline CreatePolyline()
		{
			Node node = FixingNode.Value;
			if (node != null)
			{
				X = node.AbsX;
				Y = node.AbsY;
			}

			node = R1Node.Value;
			if (node != null)
				R1 = CalculateHypotenuse(node.AbsX - X, node.AbsY - Y);

			node = R2Node.Value;
			if (node != null)
				R2 = CalculateHypotenuse(node.AbsX - X, node.AbsY - Y);

			Polyline polyline = new Polyline();

			double x = X;
			double y = Y;
			double r1 = R1;
			double r2 = R2;
			double step = Math.PI * 2 / Number;
			for(int i = 0; i < Number; i++)
			{
				double angle = Angle / 180 * Math.PI + step * i;
				double s1 = Math.Sin(angle);
				double c1 = Math.Cos(angle);
				polyline.Add(x + r1 * c1,y + r1 * s1);

				double s2 = Math.Sin(angle + step / 2);
				double c2 = Math.Cos(angle + step / 2);
				polyline.Add(x + r2 * c2,y + r2 * s2);
			}

            if (Number != 0)
			    polyline.Add(polyline.get_X(0), polyline.get_Y(0));

			return polyline;
		}

		/// <summary>
		/// дополнительный уровень абстракции при рисовании звезды
		/// </summary>
		/// <param name="graphics"></param>
		public void DrawCursor(TFlex.Drawing.Graphics graphics)
		{
            Draw(graphics);
        }

		/// <summary>
		/// Нужно обязательно перекрыть, чтобы что-то нарисовать на экране
		/// </summary>
		/// <param name="graphics"></param>
        protected override void Draw(TFlex.Drawing.Graphics graphics)
        {
			//Если параметры звезды заданы переменными, при каждой прорисовке звезды они
			//синхронизируются с ними
			SyncPropertiesWithVariables();

            //режим рисования ON
            graphics.BeginDraw();

            //Создание полилинии
            Polyline pl = CreatePolyline();

            //устанавливаем параметры рисования:
            //если звезда подтверждена, берём для рисования цвет ExternalObject и синхронизируем с цветом StarObject
            if (this.Owner2D != null)
                Color = graphics.Color = this.Owner2D.Color.IntValue;

            //если же ExternalObject не задан, используем цвет StarObject
            else
                graphics.SetColor(Color);

            //Либо заливка полилинии
            if (Fill)
                graphics.Fill(pl);
            //Либо просто её прорисовка
            else
            {
                graphics.SetLineWidth(Thickness);
                graphics.Polyline(pl);
            }

            graphics.SetColor((int)TFlex.Drawing.Color.Black);
            graphics.DrawMarker(MarkerType.FreeNode, new TFlex.Drawing.Point(X, Y));

            //режим рисования OFF
            graphics.EndDraw();
        }

		/// <summary>
		/// Может ли звезда быть перенесена перетаскиванием? Да.
		/// </summary>
		/// <param name="Context"></param>
		protected override void CanTransform(TransformContext Context)
        {
            Context.ReturnOK();
        }

		/// <summary>
		/// Перетащите пожалуйста в новое положение...
		/// </summary>
		/// <param name="Context"></param>
        protected override bool Transform(TransformContext Context)
		{
            if (FixingNode.Value == null)
            {
                double refX, refY;
                refX = X; refY = Y;

                Context.Map.ToWCS(ref refX, ref refY);
                X = refX; Y = refY;

				if (R1Node.Value == null)
					R1 *= Context.Map.Scale;
				if (R2Node.Value == null)
					R2 *= Context.Map.Scale;

                double angle = Angle / 180 * Math.PI;
                Context.Map.ApplyToAngle(ref angle);
                Angle = angle / Math.PI * 180;
                return true;
            }

			return false;
		}

        //Если перекрыть эти 3 метода, то на основе точек звезды могут создаваться узлы модели---------------------------

		/// <summary>
		/// Возвращает число узлов
		/// </summary>
		protected override int GetNodeCount() {return Number * 2;}

		/// <summary>
		/// Возвращает ID заданного узла
		/// </summary>
		protected override int GetNodeID(int Number) {return Number;}

		/// <summary>
		/// Возвращает узел
		/// </summary>
		protected override bool GetNode(int ID, ref double rx, ref double ry)
		{
			double x1 = X;
			double y1 = Y;
			double r1 = R1;

			if (ID % 2 != 0)
				r1 = R2;

			double step = Math.PI * 2 / Number;

			double angle = (ID / 2) * step;
			if (ID % 2 != 0)
				angle += step / 2;

			angle += Angle / 180 * Math.PI;

			double s1 = Math.Sin(angle);
			double c1 = Math.Cos(angle);

			rx = x1 + r1 * c1;
			ry = y1 + r1 * s1;

			return true;
		}

		/// <summary>
		/// Если перекрыть этот метод, то можно добавить свои команды в контекстное меню данного
        ///объекта, или удалить лишние команды
		/// </summary>
		/// <param name="Menu"></param>
		/// <returns></returns>
		protected override bool GetContextMenu(TFlex.Menu Menu)
		{
			Menu.Append((int)ObjectCommands.Fill, "&Заливка", true, Fill, Plugin);

			return true;
		}

		/// <summary>
		/// Здесь можно обработать команду контекстного меню
		/// </summary>
		/// <param name="CommandID"></param>
		/// <param name="View"></param>
		/// <returns></returns>
		protected override bool OnCommand(int CommandID, TFlex.Model.View View)
		{
            switch ((ObjectCommands)CommandID)
            {
                //команда контекстного меню "Заливка" делает следующее (точка входа):
                case ObjectCommands.Fill:
                    //Вот так мы меняем существующий объект модели
                    Document doc = this.Owner2D.Document;
                    doc.BeginChanges("Изменить заливку звезды");
                    StarObject newStar = this.Owner2D.VolatileObject as StarObject;
                    newStar.Fill = !Fill;
                    doc.EndChanges();
                    return true;
            }

			return false;
		}


		//перегрузка команды контекстного меню "Изменить" (точка входа)
		protected override bool Edit(TFlex.Model.View v)
		{
			ExternalObject selectedOwner = this.Owner2D;
			Document doc = selectedOwner.Document;

			EditCommand command = new EditCommand(Plugin, selectedOwner, doc);
			doc.Selection.DeselectAll();
			command.Run(v);

			return true;
		}

        //Если перекрыть следующие 2 метода, то звезду можно измерять в команде "Измерить"-------------------------------

		/// <summary>
		/// Возвращает список свойств
		/// </summary>
		/// <param name="List"></param>
		protected override void GetPropList(PropertyArray List)
		{
			switch (List.Reason)
			{
                case PropertyArray.ReasonType.Measure:
                    List.Add("PERIMETER", "Периметр", PropertyArray.Type.Real);
                    break;
                case PropertyArray.ReasonType.Properties:
                    List.Add("RAYCOUNT", "Количество лучей", PropertyArray.Type.Real);
                    break;
            }
            List.Add("R1", "Радиус 1", PropertyArray.Type.Real);
            List.Add("R2", "Радиус 2", PropertyArray.Type.Real);
        }

		/// <summary>
		/// Считает реальные свойства звезды
		/// </summary>
		/// <param name="Name"></param>
		/// <returns></returns>
		protected override double GetRealProp(string Name)
		{
			switch (Name.ToUpper())
			{
			case "PERIMETER":
				Polyline pl = CreatePolyline();

				double result = 0.0;
				for (int i = 1, sz = pl.get_PointCount(0); i < sz; ++i)
					result += CalculateHypotenuse(pl.get_X(i - 1) - pl.get_X(i), pl.get_Y(i - 1) - pl.get_Y(i));

				return result;

			case "RAYCOUNT":
				return Number;

			case "R1":
				return R1;

			case "R2":
				return R2;

			default:
				return 0.0;
			}
		}

		/// <summary>
		/// Этот метод нужно реализовать чтобы работала команда "Свойства"
		/// </summary>
		/// <returns></returns>
		protected override PropertyChange EditProperties()
		{
			//"this" - ConstObject здесь, поэтому VolatileObject берётся как this.Owner2D.VolatileObject as StarObject

			StarPropertiesDialog dialog = new StarPropertiesDialog(this);
			Document doc = this.Owner2D.Document;

			PropertyChange result = PropertyChange.Nothing;


			if (dialog.ShowDialog())
			{
				doc.BeginChanges("Изменить параметры звезды");

				StarObject newStar = Owner2D.VolatileObject as StarObject;
				newStar.Assign(dialog.Star);
				newStar.ApplyReferences(dialog.Star);

				doc.EndChanges();

				result = PropertyChange.Regenerate;
			}


			return result;
		}

		/// <summary>
		/// эта команда копирует объект звезды по значению,
		/// копия привязана к тем же переменным, что и оригинал
		/// </summary>
		/// <param name="source"></param>
		public void Assign(StarObject source)
		{
			X = source.X;
			Y = source.Y;
			R1 = source.R1;
			R2 = source.R2;
			Number = source.Number;
			Angle = source.Angle;
			Thickness = source.Thickness;
			Fill = source.Fill;
			Color = source.Color;

			VarX.Value = source.VarX.Value;
			VarY.Value = source.VarY.Value;
			VarR1.Value = source.VarR1.Value;
			VarR2.Value = source.VarR2.Value;
			VarNumber.Value = source.VarNumber.Value;
			VarAngle.Value = source.VarAngle.Value;
			VarThickness.Value = source.VarThickness.Value;
		}

		/// <summary>
		/// этот метод синхронизирует параметры звезды, заданные переменными,
		/// с текущими значениями этих переменных
		/// </summary>
		private void SyncPropertiesWithVariables()
		{
			if (VarX.Value != null) X = VarX.Value.RealValue;
			if (VarY.Value != null) Y = VarY.Value.RealValue;
			if (VarR1.Value != null) R1 = VarR1.Value.RealValue;
			if (VarR2.Value != null) R2 = VarR2.Value.RealValue;
			if (VarNumber.Value != null) Number = (int)VarNumber.Value.RealValue;
			if (VarAngle.Value != null) Angle = VarAngle.Value.RealValue;
			if (VarThickness.Value != null) Thickness = VarThickness.Value.RealValue;
		}

		/// <summary>
		/// этот метод уничтожает связь свойств звезды с переменными
		/// </summary>
		internal void RevokeReferences()
		{
			VarX.ReleaseReference();
			VarY.ReleaseReference();
			VarR1.ReleaseReference();
			VarR2.ReleaseReference();
			VarNumber.ReleaseReference();
			VarAngle.ReleaseReference();
			VarThickness.ReleaseReference();
			FixingNode.ReleaseReference();
			R1Node.ReleaseReference();
			R2Node.ReleaseReference();
		}

		/// <summary>
		/// этот метод устанавливает связь свойств звезды со ссылочными параметрами
		/// </summary>
		internal void ApplyReferences(StarObject star)
		{
			VarX.SetReference(star.VarX.Value);
			VarY.SetReference(star.VarY.Value);
			VarR1.SetReference(star.VarR1.Value);
			VarR2.SetReference(star.VarR2.Value);
			VarNumber.SetReference(star.VarNumber.Value);
			VarAngle.SetReference(star.VarAngle.Value);
			VarThickness.SetReference(star.VarThickness.Value);
			FixingNode.SetReference(star.FixingNode.Value);
			R1Node.SetReference(star.R1Node.Value);
			R2Node.SetReference(star.R2Node.Value);
		}

		/// <summary>
		/// Реализация интерфейса ICloneable
		/// </summary>
		/// <returns></returns>
		protected override ProxyObject Clone(IntPtr OwnerHandle)
		{
			StarObject newStar = base.Clone(OwnerHandle) as StarObject;
			newStar.Assign(this);

			return newStar;
		}
	}
}
