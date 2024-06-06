using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using TFlex.Model;
using TFlex.Drawing;
using TFlex.Model.Model2D;

namespace Stars
{
	/// <summary>
	/// Класс, являющийся обёрткой вокруг UserControl-а, отображающего список звёзд в текущем документе
	/// Логика UserControl-а.
	/// </summary>
    public partial class StarsFloatingWindow : UserControl
    {
		public delegate void DelegatedUpdate(TFlex.Model.Document doc);
		private delegate void DelegatedShow(object sender, EventArgs e);

		private Document currentDocument;
		private StarsPlugin _plugin;

		/// <summary>
		/// возвращает максимум
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		private static double max(double a, double b)
		{
			return (a > b ? a : b);
		}

		/// <summary>
		/// Конструктор
		/// </summary>
        public StarsFloatingWindow(StarsPlugin plugin)
        {
            InitializeComponent();
            SizeChanged += new EventHandler(Sized);
			_plugin = plugin;
        }

		/// <summary>
		/// Метод изменения размеров UserControl-а
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
        protected void Sized(Object sender, EventArgs args)
        {
            System.Drawing.Rectangle rect = ClientRectangle;
			List1.Size = new System.Drawing.Size(rect.Size.Width - 4, rect.Size.Height - 4);
        }

		/// <summary>
		/// Метод обновления списка звёзд
		/// </summary>
		/// <param name="doc"></param>
        public void UpdateContents(TFlex.Model.Document doc)
        {
			ListViewItem currentItem;

			List1.Items.Clear();

            if (doc != null && !doc.IsDisposed)
            {
                foreach (TFlex.Model.Model2D.ExternalObject extObject in doc.GetExternalObjects())
                {
					if (extObject.ConstObject != null && extObject.ConstObject.Plugin.Equals(_plugin))
					{
						currentItem = new ListViewItem();
						currentItem.Text = extObject.DisplayName;
						currentItem.Tag = extObject.ObjectId;
						List1.Items.Add(currentItem);
					}
                }
            }

			currentDocument = doc;
        }

		/// <summary>
		/// Отложенное обновление списка звёзд
		/// </summary>
		/// <param name="doc"></param>
		/// <param name="firstObjectDeleted"></param>
		public void DelayedUpdate(TFlex.Model.Document doc, ref bool firstObjectDeleted)
		{
			DelegatedUpdate updateMethod = new DelegatedUpdate( UpdateContents );
			BeginInvoke(updateMethod, doc);
			firstObjectDeleted = false;
		}

		/// <summary>
		/// Возвращает первый объект, выбранный в списке звёзд
		/// </summary>
		private ModelObject GetSelectedObject()
		{
			if (List1.SelectedItems.Count == 0)
				return null;

			ObjectId objectId = List1.SelectedItems[0].Tag as ObjectId;
			return objectId != null && objectId.IsValid ? currentDocument.GetObjectById(objectId) : null;
		}

		/// <summary>
		/// обработчик события нажатия в контестном меню на опцию "Показать"
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShowToolStripMenuItem_Click(object sender, EventArgs e)
		{
			StarObject star = null;
			TFlex.Model.Model2D.ExternalObject extObject = null;
			ModelObject obj = GetSelectedObject();

			//в принципе, дальнейшие три проверки излишни, т.к. объект обязан быть внешним объектом,
			//а внутри у него обязана лежать звезда
			if (obj != null)
				extObject = obj as TFlex.Model.Model2D.ExternalObject;
			if (extObject != null)
				star = extObject.ConstObject as StarObject;
			if (star != null)
			{
				//Если звезда находится в команде редактирования, отключаем опцию "Показать" и выделение контура,
				//т.к. их в данный момент два - старый и новый (неподтверждённый)
				if (extObject.GetAttributes().GetIntAttribute("Hidden") != 1)
				{
					//для выделенной в списке звезды берём границы обрамляющего прямоугольника
					TFlex.Drawing.Point topLeft = new TFlex.Drawing.Point(star.X - max(star.R1, star.R2), star.Y - max(star.R1, star.R2));
					TFlex.Drawing.Point bottomRight = new TFlex.Drawing.Point(star.X + max(star.R1, star.R2), star.Y + max(star.R1, star.R2));

					//цикл по видам документа
					foreach (TFlex.Model.View view in currentDocument.Views)
					{
						//для каждого вида выбираем страницу, содержащую наш объект и делаем приближение звезды
						View2D v2d = view as View2D;
						view.Page = extObject.Page;
						if (v2d != null) v2d.ZoomRectangle = new TFlex.Drawing.Rectangle(topLeft, bottomRight);
					}
				}
			}

		}

		/// <summary>
		/// обработчик события нажатия в контестном меню на опцию "Редактировать"
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EditToolStripMenuItem_Click(object sender, EventArgs e)
		{
			StarObject star = null;
			TFlex.Model.Model2D.ExternalObject extObject = null;
			ModelObject obj = GetSelectedObject();

			if (obj != null)
				extObject = obj as TFlex.Model.Model2D.ExternalObject;
			if (extObject != null)
				star = extObject.ConstObject as StarObject;
			if (star != null) //выделить звезду
			{
				ExternalObject selectedOwner = star.Owner2D;
				EditCommand command = new EditCommand(star.Plugin, selectedOwner, currentDocument);
				currentDocument.Selection.DeselectAll();
				command.Run(currentDocument.ActiveView);
			}
		}

		/// <summary>
		/// обработичк события нажатия в контестном меню на опцию "Удалить"
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			//взяли объект по ID, основываясь на тексте выделенного эл-та списка
			TFlex.Model.Model2D.ExternalObject extObject = null;
			ModelObject obj = GetSelectedObject();

			//в принципе, дальнейшие три проверки излишни, т.к. объект обязан быть внешним объектом,
			//а внутри у него обязана лежать звезда
			if (obj != null)
				extObject = obj as TFlex.Model.Model2D.ExternalObject;
			if (extObject != null)
				currentDocument.DeleteObjects(new ObjectArray(extObject), new DeleteOptions());
		}

		/// <summary>
		/// обработчик события щелчка мыши (поднятия клавиши мыши):
		/// если нажата правая клавиша над одним из эл-тов ListView, включаем контекстное меню,
		/// если же она нажата где-то ещё - отключаем его
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void List1_MouseUp(object sender, MouseEventArgs e)
		{
			//Показать меню, только если был клик ПКМ
			if (e.Button == MouseButtons.Right)
			{
				//Получить узел, на котором был клик
				ListViewItem currentItem = List1.GetItemAt(e.X, e.Y);
				if (currentItem != null)
					this.List1.ContextMenuStrip = contextMenuStrip1;
				else
					this.List1.ContextMenuStrip = null;
			}
		}

		/// <summary>
		/// обработчик события нажатия клавиши мыши:
		/// сбрасывает видимость контекстного меню в false,
		/// она восстанавливается или нет в зависимости от места щелчка
		/// уже при поднятии клавиши мыши
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void List1_MouseDown(object sender, MouseEventArgs e)
		{
			this.List1.ContextMenuStrip = null;
		}

		/// <summary>
		/// обработчик события смены выделенного имени в списке
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void List1_SelectedIndexChanged(object sender, EventArgs e)
		{
			StarObject star = null;
			TFlex.Model.Model2D.ExternalObject extObject = null;
			ModelObject obj = GetSelectedObject();

			if (obj != null)
				extObject = obj as TFlex.Model.Model2D.ExternalObject;
			if (extObject != null)
				star = extObject.ConstObject as StarObject;
			//Если звезда находится в команде редактирования, отключаем выделение контура,
			//т.к. их в данный момент два - старый и новый (неподтверждённый)
			if ((star != null) && (extObject.GetAttributes().GetIntAttribute("Hidden") != 1))
				//выделить звезду
				currentDocument.Selection.Select(extObject);
		}

		private void List1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			if (List1.SelectedItems.Count == 0)
				currentDocument.Selection.DeselectAll();
		}
    }
}
